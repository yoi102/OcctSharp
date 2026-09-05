#include "Modeling/ContourPrograms.hxx"
#include "Modeling/ScalarLaws.hxx"
#include <BRepFilletAPI_MakeFillet.hxx>
#include <ChFiDS_CircSection.hxx>
#include <GeomAbs_Shape.hxx>
#include <NCollection_Array1.hxx>
#include <NCollection_HArray1.hxx>
#include <gp_Circ.hxx>
#include <gp_Pnt2d.hxx>
#include <Law_Interpol.hxx>
#include <Law_BSpline.hxx>
#include <BRepGProp.hxx>
#include <GProp_GProps.hxx>
#include <map>
#include <set>
#include <numbers>
#include <optional>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
namespace {
struct RadiusEvidence { double Error = 0; int Samples = 0; };
// OCCT 8.0.1's Law_Function setter clears the radius sequence without retaining
// the supplied law. Use the checked sample API instead; expose measured adapter
// error, never advertise this native interpolation as an exact arbitrary law.
RadiusEvidence ApplyLaw(BRepFilletAPI_MakeFillet& algorithm, int contour,
  const ScalarLawData& law, double tolerance) {
  RadiusEvidence evidence;
  if (algorithm.Closed(contour)) {
    const auto first = SampleScalarLaw(law, 0), last = SampleScalarLaw(law, 1);
    const auto matches = [](double a, double b) {
      return std::abs(a - b) <= 1e-10 * std::max({1.0, std::abs(a), std::abs(b)});
    };
    Require(matches(first.value, last.value) && matches(first.first_derivative, last.first_derivative),
      "A closed contour radius law must match value and first derivative at its seam.");
  }
  const double length = algorithm.Length(contour); Positive(length); double distance = 0;
  for (int edge = 1; edge <= algorithm.NbEdges(contour); ++edge) {
    GProp_GProps properties; BRepGProp::LinearProperties(algorithm.Edge(contour, edge), properties);
    const double edgeLength = properties.Mass(); Positive(edgeLength);
    const double first = distance / length;
    const double last = edge == algorithm.NbEdges(contour) ? 1 : (distance + edgeLength) / length;
    Require(last > first && last <= 1 + 1e-8, "Invalid contour arc-length domain.");
    int count = 17; bool accepted = false;
    for (; count <= 4097; count = (count - 1) * 2 + 1) {
      NCollection_Array1<gp_Pnt2d> profile(1, count);
      for (int i = 0; i < count; ++i) {
        const double u = i == 0 ? 0 : i == count - 1 ? 1 : (1 - std::cos(std::numbers::pi * i / (count - 1))) / 2;
        const double parameter = std::clamp(first + (last - first) * u, 0.0, 1.0);
        profile(i + 1) = gp_Pnt2d(u, SampleScalarLaw(law, parameter).value);
      }
      occ::handle<Law_Interpol> interpolated = new Law_Interpol(); interpolated->Set(profile, 0., 0., false);
      const auto curve = interpolated->Curve(); bool positive = true;
      for (int pole = 1; pole <= curve->NbPoles(); ++pole) positive &= curve->Pole(pole) > 0;
      double error = 0;
      for (int i = 0; i < 4 * (count - 1); ++i) {
        const int segment = i / 4;
        const double u = profile(segment + 1).X() + (profile(segment + 2).X() - profile(segment + 1).X()) * ((i % 4 + .5) / 4);
        error = std::max(error, std::abs(interpolated->Value(u) - SampleScalarLaw(law, std::clamp(first + (last - first) * u, 0.0, 1.0)).value));
      }
      if (positive && error <= tolerance) {
        Require(evidence.Samples <= 65536 - count, "Contour radius samples exceed the bounded limit.");
        algorithm.SetRadius(profile, contour, edge); evidence.Samples += count; evidence.Error = std::max(evidence.Error, error);
        accepted = true; break;
      }
    }
    Require(accepted, "The scalar law cannot meet the sampled radius approximation policy within 4097 samples per edge.");
    distance += edgeLength;
  }
  return evidence;
}
void Faults(BRepFilletAPI_MakeFillet& algorithm, const InputGraph& graph, Result& result) {
  const int contours = algorithm.NbFaultyContours(), vertices = algorithm.NbFaultyVertices();
  Require(contours >= 0 && contours <= 256 && vertices >= 0 && vertices <= 100000, "Fillet fault count exceeds the limit.");
  for (int i = 1; i <= contours; ++i) {
    const int ic = algorithm.FaultyContour(i); Require(ic > 0 && ic <= algorithm.NbContours(), "Invalid faulty contour index.");
    const int source = graph.Index(0, algorithm.Edge(ic, 1));
    result.Data().Faults.push_back({0, ic - 1, source, static_cast<int>(algorithm.StripeStatus(ic))});
    result.Add(algorithm.Edge(ic, 1), ProblemShape, 0, source, ic - 1, TopAbs_EDGE);
  }
  for (int i = 1; i <= vertices; ++i) {
    const auto vertex = algorithm.FaultyVertex(i); const int source = graph.Index(0, vertex);
    result.Data().Faults.push_back({1, -1, source, -1}); result.Add(vertex, ProblemShape, 0, source, -1, TopAbs_VERTEX);
  }
  if (algorithm.HasResult()) {
    auto partial = algorithm.BadShape();
    if (!partial.IsNull()) { result.Data().Info.partial = 1; result.Add(partial, Partial); }
  }
}
void Simulate(BRepFilletAPI_MakeFillet& algorithm, Result& result) {
  for (int ic = 1; ic <= algorithm.NbContours(); ++ic) {
    algorithm.Simulate(ic); const int surfaces = algorithm.NbSurf(ic);
    Require(surfaces >= 0 && surfaces <= 65536, "Too many simulated fillet patches.");
    for (int patch = 1; patch <= surfaces; ++patch) {
      const auto sections = algorithm.Sect(ic, patch); if (sections.IsNull()) continue;
      Require(sections->Length() <= 65536 && result.Data().Sections.size() + sections->Length() <= 65536, "Too many simulated sections.");
      for (int i = sections->Lower(); i <= sections->Upper(); ++i) {
        gp_Circ circle; double first = 0, last = 0; sections->Value(i).Get(circle, first, last);
        Positive(circle.Radius()); Require(std::isfinite(first) && std::isfinite(last), "Invalid simulated circle parameters.");
        result.Data().Sections.push_back({ic - 1, patch - 1, i - sections->Lower(), 0,
          Xyz(circle.Location().XYZ()), Xyz(circle.Axis().Direction().XYZ()), Xyz(circle.XAxis().Direction().XYZ()),
          circle.Radius(), first, last});
      }
    }
  }
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_contour_fillet(const OcctSharp_ShapeHandle* source,
  const OcctSharp_FilletProgram* programs, int32_t count, const OcctSharp_RadiusSample* samples, int32_t sampleCount,
  const OcctSharp_VertexRadius* vertices, int32_t vertexCount, const OcctSharp_LawInput* laws, int32_t lawCount,
  const OcctSharp_FilletOptions* options, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output && options, "Missing fillet options or output."); const auto& o = *options;
    Require(count >= 0 && count <= 256 && (!count || programs), "Invalid fillet program buffer.");
    Require(sampleCount >= 0 && sampleCount <= 65536 && (!sampleCount || samples)
      && vertexCount >= 0 && vertexCount <= 65536 && (!vertexCount || vertices)
      && lawCount >= 0 && lawCount <= 256 && (!lawCount || laws), "Invalid radius buffers.");
    Require(o.action >= 0 && o.action <= 2 && o.representation >= 0 && o.representation <= 2
      && o.continuity >= 0 && o.continuity <= 2 && o.reserved == 0, "Invalid fillet action/representation/continuity.");
    for (double value : {o.tangent_tolerance, o.tolerance_3d, o.tolerance_2d, o.approximation_3d,
      o.approximation_2d, o.deflection, o.angular_tolerance}) Positive(value);
    InputGraph graph(&source, 1); Result result(0);
    if (!count) { result.Owner->Result = graph.At(0); result.Data().Info.ready = 1; result.Data().Info.done = o.action == 2; result.Publish(output); return; }
    std::vector<int> seeds; std::vector<ScalarLawData> builtLaws;
    for (int i = 0; i < lawCount; ++i) {
      Require(laws[i].first == 0 && laws[i].last == 1, "Fillet law domain must be normalized to [0,1].");
      auto law = BuildScalarLaw(laws[i]); Require(law.LowerBound > 0, "Fillet laws require a positive conservative control bound."); builtLaws.push_back(std::move(law));
    }
    for (int i = 0; i < count; ++i) {
      const auto& p = programs[i]; graph.Subshape(0, p.seed, TopAbs_EDGE);
      Require(p.reserved == 0 && p.mode >= 0 && p.mode <= 2, "Invalid radius program mode.");
      Require(p.sample_offset >= 0 && p.sample_count >= 0 && p.sample_offset <= sampleCount - p.sample_count
        && p.vertex_offset >= 0 && p.vertex_count >= 0 && p.vertex_offset <= vertexCount - p.vertex_count, "Invalid radius program ranges.");
      if (p.mode == 0) Positive(p.radius);
      if (p.mode == 1) Require(p.law_index >= 0 && p.law_index < lawCount, "Invalid scalar-law index.");
      if (p.mode == 2) {
        Require(p.sample_count >= 2, "A sampled radius profile requires endpoints.");
        for (int j = 0; j < p.sample_count; ++j) {
          const auto& sample = samples[p.sample_offset + j]; Positive(sample.radius);
          Require(std::isfinite(sample.parameter) && sample.parameter >= 0 && sample.parameter <= 1
            && (!j || sample.parameter > samples[p.sample_offset + j - 1].parameter), "Radius sample parameters must strictly increase in [0,1].");
        }
        Require(samples[p.sample_offset].parameter == 0 && samples[p.sample_offset + p.sample_count - 1].parameter == 1, "Radius samples must cover the full contour.");
      }
      for (int j = 0; j < p.vertex_count; ++j) {
        const auto& vertex = vertices[p.vertex_offset + j]; Require(vertex.reserved == 0, "Nonzero vertex-radius reserved field.");
        graph.Subshape(0, vertex.vertex, TopAbs_VERTEX); Positive(vertex.radius);
      }
      seeds.push_back(p.seed);
    }
    BRepFilletAPI_MakeFillet algorithm(graph.At(0), static_cast<ChFi3d_FilletShape>(o.representation));
    algorithm.SetParams(o.tangent_tolerance, o.tolerance_3d, o.tolerance_2d, o.approximation_3d, o.approximation_2d, o.deflection);
    const GeomAbs_Shape continuity[] = {GeomAbs_C0, GeomAbs_C1, GeomAbs_C2};
    algorithm.SetContinuity(continuity[o.continuity], o.angular_tolerance);
    std::map<int, double> junctions;
    std::map<int, RadiusEvidence> lawEvidence;
    for (int i = 0; i < count; ++i) {
      const auto& p = programs[i]; const auto& edge = TopoDS::Edge(graph.Subshape(0, p.seed, TopAbs_EDGE));
      Require(algorithm.Contour(edge) == 0, "Conflicting radius assignments on one tangent contour.");
      algorithm.Add(edge); const int ic = algorithm.Contour(edge);
      Require(ic > 0, "The selected edge does not form an eligible fillet contour.");
      std::optional<ScalarLawData> sampledLaw;
      const ScalarLawData* radiusLaw = nullptr;
      if (p.mode == 0) {
        for (int e = 1; e <= algorithm.NbEdges(ic); ++e) algorithm.SetRadius(p.radius, ic, algorithm.Edge(ic, e));
      }
      else if (p.mode == 1) {
        radiusLaw = &builtLaws[p.law_index];
        lawEvidence.emplace(ic - 1, ApplyLaw(algorithm, ic, *radiusLaw, o.approximation_3d));
      }
      else {
        // Sampled recipes define one normalized global interpolation, not a
        // fresh full-range profile repeated independently on every edge.
        std::vector<double> values;
        for (int j = 0; j < p.sample_count; ++j) values.push_back(samples[p.sample_offset + j].radius);
        for (int j = 0; j < p.sample_count; ++j) values.push_back(samples[p.sample_offset + j].parameter);
        OcctSharp_LawSpan span{}; span.kind = 2; span.tangents = 1; span.last = span.active_last = 1;
        span.value_count = span.parameter_count = p.sample_count; span.parameter_offset = p.sample_count;
        OcctSharp_LawInput input{&span, values.data(), nullptr, 1, static_cast<int>(values.size()), 0, 0, 0, 1};
        sampledLaw.emplace(BuildScalarLaw(input)); radiusLaw = &*sampledLaw;
        Require(radiusLaw->LowerBound > 0, "The sampled radius interpolation has no positive control-hull bound.");
        lawEvidence.emplace(ic - 1, ApplyLaw(algorithm, ic, *radiusLaw, o.approximation_3d));
      }
      std::set<int> assignedVertices;
      for (int j = 0; j < p.vertex_count; ++j) {
        const auto& v = vertices[p.vertex_offset + j]; const auto& vertex = TopoDS::Vertex(graph.Subshape(0, v.vertex, TopAbs_VERTEX));
        const double parameter = algorithm.RelativeAbscissa(ic, vertex);
        Require(parameter >= 0, "A constrained vertex is outside its contour.");
        Require(assignedVertices.insert(v.vertex).second, "Duplicate vertex-radius constraint.");
        auto [it, inserted] = junctions.emplace(v.vertex, v.radius);
        Require(inserted || it->second == v.radius, "Conflicting radius constraints at a shared contour junction.");
        if (radiusLaw) {
          // A vertex setter would replace sampled law data and invalidate its
          // measured approximation evidence. Law anchors are consistency checks,
          // not silent edits to the authored law.
          const double expected = SampleScalarLaw(*radiusLaw, parameter).value;
          Require(std::abs(expected - v.radius) <= 1e-10 * std::max({1.0, expected, v.radius}),
            "A vertex radius conflicts with the authored contour law; edit the law explicitly.");
        } else algorithm.SetRadius(v.radius, ic, vertex);
      }
    }
    CopyContours(algorithm, graph, seeds, result); result.Data().Info.ready = 1;
    for (auto& contour : result.Data().Contours) if (lawEvidence.contains(contour.index)) {
      const auto& evidence = lawEvidence.at(contour.index); contour.law_approximated = 1;
      contour.law_probe_error = evidence.Error; contour.law_sample_count = evidence.Samples;
    }
    try {
      if (o.action == 1) { Simulate(algorithm, result); result.Owner->Message = "Copied circular fillet sections; parameters are circle trims."; }
      if (o.action == 2) {
        algorithm.Build(); result.Data().Info.done = algorithm.IsDone();
        if (algorithm.IsDone()) {
          result.Owner->Result = algorithm.Shape(); History(algorithm, graph, result); result.Data().Info.group_support |= Patches;
          // Generated(edge/vertex) gives an exact contour source association. NewFaces
          // supplies OCCT surface groups without guessing contour membership.
          const int patches = algorithm.NbSurfaces(); Require(patches >= 0 && patches <= 100000, "Too many fillet surface groups.");
          for (int patch = 1; patch <= patches; ++patch)
            for (const auto& face : algorithm.NewFaces(patch)) result.Add(face, SurfacePatch, -1, -1, patch - 1);
          result.Owner->Message = "Fillet built; validity and protected acceptance are separate checks.";
        } else { result.Fail("Fillet build failed; partial topology is diagnostic only."); Faults(algorithm, graph, result); }
      }
    } catch (const Standard_Failure& error) { result.Fail(error.GetMessageString()); Faults(algorithm, graph, result); }
    result.Publish(output);
  });
}

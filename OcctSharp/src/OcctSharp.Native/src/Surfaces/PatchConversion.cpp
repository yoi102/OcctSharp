#include "Modeling/GuidedAuthoring.hxx"
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepTools.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BSplineSurface.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_BezierSurface.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <GeomConvert.hxx>
#include <GeomConvert_CompCurveToBSplineCurve.hxx>
#include <GeomConvert_BSplineCurveToBezierCurve.hxx>
#include <GeomConvert_BSplineSurfaceToBezierSurface.hxx>
#include <GeomFill_BSplineCurves.hxx>
#include <GeomFill_BezierCurves.hxx>
#include <NCollection_Array1.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Face.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

namespace {
opencascade::handle<Geom_BSplineCurve> Curve(const InputGraph& graph, int index, bool orient, bool exactParameters,
  double& first, double& last) {
  const auto& edge = graph.Typed(index, TopAbs_EDGE);
  auto original = BRep_Tool::Curve(TopoDS::Edge(edge), first, last);
  Require(!original.IsNull() && std::isfinite(first) && std::isfinite(last) && first < last, "Curve requires finite increasing 3D bounds.");
  auto spline = opencascade::handle<Geom_BSplineCurve>::DownCast(original);
  if (exactParameters) Require(!spline.IsNull(), "Span operations require a B-spline curve to retain exact original parameters.");
  if (!spline.IsNull()) { spline = opencascade::handle<Geom_BSplineCurve>::DownCast(spline->Copy()); spline->Segment(first, last); }
  else spline = GeomConvert::CurveToBSplineCurve(new Geom_TrimmedCurve(original, first, last));
  if (orient && edge.Orientation() == TopAbs_REVERSED) spline->Reverse();
  Require(spline->NbPoles() <= 65536 && spline->NbKnots() <= 4096, "Curve exceeds conversion limits.");
  return spline;
}
opencascade::handle<Geom_BSplineSurface> Surface(const InputGraph& graph, double& u0, double& u1, double& v0, double& v1) {
  const auto face = TopoDS::Face(graph.Typed(0, TopAbs_FACE));
  auto spline = opencascade::handle<Geom_BSplineSurface>::DownCast(BRep_Tool::Surface(face));
  Require(!spline.IsNull(), "Patch operations require a B-spline surface to retain original UV spans.");
  BRepTools::UVBounds(face, u0, u1, v0, v1);
  Require(std::isfinite(u0) && std::isfinite(u1) && std::isfinite(v0) && std::isfinite(v1), "Patch bounds must be finite.");
  spline = opencascade::handle<Geom_BSplineSurface>::DownCast(spline->Copy()); spline->Segment(u0, u1, v0, v1);
  Require(static_cast<int64_t>(spline->NbUPoles()) * spline->NbVPoles() <= 65536
    && static_cast<int64_t>(spline->NbUKnots() - 1) * (spline->NbVKnots() - 1) <= 4096, "Surface exceeds copied patch limits.");
  return spline;
}
TopoDS_Shape Edge(const opencascade::handle<Geom_Curve>& curve) {
  BRepBuilderAPI_MakeEdge builder(curve); Require(builder.IsDone(), "Converted curve could not become an edge."); return builder.Shape();
}
TopoDS_Shape Face(const opencascade::handle<Geom_Surface>& surface, double tolerance, int orientation) {
  BRepBuilderAPI_MakeFace builder(surface, tolerance); Require(builder.IsDone(), "Converted surface could not become a face.");
  auto shape = builder.Shape(); shape.Orientation(static_cast<TopAbs_Orientation>(orientation)); return shape;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_patch_convert(const OcctSharp_ShapeHandle* const* inputs, int32_t inputCount,
  const OcctSharp_PatchOptions* options, OcctSharp_PatchSpan* spans, int32_t capacity, int32_t* spanCount,
  OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && spanCount && info && output && capacity >= 0 && (capacity == 0 || spans), "Invalid patch-conversion outputs.");
    const auto& o = *options; Require(o.reserved == 0 && o.operation >= 0 && o.operation <= 5 && o.style >= 0 && o.style <= 2
      && o.minimum_multiplicity >= 0 && o.minimum_multiplicity <= 25, "Invalid conversion controls.");
    Flag(o.with_ratio); Flag(o.bezier); Positive(o.tolerance); InputGraph graph(inputs, inputCount);
    auto result = std::make_unique<OcctSharp_FeatureResultHandle>(); std::vector<OcctSharp_PatchSpan> provenance;
    const int orientation = graph.At(0).Orientation();
    if (o.operation == 0) {
      Require(inputCount >= 2 && inputCount <= 4, "Boundary patches require two to four eligible curves.");
      std::vector<opencascade::handle<Geom_BSplineCurve>> curves;
      for (int i = 0; i < inputCount; ++i) { double first, last; curves.push_back(Curve(graph, i, true, false, first, last)); }
      // OCCT's B-spline Coons constructor requires at least four poles in both
      // directions. Exact degree elevation makes linear/quadratic boundaries
      // eligible without changing their geometry or approximating them.
      if (o.style == GeomFill_CoonsStyle)
        for (const auto& curve : curves) if (curve->Degree() < 3) curve->IncreaseDegree(3);
      const auto style = static_cast<GeomFill_FillingStyle>(o.style); opencascade::handle<Geom_Surface> patch;
      if (o.bezier) {
        std::vector<opencascade::handle<Geom_BezierCurve>> bezier;
        for (const auto& curve : curves) {
          GeomConvert_BSplineCurveToBezierCurve converter(curve); Require(converter.NbArcs() == 1, "Bezier patch boundaries must each be one polynomial/rational span.");
          bezier.push_back(converter.Arc(1));
        }
        GeomFill_BezierCurves fill;
        if (inputCount == 2) fill.Init(bezier[0], bezier[1], style);
        else if (inputCount == 3) fill.Init(bezier[0], bezier[1], bezier[2], style);
        else fill.Init(bezier[0], bezier[1], bezier[2], bezier[3], style);
        patch = fill.Surface();
      } else {
        GeomFill_BSplineCurves fill;
        if (inputCount == 2) fill.Init(curves[0], curves[1], style);
        else if (inputCount == 3) fill.Init(curves[0], curves[1], curves[2], style);
        else fill.Init(curves[0], curves[1], curves[2], curves[3], style);
        patch = fill.Surface();
      }
      result->Result = Face(patch, o.tolerance, TopAbs_FORWARD);
    } else if (o.operation == 1) {
      Require(inputCount >= 1 && inputCount <= 128, "Composite curve assembly accepts at most 128 spans.");
      GeomConvert_CompCurveToBSplineCurve assembly;
      for (int i = 0; i < inputCount; ++i) {
        double first, last; auto curve = Curve(graph, i, true, false, first, last);
        const double start = i == 0 ? curve->FirstParameter() : assembly.BSplineCurve()->LastParameter();
        // After=true preserves the existing result parameter domain at each append.
        Require(assembly.Add(curve, o.tolerance, true, o.with_ratio != 0, o.minimum_multiplicity), "Consecutive curve spans are not G0 within tolerance.");
        provenance.push_back({i, 0, 0, graph.At(i).Orientation(), first, last, 0, 0, start, assembly.BSplineCurve()->LastParameter()});
      }
      result->Result = Edge(assembly.BSplineCurve());
    } else if (o.operation == 2 || o.operation == 4) {
      Require(inputCount == 1, "Curve span operations take one source."); double first, last; auto curve = Curve(graph, 0, false, true, first, last);
      if (o.operation == 4) {
        Require(std::isfinite(o.first) && std::isfinite(o.last) && o.first >= first && o.last <= last && o.first < o.last,
          "Extracted curve range lies outside its active domain.");
        curve->Segment(o.first, o.last); result->Result = Edge(curve); result->Result.Orientation(static_cast<TopAbs_Orientation>(orientation));
        provenance.push_back({0, 0, 0, orientation, o.first, o.last, 0, 0, o.first, o.last});
      } else {
        GeomConvert_BSplineCurveToBezierCurve converter(curve); const int count = converter.NbArcs(); Require(count <= 4096, "Too many Bezier spans.");
        NCollection_Array1<double> knots(1, count + 1); converter.Knots(knots);
        for (int i = 1; i <= count; ++i) {
          auto shape = Edge(converter.Arc(i)); shape.Orientation(static_cast<TopAbs_Orientation>(orientation)); Add(*result, shape, 8, 0, i - 1, TopAbs_EDGE);
          provenance.push_back({0, i - 1, 0, orientation, knots(i), knots(i + 1), 0, 0, 0, 1});
        }
      }
    } else {
      Require(inputCount == 1, "Surface patch operations take one source."); double u0, u1, v0, v1; auto surface = Surface(graph, u0, u1, v0, v1);
      if (o.operation == 5) {
        Require(std::isfinite(o.first_u) && std::isfinite(o.last_u) && std::isfinite(o.first_v) && std::isfinite(o.last_v)
          && o.first_u >= u0 && o.last_u <= u1 && o.first_v >= v0 && o.last_v <= v1 && o.first_u < o.last_u && o.first_v < o.last_v,
          "Extracted patch lies outside the active UV domain.");
        surface->Segment(o.first_u, o.last_u, o.first_v, o.last_v); result->Result = Face(surface, o.tolerance, orientation);
        provenance.push_back({0, 0, 0, orientation, o.first_u, o.last_u, o.first_v, o.last_v, o.first_u, o.last_u});
      } else {
        GeomConvert_BSplineSurfaceToBezierSurface converter(surface); const int nu = converter.NbUPatches(), nv = converter.NbVPatches();
        Require(static_cast<int64_t>(nu) * nv <= 4096, "Too many Bezier patches.");
        NCollection_Array1<double> u(1, nu + 1), v(1, nv + 1); converter.UKnots(u); converter.VKnots(v);
        for (int i = 1; i <= nu; ++i) for (int j = 1; j <= nv; ++j) {
          Add(*result, Face(converter.Patch(i, j), o.tolerance, orientation), 9, 0, static_cast<int>(provenance.size()), TopAbs_FACE);
          provenance.push_back({0, i - 1, j - 1, orientation, u(i), u(i + 1), v(j), v(j + 1), 0, 1});
        }
      }
    }
    Require(provenance.size() <= 4096, "Converted span count exceeds the bound.");
    const int required = static_cast<int>(provenance.size());
    if (capacity == 0 && spans == nullptr && required > 0) { *spanCount = required; return; }
    Require(capacity >= required, "The span output buffer is too small.");
    OcctSharp_AuthoringInfo state{}; state.ready = 1; state.done = 1; state.continuity_limit = -1;
    state.section_count = required; result->Message = "Conversion returned owning temporary topology and copied parameter provenance.";
    Finish(*result, state); *output = RegisterFeatureResult(std::move(result)); *info = state;
    if (required) std::copy(provenance.begin(), provenance.end(), spans); *spanCount = required;
  });
}

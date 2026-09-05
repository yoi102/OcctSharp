#include "Modeling/GuidedAuthoring.hxx"
#include "Modeling/ScalarLaws.hxx"
#include <BRepOffsetAPI_MakePipeShell.hxx>
#include <BRepTools_WireExplorer.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <Geom2d_Curve.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS_Wire.hxx>
#include <TopoDS_Vertex.hxx>
#include <gp_Ax2.hxx>
#include <gp_Vec.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

namespace {
void Support(const TopoDS_Wire& spine, const TopoDS_Shape& support) {
  for (TopExp_Explorer edge(spine, TopAbs_EDGE); edge.More(); edge.Next()) {
    bool supported = false;
    for (TopExp_Explorer face(support, TopAbs_FACE); face.More(); face.Next()) {
      double first, last;
      if (!BRep_Tool::CurveOnSurface(TopoDS::Edge(edge.Current()), TopoDS::Face(face.Current()), first, last).IsNull()) {
        supported = true; break;
      }
    }
    Require(supported, "Every spine edge must have a pcurve on an explicitly supplied support face.");
  }
}
void Frame(BRepOffsetAPI_MakePipeShell& builder, const TopoDS_Wire& spine,
  const InputGraph& graph, const OcctSharp_SweepOptions& o) {
  switch (o.frame) {
    case 0: builder.SetMode(false); break;
    case 1: builder.SetMode(true); break;
    case 2: builder.SetMode(gp_Ax2(Point(o.origin), Direction(o.direction), Direction(o.x_direction))); break;
    case 3: {
      const gp_Dir binormal = Direction(o.direction);
      for (TopExp_Explorer edge(spine, TopAbs_EDGE); edge.More(); edge.Next()) {
        BRepAdaptor_Curve curve(TopoDS::Edge(edge.Current()));
        for (int i = 0; i <= 16; ++i) {
          gp_Pnt p; gp_Vec tangent; curve.D1(curve.FirstParameter() + (curve.LastParameter() - curve.FirstParameter()) * i / 16, p, tangent);
          Require(tangent.SquareMagnitude() > 1e-24 && gp_Vec(binormal).Crossed(tangent.Normalized()).SquareMagnitude() > 1e-20,
            "Fixed binormal is degenerate with a sampled spine tangent.");
        }
      }
      builder.SetMode(binormal); break;
    }
    case 4: builder.SetDiscreteMode(); break;
    case 5: Support(spine, graph.At(o.secondary_index)); Require(builder.SetMode(graph.At(o.secondary_index)), "Support framing was rejected by OCCT."); break;
    case 6: builder.SetMode(TopoDS::Wire(graph.Typed(o.secondary_index, TopAbs_WIRE)), o.curvilinear != 0,
      static_cast<BRepFill_TypeOfContact>(o.contact)); break;
    default: Require(false, "Unknown sweep frame.");
  }
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_guided_sweep(const OcctSharp_ShapeHandle* const* inputs, int32_t count,
  const OcctSharp_SweepSection* sections, int32_t sectionCount, const OcctSharp_SweepOptions* options,
  const OcctSharp_LawInput* lawInput, OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options != nullptr && info != nullptr && output != nullptr && sections != nullptr, "A guided sweep argument is null.");
    const auto& o = *options;
    Require(o.reserved == 0 && o.frame >= 0 && o.frame <= 6 && o.transition >= 0 && o.transition <= 2 && o.contact >= 0 && o.contact <= 2,
      "Invalid sweep modes.");
    Require(sectionCount >= 1 && sectionCount <= 128 && o.maximum_degree >= 2 && o.maximum_degree <= 25
      && o.maximum_segments >= 1 && o.maximum_segments <= 512 && o.solid_policy >= 0 && o.solid_policy <= 2
      && o.operation >= 0 && o.operation <= 2 && o.simulation_count >= 2 && o.simulation_count <= 256, "Sweep limits are out of range.");
    Flag(o.curvilinear); Flag(o.force_c1); Positive(o.tolerance_3d); Positive(o.tolerance_boundary); Positive(o.tolerance_angular);
    Require(o.frame == 6 || o.contact == 0, "Auxiliary contact requires an auxiliary spine.");
    Require(lawInput == nullptr || (sectionCount == 1 && o.frame != 6), "Homothetic laws require one section and cannot use auxiliary-spine framing.");
    Require(o.contact != 2 || sectionCount == 1, "Border contact computes one automatic scale law and requires exactly one section.");
    InputGraph graph(inputs, count); const auto spine = TopoDS::Wire(graph.Typed(0, TopAbs_WIRE));
    ShapeMap orderedVertices;
    for (BRepTools_WireExplorer it(spine); it.More(); it.Next()) orderedVertices.Add(it.CurrentVertex());
    TopoDS_Vertex start, end; TopExp::Vertices(spine, start, end); if (!end.IsNull()) orderedVertices.Add(end);
    int previousLocation = 0, closure = -1;
    for (int i = 0; i < sectionCount; ++i) {
      const auto& section = sections[i]; Flag(section.contact); Flag(section.correction);
      const auto& profile = graph.At(section.shape_index);
      if (o.contact == 2) {
        Require(profile.ShapeType() == TopAbs_WIRE, "Border contact requires a planar wire section.");
        BRepBuilderAPI_MakeFace plane(TopoDS::Wire(profile), true);
        // BRepFill_PipeShell::Add assumes this planar face exists and otherwise
        // dereferences a null surface. Reject the unsupported input before Add.
        Require(plane.IsDone() && !plane.Shape().IsNull()
          && !BRep_Tool::Surface(TopoDS::Face(plane.Shape())).IsNull(), "Border contact requires a nondegenerate planar section.");
      }
      Require(profile.ShapeType() == TopAbs_WIRE || ((i == 0 || i == sectionCount - 1) && profile.ShapeType() == TopAbs_VERTEX),
        "Sections must be wires, with optional punctual endpoints.");
      if (profile.ShapeType() == TopAbs_WIRE) {
        const int closed = BRep_Tool::IsClosed(profile); if (closure < 0) closure = closed;
        Require(closure == closed, "Sweep section closure must agree.");
      }
      if (section.location_index >= 0) {
        const auto& location = graph.Typed(section.location_index, TopAbs_VERTEX);
        const int index = orderedVertices.FindIndex(location);
        Require(index > 0 && index > previousLocation, "Attachments must be actual spine vertices in strictly increasing traversal order.");
        previousLocation = index;
      }
    }
    auto result = std::make_unique<OcctSharp_FeatureResultHandle>();
    OcctSharp_AuthoringInfo state{}; state.continuity_limit = o.frame == 6 && o.contact != 0 ? 0 : -1;
    try {
      BRepOffsetAPI_MakePipeShell builder(spine); builder.SetIsBuildHistory(true); Frame(builder, spine, graph, o);
      builder.SetTolerance(o.tolerance_3d, o.tolerance_boundary, o.tolerance_angular);
      builder.SetMaxDegree(o.maximum_degree); builder.SetMaxSegments(o.maximum_segments); builder.SetForceApproxC1(o.force_c1 != 0);
      builder.SetTransitionMode(static_cast<BRepBuilderAPI_TransitionMode>(o.transition));
      ScalarLawData law;
      if (lawInput) {
        law = BuildScalarLaw(*lawInput);
        Require(law.LowerBound > 0, "A swept scale law requires a strictly positive control-hull bound, not sampled positivity alone.");
        for (size_t i = 1; i < law.Spans.size(); ++i)
          Require(std::abs(law.Spans[i - 1]->Value(law.Ends[i - 1]) - law.Spans[i]->Value(law.Ends[i - 1])) <= o.tolerance_3d,
            "A swept scale law must be positionally continuous at each composite join.");
      }
      for (int i = 0; i < sectionCount; ++i) {
        const auto& section = sections[i]; const auto& profile = graph.At(section.shape_index);
        if (lawInput) {
          if (section.location_index >= 0) builder.SetLaw(profile, law.Function, TopoDS::Vertex(graph.At(section.location_index)), section.contact != 0, section.correction != 0);
          else builder.SetLaw(profile, law.Function, section.contact != 0, section.correction != 0);
        } else {
          if (section.location_index >= 0) builder.Add(profile, TopoDS::Vertex(graph.At(section.location_index)), section.contact != 0, section.correction != 0);
          else builder.Add(profile, section.contact != 0, section.correction != 0);
        }
      }
      state.ready = builder.IsReady(); state.algorithm_status = builder.GetStatus();
      if (!state.ready) result->Message = "OCCT pipe-shell builder is not ready.";
      else if (o.operation == 0) result->Message = "Section/frame/attachment checks passed; IsReady is not a build-success guarantee.";
      else if (o.operation == 1) {
        NCollection_List<TopoDS_Shape> simulated; builder.Simulate(o.simulation_count, simulated);
        for (const auto& section : simulated) Add(*result, section, 4);
        state.section_count = simulated.Size(); state.done = state.section_count == o.simulation_count;
        state.algorithm_status = builder.GetStatus(); result->Message = "OCCT equally spaced simulated sections; no arbitrary-station guarantee.";
      } else {
        builder.Build(); state.algorithm_status = builder.GetStatus(); state.done = builder.IsDone();
        if (state.done) {
          result->Result = builder.Shape(); state.approximation_error = builder.ErrorOnSurface();
          state.error_available = std::isfinite(state.approximation_error) && state.approximation_error >= 0;
          Add(*result, builder.FirstShape(), 2); Add(*result, builder.LastShape(), 3); History(builder, graph, *result);
          if (o.solid_policy != 0 && !builder.MakeSolid()) {
            result->Message = "Solidification failed; a shell is retained only under the explicit allow-shell policy.";
            if (o.solid_policy == 1) { result->Result.Nullify(); state.done = 0; }
          } else { result->Result = builder.Shape(); result->Message = "Guided sweep completed; validity and approximation error are reported separately."; }
        } else result->Message = "OCCT guided sweep did not complete.";
      }
    } catch (const Standard_Failure& error) { state.done = 0; result->Result.Nullify(); result->Message = error.GetMessageString(); }
    Finish(*result, state); *output = RegisterFeatureResult(std::move(result)); *info = state;
  });
}

#include "Modeling/LocalFeatures.hxx"
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepOffsetAPI_DraftAngle.hxx>
#include <BRepOffsetAPI_MakeDraft.hxx>
#include <BRep_Tool.hxx>
#include <Geom_Surface.hxx>
#include <gp_Pln.hxx>
#include <numbers>
#include <set>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::LocalFeatures;
namespace {
void ValidateShellLimitProfile(const TopoDS_Shape& profile) {
  // OCCT 8.0.1 BRepFill_Draft::Fuse queries BRepTools_History with
  // compound/null sweep sections at corners, causing an uncatchable Debug CRT
  // assertion. Restrict limit-driven drafts to a verified analytic single-edge
  // boundary, in both configurations. Length-only drafts do not use Fuse.
  const auto topology = Map(profile);
  TopoDS_Edge boundary;
  int edges = 0;
  for (const auto& shape : topology) if (shape.ShapeType() == TopAbs_EDGE) {
    boundary = TopoDS::Edge(shape); ++edges;
  }
  Require(edges == 1, "OCCT 8.0.1 limit-driven shell draft requires one analytic line/circle boundary edge; cornered profiles can assert inside the SDK.");
  BRepAdaptor_Curve curve(boundary);
  Require(curve.GetType() == GeomAbs_Line || curve.GetType() == GeomAbs_Circle,
    "Limit-driven shell draft currently supports an analytic line/circle boundary only.");
}
void ShellHistory(BRepOffsetAPI_MakeDraft& algorithm, const InputGraph& graph, Result& result) {
  // BRepFill_Draft::Generated unconditionally casts its argument to an edge;
  // its apparent vertex branch is not safe in Debug. Modified/IsDeleted are
  // base-class defaults, not an available shell-draft evolution contract.
  result.Data().Info.group_support |= Evolution;
  const auto finalMap = Map(result.Owner->Result);
  const auto& topology = graph.Topology[0];
  ShapeMap laterals;
  for (int i = 1; i <= topology.Extent(); ++i) {
    const auto& edge = topology(i);
    if (edge.ShapeType() != TopAbs_EDGE) continue;
    bool mapped = false;
    for (const auto& generated : algorithm.Generated(edge)) {
      if (generated.IsNull()) continue;
      const int index = finalMap.FindIndex(generated);
      result.Add(index ? finalMap(index) : generated, Generated, 0, i - 1, -1, TopAbs_EDGE);
      for (const auto& member : Map(generated))
        if (member.ShapeType() == TopAbs_FACE && finalMap.Contains(member))
          laterals.Add(finalMap(finalMap.FindIndex(member)));
      mapped = true;
    }
    if (finalMap.Contains(edge)) result.Add(finalMap(finalMap.FindIndex(edge)), Unchanged, 0, i - 1, -1, TopAbs_EDGE);
    else if (!mapped) result.Add({}, Unmapped, 0, i - 1, -1, TopAbs_EDGE);
  }
  for (const auto& face : laterals) result.Add(face, Lateral);
}
void DraftFailure(BRepOffsetAPI_DraftAngle& algorithm, const InputGraph& graph, Result& result, int program) {
  const auto problem = algorithm.ProblematicShape(); const int status = static_cast<int>(algorithm.Status());
  const int index = graph.Index(0, problem); result.Data().Faults.push_back({2, program, index, status});
  result.Add(problem, ProblemShape, 0, index, program, problem.IsNull() ? -1 : problem.ShapeType());
  result.Fail("Draft addition/build failed; the problematic shape and SDK status are preserved.", status);
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_face_draft(const OcctSharp_ShapeHandle* source,
  const OcctSharp_FaceDraftProgram* programs, int32_t count, int32_t build, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output && programs && count >= 1 && count <= 256, "Invalid per-face draft programs."); Flag(build);
    InputGraph graph(&source, 1); Result result(2); std::set<int> affected;
    for (int i = 0; i < count; ++i) {
      const auto& p = programs[i]; graph.Subshape(0, p.face, TopAbs_FACE); Flag(p.propagation);
      Require(p.reserved1 == 0 && p.reserved2 == 0 && std::isfinite(p.angle)
        && std::abs(p.angle) > 1e-4 && std::abs(p.angle) < std::numbers::pi / 2, "Draft angles must exceed the OCCT no-op threshold and stay below pi/2.");
      Direction(p.direction); Point(p.plane_origin); Direction(p.plane_normal);
    }
    try {
      // Each probe discovers native propagation before a shared builder accepts
      // the complete disjoint face program. Add's Boolean is deliberately not
      // advertised as a switch that disables tangent propagation.
      for (int i = 0; i < count; ++i) {
        const auto& p = programs[i]; const auto& face = TopoDS::Face(graph.Subshape(0, p.face, TopAbs_FACE));
        BRepOffsetAPI_DraftAngle probe(graph.At(0));
        probe.Add(face, Direction(p.direction), p.angle, gp_Pln(Point(p.plane_origin), Direction(p.plane_normal)), true);
        if (!probe.AddDone()) { DraftFailure(probe, graph, result, i); result.Publish(output); return; }
        // ConnectedFaces requires Draft_Modification::IsDone(), not just AddDone.
        probe.Build();
        if (!probe.IsDone()) { DraftFailure(probe, graph, result, i); result.Publish(output); return; }
        auto connected = probe.ConnectedFaces(face); if (connected.IsEmpty()) connected.Append(face);
        for (const auto& item : connected) {
          const int index = graph.Index(0, item); Require(index >= 0, "Draft propagation escaped the source correspondence.");
          Require(p.propagation != 0 || index == p.face, "Draft would propagate beyond the explicitly selected face.");
          Require(affected.insert(index).second, "Per-face draft programs overlap through tangent propagation.");
          result.Add(item, AffectedFace, 0, index, i, TopAbs_FACE);
        }
      }
      BRepOffsetAPI_DraftAngle algorithm(graph.At(0));
      for (int i = 0; i < count; ++i) {
        const auto& p = programs[i];
        algorithm.Add(TopoDS::Face(graph.Subshape(0, p.face, TopAbs_FACE)), Direction(p.direction), p.angle,
          gp_Pln(Point(p.plane_origin), Direction(p.plane_normal)), true);
        if (!algorithm.AddDone()) { DraftFailure(algorithm, graph, result, i); result.Publish(output); return; }
      }
      for (const auto& face : algorithm.ModifiedFaces())
        Require(affected.contains(graph.Index(0, face)), "Effective draft faces exceed the preflight acceptance set.");
      result.Data().Info.ready = 1;
      if (build) {
        algorithm.Build(); result.Data().Info.done = algorithm.IsDone();
        if (algorithm.IsDone()) { result.Owner->Result = algorithm.Shape(); History(algorithm, graph, result); result.Owner->Message = "Per-face draft built with verified tangent-propagation correspondence."; }
        else DraftFailure(algorithm, graph, result, -1);
      }
    } catch (const Standard_Failure& error) { result.Fail(error.GetMessageString()); }
    result.Publish(output);
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_shell_draft(const OcctSharp_ShapeHandle* const* inputs,
  int32_t count, const OcctSharp_ShellDraftOptions* options, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(output && options, "Missing shell draft options/output."); const auto& o = *options;
    Require(o.limit_kind >= 0 && o.limit_kind <= 2 && count == (o.limit_kind == 0 ? 1 : 2), "Shell-draft limit inputs do not match its mode.");
    Flag(o.keep); Flag(o.internal_draft); Require(o.transition == 1 || o.transition == 2, "Draft transition must be right or round corner.");
    Require(std::isfinite(o.angle) && std::abs(o.angle) < std::numbers::pi / 2, "Invalid shell draft angle.");
    Positive(o.angle_minimum); Positive(o.angle_maximum); Require(o.angle_minimum < o.angle_maximum && o.angle_maximum <= std::numbers::pi, "Invalid draft transition angles.");
    if (o.limit_kind == 0) Positive(o.length);
    InputGraph graph(inputs, count); Result result(3); auto profile = graph.At(0);
    if (profile.ShapeType() == TopAbs_EDGE) profile = BRepBuilderAPI_MakeWire(TopoDS::Edge(profile)).Wire();
    Require(profile.ShapeType() == TopAbs_WIRE || profile.ShapeType() == TopAbs_FACE || profile.ShapeType() == TopAbs_SHELL,
      "Draft shells require an edge, wire, face or open shell.");
    if (o.limit_kind == 1) graph.Typed(1, TopAbs_FACE);
    if (o.limit_kind != 0) ValidateShellLimitProfile(profile);
    try {
      BRepOffsetAPI_MakeDraft algorithm(profile, Direction(o.direction), o.angle);
      algorithm.SetOptions(static_cast<BRepBuilderAPI_TransitionMode>(o.transition), o.angle_minimum, o.angle_maximum);
      algorithm.SetDraft(o.internal_draft != 0); result.Data().Info.ready = 1;
      if (o.limit_kind == 0) algorithm.Perform(o.length);
      else if (o.limit_kind == 1) algorithm.Perform(BRep_Tool::Surface(TopoDS::Face(graph.At(1))), o.keep != 0);
      else algorithm.Perform(graph.At(1), o.keep != 0);
      result.Data().Info.done = algorithm.IsDone();
      if (algorithm.IsDone()) {
        result.Owner->Result = algorithm.Shape(); ShellHistory(algorithm, graph, result); result.Data().Info.group_support |= Laterals;
        // Shell() is the pre-restriction sweep, not necessarily the final lateral
        // topology. Keep that provenance explicit; ShellHistory maps final faces.
        result.Add(algorithm.Shell(), PreLimitShape); if (count == 2) result.Add(graph.At(1), Limit, 1, 0);
        result.Owner->Message = "Draft-shell extent uses the requested length, support surface or stop shape.";
      } else result.Fail("Draft shell did not reach an eligible limit.");
    } catch (const Standard_Failure& error) { result.Fail(error.GetMessageString()); }
    result.Publish(output);
  });
}

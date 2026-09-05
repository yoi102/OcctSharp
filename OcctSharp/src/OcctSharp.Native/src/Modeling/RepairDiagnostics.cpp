#include "Modeling/Repair.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <BRepAdaptor_Curve.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepCheck_Result.hxx>
#include <BRepTools_WireExplorer.hxx>
#include <BRepGProp.hxx>
#include <BRep_Tool.hxx>
#include <GProp_GProps.hxx>
#include <ShapeAnalysis_CheckSmallFace.hxx>
#include <ShapeAnalysis_Shell.hxx>
#include <ShapeAnalysis_Wire.hxx>
#include <ShapeAnalysis_WireOrder.hxx>
#include <ShapeExtend_WireData.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Wire.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Iterator.hxx>
#include <cmath>
#include <queue>

namespace OcctSharp::Native::Repair {
namespace {
void WireFindings(const TopoDS_Wire& wire, const TopoDS_Face& face, const ShapeMap& map,
  const OcctSharp_RepairInspectionOptions& options, std::vector<OcctSharp_RepairFinding>& findings,
  OcctSharp_RepairMetrics& metrics) {
  int wireIndex = map.FindIndex(wire) - 1;
  std::vector<TopoDS_Edge> edges;
  // A valid TopoDS wire need not store its edges in traversal order. Prefer the
  // connected traversal when complete; retain every edge for damaged chains.
  std::vector<TopoDS_Edge> traversal;
  for (BRepTools_WireExplorer item(wire); item.More(); item.Next()) traversal.push_back(item.Current());
  int rawCount = 0; for (TopoDS_Iterator item(wire); item.More(); item.Next()) ++rawCount;
  if (traversal.size() != static_cast<size_t>(rawCount)) {
    traversal.clear();
    for (TopoDS_Iterator item(wire); item.More(); item.Next()) traversal.push_back(TopoDS::Edge(item.Value()));
  }
  ShapeAnalysis_WireOrder order(true, options.tolerance);
  bool endpointsKnown = true;
  std::vector<std::pair<gp_Pnt, gp_Pnt>> endpoints;
  for (const auto& edge : traversal) {
    edges.push_back(edge);
    if (BRep_Tool::Degenerated(edge)) findings.push_back({5, map.FindIndex(edge) - 1, wireIndex, 1, 0, options.small_length});
    try {
      BRepAdaptor_Curve curve(edge);
      gp_Pnt start = curve.Value(curve.FirstParameter()), end = curve.Value(curve.LastParameter());
      if (edge.Orientation() == TopAbs_REVERSED) std::swap(start, end);
      endpoints.emplace_back(start, end); order.Add(start.XYZ(), end.XYZ());
    } catch (const Standard_Failure&) { endpointsKnown = false; }
  }
  if (endpointsKnown && !edges.empty()) {
    order.Perform(wire.Closed());
    if (order.IsDone()) {
      order.SetChains(options.tolerance);
      if (order.NbChains() > 1) findings.push_back({2, wireIndex, -1, 1, double(order.NbChains()), 1});
      for (int i = 1; i <= order.NbEdges(); ++i) {
        int ordered = order.Ordered(i);
        if (ordered != i)
          findings.push_back({3, map.FindIndex(edges[std::abs(ordered) - 1]) - 1, wireIndex,
            ordered < 0 ? 2 : 1, double(i - 1), double(std::abs(ordered) - 1)});
      }
    } else findings.push_back({3, wireIndex, -1, 3, 0, 0});
    for (size_t i = 1; i < endpoints.size() + (wire.Closed() ? 1 : 0); ++i) {
      size_t next = i % endpoints.size();
      double gap = endpoints[i - 1].second.Distance(endpoints[next].first);
      metrics.maximum_gap = std::max(metrics.maximum_gap, gap);
      if (gap > options.tolerance)
        findings.push_back({4, map.FindIndex(edges[i - 1]) - 1, map.FindIndex(edges[next]) - 1, 1, gap, options.tolerance});
    }
  } else findings.push_back({4, wireIndex, -1, 3, 0, options.tolerance});
  ShapeAnalysis_Wire analysis(wire, face, options.tolerance);
  for (int i = wire.Closed() ? 1 : 2; i <= analysis.NbEdges(); ++i) {
    int previous = i > 1 ? i - 1 : analysis.NbEdges();
    int first = map.FindIndex(analysis.WireData()->Edge(previous)) - 1;
    int second = map.FindIndex(analysis.WireData()->Edge(i)) - 1;
    bool gap3d = analysis.CheckGap3d(i);
    bool unavailable3d = analysis.LastCheckStatus(ShapeExtend_FAIL);
    findings.push_back({12, first, second, unavailable3d ? 3 : gap3d ? 1 : 0,
      unavailable3d ? 0 : analysis.MinDistance3d(), options.tolerance});
    bool gap2d = analysis.CheckGap2d(i);
    bool unavailable2d = analysis.LastCheckStatus(ShapeExtend_FAIL);
    // UV distances are not document lengths. OCCT derives a surface-dependent
    // parametric threshold from precision; no world-space scalar limit is claimed.
    findings.push_back({13, first, second, unavailable2d ? 3 : gap2d ? 1 : 0,
      unavailable2d ? 0 : analysis.MinDistance2d(), 0});
  }
  // Wire self-intersection is a pcurve/face check. A missing support is not a clean result.
  if (face.IsNull() || edges.size() > 512) {
    findings.push_back({6, wireIndex, -1, 3, 0, 0}); return;
  }
  for (int i = 1; i <= analysis.NbEdges(); ++i) {
    if (analysis.CheckSelfIntersectingEdge(i))
      findings.push_back({6, map.FindIndex(analysis.WireData()->Edge(i)) - 1, -1, 1, 0, 0});
    else if (analysis.LastCheckStatus(ShapeExtend_FAIL))
      findings.push_back({6, map.FindIndex(analysis.WireData()->Edge(i)) - 1, -1, 3, 0, 0});
    for (int j = i + 1; j <= analysis.NbEdges(); ++j) {
      bool intersection = analysis.CheckIntersectingEdges(i, j);
      if (intersection || analysis.LastCheckStatus(ShapeExtend_FAIL))
        findings.push_back({6, map.FindIndex(analysis.WireData()->Edge(i)) - 1,
          map.FindIndex(analysis.WireData()->Edge(j)) - 1, intersection ? 1 : 3, 0, 0});
    }
  }
}
void ShellFindings(const TopoDS_Shape& shell, const ShapeMap& map,
  std::vector<OcctSharp_RepairFinding>& findings) {
  ShapeAnalysis_Shell analysis;
  analysis.CheckOrientedShells(shell, true, true);
  if (analysis.HasBadEdges())
    for (TopExp_Explorer edge(analysis.BadEdges(), TopAbs_EDGE); edge.More(); edge.Next())
      findings.push_back({7, map.FindIndex(edge.Current()) - 1, map.FindIndex(shell) - 1, 1, 0, 0});
  std::vector<ShapeMap> faceEdges;
  for (TopExp_Explorer face(shell, TopAbs_FACE); face.More(); face.Next()) {
    ShapeMap edges; TopExp::MapShapes(face.Current(), TopAbs_EDGE, edges); faceEdges.push_back(edges);
  }
  std::vector<bool> reached(faceEdges.size(), false); int components = 0;
  for (size_t start = 0; start < faceEdges.size(); ++start) {
    if (reached[start]) continue;
    ++components; std::queue<size_t> pending; pending.push(start); reached[start] = true;
    while (!pending.empty()) {
      auto current = pending.front(); pending.pop();
      for (size_t other = 0; other < faceEdges.size(); ++other) {
        if (reached[other]) continue;
        for (const auto& edge : faceEdges[current]) if (faceEdges[other].Contains(edge)) {
          reached[other] = true; pending.push(other); break;
        }
      }
    }
  }
  if (components > 1) findings.push_back({8, map.FindIndex(shell) - 1, -1, 1, double(components), 1});
}
}
std::vector<OcctSharp_RepairFinding> Inspect(const TopoDS_Shape& source,
  const OcctSharp_RepairInspectionOptions& options, OcctSharp_RepairMetrics& metrics) {
  Positive(options.tolerance, "Inspection tolerance must be positive.");
  Positive(options.small_length, "Small length must be positive.");
  Positive(options.small_area, "Small area must be positive.");
  Positive(options.tolerance_outlier, "Tolerance threshold must be positive.");
  auto map = Map(source); metrics = Metrics(source);
  std::vector<OcctSharp_RepairFinding> result;
  BRepCheck_Analyzer analyzer(source);
  ShapeAnalysis_CheckSmallFace small;
  for (int i = 1; i <= map.Extent(); ++i) {
    const auto& shape = map(i);
    auto checks = analyzer.Result(shape);
    if (!checks.IsNull()) for (auto status : checks->Status())
      if (status != BRepCheck_NoError) result.push_back({0, i - 1, -1, int(status), 0, 0});
    double tolerance = Tolerance(shape);
    if (tolerance > options.tolerance_outlier)
      result.push_back({1, i - 1, -1, 1, tolerance, options.tolerance_outlier});
    if (shape.ShapeType() == TopAbs_WIRE) {
      TopoDS_Face support;
      for (TopExp_Explorer face(source, TopAbs_FACE); face.More() && support.IsNull(); face.Next())
        for (TopoDS_Iterator wire(face.Current()); wire.More(); wire.Next())
          if (wire.Value().IsSame(shape)) { support = TopoDS::Face(face.Current()); break; }
      WireFindings(TopoDS::Wire(shape), support, map, options, result, metrics);
    }
    if (shape.ShapeType() == TopAbs_SHELL) ShellFindings(shape, map, result);
    if (shape.ShapeType() == TopAbs_FACE) {
      auto face = TopoDS::Face(shape); GProp_GProps area;
      BRepGProp::SurfaceProperties(face, area);
      if (std::abs(area.Mass()) < options.small_area)
        result.push_back({9, i - 1, -1, 1, std::abs(area.Mass()), options.small_area});
      if (small.CheckSpotFace(face, options.small_length)) result.push_back({11, i - 1, -1, 1, 0, options.small_length});
      TopoDS_Edge first, second;
      if (small.CheckStripFace(face, first, second, options.small_length))
        result.push_back({10, i - 1, map.FindIndex(first) - 1, 1, 0, options.small_length});
    }
  }
  return result;
}
}

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Repair;
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_inspect(
  const OcctSharp_ShapeHandle* source, const OcctSharp_RepairInspectionOptions* options,
  OcctSharp_RepairMetrics* metrics, OcctSharp_RepairFinding* findings, int32_t capacity, int32_t* count) {
  if (metrics) *metrics = {};
  return Guard([&] {
    ValidateUsableShape(source); Require(options && metrics, "Missing repair inspection options or output.");
    CopyBuffer(Inspect(source->Value, *options, *metrics), findings, capacity, count);
  });
}

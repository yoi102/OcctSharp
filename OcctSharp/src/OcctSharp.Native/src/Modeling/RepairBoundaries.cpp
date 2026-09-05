#include "Modeling/Repair.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <BRep_Builder.hxx>
#include <BRep_Tool.hxx>
#include <GProp_GProps.hxx>
#include <ShapeAnalysis_FreeBounds.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <cmath>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Repair;
namespace {
std::vector<TopoDS_Wire> Boundaries(const TopoDS_Shape& source) {
  std::vector<TopoDS_Wire> result;
  TopoDS_Shell shell; BRep_Builder builder; builder.MakeShell(shell);
  bool hasFaces = false;
  for (TopExp_Explorer face(source, TopAbs_FACE); face.More(); face.Next()) {
    builder.Add(shell, face.Current()); hasFaces = true;
  }
  if (hasFaces) {
    ShapeAnalysis_FreeBounds free(shell, false, true, false);
    for (const auto& group : {free.GetClosedWires(), free.GetOpenWires()})
      for (TopExp_Explorer wire(group, TopAbs_WIRE); wire.More(); wire.Next())
        result.push_back(TopoDS::Wire(wire.Current()));
  }
  for (TopExp_Explorer wire(source, TopAbs_WIRE, TopAbs_FACE); wire.More(); wire.Next())
    result.push_back(TopoDS::Wire(wire.Current()));
  return result;
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_boundary(
  const OcctSharp_ShapeHandle* source, double tolerance, int32_t index,
  OcctSharp_RepairBoundary* info, int32_t* source_edges, int32_t capacity,
  int32_t* edge_count, int32_t* boundary_count, OcctSharp_ShapeHandle** wire) {
  if (wire) *wire = nullptr;
  if (info) *info = {};
  return Guard([&] {
    ValidateUsableShape(source); Positive(tolerance, "Boundary tolerance must be positive.");
    Require(boundary_count && edge_count && capacity >= 0, "Missing boundary count output.");
    auto values = Boundaries(source->Value); *boundary_count = static_cast<int>(values.size()); *edge_count = 0;
    if (index == -1) return;
    Require(index >= 0 && index < *boundary_count && info && wire, "Invalid boundary index or output.");
    auto map = Map(source->Value); const auto& value = values[index];
    std::vector<int32_t> indices;
    for (TopExp_Explorer edge(value, TopAbs_EDGE); edge.More(); edge.Next())
      indices.push_back(map.FindIndex(edge.Current()) - 1);
    CopyBuffer(indices, source_edges, capacity, edge_count);
    if (!source_edges && capacity == 0) return;
    info->closed = BRep_Tool::IsClosed(value); info->edge_count = *edge_count;
    GProp_GProps length; BRepGProp::LinearProperties(value, length); info->length = length.Mass();
    TopoDS_Vertex first, last; TopExp::Vertices(value, first, last);
    info->endpoint_gap = first.IsNull() || last.IsNull() ? -1 : BRep_Tool::Pnt(first).Distance(BRep_Tool::Pnt(last));
    if (info->closed) {
      // Face construction can attach pcurves to its wire. Measurement must not
      // modify the source snapshot or change its portable fingerprint.
      BRepBuilderAPI_MakeFace planar(TopoDS::Wire(Copy(value)), true);
      if (planar.IsDone() && BRepCheck_Analyzer(planar.Face()).IsValid()) {
        GProp_GProps area; BRepGProp::SurfaceProperties(planar.Face(), area);
        info->area = std::abs(area.Mass()); info->area_available = std::isfinite(info->area);
      }
    }
    *wire = AllocateShape(Copy(value));
  });
}

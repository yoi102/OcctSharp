#include "Modeling/Repair.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <BRep_Builder.hxx>
#include <BRep_Tool.hxx>
#include <BRepTools.hxx>
#include <GProp_GProps.hxx>
#include <Geom_Curve.hxx>
#include <ShapeBuild_Edge.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Iterator.hxx>
#include <cmath>
#include <sstream>

namespace OcctSharp::Native::Repair {
void Require(bool condition, const char* message) {
  if (!condition) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
void Positive(double value, const char* message) { Require(std::isfinite(value) && value > 0, message); }
ShapeMap Map(const TopoDS_Shape& shape) {
  ShapeMap result;
  if (!shape.IsNull()) TopExp::MapShapes(shape, result);
  return result;
}
double Tolerance(const TopoDS_Shape& shape) {
  switch (shape.ShapeType()) {
    case TopAbs_VERTEX: return BRep_Tool::Tolerance(TopoDS::Vertex(shape));
    case TopAbs_EDGE: return BRep_Tool::Tolerance(TopoDS::Edge(shape));
    case TopAbs_FACE: return BRep_Tool::Tolerance(TopoDS::Face(shape));
    default: return 0;
  }
}
TopoDS_Shape Copy(const TopoDS_Shape& source, std::vector<TopoDS_Shape>* correspondence) {
  Require(!source.IsNull(), "Cannot copy null repair topology.");
  BRepBuilderAPI_Copy copier(source, true, false);
  Require(copier.IsDone(), "Independent repair copy failed.");
  auto map = Map(source);
  // The generic copier omits representations on faces outside the copied closure.
  // Preserve those pcurves as copies. In-closure representations still reference the
  // copied surfaces; any additional outside support is read-only during repair.
  ShapeBuild_Edge edgeBuilder;
  for (const auto& item : map) {
    auto copied = copier.ModifiedShape(item);
    if (!copied.IsNull()) copied.Orientation(item.Orientation());
    if (item.ShapeType() == TopAbs_EDGE && !copied.IsNull()) {
      bool hasSupport = false;
      for (TopExp_Explorer face(source, TopAbs_FACE); face.More() && !hasSupport; face.Next())
        hasSupport = Map(face.Current()).Contains(item);
      if (!hasSupport) edgeBuilder.CopyPCurves(TopoDS::Edge(copied), TopoDS::Edge(item));
    }
    if (correspondence) correspondence->push_back(copied);
  }
  return copier.Shape();
}
OcctSharp_RepairMetrics Metrics(const TopoDS_Shape& shape) {
  OcctSharp_RepairMetrics result{};
  auto map = Map(shape); result.topology_count = map.Extent();
  if (shape.IsNull()) return result;
  result.valid = BRepCheck_Analyzer(shape).IsValid();
  bool hasFace = false, hasSolid = false, allClosed = true;
  for (const auto& item : map) {
    result.maximum_tolerance = std::max(result.maximum_tolerance, Tolerance(item));
    hasFace |= item.ShapeType() == TopAbs_FACE;
    hasSolid |= item.ShapeType() == TopAbs_SOLID;
    if (item.ShapeType() == TopAbs_SHELL) allClosed &= BRep_Tool::IsClosed(item);
  }
  GProp_GProps area, volume;
  if (hasFace) {
    BRepGProp::SurfaceProperties(shape, area);
    result.area = area.Mass();
    result.area_available = result.valid && std::isfinite(result.area);
  }
  if (hasSolid) {
    BRepGProp::VolumeProperties(shape, volume, true);
    result.volume = volume.Mass();
    result.volume_available = result.valid && allClosed && std::isfinite(result.volume);
  }
  return result;
}
const OcctSharp_RepairResultHandle& Checked(const OcctSharp_RepairResultHandle* result) {
  if (!result) throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "Repair result is null.");
  if (!IsLiveValue(result, LiveRepairResults))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "Repair result is released or invalid.");
  return *result;
}
}

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Repair;

OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_serialized(
  const OcctSharp_ShapeHandle* source, uint8_t* output, int32_t capacity, int32_t* count) {
  return Guard([&] {
    ValidateUsableShape(source); std::ostringstream stream;
    BRepTools::Write(source->Value, stream, false, false, TopTools_FormatVersion_CURRENT);
    auto value = stream.str();
    Require(value.size() <= 268435456, "Portable repair identity exceeds the 256 MiB bound.");
    CopyBuffer(std::vector<uint8_t>(value.begin(), value.end()), output, capacity, count);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_copy(
  const OcctSharp_ShapeHandle* source, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] { ValidateUsableShape(source); Require(output, "Missing copy output.");
    *output = AllocateShape(Copy(source->Value)); });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_topology(
  const OcctSharp_ShapeHandle* source, OcctSharp_RepairTopology* output, int32_t capacity, int32_t* count) {
  return Guard([&] {
    ValidateUsableShape(source); auto map = Map(source->Value);
    std::vector<int> parents(map.Extent(), -1);
    for (int i = 1; i <= map.Extent(); ++i)
      for (TopoDS_Iterator child(map(i)); child.More(); child.Next()) {
        int index = map.FindIndex(child.Value());
        if (index && parents[index - 1] < 0) parents[index - 1] = i - 1;
      }
    std::vector<OcctSharp_RepairTopology> values;
    for (int i = 1; i <= map.Extent(); ++i)
      values.push_back({i - 1, map(i).ShapeType(), map(i).Orientation(), parents[i - 1], Tolerance(map(i))});
    CopyBuffer(values, output, capacity, count);
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_subshape(
  const OcctSharp_ShapeHandle* source, int32_t index, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    ValidateUsableShape(source); auto map = Map(source->Value);
    Require(output && index >= 0 && index < map.Extent(), "Repair topology index is out of range.");
    *output = AllocateShape(Copy(map(index + 1)));
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_shape(
  const OcctSharp_RepairResultHandle* result, OcctSharp_ShapeHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] { const auto& value = Checked(result); Require(output, "Missing repair shape output.");
    *output = AllocateShape(value.Shape); });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_history(
  const OcctSharp_RepairResultHandle* result, OcctSharp_RepairRelation* output, int32_t capacity, int32_t* count) {
  return Guard([&] { CopyBuffer(Checked(result).History, output, capacity, count); });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_findings(
  const OcctSharp_RepairResultHandle* result, OcctSharp_RepairFinding* output, int32_t capacity, int32_t* count) {
  return Guard([&] { CopyBuffer(Checked(result).Findings, output, capacity, count); });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_result_release(OcctSharp_RepairResultHandle* result) {
  return Guard([&] { if (!result) return; Checked(result);
    Require(UnregisterValue(result, LiveRepairResults), "Repair result was already released."); delete result; });
}

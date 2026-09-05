#include "Modeling/Regions.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include <BRepAlgoAPI_Check.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepGProp.hxx>
#include <BRepClass3d_SolidClassifier.hxx>
#include <GProp_GProps.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <algorithm>
#include <cmath>

static_assert(sizeof(OcctSharp_RegionInfo) == 32);
static_assert(sizeof(OcctSharp_RegionItem) == 32);
static_assert(sizeof(OcctSharp_PartitionOptions) == 24);
static_assert(sizeof(OcctSharp_RegionRule) == 32);
static_assert(sizeof(OcctSharp_RegionOutput) == 16);
static_assert(sizeof(OcctSharp_VolumeOptions) == 24);

namespace OcctSharp::Native::Regions {
int Dimension(const TopoDS_Shape& shape) {
  switch (shape.ShapeType()) {
  case TopAbs_VERTEX: return 0;
  case TopAbs_EDGE: case TopAbs_WIRE: return 1;
  case TopAbs_FACE: case TopAbs_SHELL: return 2;
  case TopAbs_SOLID: case TopAbs_COMPSOLID: return 3;
  default: return -1;
  }
}
double Measure(const TopoDS_Shape& shape, int dimension) {
  GProp_GProps properties;
  if (dimension == 0) return 1;
  if (dimension == 1) BRepGProp::LinearProperties(shape, properties);
  else if (dimension == 2) BRepGProp::SurfaceProperties(shape, properties);
  else if (dimension == 3) BRepGProp::VolumeProperties(shape, properties);
  else return 0;
  return std::abs(properties.Mass());
}
void Add(OcctSharp_RegionData& data, int kind, int a, int b, int c, int d,
  int flags, double measure, const TopoDS_Shape& shape) {
  if (data.Items.size() >= 1000000) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Region snapshot exceeds one million records.");
  data.Items.push_back({kind, a, b, c, d, flags, measure});
  data.Shapes.push_back(shape);
}
bool CheckInputs(const std::vector<TopoDS_Shape>& inputs, OcctSharp_FeatureResultHandle& owner) {
  bool valid = true;
  for (int i = 0; i < static_cast<int>(inputs.size()); ++i) {
    BRepAlgoAPI_Check check(inputs[i], true, true);
    if (!check.IsValid()) {
      Add(*owner.Regions, Fault, i, -1, 1, -1, 0, 0, inputs[i]); valid = false;
      ShapeMap topology; TopExp::MapShapes(inputs[i], topology);
      for (const auto& finding : check.Result()) for (const auto& shape : finding.GetFaultyShapes1())
        Add(*owner.Regions, Fault, i, topology.FindIndex(shape) - 1, finding.GetCheckStatus(), -1, 0, 0, shape);
    }
  }
  if (!valid) owner.Message = "Invalid or self-interfering partition/volume arguments; inspect source-indexed faults.";
  return valid;
}
void InspectBoundaries(const std::vector<TopoDS_Shape>& cells, OcctSharp_RegionData& data) {
  ShapeMap boundaries;
  for (int i = 0; i < static_cast<int>(cells.size()); ++i) {
    const int dimension = Dimension(cells[i]);
    if (dimension == 0) continue;
    const auto kind = dimension == 3 ? TopAbs_FACE : dimension == 2 ? TopAbs_EDGE : TopAbs_VERTEX;
    for (TopExp_Explorer e(cells[i], kind); e.More(); e.Next()) {
      const auto& boundary = e.Current();
      int index = boundaries.FindIndex(boundary);
      if (!index) {
        index = boundaries.Add(boundary);
        Add(data, Boundary, index - 1, dimension - 1, static_cast<int>(kind), -1, 0,
          Measure(boundary, dimension - 1), boundary);
      }
      Add(data, BoundaryUse, index - 1, i, static_cast<int>(boundary.Orientation()));
    }
  }
}
void Publish(std::unique_ptr<OcctSharp_FeatureResultHandle> owner, OcctSharp_FeatureResultHandle** output) {
  auto& info = owner->Regions->Info;
  info.item_count = static_cast<int>(owner->Regions->Items.size());
  owner->Info.succeeded = info.done; owner->Info.result_is_valid = info.valid;
  *output = RegisterFeatureResult(std::move(owner));
}
}

using namespace OcctSharp::Native;
OcctSharp_Status OCCTSHARP_CALL occtsharp_region_snapshot(const OcctSharp_FeatureResultHandle* result,
  OcctSharp_RegionInfo* info, OcctSharp_RegionItem* items, int32_t capacity) {
  if (info) *info = {};
  return Guard([&] {
    if (!info || capacity < 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Invalid region snapshot output.");
    ValidateFeatureResult(result);
    if (!result->Regions) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Not a region result.");
    const auto& data = *result->Regions;
    if (items || capacity) {
      if (!items || capacity < data.Info.item_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Insufficient region snapshot capacity.");
      std::copy(data.Items.begin(), data.Items.end(), items);
    }
    *info = data.Info;
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_region_item_shape(const OcctSharp_FeatureResultHandle* result,
  int32_t index, OcctSharp_ShapeHandle** shape) {
  if (shape) *shape = nullptr;
  return Guard([&] {
    if (!shape) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Missing region topology output.");
    ValidateFeatureResult(result);
    if (!result->Regions || index < 0 || index >= static_cast<int>(result->Regions->Shapes.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Invalid region item index.");
    const auto& value = result->Regions->Shapes[index];
    // This owning value is private to the managed result. Public getters copy it.
    if (!value.IsNull()) *shape = AllocateShape(value);
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_region_classify_solid(const OcctSharp_ShapeHandle* solid,
  OcctSharp_Xyz point, double tolerance, int32_t* state) {
  if (state) *state = TopAbs_UNKNOWN;
  return Guard([&] {
    if (!state || !std::isfinite(tolerance) || tolerance <= 0 || !std::isfinite(point.x)
      || !std::isfinite(point.y) || !std::isfinite(point.z))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Invalid volume classification query.");
    ValidateUsableShape(solid);
    if (solid->Value.ShapeType() != TopAbs_SOLID)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Point classification requires one solid.");
    BRepClass3d_SolidClassifier classifier(solid->Value, gp_Pnt(point.x, point.y, point.z), tolerance);
    *state = static_cast<int>(classifier.State());
  });
}

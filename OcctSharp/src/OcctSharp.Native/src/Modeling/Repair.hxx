#pragma once
#include "OcctSharp.Native.Repair.h"
#include <TopoDS_Shape.hxx>
#include <NCollection_IndexedMap.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <ShapeBuild_ReShape.hxx>
#include <BRepTools_History.hxx>
#include <memory>
#include <algorithm>
#include <vector>

static_assert(sizeof(OcctSharp_RepairTopology) == 24);
static_assert(sizeof(OcctSharp_RepairFinding) == 32);
static_assert(sizeof(OcctSharp_RepairMetrics) == 48);
static_assert(sizeof(OcctSharp_RepairInspectionOptions) == 32);
static_assert(sizeof(OcctSharp_RepairStage) == 56);
static_assert(sizeof(OcctSharp_RepairRelation) == 16);
static_assert(sizeof(OcctSharp_RepairBoundary) == 40);

struct OcctSharp_RepairResultHandle {
  TopoDS_Shape Shape;
  std::vector<OcctSharp_RepairRelation> History;
  std::vector<OcctSharp_RepairFinding> Findings;
};

namespace OcctSharp::Native::Repair {
using ShapeMap = NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher>;
void Require(bool condition, const char* message);
void Positive(double value, const char* message);
ShapeMap Map(const TopoDS_Shape& shape);
double Tolerance(const TopoDS_Shape& shape);
TopoDS_Shape Copy(const TopoDS_Shape& source, std::vector<TopoDS_Shape>* correspondence = nullptr);
OcctSharp_RepairMetrics Metrics(const TopoDS_Shape& shape);
std::vector<OcctSharp_RepairFinding> Inspect(const TopoDS_Shape& source,
  const OcctSharp_RepairInspectionOptions& options, OcctSharp_RepairMetrics& metrics);
struct Outcome {
  TopoDS_Shape Shape;
  occ::handle<ShapeBuild_ReShape> Context;
  occ::handle<BRepTools_History> History;
  std::vector<OcctSharp_RepairFinding> Findings;
};
Outcome Fix(const TopoDS_Shape& shape, const OcctSharp_RepairStage& options);
Outcome Normalize(const TopoDS_Shape& shape, const OcctSharp_RepairStage& options,
  const std::vector<TopoDS_Shape>& protectedShapes, const std::vector<TopoDS_Shape>& selectedHoles = {});
Outcome Sew(const TopoDS_Shape& shape, const OcctSharp_RepairStage& options);
const OcctSharp_RepairResultHandle& Checked(const OcctSharp_RepairResultHandle* result);
template<class T> void CopyBuffer(const std::vector<T>& values, T* output, int capacity, int* count) {
  Require(count && capacity >= 0, "Invalid repair result buffer.");
  *count = static_cast<int>(values.size());
  if (!output && capacity == 0) return;
  Require(output && capacity >= *count, "Repair result buffer is too small.");
  std::copy(values.begin(), values.end(), output);
}
}

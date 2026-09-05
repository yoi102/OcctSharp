#pragma once

// Private native Modeling/Topology contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Shape.hxx"
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS_Shape.hxx>
#include <vector>

namespace OcctSharp::Native
{
struct ValidationData
{
  bool IsValid = false;
  std::vector<OcctSharp_ValidationIssue> Issues;
};

bool SupportsShapeHistory(const TopoDS_Shape& shape);

// Exact algorithms may accept edges/wires, but must not infer surfaces from cached triangles.
void RequireExactFaceSupport(const TopoDS_Shape& shape);

int32_t CheckedTopologyCount(const TopoDS_Shape& shape, const TopAbs_ShapeEnum kind, const bool unique);

OcctSharp_TopologyCounts BuildTopologyCounts(const TopoDS_Shape& shape, const bool unique);

void BuildToleranceRange(
  const TopoDS_Shape& shape,
  const TopAbs_ShapeEnum kind,
  double& minimum,
  double& maximum);

bool IsTopologyClosed(const TopoDS_Shape& shape);

ValidationData BuildValidationData(
  const OcctSharp_ShapeHandle* shape,
  const bool geometryChecks,
  const bool exact);
}

#pragma once

// Private native Modeling/Inspection contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Shape.hxx"
#include "../../generated/Modeling/OcctSharp.PeriodicExactDistance.Generated.hxx"
#include <gp_Mat.hxx>

namespace OcctSharp::Native
{
OcctSharp_PeriodicExactDistance ComputeExactDistance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second);

void CopyInertia(const gp_Mat& matrix, OcctSharp_InspectionProperties& properties);
bool RequiresInspectionSpanPartition(const TopoDS_Shape& shape);

// Partitions copied knot spans before adaptive integration. The source is never modified.
double ComputeAdaptiveProperties(const TopoDS_Shape& shape, int32_t propertyKind,
  double relativeTolerance, OcctSharp_InspectionProperties& properties);
}

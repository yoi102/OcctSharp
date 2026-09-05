#pragma once

// Private native Modeling/Inspection contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Shape.hxx"
#include <BRepExtrema_DistShapeShape.hxx>
#include <gp_Mat.hxx>

namespace OcctSharp::Native
{
BRepExtrema_DistShapeShape ComputeExactDistance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second);

void CopyInertia(const gp_Mat& matrix, OcctSharp_InspectionProperties& properties);
}

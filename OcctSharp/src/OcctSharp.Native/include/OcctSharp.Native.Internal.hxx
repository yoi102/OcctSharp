#pragma once

#include "OcctSharp.Native.h"

#include <TopoDS_Shape.hxx>

// Internal C++ bridge support shared with generated translation units.
// This header is not part of the public C ABI.
void OcctSharp_Internal_SetLastError(const char* message);
OcctSharp_Status OcctSharp_Internal_TryGetShape(
  const OcctSharp_ShapeHandle* handle,
  const TopoDS_Shape** out_shape);
OcctSharp_ShapeHandle* OcctSharp_Internal_AllocateShape(TopoDS_Shape shape);

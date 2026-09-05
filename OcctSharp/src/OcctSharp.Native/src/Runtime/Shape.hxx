#pragma once

// Private native Runtime/Shape contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <TopoDS_Shape.hxx>
#include <utility>

struct OcctSharp_ShapeHandle
{
  explicit OcctSharp_ShapeHandle(TopoDS_Shape shape)
    : Value(std::move(shape))
  {
  }

  TopoDS_Shape Value;
};

namespace OcctSharp::Native
{
void RegisterShape(OcctSharp_ShapeHandle* shape);

bool IsLiveShape(const OcctSharp_ShapeHandle* shape);

bool UnregisterShape(const OcctSharp_ShapeHandle* shape);

OcctSharp_ShapeHandle* AllocateShape(TopoDS_Shape shape);

void ValidateShape(const OcctSharp_ShapeHandle* shape);

void ValidateUsableShape(const OcctSharp_ShapeHandle* shape);
}

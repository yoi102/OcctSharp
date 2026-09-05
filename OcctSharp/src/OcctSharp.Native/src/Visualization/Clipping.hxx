#pragma once

// Private native Visualization/Clipping contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Visualization/Context.hxx"
#include <Graphic3d_ClipPlane.hxx>
#include <Standard_Handle.hxx>

namespace OcctSharp::Native
{
opencascade::handle<Graphic3d_ClipPlane> FindClipPlane(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t clipPlaneId);
}

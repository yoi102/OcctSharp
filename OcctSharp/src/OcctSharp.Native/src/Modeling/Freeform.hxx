#pragma once

// Private native Modeling/Freeform contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Shape.hxx"
#include <Geom_Curve.hxx>
#include <Geom_Surface.hxx>
#include <Standard_Handle.hxx>

namespace OcctSharp::Native
{
opencascade::handle<Geom_Curve> GetEdgeCurve(const OcctSharp_ShapeHandle* edge);

opencascade::handle<Geom_Surface> GetFaceSurface(const OcctSharp_ShapeHandle* face);

GeomAbs_Shape ToContinuity(const int32_t value);
}

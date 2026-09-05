#pragma once

// Private native Geometry/Conversions contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

namespace OcctSharp::Native
{
gp_Pnt ToPoint(const OcctSharp_Xyz& value, const char* message);

gp_Vec ToVector(const OcctSharp_Xyz& value, const char* message);

OcctSharp_Xyz FromPoint(const gp_Pnt& value);

OcctSharp_Xyz CopyPoint(const gp_Pnt& point);

gp_Pnt ToPoint(const OcctSharp_Xyz& value);

gp_Dir ToDirection(const OcctSharp_Xyz& value);

gp_Ax2 ToAxis(const OcctSharp_Ax2& value);

gp_Pln ToPlane(const OcctSharp_Plane& value);

OcctSharp_Ax2 CopyAxis(const gp_Ax2& value);

OcctSharp_Plane CopyPlane(const gp_Pln& value);

bool IsFinite(const OcctSharp_Xyz& value);

void ValidateColor(const double red, const double green, const double blue);
}

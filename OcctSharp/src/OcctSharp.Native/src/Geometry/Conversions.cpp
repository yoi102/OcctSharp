// Native Geometry/Conversions implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <cmath>
#include <gp.hxx>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

namespace OcctSharp::Native
{
gp_Pnt ToPoint(const OcctSharp_Xyz& value, const char* message)
{
  if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  return gp_Pnt(value.x, value.y, value.z);
}

gp_Vec ToVector(const OcctSharp_Xyz& value, const char* message)
{
  if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  const gp_Vec result(value.x, value.y, value.z);
  if (result.SquareMagnitude() <= gp::Resolution())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  return result;
}

OcctSharp_Xyz FromPoint(const gp_Pnt& value)
{
  return {value.X(), value.Y(), value.Z()};
}

OcctSharp_Xyz CopyPoint(const gp_Pnt& point)
{
  return { point.X(), point.Y(), point.Z() };
}

gp_Pnt ToPoint(const OcctSharp_Xyz& value) { return gp_Pnt(value.x, value.y, value.z); }

gp_Dir ToDirection(const OcctSharp_Xyz& value) { return gp_Dir(value.x, value.y, value.z); }

gp_Ax2 ToAxis(const OcctSharp_Ax2& value)
{
  return gp_Ax2(ToPoint(value.origin), ToDirection(value.direction), ToDirection(value.x_direction));
}

gp_Pln ToPlane(const OcctSharp_Plane& value) { return gp_Pln(ToPoint(value.origin), ToDirection(value.normal)); }

OcctSharp_Ax2 CopyAxis(const gp_Ax2& value)
{
  return {
    CopyPoint(value.Location()),
    { value.XDirection().X(), value.XDirection().Y(), value.XDirection().Z() },
    { value.YDirection().X(), value.YDirection().Y(), value.YDirection().Z() },
    { value.Direction().X(), value.Direction().Y(), value.Direction().Z() }
  };
}

OcctSharp_Plane CopyPlane(const gp_Pln& value)
{
  return {
    CopyPoint(value.Location()),
    { value.Axis().Direction().X(), value.Axis().Direction().Y(), value.Axis().Direction().Z() }
  };
}

bool IsFinite(const OcctSharp_Xyz& value)
{
  return std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z);
}

void ValidateColor(const double red, const double green, const double blue)
{
  if (!std::isfinite(red) || !std::isfinite(green) || !std::isfinite(blue)
      || red < 0.0 || red > 1.0 || green < 0.0 || green > 1.0 || blue < 0.0 || blue > 1.0)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Viewer RGB components must be finite values in the inclusive range 0 to 1.");
  }
}
}

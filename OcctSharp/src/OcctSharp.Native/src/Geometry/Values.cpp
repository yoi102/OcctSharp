// Native Geometry/Values implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <gp_Ax2.hxx>
#include <gp_Ax3.hxx>
#include <gp_Circ.hxx>
#include <gp_Dir.hxx>
#include <gp_Lin.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>
#include <gp_XYZ.hxx>

using namespace OcctSharp::Native;

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_default(void)
{
  const gp_XYZ value;
  return { value.X(), value.Y(), value.Z() };
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_create(const double x, const double y, const double z)
{
  const gp_XYZ value(x, y, z);
  return { value.X(), value.Y(), value.Z() };
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_copy(const OcctSharp_Xyz value)
{
  return value;
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_added(const OcctSharp_Xyz left, const OcctSharp_Xyz right)
{
  const gp_XYZ result = gp_XYZ(left.x, left.y, left.z).Added(gp_XYZ(right.x, right.y, right.z));
  return { result.X(), result.Y(), result.Z() };
}

OcctSharp_Xyz OCCTSHARP_CALL occtsharp_gp_xyz_crossed(const OcctSharp_Xyz left, const OcctSharp_Xyz right)
{
  const gp_XYZ result = gp_XYZ(left.x, left.y, left.z).Crossed(gp_XYZ(right.x, right.y, right.z));
  return { result.X(), result.Y(), result.Z() };
}

double OCCTSHARP_CALL occtsharp_gp_xyz_dot(const OcctSharp_Xyz left, const OcctSharp_Xyz right)
{
  return gp_XYZ(left.x, left.y, left.z).Dot(gp_XYZ(right.x, right.y, right.z));
}

double OCCTSHARP_CALL occtsharp_gp_xyz_modulus(const OcctSharp_Xyz value)
{
  return gp_XYZ(value.x, value.y, value.z).Modulus();
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_xyz_normalized(const OcctSharp_Xyz value, OcctSharp_Xyz* result)
{
  if (result == nullptr) { SetLastError("The gp_XYZ normalized output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_XYZ normalized = gp_XYZ(value.x, value.y, value.z).Normalized();
    *result = { normalized.X(), normalized.Y(), normalized.Z() };
  });
}

OcctSharp_Line OCCTSHARP_CALL occtsharp_gp_lin_default(void)
{
  const gp_Lin line;
  return { { line.Location().X(), line.Location().Y(), line.Location().Z() }, { line.Direction().X(), line.Direction().Y(), line.Direction().Z() } };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_lin_create(const OcctSharp_Xyz origin, const OcctSharp_Xyz direction, OcctSharp_Line* result)
{
  if (result == nullptr) { SetLastError("The gp_Lin output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Lin line(gp_Pnt(origin.x, origin.y, origin.z), gp_Dir(direction.x, direction.y, direction.z));
    *result = { { line.Location().X(), line.Location().Y(), line.Location().Z() }, { line.Direction().X(), line.Direction().Y(), line.Direction().Z() } };
  });
}

OcctSharp_Line OCCTSHARP_CALL occtsharp_gp_lin_reversed(const OcctSharp_Line value)
{
  const gp_Lin source(gp_Pnt(value.origin.x, value.origin.y, value.origin.z), gp_Dir(value.direction.x, value.direction.y, value.direction.z));
  const gp_Lin line = source.Reversed();
  return { { line.Location().X(), line.Location().Y(), line.Location().Z() }, { line.Direction().X(), line.Direction().Y(), line.Direction().Z() } };
}

double OCCTSHARP_CALL occtsharp_gp_lin_distance(const OcctSharp_Line line, const OcctSharp_Xyz point)
{
  return gp_Lin(gp_Pnt(line.origin.x, line.origin.y, line.origin.z), gp_Dir(line.direction.x, line.direction.y, line.direction.z)).Distance(gp_Pnt(point.x, point.y, point.z));
}

double OCCTSHARP_CALL occtsharp_gp_lin_angle(const OcctSharp_Line left, const OcctSharp_Line right)
{
  return gp_Lin(gp_Pnt(left.origin.x, left.origin.y, left.origin.z), gp_Dir(left.direction.x, left.direction.y, left.direction.z)).Angle(gp_Lin(gp_Pnt(right.origin.x, right.origin.y, right.origin.z), gp_Dir(right.direction.x, right.direction.y, right.direction.z)));
}

OcctSharp_Circle OCCTSHARP_CALL occtsharp_gp_circ_default(void)
{
  const gp_Circ circle;
  return { { circle.Location().X(), circle.Location().Y(), circle.Location().Z() }, { circle.Axis().Direction().X(), circle.Axis().Direction().Y(), circle.Axis().Direction().Z() }, circle.Radius() };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_circ_create(const OcctSharp_Xyz center, const OcctSharp_Xyz normal, const double radius, OcctSharp_Circle* result)
{
  if (result == nullptr) { SetLastError("The gp_Circ output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Circ circle(gp_Ax2(gp_Pnt(center.x, center.y, center.z), gp_Dir(normal.x, normal.y, normal.z)), radius);
    *result = { { circle.Location().X(), circle.Location().Y(), circle.Location().Z() }, { circle.Axis().Direction().X(), circle.Axis().Direction().Y(), circle.Axis().Direction().Z() }, circle.Radius() };
  });
}

double OCCTSHARP_CALL occtsharp_gp_circ_area(const OcctSharp_Circle value)
{ return gp_Circ(gp_Ax2(gp_Pnt(value.center.x, value.center.y, value.center.z), gp_Dir(value.normal.x, value.normal.y, value.normal.z)), value.radius).Area(); }

double OCCTSHARP_CALL occtsharp_gp_circ_length(const OcctSharp_Circle value)
{ return gp_Circ(gp_Ax2(gp_Pnt(value.center.x, value.center.y, value.center.z), gp_Dir(value.normal.x, value.normal.y, value.normal.z)), value.radius).Length(); }

double OCCTSHARP_CALL occtsharp_gp_circ_distance(const OcctSharp_Circle value, const OcctSharp_Xyz point)
{ return gp_Circ(gp_Ax2(gp_Pnt(value.center.x, value.center.y, value.center.z), gp_Dir(value.normal.x, value.normal.y, value.normal.z)), value.radius).Distance(gp_Pnt(point.x, point.y, point.z)); }

OcctSharp_Ax2 OCCTSHARP_CALL occtsharp_gp_ax2_default(void)
{
  const gp_Ax2 axis;
  return { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() }, { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() }, { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() }, { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_ax2_create(const OcctSharp_Xyz origin, const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction, OcctSharp_Ax2* result)
{
  if (result == nullptr) { SetLastError("The gp_Ax2 output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Ax2 axis(gp_Pnt(origin.x, origin.y, origin.z), gp_Dir(normal.x, normal.y, normal.z), gp_Dir(x_direction.x, x_direction.y, x_direction.z));
    *result = { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() }, { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() }, { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() }, { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
  });
}

double OCCTSHARP_CALL occtsharp_gp_ax2_angle(const OcctSharp_Ax2 left, const OcctSharp_Ax2 right)
{
  const gp_Ax2 a(gp_Pnt(left.origin.x, left.origin.y, left.origin.z), gp_Dir(left.direction.x, left.direction.y, left.direction.z), gp_Dir(left.x_direction.x, left.x_direction.y, left.x_direction.z));
  const gp_Ax2 b(gp_Pnt(right.origin.x, right.origin.y, right.origin.z), gp_Dir(right.direction.x, right.direction.y, right.direction.z), gp_Dir(right.x_direction.x, right.x_direction.y, right.x_direction.z));
  return a.Angle(b);
}

OcctSharp_Ax3 OCCTSHARP_CALL occtsharp_gp_ax3_default(void)
{
  const gp_Ax3 axis;
  return { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() },
           { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() },
           { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() },
           { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_ax3_create(
  const OcctSharp_Xyz origin, const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction,
  OcctSharp_Ax3* result)
{
  if (result == nullptr) { SetLastError("The gp_Ax3 output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Ax3 axis(gp_Pnt(origin.x, origin.y, origin.z),
                      gp_Dir(normal.x, normal.y, normal.z),
                      gp_Dir(x_direction.x, x_direction.y, x_direction.z));
    *result = { { axis.Location().X(), axis.Location().Y(), axis.Location().Z() },
                { axis.XDirection().X(), axis.XDirection().Y(), axis.XDirection().Z() },
                { axis.YDirection().X(), axis.YDirection().Y(), axis.YDirection().Z() },
                { axis.Direction().X(), axis.Direction().Y(), axis.Direction().Z() } };
  });
}

int32_t OCCTSHARP_CALL occtsharp_gp_ax3_direct(const OcctSharp_Ax3 value)
{
  const gp_Ax3 axis(gp_Pnt(value.origin.x, value.origin.y, value.origin.z),
                    gp_Dir(value.direction.x, value.direction.y, value.direction.z),
                    gp_Dir(value.x_direction.x, value.x_direction.y, value.x_direction.z));
  return axis.Direct() ? 1 : 0;
}

OcctSharp_Plane OCCTSHARP_CALL occtsharp_gp_pln_default(void)
{ return { { 0., 0., 0. }, { 0., 0., 1. } }; }

OcctSharp_Status OCCTSHARP_CALL occtsharp_gp_pln_create(const OcctSharp_Xyz origin, const OcctSharp_Xyz normal, OcctSharp_Plane* result)
{
  if (result == nullptr) { SetLastError("The gp_Pln output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *result = {};
  return Guard([&]
  {
    const gp_Pln plane(gp_Pnt(origin.x, origin.y, origin.z), gp_Dir(normal.x, normal.y, normal.z));
    *result = { { plane.Location().X(), plane.Location().Y(), plane.Location().Z() }, { plane.Axis().Direction().X(), plane.Axis().Direction().Y(), plane.Axis().Direction().Z() } };
  });
}

double OCCTSHARP_CALL occtsharp_gp_pln_distance(const OcctSharp_Plane plane, const OcctSharp_Xyz point)
{ return gp_Pln(gp_Pnt(plane.origin.x, plane.origin.y, plane.origin.z), gp_Dir(plane.normal.x, plane.normal.y, plane.normal.z)).Distance(gp_Pnt(point.x, point.y, point.z)); }

double OCCTSHARP_CALL occtsharp_gp_pln_signed_distance(const OcctSharp_Plane plane, const OcctSharp_Xyz point)
{ return gp_Pln(gp_Pnt(plane.origin.x, plane.origin.y, plane.origin.z), gp_Dir(plane.normal.x, plane.normal.y, plane.normal.z)).SignedDistance(gp_Pnt(point.x, point.y, point.z)); }

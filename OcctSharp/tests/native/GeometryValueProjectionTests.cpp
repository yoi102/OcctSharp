#include "Foundation/OcctSharp.Foundation.GeometryValues.Generated.h"
#include <limits>
#include <stdexcept>
#include <source_location>
#include <cstdio>

using namespace OcctSharp_GeometryValues;
static void Require(bool value, std::source_location source = std::source_location::current())
{
  if (!value) throw std::runtime_error("Geometric projection mismatch at line " + std::to_string(source.line()));
}

int main()
{
  try {
  // Distinct, non-symmetric entries expose row/column transposition immediately.
  const auto m2 = ToNative(OcctSharp_Value_Matrix2x2{1, 2, 3, 5});
  Require(m2.Value(1, 2) == 2 && m2.Value(2, 1) == 3 && m2.Determinant() == -1);
  const auto p2 = gp_XY(7, 11).Multiplied(m2);
  Require(p2.X() == 29 && p2.Y() == 76);
  const auto copied2 = FromNative(m2);
  Require(copied2.M12 == 2 && copied2.M21 == 3);
  const auto m3 = ToNative(OcctSharp_Value_Matrix3x3{1,2,3, 4,5,6, 7,8,10});
  const auto p3 = gp_XYZ(2,3,5).Multiplied(m3);
  Require(p3.X() == 23 && p3.Y() == 53 && p3.Z() == 88);
  const auto copied3 = FromNative(m3);
  Require(copied3.M13 == 3 && copied3.M31 == 7 && copied3.M33 == 10);
  const auto xy = FromNative(ToNative(OcctSharp_Value_Coordinates2d{7,-9}));
  const auto xyz = FromNative(ToNative(OcctSharp_Value_Coordinates3d{7,-9,11}));
  Require(xy.X == 7 && xy.Y == -9 && xyz.Z == 11);
  const auto dir = FromNative(ToNative(OcctSharp_Value_Direction3d{2,3,6}));
  Require(std::abs(dir.X - 2.0/7) < 1e-15 && std::abs(dir.Z - 6.0/7) < 1e-15);
  bool rejected = false;
  try { (void)ToNative(OcctSharp_Value_Matrix2x2{1,2,3,std::numeric_limits<double>::infinity()}); }
  catch (const Standard_DomainError&) { rejected = true; }
  Require(rejected);
  const auto t2 = ToNative(OcctSharp_Value_Transform2d{0,-2,3, 2,0,5});
  const auto tp2 = gp_Pnt2d(7,11).Transformed(t2);
  Require(tp2.X() == -19 && tp2.Y() == 19);
  const auto t3 = ToNative(OcctSharp_Value_Transform3d{0,-2,0,3, 2,0,0,5, 0,0,2,7});
  const auto tp3 = gp_Pnt(7,11,13).Transformed(t3);
  Require(tp3.X() == -19 && tp3.Y() == 19 && tp3.Z() == 33);
  const auto a2 = ToNative(OcctSharp_Value_AffineTransform2d{1,2,3, 0,1,5});
  const auto ap2 = a2.Transformed(gp_XY(7,11));
  Require(ap2.X() == 32 && ap2.Y() == 16);
  const auto a3 = ToNative(OcctSharp_Value_AffineTransform3d{1,2,0,3, 0,1,0,5, 0,0,1,7});
  gp_XYZ ap3(7,11,13);
  a3.Transforms(ap3);
  Require(ap3.X() == 32 && ap3.Y() == 16 && ap3.Z() == 20);
  Require(ToNative(OcctSharp_Value_AffineTransform3d{1,0,0,3, 0,1,0,5, 0,0,1,7}).Trsf().Value(1,4) == 3);
  Require(ToNative(OcctSharp_Value_AffineTransform2d{1,0,3, 0,1,5}).Trsf2d().Value(1,3) == 3);
  rejected = false;
  try { (void)ToNative(OcctSharp_Value_Transform3d{1,2,0,0, 0,1,0,0, 0,0,1,0}); }
  catch (const Standard_Failure&) { rejected = true; }
  Require(rejected);
  Require(ToNative(OcctSharp_Value_AffineTransform3d{1,2,3,4, 2,4,6,7, 3,6,9,10}).IsSingular());
  return 0;
  }
  catch (const Standard_Failure& error) { std::fprintf(stderr, "OCCT: %s\n", error.what()); return 1; }
  catch (const std::exception& error) { std::fprintf(stderr, "%s\n", error.what()); return 1; }
}

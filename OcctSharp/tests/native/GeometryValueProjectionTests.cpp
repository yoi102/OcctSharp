#include "Foundation/OcctSharp.Foundation.GeometryValues.Generated.h"
#include <limits>
#include <stdexcept>

using namespace OcctSharp_GeometryValues;
static void Require(bool value) { if (!value) throw std::runtime_error("Geometric projection mismatch"); }

int main()
{
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
  return 0;
}

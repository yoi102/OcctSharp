#include "Surfaces/ConstraintResiduals.hxx"
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <GeomLProp_SLProps.hxx>
#include <gp_Vec.hxx>
#include <algorithm>
#include <cmath>

namespace OcctSharp::Native::Authoring {
namespace {
struct CurvatureTensor {
  gp_Vec Maximum, Minimum;
  double KMaximum = 0, KMinimum = 0;
  double At(const gp_Vec& a, const gp_Vec& b) const {
    return KMaximum * a.Dot(Maximum) * b.Dot(Maximum) + KMinimum * a.Dot(Minimum) * b.Dot(Minimum);
  }
};
CurvatureTensor Tensor(GeomLProp_SLProps& props, double normalSign) {
  gp_Dir maxDirection, minDirection; props.CurvatureDirections(maxDirection, minDirection);
  return {gp_Vec(maxDirection), gp_Vec(minDirection), props.MaxCurvature() * normalSign, props.MinCurvature() * normalSign};
}
}
OcctSharp_ConstraintResidual SurfaceResidual(const opencascade::handle<Geom_Surface>& result,
  const gp_Pnt& target, const opencascade::handle<Geom_Surface>& support, double supportU, double supportV,
  int order, double tolerance) {
  OcctSharp_ConstraintResidual value{};
  GeomAPI_ProjectPointOnSurf projection(target, result, tolerance);
  if (!projection.IsDone() || projection.NbPoints() == 0) return value;
  double u, v; projection.LowerDistanceParameters(u, v); value.position = projection.LowerDistance();
  if (!std::isfinite(value.position)) return value;
  value.defined = 1; value.sample_count = 1;
  if (order == 0 || support.IsNull()) return value;
  GeomLProp_SLProps actual(result, u, v, 2, 1e-10), expected(support, supportU, supportV, 2, 1e-10);
  if (!actual.IsNormalDefined() || !expected.IsNormalDefined()) return value;
  const double dot = std::clamp(actual.Normal().Dot(expected.Normal()), -1.0, 1.0);
  value.angle = std::acos(std::abs(dot)); value.defined |= 2;
  if (order == 1 || !actual.IsCurvatureDefined() || !expected.IsCurvatureDefined()) return value;
  const auto a = Tensor(actual, dot < 0 ? -1 : 1), b = Tensor(expected, 1);
  // Compare the full second-fundamental-form tensors in a common tangent basis;
  // unordered principal curvature values alone would miss rotated anisotropy.
  const gp_Vec x = b.Maximum, y = b.Minimum;
  const double dxx = a.At(x, x) - b.At(x, x), dyy = a.At(y, y) - b.At(y, y), dxy = a.At(x, y) - b.At(x, y);
  value.curvature = std::hypot(std::hypot(dxx, dyy), std::sqrt(2.0) * dxy);
  if (std::isfinite(value.curvature)) value.defined |= 4;
  return value;
}
}

#include "Modeling/GuidedAuthoring.hxx"
#include "Surfaces/ConstraintResiduals.hxx"
#include <BRepAdaptor_Curve.hxx>
#include <Geom2d_Curve.hxx>
#include <GeomLProp_CLProps.hxx>
#include <Geom_Curve.hxx>
#include <gp_Vec.hxx>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_surface_join(const OcctSharp_ShapeHandle* boundary,
  const OcctSharp_ShapeHandle* firstFace, const OcctSharp_ShapeHandle* secondFace, int32_t count, double tolerance,
  OcctSharp_ConstraintResidual* residuals, int32_t capacity) {
  return Guard([&] {
    Require(residuals && count >= 2 && count <= 4096 && capacity >= count, "Invalid continuity sample buffer."); Positive(tolerance);
    const OcctSharp_ShapeHandle* inputs[]{boundary, firstFace, secondFace}; InputGraph graph(inputs, 3);
    const auto edge = TopoDS::Edge(graph.Typed(0, TopAbs_EDGE)); const auto first = TopoDS::Face(graph.Typed(1, TopAbs_FACE));
    const auto second = TopoDS::Face(graph.Typed(2, TopAbs_FACE)); double a, b;
    const auto pcurve = BRep_Tool::CurveOnSurface(edge, first, a, b); Require(!pcurve.IsNull(), "Boundary requires a pcurve on the first support face.");
    const auto support = BRep_Tool::Surface(first), target = BRep_Tool::Surface(second);
    Require(!support.IsNull() && !target.IsNull(), "Surface join requires exact surfaces.");
    std::vector<OcctSharp_ConstraintResidual> copied; copied.reserve(count);
    for (int i = 0; i < count; ++i) {
      const auto uv = pcurve->Value(a + (b - a) * i / (count - 1));
      auto sample = SurfaceResidual(target, support->Value(uv.X(), uv.Y()), support, uv.X(), uv.Y(), 2, tolerance);
      sample.id = i; copied.push_back(sample);
    }
    std::copy(copied.begin(), copied.end(), residuals);
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_authoring_curve_join(const OcctSharp_ShapeHandle* firstCurve,
  const OcctSharp_ShapeHandle* secondCurve, double firstParameter, double secondParameter, int32_t reverseSecond,
  OcctSharp_ConstraintResidual* residual) {
  return Guard([&] {
    Require(residual && std::isfinite(firstParameter) && std::isfinite(secondParameter), "Invalid curve-join parameters."); Flag(reverseSecond);
    const OcctSharp_ShapeHandle* inputs[]{firstCurve, secondCurve}; InputGraph graph(inputs, 2);
    double a0, a1, b0, b1;
    const auto a = BRep_Tool::Curve(TopoDS::Edge(graph.Typed(0, TopAbs_EDGE)), a0, a1);
    const auto b = BRep_Tool::Curve(TopoDS::Edge(graph.Typed(1, TopAbs_EDGE)), b0, b1);
    Require(!a.IsNull() && !b.IsNull() && firstParameter >= a0 && firstParameter <= a1 && secondParameter >= b0 && secondParameter <= b1,
      "Curve-join parameters lie outside the edge domains.");
    GeomLProp_CLProps first(a, firstParameter, 2, 1e-10), second(b, secondParameter, 2, 1e-10);
    OcctSharp_ConstraintResidual value{}; value.position = first.Value().Distance(second.Value()); value.defined = 1; value.sample_count = 1;
    // CLProps may infer a limiting tangent from D2 when D1 is zero. This API's
    // regular-parameter derivative contract instead marks that sample undefined.
    if (first.D1().SquareMagnitude() > 1e-20 && second.D1().SquareMagnitude() > 1e-20
        && first.IsTangentDefined() && second.IsTangentDefined()) {
      gp_Dir t1, t2; first.Tangent(t1); second.Tangent(t2); if (reverseSecond) t2.Reverse();
      value.angle = t1.Angle(t2); value.defined |= 2;
      const auto d1 = first.D1(), d2 = second.D1();
      const auto k1 = (first.D2() - d1 * (first.D2().Dot(d1) / d1.SquareMagnitude())) / d1.SquareMagnitude();
      const auto k2 = (second.D2() - d2 * (second.D2().Dot(d2) / d2.SquareMagnitude())) / d2.SquareMagnitude();
      value.curvature = (k1 - k2).Magnitude(); if (std::isfinite(value.curvature)) value.defined |= 4;
    }
    *residual = value;
  });
}

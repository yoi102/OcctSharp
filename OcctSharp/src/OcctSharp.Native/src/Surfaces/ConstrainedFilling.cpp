#include "Modeling/GuidedAuthoring.hxx"
#include "Surfaces/ConstraintResiduals.hxx"
#include <BRepOffsetAPI_MakeFilling.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepTools.hxx>
#include <BRepClass_FaceClassifier.hxx>
#include <Geom2d_Curve.hxx>
#include <Geom_Surface.hxx>
#include <TopoDS_Edge.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Vec.hxx>
#include <algorithm>
#include <numeric>
#include <set>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Authoring;

namespace {
// OCCT 8.0.1 forwards this enum numerically into GeomPlate/BRepFill constraint
// constructors taking derivative order 0/1/2. GeomAbs_G2 is 3 and is rejected.
// Preserve the public G0/G1/G2 intent with the SDK's actual integer-order contract.
GeomAbs_Shape Order(int order) { return static_cast<GeomAbs_Shape>(order); }
void Seed(const TopoDS_Face& face) {
  const auto surface = BRep_Tool::Surface(face); Require(!surface.IsNull(), "Initial face has no surface.");
  double u0, u1, v0, v1; BRepTools::UVBounds(face, u0, u1, v0, v1);
  Require(std::isfinite(u0) && std::isfinite(u1) && std::isfinite(v0) && std::isfinite(v1), "Initial surface needs finite UV bounds.");
  for (int i = 0; i < 5; ++i) for (int j = 0; j < 5; ++j) {
    gp_Pnt p; gp_Vec du, dv; surface->D1(u0 + (u1 - u0) * (i + 0.5) / 5, v0 + (v1 - v0) * (j + 0.5) / 5, p, du, dv);
    Require(du.Magnitude() > 1e-12 && dv.Magnitude() > 1e-12 && std::abs(du.Normalized().Dot(dv.Normalized())) <= 1e-8,
      "Initial surface coordinates must be regular and orthogonal at the checked grid.");
  }
}
OcctSharp_ConstraintResidual Measure(const TopoDS_Face& result, const OcctSharp_FillConstraint& c,
  const InputGraph& graph, const OcctSharp_FillOptions& o) {
  const auto surface = BRep_Tool::Surface(result);
  opencascade::handle<Geom_Surface> support;
  if (c.support_index >= 0) support = BRep_Tool::Surface(TopoDS::Face(graph.Typed(c.support_index, TopAbs_FACE)));
  OcctSharp_ConstraintResidual residual{}; residual.id = c.id; residual.required = c.required;
  residual.defined = c.order == 0 ? 1 : c.order == 1 ? 3 : 7;
  auto append = [&](const gp_Pnt& point, double u, double v) {
    const auto value = SurfaceResidual(surface, point, support, u, v, c.order, o.tolerance_2d);
    residual.defined &= value.defined; residual.position = std::max(residual.position, value.position);
    residual.angle = std::max(residual.angle, value.angle); residual.curvature = std::max(residual.curvature, value.curvature);
    residual.sample_count += value.sample_count;
  };
  if (c.kind == 0) {
    const auto edge = TopoDS::Edge(graph.Typed(c.shape_index, TopAbs_EDGE)); BRepAdaptor_Curve curve(edge);
    opencascade::handle<Geom2d_Curve> pcurve; double first = 0, last = 0;
    if (c.support_index >= 0) pcurve = BRep_Tool::CurveOnSurface(edge, TopoDS::Face(graph.At(c.support_index)), first, last);
    for (int i = 0; i < o.verification_samples; ++i) {
      const double parameter = curve.FirstParameter() + (curve.LastParameter() - curve.FirstParameter()) * i / (o.verification_samples - 1);
      gp_Pnt2d uv;
      if (!pcurve.IsNull()) uv = pcurve->Value(parameter);
      append(curve.Value(parameter), uv.X(), uv.Y());
    }
  } else if (c.kind == 1) append(support->Value(c.u, c.v), c.u, c.v);
  else append(Point(c.point), 0, 0);
  const int needed = c.order == 0 ? 1 : c.order == 1 ? 3 : 7;
  residual.accepted = (residual.defined & needed) == needed && residual.sample_count > 0 && residual.position <= o.tolerance_3d
    && (c.order < 1 || residual.angle <= o.tolerance_angular) && (c.order < 2 || residual.curvature <= o.tolerance_curvature);
  return residual;
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_constrained_fill(const OcctSharp_ShapeHandle* const* inputs, int32_t inputCount,
  const OcctSharp_FillConstraint* constraints, int32_t constraintCount, const OcctSharp_FillOptions* options,
  OcctSharp_ConstraintResidual* residuals, int32_t capacity, OcctSharp_AuthoringInfo* info, OcctSharp_FeatureResultHandle** output) {
  if (output) *output = nullptr;
  return Guard([&] {
    Require(options && constraints && info && output && residuals, "A constrained filling argument is null."); const auto& o = *options;
    Require(constraintCount >= 1 && constraintCount <= 256 && capacity >= constraintCount && o.degree >= 2 && o.degree <= 8
      && o.points_per_curve >= 5 && o.points_per_curve <= 200 && o.iterations >= 1 && o.iterations <= 8
      && o.maximum_degree >= 2 && o.maximum_degree <= 25 && o.maximum_segments >= 1 && o.maximum_segments <= 128
      && o.verification_samples >= 3 && o.verification_samples <= 257, "Filling controls exceed bounded limits.");
    Require(static_cast<int64_t>(constraintCount) * o.points_per_curve * (1 << o.iterations) <= 1000000,
      "The filling refinement budget exceeds one million constraint samples.");
    Flag(o.anisotropic); Positive(o.tolerance_2d); Positive(o.tolerance_3d); Positive(o.tolerance_angular); Positive(o.tolerance_curvature);
    InputGraph graph(inputs, inputCount); std::set<int> ids; int boundaryCount = 0;
    for (int i = 0; i < constraintCount; ++i) {
      const auto& c = constraints[i]; Flag(c.required); Flag(c.boundary);
      Require(c.reserved == 0 && c.id >= 0 && ids.insert(c.id).second && c.kind >= 0 && c.kind <= 2 && c.order >= 0 && c.order <= 2,
        "Constraint IDs must be unique and modes valid.");
      Require(c.kind == 0 || !c.boundary, "Only edge constraints can be boundary edges.");
      if (c.kind == 0) {
        const auto edge = TopoDS::Edge(graph.Typed(c.shape_index, TopAbs_EDGE)); boundaryCount += c.boundary;
        Require(c.order == 0 || c.support_index >= 0, "G1/G2 edge constraints require an explicit support face.");
        if (c.support_index >= 0) {
          double first, last; const auto face = TopoDS::Face(graph.Typed(c.support_index, TopAbs_FACE));
          Require(!BRep_Tool::CurveOnSurface(edge, face, first, last).IsNull(), "The edge has no pcurve on its support face.");
        }
      } else if (c.kind == 1) {
        Require(std::isfinite(c.u) && std::isfinite(c.v), "Constraint UVs must be finite.");
        const auto face = TopoDS::Face(graph.Typed(c.support_index, TopAbs_FACE));
        BRepClass_FaceClassifier classified(face, gp_Pnt2d(c.u, c.v), o.tolerance_2d);
        Require(classified.State() == TopAbs_IN || classified.State() == TopAbs_ON, "UV point is outside the support face.");
      } else { Require(c.order == 0, "Free 3D points only support G0."); Point(c.point); }
    }
    Require(boundaryCount >= 2, "Filling needs at least two explicit boundary edges.");
    if (o.seed_index >= 0) Seed(TopoDS::Face(graph.Typed(o.seed_index, TopAbs_FACE)));
    auto result = std::make_unique<OcctSharp_FeatureResultHandle>(); OcctSharp_AuthoringInfo state{}; state.continuity_limit = -1;
    std::vector<OcctSharp_ConstraintResidual> measured(constraintCount);
    for (int i = 0; i < constraintCount; ++i) { measured[i].id = constraints[i].id; measured[i].required = constraints[i].required; }
    try {
      BRepOffsetAPI_MakeFilling builder(o.degree, o.points_per_curve, o.iterations, o.anisotropic != 0,
        o.tolerance_2d, o.tolerance_3d, o.tolerance_angular, o.tolerance_curvature, o.maximum_degree, o.maximum_segments);
      if (o.seed_index >= 0) builder.LoadInitSurface(TopoDS::Face(graph.At(o.seed_index)));
      // The SDK numbers boundary edges first, interior edges next, points last.
      // Insertion in this order keeps returned constraint indices stable.
      std::vector<int> insertion(constraintCount); std::iota(insertion.begin(), insertion.end(), 0);
      std::stable_sort(insertion.begin(), insertion.end(), [&](int a, int b) {
        auto priority = [&](int i) { return constraints[i].kind == 0 ? (constraints[i].boundary ? 0 : 1) : 2; };
        return priority(a) < priority(b);
      });
      std::vector<int> kernelIndices(constraintCount);
      for (int i : insertion) {
        const auto& c = constraints[i]; int index;
        if (c.kind == 0) {
          const auto edge = TopoDS::Edge(graph.At(c.shape_index));
          index = c.support_index >= 0 ? builder.Add(edge, TopoDS::Face(graph.At(c.support_index)), Order(c.order), c.boundary != 0)
            : builder.Add(edge, Order(c.order), c.boundary != 0);
        } else if (c.kind == 1) index = builder.Add(c.u, c.v, TopoDS::Face(graph.At(c.support_index)), Order(c.order));
        else index = builder.Add(Point(c.point));
        kernelIndices[i] = index;
      }
      state.ready = 1; builder.Build(); state.done = builder.IsDone();
      if (state.done) {
        result->Result = builder.Shape(); bool accepted = true;
        for (int i = 0; i < constraintCount; ++i) {
          measured[i] = Measure(TopoDS::Face(result->Result), constraints[i], graph, o); measured[i].kernel_index = kernelIndices[i];
          // Do not call OCCT 8.0.1's per-index G*Error getters. They allocate
          // myNbPtsOnCur entries but EcartContraintesMil writes the refined curve
          // sample count and may overrun them (or leave entries uninitialized).
          // The independent final-surface measurements above are bounded, apply
          // equally to edge/point constraints, and expose unavailable derivatives.
          if (constraints[i].required && !measured[i].accepted) accepted = false;
        }
        History(builder, graph, *result);
        result->Message = accepted ? "Required constraints pass independent final-surface residual checks; these bounded samples are not a global error proof."
          : "One or more required constraints are unavailable, ignored or outside residual tolerances; do not accept the result.";
      } else result->Message = "OCCT constrained filling did not complete.";
    } catch (const Standard_Failure& error) { state.done = 0; result->Result.Nullify(); result->Message = error.GetMessageString(); }
    Finish(*result, state); *output = RegisterFeatureResult(std::move(result)); *info = state;
    std::copy(measured.begin(), measured.end(), residuals);
  });
}

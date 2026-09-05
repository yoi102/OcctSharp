#include "SurfaceCommon.hxx"
#include <Geom2d_BSplineCurve.hxx>
#include <Geom2d_TrimmedCurve.hxx>
#include <Geom2d_OffsetCurve.hxx>
#include <Geom2dConvert.hxx>
#include <Geom2dConvert_ApproxCurve.hxx>
#include <Geom2dAPI_Interpolate.hxx>
#include <Geom2dAPI_PointsToBSpline.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <GeomAPI_IntCS.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <Geom2dAPI_ProjectPointOnCurve.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <NCollection_IndexedMap.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <ShapeConstruct_ProjectCurveOnSurface.hxx>
#include <NCollection_HArray1.hxx>
#include <Precision.hxx>

using namespace OcctSharp::SurfaceBridge;

namespace {
GeomAbs_Shape Continuity(int value) {
  switch (value) { case 0: return GeomAbs_C0; case 1: return GeomAbs_C1; case 2: return GeomAbs_C2; case 3: return GeomAbs_C3; }
  throw Failure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Continuity must be C0, C1, C2 or C3.");
}
void CopyCurve(const opencascade::handle<Geom2d_Curve>& curve, double first, double last,
  bool reversed, double tolerance, double residual, OcctSharp_SurfaceCurveInfo* info,
  OcctSharp_SketchPoint2d* poles, double* weights, int pole_capacity,
  double* knots, int32_t* multiplicities, int knot_capacity)
{
  Require(info && pole_capacity >= 0 && knot_capacity >= 0, "Invalid curve definition output.");
  opencascade::handle<Geom2d_Curve> bounded = new Geom2d_TrimmedCurve(curve, first, last, true, false);
  opencascade::handle<Geom2d_BSplineCurve> spline;
  bool exact = true;
  try {
    // Trimming an entire periodic spline would unnecessarily make the copy nonperiodic.
    if (curve->IsKind(STANDARD_TYPE(Geom2d_BSplineCurve))
      && first == curve->FirstParameter() && last == curve->LastParameter())
      spline = opencascade::handle<Geom2d_BSplineCurve>::DownCast(curve->Copy());
    else spline = Geom2dConvert::CurveToBSplineCurve(bounded);
  }
  catch (const Standard_Failure&) {
    Geom2dConvert_ApproxCurve approximation(bounded, tolerance, GeomAbs_C1, 128, 14);
    Require(approximation.IsDone() && approximation.HasResult(), "UV curve approximation failed.");
    spline = approximation.Curve(); residual = std::max(residual, approximation.MaxError()); exact = false;
  }
  Require(!spline.IsNull(), "The copied UV curve has no B-spline representation.");
  const bool same_parameter = exact && curve->IsKind(STANDARD_TYPE(Geom2d_BSplineCurve));
  *info = {spline->Degree(), spline->IsPeriodic(), spline->NbPoles(), spline->NbKnots(),
    reversed, exact, same_parameter, 0, spline->FirstParameter(), spline->LastParameter(), first, last, residual};
  if (!poles && !weights && !knots && !multiplicities && pole_capacity == 0 && knot_capacity == 0) return;
  Require(poles && weights && knots && multiplicities && pole_capacity >= spline->NbPoles()
    && knot_capacity >= spline->NbKnots(), "Curve definition output capacity is insufficient.");
  for (int index = 1; index <= spline->NbPoles(); ++index) {
    poles[index - 1] = Copy2d(spline->Pole(index)); weights[index - 1] = spline->Weight(index);
  }
  for (int index = 1; index <= spline->NbKnots(); ++index) {
    knots[index - 1] = spline->Knot(index); multiplicities[index - 1] = spline->Multiplicity(index);
  }
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_curve_definition(
  const OcctSharp_ShapeHandle* face_handle, const OcctSharp_ShapeHandle* edge_handle,
  int32_t branch, int32_t derive, double tolerance, OcctSharp_SurfaceCurveInfo* info,
  OcctSharp_SketchPoint2d* poles, double* weights, int32_t pole_capacity,
  double* knots, int32_t* multiplicities, int32_t knot_capacity)
{
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(face_handle);
    const auto edge = TopoDS::Edge(TypedShape(edge_handle, TopAbs_EDGE));
    Require((branch == 0 || branch == 1) && (derive == 0 || derive == 1), "Invalid pcurve branch or projection mode.");
    double first, last, residual = 0;
    opencascade::handle<Geom2d_Curve> curve;
    if (derive) {
      Require(branch == 0, "Derived pcurves have one explicit branch.");
      auto source = BRep_Tool::Curve(edge, first, last);
      Require(!source.IsNull(), "Deriving a pcurve requires an existing 3D edge curve.");
      source = opencascade::handle<Geom_Curve>::DownCast(source->Transformed(data.location.Transformation().Inverted()));
      ShapeConstruct_ProjectCurveOnSurface projector;
      const double scale = std::abs(data.location.Transformation().ScaleFactor());
      projector.Init(data.surface, tolerance / scale);
      Require(projector.Perform(source, first, last, curve), "Deriving the UV curve failed.");
      Require(!curve.IsNull(), "The UV curve projection is null.");
      for (int index = 0; index <= 64; ++index) {
        const double parameter = first + (last - first) * index / 64;
        const auto uv = curve->Value(parameter);
        residual = std::max(residual, scale * data.surface->Value(uv.X(), uv.Y()).Distance(source->Value(parameter)));
      }
      Require(residual <= tolerance, "The input 3D edge is not on the surface within the requested tolerance.");
    } else {
      Require(branch == 0 || BRep_Tool::IsClosed(edge, data.face), "This edge has no second seam branch on the face.");
      auto selected = TopoDS::Edge(edge.Oriented(branch == 0 ? TopAbs_FORWARD : TopAbs_REVERSED));
      curve = BRep_Tool::CurveOnSurface(selected, data.face, first, last);
      Require(!curve.IsNull(), "The edge has no pcurve on the supplied face.");
    }
    CopyCurve(curve, first, last, edge.Orientation() == TopAbs_REVERSED, tolerance, residual,
      info, poles, weights, pole_capacity, knots, multiplicities, knot_capacity);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_fit_uv(
  const OcctSharp_SketchPoint2d* points, int32_t count, int32_t interpolate, int32_t periodic,
  int32_t minimum_degree, int32_t maximum_degree, int32_t continuity, double tolerance,
  OcctSharp_SurfaceCurveInfo* info, OcctSharp_SketchPoint2d* poles, double* weights, int32_t pole_capacity,
  double* knots, int32_t* multiplicities, int32_t knot_capacity)
{
  return Invoke([&] {
    Tolerance(tolerance);
    Require(points && count >= 3 && count <= 100000 && (interpolate == 0 || interpolate == 1)
      && (periodic == 0 || periodic == 1), "Invalid UV fitting input.");
    Require(minimum_degree >= 2 && maximum_degree >= minimum_degree && maximum_degree <= 25,
      "UV approximation degrees must be between 2 and 25.");
    opencascade::handle<NCollection_HArray1<gp_Pnt2d>> input = new NCollection_HArray1<gp_Pnt2d>(1, count);
    for (int index = 0; index < count; ++index) input->SetValue(index + 1, Point2d(points[index]));
    opencascade::handle<Geom2d_BSplineCurve> curve;
    if (interpolate) {
      Geom2dAPI_Interpolate builder(input, periodic != 0, tolerance); builder.Perform();
      Require(builder.IsDone(), "Smooth UV interpolation failed."); curve = builder.Curve();
    } else {
      Require(periodic == 0, "Periodic approximation is not supported; use periodic interpolation.");
      Geom2dAPI_PointsToBSpline builder(input->Array1(), minimum_degree, maximum_degree, Continuity(continuity), tolerance);
      Require(builder.IsDone(), "UV B-spline approximation failed."); curve = builder.Curve();
    }
    double residual = 0;
    for (int index = 1; index <= count; ++index) {
      Geom2dAPI_ProjectPointOnCurve projection(input->Value(index), curve);
      Require(projection.NbPoints() > 0, "UV fit residual could not be measured.");
      residual = std::max(residual, projection.LowerDistance());
    }
    Require(residual <= tolerance, "The UV fit exceeds the requested input-point residual.");
    CopyCurve(curve, curve->FirstParameter(), curve->LastParameter(), false, tolerance, residual,
      info, poles, weights, pole_capacity, knots, multiplicities, knot_capacity);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_offset_uv(
  const OcctSharp_SketchCurve* input, double distance, double tolerance, OcctSharp_SurfaceCurveInfo* info,
  OcctSharp_SketchPoint2d* poles, double* weights, int32_t pole_capacity,
  double* knots, int32_t* multiplicities, int32_t knot_capacity)
{
  return Invoke([&] {
    Tolerance(tolerance); Require(input && std::isfinite(distance) && distance != 0, "Invalid UV offset input.");
    auto basis = SketchCurve(*input); if (input->reversed) basis = basis->Reversed();
    opencascade::handle<Geom2d_Curve> curve = new Geom2d_OffsetCurve(basis, distance);
    CopyCurve(curve, curve->FirstParameter(), curve->LastParameter(), false, tolerance, 0,
      info, poles, weights, pole_capacity, knots, multiplicities, knot_capacity);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_intersect_curve(
  const OcctSharp_ShapeHandle* face_handle, const OcctSharp_ShapeHandle* edge_handle, double tolerance,
  OcctSharp_SurfaceIntersection* output, int32_t capacity, int32_t* count)
{
  if (count) *count = 0;
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(face_handle);
    const auto edge = TopoDS::Edge(TypedShape(edge_handle, TopAbs_EDGE));
    double first, last; auto curve = BRep_Tool::Curve(edge, first, last);
    Require(!curve.IsNull(), "Curve/surface intersection requires a 3D curve.");
    opencascade::handle<Geom_Curve> bounded = new Geom_TrimmedCurve(curve, first, last, true, false);
    opencascade::handle<Geom_Surface> surface = new Geom_RectangularTrimmedSurface(BRep_Tool::Surface(data.face), data.u0, data.u1, data.v0, data.v1);
    GeomAPI_IntCS intersector(bounded, surface);
    Require(intersector.IsDone(), "Curve/surface intersection failed.");
    std::vector<OcctSharp_SurfaceIntersection> values;
    for (int index = 1; index <= intersector.NbPoints(); ++index) {
      double u, v, parameter; intersector.Parameters(index, u, v, parameter);
      const int state = data.State(u, v, tolerance);
      if (state != TopAbs_IN && state != TopAbs_ON) continue;
      if (parameter < first - tolerance || parameter > last + tolerance) continue;
      const auto point = Copy(intersector.Point(index));
      values.push_back({0, state, parameter, parameter, point, point, {u, v}, {u, v}});
    }
    // IntCS does not consistently report coplanar segments. A non-destructive Boolean
    // supplies bounded overlap edges and clips them against real wires, including holes.
    BRepAlgoAPI_Common common;
    NCollection_List<TopoDS_Shape> arguments, tools;
    arguments.Append(edge); tools.Append(data.face);
    common.SetArguments(arguments); common.SetTools(tools);
    common.SetNonDestructive(true); common.SetFuzzyValue(tolerance); common.Build();
    Require(common.IsDone() && !common.HasErrors(), "Coincident interval clipping failed.");
    auto uvAt = [&](const gp_Pnt& world) {
      GeomAPI_ProjectPointOnSurf projection(world.Transformed(data.location.Transformation().Inverted()),
        data.surface, data.u0, data.u1, data.v0, data.v1, tolerance);
      Require(projection.NbPoints() > 0, "An intersection witness could not be mapped to UV.");
      double u, v; projection.LowerDistanceParameters(u, v); return gp_Pnt2d(u, v);
    };
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> overlaps;
    TopExp::MapShapes(common.Shape(), TopAbs_EDGE, overlaps);
    for (const auto& item : overlaps) {
      double start, end; auto segment = BRep_Tool::Curve(TopoDS::Edge(item), start, end);
      if (segment.IsNull() || start >= end) continue;
      const auto a = segment->Value(start), b = segment->Value(end);
      const auto uv0 = uvAt(a), uv1 = uvAt(b), mid = uvAt(segment->Value((start + end) / 2));
      values.push_back({1, data.State(mid.X(), mid.Y(), tolerance), start, end, Copy(a), Copy(b), Copy2d(uv0), Copy2d(uv1)});
    }
    std::stable_sort(values.begin(), values.end(), [](const auto& a, const auto& b) { return a.first_parameter < b.first_parameter; });
    CopyValues(values, output, capacity, count);
  });
}

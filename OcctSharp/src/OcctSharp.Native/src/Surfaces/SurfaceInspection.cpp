#include "SurfaceCommon.hxx"
#include <BRepAdaptor_Surface.hxx>
#include <BRepAdaptor_Curve.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <GeomLProp_SLProps.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <GCPnts_UniformAbscissa.hxx>

using namespace OcctSharp::SurfaceBridge;
static_assert(sizeof(OcctSharp_SurfaceInfo) == 72);
static_assert(sizeof(OcctSharp_SurfaceSample) == 160);
static_assert(sizeof(OcctSharp_SurfacePointSolution) == 56);

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_describe(
  const OcctSharp_ShapeHandle* handle, OcctSharp_SurfaceInfo* output)
{
  return Invoke([&] {
    Require(output != nullptr, "The surface descriptor output is null.");
    FaceData data(handle);
    BRepAdaptor_Surface adaptor(data.face, true);
    *output = {static_cast<int>(adaptor.GetType()), static_cast<int>(data.face.Orientation()),
      data.surface->IsUClosed(), data.surface->IsVClosed(), data.surface->IsUPeriodic(), data.surface->IsVPeriodic(),
      data.u0, data.u1, data.v0, data.v1,
      data.surface->IsUPeriodic() ? data.surface->UPeriod() : 0,
      data.surface->IsVPeriodic() ? data.surface->VPeriod() : 0};
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_classify(
  const OcctSharp_ShapeHandle* handle, const OcctSharp_SketchPoint2d* points,
  int32_t count, double tolerance, int32_t* output)
{
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(handle);
    Require(count >= 0 && count <= 1000000 && (count == 0 || (points && output)), "Invalid UV classification buffers.");
    std::vector<int32_t> states; states.reserve(count);
    for (int index = 0; index < count; ++index) {
      Point2d(points[index]); states.push_back(data.State(points[index].x, points[index].y, tolerance));
    }
    std::copy(states.begin(), states.end(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_evaluate(
  const OcctSharp_ShapeHandle* handle, const OcctSharp_SketchPoint2d* points,
  int32_t count, double tolerance, OcctSharp_SurfaceSample* output)
{
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(handle);
    Require(count >= 0 && count <= 1000000 && (count == 0 || (points && output)), "Invalid surface evaluation buffers.");
    std::vector<OcctSharp_SurfaceSample> values; values.reserve(count);
    for (int index = 0; index < count; ++index) {
      const gp_Pnt2d uv = Point2d(points[index]);
      Require(uv.X() >= data.u0 - tolerance && uv.X() <= data.u1 + tolerance
        && uv.Y() >= data.v0 - tolerance && uv.Y() <= data.v1 + tolerance, "UV evaluation is outside the bounded domain.");
      GeomLProp_SLProps properties(data.surface, uv.X(), uv.Y(), 2, tolerance);
      const auto transform = data.location.Transformation();
      OcctSharp_SurfaceSample value{};
      value.uv = points[index]; value.state = data.State(uv.X(), uv.Y(), tolerance);
      value.point = Copy(properties.Value().Transformed(transform));
      value.du = Copy(properties.D1U().Transformed(transform));
      value.dv = Copy(properties.D1V().Transformed(transform));
      value.normal_defined = properties.IsNormalDefined();
      // A well-defined limiting OCCT normal at a pole does not imply a regular UV chart.
      const gp_Vec cross = properties.D1U().Crossed(properties.D1V());
      value.reserved = cross.SquareMagnitude() <= tolerance * tolerance ? 1 : 0;
      const bool reverse = data.face.Orientation() == TopAbs_REVERSED;
      if (value.normal_defined) {
        auto normal = properties.Normal().Transformed(transform);
        if (reverse) normal.Reverse();
        value.normal = Copy(normal);
      }
      value.curvature_defined = properties.IsCurvatureDefined();
      if (value.curvature_defined) {
        const double scale = std::abs(transform.ScaleFactor());
        double minimum = properties.MinCurvature() / scale, maximum = properties.MaxCurvature() / scale;
        if (reverse) { const double old = minimum; minimum = -maximum; maximum = -old; }
        value.minimum_curvature = minimum; value.maximum_curvature = maximum;
        value.mean_curvature = (minimum + maximum) / 2;
        value.gaussian_curvature = minimum * maximum;
      }
      values.push_back(value);
    }
    std::copy(values.begin(), values.end(), output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_project_points(
  const OcctSharp_ShapeHandle* handle, const OcctSharp_Xyz* points, int32_t point_count,
  int32_t limit_to_face, double tolerance, OcctSharp_SurfacePointSolution* output, int32_t capacity, int32_t* count)
{
  if (count) *count = 0;
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(handle);
    Require(point_count >= 0 && point_count <= 100000 && (point_count == 0 || points), "Invalid projection input buffer.");
    Require(limit_to_face == 0 || limit_to_face == 1, "Face limiting must be Boolean.");
    std::vector<OcctSharp_SurfacePointSolution> values;
    for (int index = 0; index < point_count; ++index) {
      const auto world = Point(points[index]);
      const auto local = world.Transformed(data.location.Transformation().Inverted());
      GeomAPI_ProjectPointOnSurf projector(local, data.surface, data.u0, data.u1, data.v0, data.v1, tolerance);
      for (int solution = 1; solution <= projector.NbPoints(); ++solution) {
        double u, v; projector.Parameters(solution, u, v);
        const int state = data.State(u, v, tolerance);
        if (limit_to_face && state != TopAbs_IN && state != TopAbs_ON) continue;
        const auto point = data.WorldPoint(u, v);
        values.push_back({index, state, {u, v}, Copy(point), point.Distance(world)});
      }
    }
    std::stable_sort(values.begin(), values.end(), [](const auto& a, const auto& b) {
      if (a.source_index != b.source_index) return a.source_index < b.source_index;
      if (a.distance != b.distance) return a.distance < b.distance;
      return a.uv.x == b.uv.x ? a.uv.y < b.uv.y : a.uv.x < b.uv.x;
    });
    CopyValues(values, output, capacity, count);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_iso(
  const OcctSharp_ShapeHandle* handle, int32_t direction, double parameter,
  double first, double last, OcctSharp_ShapeHandle** output)
{
  if (output) *output = nullptr;
  return Invoke([&] {
    FaceData data(handle);
    Require((direction == 0 || direction == 1) && std::isfinite(parameter)
      && std::isfinite(first) && std::isfinite(last) && first < last, "Invalid iso-curve parameters.");
    Require(direction == 0 ? parameter >= data.u0 && parameter <= data.u1 && first >= data.v0 && last <= data.v1
      : parameter >= data.v0 && parameter <= data.v1 && first >= data.u0 && last <= data.u1, "Iso-curve range is outside UV bounds.");
    auto curve = direction == 0 ? data.surface->UIso(parameter) : data.surface->VIso(parameter);
    BRepBuilderAPI_MakeEdge builder(curve, first, last);
    Require(builder.IsDone(), "Iso-curve edge construction failed.");
    TopoDS_Shape edge = builder.Edge().Moved(data.location);
    if (data.face.Orientation() == TopAbs_REVERSED) edge.Reverse();
    Result(edge, output);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_surface_sample_curve(
  const OcctSharp_ShapeHandle* face_handle, const OcctSharp_ShapeHandle* edge_handle,
  int32_t count, double tolerance, OcctSharp_SurfaceCurveSample* output)
{
  return Invoke([&] {
    Tolerance(tolerance); FaceData data(face_handle);
    auto edge = TopoDS::Edge(TypedShape(edge_handle, TopAbs_EDGE));
    Require(count >= 2 && count <= 1000000 && output, "Invalid curve sample count or output.");
    Require(BRep_Tool::SameParameter(edge) && BRep_Tool::SameRange(edge), "Arc-length UV sampling requires SameParameter and SameRange topology.");
    double first, last;
    auto pcurve = BRep_Tool::CurveOnSurface(edge, data.face, first, last);
    Require(!pcurve.IsNull(), "The edge has no pcurve on the supplied face.");
    BRepAdaptor_Curve adaptor(edge);
    GCPnts_UniformAbscissa sampler(adaptor, count, first, last, tolerance);
    Require(sampler.IsDone() && sampler.NbPoints() == count, "3D arc-length sampling failed.");
    std::vector<OcctSharp_SurfaceCurveSample> values; values.reserve(count);
    const bool reversed = edge.Orientation() == TopAbs_REVERSED;
    for (int index = 1; index <= count; ++index) {
      const double parameter = sampler.Parameter(reversed ? count + 1 - index : index);
      gp_Pnt point; gp_Vec tangent; adaptor.D1(parameter, point, tangent);
      Require(tangent.Magnitude() > tolerance, "The curve tangent is singular at a sample.");
      tangent.Normalize(); if (reversed) tangent.Reverse();
      values.push_back({parameter, Copy2d(pcurve->Value(parameter)), Copy(point), Copy(tangent)});
    }
    std::copy(values.begin(), values.end(), output);
  });
}

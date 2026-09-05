// Native Modeling/GeometryInspection implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRep_Tool.hxx>
#include <GCPnts_AbscissaPoint.hxx>
#include <Geom2d_Curve.hxx>
#include <GeomAPI_ProjectPointOnCurve.hxx>
#include <GeomAPI_ProjectPointOnSurf.hxx>
#include <Geom_Curve.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <Geom_Surface.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <Standard_Handle.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <cmath>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Vec.hxx>
#include <gp_Vec2d.hxx>
#include <utility>

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_curve_snapshot(
  const OcctSharp_ShapeHandle* edge, OcctSharp_EdgeCurveSnapshot* out_snapshot)
{
  if (out_snapshot == nullptr)
  {
    SetLastError("The edge curve snapshot output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_snapshot = {};
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A BRep curve snapshot requires an edge shape.");

    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    const double first = curve.FirstParameter();
    const double last = curve.LastParameter();
    if (!std::isfinite(first) || !std::isfinite(last))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge curve does not have finite parameter bounds.");

    const gp_Pnt start = curve.Value(first);
    const gp_Pnt end = curve.Value(last);
    *out_snapshot = {
      static_cast<int32_t>(curve.GetType()),
      first,
      last,
      { start.X(), start.Y(), start.Z() },
      { end.X(), end.Y(), end.Z() }
    };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_surface_snapshot(
  const OcctSharp_ShapeHandle* face, const int32_t restrict_to_face,
  OcctSharp_FaceSurfaceSnapshot* out_snapshot)
{
  if (out_snapshot == nullptr)
  {
    SetLastError("The face surface snapshot output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_snapshot = {};
  if (restrict_to_face != 0 && restrict_to_face != 1)
  {
    SetLastError("The face surface restriction flag must be zero or one.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A BRep surface snapshot requires a face shape.");

    const BRepAdaptor_Surface surface(TopoDS::Face(face->Value), restrict_to_face != 0);
    *out_snapshot = {
      static_cast<int32_t>(surface.GetType()),
      surface.FirstUParameter(),
      surface.LastUParameter(),
      surface.FirstVParameter(),
      surface.LastVParameter()
    };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_evaluate(
  const OcctSharp_ShapeHandle* edge, const double parameter, OcctSharp_CurveEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The curve evaluation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(parameter)) { SetLastError("The curve parameter must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve evaluation requires an edge shape.");
    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    const double first = curve.FirstParameter();
    const double last = curve.LastParameter();
    if (parameter < first || parameter > last)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The curve parameter is outside the edge range.");
    gp_Pnt point;
    gp_Vec derivative;
    curve.D1(parameter, point, derivative);
    if (derivative.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no defined tangent at the requested parameter.");
    const gp_Dir tangent(derivative);
    *out_result = { parameter,
      { point.X(), point.Y(), point.Z() },
      { tangent.X(), tangent.Y(), tangent.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_evaluate_derivatives(
  const OcctSharp_ShapeHandle* edge, const double parameter,
  OcctSharp_CurveDerivativeEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The curve derivative output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(parameter)) { SetLastError("The curve parameter must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve derivative evaluation requires an edge shape.");
    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    if (parameter < curve.FirstParameter() || parameter > curve.LastParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The curve parameter is outside the edge range.");
    gp_Pnt point;
    gp_Vec first_derivative;
    gp_Vec second_derivative;
    curve.D2(parameter, point, first_derivative, second_derivative);
    *out_result = { parameter,
      { point.X(), point.Y(), point.Z() },
      { first_derivative.X(), first_derivative.Y(), first_derivative.Z() },
      { second_derivative.X(), second_derivative.Y(), second_derivative.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_pcurve_snapshot(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face,
  OcctSharp_PcurveSnapshot* out_result)
{
  if (out_result == nullptr) { SetLastError("The pcurve snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  return Guard([&]
  {
    ValidateUsableShape(edge);
    ValidateUsableShape(face);
    if (edge->Value.ShapeType() != TopAbs_EDGE || face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A pcurve snapshot requires an edge and a face.");
    double first = 0.0;
    double last = 0.0;
    const opencascade::handle<Geom2d_Curve> curve = BRep_Tool::CurveOnSurface(
      TopoDS::Edge(edge->Value), TopoDS::Face(face->Value), first, last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no pcurve on the supplied face.");
    const gp_Pnt2d start = curve->Value(first);
    const gp_Pnt2d end = curve->Value(last);
    *out_result = { first, last, { start.X(), start.Y() }, { end.X(), end.Y() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_pcurve_evaluate(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face, const double parameter,
  OcctSharp_PcurveEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The pcurve evaluation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(parameter)) { SetLastError("The pcurve parameter must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    ValidateUsableShape(face);
    if (edge->Value.ShapeType() != TopAbs_EDGE || face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pcurve evaluation requires an edge and a face.");
    double first = 0.0;
    double last = 0.0;
    const opencascade::handle<Geom2d_Curve> curve = BRep_Tool::CurveOnSurface(
      TopoDS::Edge(edge->Value), TopoDS::Face(face->Value), first, last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no pcurve on the supplied face.");
    if (parameter < first || parameter > last)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The pcurve parameter is outside the edge-on-face range.");
    gp_Pnt2d point;
    gp_Vec2d derivative;
    curve->D1(parameter, point, derivative);
    if (derivative.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The pcurve has no defined tangent at the requested parameter.");
    derivative.Normalize();
    *out_result = { parameter, { point.X(), point.Y() }, { derivative.X(), derivative.Y() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_length(
  const OcctSharp_ShapeHandle* edge, double* out_length)
{
  if (out_length == nullptr) { SetLastError("The edge length output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_length = 0.0;
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve length requires an edge shape.");
    const BRepAdaptor_Curve curve(TopoDS::Edge(edge->Value));
    *out_length = GCPnts_AbscissaPoint::Length(curve, curve.FirstParameter(), curve.LastParameter());
    if (!std::isfinite(*out_length) || *out_length < 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned an invalid edge length.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_project_point(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_Xyz point, OcctSharp_CurveProjection* out_result)
{
  if (out_result == nullptr) { SetLastError("The curve projection output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  return Guard([&]
  {
    ValidateFinite(point.x, "Projection point X must be finite.");
    ValidateFinite(point.y, "Projection point Y must be finite.");
    ValidateFinite(point.z, "Projection point Z must be finite.");
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve projection requires an edge shape.");
    double first = 0.0;
    double last = 0.0;
    const opencascade::handle<Geom_Curve> curve = BRep_Tool::Curve(TopoDS::Edge(edge->Value), first, last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no 3D curve to project onto.");
    GeomAPI_ProjectPointOnCurve projection(gp_Pnt(point.x, point.y, point.z), curve, first, last);
    const int count = projection.NbPoints();
    if (count <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT found no point projection on the edge curve.");
    const gp_Pnt nearest = projection.NearestPoint();
    *out_result = { projection.LowerDistanceParameter(),
      { nearest.X(), nearest.Y(), nearest.Z() }, projection.LowerDistance(), count };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate(
  const OcctSharp_ShapeHandle* face, const double u_parameter, const double v_parameter,
  OcctSharp_SurfaceEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The surface evaluation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(u_parameter) || !std::isfinite(v_parameter))
  { SetLastError("Surface parameters must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface evaluation requires a face shape.");
    const BRepAdaptor_Surface surface(TopoDS::Face(face->Value), true);
    if (u_parameter < surface.FirstUParameter() || u_parameter > surface.LastUParameter()
        || v_parameter < surface.FirstVParameter() || v_parameter > surface.LastVParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UV parameters are outside the face range.");
    gp_Pnt point;
    gp_Vec derivative_u;
    gp_Vec derivative_v;
    surface.D1(u_parameter, v_parameter, point, derivative_u, derivative_v);
    gp_Vec normal = derivative_u.Crossed(derivative_v);
    if (normal.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no defined normal at the requested parameters.");
    normal.Normalize();
    if (face->Value.Orientation() == TopAbs_REVERSED) normal.Reverse();
    *out_result = { u_parameter, v_parameter,
      { point.X(), point.Y(), point.Z() }, { normal.X(), normal.Y(), normal.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_evaluate_derivatives(
  const OcctSharp_ShapeHandle* face, const double u_parameter, const double v_parameter,
  OcctSharp_SurfaceDerivativeEvaluation* out_result)
{
  if (out_result == nullptr) { SetLastError("The surface derivative output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(u_parameter) || !std::isfinite(v_parameter))
  { SetLastError("Surface parameters must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface derivative evaluation requires a face shape.");
    const BRepAdaptor_Surface surface(TopoDS::Face(face->Value), true);
    if (u_parameter < surface.FirstUParameter() || u_parameter > surface.LastUParameter()
        || v_parameter < surface.FirstVParameter() || v_parameter > surface.LastVParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The UV parameters are outside the face range.");
    gp_Pnt point;
    gp_Vec derivative_u;
    gp_Vec derivative_v;
    surface.D1(u_parameter, v_parameter, point, derivative_u, derivative_v);
    gp_Vec normal = derivative_u.Crossed(derivative_v);
    if (normal.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no defined normal at the requested parameters.");
    normal.Normalize();
    if (face->Value.Orientation() == TopAbs_REVERSED) normal.Reverse();
    *out_result = { u_parameter, v_parameter,
      { point.X(), point.Y(), point.Z() },
      { derivative_u.X(), derivative_u.Y(), derivative_u.Z() },
      { derivative_v.X(), derivative_v.Y(), derivative_v.Z() },
      { normal.X(), normal.Y(), normal.Z() } };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_project_point(
  const OcctSharp_ShapeHandle* face, const OcctSharp_Xyz point, const double tolerance,
  OcctSharp_SurfaceProjection* out_result)
{
  if (out_result == nullptr) { SetLastError("The surface projection output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  if (!std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Surface projection tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateFinite(point.x, "Projection point X must be finite.");
    ValidateFinite(point.y, "Projection point Y must be finite.");
    ValidateFinite(point.z, "Projection point Z must be finite.");
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface projection requires a face shape.");
    const TopoDS_Face topology_face = TopoDS::Face(face->Value);
    const BRepAdaptor_Surface bounds(topology_face, true);
    const opencascade::handle<Geom_Surface> surface = BRep_Tool::Surface(topology_face);
    if (surface.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no surface to project onto.");
    GeomAPI_ProjectPointOnSurf projection(
      gp_Pnt(point.x, point.y, point.z), surface,
      bounds.FirstUParameter(), bounds.LastUParameter(),
      bounds.FirstVParameter(), bounds.LastVParameter(), tolerance);
    const int count = projection.NbPoints();
    if (!projection.IsDone() || count <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT found no point projection on the face surface.");
    double u = 0.0;
    double v = 0.0;
    projection.LowerDistanceParameters(u, v);
    const gp_Pnt nearest = projection.NearestPoint();
    *out_result = { u, v, { nearest.X(), nearest.Y(), nearest.Z() }, projection.LowerDistance(), count };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_edge_trim(
  const OcctSharp_ShapeHandle* edge, const double first_parameter, const double last_parameter,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The trimmed edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(first_parameter) || !std::isfinite(last_parameter) || first_parameter >= last_parameter)
  { SetLastError("Trimmed edge parameters must be finite and strictly increasing."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Edge trimming requires an edge shape.");
    double source_first = 0.0;
    double source_last = 0.0;
    const opencascade::handle<Geom_Curve> curve = BRep_Tool::Curve(TopoDS::Edge(edge->Value), source_first, source_last);
    if (curve.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no 3D curve to trim.");
    if (first_parameter < source_first || last_parameter > source_last)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested trim interval is outside the edge range.");
    const opencascade::handle<Geom_TrimmedCurve> trimmed = new Geom_TrimmedCurve(curve, first_parameter, last_parameter);
    BRepBuilderAPI_MakeEdge builder(trimmed);
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the trimmed edge.");
    TopoDS_Edge result = builder.Edge();
    if (edge->Value.Orientation() == TopAbs_REVERSED) result.Reverse();
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_trim(
  const OcctSharp_ShapeHandle* face,
  const double first_u_parameter, const double last_u_parameter,
  const double first_v_parameter, const double last_v_parameter,
  const double tolerance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The trimmed face output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(first_u_parameter) || !std::isfinite(last_u_parameter)
      || !std::isfinite(first_v_parameter) || !std::isfinite(last_v_parameter)
      || first_u_parameter >= last_u_parameter || first_v_parameter >= last_v_parameter
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Trimmed face bounds must be finite and increasing, and tolerance must be positive."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(face);
    if (face->Value.ShapeType() != TopAbs_FACE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Face trimming requires a face shape.");
    const TopoDS_Face topology_face = TopoDS::Face(face->Value);
    const BRepAdaptor_Surface bounds(topology_face, true);
    if (first_u_parameter < bounds.FirstUParameter() || last_u_parameter > bounds.LastUParameter()
        || first_v_parameter < bounds.FirstVParameter() || last_v_parameter > bounds.LastVParameter())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested UV trim rectangle is outside the face range.");
    const opencascade::handle<Geom_Surface> surface = BRep_Tool::Surface(topology_face);
    if (surface.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no surface to trim.");
    const opencascade::handle<Geom_RectangularTrimmedSurface> trimmed =
      new Geom_RectangularTrimmedSurface(
        surface, first_u_parameter, last_u_parameter, first_v_parameter, last_v_parameter, true, true);
    BRepBuilderAPI_MakeFace builder(trimmed, tolerance);
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the trimmed face.");
    TopoDS_Face result = builder.Face();
    if (face->Value.Orientation() == TopAbs_REVERSED) result.Reverse();
    *out_shape = AllocateShape(std::move(result));
  });
}

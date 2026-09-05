// Native Modeling/Freeform implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "Modeling/Freeform.hxx"
#include "Modeling/Topology.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepAlgoAPI_Splitter.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepFill.hxx>
#include <BRepFill_Filling.hxx>
#include <BRepOffsetAPI_MakeOffset.hxx>
#include <BRepOffsetAPI_MakePipeShell.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepTools.hxx>
#include <BRep_Tool.hxx>
#include <GeomAPI_ExtremaCurveCurve.hxx>
#include <GeomAPI_IntCS.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <GeomAPI_PointsToBSpline.hxx>
#include <GeomAPI_PointsToBSplineSurface.hxx>
#include <GeomAPI_ProjectPointOnCurve.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BSplineSurface.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_BezierSurface.hxx>
#include <Geom_Curve.hxx>
#include <Geom_RectangularTrimmedSurface.hxx>
#include <Geom_Surface.hxx>
#include <Geom_TrimmedCurve.hxx>
#include <NCollection_Array1.hxx>
#include <NCollection_Array2.hxx>
#include <NCollection_HArray1.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_List.hxx>
#include <ShapeFix_Face.hxx>
#include <ShapeFix_Shape.hxx>
#include <ShapeFix_Shell.hxx>
#include <Standard_Handle.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Shape.hxx>
#include <cmath>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

namespace OcctSharp::Native
{
opencascade::handle<Geom_Curve> GetEdgeCurve(const OcctSharp_ShapeHandle* edge)
{
  ValidateUsableShape(edge);
  if (edge->Value.ShapeType() != TopAbs_EDGE)
    throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The freeform operation requires an edge.");
  double first = 0.0;
  double last = 0.0;
  opencascade::handle<Geom_Curve> curve = BRep_Tool::Curve(TopoDS::Edge(edge->Value), first, last);
  if (curve.IsNull())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The edge has no usable 3D curve.");
  return curve;
}

opencascade::handle<Geom_Surface> GetFaceSurface(const OcctSharp_ShapeHandle* face)
{
  ValidateUsableShape(face);
  if (face->Value.ShapeType() != TopAbs_FACE)
    throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The freeform operation requires a face.");
  opencascade::handle<Geom_Surface> surface = BRep_Tool::Surface(TopoDS::Face(face->Value));
  if (surface.IsNull())
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The face has no usable surface.");
  return surface;
}

GeomAbs_Shape ToContinuity(const int32_t value)
{
  switch (value)
  {
    case 0: return GeomAbs_C0;
    case 1: return GeomAbs_C1;
    case 2: return GeomAbs_C2;
    case 3: return GeomAbs_C3;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Continuity must be between C0 and C3.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_create(
  const int32_t kind, const OcctSharp_Xyz* poles, const double* weights, const int32_t pole_count,
  const double* knots, const int32_t* multiplicities, const int32_t knot_count,
  const int32_t degree, const int32_t periodic, OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The curve output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  if (poles == nullptr || pole_count < 2 || (periodic != 0 && periodic != 1))
  { SetLastError("A freeform curve requires at least two poles and a Boolean periodic flag."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array1<gp_Pnt> native_poles(1, pole_count);
    NCollection_Array1<double> native_weights(1, pole_count);
    for (int32_t index = 0; index < pole_count; ++index)
    {
      native_poles.SetValue(index + 1, ToPoint(poles[index], "Curve poles must be finite."));
      const double weight = weights == nullptr ? 1.0 : weights[index];
      if (!std::isfinite(weight) || weight <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve weights must be finite and greater than zero.");
      native_weights.SetValue(index + 1, weight);
    }

    opencascade::handle<Geom_Curve> curve;
    if (kind == 1)
    {
      if (pole_count > Geom_BezierCurve::MaxDegree() + 1)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The Bezier pole count exceeds the OCCT maximum degree.");
      curve = weights == nullptr
        ? opencascade::handle<Geom_Curve>(new Geom_BezierCurve(native_poles))
        : opencascade::handle<Geom_Curve>(new Geom_BezierCurve(native_poles, native_weights));
    }
    else if (kind == 2)
    {
      if (knots == nullptr || multiplicities == nullptr || knot_count < 2 || degree < 1 || degree > Geom_BSplineCurve::MaxDegree())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A B-spline curve requires degree and matching knot/multiplicity arrays.");
      NCollection_Array1<double> native_knots(1, knot_count);
      NCollection_Array1<int> native_multiplicities(1, knot_count);
      for (int32_t index = 0; index < knot_count; ++index)
      {
        if (!std::isfinite(knots[index]) || (index > 0 && knots[index] <= knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "B-spline knots must be finite and strictly increasing.");
        native_knots.SetValue(index + 1, knots[index]);
        native_multiplicities.SetValue(index + 1, multiplicities[index]);
      }
      curve = weights == nullptr
        ? opencascade::handle<Geom_Curve>(new Geom_BSplineCurve(native_poles, native_knots, native_multiplicities, degree, periodic != 0))
        : opencascade::handle<Geom_Curve>(new Geom_BSplineCurve(native_poles, native_weights, native_knots, native_multiplicities, degree, periodic != 0));
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve kind must be Bezier or B-spline.");

    BRepBuilderAPI_MakeEdge builder(curve);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT freeform edge construction did not complete.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_interpolate(
  const OcctSharp_Xyz* points, const int32_t point_count, const OcctSharp_Xyz* endpoint_tangents,
  const int32_t tangent_count, const int32_t periodic, const double tolerance,
  OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The interpolation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  if (points == nullptr || point_count < (periodic == 0 ? 2 : 3) || (periodic != 0 && periodic != 1)
      || (tangent_count != 0 && tangent_count != 2) || (tangent_count == 2 && endpoint_tangents == nullptr)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Interpolation points, endpoint tangents, periodic flag, or tolerance are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<NCollection_HArray1<gp_Pnt>> native_points = new NCollection_HArray1<gp_Pnt>(1, point_count);
    for (int32_t index = 0; index < point_count; ++index)
      native_points->SetValue(index + 1, ToPoint(points[index], "Interpolation points must be finite."));
    GeomAPI_Interpolate interpolation(native_points, periodic != 0, tolerance);
    if (tangent_count == 2)
      interpolation.Load(ToVector(endpoint_tangents[0], "The initial tangent must be finite and non-zero."),
                         ToVector(endpoint_tangents[1], "The final tangent must be finite and non-zero."), true);
    interpolation.Perform();
    if (!interpolation.IsDone() || interpolation.Curve().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve interpolation did not complete.");
    BRepBuilderAPI_MakeEdge builder(interpolation.Curve());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the interpolated edge.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_approximate(
  const OcctSharp_Xyz* points, const int32_t point_count, const int32_t minimum_degree,
  const int32_t maximum_degree, const int32_t continuity, const double tolerance,
  OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The approximation output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  if (points == nullptr || point_count < 2 || minimum_degree < 1 || maximum_degree < minimum_degree
      || maximum_degree > Geom_BSplineCurve::MaxDegree() || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Curve approximation arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array1<gp_Pnt> native_points(1, point_count);
    for (int32_t index = 0; index < point_count; ++index)
      native_points.SetValue(index + 1, ToPoint(points[index], "Approximation points must be finite."));
    GeomAPI_PointsToBSpline approximation(native_points, minimum_degree, maximum_degree, ToContinuity(continuity), tolerance);
    if (!approximation.IsDone() || approximation.Curve().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve approximation did not complete.");
    BRepBuilderAPI_MakeEdge builder(approximation.Curve());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the approximated edge.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_info(
  const OcctSharp_ShapeHandle* edge, OcctSharp_FreeformCurveInfo* out_info)
{
  if (out_info == nullptr) { SetLastError("The curve-info output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> curve = GetEdgeCurve(edge);
    opencascade::handle<Geom_TrimmedCurve> trimmed = opencascade::handle<Geom_TrimmedCurve>::DownCast(curve);
    if (!trimmed.IsNull()) curve = trimmed->BasisCurve();
    double first = 0.0, last = 0.0;
    BRep_Tool::Range(TopoDS::Edge(edge->Value), first, last);
    const opencascade::handle<Geom_BezierCurve> bezier = opencascade::handle<Geom_BezierCurve>::DownCast(curve);
    const opencascade::handle<Geom_BSplineCurve> bspline = opencascade::handle<Geom_BSplineCurve>::DownCast(curve);
    if (!bezier.IsNull()) *out_info = {1, bezier->Degree(), 0, bezier->IsRational() ? 1 : 0, bezier->NbPoles(), 0, first, last};
    else if (!bspline.IsNull()) *out_info = {2, bspline->Degree(), bspline->IsPeriodic() ? 1 : 0,
      bspline->IsRational() ? 1 : 0, bspline->NbPoles(), bspline->NbKnots(), first, last};
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The edge is not backed by a Bezier or B-spline curve.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_copy_definition(
  const OcctSharp_ShapeHandle* edge, OcctSharp_Xyz* poles, const int32_t pole_capacity,
  double* weights, const int32_t weight_capacity, double* knots, const int32_t knot_capacity,
  int32_t* multiplicities, const int32_t multiplicity_capacity)
{
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> curve = GetEdgeCurve(edge);
    const opencascade::handle<Geom_TrimmedCurve> trimmed = opencascade::handle<Geom_TrimmedCurve>::DownCast(curve);
    if (!trimmed.IsNull()) curve = trimmed->BasisCurve();
    const opencascade::handle<Geom_BezierCurve> bezier = opencascade::handle<Geom_BezierCurve>::DownCast(curve);
    const opencascade::handle<Geom_BSplineCurve> bspline = opencascade::handle<Geom_BSplineCurve>::DownCast(curve);
    const int32_t pole_count = !bezier.IsNull() ? bezier->NbPoles() : !bspline.IsNull() ? bspline->NbPoles() : 0;
    const int32_t knot_count = !bspline.IsNull() ? bspline->NbKnots() : 0;
    if (pole_count == 0) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The edge is not backed by a Bezier or B-spline curve.");
    ValidateOutputCapacity(pole_capacity, pole_count, poles, "The curve pole buffer is too small.");
    ValidateOutputCapacity(weight_capacity, pole_count, weights, "The curve weight buffer is too small.");
    ValidateOutputCapacity(knot_capacity, knot_count, knots, "The curve knot buffer is too small.");
    ValidateOutputCapacity(multiplicity_capacity, knot_count, multiplicities, "The curve multiplicity buffer is too small.");
    for (int32_t index = 1; index <= pole_count; ++index)
    {
      poles[index - 1] = FromPoint(!bezier.IsNull() ? bezier->Pole(index) : bspline->Pole(index));
      weights[index - 1] = !bezier.IsNull() ? bezier->Weight(index) : bspline->Weight(index);
    }
    for (int32_t index = 1; index <= knot_count; ++index)
    {
      knots[index - 1] = bspline->Knot(index);
      multiplicities[index - 1] = bspline->Multiplicity(index);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_edit(
  const OcctSharp_ShapeHandle* edge, const int32_t operation, const int32_t degree,
  const double first_parameter, const double last_parameter, OcctSharp_ShapeHandle** out_edge)
{
  if (out_edge == nullptr) { SetLastError("The edited-curve output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_edge = nullptr;
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> source = GetEdgeCurve(edge);
    const opencascade::handle<Geom_TrimmedCurve> trimmed = opencascade::handle<Geom_TrimmedCurve>::DownCast(source);
    if (!trimmed.IsNull()) source = trimmed->BasisCurve();
    opencascade::handle<Geom_BezierCurve> bezier;
    opencascade::handle<Geom_BSplineCurve> bspline;
    const auto source_bezier = opencascade::handle<Geom_BezierCurve>::DownCast(source);
    const auto source_bspline = opencascade::handle<Geom_BSplineCurve>::DownCast(source);
    if (!source_bezier.IsNull()) bezier = new Geom_BezierCurve(*source_bezier);
    else if (!source_bspline.IsNull()) bspline = new Geom_BSplineCurve(*source_bspline);
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Curve editing requires a Bezier or B-spline edge.");
    if (operation == 1)
    {
      if (degree < 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The elevated degree must be positive.");
      if (!bezier.IsNull()) bezier->Increase(degree); else bspline->IncreaseDegree(degree);
    }
    else if (operation == 2) { if (!bezier.IsNull()) bezier->Reverse(); else bspline->Reverse(); }
    else if (operation == 3)
    {
      if (!std::isfinite(first_parameter) || !std::isfinite(last_parameter) || first_parameter >= last_parameter)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve segment bounds must be finite and increasing.");
      if (!bezier.IsNull()) bezier->Segment(first_parameter, last_parameter);
      else bspline->Segment(first_parameter, last_parameter);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The curve edit operation is unknown.");
    BRepBuilderAPI_MakeEdge builder(!bezier.IsNull() ? opencascade::handle<Geom_Curve>(bezier) : opencascade::handle<Geom_Curve>(bspline));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the edited curve edge.");
    *out_edge = AllocateShape(builder.Edge());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_split(
  const OcctSharp_ShapeHandle* edge, const double* parameters, const int32_t parameter_count,
  OcctSharp_ShapeHandle** out_edges, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The split written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    opencascade::handle<Geom_Curve> curve = GetEdgeCurve(edge);
    double first = 0.0, last = 0.0;
    BRep_Tool::Range(TopoDS::Edge(edge->Value), first, last);
    if (parameter_count < 0 || (parameter_count > 0 && parameters == nullptr))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve split parameters are invalid.");
    ValidateOutputCapacity(capacity, parameter_count + 1, out_edges, "The curve split output buffer is too small.");
    double start = first;
    for (int32_t index = 0; index <= parameter_count; ++index)
    {
      const double end = index == parameter_count ? last : parameters[index];
      if (!std::isfinite(end) || end <= start || end >= last && index < parameter_count)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Curve split parameters must be strictly increasing inside the edge range.");
      const opencascade::handle<Geom_TrimmedCurve> segment = new Geom_TrimmedCurve(curve, start, end);
      BRepBuilderAPI_MakeEdge builder(segment);
      if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build a curve split segment.");
      out_edges[index] = AllocateShape(builder.Edge());
      ++*out_written;
      start = end;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_project_count(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_Xyz point, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The projection count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    GeomAPI_ProjectPointOnCurve projection(ToPoint(point, "The projection point must be finite."), GetEdgeCurve(edge));
    *out_count = projection.NbPoints();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_project_copy(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_Xyz point,
  OcctSharp_FreeformSolution* solutions, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The projection written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    GeomAPI_ProjectPointOnCurve projection(ToPoint(point, "The projection point must be finite."), GetEdgeCurve(edge));
    ValidateOutputCapacity(capacity, projection.NbPoints(), solutions, "The projection output buffer is too small.");
    for (int32_t index = 1; index <= projection.NbPoints(); ++index)
    {
      solutions[index - 1] = {FromPoint(projection.Point(index)), point, projection.Parameter(index), 0.0, 0.0, projection.Distance(index)};
      ++*out_written;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_extrema_count(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The extrema count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { GeomAPI_ExtremaCurveCurve extrema(GetEdgeCurve(first), GetEdgeCurve(second)); *out_count = extrema.NbExtrema(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_extrema_copy(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_FreeformSolution* solutions, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The extrema written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    GeomAPI_ExtremaCurveCurve extrema(GetEdgeCurve(first), GetEdgeCurve(second));
    ValidateOutputCapacity(capacity, extrema.NbExtrema(), solutions, "The extrema output buffer is too small.");
    for (int32_t index = 1; index <= extrema.NbExtrema(); ++index)
    {
      gp_Pnt first_point, second_point; double first_parameter = 0.0, second_parameter = 0.0;
      extrema.Points(index, first_point, second_point); extrema.Parameters(index, first_parameter, second_parameter);
      solutions[index - 1] = {FromPoint(first_point), FromPoint(second_point), first_parameter, second_parameter, 0.0, extrema.Distance(index)};
      ++*out_written;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_face_intersection_count(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The intersection count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    GeomAPI_IntCS intersection(GetEdgeCurve(edge), GetFaceSurface(face));
    if (!intersection.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve-surface intersection did not complete.");
    *out_count = intersection.NbPoints();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_curve_face_intersection_copy(
  const OcctSharp_ShapeHandle* edge, const OcctSharp_ShapeHandle* face,
  OcctSharp_FreeformSolution* solutions, const int32_t capacity, int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The intersection written-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  return Guard([&]
  {
    GeomAPI_IntCS intersection(GetEdgeCurve(edge), GetFaceSurface(face));
    if (!intersection.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve-surface intersection did not complete.");
    ValidateOutputCapacity(capacity, intersection.NbPoints(), solutions, "The intersection output buffer is too small.");
    for (int32_t index = 1; index <= intersection.NbPoints(); ++index)
    {
      double curve_parameter = 0.0, u = 0.0, v = 0.0;
      intersection.Parameters(index, curve_parameter, u, v);
      const gp_Pnt& point = intersection.Point(index);
      solutions[index - 1] = {FromPoint(point), FromPoint(point), curve_parameter, u, v, 0.0};
      ++*out_written;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_planar_profile(
  const OcctSharp_Xyz* points, const int32_t point_count, const OcctSharp_Xyz origin,
  const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction, const int32_t interpolate,
  const double tolerance, OcctSharp_ShapeHandle** out_wire)
{
  if (out_wire == nullptr) { SetLastError("The profile output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_wire = nullptr;
  if (points == nullptr || point_count < 3 || (interpolate != 0 && interpolate != 1)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Planar-profile arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const gp_Ax2 plane(ToPoint(origin, "The profile origin must be finite."),
      gp_Dir(ToVector(normal, "The profile normal must be finite and non-zero.")),
      gp_Dir(ToVector(x_direction, "The profile X direction must be finite and non-zero.")));
    auto located = [&](const OcctSharp_Xyz& value)
    {
      if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z) || std::abs(value.z) > tolerance)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Located profile points must have finite XY and zero local Z within tolerance.");
      return plane.Location().Translated(gp_Vec(plane.XDirection()) * value.x + gp_Vec(plane.YDirection()) * value.y);
    };
    if (interpolate != 0)
    {
      const opencascade::handle<NCollection_HArray1<gp_Pnt>> native_points = new NCollection_HArray1<gp_Pnt>(1, point_count);
      for (int32_t index = 0; index < point_count; ++index) native_points->SetValue(index + 1, located(points[index]));
      GeomAPI_Interpolate interpolation(native_points, true, tolerance); interpolation.Perform();
      if (!interpolation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT planar-profile interpolation did not complete.");
      BRepBuilderAPI_MakeEdge edge(interpolation.Curve()); BRepBuilderAPI_MakeWire wire(edge.Edge());
      if (!wire.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the interpolated profile wire.");
      *out_wire = AllocateShape(wire.Wire());
    }
    else
    {
      BRepBuilderAPI_MakePolygon polygon;
      for (int32_t index = 0; index < point_count; ++index) polygon.Add(located(points[index]));
      polygon.Close();
      if (!polygon.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the polygon profile wire.");
      *out_wire = AllocateShape(polygon.Wire());
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_planar_offset(
  const OcctSharp_ShapeHandle* wire, const double distance, const double altitude,
  const int32_t join_type, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The planar-offset output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(distance) || !std::isfinite(altitude) || join_type < 0 || join_type > 2)
  { SetLastError("Planar-offset distance, altitude, or join type is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(wire);
    if (wire->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar offset requires a wire.");
    BRepOffsetAPI_MakeOffset builder(TopoDS::Wire(wire->Value), static_cast<GeomAbs_JoinType>(join_type), false);
    builder.Perform(distance, altitude);
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT planar-wire offset did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_create(
  const int32_t kind, const OcctSharp_Xyz* poles, const double* weights,
  const int32_t u_pole_count, const int32_t v_pole_count,
  const double* u_knots, const int32_t* u_multiplicities, const int32_t u_knot_count,
  const double* v_knots, const int32_t* v_multiplicities, const int32_t v_knot_count,
  const int32_t u_degree, const int32_t v_degree, const int32_t u_periodic, const int32_t v_periodic,
  const double* bounds, const double tolerance, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The surface output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  if (poles == nullptr || u_pole_count < 2 || v_pole_count < 2
      || (u_periodic != 0 && u_periodic != 1) || (v_periodic != 0 && v_periodic != 1)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Freeform-surface pole grid, periodic flags, or tolerance are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array2<gp_Pnt> native_poles(1, u_pole_count, 1, v_pole_count);
    NCollection_Array2<double> native_weights(1, u_pole_count, 1, v_pole_count);
    for (int32_t u = 0; u < u_pole_count; ++u)
      for (int32_t v = 0; v < v_pole_count; ++v)
      {
        const int32_t index = u * v_pole_count + v;
        native_poles.SetValue(u + 1, v + 1, ToPoint(poles[index], "Surface poles must be finite."));
        const double weight = weights == nullptr ? 1.0 : weights[index];
        if (!std::isfinite(weight) || weight <= 0.0)
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface weights must be finite and greater than zero.");
        native_weights.SetValue(u + 1, v + 1, weight);
      }

    opencascade::handle<Geom_Surface> surface;
    if (kind == 1)
    {
      if (u_pole_count > Geom_BezierSurface::MaxDegree() + 1 || v_pole_count > Geom_BezierSurface::MaxDegree() + 1)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The Bezier surface grid exceeds the OCCT maximum degree.");
      surface = weights == nullptr
        ? opencascade::handle<Geom_Surface>(new Geom_BezierSurface(native_poles))
        : opencascade::handle<Geom_Surface>(new Geom_BezierSurface(native_poles, native_weights));
    }
    else if (kind == 2)
    {
      if (u_knots == nullptr || v_knots == nullptr || u_multiplicities == nullptr || v_multiplicities == nullptr
          || u_knot_count < 2 || v_knot_count < 2 || u_degree < 1 || v_degree < 1
          || u_degree > Geom_BSplineSurface::MaxDegree() || v_degree > Geom_BSplineSurface::MaxDegree())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A B-spline surface requires valid U/V degree, knot, and multiplicity arrays.");
      NCollection_Array1<double> native_u_knots(1, u_knot_count), native_v_knots(1, v_knot_count);
      NCollection_Array1<int> native_u_multiplicities(1, u_knot_count), native_v_multiplicities(1, v_knot_count);
      for (int32_t index = 0; index < u_knot_count; ++index)
      {
        if (!std::isfinite(u_knots[index]) || (index > 0 && u_knots[index] <= u_knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface U knots must be finite and strictly increasing.");
        native_u_knots.SetValue(index + 1, u_knots[index]); native_u_multiplicities.SetValue(index + 1, u_multiplicities[index]);
      }
      for (int32_t index = 0; index < v_knot_count; ++index)
      {
        if (!std::isfinite(v_knots[index]) || (index > 0 && v_knots[index] <= v_knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface V knots must be finite and strictly increasing.");
        native_v_knots.SetValue(index + 1, v_knots[index]); native_v_multiplicities.SetValue(index + 1, v_multiplicities[index]);
      }
      surface = weights == nullptr
        ? opencascade::handle<Geom_Surface>(new Geom_BSplineSurface(native_poles, native_u_knots, native_v_knots,
            native_u_multiplicities, native_v_multiplicities, u_degree, v_degree, u_periodic != 0, v_periodic != 0))
        : opencascade::handle<Geom_Surface>(new Geom_BSplineSurface(native_poles, native_weights, native_u_knots, native_v_knots,
            native_u_multiplicities, native_v_multiplicities, u_degree, v_degree, u_periodic != 0, v_periodic != 0));
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface kind must be Bezier or B-spline.");

    BRepBuilderAPI_MakeFace builder;
    if (bounds == nullptr) builder.Init(surface, true, tolerance);
    else
    {
      for (int index = 0; index < 4; ++index) ValidateFinite(bounds[index], "Surface trim bounds must be finite.");
      if (bounds[0] >= bounds[1] || bounds[2] >= bounds[3])
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface trim bounds must be increasing.");
      builder.Init(surface, bounds[0], bounds[1], bounds[2], bounds[3], tolerance);
    }
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT freeform face construction did not complete.");
    *out_face = AllocateShape(builder.Face());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_approximate(
  const OcctSharp_Xyz* points, const int32_t u_count, const int32_t v_count,
  const int32_t minimum_degree, const int32_t maximum_degree, const int32_t continuity,
  const double tolerance, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The approximated-surface output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  if (points == nullptr || u_count < 2 || v_count < 2 || minimum_degree < 1 || maximum_degree < minimum_degree
      || maximum_degree > Geom_BSplineSurface::MaxDegree() || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Surface approximation arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array2<gp_Pnt> native_points(1, u_count, 1, v_count);
    for (int32_t u = 0; u < u_count; ++u)
      for (int32_t v = 0; v < v_count; ++v)
        native_points.SetValue(u + 1, v + 1, ToPoint(points[u * v_count + v], "Surface approximation points must be finite."));
    GeomAPI_PointsToBSplineSurface approximation;
    if (continuity == -1) approximation.Interpolate(native_points);
    else approximation.Init(native_points, minimum_degree, maximum_degree, ToContinuity(continuity), tolerance);
    if (!approximation.IsDone() || approximation.Surface().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT surface approximation did not complete.");
    BRepBuilderAPI_MakeFace builder(approximation.Surface(), tolerance);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the approximated surface face.");
    *out_face = AllocateShape(builder.Face());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_info(
  const OcctSharp_ShapeHandle* face, OcctSharp_FreeformSurfaceInfo* out_info)
{
  if (out_info == nullptr) { SetLastError("The surface-info output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<Geom_Surface> surface = GetFaceSurface(face);
    const opencascade::handle<Geom_RectangularTrimmedSurface> trimmed = opencascade::handle<Geom_RectangularTrimmedSurface>::DownCast(surface);
    if (!trimmed.IsNull()) surface = trimmed->BasisSurface();
    double u1 = 0.0, u2 = 0.0, v1 = 0.0, v2 = 0.0; BRepTools::UVBounds(TopoDS::Face(face->Value), u1, u2, v1, v2);
    const auto bezier = opencascade::handle<Geom_BezierSurface>::DownCast(surface);
    const auto bspline = opencascade::handle<Geom_BSplineSurface>::DownCast(surface);
    if (!bezier.IsNull()) *out_info = {1, bezier->UDegree(), bezier->VDegree(), 0, 0,
      (bezier->IsURational() || bezier->IsVRational()) ? 1 : 0, bezier->NbUPoles(), bezier->NbVPoles(), 0, 0, u1, u2, v1, v2};
    else if (!bspline.IsNull()) *out_info = {2, bspline->UDegree(), bspline->VDegree(), bspline->IsUPeriodic() ? 1 : 0,
      bspline->IsVPeriodic() ? 1 : 0, (bspline->IsURational() || bspline->IsVRational()) ? 1 : 0,
      bspline->NbUPoles(), bspline->NbVPoles(), bspline->NbUKnots(), bspline->NbVKnots(), u1, u2, v1, v2};
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The face is not backed by a Bezier or B-spline surface.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_copy_definition(
  const OcctSharp_ShapeHandle* face, OcctSharp_Xyz* poles, const int32_t pole_capacity,
  double* weights, const int32_t weight_capacity,
  double* u_knots, const int32_t u_knot_capacity, int32_t* u_multiplicities, const int32_t u_multiplicity_capacity,
  double* v_knots, const int32_t v_knot_capacity, int32_t* v_multiplicities, const int32_t v_multiplicity_capacity)
{
  return Guard([&]
  {
    opencascade::handle<Geom_Surface> surface = GetFaceSurface(face);
    const auto trimmed = opencascade::handle<Geom_RectangularTrimmedSurface>::DownCast(surface);
    if (!trimmed.IsNull()) surface = trimmed->BasisSurface();
    const auto bezier = opencascade::handle<Geom_BezierSurface>::DownCast(surface);
    const auto bspline = opencascade::handle<Geom_BSplineSurface>::DownCast(surface);
    const int32_t u_count = !bezier.IsNull() ? bezier->NbUPoles() : !bspline.IsNull() ? bspline->NbUPoles() : 0;
    const int32_t v_count = !bezier.IsNull() ? bezier->NbVPoles() : !bspline.IsNull() ? bspline->NbVPoles() : 0;
    if (u_count == 0) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The face is not backed by a Bezier or B-spline surface.");
    const int32_t u_knot_count = !bspline.IsNull() ? bspline->NbUKnots() : 0;
    const int32_t v_knot_count = !bspline.IsNull() ? bspline->NbVKnots() : 0;
    ValidateOutputCapacity(pole_capacity, u_count * v_count, poles, "The surface pole buffer is too small.");
    ValidateOutputCapacity(weight_capacity, u_count * v_count, weights, "The surface weight buffer is too small.");
    ValidateOutputCapacity(u_knot_capacity, u_knot_count, u_knots, "The surface U-knot buffer is too small.");
    ValidateOutputCapacity(u_multiplicity_capacity, u_knot_count, u_multiplicities, "The surface U-multiplicity buffer is too small.");
    ValidateOutputCapacity(v_knot_capacity, v_knot_count, v_knots, "The surface V-knot buffer is too small.");
    ValidateOutputCapacity(v_multiplicity_capacity, v_knot_count, v_multiplicities, "The surface V-multiplicity buffer is too small.");
    for (int32_t u = 1; u <= u_count; ++u)
      for (int32_t v = 1; v <= v_count; ++v)
      {
        const int32_t index = (u - 1) * v_count + v - 1;
        poles[index] = FromPoint(!bezier.IsNull() ? bezier->Pole(u, v) : bspline->Pole(u, v));
        weights[index] = !bezier.IsNull() ? bezier->Weight(u, v) : bspline->Weight(u, v);
      }
    for (int32_t index = 1; index <= u_knot_count; ++index) { u_knots[index - 1] = bspline->UKnot(index); u_multiplicities[index - 1] = bspline->UMultiplicity(index); }
    for (int32_t index = 1; index <= v_knot_count; ++index) { v_knots[index - 1] = bspline->VKnot(index); v_multiplicities[index - 1] = bspline->VMultiplicity(index); }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_surface_edit(
  const OcctSharp_ShapeHandle* face, const int32_t operation, const int32_t u_degree, const int32_t v_degree,
  const double* bounds, const double tolerance, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The edited-surface output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  if (!std::isfinite(tolerance) || tolerance <= 0.0) { SetLastError("Surface edit tolerance is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    opencascade::handle<Geom_Surface> source = GetFaceSurface(face);
    const auto trimmed = opencascade::handle<Geom_RectangularTrimmedSurface>::DownCast(source);
    if (!trimmed.IsNull()) source = trimmed->BasisSurface();
    const auto source_bezier = opencascade::handle<Geom_BezierSurface>::DownCast(source);
    const auto source_bspline = opencascade::handle<Geom_BSplineSurface>::DownCast(source);
    opencascade::handle<Geom_BezierSurface> bezier;
    opencascade::handle<Geom_BSplineSurface> bspline;
    if (!source_bezier.IsNull()) bezier = new Geom_BezierSurface(*source_bezier);
    else if (!source_bspline.IsNull()) bspline = new Geom_BSplineSurface(*source_bspline);
    else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Surface editing requires a Bezier or B-spline face.");
    if (operation == 1) { if (!bezier.IsNull()) bezier->Increase(u_degree, v_degree); else bspline->IncreaseDegree(u_degree, v_degree); }
    else if (operation == 2) { if (!bezier.IsNull()) bezier->UReverse(); else bspline->UReverse(); }
    else if (operation == 3) { if (!bezier.IsNull()) bezier->VReverse(); else bspline->VReverse(); }
    else if (operation != 4) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The surface edit operation is unknown.");
    opencascade::handle<Geom_Surface> result = !bezier.IsNull() ? opencascade::handle<Geom_Surface>(bezier) : opencascade::handle<Geom_Surface>(bspline);
    BRepBuilderAPI_MakeFace builder;
    if (bounds != nullptr || operation == 4)
    {
      if (bounds == nullptr) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface segmentation requires four bounds.");
      for (int index = 0; index < 4; ++index) ValidateFinite(bounds[index], "Surface segment bounds must be finite.");
      if (bounds[0] >= bounds[1] || bounds[2] >= bounds[3]) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Surface segment bounds must be increasing.");
      if (operation == 4) { if (!bezier.IsNull()) bezier->Segment(bounds[0], bounds[1], bounds[2], bounds[3]); else bspline->Segment(bounds[0], bounds[1], bounds[2], bounds[3]); }
      builder.Init(result, bounds[0], bounds[1], bounds[2], bounds[3], tolerance);
    }
    else builder.Init(result, true, tolerance);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the edited freeform face.");
    *out_face = AllocateShape(builder.Face());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_ruled_face(
  const OcctSharp_ShapeHandle* first_edge, const OcctSharp_ShapeHandle* second_edge, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr) { SetLastError("The ruled-face output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(first_edge); ValidateUsableShape(second_edge);
    if (first_edge->Value.ShapeType() != TopAbs_EDGE || second_edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A ruled face requires two edges.");
    TopoDS_Face face = BRepFill::Face(TopoDS::Edge(first_edge->Value), TopoDS::Edge(second_edge->Value));
    if (face.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT ruled-face construction produced a null face.");
    *out_face = AllocateShape(face);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_fill(
  const OcctSharp_ShapeHandle* const* edges, const int32_t edge_count,
  const OcctSharp_Xyz* points, const int32_t point_count, const int32_t continuity,
  const double tolerance, OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_face)
{
  if (out_face == nullptr || out_diagnostics == nullptr) { SetLastError("The fill output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_face = nullptr; *out_diagnostics = {};
  if (edges == nullptr || edge_count < 2 || point_count < 0 || (point_count > 0 && points == nullptr)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Boundary-fill arguments are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepFill_Filling fill(3, 15, 2, false, tolerance, tolerance, 0.01, 0.1, 8, 12);
    for (int32_t index = 0; index < edge_count; ++index)
    {
      ValidateUsableShape(edges[index]);
      if (edges[index]->Value.ShapeType() != TopAbs_EDGE) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Boundary fill accepts edge constraints only.");
      fill.Add(TopoDS::Edge(edges[index]->Value), ToContinuity(continuity), true);
    }
    for (int32_t index = 0; index < point_count; ++index) fill.Add(ToPoint(points[index], "Fill constraints must be finite."));
    fill.Build();
    if (!fill.IsDone() || fill.Face().IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boundary filling did not complete.");
    const TopoDS_Face result = fill.Face();
    *out_diagnostics = {0, edge_count + point_count, 1, 0, 0, 0, BRepCheck_Analyzer(result).IsValid() ? 1 : 0,
      BRep_Tool::IsClosed(result) ? 1 : 0, fill.G0Error(), fill.G1Error(), fill.G2Error(), fill.G0Error()};
    *out_face = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_split(
  const OcctSharp_ShapeHandle* const* objects, const int32_t object_count,
  const OcctSharp_ShapeHandle* const* tools, const int32_t tool_count,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The split output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (objects == nullptr || object_count < 1 || tools == nullptr || tool_count < 1)
  { SetLastError("Topology splitting requires object and tool shapes."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_List<TopoDS_Shape> arguments, tool_shapes;
    for (int32_t index = 0; index < object_count; ++index) { ValidateUsableShape(objects[index]); arguments.Append(objects[index]->Value); }
    for (int32_t index = 0; index < tool_count; ++index) { ValidateUsableShape(tools[index]); tool_shapes.Append(tools[index]->Value); }
    BRepAlgoAPI_Splitter splitter; splitter.SetArguments(arguments); splitter.SetTools(tool_shapes); splitter.SetNonDestructive(true); splitter.Build();
    if (!splitter.IsDone() || splitter.Shape().IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT topology splitting did not complete.");
    int32_t modified = 0, generated = 0, deleted = 0;
    for (int32_t index = 0; index < object_count; ++index)
    {
      NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> sources;
      if (SupportsShapeHistory(objects[index]->Value)) sources.Add(objects[index]->Value);
      else TopExp::MapShapes(objects[index]->Value, sources);
      for (int source_index = 1; source_index <= sources.Extent(); ++source_index)
      {
        const TopoDS_Shape& source = sources(source_index);
        if (!SupportsShapeHistory(source)) continue;
        modified += splitter.Modified(source).Size();
        generated += splitter.Generated(source).Size();
        if (splitter.IsDeleted(source)) ++deleted;
      }
    }
    const TopoDS_Shape result = splitter.Shape();
    *out_diagnostics = {0, object_count + tool_count, CheckedTopologyCount(result, TopAbs_SOLID, false) + CheckedTopologyCount(result, TopAbs_FACE, false),
      modified, generated, deleted, BRepCheck_Analyzer(result).IsValid() ? 1 : 0, BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, 0.0};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_pipe_shell(
  const OcctSharp_ShapeHandle* spine, const OcctSharp_ShapeHandle* const* profiles,
  const int32_t profile_count, const int32_t make_solid, const int32_t frenet, const int32_t transition_mode,
  const double tolerance, const int32_t maximum_degree, const int32_t maximum_segments,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The pipe-shell output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (profiles == nullptr || profile_count < 1 || (make_solid != 0 && make_solid != 1) || (frenet != 0 && frenet != 1)
      || transition_mode < 0 || transition_mode > 2 || !std::isfinite(tolerance) || tolerance <= 0.0
      || maximum_degree < 1 || maximum_segments < 1)
  { SetLastError("Pipe-shell options are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(spine);
    if (spine->Value.ShapeType() != TopAbs_WIRE) throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pipe-shell construction requires a wire spine.");
    BRepOffsetAPI_MakePipeShell pipe(TopoDS::Wire(spine->Value)); pipe.SetMode(frenet != 0);
    pipe.SetTransitionMode(static_cast<BRepBuilderAPI_TransitionMode>(transition_mode));
    pipe.SetTolerance(tolerance, tolerance, 0.01); pipe.SetMaxDegree(maximum_degree); pipe.SetMaxSegments(maximum_segments); pipe.SetForceApproxC1(true);
    for (int32_t index = 0; index < profile_count; ++index) { ValidateUsableShape(profiles[index]); pipe.Add(profiles[index]->Value, false, true); }
    if (!pipe.IsReady()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The pipe-shell definition is not ready.");
    pipe.Build();
    if (!pipe.IsDone() || (make_solid != 0 && !pipe.MakeSolid()) || pipe.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT pipe-shell construction did not complete.");
    const TopoDS_Shape result = pipe.Shape();
    *out_diagnostics = {static_cast<int32_t>(pipe.GetStatus()), profile_count + 1, 1, 0, 0, 0,
      BRepCheck_Analyzer(result).IsValid() ? 1 : 0, BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, pipe.ErrorOnSurface()};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_loft(
  const OcctSharp_ShapeHandle* const* sections, const int32_t section_count,
  const int32_t make_solid, const int32_t ruled, const int32_t smoothing, const int32_t continuity,
  const int32_t maximum_degree, const double tolerance, OcctSharp_FreeformDiagnostics* out_diagnostics,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The loft output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (sections == nullptr || section_count < 2 || (make_solid != 0 && make_solid != 1) || (ruled != 0 && ruled != 1)
      || (smoothing != 0 && smoothing != 1) || maximum_degree < 1 || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Controlled-loft options are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepOffsetAPI_ThruSections loft(make_solid != 0, ruled != 0, tolerance); loft.CheckCompatibility(true);
    loft.SetSmoothing(smoothing != 0); loft.SetContinuity(ToContinuity(continuity)); loft.SetMaxDegree(maximum_degree); loft.SetMutableInput(false);
    for (int32_t index = 0; index < section_count; ++index)
    {
      ValidateUsableShape(sections[index]);
      if (sections[index]->Value.ShapeType() == TopAbs_WIRE) loft.AddWire(TopoDS::Wire(sections[index]->Value));
      else if (sections[index]->Value.ShapeType() == TopAbs_VERTEX && (index == 0 || index == section_count - 1)) loft.AddVertex(TopoDS::Vertex(sections[index]->Value));
      else throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Loft sections must be wires, with optional endpoint vertices.");
    }
    loft.Build();
    if (!loft.IsDone() || loft.Shape().IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT controlled loft did not complete.");
    const TopoDS_Shape result = loft.Shape();
    *out_diagnostics = {static_cast<int32_t>(loft.GetStatus()), section_count, 1, 0, 0, 0,
      BRepCheck_Analyzer(result).IsValid() ? 1 : 0, BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, 0.0};
    *out_shape = AllocateShape(result);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_freeform_heal(
  const OcctSharp_ShapeHandle* shape, const double tolerance,
  OcctSharp_FreeformDiagnostics* out_diagnostics, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || out_diagnostics == nullptr) { SetLastError("The heal output pointers are null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr; *out_diagnostics = {};
  if (!std::isfinite(tolerance) || tolerance <= 0.0) { SetLastError("Healing tolerance is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape); const bool before_valid = BRepCheck_Analyzer(shape->Value).IsValid(); TopoDS_Shape result;
    if (shape->Value.ShapeType() == TopAbs_FACE)
    {
      ShapeFix_Face fix(TopoDS::Face(shape->Value)); fix.SetPrecision(tolerance); fix.SetMinTolerance(tolerance * 0.1); fix.SetMaxTolerance(tolerance * 10.0); fix.Perform(); result = fix.Result();
    }
    else if (shape->Value.ShapeType() == TopAbs_SHELL)
    {
      ShapeFix_Shell fix(TopoDS::Shell(shape->Value)); fix.SetPrecision(tolerance); fix.SetMinTolerance(tolerance * 0.1); fix.SetMaxTolerance(tolerance * 10.0); fix.Perform(); result = fix.Shape();
    }
    else
    {
      ShapeFix_Shape fix(shape->Value); fix.SetPrecision(tolerance); fix.SetMinTolerance(tolerance * 0.1); fix.SetMaxTolerance(tolerance * 10.0); fix.Perform(); result = fix.Shape();
    }
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT freeform healing produced a null result.");
    const bool after_valid = BRepCheck_Analyzer(result).IsValid();
    *out_diagnostics = {0, 1, 1, before_valid == after_valid ? 0 : 1, 0, 0, after_valid ? 1 : 0,
      BRep_Tool::IsClosed(result) ? 1 : 0, 0.0, 0.0, 0.0, 0.0};
    *out_shape = AllocateShape(result);
  });
}

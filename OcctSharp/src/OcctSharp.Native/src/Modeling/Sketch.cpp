// Native Modeling/Sketch implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "Modeling/Sketch.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepBuilderAPI_Copy.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepCheck_Analyzer.hxx>
#include <BRepClass_FaceClassifier.hxx>
#include <BRep_Builder.hxx>
#include <Geom2dAPI_InterCurveCurve.hxx>
#include <Geom2dAPI_ProjectPointOnCurve.hxx>
#include <Geom2d_BSplineCurve.hxx>
#include <Geom2d_BezierCurve.hxx>
#include <Geom2d_Circle.hxx>
#include <Geom2d_Curve.hxx>
#include <Geom2d_Ellipse.hxx>
#include <Geom2d_Line.hxx>
#include <Geom2d_TrimmedCurve.hxx>
#include <GeomAPI.hxx>
#include <Geom_Curve.hxx>
#include <IntRes2d_IntersectionPoint.hxx>
#include <NCollection_Array1.hxx>
#include <Precision.hxx>
#include <Standard_Handle.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <gp.hxx>
#include <gp_Ax22d.hxx>
#include <gp_Ax3.hxx>
#include <gp_Circ2d.hxx>
#include <gp_Dir.hxx>
#include <gp_Dir2d.hxx>
#include <gp_Elips2d.hxx>
#include <gp_Lin2d.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>
#include <gp_Pnt2d.hxx>
#include <gp_Vec.hxx>
#include <gp_Vec2d.hxx>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
gp_Pnt2d ToSketchPoint(const OcctSharp_SketchPoint2d& value, const char* message)
{
  if (!std::isfinite(value.x) || !std::isfinite(value.y))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
  return gp_Pnt2d(value.x, value.y);
}

OcctSharp_SketchPoint2d FromSketchPoint(const gp_Pnt2d& value)
{
  return {value.X(), value.Y()};
}

opencascade::handle<Geom2d_Curve> BuildSketchCurve(const OcctSharp_SketchCurve& definition)
{
  if (definition.reversed != 0 && definition.reversed != 1)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The sketch-curve reversed flag must be Boolean.");
  if (!std::isfinite(definition.first_parameter) || !std::isfinite(definition.last_parameter)
      || definition.first_parameter >= definition.last_parameter)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch-curve parameter bounds must be finite and increasing.");

  opencascade::handle<Geom2d_Curve> curve;
  if (definition.kind == 1)
  {
    if (definition.poles == nullptr || definition.pole_count != 2)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A sketch segment requires exactly two points.");
    const gp_Pnt2d first = ToSketchPoint(definition.poles[0], "Sketch-segment points must be finite.");
    const gp_Pnt2d last = ToSketchPoint(definition.poles[1], "Sketch-segment points must be finite.");
    const gp_Vec2d delta(first, last);
    const double length = delta.Magnitude();
    if (length <= Precision::Confusion())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A sketch segment must have non-zero length.");
    curve = new Geom2d_Line(gp_Lin2d(first, gp_Dir2d(delta)));
    curve = new Geom2d_TrimmedCurve(curve, definition.first_parameter, definition.last_parameter, true, false);
  }
  else if (definition.kind == 2 || definition.kind == 3)
  {
    if (definition.poles == nullptr || definition.pole_count != 1
        || !std::isfinite(definition.major_radius) || definition.major_radius <= 0.0
        || !std::isfinite(definition.axis_angle))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A sketch circle or ellipse requires a finite center, axis angle, and positive radius.");
    const gp_Pnt2d center = ToSketchPoint(definition.poles[0], "The sketch conic center must be finite.");
    const gp_Dir2d x_direction(std::cos(definition.axis_angle), std::sin(definition.axis_angle));
    const gp_Ax22d axes(center, x_direction, true);
    if (definition.kind == 2)
    {
      curve = new Geom2d_Circle(gp_Circ2d(axes, definition.major_radius));
    }
    else
    {
      if (!std::isfinite(definition.minor_radius) || definition.minor_radius <= 0.0
          || definition.minor_radius > definition.major_radius)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A sketch ellipse requires positive major/minor radii with major radius not smaller than minor radius.");
      curve = new Geom2d_Ellipse(gp_Elips2d(axes, definition.major_radius, definition.minor_radius));
    }
    curve = new Geom2d_TrimmedCurve(curve, definition.first_parameter, definition.last_parameter);
  }
  else if (definition.kind == 4 || definition.kind == 5)
  {
    if (definition.poles == nullptr || definition.pole_count < 2
        || (definition.rational != 0 && definition.rational != 1)
        || (definition.periodic != 0 && definition.periodic != 1))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A freeform sketch curve requires copied poles and Boolean rational/periodic flags.");
    NCollection_Array1<gp_Pnt2d> poles(1, definition.pole_count);
    NCollection_Array1<double> weights(1, definition.pole_count);
    for (int32_t index = 0; index < definition.pole_count; ++index)
    {
      poles.SetValue(index + 1, ToSketchPoint(definition.poles[index], "Sketch-curve poles must be finite."));
      const double weight = definition.weights == nullptr ? 1.0 : definition.weights[index];
      if (!std::isfinite(weight) || weight <= 0.0)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch-curve weights must be finite and positive.");
      weights.SetValue(index + 1, weight);
    }
    if (definition.kind == 4)
    {
      if (definition.pole_count > Geom2d_BezierCurve::MaxDegree() + 1)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The sketch Bezier degree exceeds the OCCT maximum.");
      curve = definition.rational == 0
        ? opencascade::handle<Geom2d_Curve>(new Geom2d_BezierCurve(poles))
        : opencascade::handle<Geom2d_Curve>(new Geom2d_BezierCurve(poles, weights));
    }
    else
    {
      if (definition.knots == nullptr || definition.multiplicities == nullptr
          || definition.knot_count < 2 || definition.degree < 1
          || definition.degree > Geom2d_BSplineCurve::MaxDegree())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A sketch B-spline requires valid degree, knots, and multiplicities.");
      NCollection_Array1<double> knots(1, definition.knot_count);
      NCollection_Array1<int> multiplicities(1, definition.knot_count);
      for (int32_t index = 0; index < definition.knot_count; ++index)
      {
        if (!std::isfinite(definition.knots[index])
            || (index > 0 && definition.knots[index] <= definition.knots[index - 1]))
          throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch B-spline knots must be finite and strictly increasing.");
        knots.SetValue(index + 1, definition.knots[index]);
        multiplicities.SetValue(index + 1, definition.multiplicities[index]);
      }
      curve = definition.rational == 0
        ? opencascade::handle<Geom2d_Curve>(new Geom2d_BSplineCurve(
            poles, knots, multiplicities, definition.degree, definition.periodic != 0))
        : opencascade::handle<Geom2d_Curve>(new Geom2d_BSplineCurve(
            poles, weights, knots, multiplicities, definition.degree, definition.periodic != 0));
    }
    const double basis_first = curve->FirstParameter();
    const double basis_last = curve->LastParameter();
    if (definition.first_parameter > basis_first + Precision::PConfusion()
        || definition.last_parameter < basis_last - Precision::PConfusion())
      curve = new Geom2d_TrimmedCurve(curve, definition.first_parameter, definition.last_parameter, true, false);
  }
  else
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The sketch-curve kind is unknown.");
  }

  return curve;
}

double SketchBasisParameter(const OcctSharp_SketchCurve& definition, const double parameter)
{
  return definition.reversed == 0
    ? parameter
    : definition.first_parameter + definition.last_parameter - parameter;
}

double SketchResultParameter(const OcctSharp_SketchCurve& definition, double parameter)
{
  // OCCT moves trimmed conic domains into an equivalent positive period. Map
  // solutions back into the copied definition's domain before reversing them.
  if (definition.kind == 2 || definition.kind == 3)
  {
    const double period = 2.0 * std::acos(-1.0);
    parameter += period * std::ceil((definition.first_parameter - parameter - 1e-12) / period);
  }
  parameter = std::clamp(parameter, definition.first_parameter, definition.last_parameter);
  return SketchBasisParameter(definition, parameter);
}

gp_Pln BuildSketchPlane(const OcctSharp_SketchPlane& definition)
{
  const gp_Pnt origin = ToPoint(definition.origin, "The sketch-plane origin must be finite.");
  if (!std::isfinite(definition.x_direction.x) || !std::isfinite(definition.x_direction.y)
      || !std::isfinite(definition.x_direction.z) || !std::isfinite(definition.y_direction.x)
      || !std::isfinite(definition.y_direction.y) || !std::isfinite(definition.y_direction.z))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch-plane axes must be finite.");
  const gp_Vec x_vector(definition.x_direction.x, definition.x_direction.y, definition.x_direction.z);
  const gp_Vec y_vector(definition.y_direction.x, definition.y_direction.y, definition.y_direction.z);
  if (x_vector.SquareMagnitude() <= gp::Resolution() || y_vector.SquareMagnitude() <= gp::Resolution())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch-plane axes must be non-zero.");
  const gp_Vec normal = x_vector.Crossed(y_vector);
  if (normal.SquareMagnitude() <= gp::Resolution())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch-plane axes must be linearly independent.");
  const gp_Dir x_direction(x_vector);
  const gp_Dir y_direction(y_vector);
  if (std::abs(x_direction.Dot(y_direction)) > 1.0e-10)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Sketch-plane axes must be orthogonal.");
  return gp_Pln(gp_Ax3(origin, gp_Dir(normal), x_direction));
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OcctSharp_Internal_BuildSketchCurve(
  const OcctSharp_SketchCurve& definition, opencascade::handle<Geom2d_Curve>& curve)
{
  return Guard([&] { curve = BuildSketchCurve(definition); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_curve_evaluate(
  const OcctSharp_SketchCurve* curve,
  const double parameter,
  OcctSharp_SketchEvaluation* out_evaluation)
{
  if (curve == nullptr || out_evaluation == nullptr)
  {
    SetLastError("A sketch evaluation input or output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_evaluation = {};
  if (!std::isfinite(parameter))
  {
    SetLastError("The sketch evaluation parameter must be finite.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    const opencascade::handle<Geom2d_Curve> native_curve = BuildSketchCurve(*curve);
    gp_Pnt2d point;
    gp_Vec2d derivative;
    native_curve->D1(SketchBasisParameter(*curve, parameter), point, derivative);
    if (curve->reversed != 0) derivative.Reverse();
    *out_evaluation = {
      FromSketchPoint(point),
      {derivative.X(), derivative.Y()},
      parameter
    };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_curve_project(
  const OcctSharp_SketchCurve* curve,
  const OcctSharp_SketchPoint2d point,
  OcctSharp_SketchProjection* results,
  const int32_t capacity,
  int32_t* out_count)
{
  if (curve == nullptr || out_count == nullptr)
  {
    SetLastError("A sketch projection input or count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_count = 0;
  return Guard([&]
  {
    const gp_Pnt2d native_point = ToSketchPoint(point, "The sketch projection point must be finite.");
    const opencascade::handle<Geom2d_Curve> native_curve = BuildSketchCurve(*curve);
    Geom2dAPI_ProjectPointOnCurve projection(
      native_point, native_curve, native_curve->FirstParameter(), native_curve->LastParameter());
    std::vector<OcctSharp_SketchProjection> copied;
    copied.reserve(static_cast<size_t>(projection.NbPoints()));
    for (int32_t index = 1; index <= projection.NbPoints(); ++index)
      copied.push_back({
        FromSketchPoint(projection.Point(index)),
        SketchResultParameter(*curve, projection.Parameter(index)),
        projection.Distance(index)});
    std::stable_sort(copied.begin(), copied.end(), [](const auto& first, const auto& second)
    {
      if (first.distance != second.distance) return first.distance < second.distance;
      return first.parameter < second.parameter;
    });
    *out_count = static_cast<int32_t>(copied.size());
    if (capacity == 0 && results == nullptr) return;
    ValidateOutputCapacity(capacity, *out_count, results, "The sketch projection output buffer is too small.");
    std::copy(copied.begin(), copied.end(), results);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_curve_intersect(
  const OcctSharp_SketchCurve* first,
  const OcctSharp_SketchCurve* second,
  const double tolerance,
  OcctSharp_SketchIntersection* results,
  const int32_t capacity,
  int32_t* out_count)
{
  if (first == nullptr || out_count == nullptr)
  {
    SetLastError("A sketch intersection input or count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_count = 0;
  if (!std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The sketch intersection tolerance must be finite and positive.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    const opencascade::handle<Geom2d_Curve> first_curve = BuildSketchCurve(*first);
    const opencascade::handle<Geom2d_Curve> second_curve = second == nullptr
      ? first_curve : BuildSketchCurve(*second);
    Geom2dAPI_InterCurveCurve intersection = second == nullptr
      ? Geom2dAPI_InterCurveCurve(first_curve, tolerance)
      : Geom2dAPI_InterCurveCurve(first_curve, second_curve, tolerance);
    std::vector<OcctSharp_SketchIntersection> copied;
    copied.reserve(static_cast<size_t>(intersection.NbPoints()));
    const auto append_point = [&](const IntRes2d_IntersectionPoint& native_point)
    {
      copied.push_back({
        FromSketchPoint(native_point.Value()),
        SketchResultParameter(*first, native_point.ParamOnFirst()),
        SketchResultParameter(second == nullptr ? *first : *second, native_point.ParamOnSecond())
      });
    };
    for (int32_t index = 1; index <= intersection.NbPoints(); ++index)
      append_point(intersection.Intersector().Point(index));
    // Tangential and coincident spans have boundary solutions too. Ignoring them
    // would let overlapping curves or touching profile holes pass validation.
    for (int32_t index = 1; index <= intersection.NbSegments(); ++index)
    {
      const auto& segment = intersection.Intersector().Segment(index);
      if (segment.HasFirstPoint()) append_point(segment.FirstPoint());
      if (segment.HasLastPoint()) append_point(segment.LastPoint());
    }
    std::stable_sort(copied.begin(), copied.end(), [](const auto& left, const auto& right)
    {
      if (left.first_parameter != right.first_parameter)
        return left.first_parameter < right.first_parameter;
      return left.second_parameter < right.second_parameter;
    });
    *out_count = static_cast<int32_t>(copied.size());
    if (capacity == 0 && results == nullptr) return;
    ValidateOutputCapacity(capacity, *out_count, results, "The sketch intersection output buffer is too small.");
    std::copy(copied.begin(), copied.end(), results);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_curve_make_edge(
  const OcctSharp_SketchCurve* curve,
  const OcctSharp_SketchPlane* plane,
  OcctSharp_ShapeHandle** out_shape)
{
  if (curve == nullptr || plane == nullptr || out_shape == nullptr)
  {
    SetLastError("A sketch edge input or output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    const opencascade::handle<Geom_Curve> curve3d = GeomAPI::To3d(BuildSketchCurve(*curve), BuildSketchPlane(*plane));
    if (curve3d.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not lift the sketch curve into its plane.");
    BRepBuilderAPI_MakeEdge builder(curve3d);
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build an edge from the sketch curve.");
    TopoDS_Edge edge = builder.Edge();
    if (curve->reversed != 0) edge.Reverse();
    *out_shape = AllocateShape(std::move(edge));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_profile_make_face(
  const OcctSharp_ShapeHandle* outer_wire,
  const OcctSharp_ShapeHandle* const* inner_wires,
  const int32_t inner_wire_count,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The sketch face output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  if (inner_wire_count < 0 || (inner_wire_count > 0 && inner_wires == nullptr))
  {
    SetLastError("The sketch inner-wire array is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(outer_wire);
    if (outer_wire->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "The sketch outer loop must be a wire.");
    BRepBuilderAPI_MakeFace builder(TopoDS::Wire(outer_wire->Value));
    for (int32_t index = 0; index < inner_wire_count; ++index)
    {
      ValidateUsableShape(inner_wires[index]);
      if (inner_wires[index]->Value.ShapeType() != TopAbs_WIRE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Every sketch inner loop must be a wire.");
      builder.Add(TopoDS::Wire(inner_wires[index]->Value));
    }
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the sketch face with holes.");
    const TopoDS_Face face = builder.Face();
    if (!BRepCheck_Analyzer(face).IsValid())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The sketch face contains invalid or intersecting boundaries.");
    *out_shape = AllocateShape(face);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_make_wire(
  const OcctSharp_ShapeHandle* const* edges,
  const int32_t edge_count,
  const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr || edges == nullptr || edge_count <= 0
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The sketch wire inputs, output, or tolerance are invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    BRepBuilderAPI_MakeWire builder;
    BRep_Builder topology;
    for (int32_t index = 0; index < edge_count; ++index)
    {
      ValidateUsableShape(edges[index]);
      if (edges[index]->Value.ShapeType() != TopAbs_EDGE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "A sketch wire input must be an edge.");
      // Vertex tolerances belong to copied topology, never to a caller's input.
      BRepBuilderAPI_Copy copy(edges[index]->Value, false, false);
      const TopoDS_Edge edge = TopoDS::Edge(copy.Shape());
      for (TopExp_Explorer vertex(edge, TopAbs_VERTEX); vertex.More(); vertex.Next())
        topology.UpdateVertex(TopoDS::Vertex(vertex.Current()), tolerance);
      builder.Add(edge);
      if (!builder.IsDone())
        throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The sketch edges cannot be connected within tolerance.");
    }
    *out_shape = AllocateShape(builder.Wire());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_sketch_wire_contains(
  const OcctSharp_ShapeHandle* wire,
  const OcctSharp_SketchPoint2d point,
  const double tolerance,
  int32_t* out_inside)
{
  if (out_inside == nullptr || !std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The sketch containment output or tolerance is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_inside = 0;
  return Guard([&]
  {
    ValidateUsableShape(wire);
    if (wire->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Sketch containment requires a wire in the XY plane.");
    const gp_Pnt2d local = ToSketchPoint(point, "The sketch containment point must be finite.");
    BRepBuilderAPI_MakeFace builder(gp_Pln(gp::XOY()), TopoDS::Wire(wire->Value), true);
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not build the containment face.");
    BRepClass_FaceClassifier classifier(builder.Face(), gp_Pnt(local.X(), local.Y(), 0.0), tolerance);
    *out_inside = classifier.State() == TopAbs_IN ? 1 : 0;
  });
}

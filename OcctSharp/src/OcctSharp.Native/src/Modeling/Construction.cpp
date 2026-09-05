// Native Modeling/Construction implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <BRepBuilderAPI_MakeFace.hxx>
#include <BRepBuilderAPI_MakePolygon.hxx>
#include <BRepBuilderAPI_MakeWire.hxx>
#include <BRepBuilderAPI_Sewing.hxx>
#include <BRepOffsetAPI_MakePipe.hxx>
#include <BRepOffsetAPI_ThruSections.hxx>
#include <BRepPrimAPI_MakeBox.hxx>
#include <BRepPrimAPI_MakeCone.hxx>
#include <BRepPrimAPI_MakeCylinder.hxx>
#include <BRepPrimAPI_MakeSphere.hxx>
#include <BRepPrimAPI_MakeTorus.hxx>
#include <BRepPrimAPI_MakeWedge.hxx>
#include <BRep_Builder.hxx>
#include <GC_MakeArcOfCircle.hxx>
#include <GeomAPI_Interpolate.hxx>
#include <Geom_BezierCurve.hxx>
#include <NCollection_Array1.hxx>
#include <NCollection_HArray1.hxx>
#include <Standard_Handle.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Shape.hxx>
#include <cmath>
#include <gp_Ax2.hxx>
#include <gp_Circ.hxx>
#include <gp_Dir.hxx>
#include <gp_Elips.hxx>
#include <gp_Pnt.hxx>
#include <utility>

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_box(
  const double size_x,
  const double size_y,
  const double size_z,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;

  if (!std::isfinite(size_x) || !std::isfinite(size_y) || !std::isfinite(size_z)
      || size_x <= 0.0 || size_y <= 0.0 || size_z <= 0.0)
  {
    SetLastError("Box dimensions must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    TopoDS_Shape shape = BRepPrimAPI_MakeBox(size_x, size_y, size_z).Shape();
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_null(
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The null shape output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&] { *out_shape = AllocateShape(TopoDS_Shape()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_sphere(
  const double radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The output shape pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0) { SetLastError("Sphere radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { *out_shape = AllocateShape(BRepPrimAPI_MakeSphere(radius).Shape()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_cylinder(
  const double radius, const double height, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The output shape pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || !std::isfinite(height) || radius <= 0.0 || height <= 0.0)
  { SetLastError("Cylinder radius and height must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&] { *out_shape = AllocateShape(BRepPrimAPI_MakeCylinder(radius, height).Shape()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_cone(
  const double bottom_radius, const double top_radius, const double height,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The cone output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(bottom_radius) || !std::isfinite(top_radius) || !std::isfinite(height)
      || bottom_radius < 0.0 || top_radius < 0.0 || height <= 0.0
      || (bottom_radius == 0.0 && top_radius == 0.0) || bottom_radius == top_radius)
  {
    SetLastError("Cone radii must be finite, non-negative, different, and not both zero; height must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    BRepPrimAPI_MakeCone builder(bottom_radius, top_radius, height);
    TopoDS_Shape result = builder.Shape();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT cone construction did not complete.");
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT cone construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_torus(
  const double major_radius, const double minor_radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The torus output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(major_radius) || !std::isfinite(minor_radius)
      || major_radius <= 0.0 || minor_radius <= 0.0 || major_radius <= minor_radius)
  {
    SetLastError("Torus radii must be finite and greater than zero, and the major radius must exceed the minor radius.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    BRepPrimAPI_MakeTorus builder(major_radius, minor_radius);
    TopoDS_Shape result = builder.Shape();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT torus construction did not complete.");
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT torus construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_wedge(
  const double size_x, const double size_y, const double size_z, const double top_x_length,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The wedge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(size_x) || !std::isfinite(size_y) || !std::isfinite(size_z)
      || !std::isfinite(top_x_length) || size_x <= 0.0 || size_y <= 0.0 || size_z <= 0.0
      || top_x_length < 0.0)
  {
    SetLastError("Wedge dimensions must be finite and greater than zero, and the top X length must be finite and non-negative.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    BRepPrimAPI_MakeWedge builder(size_x, size_y, size_z, top_x_length);
    builder.Build();
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT wedge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_edge(
  const OcctSharp_Xyz start, const OcctSharp_Xyz end, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateFinite(start.x, "Edge start X must be finite."); ValidateFinite(start.y, "Edge start Y must be finite."); ValidateFinite(start.z, "Edge start Z must be finite.");
    ValidateFinite(end.x, "Edge end X must be finite."); ValidateFinite(end.y, "Edge end Y must be finite."); ValidateFinite(end.z, "Edge end Z must be finite.");
    if (start.x == end.x && start.y == end.y && start.z == end.z)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Edge endpoints must be distinct.");
    BRepBuilderAPI_MakeEdge builder(gp_Pnt(start.x, start.y, start.z), gp_Pnt(end.x, end.y, end.z));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_circle_edge(
  const OcctSharp_Xyz center, const OcctSharp_Xyz normal, const double radius,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The circle edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0)
  { SetLastError("The circle radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateFinite(center.x, "Circle center X must be finite."); ValidateFinite(center.y, "Circle center Y must be finite."); ValidateFinite(center.z, "Circle center Z must be finite.");
    ValidateFinite(normal.x, "Circle normal X must be finite."); ValidateFinite(normal.y, "Circle normal Y must be finite."); ValidateFinite(normal.z, "Circle normal Z must be finite.");
    BRepBuilderAPI_MakeEdge builder(gp_Circ(gp_Ax2(gp_Pnt(center.x, center.y, center.z), gp_Dir(normal.x, normal.y, normal.z)), radius));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT circle edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_arc_edge(
  const OcctSharp_Xyz start, const OcctSharp_Xyz middle, const OcctSharp_Xyz end,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The arc edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateFinite(start.x, "Arc start X must be finite."); ValidateFinite(start.y, "Arc start Y must be finite."); ValidateFinite(start.z, "Arc start Z must be finite.");
    ValidateFinite(middle.x, "Arc middle X must be finite."); ValidateFinite(middle.y, "Arc middle Y must be finite."); ValidateFinite(middle.z, "Arc middle Z must be finite.");
    ValidateFinite(end.x, "Arc end X must be finite."); ValidateFinite(end.y, "Arc end Y must be finite."); ValidateFinite(end.z, "Arc end Z must be finite.");
    GC_MakeArcOfCircle arc(gp_Pnt(start.x, start.y, start.z), gp_Pnt(middle.x, middle.y, middle.z), gp_Pnt(end.x, end.y, end.z));
    if (!arc.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT circular arc construction did not complete.");
    BRepBuilderAPI_MakeEdge builder(arc.Value());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT arc edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_ellipse_edge(
  const OcctSharp_Xyz center, const OcctSharp_Xyz normal, const OcctSharp_Xyz x_direction,
  const double major_radius, const double minor_radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The ellipse edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(major_radius) || !std::isfinite(minor_radius)
      || major_radius <= 0.0 || minor_radius <= 0.0 || major_radius < minor_radius)
  { SetLastError("Ellipse radii must be finite and positive, with major radius greater than or equal to minor radius."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateFinite(center.x, "Ellipse center X must be finite."); ValidateFinite(center.y, "Ellipse center Y must be finite."); ValidateFinite(center.z, "Ellipse center Z must be finite.");
    ValidateFinite(normal.x, "Ellipse normal X must be finite."); ValidateFinite(normal.y, "Ellipse normal Y must be finite."); ValidateFinite(normal.z, "Ellipse normal Z must be finite.");
    ValidateFinite(x_direction.x, "Ellipse X direction X must be finite."); ValidateFinite(x_direction.y, "Ellipse X direction Y must be finite."); ValidateFinite(x_direction.z, "Ellipse X direction Z must be finite.");
    const gp_Ax2 axis(gp_Pnt(center.x, center.y, center.z), gp_Dir(normal.x, normal.y, normal.z), gp_Dir(x_direction.x, x_direction.y, x_direction.z));
    BRepBuilderAPI_MakeEdge builder(gp_Elips(axis, major_radius, minor_radius));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT ellipse edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_bezier_edge(
  const OcctSharp_Xyz* poles, const int32_t count, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The Bezier edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (poles == nullptr || count < 2) { SetLastError("A Bezier edge requires at least two poles."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    NCollection_Array1<gp_Pnt> native_poles(1, count);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateFinite(poles[index].x, "Bezier pole X must be finite."); ValidateFinite(poles[index].y, "Bezier pole Y must be finite."); ValidateFinite(poles[index].z, "Bezier pole Z must be finite.");
      native_poles.SetValue(index + 1, gp_Pnt(poles[index].x, poles[index].y, poles[index].z));
    }
    const opencascade::handle<Geom_BezierCurve> curve = new Geom_BezierCurve(native_poles);
    BRepBuilderAPI_MakeEdge builder(curve);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT Bezier edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_interpolated_edge(
  const OcctSharp_Xyz* points, const int32_t count, const int32_t periodic, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The interpolated edge output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (points == nullptr || count < 2 || (periodic != 0 && count < 3))
  { SetLastError("Interpolation requires at least two points, or three for a periodic curve."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if ((periodic != 0 && periodic != 1) || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("The periodic flag must be zero or one and tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const opencascade::handle<NCollection_HArray1<gp_Pnt>> native_points =
      new NCollection_HArray1<gp_Pnt>(1, count);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateFinite(points[index].x, "Interpolation point X must be finite."); ValidateFinite(points[index].y, "Interpolation point Y must be finite."); ValidateFinite(points[index].z, "Interpolation point Z must be finite.");
      native_points->SetValue(index + 1, gp_Pnt(points[index].x, points[index].y, points[index].z));
    }
    GeomAPI_Interpolate interpolation(native_points, periodic != 0, tolerance);
    interpolation.Perform();
    if (!interpolation.IsDone() || interpolation.Curve().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT curve interpolation did not complete.");
    BRepBuilderAPI_MakeEdge builder(interpolation.Curve());
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT interpolated edge construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_loft(
  const OcctSharp_ShapeHandle* const* sections, const int32_t count,
  const int32_t make_solid, const int32_t ruled, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The loft output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (sections == nullptr || count < 2)
  { SetLastError("A loft requires at least two wire or endpoint-vertex sections."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if ((make_solid != 0 && make_solid != 1) || (ruled != 0 && ruled != 1)
      || !std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Loft flags must be zero or one and tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepOffsetAPI_ThruSections builder(make_solid != 0, ruled != 0, tolerance);
    builder.CheckCompatibility(true);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateUsableShape(sections[index]);
      const TopAbs_ShapeEnum kind = sections[index]->Value.ShapeType();
      if (kind == TopAbs_WIRE) builder.AddWire(TopoDS::Wire(sections[index]->Value));
      else if (kind == TopAbs_VERTEX && (index == 0 || index == count - 1))
        builder.AddVertex(TopoDS::Vertex(sections[index]->Value));
      else
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Loft sections must be wires; only the first or last section may be a vertex.");
    }
    builder.Build();
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT loft construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_pipe(
  const OcctSharp_ShapeHandle* spine, const OcctSharp_ShapeHandle* profile,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The pipe output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(spine);
    ValidateUsableShape(profile);
    if (spine->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Pipe construction requires a wire spine.");
    BRepOffsetAPI_MakePipe builder(TopoDS::Wire(spine->Value), profile->Value);
    builder.Build();
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT pipe construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_sew(
  const OcctSharp_ShapeHandle* const* shapes, const int32_t count, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The sewing output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (shapes == nullptr || count < 1)
  { SetLastError("Sewing requires at least one topology shape."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (!std::isfinite(tolerance) || tolerance <= 0.0)
  { SetLastError("Sewing tolerance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepBuilderAPI_Sewing builder(tolerance, true, true, true, false);
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateUsableShape(shapes[index]);
      builder.Add(shapes[index]->Value);
    }
    builder.Perform();
    TopoDS_Shape result = builder.SewedShape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT sewing produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_polygon_wire(
  const OcctSharp_Xyz* points, const int32_t count, const int32_t close,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The wire output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (count < 2 || points == nullptr) { SetLastError("A polygon wire requires at least two points."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepBuilderAPI_MakePolygon builder;
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateFinite(points[index].x, "Wire point X must be finite.");
      ValidateFinite(points[index].y, "Wire point Y must be finite.");
      ValidateFinite(points[index].z, "Wire point Z must be finite.");
      builder.Add(gp_Pnt(points[index].x, points[index].y, points[index].z));
    }
    if (close != 0) builder.Close();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT polygon wire construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_wire(
  const OcctSharp_ShapeHandle* const* edges, const int32_t count,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The wire output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (count <= 0 || edges == nullptr)
  { SetLastError("Wire construction requires at least one edge handle."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    BRepBuilderAPI_MakeWire builder;
    for (int32_t index = 0; index < count; ++index)
    {
      ValidateUsableShape(edges[index]);
      if (edges[index]->Value.ShapeType() != TopAbs_EDGE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Wire construction accepts edge shapes only.");
      builder.Add(TopoDS::Edge(edges[index]->Value));
    }
    if (!builder.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not connect the supplied edges into a wire.");
    *out_shape = AllocateShape(builder.Wire());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_planar_face(
  const OcctSharp_ShapeHandle* wire, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The face output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(wire);
    if (wire->Value.ShapeType() != TopAbs_WIRE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Planar face construction requires a wire shape.");
    BRepBuilderAPI_MakeFace builder(TopoDS::Wire(wire->Value));
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT planar face construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_create_compound(OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  *out_shape = nullptr;
  return Guard([&]
  {
    TopoDS_Compound compound;
    BRep_Builder builder;
    builder.MakeCompound(compound);
    *out_shape = AllocateShape(std::move(compound));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_compound_add(
  OcctSharp_ShapeHandle* compound,
  const OcctSharp_ShapeHandle* child)
{
  return Guard([&]
  {
    ValidateShape(compound);
    ValidateShape(child);
    if (compound->Value.ShapeType() != TopAbs_COMPOUND)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The target shape is not a compound.");
    }

    TopoDS_Compound target = TopoDS::Compound(compound->Value);
    BRep_Builder builder;
    builder.Add(target, child->Value);
    compound->Value = target;
  });
}

// Native Modeling/Drawing implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "Modeling/Drawing.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepAdaptor_Curve.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepMesh_IncrementalMesh.hxx>
#include <BRep_Builder.hxx>
#include <HLRAlgo_Projector.hxx>
#include <HLRBRep_Algo.hxx>
#include <HLRBRep_HLRToShape.hxx>
#include <HLRBRep_PolyAlgo.hxx>
#include <HLRBRep_PolyHLRToShape.hxx>
#include <Precision.hxx>
#include <Standard_Handle.hxx>
#include <TopExp_Explorer.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Compound.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <gp.hxx>
#include <gp_Ax2.hxx>
#include <gp_Dir.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>
#include <limits>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
TopoDS_Shape NonNullDrawingLayer(TopoDS_Shape shape)
{
  if (!shape.IsNull()) return shape;
  TopoDS_Compound compound;
  BRep_Builder builder;
  builder.MakeCompound(compound);
  return compound;
}

HLRAlgo_Projector MakeDrawingProjector(const OcctSharp_DrawingProjection& projection)
{
  const gp_Pnt origin = ToPoint(projection.origin, "Drawing projection origin must be finite.");
  const gp_Vec view = ToVector(projection.view_direction, "Drawing view direction must be finite and non-zero.");
  const gp_Vec up = ToVector(projection.up_direction, "Drawing up direction must be finite and non-zero.");
  gp_Vec right = up.Crossed(view);
  if (right.SquareMagnitude() <= gp::Resolution())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing up and view directions must not be parallel.");
  if ((projection.perspective != 0 && projection.perspective != 1)
      || !std::isfinite(projection.focus) || projection.focus <= 0.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing perspective flag or focus is invalid.");
  const gp_Ax2 coordinate_system(origin, gp_Dir(view), gp_Dir(right));
  return projection.perspective == 0
    ? HLRAlgo_Projector(coordinate_system)
    : HLRAlgo_Projector(coordinate_system, projection.focus);
}

DrawingPolylineData BuildDrawingPolylines(const TopoDS_Shape& shape, const int32_t samples_per_curve)
{
  if (samples_per_curve < 2 || samples_per_curve > 4096)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing curve samples must be between 2 and 4096.");

  DrawingPolylineData data;
  for (TopExp_Explorer explorer(shape, TopAbs_EDGE); explorer.More(); explorer.Next())
  {
    const TopoDS_Edge edge = TopoDS::Edge(explorer.Current());
    BRepAdaptor_Curve curve(edge);
    const double first = curve.FirstParameter();
    const double last = curve.LastParameter();
    if (!std::isfinite(first) || !std::isfinite(last) || last < first) continue;
    const int32_t count = curve.GetType() == GeomAbs_Line ? 2 : samples_per_curve;
    if (data.Points.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max() - count))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing polyline point count exceeds the 32-bit ABI.");
    const int32_t offset = static_cast<int32_t>(data.Points.size());
    for (int32_t index = 0; index < count; ++index)
    {
      const double parameter = count == 1 ? first : first + (last - first) * index / (count - 1);
      data.Points.push_back(FromPoint(curve.Value(parameter)));
    }
    const bool closed = count > 2
      && curve.Value(first).SquareDistance(curve.Value(last)) <= Precision::SquareConfusion();
    data.Polylines.push_back({offset, count, closed ? 1 : 0});
  }
  return data;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_compute(
  const OcctSharp_ShapeHandle* const* shapes, const int32_t shape_count,
  const OcctSharp_DrawingProjection projection, const int32_t exact, const int32_t iso_count,
  const double deflection, OcctSharp_ShapeHandle** out_layers, const int32_t layer_capacity)
{
  if (shapes == nullptr || shape_count <= 0 || out_layers == nullptr || layer_capacity != 10)
  {
    SetLastError("Drawing computation requires at least one shape and exactly ten output layers.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  for (int32_t index = 0; index < layer_capacity; ++index) out_layers[index] = nullptr;
  if ((exact != 0 && exact != 1) || iso_count < 0 || iso_count > 100
      || !std::isfinite(deflection) || deflection <= 0.0)
  {
    SetLastError("Drawing exact flag, isoparameter count, or deflection is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    const HLRAlgo_Projector projector = MakeDrawingProjector(projection);
    std::vector<TopoDS_Shape> layers(10);
    if (exact != 0)
    {
      opencascade::handle<HLRBRep_Algo> algorithm = new HLRBRep_Algo();
      for (int32_t index = 0; index < shape_count; ++index)
      {
        ValidateUsableShape(shapes[index]);
        algorithm->Add(shapes[index]->Value, iso_count);
      }
      algorithm->Projector(projector);
      algorithm->Update();
      algorithm->Hide();
      HLRBRep_HLRToShape extraction(algorithm);
      layers[0] = extraction.VCompound();
      layers[1] = extraction.HCompound();
      layers[2] = extraction.Rg1LineVCompound();
      layers[3] = extraction.Rg1LineHCompound();
      layers[4] = extraction.RgNLineVCompound();
      layers[5] = extraction.RgNLineHCompound();
      layers[6] = extraction.OutLineVCompound();
      layers[7] = extraction.OutLineHCompound();
      layers[8] = extraction.IsoLineVCompound();
      layers[9] = extraction.IsoLineHCompound();
    }
    else
    {
      opencascade::handle<HLRBRep_PolyAlgo> algorithm = new HLRBRep_PolyAlgo();
      for (int32_t index = 0; index < shape_count; ++index)
      {
        ValidateUsableShape(shapes[index]);
        BRepMesh_IncrementalMesh mesh(shapes[index]->Value, deflection, false, 0.5, true);
        algorithm->Load(shapes[index]->Value);
      }
      algorithm->Projector(projector);
      algorithm->Update();
      HLRBRep_PolyHLRToShape extraction;
      extraction.Update(algorithm);
      layers[0] = extraction.VCompound();
      layers[1] = extraction.HCompound();
      layers[2] = extraction.Rg1LineVCompound();
      layers[3] = extraction.Rg1LineHCompound();
      layers[4] = extraction.RgNLineVCompound();
      layers[5] = extraction.RgNLineHCompound();
      layers[6] = extraction.OutLineVCompound();
      layers[7] = extraction.OutLineHCompound();
    }

    try
    {
      for (int32_t index = 0; index < layer_capacity; ++index)
        out_layers[index] = AllocateShape(NonNullDrawingLayer(std::move(layers[index])));
    }
    catch (...)
    {
      for (int32_t index = 0; index < layer_capacity; ++index)
      {
        if (out_layers[index] == nullptr) continue;
        UnregisterShape(out_layers[index]);
        delete out_layers[index];
        out_layers[index] = nullptr;
      }
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_section(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_Xyz plane_origin,
  const OcctSharp_Xyz plane_normal, const int32_t approximate, OcctSharp_ShapeHandle** out_section)
{
  if (out_section == nullptr)
  {
    SetLastError("The drawing section output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_section = nullptr;
  if (approximate != 0 && approximate != 1)
  {
    SetLastError("The drawing section approximation flag is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const gp_Pln plane(ToPoint(plane_origin, "Section plane origin must be finite."),
                       gp_Dir(ToVector(plane_normal, "Section plane normal must be finite and non-zero.")));
    BRepAlgoAPI_Section section(shape->Value, plane, false);
    section.Approximation(approximate != 0);
    section.Build();
    if (!section.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT section computation did not complete.");
    *out_section = AllocateShape(NonNullDrawingLayer(section.Shape()));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_polyline_count(
  const OcctSharp_ShapeHandle* shape, const int32_t samples_per_curve,
  int32_t* out_polyline_count, int32_t* out_point_count)
{
  if (out_polyline_count == nullptr || out_point_count == nullptr)
  {
    SetLastError("Drawing polyline count output pointers are null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_polyline_count = 0;
  *out_point_count = 0;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const DrawingPolylineData data = BuildDrawingPolylines(shape->Value, samples_per_curve);
    if (data.Polylines.size() > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Drawing polyline count exceeds the 32-bit ABI.");
    *out_polyline_count = static_cast<int32_t>(data.Polylines.size());
    *out_point_count = static_cast<int32_t>(data.Points.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_drawing_polyline_copy(
  const OcctSharp_ShapeHandle* shape, const int32_t samples_per_curve,
  OcctSharp_DrawingPolyline* polylines, const int32_t polyline_capacity,
  OcctSharp_Xyz* points, const int32_t point_capacity,
  int32_t* out_polylines_written, int32_t* out_points_written)
{
  if (out_polylines_written == nullptr || out_points_written == nullptr)
  {
    SetLastError("Drawing polyline copy output pointers are null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_polylines_written = 0;
  *out_points_written = 0;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const DrawingPolylineData data = BuildDrawingPolylines(shape->Value, samples_per_curve);
    ValidateOutputCapacity(polyline_capacity, static_cast<int32_t>(data.Polylines.size()), polylines,
      "The drawing polyline output buffer is too small.");
    ValidateOutputCapacity(point_capacity, static_cast<int32_t>(data.Points.size()), points,
      "The drawing point output buffer is too small.");
    std::copy(data.Polylines.begin(), data.Polylines.end(), polylines);
    std::copy(data.Points.begin(), data.Points.end(), points);
    *out_polylines_written = static_cast<int32_t>(data.Polylines.size());
    *out_points_written = static_cast<int32_t>(data.Points.size());
  });
}

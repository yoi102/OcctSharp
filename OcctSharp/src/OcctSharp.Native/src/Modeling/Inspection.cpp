// Native Modeling/Inspection implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "Modeling/Inspection.hxx"
#include "Modeling/Topology.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepAdaptor_Curve.hxx>
#include <BRepAdaptor_Surface.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepBndLib.hxx>
#include <BRepExtrema_DistShapeShape.hxx>
#include <BRepGProp.hxx>
#include <Bnd_Box.hxx>
#include <Bnd_OBB.hxx>
#include <GProp_GProps.hxx>
#include <NCollection_List.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <gp_Dir.hxx>
#include <gp_Mat.hxx>
#include <gp_Pnt.hxx>
#include <vector>

namespace OcctSharp::Native
{
BRepExtrema_DistShapeShape ComputeExactDistance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second)
{
  ValidateUsableShape(first);
  ValidateUsableShape(second);
  BRepExtrema_DistShapeShape operation(first->Value, second->Value);
  if (!operation.IsDone() || operation.NbSolution() <= 0)
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT exact distance did not produce a solution.");
  return operation;
}

void CopyInertia(const gp_Mat& matrix, OcctSharp_InspectionProperties& properties)
{
  properties.i11 = matrix.Value(1, 1);
  properties.i12 = matrix.Value(1, 2);
  properties.i13 = matrix.Value(1, 3);
  properties.i21 = matrix.Value(2, 1);
  properties.i22 = matrix.Value(2, 2);
  properties.i23 = matrix.Value(2, 3);
  properties.i31 = matrix.Value(3, 1);
  properties.i32 = matrix.Value(3, 2);
  properties.i33 = matrix.Value(3, 3);
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_bounding_box(
  const OcctSharp_ShapeHandle* shape, OcctSharp_BoundingBox* out_bounds)
{
  if (out_bounds == nullptr) { SetLastError("The bounding-box output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_bounds = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    Bnd_Box box;
    BRepBndLib::AddOptimal(shape->Value, box, false, true);
    if (box.IsVoid() || box.IsOpen())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT did not produce finite shape bounds.");
    box.Get(out_bounds->min_x, out_bounds->min_y, out_bounds->min_z,
            out_bounds->max_x, out_bounds->max_y, out_bounds->max_z);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_oriented_bounding_box(
  const OcctSharp_ShapeHandle* shape, OcctSharp_OrientedBoundingBox* out_bounds)
{
  if (out_bounds == nullptr)
  {
    SetLastError("The oriented-bounding-box output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_bounds = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    Bnd_OBB box;
    BRepBndLib::AddOBB(shape->Value, box, true, true, false);
    if (box.IsVoid())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT did not produce finite oriented shape bounds.");
    const gp_Pnt center = box.Center();
    const gp_Dir xDirection = box.XDirection();
    const gp_Dir yDirection = box.YDirection();
    const gp_Dir zDirection = box.ZDirection();
    out_bounds->center = {center.X(), center.Y(), center.Z()};
    out_bounds->x_direction = {xDirection.X(), xDirection.Y(), xDirection.Z()};
    out_bounds->y_direction = {yDirection.X(), yDirection.Y(), yDirection.Z()};
    out_bounds->z_direction = {zDirection.X(), zDirection.Y(), zDirection.Z()};
    out_bounds->half_size_x = box.XHSize();
    out_bounds->half_size_y = box.YHSize();
    out_bounds->half_size_z = box.ZHSize();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_digital_mockup_candidate_pairs(
  const OcctSharp_ShapeHandle* const* shapes, const int32_t shape_count,
  const double expansion, int32_t* pairs, const int32_t pair_capacity,
  int32_t* out_pair_count, int32_t* out_axis_comparison_count)
{
  if (out_pair_count == nullptr || out_axis_comparison_count == nullptr)
  {
    SetLastError("A digital mock-up candidate output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_pair_count = 0;
  *out_axis_comparison_count = 0;
  if (shape_count < 0 || pair_capacity < 0 || !std::isfinite(expansion) || expansion < 0.0
      || (shape_count > 0 && shapes == nullptr) || (pair_capacity > 0 && pairs == nullptr))
  {
    SetLastError("Digital mock-up candidate arguments are invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    struct IndexedBounds
    {
      int32_t Index;
      double MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
    };
    std::vector<IndexedBounds> bounds;
    bounds.reserve(static_cast<size_t>(shape_count));
    for (int32_t index = 0; index < shape_count; ++index)
    {
      ValidateUsableShape(shapes[index]);
      Bnd_Box box;
      BRepBndLib::AddOptimal(shapes[index]->Value, box, false, true);
      if (box.IsVoid() || box.IsOpen())
        throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT did not produce finite candidate bounds.");
      IndexedBounds item{};
      item.Index = index;
      box.Get(item.MinX, item.MinY, item.MinZ, item.MaxX, item.MaxY, item.MaxZ);
      bounds.push_back(item);
    }
    std::sort(bounds.begin(), bounds.end(), [](const IndexedBounds& left, const IndexedBounds& right)
    {
      if (left.MinX != right.MinX) return left.MinX < right.MinX;
      return left.Index < right.Index;
    });
    int32_t pairCount = 0;
    int32_t comparisons = 0;
    for (size_t firstIndex = 0; firstIndex < bounds.size(); ++firstIndex)
    {
      const IndexedBounds& first = bounds[firstIndex];
      for (size_t secondIndex = firstIndex + 1; secondIndex < bounds.size(); ++secondIndex)
      {
        const IndexedBounds& second = bounds[secondIndex];
        ++comparisons;
        if (second.MinX > first.MaxX + expansion) break;
        if (second.MinY > first.MaxY + expansion || first.MinY > second.MaxY + expansion
            || second.MinZ > first.MaxZ + expansion || first.MinZ > second.MaxZ + expansion)
          continue;
        if (pairCount < pair_capacity)
        {
          const int32_t low = std::min(first.Index, second.Index);
          const int32_t high = std::max(first.Index, second.Index);
          pairs[pairCount * 2] = low;
          pairs[pairCount * 2 + 1] = high;
        }
        ++pairCount;
      }
    }
    *out_pair_count = pairCount;
    *out_axis_comparison_count = comparisons;
    if (pair_capacity != 0 && pair_capacity < pairCount)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The digital mock-up candidate buffer is too small.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_distance(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  OcctSharp_ShapeDistanceResult* out_result)
{
  if (out_result == nullptr) { SetLastError("The shape distance output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_result = {};
  return Guard([&]
  {
    ValidateUsableShape(first); ValidateUsableShape(second);
    BRepExtrema_DistShapeShape operation(first->Value, second->Value);
    if (!operation.IsDone() || operation.NbSolution() <= 0)
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT shape distance did not produce a solution.");

    const gp_Pnt point1 = operation.PointOnShape1(1);
    const gp_Pnt point2 = operation.PointOnShape2(1);
    *out_result = {
      operation.Value(),
      { point1.X(), point1.Y(), point1.Z() },
      { point2.X(), point2.Y(), point2.Z() },
      operation.NbSolution()
    };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_exact_distance_count(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, int32_t* count)
{
  if (count == nullptr) { SetLastError("The exact-distance count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&]
  {
    const BRepExtrema_DistShapeShape operation = ComputeExactDistance(first, second);
    *count = operation.NbSolution();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_exact_distance_solution(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  const int32_t index, OcctSharp_ExtremaSolution* solution)
{
  if (solution == nullptr) { SetLastError("The exact-distance solution pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *solution = {};
  return Guard([&]
  {
    const BRepExtrema_DistShapeShape operation = ComputeExactDistance(first, second);
    if (index < 1 || index > operation.NbSolution())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The exact-distance solution index is outside the valid 1-based range.");

    const BRepExtrema_SupportType firstKind = operation.SupportTypeShape1(index);
    const BRepExtrema_SupportType secondKind = operation.SupportTypeShape2(index);
    OcctSharp_ShapeHandle* firstSupport = nullptr;
    OcctSharp_ShapeHandle* secondSupport = nullptr;
    try
    {
      firstSupport = AllocateShape(operation.SupportOnShape1(index));
      secondSupport = AllocateShape(operation.SupportOnShape2(index));
      solution->distance = operation.Value();
      solution->point_on_first = CopyPoint(operation.PointOnShape1(index));
      solution->point_on_second = CopyPoint(operation.PointOnShape2(index));
      solution->first_support_kind = static_cast<int32_t>(firstKind);
      solution->second_support_kind = static_cast<int32_t>(secondKind);
      solution->is_inner_solution = operation.InnerSolution() ? 1 : 0;
      if (firstKind == BRepExtrema_IsOnEdge)
      {
        operation.ParOnEdgeS1(index, solution->first_edge_parameter);
        solution->has_first_edge_parameter = 1;
      }
      else if (firstKind == BRepExtrema_IsInFace)
      {
        operation.ParOnFaceS1(index, solution->first_face_u, solution->first_face_v);
        solution->has_first_face_parameters = 1;
      }
      if (secondKind == BRepExtrema_IsOnEdge)
      {
        operation.ParOnEdgeS2(index, solution->second_edge_parameter);
        solution->has_second_edge_parameter = 1;
      }
      else if (secondKind == BRepExtrema_IsInFace)
      {
        operation.ParOnFaceS2(index, solution->second_face_u, solution->second_face_v);
        solution->has_second_face_parameters = 1;
      }
      solution->first_support = firstSupport;
      solution->second_support = secondSupport;
      firstSupport = nullptr;
      secondSupport = nullptr;
    }
    catch (...)
    {
      if (firstSupport != nullptr) occtsharp_shape_release(firstSupport);
      if (secondSupport != nullptr) occtsharp_shape_release(secondSupport);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_pair_classify(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  const double tolerance, int32_t* classification, double* distance,
  double* overlap_volume, OcctSharp_ShapeHandle** overlap_shape)
{
  if (classification == nullptr || distance == nullptr || overlap_volume == nullptr || overlap_shape == nullptr)
  { SetLastError("A shape-pair classification output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *classification = 0; *distance = 0.0; *overlap_volume = 0.0; *overlap_shape = nullptr;
  if (!std::isfinite(tolerance) || tolerance < 0.0)
  { SetLastError("Shape-pair tolerance must be finite and non-negative."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    const BRepExtrema_DistShapeShape extrema = ComputeExactDistance(first, second);
    *distance = extrema.Value();
    if (*distance > tolerance) { *classification = 0; return; }

    BRepAlgoAPI_Common common(first->Value, second->Value);
    common.Build();
    if (!common.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not compute the pair overlap.");
    const TopoDS_Shape overlap = common.Shape();
    if (!overlap.IsNull())
    {
      GProp_GProps overlapProps;
      BRepGProp::VolumeProperties(overlap, overlapProps, true);
      *overlap_volume = std::abs(overlapProps.Mass());
    }
    const double volumeTolerance = std::max(tolerance * tolerance * tolerance, 1.0e-18);
    if (*overlap_volume <= volumeTolerance) { *classification = 1; return; }

    GProp_GProps firstProps;
    GProp_GProps secondProps;
    BRepGProp::VolumeProperties(first->Value, firstProps, true);
    BRepGProp::VolumeProperties(second->Value, secondProps, true);
    const double smallerVolume = std::min(std::abs(firstProps.Mass()), std::abs(secondProps.Mass()));
    const double containmentTolerance = std::max(volumeTolerance, smallerVolume * 1.0e-9);
    *classification = smallerVolume > volumeTolerance
      && std::abs(*overlap_volume - smallerVolume) <= containmentTolerance ? 2 : 3;
    *overlap_shape = AllocateShape(overlap);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_digital_mockup_pair_analyze(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second,
  const double confusion_tolerance, const double fuzzy_tolerance,
  const int32_t run_parallel, const int32_t non_destructive,
  int32_t* classification, double* distance, double* overlap_volume,
  OcctSharp_ShapeHandle** issue_shape)
{
  if (classification == nullptr || distance == nullptr || overlap_volume == nullptr || issue_shape == nullptr)
  {
    SetLastError("A digital mock-up pair-analysis output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *classification = 0;
  *distance = 0.0;
  *overlap_volume = 0.0;
  *issue_shape = nullptr;
  if (!std::isfinite(confusion_tolerance) || confusion_tolerance < 0.0
      || !std::isfinite(fuzzy_tolerance) || fuzzy_tolerance < 0.0)
  {
    SetLastError("Digital mock-up tolerances must be finite and non-negative.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(first);
    ValidateUsableShape(second);
    const BRepExtrema_DistShapeShape extrema = ComputeExactDistance(first, second);
    *distance = extrema.Value();
    const double contactTolerance = std::max(confusion_tolerance, fuzzy_tolerance);
    if (*distance > contactTolerance)
    {
      *classification = 0;
      return;
    }

    NCollection_List<TopoDS_Shape> arguments;
    NCollection_List<TopoDS_Shape> tools;
    arguments.Append(first->Value);
    tools.Append(second->Value);
    BRepAlgoAPI_Common common;
    common.SetArguments(arguments);
    common.SetTools(tools);
    common.SetFuzzyValue(fuzzy_tolerance);
    common.SetRunParallel(run_parallel != 0);
    common.SetNonDestructive(non_destructive != 0);
    common.Build();
    if (!common.IsDone())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not compute the digital mock-up pair overlap.");
    const TopoDS_Shape overlap = common.Shape();
    if (!overlap.IsNull())
    {
      GProp_GProps overlapProps;
      BRepGProp::VolumeProperties(overlap, overlapProps, true);
      *overlap_volume = std::abs(overlapProps.Mass());
    }
    const double effectiveTolerance = std::max(confusion_tolerance, fuzzy_tolerance);
    const double volumeTolerance = std::max(
      effectiveTolerance * effectiveTolerance * effectiveTolerance, 1.0e-18);
    if (*overlap_volume <= volumeTolerance)
    {
      *classification = 1;
      BRepAlgoAPI_Section section(first->Value, second->Value, false);
      section.SetFuzzyValue(fuzzy_tolerance);
      section.SetRunParallel(run_parallel != 0);
      section.SetNonDestructive(non_destructive != 0);
      section.Build();
      if (section.IsDone() && !section.Shape().IsNull())
        *issue_shape = AllocateShape(section.Shape());
      return;
    }

    GProp_GProps firstProps;
    GProp_GProps secondProps;
    BRepGProp::VolumeProperties(first->Value, firstProps, true);
    BRepGProp::VolumeProperties(second->Value, secondProps, true);
    const double firstVolume = std::abs(firstProps.Mass());
    const double secondVolume = std::abs(secondProps.Mass());
    const double firstTolerance = std::max(volumeTolerance, firstVolume * 1.0e-9);
    const double secondTolerance = std::max(volumeTolerance, secondVolume * 1.0e-9);
    const bool containsFirst = firstVolume > volumeTolerance
      && std::abs(*overlap_volume - firstVolume) <= firstTolerance;
    const bool containsSecond = secondVolume > volumeTolerance
      && std::abs(*overlap_volume - secondVolume) <= secondTolerance;
    *classification = containsFirst && containsSecond ? 4
      : containsFirst ? 2
      : containsSecond ? 3
      : 5;
    *issue_shape = AllocateShape(overlap);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_inspection_properties(
  const OcctSharp_ShapeHandle* shape, const int32_t property_kind,
  OcctSharp_InspectionProperties* properties)
{
  if (properties == nullptr) { SetLastError("The inspection-properties output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *properties = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    const TopAbs_ShapeEnum kind = shape->Value.ShapeType();
    RequireExactFaceSupport(shape->Value);
    GProp_GProps result;
    switch (property_kind)
    {
      case 0:
        if (kind != TopAbs_EDGE && kind != TopAbs_WIRE)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Length inspection requires an edge or wire.");
        BRepGProp::LinearProperties(shape->Value, result);
        break;
      case 1:
        if (kind != TopAbs_FACE && kind != TopAbs_SHELL)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Area inspection requires a face or shell.");
        BRepGProp::SurfaceProperties(shape->Value, result);
        break;
      case 2:
        if (kind != TopAbs_SOLID && kind != TopAbs_COMPSOLID && kind != TopAbs_COMPOUND)
          throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Volume inspection requires a solid, compsolid, or compound.");
        BRepGProp::VolumeProperties(shape->Value, result, true);
        break;
      default:
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The inspection property kind is outside the supported range.");
    }
    properties->mass = result.Mass();
    properties->center = CopyPoint(result.CentreOfMass());
    CopyInertia(result.MatrixOfInertia(), *properties);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_angle(
  const OcctSharp_ShapeHandle* first, const OcctSharp_ShapeHandle* second, double* radians)
{
  if (radians == nullptr) { SetLastError("The shape-angle output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *radians = 0.0;
  return Guard([&]
  {
    ValidateUsableShape(first); ValidateUsableShape(second);
    if (first->Value.ShapeType() == TopAbs_EDGE && second->Value.ShapeType() == TopAbs_EDGE)
    {
      BRepAdaptor_Curve firstCurve(TopoDS::Edge(first->Value));
      BRepAdaptor_Curve secondCurve(TopoDS::Edge(second->Value));
      if (firstCurve.GetType() != GeomAbs_Line || secondCurve.GetType() != GeomAbs_Line)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Edge angle inspection requires two linear edges.");
      *radians = firstCurve.Line().Direction().Angle(secondCurve.Line().Direction());
      return;
    }
    if (first->Value.ShapeType() == TopAbs_FACE && second->Value.ShapeType() == TopAbs_FACE)
    {
      BRepAdaptor_Surface firstSurface(TopoDS::Face(first->Value), true);
      BRepAdaptor_Surface secondSurface(TopoDS::Face(second->Value), true);
      if (firstSurface.GetType() != GeomAbs_Plane || secondSurface.GetType() != GeomAbs_Plane)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Face angle inspection requires two planar faces.");
      gp_Dir firstNormal = firstSurface.Plane().Axis().Direction();
      gp_Dir secondNormal = secondSurface.Plane().Axis().Direction();
      if (first->Value.Orientation() == TopAbs_REVERSED) firstNormal.Reverse();
      if (second->Value.Orientation() == TopAbs_REVERSED) secondNormal.Reverse();
      *radians = firstNormal.Angle(secondNormal);
      return;
    }
    throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Angle inspection requires two linear edges or two planar faces.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_radial_measurement(
  const OcctSharp_ShapeHandle* shape, OcctSharp_RadialMeasurement* measurement)
{
  if (measurement == nullptr) { SetLastError("The radial-measurement output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *measurement = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    if (shape->Value.ShapeType() == TopAbs_EDGE)
    {
      BRepAdaptor_Curve curve(TopoDS::Edge(shape->Value));
      if (curve.GetType() != GeomAbs_Circle)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Edge radial inspection requires circular geometry.");
      measurement->geometry_kind = 0;
      measurement->radius = curve.Circle().Radius();
    }
    else if (shape->Value.ShapeType() == TopAbs_FACE)
    {
      BRepAdaptor_Surface surface(TopoDS::Face(shape->Value), true);
      if (surface.GetType() == GeomAbs_Cylinder)
      {
        measurement->geometry_kind = 1;
        measurement->radius = surface.Cylinder().Radius();
      }
      else if (surface.GetType() == GeomAbs_Cone)
      {
        measurement->geometry_kind = 2;
        measurement->radius = surface.Cone().RefRadius();
        measurement->semi_angle = surface.Cone().SemiAngle();
      }
      else
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Face radial inspection requires cylindrical or conical geometry.");
    }
    else
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Radial inspection requires an edge or face.");
    measurement->diameter = measurement->radius * 2.0;
  });
}

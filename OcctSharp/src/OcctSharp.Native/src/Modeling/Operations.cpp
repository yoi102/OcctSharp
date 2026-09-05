// Native Modeling/Operations implementation. Public contracts and ownership are unchanged.
#include "Geometry/Transforms.hxx"
#include "Modeling/Topology.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepAlgoAPI_BooleanOperation.hxx>
#include <BRepAlgoAPI_Common.hxx>
#include <BRepAlgoAPI_Cut.hxx>
#include <BRepAlgoAPI_Fuse.hxx>
#include <BRepAlgoAPI_Section.hxx>
#include <BRepFilletAPI_MakeChamfer.hxx>
#include <BRepFilletAPI_MakeFillet.hxx>
#include <BRepOffsetAPI_MakeOffsetShape.hxx>
#include <BRepOffsetAPI_MakeThickSolid.hxx>
#include <BRepPrimAPI_MakePrism.hxx>
#include <BRepPrimAPI_MakeRevol.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_List.hxx>
#include <ShapeFix_Shape.hxx>
#include <ShapeUpgrade_UnifySameDomain.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <memory>
#include <utility>

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_extrude(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_VecHandle* direction,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The extrusion output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateVector(direction);
    if (direction->Value.SquareMagnitude() <= 0.0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The extrusion direction must be non-zero.");
    BRepPrimAPI_MakePrism builder(shape->Value, direction->Value, false, false);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT extrusion did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT extrusion produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_revolve(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_Ax1Handle* axis,
  const double angle_radians, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The revolution output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  const double full_turn = 2.0 * std::acos(-1.0);
  if (!std::isfinite(angle_radians) || angle_radians == 0.0 || std::abs(angle_radians) > full_turn)
  {
    SetLastError("The revolution angle must be finite, non-zero, and no greater than one full turn in magnitude.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateAxis(axis);
    BRepPrimAPI_MakeRevol builder(shape->Value, axis->Value, angle_radians, false);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT revolution did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT revolution produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fillet_all(
  const OcctSharp_ShapeHandle* shape, const double radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The fillet output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0) { SetLastError("The fillet radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> edges;
    TopExp::MapShapes(shape->Value, TopAbs_EDGE, edges);
    if (edges.IsEmpty()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The source shape has no edges to fillet.");
    BRepFilletAPI_MakeFillet builder(shape->Value);
    for (Standard_Integer index = 1; index <= edges.Extent(); ++index) builder.Add(radius, TopoDS::Edge(edges(index)));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fillet_edge(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* edge,
  const double radius, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The fillet output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(radius) || radius <= 0.0) { SetLastError("The fillet radius must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Fillet construction requires an edge shape.");
    BRepFilletAPI_MakeFillet builder(shape->Value);
    builder.Add(radius, TopoDS::Edge(edge->Value));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT fillet construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_chamfer_all(
  const OcctSharp_ShapeHandle* shape, const double distance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The chamfer output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(distance) || distance <= 0.0) { SetLastError("The chamfer distance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> edges;
    TopExp::MapShapes(shape->Value, TopAbs_EDGE, edges);
    if (edges.IsEmpty()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The source shape has no edges to chamfer.");
    BRepFilletAPI_MakeChamfer builder(shape->Value);
    for (Standard_Integer index = 1; index <= edges.Extent(); ++index) builder.Add(distance, TopoDS::Edge(edges(index)));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_chamfer_edge(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* edge,
  const double distance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The chamfer output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(distance) || distance <= 0.0) { SetLastError("The chamfer distance must be finite and greater than zero."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape); ValidateUsableShape(edge);
    if (edge->Value.ShapeType() != TopAbs_EDGE)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Chamfer construction requires an edge shape.");
    BRepFilletAPI_MakeChamfer builder(shape->Value);
    builder.Add(distance, TopoDS::Edge(edge->Value));
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT chamfer construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_offset(
  const OcctSharp_ShapeHandle* shape, const double offset, const double tolerance,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The offset output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (!std::isfinite(offset) || offset == 0.0 || !std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The offset must be finite and non-zero, and tolerance must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    BRepOffsetAPI_MakeOffsetShape builder;
    builder.PerformByJoin(shape->Value, offset, tolerance);
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT offset construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT offset construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_make_thick_solid(
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_ShapeHandle* const* closing_faces, const int32_t face_count,
  const double offset, const double tolerance, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The thick-solid output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  if (closing_faces == nullptr || face_count < 1)
  { SetLastError("A thick solid requires at least one closing face to remove."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (!std::isfinite(offset) || offset == 0.0 || !std::isfinite(tolerance) || tolerance <= 0.0)
  {
    SetLastError("The wall offset must be finite and non-zero, and tolerance must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    if (shape->Value.ShapeType() != TopAbs_SOLID)
      throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Thick-solid construction requires a solid source shape.");
    NCollection_List<TopoDS_Shape> faces;
    for (int32_t index = 0; index < face_count; ++index)
    {
      ValidateUsableShape(closing_faces[index]);
      if (closing_faces[index]->Value.ShapeType() != TopAbs_FACE)
        throw OperationFailure(OCCTSHARP_STATUS_TYPE_MISMATCH, "Every thick-solid closing shape must be a face.");
      faces.Append(closing_faces[index]->Value);
    }
    BRepOffsetAPI_MakeThickSolid builder;
    builder.MakeThickSolidByJoin(shape->Value, faces, offset, tolerance);
    if (!builder.IsDone() || builder.Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT thick-solid construction did not complete.");
    *out_shape = AllocateShape(builder.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_section(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The section output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Section builder(left->Value, right->Value, false);
    builder.Build();
    if (!builder.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT section construction did not complete.");
    TopoDS_Shape result = builder.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT section construction produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_fuse(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The boolean fuse output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Fuse operation(left->Value, right->Value);
    operation.Build();
    if (!operation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean fuse did not complete.");
    *out_shape = AllocateShape(operation.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_cut(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The boolean cut output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Cut operation(left->Value, right->Value);
    operation.Build();
    if (!operation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean cut did not complete.");
    *out_shape = AllocateShape(operation.Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_common(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The boolean common output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(left); ValidateUsableShape(right);
    BRepAlgoAPI_Common operation(left->Value, right->Value);
    operation.Build();
    if (!operation.IsDone()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean common did not complete.");
    TopoDS_Shape result = operation.Shape();
    if (result.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean common produced a null result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_boolean_with_history(
  const OcctSharp_ShapeHandle* left, const OcctSharp_ShapeHandle* right,
  const int32_t operation_kind, const int32_t tracked_kind,
  OcctSharp_ShapeHandle** out_shape, OcctSharp_BooleanHistorySummary* out_history)
{
  if (out_shape == nullptr || out_history == nullptr)
  { SetLastError("Boolean history output pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  *out_history = {};
  if (operation_kind < 0 || operation_kind > 2)
  { SetLastError("Boolean history operation must be Fuse, Cut, or Common."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (tracked_kind < 0 || tracked_kind > 7)
  { SetLastError("Boolean history tracked kind must be Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(left);
    ValidateUsableShape(right);
    std::unique_ptr<BRepAlgoAPI_BooleanOperation> operation;
    if (operation_kind == 0) operation = std::make_unique<BRepAlgoAPI_Fuse>(left->Value, right->Value);
    else if (operation_kind == 1) operation = std::make_unique<BRepAlgoAPI_Cut>(left->Value, right->Value);
    else operation = std::make_unique<BRepAlgoAPI_Common>(left->Value, right->Value);
    operation->Build();
    if (!operation->IsDone() || operation->Shape().IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT boolean operation with history did not complete.");

    const TopAbs_ShapeEnum kind = static_cast<TopAbs_ShapeEnum>(tracked_kind);
    auto summarize = [&](const TopoDS_Shape& input,
                         int32_t& source_count,
                         int32_t& modified_source_count,
                         int32_t& generated_source_count,
                         int32_t& deleted_source_count,
                         int32_t& modified_result_count,
                         int32_t& generated_result_count)
    {
      NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> sources;
      TopExp::MapShapes(input, kind, sources);
      source_count = sources.Extent();
      for (int32_t index = 1; index <= sources.Extent(); ++index)
      {
        const TopoDS_Shape& source = sources.FindKey(index);
        if (!SupportsShapeHistory(source)) continue;
        const auto& modified = operation->Modified(source);
        const auto& generated = operation->Generated(source);
        if (!modified.IsEmpty()) ++modified_source_count;
        if (!generated.IsEmpty()) ++generated_source_count;
        if (operation->IsDeleted(source)) ++deleted_source_count;
        modified_result_count += modified.Extent();
        generated_result_count += generated.Extent();
      }
    };
    summarize(left->Value,
      out_history->left_source_count,
      out_history->left_modified_source_count,
      out_history->left_generated_source_count,
      out_history->left_deleted_source_count,
      out_history->left_modified_result_count,
      out_history->left_generated_result_count);
    summarize(right->Value,
      out_history->right_source_count,
      out_history->right_modified_source_count,
      out_history->right_generated_source_count,
      out_history->right_deleted_source_count,
      out_history->right_modified_result_count,
      out_history->right_generated_result_count);
    *out_shape = AllocateShape(operation->Shape());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_fix(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The shape fix output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ShapeFix_Shape fixer(shape->Value);
    fixer.Perform();
    TopoDS_Shape fixed = fixer.Shape();
    if (fixed.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "ShapeFix_Shape produced a null result.");
    }
    *out_shape = AllocateShape(std::move(fixed));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_unify_same_domain(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The unify-same-domain output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ShapeUpgrade_UnifySameDomain operation(shape->Value, true, true, false);
    operation.Build();
    TopoDS_Shape unified = operation.Shape();
    if (unified.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "ShapeUpgrade_UnifySameDomain produced a null result.");
    }
    *out_shape = AllocateShape(std::move(unified));
  });
}

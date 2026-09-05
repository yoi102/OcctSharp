// Native Modeling/Topology implementation. Public contracts and ownership are unchanged.
#include "Modeling/Topology.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <BRepCheck_Analyzer.hxx>
#include <BRepCheck_Result.hxx>
#include <BRepCheck_Status.hxx>
#include <BRepTools_ReShape.hxx>
#include <BRep_Tool.hxx>
#include <NCollection_IndexedDataMap.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_List.hxx>
#include <Standard_Handle.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp.hxx>
#include <TopExp_Explorer.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Face.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cstddef>
#include <cstring>
#include <limits>
#include <utility>

namespace OcctSharp::Native
{
bool SupportsShapeHistory(const TopoDS_Shape& shape)
{
  const TopAbs_ShapeEnum kind = shape.ShapeType();
  return kind == TopAbs_VERTEX || kind == TopAbs_EDGE || kind == TopAbs_FACE || kind == TopAbs_SOLID;
}

int32_t CheckedTopologyCount(const TopoDS_Shape& shape, const TopAbs_ShapeEnum kind, const bool unique)
{
  size_t count = 0;
  if (unique)
  {
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> shapes;
    TopExp::MapShapes(shape, kind, shapes);
    count = static_cast<size_t>(shapes.Extent());
  }
  else
  {
    if (shape.ShapeType() == kind) ++count;
    for (TopExp_Explorer explorer(shape, kind); explorer.More(); explorer.Next()) ++count;
  }
  if (count > static_cast<size_t>(std::numeric_limits<int32_t>::max()))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology count exceeds the 32-bit ABI.");
  }
  return static_cast<int32_t>(count);
}

OcctSharp_TopologyCounts BuildTopologyCounts(const TopoDS_Shape& shape, const bool unique)
{
  return {
    CheckedTopologyCount(shape, TopAbs_VERTEX, unique),
    CheckedTopologyCount(shape, TopAbs_EDGE, unique),
    CheckedTopologyCount(shape, TopAbs_WIRE, unique),
    CheckedTopologyCount(shape, TopAbs_FACE, unique),
    CheckedTopologyCount(shape, TopAbs_SHELL, unique),
    CheckedTopologyCount(shape, TopAbs_SOLID, unique),
    CheckedTopologyCount(shape, TopAbs_COMPSOLID, unique),
    CheckedTopologyCount(shape, TopAbs_COMPOUND, unique)
  };
}

void BuildToleranceRange(
  const TopoDS_Shape& shape,
  const TopAbs_ShapeEnum kind,
  double& minimum,
  double& maximum)
{
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> shapes;
  TopExp::MapShapes(shape, kind, shapes);
  minimum = 0.0;
  maximum = 0.0;
  if (shapes.IsEmpty()) return;

  minimum = std::numeric_limits<double>::infinity();
  for (int32_t index = 1; index <= shapes.Extent(); ++index)
  {
    const TopoDS_Shape& item = shapes(index);
    double tolerance = 0.0;
    switch (kind)
    {
      case TopAbs_VERTEX: tolerance = BRep_Tool::Tolerance(TopoDS::Vertex(item)); break;
      case TopAbs_EDGE: tolerance = BRep_Tool::Tolerance(TopoDS::Edge(item)); break;
      case TopAbs_FACE: tolerance = BRep_Tool::Tolerance(TopoDS::Face(item)); break;
      default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tolerance is available only for vertices, edges, and faces.");
    }
    minimum = std::min(minimum, tolerance);
    maximum = std::max(maximum, tolerance);
  }
}

bool IsTopologyClosed(const TopoDS_Shape& shape)
{
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> shells;
  TopExp::MapShapes(shape, TopAbs_SHELL, shells);
  if (!shells.IsEmpty())
  {
    for (int32_t index = 1; index <= shells.Extent(); ++index)
      if (!BRep_Tool::IsClosed(shells(index))) return false;
    return true;
  }
  return BRep_Tool::IsClosed(shape);
}

ValidationData BuildValidationData(
  const OcctSharp_ShapeHandle* shape,
  const bool geometryChecks,
  const bool exact)
{
  ValidateUsableShape(shape);
  BRepCheck_Analyzer analyzer(shape->Value, geometryChecks, false, exact);
  ValidationData data;
  data.IsValid = analyzer.IsValid();
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> subshapes;
  TopExp::MapShapes(shape->Value, subshapes);
  for (int32_t index = 1; index <= subshapes.Extent(); ++index)
  {
    const TopoDS_Shape& subshape = subshapes(index);
    const opencascade::handle<BRepCheck_Result>& result = analyzer.Result(subshape);
    if (result.IsNull()) continue;
    const NCollection_List<BRepCheck_Status>& statuses = result->Status();
    for (NCollection_List<BRepCheck_Status>::Iterator iterator(statuses); iterator.More(); iterator.Next())
    {
      const BRepCheck_Status status = iterator.Value();
      if (status == BRepCheck_NoError) continue;
      if (data.Issues.size() == static_cast<size_t>(std::numeric_limits<int32_t>::max()))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The validation issue count exceeds the 32-bit ABI.");
      data.Issues.push_back({
        static_cast<int32_t>(subshape.ShapeType()),
        static_cast<int32_t>(status) });
    }
  }
  return data;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_get_face_count(
  const OcctSharp_ShapeHandle* shape,
  int32_t* out_face_count)
{
  if (shape == nullptr)
  {
    SetLastError("The shape handle is null.");
    return OCCTSHARP_STATUS_NULL_HANDLE;
  }

  if (out_face_count == nullptr)
  {
    SetLastError("The output face-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }

  return Guard([&]
  {
    ValidateShape(shape);
    int32_t count = 0;
    for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next())
    {
      ++count;
    }

    *out_face_count = count;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_face_snapshot(
  const OcctSharp_ShapeHandle* shape,
  OcctSharp_ShapeHandle** out_faces,
  const int32_t capacity,
  int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The face snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  if (capacity < 0 || (capacity > 0 && out_faces == nullptr))
  { SetLastError("The face snapshot capacity or output buffer is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    int32_t required = 0;
    for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next()) ++required;
    *out_written = required;
    if (capacity < required)
    { throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The face snapshot buffer is too small."); }
    int32_t index = 0;
    try
    {
      for (TopExp_Explorer explorer(shape->Value, TopAbs_FACE); explorer.More(); explorer.Next())
      {
        out_faces[index++] = AllocateShape(TopoDS::Face(explorer.Current()));
      }
    }
    catch (...)
    {
      for (int32_t cleanup = 0; cleanup < index; ++cleanup) occtsharp_shape_release(out_faces[cleanup]);
      *out_written = 0;
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_subshape_snapshot(
  const OcctSharp_ShapeHandle* shape,
  const int32_t kind,
  OcctSharp_ShapeHandle** out_shapes,
  const int32_t capacity,
  int32_t* out_written)
{
  if (out_written == nullptr) { SetLastError("The subshape snapshot count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_written = 0;
  if (kind < 0 || kind > 7) { SetLastError("The subshape kind must be a TopAbs kind from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (capacity < 0 || (capacity > 0 && out_shapes == nullptr))
  { SetLastError("The subshape snapshot capacity or output buffer is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    const TopAbs_ShapeEnum targetKind = static_cast<TopAbs_ShapeEnum>(kind);
    int32_t required = 0;
    for (TopExp_Explorer explorer(shape->Value, targetKind); explorer.More(); explorer.Next()) ++required;
    *out_written = required;
    if (capacity < required)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The subshape snapshot buffer is too small.");
    int32_t index = 0;
    try
    {
      for (TopExp_Explorer explorer(shape->Value, targetKind); explorer.More(); explorer.Next())
        out_shapes[index++] = AllocateShape(explorer.Current());
    }
    catch (...)
    {
      for (int32_t cleanup = 0; cleanup < index; ++cleanup) occtsharp_shape_release(out_shapes[cleanup]);
      *out_written = 0;
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_subshape_count(
  const OcctSharp_ShapeHandle* shape, const int32_t kind, int32_t* out_count)
{
  if (out_count == nullptr) { SetLastError("The subshape count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_count = 0;
  if (kind < 0 || kind > 7) { SetLastError("The subshape kind must be a TopAbs kind from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateShape(shape);
    for (TopExp_Explorer explorer(shape->Value, static_cast<TopAbs_ShapeEnum>(kind)); explorer.More(); explorer.Next()) ++*out_count;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_adjacency_count(
  const OcctSharp_ShapeHandle* shape, const int32_t item_kind, const int32_t ancestor_kind,
  int32_t* out_item_count, int32_t* out_ancestor_count, int32_t* out_relation_count)
{
  if (out_item_count == nullptr || out_ancestor_count == nullptr || out_relation_count == nullptr)
  { SetLastError("Topology adjacency count output pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_item_count = *out_ancestor_count = *out_relation_count = 0;
  if (item_kind < 0 || item_kind > 7 || ancestor_kind < 0 || ancestor_kind > 7)
  { SetLastError("Topology adjacency kinds must be TopAbs kinds from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (item_kind <= ancestor_kind)
  { SetLastError("The topology item kind must be lower-level than the ancestor kind."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> items;
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> ancestors;
    NCollection_IndexedDataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher> adjacency;
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind), items);
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(ancestor_kind), ancestors);
    TopExp::MapShapesAndUniqueAncestors(
      shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind),
      static_cast<TopAbs_ShapeEnum>(ancestor_kind), adjacency, false);
    int64_t relations = 0;
    for (int32_t index = 1; index <= items.Extent(); ++index)
      if (adjacency.Contains(items.FindKey(index))) relations += adjacency.FindFromKey(items.FindKey(index)).Extent();
    if (items.Extent() > std::numeric_limits<int32_t>::max()
        || ancestors.Extent() > std::numeric_limits<int32_t>::max()
        || relations > std::numeric_limits<int32_t>::max())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The topology adjacency snapshot exceeds 32-bit capacity.");
    *out_item_count = items.Extent();
    *out_ancestor_count = ancestors.Extent();
    *out_relation_count = static_cast<int32_t>(relations);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_adjacency_snapshot(
  const OcctSharp_ShapeHandle* shape, const int32_t item_kind, const int32_t ancestor_kind,
  OcctSharp_ShapeHandle** out_items, const int32_t item_capacity,
  OcctSharp_ShapeHandle** out_ancestors, const int32_t ancestor_capacity,
  int32_t* out_offsets, const int32_t offset_capacity,
  int32_t* out_ancestor_indices, const int32_t relation_capacity,
  int32_t* out_items_written, int32_t* out_ancestors_written, int32_t* out_relations_written)
{
  if (out_items_written == nullptr || out_ancestors_written == nullptr || out_relations_written == nullptr)
  { SetLastError("Topology adjacency written-count pointers must not be null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_items_written = *out_ancestors_written = *out_relations_written = 0;
  if (item_kind < 0 || item_kind > 7 || ancestor_kind < 0 || ancestor_kind > 7 || item_kind <= ancestor_kind)
  { SetLastError("Topology adjacency kinds are invalid or not ordered from item to ancestor."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  if (item_capacity < 0 || ancestor_capacity < 0 || offset_capacity < 1 || relation_capacity < 0
      || (item_capacity > 0 && out_items == nullptr)
      || (ancestor_capacity > 0 && out_ancestors == nullptr)
      || out_offsets == nullptr
      || (relation_capacity > 0 && out_ancestor_indices == nullptr))
  { SetLastError("Topology adjacency buffer pointers or capacities are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateUsableShape(shape);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> items;
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> ancestors;
    NCollection_IndexedDataMap<TopoDS_Shape, NCollection_List<TopoDS_Shape>, TopTools_ShapeMapHasher> adjacency;
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind), items);
    TopExp::MapShapes(shape->Value, static_cast<TopAbs_ShapeEnum>(ancestor_kind), ancestors);
    TopExp::MapShapesAndUniqueAncestors(
      shape->Value, static_cast<TopAbs_ShapeEnum>(item_kind),
      static_cast<TopAbs_ShapeEnum>(ancestor_kind), adjacency, false);
    int32_t relations = 0;
    for (int32_t index = 1; index <= items.Extent(); ++index)
      if (adjacency.Contains(items.FindKey(index))) relations += adjacency.FindFromKey(items.FindKey(index)).Extent();
    if (item_capacity < items.Extent() || ancestor_capacity < ancestors.Extent()
        || offset_capacity < items.Extent() + 1 || relation_capacity < relations)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A topology adjacency snapshot buffer is too small.");

    int32_t item_written = 0;
    int32_t ancestor_written = 0;
    try
    {
      for (int32_t index = 1; index <= items.Extent(); ++index)
        out_items[item_written++] = AllocateShape(items.FindKey(index));
      for (int32_t index = 1; index <= ancestors.Extent(); ++index)
        out_ancestors[ancestor_written++] = AllocateShape(ancestors.FindKey(index));

      int32_t relation_written = 0;
      out_offsets[0] = 0;
      for (int32_t index = 1; index <= items.Extent(); ++index)
      {
        const TopoDS_Shape& item = items.FindKey(index);
        if (adjacency.Contains(item))
        {
          const NCollection_List<TopoDS_Shape>& list = adjacency.FindFromKey(item);
          for (NCollection_List<TopoDS_Shape>::Iterator iterator(list); iterator.More(); iterator.Next())
          {
            const int ancestor_index = ancestors.FindIndex(iterator.Value());
            if (ancestor_index <= 0)
              throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned an ancestor outside the indexed topology map.");
            out_ancestor_indices[relation_written++] = ancestor_index - 1;
          }
        }
        out_offsets[index] = relation_written;
      }
      *out_items_written = item_written;
      *out_ancestors_written = ancestor_written;
      *out_relations_written = relation_written;
    }
    catch (...)
    {
      for (int32_t index = 0; index < item_written; ++index) occtsharp_shape_release(out_items[index]);
      for (int32_t index = 0; index < ancestor_written; ++index) occtsharp_shape_release(out_ancestors[index]);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_replace_subshape(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* target,
  const OcctSharp_ShapeHandle* replacement, OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The reshaped output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ValidateUsableShape(target);
    ValidateUsableShape(replacement);
    bool contains = shape->Value.IsSame(target->Value);
    if (!contains)
    {
      for (TopExp_Explorer explorer(shape->Value, target->Value.ShapeType()); explorer.More(); explorer.Next())
      {
        if (explorer.Current().IsSame(target->Value)) { contains = true; break; }
      }
    }
    if (!contains)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The replacement target is not contained in the source topology.");
    BRepTools_ReShape reshaper;
    reshaper.Replace(target->Value, replacement->Value);
    TopoDS_Shape result = reshaper.Apply(shape->Value);
    if (result.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT produced a null replacement result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_remove_subshape(
  const OcctSharp_ShapeHandle* shape, const OcctSharp_ShapeHandle* target,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr) { SetLastError("The reshaped output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_shape = nullptr;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    ValidateUsableShape(target);
    if (shape->Value.IsSame(target->Value))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The root shape cannot be removed from itself.");
    bool contains = false;
    for (TopExp_Explorer explorer(shape->Value, target->Value.ShapeType()); explorer.More(); explorer.Next())
    {
      if (explorer.Current().IsSame(target->Value)) { contains = true; break; }
    }
    if (!contains)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The removal target is not contained in the source topology.");
    BRepTools_ReShape reshaper;
    reshaper.Remove(target->Value);
    TopoDS_Shape result = reshaper.Apply(shape->Value);
    if (result.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT produced a null removal result.");
    *out_shape = AllocateShape(std::move(result));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_is_valid(
  const OcctSharp_ShapeHandle* shape, int32_t* out_is_valid)
{
  if (out_is_valid == nullptr) { SetLastError("The validity output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *out_is_valid = 0;
  return Guard([&]
  {
    ValidateUsableShape(shape);
    BRepCheck_Analyzer analyzer(shape->Value);
    *out_is_valid = analyzer.IsValid() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_topology_summary(
  const OcctSharp_ShapeHandle* shape, OcctSharp_ShapeTopologySummary* out_summary)
{
  if (out_summary == nullptr)
  {
    SetLastError("The topology-summary output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_summary = {};
  return Guard([&]
  {
    ValidateUsableShape(shape);
    out_summary->unique_counts = BuildTopologyCounts(shape->Value, true);
    out_summary->occurrence_counts = BuildTopologyCounts(shape->Value, false);
    out_summary->is_closed = IsTopologyClosed(shape->Value) ? 1 : 0;
    BRepCheck_Analyzer analyzer(shape->Value);
    out_summary->is_valid = analyzer.IsValid() ? 1 : 0;
    BuildToleranceRange(shape->Value, TopAbs_VERTEX,
      out_summary->min_vertex_tolerance, out_summary->max_vertex_tolerance);
    BuildToleranceRange(shape->Value, TopAbs_EDGE,
      out_summary->min_edge_tolerance, out_summary->max_edge_tolerance);
    BuildToleranceRange(shape->Value, TopAbs_FACE,
      out_summary->min_face_tolerance, out_summary->max_face_tolerance);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_validation_issue_count(
  const OcctSharp_ShapeHandle* shape,
  const int32_t geometry_checks,
  const int32_t exact,
  int32_t* out_is_valid,
  int32_t* out_issue_count)
{
  if (out_is_valid == nullptr || out_issue_count == nullptr)
  {
    SetLastError("A validation count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_is_valid = 0;
  *out_issue_count = 0;
  return Guard([&]
  {
    ValidationData data = BuildValidationData(shape, geometry_checks != 0, exact != 0);
    *out_is_valid = data.IsValid ? 1 : 0;
    *out_issue_count = static_cast<int32_t>(data.Issues.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_shape_validation_issues(
  const OcctSharp_ShapeHandle* shape,
  const int32_t geometry_checks,
  const int32_t exact,
  OcctSharp_ValidationIssue* issues,
  const int32_t capacity,
  int32_t* out_is_valid,
  int32_t* out_issue_count)
{
  if (out_is_valid == nullptr || out_issue_count == nullptr || capacity < 0
      || (capacity > 0 && issues == nullptr))
  {
    SetLastError("The validation output buffer is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_is_valid = 0;
  *out_issue_count = 0;
  return Guard([&]
  {
    ValidationData data = BuildValidationData(shape, geometry_checks != 0, exact != 0);
    *out_is_valid = data.IsValid ? 1 : 0;
    *out_issue_count = static_cast<int32_t>(data.Issues.size());
    if (capacity < *out_issue_count)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The validation issue buffer is too small.");
    if (*out_issue_count > 0)
      std::memcpy(issues, data.Issues.data(), data.Issues.size() * sizeof(OcctSharp_ValidationIssue));
  });
}

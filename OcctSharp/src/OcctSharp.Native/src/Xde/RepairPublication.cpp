#include "OcctSharp.Native.Repair.h"
#include "Documents/Lifecycle.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Shape.hxx"
#include "Xde/Document.hxx"
#include <NCollection_IndexedMap.hxx>
#include <NCollection_Sequence.hxx>
#include <TDF_Label.hxx>
#include <TNaming_Builder.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <algorithm>
#include <map>
#include <set>
#include <vector>

using namespace OcctSharp::Native;
namespace {
void RequirePublication(bool condition, const char* message) {
  if (!condition) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
struct LabelMapping { TDF_Label Label; TopoDS_Shape Before, After; int SourceIndex, TargetIndex; };
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_xde_subshape_label(
  OcctSharp_OcafDocumentHandle* document, const char* definition_entry, int32_t index,
  char* entry, int32_t capacity, int32_t* written) {
  return Guard([&] {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    auto definition = ResolveOcafLabel(document, definition_entry);
    RequirePublication(XCAFDoc_ShapeTool::IsSimpleShape(definition), "Subshape metadata requires a simple definition.");
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> map;
    TopExp::MapShapes(XCAFDoc_ShapeTool::GetShape(definition), map);
    RequirePublication(index > 0 && index < map.Extent(), "Subshape metadata index is outside the definition.");
    auto label = GetXdeShapeTool(document)->AddSubShape(definition, map(index + 1));
    RequirePublication(!label.IsNull(), "XDE rejected the subshape metadata label.");
    CopyLabelEntry(label, entry, capacity, written);
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_xde_apply(
  OcctSharp_OcafDocumentHandle* document, const char* definition_entry, const OcctSharp_ShapeHandle* candidate,
  const OcctSharp_RepairRelation* history, int32_t history_count, int32_t apply,
  int32_t* conflicts, int32_t capacity, int32_t* conflict_count, int32_t* mapped_count, int32_t* occurrence_count) {
  return Guard([&] {
    ValidateOcafDocument(document); ValidateUsableShape(candidate);
    RequirePublication(history_count >= 0 && (history_count == 0 || history) && capacity >= 0
      && conflict_count && mapped_count && occurrence_count && (apply == 0 || apply == 1), "Invalid repair publication buffers.");
    auto tool = GetXdeShapeTool(document); auto definition = ResolveOcafLabel(document, definition_entry);
    RequirePublication(XCAFDoc_ShapeTool::IsSimpleShape(definition) && !XCAFDoc_ShapeTool::IsReference(definition),
      "Repair publication requires a reusable simple definition, not an occurrence or assembly.");
    auto original = XCAFDoc_ShapeTool::GetShape(definition);
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> before, after;
    TopExp::MapShapes(original, before); TopExp::MapShapes(candidate->Value, after);
    std::map<int, std::set<int>> targets;
    std::set<int> uncertain;
    for (int i = 0; i < history_count; ++i) {
      const auto& relation = history[i];
      RequirePublication(relation.source_index >= 0 && relation.source_index < before.Extent()
        && relation.result_index >= -1 && relation.result_index < after.Extent() && relation.kind >= 0 && relation.kind <= 4,
        "Repair publication history is outside the shape snapshots.");
      if (relation.kind == 3 || relation.kind == 4 || relation.result_index < 0) uncertain.insert(relation.source_index);
      else targets[relation.source_index].insert(relation.result_index);
    }
    NCollection_Sequence<TDF_Label> children, users;
    XCAFDoc_ShapeTool::GetSubShapes(definition, children);
    *occurrence_count = XCAFDoc_ShapeTool::GetUsers(definition, users, false);
    std::vector<LabelMapping> mappings; std::set<int> rejected;
    std::map<int, std::vector<int>> reverse;
    for (const auto& child : children) {
      auto oldShape = XCAFDoc_ShapeTool::GetShape(child); int sourceIndex = before.FindIndex(oldShape) - 1;
      if (sourceIndex < 0 || uncertain.contains(sourceIndex) || targets[sourceIndex].size() != 1) {
        rejected.insert(sourceIndex); continue;
      }
      int targetIndex = *targets[sourceIndex].begin();
      if (after(targetIndex + 1).ShapeType() != oldShape.ShapeType()) { rejected.insert(sourceIndex); continue; }
      mappings.push_back({child, oldShape, after(targetIndex + 1), sourceIndex, targetIndex});
      reverse[targetIndex].push_back(sourceIndex);
    }
    for (const auto& [target, sources] : reverse)
      if (sources.size() > 1) rejected.insert(sources.begin(), sources.end());
    *mapped_count = static_cast<int>(mappings.size()); *conflict_count = static_cast<int>(rejected.size());
    if (conflicts) {
      RequirePublication(capacity >= *conflict_count, "Publication conflict buffer is too small.");
      std::copy(rejected.begin(), rejected.end(), conflicts);
    } else RequirePublication(capacity == 0, "Missing publication conflict buffer.");
    if (!apply) return;
    RequireOpenOcafCommand(document);
    RequirePublication(rejected.empty(), "Ambiguous, deleted or merged metadata prevents atomic repair publication.");
    // Keep label identities, names and color/style assignments on each unambiguous
    // mapped label. Only its TNaming topology changes, within the caller's transaction.
    tool->SetShape(definition, candidate->Value);
    for (const auto& mapping : mappings) {
      TNaming_Builder naming(mapping.Label); naming.Modify(mapping.Before, mapping.After);
    }
    tool->UpdateAssemblies();
  });
}

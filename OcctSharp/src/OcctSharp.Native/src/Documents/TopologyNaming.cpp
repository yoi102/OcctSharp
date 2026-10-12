#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
#include "Documents/Parametric.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <NCollection_IndexedMap.hxx>
#include <TDF_ChildIterator.hxx>
#include <TDF_Reference.hxx>
#include <TNaming_Builder.hxx>
#include <TNaming_Iterator.hxx>
#include <TNaming_NamedShape.hxx>
#include <TNaming_Selector.hxx>
#include <TNaming_Tool.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <vector>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Parametric;

namespace
{
TopoDS_Shape NamedShape(const TDF_Label& label)
{
  occ::handle<TNaming_NamedShape> attribute;
  return OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(label,OcctSharpGenerated::DocumentAdapters::TNaming_NamedShape_GetID(), attribute) ? OcctSharpGenerated::DocumentAdapters::TNaming_Tool_GetShape(attribute) : TopoDS_Shape();
}
bool Contains(const TopoDS_Shape& context, const TopoDS_Shape& selection)
{
  if (context.IsNull() || selection.IsNull()) return false;
  NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> map;
  TopExp::MapShapes(context, map);
  return map.Contains(selection);
}
void RequireSelectionKind(int kind)
{
  Require(kind == TopAbs_VERTEX || kind == TopAbs_EDGE || kind == TopAbs_FACE,
    "Persistent selections support only vertices, edges and faces.");
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_record(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t kind,
  const OcctSharp_ShapeHandle* const* old_shapes, const OcctSharp_ShapeHandle* const* new_shapes, int32_t count)
{
  return Guard([&] {
    Require(kind >= 0 && kind <= 3 && count > 0 && count <= 1000000, "Invalid evolution kind or count.");
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    if (kind != 0) ValidateArray(old_shapes, count, "Missing evolution source buffer.");
    if (kind != 3) ValidateArray(new_shapes, count, "Missing evolution result buffer.");
    for (int i = 0; i < count; ++i) {
      if (kind != 0) ValidateUsableShape(old_shapes[i]);
      if (kind != 3) ValidateUsableShape(new_shapes[i]);
    }
    OcctSharpGenerated::DocumentAdapters::PrivateTNamingBuilder builder(label);
    for (int i = 0; i < count; ++i) {
      if (kind == 0) OcctSharpGenerated::DocumentAdapters::TNaming_Builder_Generated(builder,new_shapes[i]->Value);
      if (kind == 1) OcctSharpGenerated::DocumentAdapters::TNaming_Builder_Generated(builder,old_shapes[i]->Value, new_shapes[i]->Value);
      if (kind == 2) OcctSharpGenerated::DocumentAdapters::TNaming_Builder_Modify(builder,old_shapes[i]->Value, new_shapes[i]->Value);
      if (kind == 3) OcctSharpGenerated::DocumentAdapters::TNaming_Builder_Delete(builder,old_shapes[i]->Value);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_history(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t transaction, int32_t index,
  int32_t* count, int32_t* evolution, OcctSharp_ShapeHandle** old_shape, OcctSharp_ShapeHandle** new_shape)
{
  if (count) *count = 0; if (evolution) *evolution = 0;
  if (old_shape) *old_shape = nullptr; if (new_shape) *new_shape = nullptr;
  return Guard([&] {
    Require(count && evolution && old_shape && new_shape && index >= -1 && transaction >= -1, "Invalid history output or revision.");
    const auto label = ResolveOcafLabel(document, entry);
    const int revision = transaction < 0 ? document->Document->GetData()->Transaction() : transaction;
    Require(revision <= document->Document->GetData()->Transaction(), "History revision is in the future.");
    OcctSharpGenerated::DocumentAdapters::PrivateTNamingIterator iterator(label, revision);
    TopoDS_Shape before, after;
    for (; OcctSharpGenerated::DocumentAdapters::TNaming_Iterator_More(iterator); OcctSharpGenerated::DocumentAdapters::TNaming_Iterator_Next(iterator)) {
      if (*count == index) { before = OcctSharpGenerated::DocumentAdapters::TNaming_Iterator_OldShape(iterator); after = OcctSharpGenerated::DocumentAdapters::TNaming_Iterator_NewShape(iterator); *evolution = OcctSharpGenerated::DocumentAdapters::TNaming_Iterator_Evolution(iterator); }
      ++*count;
      Require(*count <= 1000000, "History exceeds the bounded limit.");
    }
    if (index == -1) return;
    Require(index < *count, "History index is outside the selected revision.");
    auto* first = before.IsNull() ? nullptr : AllocateShape(before);
    try { *new_shape = after.IsNull() ? nullptr : AllocateShape(after); }
    catch (...) { if (first) occtsharp_shape_release(first); throw; }
    *old_shape = first;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_select(
  const OcctSharp_OcafDocumentHandle* document, const char* selector_parent, const char* context_entry,
  const OcctSharp_ShapeHandle* selection, int32_t expected_kind, int32_t* selected)
{
  if (selected) *selected = 0;
  return Guard([&] {
    Require(selected, "Missing selection output."); RequireSelectionKind(expected_kind);
    ValidateUsableShape(selection);
    const auto parent = ResolveOcafLabel(document, selector_parent);
    const auto contextLabel = ResolveOcafLabel(document, context_entry); RequireOpenOcafCommand(document);
    Require(!OcctSharpGenerated::DocumentAdapters::TDF_Label_IsDescendant(contextLabel,parent) && parent != contextLabel, "The selector cannot contain its context.");
    Require(selection->Value.ShapeType() == expected_kind, "The selected topology has the wrong kind.");
    const auto context = NamedShape(contextLabel);
    Require(Contains(context, selection->Value), "The selected topology is unrelated to this context.");
    // TNaming clears descendants. Only this reserved child is ever passed to Select.
    const auto child = OcctSharpGenerated::DocumentAdapters::TDF_Label_FindChild(parent,1, true);
    // Imported primitives may have only a named root. Anchor the exact selected
    // TShape independently so naming does not infer one face from a whole solid.
    OcctSharpGenerated::DocumentAdapters::PrivateTNamingBuilder anchor(OcctSharpGenerated::DocumentAdapters::TDF_Label_FindChild(parent,2, true)); OcctSharpGenerated::DocumentAdapters::TNaming_Builder_Generated(anchor,selection->Value);
    OcctSharpGenerated::DocumentAdapters::PrivateTNamingSelector selector(child);
    *selected = OcctSharpGenerated::DocumentAdapters::TNaming_Selector_Select(selector,selection->Value, context, false, false) ? 1 : 0;
    if (*selected) OcctSharpGenerated::DocumentAdapters::TDF_Reference_Set(parent, contextLabel);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_naming_resolve(
  const OcctSharp_OcafDocumentHandle* document, const char* selector_parent,
  int32_t expected_kind, int32_t* status, OcctSharp_ShapeHandle** shape)
{
  if (status) *status = 1; if (shape) *shape = nullptr;
  return Guard([&] {
    Require(status && shape, "Missing selector resolution output."); RequireSelectionKind(expected_kind);
    const auto parent = ResolveOcafLabel(document, selector_parent); RequireOpenOcafCommand(document);
    occ::handle<TDF_Reference> reference;
    if (!OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(parent,OcctSharpGenerated::DocumentAdapters::TDF_Reference_GetID(), reference)) return;
    const auto child = OcctSharpGenerated::DocumentAdapters::TDF_Label_FindChild(parent,1, false);
    if (OcctSharpGenerated::DocumentAdapters::TDF_Label_IsNull(child)) return;
    const auto context = NamedShape(OcctSharpGenerated::DocumentAdapters::TDF_Reference_Get(*(reference)));
    if (context.IsNull()) { *status = 5; return; }
    NCollection_Map<TDF_Label> valid;
    valid.Add(document->Document->Main());
    for (OcctSharpGenerated::DocumentAdapters::PrivateTDFChildIterator it(document->Document->Main(), true); OcctSharpGenerated::DocumentAdapters::TDF_ChildIterator_More(it); OcctSharpGenerated::DocumentAdapters::TDF_ChildIterator_Next(it)) valid.Add(OcctSharpGenerated::DocumentAdapters::TDF_ChildIterator_Value(it));
    OcctSharpGenerated::DocumentAdapters::PrivateTNamingSelector selector(child);
    if (!OcctSharpGenerated::DocumentAdapters::TNaming_Selector_Solve(selector,valid)) { *status = 3; return; }
    const auto resolved = NamedShape(child);
    if (resolved.IsNull()) { *status = 5; return; }
    if (resolved.ShapeType() != expected_kind) {
      NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> candidates;
      TopExp::MapShapes(resolved, static_cast<TopAbs_ShapeEnum>(expected_kind), candidates);
      *status = candidates.Extent() > 1 ? 2 : 4; return;
    }
    if (!Contains(context, resolved)) { *status = 3; return; }
    *shape = AllocateShape(resolved); *status = 0;
  });
}

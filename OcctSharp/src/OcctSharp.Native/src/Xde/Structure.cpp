// Native Xde/Structure implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Foundation/Text.hxx"
#include "Geometry/Transforms.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Document.hxx"
#include "Xde/Structure.hxx"
#include <NCollection_DataMap.hxx>
#include <NCollection_Sequence.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_HAsciiString.hxx>
#include <TDataStd_ExtStringArray.hxx>
#include <TDataStd_Name.hxx>
#include <TopoDS_Shape.hxx>
#include <XCAFDoc_AssemblyItemId.hxx>
#include <XCAFDoc_AssemblyItemRef.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_GraphNode.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <string>
#include <utility>

namespace OcctSharp::Native
{
const Standard_GUID& AssemblyExternalReferencesId()
{
  static const Standard_GUID id("8fd9fa60-12a5-4fa6-a8b9-004b61cb0f61");
  return id;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_shape(
  OcctSharp_OcafDocumentHandle* document,
  const OcctSharp_ShapeHandle* shape,
  const char* name_utf8,
  const int32_t name_length,
  char* entry_buffer,
  const int32_t entry_capacity,
  int32_t* entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateUsableShape(shape);
    TDF_Label label = GetXdeShapeTool(document)->AddShape(shape->Value, false, false);
    TDataStd_Name::Set(label, MakeExtendedUtf8(name_utf8, name_length));
    CopyLabelEntry(label, entry_buffer, entry_capacity, entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_assembly(
  OcctSharp_OcafDocumentHandle* document,
  const char* name_utf8,
  const int32_t name_length,
  char* entry_buffer,
  const int32_t entry_capacity,
  int32_t* entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    TDF_Label label = GetXdeShapeTool(document)->NewShape();
    TDataStd_Name::Set(label, MakeExtendedUtf8(name_utf8, name_length));
    CopyLabelEntry(label, entry_buffer, entry_capacity, entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_add_component(
  OcctSharp_OcafDocumentHandle* document,
  const char* assembly_entry,
  const char* part_entry,
  const OcctSharp_LocationHandle* location,
  char* entry_buffer,
  const int32_t entry_capacity,
  int32_t* entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateLocationHandle(location);
    const TDF_Label assembly = ResolveOcafLabel(document, assembly_entry);
    const TDF_Label part = ResolveOcafLabel(document, part_entry);
    TDF_Label occurrence = GetXdeShapeTool(document)->AddComponent(assembly, part, location->Value);
    GetXdeShapeTool(document)->UpdateAssemblies();
    CopyLabelEntry(occurrence, entry_buffer, entry_capacity, entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_shape(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_ShapeHandle** out_shape)
{
  if (out_shape == nullptr)
  {
    SetLastError("The output XDE shape pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_shape = nullptr;
  return Guard([&]
  {
    TopoDS_Shape shape;
    if (!XCAFDoc_ShapeTool::GetShape(ResolveOcafLabel(document, entry), shape) || shape.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The XDE label does not contain a shape.");
    }
    *out_shape = AllocateShape(std::move(shape));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_is_assembly(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* is_assembly)
{
  if (is_assembly == nullptr)
  {
    SetLastError("The XDE assembly-state output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *is_assembly = 0;
  return Guard([&] { *is_assembly = XCAFDoc_ShapeTool::IsAssembly(ResolveOcafLabel(document, entry)) ? 1 : 0; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_component_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE component-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&] { *count = XCAFDoc_ShapeTool::NbComponents(ResolveOcafLabel(document, entry), false); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_component_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> components;
    if (!XCAFDoc_ShapeTool::GetComponents(ResolveOcafLabel(document, entry), components, false)
        || index < 1 || index > components.Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE component index is out of range.");
    }
    CopyLabelEntry(components.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_referred_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    TDF_Label referred;
    if (!XCAFDoc_ShapeTool::GetReferredShape(ResolveOcafLabel(document, entry), referred))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a component occurrence.");
    }
    CopyLabelEntry(referred, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_location(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_LocationHandle** out_location)
{
  if (out_location == nullptr)
  {
    SetLastError("The output XDE location pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_location = nullptr;
  return Guard([&]
  {
    *out_location = AllocateLocation(XCAFDoc_ShapeTool::GetLocation(ResolveOcafLabel(document, entry)));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_shape(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_ShapeHandle* shape)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateUsableShape(shape);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (XCAFDoc_ShapeTool::IsReference(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A component occurrence cannot own a replacement definition shape.");
    GetXdeShapeTool(document)->SetShape(label, shape->Value);
    GetXdeShapeTool(document)->UpdateAssemblies();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_location(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_LocationHandle* location,
  char* result_entry_buffer,
  const int32_t result_entry_capacity,
  int32_t* result_entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateLocationHandle(location);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    TDF_Label result;
    if (!GetXdeShapeTool(document)->SetLocation(label, location->Value, result) || result.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label location could not be changed.");
    GetXdeShapeTool(document)->UpdateAssemblies();
    CopyLabelEntry(result, result_entry_buffer, result_entry_capacity, result_entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_remove_component(
  OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!XCAFDoc_ShapeTool::IsComponent(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a component occurrence.");
    GetXdeShapeTool(document)->RemoveComponent(label);
    GetXdeShapeTool(document)->UpdateAssemblies();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_remove_shape(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t remove_completely,
  int32_t* removed)
{
  if (removed == nullptr)
  {
    SetLastError("The XDE remove-shape result pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *removed = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    *removed = GetXdeShapeTool(document)->RemoveShape(label, remove_completely != 0) ? 1 : 0;
    if (*removed != 0) GetXdeShapeTool(document)->UpdateAssemblies();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_user_count(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t recursive,
  int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE user-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> users;
    XCAFDoc_ShapeTool::GetUsers(ResolveOcafLabel(document, entry), users, recursive != 0);
    *count = users.Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_user_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t recursive,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> users;
    XCAFDoc_ShapeTool::GetUsers(ResolveOcafLabel(document, entry), users, recursive != 0);
    if (index < 1 || index > users.Length())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE user index is out of range.");
    CopyLabelEntry(users.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_clone_subtree(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* result_entry_buffer,
  const int32_t result_entry_capacity,
  int32_t* result_entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const TDF_Label source = ResolveOcafLabel(document, entry);
    const auto shape_tool = GetXdeShapeTool(document);
    NCollection_DataMap<TDF_Label, TDF_Label> label_map;
    const TDF_Label cloned = XCAFDoc_Editor::CloneShapeLabel(source, shape_tool, shape_tool, label_map);
    if (cloned.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The XDE shape subtree could not be cloned.");
    for (NCollection_DataMap<TDF_Label, TDF_Label>::Iterator iterator(label_map); iterator.More(); iterator.Next())
      XCAFDoc_Editor::CloneMetaData(iterator.Key(), iterator.Value(), nullptr, true, true, true, true, true);
    shape_tool->UpdateAssemblies();
    CopyLabelEntry(cloned, result_entry_buffer, result_entry_capacity, result_entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_external_references(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* const* references,
  const int32_t count)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    if (count < 0 || (count > 0 && references == nullptr))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The external-reference input is invalid.");
    NCollection_Sequence<occ::handle<TCollection_HAsciiString>> values;
    for (int32_t index = 0; index < count; ++index)
    {
      if (references[index] == nullptr || references[index][0] == '\0')
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "External-reference values cannot be empty.");
      values.Append(new TCollection_HAsciiString(references[index]));
    }
    const TDF_Label label = ResolveOcafLabel(document, entry);
    GetXdeShapeTool(document)->SetExternRefs(label, values);
    const Standard_GUID& attribute_id = AssemblyExternalReferencesId();
    if (count == 0)
    {
      label.ForgetAttribute(attribute_id);
      return;
    }
    occ::handle<TDataStd_ExtStringArray> attribute;
    if (!label.FindAttribute(attribute_id, attribute))
      attribute = TDataStd_ExtStringArray::Set(label, attribute_id, 1, count, true);
    else
      attribute->Init(1, count);
    for (int32_t index = 0; index < count; ++index)
      attribute->SetValue(index + 1, TCollection_ExtendedString(references[index], true));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_external_reference_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The external-reference count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    occ::handle<TDataStd_ExtStringArray> attribute;
    if (label.FindAttribute(AssemblyExternalReferencesId(), attribute))
    {
      *count = attribute->Length();
      return;
    }
    NCollection_Sequence<occ::handle<TCollection_HAsciiString>> values;
    XCAFDoc_ShapeTool::GetExternRefs(label, values);
    *count = values.Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_external_reference_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  int32_t* length)
{
  if (length == nullptr)
  {
    SetLastError("The external-reference length pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *length = 0;
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    occ::handle<TDataStd_ExtStringArray> attribute;
    if (label.FindAttribute(AssemblyExternalReferencesId(), attribute))
    {
      if (index < 1 || index > attribute->Length())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The external-reference index is out of range.");
      *length = static_cast<int32_t>(ExtendedToUtf8(attribute->Value(index)).size());
      return;
    }
    NCollection_Sequence<occ::handle<TCollection_HAsciiString>> values;
    XCAFDoc_ShapeTool::GetExternRefs(label, values);
    if (index < 1 || index > values.Length())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The external-reference index is out of range.");
    *length = values.Value(index).IsNull() ? 0 : values.Value(index)->Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_external_reference_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    occ::handle<TDataStd_ExtStringArray> attribute;
    if (label.FindAttribute(AssemblyExternalReferencesId(), attribute))
    {
      if (index < 1 || index > attribute->Length())
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The external-reference index is out of range.");
      CopyUtf8Result(ExtendedToUtf8(attribute->Value(index)), buffer, capacity, written);
      return;
    }
    NCollection_Sequence<occ::handle<TCollection_HAsciiString>> values;
    XCAFDoc_ShapeTool::GetExternRefs(label, values);
    if (index < 1 || index > values.Length())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The external-reference index is out of range.");
    const auto value = values.Value(index);
    CopyUtf8Result(value.IsNull() ? std::string() : std::string(value->ToCString()), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_assembly_item_reference(
  OcctSharp_OcafDocumentHandle* document,
  const char* holder_entry,
  const char* item_path,
  const int32_t subshape_index)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    if (item_path == nullptr || item_path[0] == '\0' || subshape_index < 0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The assembly-item reference is invalid.");
    const XCAFDoc_AssemblyItemId item_id{TCollection_AsciiString(item_path)};
    if (item_id.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The assembly-item path is empty.");
    const TDF_Label holder = ResolveOcafLabel(document, holder_entry);
    if (subshape_index == 0) XCAFDoc_AssemblyItemRef::Set(holder, item_id);
    else XCAFDoc_AssemblyItemRef::Set(holder, item_id, subshape_index);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_assembly_item_reference_info(
  const OcctSharp_OcafDocumentHandle* document,
  const char* holder_entry,
  int32_t* has_reference,
  int32_t* is_orphan,
  int32_t* subshape_index,
  int32_t* path_length)
{
  if (has_reference == nullptr || is_orphan == nullptr || subshape_index == nullptr || path_length == nullptr)
  {
    SetLastError("An assembly-item reference output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_reference = 0; *is_orphan = 0; *subshape_index = 0; *path_length = 0;
  return Guard([&]
  {
    const auto reference = XCAFDoc_AssemblyItemRef::Get(ResolveOcafLabel(document, holder_entry));
    if (reference.IsNull()) return;
    const std::string path(reference->GetItem().ToString().ToCString());
    *has_reference = 1;
    *is_orphan = reference->IsOrphan() ? 1 : 0;
    *subshape_index = reference->IsSubshapeIndex() ? reference->GetSubshapeIndex() : 0;
    *path_length = static_cast<int32_t>(path.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_assembly_item_reference_path(
  const OcctSharp_OcafDocumentHandle* document,
  const char* holder_entry,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    const auto reference = XCAFDoc_AssemblyItemRef::Get(ResolveOcafLabel(document, holder_entry));
    if (reference.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label has no assembly-item reference.");
    CopyUtf8Result(std::string(reference->GetItem().ToString().ToCString()), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_shuo_create(
  OcctSharp_OcafDocumentHandle* document,
  const char* const* occurrence_entries,
  const int32_t count,
  char* result_entry_buffer,
  const int32_t result_entry_capacity,
  int32_t* result_entry_written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    if (count < 2 || occurrence_entries == nullptr)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A SHUO chain requires at least two occurrence entries.");
    NCollection_Sequence<TDF_Label> labels;
    for (int32_t index = 0; index < count; ++index)
    {
      const TDF_Label label = ResolveOcafLabel(document, occurrence_entries[index]);
      if (!XCAFDoc_ShapeTool::IsComponent(label))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Every SHUO chain item must be a component occurrence.");
      labels.Append(label);
    }
    occ::handle<XCAFDoc_GraphNode> main;
    if (!GetXdeShapeTool(document)->SetSHUO(labels, main) || main.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The SHUO chain could not be created.");
    CopyLabelEntry(main->Label(), result_entry_buffer, result_entry_capacity, result_entry_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_shuo_link_count(
  const OcctSharp_OcafDocumentHandle* document,
  const char* shuo_entry,
  const int32_t upper,
  int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The SHUO link-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> labels;
    const TDF_Label shuo = ResolveOcafLabel(document, shuo_entry);
    if (upper != 0) XCAFDoc_ShapeTool::GetSHUOUpperUsage(shuo, labels);
    else XCAFDoc_ShapeTool::GetSHUONextUsage(shuo, labels);
    *count = labels.Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_shuo_link_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const char* shuo_entry,
  const int32_t upper,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> labels;
    const TDF_Label shuo = ResolveOcafLabel(document, shuo_entry);
    if (upper != 0) XCAFDoc_ShapeTool::GetSHUOUpperUsage(shuo, labels);
    else XCAFDoc_ShapeTool::GetSHUONextUsage(shuo, labels);
    if (index < 1 || index > labels.Length())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The SHUO link index is out of range.");
    CopyLabelEntry(labels.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_free_shape_count(
  const OcctSharp_OcafDocumentHandle* document, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE free-shape count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> labels;
    GetXdeShapeTool(document)->GetFreeShapes(labels);
    *count = labels.Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_free_shape_entry(
  const OcctSharp_OcafDocumentHandle* document,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> labels;
    GetXdeShapeTool(document)->GetFreeShapes(labels);
    if (index < 1 || index > labels.Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE free-shape index is out of range.");
    }
    CopyLabelEntry(labels.Value(index), buffer, capacity, written);
  });
}

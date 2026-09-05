// Native Documents/State implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Documents/State.hxx"
#include "Foundation/Text.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include <NCollection_List.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDF_AttributeDelta.hxx>
#include <TDF_AttributeIterator.hxx>
#include <TDF_ChildIterator.hxx>
#include <TDF_Delta.hxx>
#include <TDF_Reference.hxx>
#include <TDF_Tool.hxx>
#include <TDataStd_AsciiString.hxx>
#include <TDataStd_Comment.hxx>
#include <TDataStd_Integer.hxx>
#include <TDataStd_IntegerArray.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_Real.hxx>
#include <TDataStd_RealArray.hxx>
#include <TDataStd_ReferenceArray.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TNaming_Builder.hxx>
#include <TNaming_NamedShape.hxx>
#include <TNaming_Tool.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <limits>
#include <string>
#include <vector>

namespace OcctSharp::Native
{
std::string DocumentLabelEntry(const TDF_Label& label)
{
  TCollection_AsciiString entry;
  TDF_Tool::Entry(label, entry);
  return std::string(entry.ToCString());
}

std::string DocumentGuid(const Standard_GUID& guid)
{
  char value[Standard_GUID_SIZE_ALLOC] = {};
  guid.ToCString(value);
  return std::string(value);
}

int32_t DocumentAttributeKindOf(const occ::handle<TDF_Attribute>& attribute)
{
  const Standard_GUID& id = attribute->ID();
  if (id == TDataStd_Name::GetID()) return DocumentAttributeName;
  if (id == TDataStd_Comment::GetID()) return DocumentAttributeComment;
  if (id == TDataStd_AsciiString::GetID()) return DocumentAttributeAsciiString;
  if (id == TDataStd_Integer::GetID()) return DocumentAttributeInteger;
  if (id == TDataStd_Real::GetID()) return DocumentAttributeReal;
  if (id == TDataStd_IntegerArray::GetID()) return DocumentAttributeIntegerArray;
  if (id == TDataStd_RealArray::GetID()) return DocumentAttributeRealArray;
  if (id == TDF_Reference::GetID()) return DocumentAttributeReference;
  if (id == TDataStd_ReferenceArray::GetID()) return DocumentAttributeReferenceArray;
  if (id == TDataStd_TreeNode::GetDefaultTreeID()) return DocumentAttributeTreeNode;
  if (id == TNaming_NamedShape::GetID()) return DocumentAttributeNamedShape;
  return DocumentAttributeUnknown;
}

std::vector<occ::handle<TDF_Attribute>> DocumentAttributes(const TDF_Label& label)
{
  std::vector<occ::handle<TDF_Attribute>> attributes;
  for (TDF_AttributeIterator iterator(label); iterator.More(); iterator.Next())
    attributes.push_back(iterator.Value());
  std::sort(attributes.begin(), attributes.end(), [](const auto& left, const auto& right)
  {
    const std::string leftId = DocumentGuid(left->ID());
    const std::string rightId = DocumentGuid(right->ID());
    if (leftId != rightId) return leftId < rightId;
    return std::string(left->DynamicType()->Name()) < std::string(right->DynamicType()->Name());
  });
  return attributes;
}

const Standard_GUID& DocumentKindId(const int32_t kind)
{
  switch (kind)
  {
    case DocumentAttributeName: return TDataStd_Name::GetID();
    case DocumentAttributeComment: return TDataStd_Comment::GetID();
    case DocumentAttributeAsciiString: return TDataStd_AsciiString::GetID();
    case DocumentAttributeInteger: return TDataStd_Integer::GetID();
    case DocumentAttributeReal: return TDataStd_Real::GetID();
    case DocumentAttributeIntegerArray: return TDataStd_IntegerArray::GetID();
    case DocumentAttributeRealArray: return TDataStd_RealArray::GetID();
    case DocumentAttributeReference: return TDF_Reference::GetID();
    case DocumentAttributeReferenceArray: return TDataStd_ReferenceArray::GetID();
    case DocumentAttributeTreeNode: return TDataStd_TreeNode::GetDefaultTreeID();
    case DocumentAttributeNamedShape: return TNaming_NamedShape::GetID();
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document attribute kind is unsupported.");
  }
}

std::string DocumentTextValue(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind, bool& found)
{
  const TDF_Label label = ResolveOcafLabel(document, entry);
  found = true;
  if (kind == DocumentAttributeName)
  {
    occ::handle<TDataStd_Name> attribute;
    if (!label.FindAttribute(TDataStd_Name::GetID(), attribute)) { found = false; return {}; }
    return ExtendedToUtf8(attribute->Get());
  }
  if (kind == DocumentAttributeComment)
  {
    occ::handle<TDataStd_Comment> attribute;
    if (!label.FindAttribute(TDataStd_Comment::GetID(), attribute)) { found = false; return {}; }
    return ExtendedToUtf8(attribute->Get());
  }
  if (kind == DocumentAttributeAsciiString)
  {
    occ::handle<TDataStd_AsciiString> attribute;
    if (!label.FindAttribute(TDataStd_AsciiString::GetID(), attribute)) { found = false; return {}; }
    return std::string(attribute->Get().ToCString());
  }
  throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested attribute is not textual.");
}

occ::handle<TDataStd_TreeNode> DocumentTreeNode(const TDF_Label& label, const bool create)
{
  occ::handle<TDataStd_TreeNode> node;
  if (!label.FindAttribute(TDataStd_TreeNode::GetDefaultTreeID(), node) && create)
    node = TDataStd_TreeNode::Set(label);
  return node;
}

occ::handle<TDF_Delta> DocumentHistoryDelta(
  const OcctSharp_OcafDocumentHandle* document, const bool redo, const int32_t index)
{
  ValidateOcafDocument(document);
  const auto& list = redo ? document->Document->GetRedos() : document->Document->GetUndos();
  if (index < 1 || index > list.Size())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The history index is outside the valid 1-based range.");
  int32_t current = 1;
  for (NCollection_List<occ::handle<TDF_Delta>>::Iterator iterator(list); iterator.More(); iterator.Next(), ++current)
    if (current == index) return iterator.Value();
  throw OperationFailure(OCCTSHARP_STATUS_UNKNOWN_EXCEPTION, "The history entry could not be resolved.");
}

std::vector<std::string> DocumentHistoryLabels(const occ::handle<TDF_Delta>& delta)
{
  std::vector<std::string> entries;
  for (NCollection_List<occ::handle<TDF_AttributeDelta>>::Iterator iterator(delta->AttributeDeltas());
       iterator.More(); iterator.Next())
  {
    const TDF_Label label = iterator.Value()->Label();
    if (!label.IsNull()) entries.push_back(DocumentLabelEntry(label));
  }
  std::sort(entries.begin(), entries.end());
  entries.erase(std::unique(entries.begin(), entries.end()), entries.end());
  return entries;
}

void RequireClosedDocumentCommand(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  if (document->Document->HasOpenCommand())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document command must be committed or aborted first.");
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_save_format(
  OcctSharp_OcafDocumentHandle* document, const char* file_path, const int32_t xde, const int32_t xml)
{
  return Guard([&]
  {
    RequireClosedDocumentCommand(document);
    ValidatePath(file_path);
    document->Document->ChangeStorageFormat(TCollection_ExtendedString(
      xde != 0 ? (xml != 0 ? "XmlXCAF" : "BinXCAF") : (xml != 0 ? "XmlOcaf" : "BinOcaf")));
    const PCDM_StoreStatus status = document->Application->SaveAs(
      document->Document, TCollection_ExtendedString(file_path, true));
    if (status != PCDM_SS_OK)
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not save the requested document format.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_label_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* tag, int32_t* depth, int32_t* is_root, int32_t* has_parent,
  char* parent_buffer, const int32_t parent_capacity, int32_t* parent_written)
{
  if (tag == nullptr || depth == nullptr || is_root == nullptr || has_parent == nullptr || parent_written == nullptr)
  { SetLastError("A document label-info output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *tag = 0; *depth = 0; *is_root = 0; *has_parent = 0; *parent_written = 0;
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    *tag = label.Tag(); *depth = label.Depth(); *is_root = label.IsRoot() ? 1 : 0;
    if (!label.IsRoot())
    {
      *has_parent = 1;
      CopyUtf8Result(DocumentLabelEntry(label.Father()), parent_buffer, parent_capacity, parent_written);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_child_entry(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t index,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    if (index < 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Child indices are 1-based.");
    int32_t current = 1;
    for (TDF_ChildIterator iterator(ResolveOcafLabel(document, entry), false); iterator.More(); iterator.Next(), ++current)
      if (current == index) { CopyUtf8Result(DocumentLabelEntry(iterator.Value()), buffer, capacity, written); return; }
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The child index is out of range.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_attribute_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr) { SetLastError("The attribute-count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&] { *count = static_cast<int32_t>(DocumentAttributes(ResolveOcafLabel(document, entry)).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_attribute_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t index,
  int32_t* kind, char* id_buffer, const int32_t id_capacity, int32_t* id_written,
  char* type_buffer, const int32_t type_capacity, int32_t* type_written)
{
  if (kind == nullptr) { SetLastError("The attribute-kind pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *kind = 0;
  return Guard([&]
  {
    const auto attributes = DocumentAttributes(ResolveOcafLabel(document, entry));
    if (index < 1 || index > static_cast<int32_t>(attributes.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The attribute index is out of range.");
    const auto& attribute = attributes[static_cast<size_t>(index - 1)];
    *kind = DocumentAttributeKindOf(attribute);
    CopyUtf8Result(DocumentGuid(attribute->ID()), id_buffer, id_capacity, id_written);
    CopyUtf8Result(std::string(attribute->DynamicType()->Name()), type_buffer, type_capacity, type_written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_text_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind,
  int32_t* has_value, int32_t* length)
{
  if (has_value == nullptr || length == nullptr)
  { SetLastError("A text metadata output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_value = 0; *length = 0;
  return Guard([&]
  {
    bool found = false; const std::string value = DocumentTextValue(document, entry, kind, found);
    *has_value = found ? 1 : 0; *length = found ? static_cast<int32_t>(value.size()) : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_text_copy(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    bool found = false; const std::string value = DocumentTextValue(document, entry, kind, found);
    if (!found) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document label has no requested text attribute.");
    CopyUtf8Result(value, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_text_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind,
  const char* utf8, const int32_t length)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (kind == DocumentAttributeName) TDataStd_Name::Set(label, MakeExtendedUtf8(utf8, length));
    else if (kind == DocumentAttributeComment) TDataStd_Comment::Set(label, MakeExtendedUtf8(utf8, length));
    else if (kind == DocumentAttributeAsciiString) TDataStd_AsciiString::Set(label, MakeAsciiString(utf8, length));
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested attribute is not textual.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_scalar_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind,
  int32_t* has_value, int32_t* integer_value, double* real_value)
{
  if (has_value == nullptr || integer_value == nullptr || real_value == nullptr)
  { SetLastError("A scalar output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_value = 0; *integer_value = 0; *real_value = 0.0;
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (kind == DocumentAttributeInteger)
    {
      occ::handle<TDataStd_Integer> attribute;
      if (label.FindAttribute(TDataStd_Integer::GetID(), attribute)) { *has_value = 1; *integer_value = attribute->Get(); }
    }
    else if (kind == DocumentAttributeReal)
    {
      occ::handle<TDataStd_Real> attribute;
      if (label.FindAttribute(TDataStd_Real::GetID(), attribute)) { *has_value = 1; *real_value = attribute->Get(); }
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested attribute is not scalar.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_scalar_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind,
  const int32_t integer_value, const double real_value)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (kind == DocumentAttributeInteger) TDataStd_Integer::Set(label, integer_value);
    else if (kind == DocumentAttributeReal)
    {
      if (!std::isfinite(real_value)) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Real attributes must be finite.");
      TDataStd_Real::Set(label, real_value);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested attribute is not scalar.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_attribute_remove(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    ResolveOcafLabel(document, entry).ForgetAttribute(DocumentKindId(kind));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_array_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind,
  int32_t* has_value, int32_t* lower, int32_t* count)
{
  if (has_value == nullptr || lower == nullptr || count == nullptr)
  { SetLastError("An array metadata output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_value = 0; *lower = 0; *count = 0;
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (kind == DocumentAttributeIntegerArray)
    {
      occ::handle<TDataStd_IntegerArray> attribute;
      if (label.FindAttribute(TDataStd_IntegerArray::GetID(), attribute))
      { *has_value = 1; *lower = attribute->Lower(); *count = attribute->Upper() - attribute->Lower() + 1; }
    }
    else if (kind == DocumentAttributeRealArray)
    {
      occ::handle<TDataStd_RealArray> attribute;
      if (label.FindAttribute(TDataStd_RealArray::GetID(), attribute))
      { *has_value = 1; *lower = attribute->Lower(); *count = attribute->Upper() - attribute->Lower() + 1; }
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The requested attribute is not an array.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_integer_array_copy(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The integer-array count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    occ::handle<TDataStd_IntegerArray> attribute;
    if (!ResolveOcafLabel(document, entry).FindAttribute(TDataStd_IntegerArray::GetID(), attribute))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document label has no integer array.");
    const int32_t count = attribute->Upper() - attribute->Lower() + 1;
    ValidateDocumentOutputArray(values, capacity, count, "The integer-array output buffer is too small.");
    for (int32_t offset = 0; offset < count; ++offset) values[offset] = attribute->Value(attribute->Lower() + offset);
    *written = count;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_real_array_copy(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  double* values, const int32_t capacity, int32_t* written)
{
  if (written == nullptr) { SetLastError("The real-array count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  return Guard([&]
  {
    occ::handle<TDataStd_RealArray> attribute;
    if (!ResolveOcafLabel(document, entry).FindAttribute(TDataStd_RealArray::GetID(), attribute))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document label has no real array.");
    const int32_t count = attribute->Upper() - attribute->Lower() + 1;
    ValidateDocumentOutputArray(values, capacity, count, "The real-array output buffer is too small.");
    for (int32_t offset = 0; offset < count; ++offset) values[offset] = attribute->Value(attribute->Lower() + offset);
    *written = count;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_integer_array_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t lower,
  const int32_t* values, const int32_t count)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document); ValidateArray(values, count, "The integer-array input is invalid.");
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (count == 0) { label.ForgetAttribute(TDataStd_IntegerArray::GetID()); return; }
    const int64_t upper64 = static_cast<int64_t>(lower) + count - 1;
    if (upper64 < std::numeric_limits<int32_t>::min() || upper64 > std::numeric_limits<int32_t>::max())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The integer-array bounds overflow Int32.");
    const int32_t upper = static_cast<int32_t>(upper64);
    auto attribute = TDataStd_IntegerArray::Set(label, lower, upper);
    for (int32_t offset = 0; offset < count; ++offset) attribute->SetValue(lower + offset, values[offset]);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_real_array_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t lower,
  const double* values, const int32_t count)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document); ValidateArray(values, count, "The real-array input is invalid.");
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (count == 0) { label.ForgetAttribute(TDataStd_RealArray::GetID()); return; }
    const int64_t upper64 = static_cast<int64_t>(lower) + count - 1;
    if (upper64 < std::numeric_limits<int32_t>::min() || upper64 > std::numeric_limits<int32_t>::max())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The real-array bounds overflow Int32.");
    const int32_t upper = static_cast<int32_t>(upper64);
    auto attribute = TDataStd_RealArray::Set(label, lower, upper);
    for (int32_t offset = 0; offset < count; ++offset)
    {
      if (!std::isfinite(values[offset])) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Real-array values must be finite.");
      attribute->SetValue(lower + offset, values[offset]);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t array,
  int32_t* has_value, int32_t* count)
{
  if (has_value == nullptr || count == nullptr)
  { SetLastError("A reference metadata output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_value = 0; *count = 0;
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (array == 0)
    {
      occ::handle<TDF_Reference> attribute;
      if (label.FindAttribute(TDF_Reference::GetID(), attribute)) { *has_value = 1; *count = 1; }
    }
    else
    {
      occ::handle<TDataStd_ReferenceArray> attribute;
      if (label.FindAttribute(TDataStd_ReferenceArray::GetID(), attribute))
      { *has_value = 1; *count = attribute->Upper() - attribute->Lower() + 1; }
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_entry(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t array,
  const int32_t index, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    TDF_Label target;
    if (array == 0)
    {
      if (index != 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A direct reference has exactly one 1-based item.");
      occ::handle<TDF_Reference> attribute;
      if (!label.FindAttribute(TDF_Reference::GetID(), attribute))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document label has no direct reference.");
      target = attribute->Get();
    }
    else
    {
      occ::handle<TDataStd_ReferenceArray> attribute;
      if (!label.FindAttribute(TDataStd_ReferenceArray::GetID(), attribute))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document label has no reference array.");
      const int32_t count = attribute->Upper() - attribute->Lower() + 1;
      if (index < 1 || index > count) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The reference-array index is out of range.");
      target = attribute->Value(attribute->Lower() + index - 1);
    }
    CopyUtf8Result(DocumentLabelEntry(target), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const char* target_entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    TDF_Reference::Set(ResolveOcafLabel(document, entry), ResolveOcafLabel(document, target_entry));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_reference_array_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry,
  const char* const* target_entries, const int32_t count)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    if (count < 0 || (count > 0 && target_entries == nullptr))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The reference-array input is invalid.");
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (count == 0) { label.ForgetAttribute(TDataStd_ReferenceArray::GetID()); return; }
    auto attribute = TDataStd_ReferenceArray::Set(label, 1, count);
    for (int32_t index = 0; index < count; ++index)
      attribute->SetValue(index + 1, ResolveOcafLabel(document, target_entries[index]));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_node, int32_t* has_parent, int32_t* child_count)
{
  if (has_node == nullptr || has_parent == nullptr || child_count == nullptr)
  { SetLastError("A tree metadata output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_node = 0; *has_parent = 0; *child_count = 0;
  return Guard([&]
  {
    const auto node = DocumentTreeNode(ResolveOcafLabel(document, entry), false);
    if (node.IsNull()) return;
    *has_node = 1; *has_parent = node->HasFather() ? 1 : 0; *child_count = node->NbChildren(false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_entry(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t parent,
  const int32_t index, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const auto node = DocumentTreeNode(ResolveOcafLabel(document, entry), false);
    if (node.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document label has no application tree node.");
    if (parent != 0)
    {
      if (index != 1 || !node->HasFather()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The application tree node has no requested parent.");
      CopyUtf8Result(DocumentLabelEntry(node->Father()->Label()), buffer, capacity, written); return;
    }
    if (index < 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tree child indices are 1-based.");
    int32_t current = 1;
    for (auto child = node->First(); !child.IsNull(); child = child->Next(), ++current)
      if (current == index) { CopyUtf8Result(DocumentLabelEntry(child->Label()), buffer, capacity, written); return; }
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The application tree child index is out of range.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_reparent(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const char* parent_entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto child = DocumentTreeNode(ResolveOcafLabel(document, entry), true);
    const auto parent = DocumentTreeNode(ResolveOcafLabel(document, parent_entry), true);
    if (child == parent || child->IsFather(parent))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The application tree relationship would create a cycle.");
    if (child->HasFather()) child->Remove();
    if (!parent->Append(child)) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT could not append the application tree node.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_tree_detach(
  OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto node = DocumentTreeNode(ResolveOcafLabel(document, entry), false);
    if (!node.IsNull() && node->HasFather()) node->Remove();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_named_shape_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_shape, OcctSharp_ShapeHandle** shape)
{
  if (has_shape == nullptr || shape == nullptr)
  { SetLastError("A named-shape output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_shape = 0; *shape = nullptr;
  return Guard([&]
  {
    occ::handle<TNaming_NamedShape> attribute;
    if (!ResolveOcafLabel(document, entry).FindAttribute(TNaming_NamedShape::GetID(), attribute)) return;
    const TopoDS_Shape value = TNaming_Tool::GetShape(attribute);
    if (!value.IsNull()) { *has_shape = 1; *shape = AllocateShape(value); }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_named_shape_set(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_ShapeHandle* shape)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document); ValidateUsableShape(shape);
    TNaming_Builder(ResolveOcafLabel(document, entry)).Generated(shape->Value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_commit_named_command(
  OcctSharp_OcafDocumentHandle* document, const char* utf8, const int32_t length, int32_t* changed)
{
  if (changed == nullptr) { SetLastError("The named-command result pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *changed = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const bool committed = document->Document->CommitCommand();
    *changed = committed ? 1 : 0;
    if (committed && length > 0)
    {
      const auto& undos = document->Document->GetUndos();
      if (!undos.IsEmpty()) undos.First()->SetName(MakeExtendedUtf8(utf8, length));
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_state(
  const OcctSharp_OcafDocumentHandle* document, int32_t* undo_limit,
  int32_t* undo_count, int32_t* redo_count, int32_t* is_changed)
{
  if (undo_limit == nullptr || undo_count == nullptr || redo_count == nullptr || is_changed == nullptr)
  { SetLastError("A document history-state output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *undo_limit = 0; *undo_count = 0; *redo_count = 0; *is_changed = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    const int32_t nativeLimit = document->Document->GetUndoLimit();
    *undo_limit = nativeLimit == std::numeric_limits<int32_t>::max() ? -1 : nativeLimit;
    *undo_count = document->Document->GetAvailableUndos();
    *redo_count = document->Document->GetAvailableRedos();
    *is_changed = document->Document->IsChanged() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_set_limit(
  OcctSharp_OcafDocumentHandle* document, const int32_t undo_limit)
{
  return Guard([&]
  {
    RequireClosedDocumentCommand(document);
    if (undo_limit < -1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Undo limit must be -1, zero, or positive.");
    document->Document->SetUndoLimit(
      undo_limit == -1 ? std::numeric_limits<int32_t>::max() : undo_limit);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_action(
  OcctSharp_OcafDocumentHandle* document, const int32_t action, int32_t* changed)
{
  if (changed == nullptr) { SetLastError("The history-action result pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *changed = 0;
  return Guard([&]
  {
    RequireClosedDocumentCommand(document);
    if (action == 0) *changed = document->Document->Undo() ? 1 : 0;
    else if (action == 1) *changed = document->Document->Redo() ? 1 : 0;
    else if (action == 2) { document->Document->ClearUndos(); *changed = 1; }
    else if (action == 3) { document->Document->ClearRedos(); *changed = 1; }
    else if (action == 4) { document->Document->SetSaved(); *changed = 1; }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The document history action is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_entry_info(
  const OcctSharp_OcafDocumentHandle* document, const int32_t redo, const int32_t index,
  int32_t* begin_time, int32_t* end_time, int32_t* delta_count,
  int32_t* label_count, int32_t* name_length)
{
  if (begin_time == nullptr || end_time == nullptr || delta_count == nullptr || label_count == nullptr || name_length == nullptr)
  { SetLastError("A history-entry output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *begin_time = 0; *end_time = 0; *delta_count = 0; *label_count = 0; *name_length = 0;
  return Guard([&]
  {
    const auto delta = DocumentHistoryDelta(document, redo != 0, index);
    *begin_time = delta->BeginTime(); *end_time = delta->EndTime();
    *delta_count = delta->AttributeDeltas().Size();
    *label_count = static_cast<int32_t>(DocumentHistoryLabels(delta).size());
    *name_length = delta->Name().LengthOfCString();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_entry_name(
  const OcctSharp_OcafDocumentHandle* document, const int32_t redo, const int32_t index,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    CopyUtf8Result(ExtendedToUtf8(DocumentHistoryDelta(document, redo != 0, index)->Name()), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_document_history_entry_label(
  const OcctSharp_OcafDocumentHandle* document, const int32_t redo, const int32_t index,
  const int32_t label_index, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const auto labels = DocumentHistoryLabels(DocumentHistoryDelta(document, redo != 0, index));
    if (label_index < 1 || label_index > static_cast<int32_t>(labels.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The history label index is out of range.");
    CopyUtf8Result(labels[static_cast<size_t>(label_index - 1)], buffer, capacity, written);
  });
}

#pragma once

// Private native Documents/State contract; never a public ABI or a second owner.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.h"
#include "Runtime/Error.hxx"
#include <Standard_Handle.hxx>
#include <TDF_Delta.hxx>
#include <TDataStd_TreeNode.hxx>
#include <string>
#include <vector>

namespace OcctSharp::Native
{
enum DocumentAttributeKind
{
  DocumentAttributeUnknown = 0,
  DocumentAttributeName = 1,
  DocumentAttributeComment = 2,
  DocumentAttributeAsciiString = 3,
  DocumentAttributeInteger = 4,
  DocumentAttributeReal = 5,
  DocumentAttributeIntegerArray = 6,
  DocumentAttributeRealArray = 7,
  DocumentAttributeReference = 8,
  DocumentAttributeReferenceArray = 9,
  DocumentAttributeTreeNode = 10,
  DocumentAttributeNamedShape = 11
};

std::string DocumentLabelEntry(const TDF_Label& label);

std::string DocumentGuid(const Standard_GUID& guid);

int32_t DocumentAttributeKindOf(const occ::handle<TDF_Attribute>& attribute);

std::vector<occ::handle<TDF_Attribute>> DocumentAttributes(const TDF_Label& label);

const Standard_GUID& DocumentKindId(const int32_t kind);

std::string DocumentTextValue(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const int32_t kind, bool& found);

occ::handle<TDataStd_TreeNode> DocumentTreeNode(const TDF_Label& label, const bool create);

occ::handle<TDF_Delta> DocumentHistoryDelta(
  const OcctSharp_OcafDocumentHandle* document, const bool redo, const int32_t index);

std::vector<std::string> DocumentHistoryLabels(const occ::handle<TDF_Delta>& delta);

void RequireClosedDocumentCommand(const OcctSharp_OcafDocumentHandle* document);

template <typename T>
void ValidateDocumentOutputArray(T* values, const int32_t capacity, const int32_t required, const char* message)
{
  if (capacity < required || (required > 0 && values == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
}

// Native Documents/Lifecycle implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Foundation/Text.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include <BinDrivers.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDF_TagSource.hxx>
#include <TDF_Tool.hxx>
#include <TDataStd_Name.hxx>
#include <TDocStd_Application.hxx>
#include <TDocStd_Document.hxx>
#include <XmlDrivers.hxx>
#include <string>
#include <utility>

namespace OcctSharp::Native
{
void ValidateOcafDocument(const OcctSharp_OcafDocumentHandle* handle)
{
  if (handle == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The OCAF document handle is null.");
  }
  if (!IsLiveValue(handle, LiveOcafDocuments))
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_HANDLE,
      "The OCAF document handle is invalid or already released.");
  }
  if (handle->Application.IsNull() || handle->Document.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The OCAF document is closed.");
  }
}

TDF_Label ResolveOcafLabel(const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  ValidateOcafDocument(document);
  if (entry == nullptr || entry[0] == '\0')
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The OCAF label entry is null or empty.");
  }
  TDF_Label label;
  TDF_Tool::Label(document->Document->GetData(), entry, label, false);
  if (label.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The OCAF label entry does not exist.");
  }
  return label;
}

void RequireOpenOcafCommand(const OcctSharp_OcafDocumentHandle* document)
{
  if (!document->Document->HasOpenCommand())
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "An OCAF transaction must be open before modifying labels.");
  }
}

void CopyLabelEntry(const TDF_Label& label, char* buffer, const int32_t capacity, int32_t* written)
{
  if (label.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT returned a null XDE label.");
  }
  TCollection_AsciiString entry;
  TDF_Tool::Entry(label, entry);
  CopyUtf8Result(std::string(entry.ToCString()), buffer, capacity, written);
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_create(
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output OCAF document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
    BinDrivers::DefineFormat(application);
    XmlDrivers::DefineFormat(application);
    opencascade::handle<TDocStd_Document> document;
    application->NewDocument(TCollection_ExtendedString("BinOcaf"), document);
    if (document.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null OCAF document.");
    }
    document->SetUndoLimit(10);
    *out_document = AllocateValue(
      new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
      LiveOcafDocuments);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_open(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output OCAF document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
    BinDrivers::DefineFormat(application);
    XmlDrivers::DefineFormat(application);
    opencascade::handle<TDocStd_Document> document;
    const PCDM_ReaderStatus status = application->Open(
      TCollection_ExtendedString(file_path, true), document);
    if (status != PCDM_RS_OK || document.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not open the binary OCAF document.");
    }
    document->SetUndoLimit(10);
    *out_document = AllocateValue(
      new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
      LiveOcafDocuments);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_save(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_INVALID_ARGUMENT,
        "The OCAF transaction must be committed or aborted before saving.");
    }
    const PCDM_StoreStatus status = document->Application->SaveAs(
      document->Document, TCollection_ExtendedString(file_path, true));
    if (status != PCDM_SS_OK)
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not save the binary OCAF document.");
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_has_open_command(
  const OcctSharp_OcafDocumentHandle* document, int32_t* has_open_command)
{
  if (has_open_command == nullptr)
  {
    SetLastError("The OCAF command-state output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_open_command = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    *has_open_command = document->Document->HasOpenCommand() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_begin_command(
  OcctSharp_OcafDocumentHandle* document)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An OCAF transaction is already open.");
    }
    document->Document->NewCommand();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_commit_command(
  OcctSharp_OcafDocumentHandle* document, int32_t* changed)
{
  if (changed == nullptr)
  {
    SetLastError("The OCAF commit result pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *changed = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    *changed = document->Document->CommitCommand() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_abort_command(
  OcctSharp_OcafDocumentHandle* document)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    document->Document->AbortCommand();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_document_main_entry(
  const OcctSharp_OcafDocumentHandle* document,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    TCollection_AsciiString entry;
    TDF_Tool::Entry(document->Document->Main(), entry);
    CopyUtf8Result(std::string(entry.ToCString()), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_add_child(
  OcctSharp_OcafDocumentHandle* document, const char* parent_entry, int32_t* child_tag)
{
  if (child_tag == nullptr)
  {
    SetLastError("The OCAF child-tag output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *child_tag = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const TDF_Label parent = ResolveOcafLabel(document, parent_entry);
    const TDF_Label child = TDF_TagSource::NewChild(parent);
    if (child.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null OCAF child label.");
    }
    *child_tag = child.Tag();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_child_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The OCAF child-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&] { *count = ResolveOcafLabel(document, entry).NbChildren(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_set_name(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* utf8,
  const int32_t length)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    TDataStd_Name::Set(ResolveOcafLabel(document, entry), MakeExtendedUtf8(utf8, length));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_name,
  int32_t* length)
{
  if (has_name == nullptr || length == nullptr)
  {
    SetLastError("An OCAF name metadata output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_name = 0;
  *length = 0;
  return Guard([&]
  {
    opencascade::handle<TDataStd_Name> name;
    if (ResolveOcafLabel(document, entry).FindAttribute(TDataStd_Name::GetID(), name))
    {
      *has_name = 1;
      *length = name->Get().LengthOfCString();
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_ocaf_label_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    opencascade::handle<TDataStd_Name> name;
    if (!ResolveOcafLabel(document, entry).FindAttribute(TDataStd_Name::GetID(), name))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The OCAF label has no name attribute.");
    }
    CopyUtf8Result(ExtendedToUtf8(name->Get()), buffer, capacity, written);
  });
}

void OCCTSHARP_CALL occtsharp_ocaf_document_release(OcctSharp_OcafDocumentHandle* document)
{
  if (document != nullptr && UnregisterValue(document, LiveOcafDocuments))
  {
    if (!document->Application.IsNull() && !document->Document.IsNull())
    {
      if (document->Document->HasOpenCommand())
      {
        document->Document->AbortCommand();
      }
      document->Application->Close(document->Document);
    }
    delete document;
  }
}

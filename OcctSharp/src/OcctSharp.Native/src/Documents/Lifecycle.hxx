#pragma once

// Private native Documents/Lifecycle contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <Standard_Handle.hxx>
#include <TDocStd_Application.hxx>
#include <TDocStd_Document.hxx>
#include <utility>

struct OcctSharp_OcafDocumentHandle
{
  OcctSharp_OcafDocumentHandle(opencascade::handle<TDocStd_Application> application,
                               opencascade::handle<TDocStd_Document> document)
    : Application(std::move(application)), Document(std::move(document))
  {
  }

  opencascade::handle<TDocStd_Application> Application;
  opencascade::handle<TDocStd_Document> Document;
};

namespace OcctSharp::Native
{
void ValidateOcafDocument(const OcctSharp_OcafDocumentHandle* handle);

TDF_Label ResolveOcafLabel(const OcctSharp_OcafDocumentHandle* document, const char* entry);

void RequireOpenOcafCommand(const OcctSharp_OcafDocumentHandle* document);

void CopyLabelEntry(const TDF_Label& label, char* buffer, const int32_t capacity, int32_t* written);
}

#pragma once

// Private native Xde/Document contract; never a public ABI or a second owner.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.h"
#include <Standard_Handle.hxx>
#include <TDocStd_Document.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <vector>

namespace OcctSharp::Native
{
occ::handle<TDocStd_Document> CreateXdeDocument();

void InitializeXdeTools(const occ::handle<TDocStd_Document>& document);

std::vector<TDF_Label> CloneXdeRootsIntoDocument(
  const occ::handle<TDocStd_Document>& source_document,
  const occ::handle<TDocStd_Document>& output_document,
  const char* source_format);

OcctSharp_OcafDocumentHandle* CreateOwnedXdeDocument();

opencascade::handle<XCAFDoc_ShapeTool> GetXdeShapeTool(
  const OcctSharp_OcafDocumentHandle* document);
}

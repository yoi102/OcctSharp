#pragma once
#include <Standard_Handle.hxx>
#include <TDocStd_Document.hxx>

namespace OcctSharp::Native
{
// Private export copy. No source mutation, topology rebinding or new public ABI.
occ::handle<TDocStd_Document> CreateIgesExportCopy(
  const occ::handle<TDocStd_Document>& source);
}

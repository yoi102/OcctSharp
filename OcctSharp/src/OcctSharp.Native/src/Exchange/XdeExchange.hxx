#pragma once

// Private native Exchange/XdeExchange contract; never a public ABI or a second owner.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.h"
#include "Runtime/Error.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Document.hxx"
#include <BRepMesh_IncrementalMesh.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESCAFControl_Reader.hxx>
#include <NCollection_Sequence.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <TDocStd_Document.hxx>
#include <TopoDS_Shape.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <vector>

namespace OcctSharp::Native
{
void ConfigureXdeReader(
  STEPCAFControl_Reader& reader,
  const bool read_names = true,
  const bool read_colors = true,
  const bool read_layers = true,
  const bool read_validation_properties = true,
  const bool read_materials = true,
  const bool read_gdt = true,
  const bool read_views = true);

IFSelect_ReturnStatus ReadXdeStepFile(STEPCAFControl_Reader& reader, const char* file_path);

void PreTransferStepStyleTargets(STEPCAFControl_Reader& reader);

void RecoverStepPresentationStyles(
  STEPCAFControl_Reader& reader,
  const occ::handle<TDocStd_Document>& document);

void ConfigureXdeWriter(
  STEPCAFControl_Writer& writer,
  const bool write_names = true,
  const bool write_colors = true,
  const bool write_layers = true,
  const bool write_validation_properties = true,
  const bool write_materials = true,
  const bool write_gdt = true);

std::vector<TDF_Label> ImportStepRootsIntoXdeDocument(
  const char* file_path,
  const occ::handle<TDocStd_Document>& output_document);

void ConfigureXdeIgesReader(
  IGESCAFControl_Reader& reader,
  const bool read_names,
  const bool read_colors,
  const bool read_layers);

occ::handle<TDocStd_Document> ReadIgesXdeDocument(
  const char* file_path,
  const bool read_names,
  const bool read_colors,
  const bool read_layers,
  OcctSharp_IgesReadReport* report);

std::vector<TDF_Label> ImportIgesRootsIntoXdeDocument(
  const char* file_path,
  const occ::handle<TDocStd_Document>& output_document);

template <typename TProvider>
OcctSharp_Status ReadXdeMeshDocument(
  const char* file_path,
  OcctSharp_OcafDocumentHandle** out_document,
  TProvider& provider,
  const char* failure_message)
{
  if (out_document == nullptr)
  {
    SetLastError("The output mesh-scene XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    OcctSharp_OcafDocumentHandle* result = CreateOwnedXdeDocument();
    try
    {
      if (!provider.Read(TCollection_AsciiString(file_path), result->Document))
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, failure_message);
      GetXdeShapeTool(result)->UpdateAssemblies();
      *out_document = result;
    }
    catch (...)
    {
      occtsharp_ocaf_document_release(result);
      throw;
    }
  });
}

template <typename TProvider>
OcctSharp_Status WriteXdeMeshDocument(
  const OcctSharp_OcafDocumentHandle* document,
  const char* file_path,
  TProvider& provider,
  const char* failure_message)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE transaction must be closed before mesh-scene export.");
    NCollection_Sequence<TDF_Label> roots;
    const opencascade::handle<XCAFDoc_ShapeTool> shapeTool = GetXdeShapeTool(document);
    shapeTool->GetFreeShapes(roots);
    for (int32_t index = 1; index <= roots.Length(); ++index)
    {
      const TopoDS_Shape shape = shapeTool->GetShape(roots.Value(index));
      if (shape.IsNull()) continue;
      BRepMesh_IncrementalMesh mesher(shape, 0.1, false, 0.5, true);
      if (!mesher.IsDone())
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not triangulate an XDE scene root for mesh export.");
    }
    if (!provider.Write(TCollection_AsciiString(file_path), document->Document))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, failure_message);
  });
}
}

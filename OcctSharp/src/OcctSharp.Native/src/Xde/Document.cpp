// Native Xde/Document implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Document.hxx"
#include <BinXCAFDrivers.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_Sequence.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TDocStd_Application.hxx>
#include <TDocStd_Document.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XmlXCAFDrivers.hxx>
#include <cstddef>
#include <string>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
occ::handle<TDocStd_Document> CreateXdeDocument()
{
  occ::handle<TDocStd_Document> document =
    new TDocStd_Document(TCollection_ExtendedString("BinXCAF"));
  XCAFDoc_DocumentTool::Set(document->Main());
  return document;
}

void InitializeXdeTools(const occ::handle<TDocStd_Document>& document)
{
  const TDF_Label& main = document->Main();
  XCAFDoc_DocumentTool::ShapeTool(main);
  XCAFDoc_DocumentTool::ColorTool(main);
  XCAFDoc_DocumentTool::LayerTool(main);
  XCAFDoc_DocumentTool::DimTolTool(main);
  XCAFDoc_DocumentTool::MaterialTool(main);
  XCAFDoc_DocumentTool::VisMaterialTool(main);
  XCAFDoc_DocumentTool::ViewTool(main);
  XCAFDoc_DocumentTool::ClippingPlaneTool(main);
}

std::vector<TDF_Label> CloneXdeRootsIntoDocument(
  const occ::handle<TDocStd_Document>& source_document,
  const occ::handle<TDocStd_Document>& output_document,
  const char* source_format)
{
  occ::handle<XCAFDoc_ShapeTool> source_shape_tool =
    XCAFDoc_DocumentTool::ShapeTool(source_document->Main());
  occ::handle<XCAFDoc_ShapeTool> output_shape_tool =
    XCAFDoc_DocumentTool::ShapeTool(output_document->Main());
  occ::handle<XCAFDoc_LayerTool> source_layer_tool =
    XCAFDoc_DocumentTool::LayerTool(source_document->Main());
  occ::handle<XCAFDoc_LayerTool> output_layer_tool =
    XCAFDoc_DocumentTool::LayerTool(output_document->Main());
  NCollection_Sequence<TDF_Label> source_roots;
  source_shape_tool->GetFreeShapes(source_roots);
  if (source_roots.IsEmpty())
  {
    const std::string message = std::string("A ") + source_format + " input produced no free XDE shape roots.";
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, message.c_str());
  }

  std::vector<TDF_Label> imported_roots;
  imported_roots.reserve(static_cast<size_t>(source_roots.Size()));
  NCollection_DataMap<occ::handle<XCAFDoc_VisMaterial>, occ::handle<XCAFDoc_VisMaterial>>
    visual_material_map;
  for (NCollection_Sequence<TDF_Label>::Iterator root_iterator(source_roots); root_iterator.More();
       root_iterator.Next())
  {
    NCollection_DataMap<TDF_Label, TDF_Label> label_map;
    TDF_Label cloned_root = XCAFDoc_Editor::CloneShapeLabel(
      root_iterator.Value(), source_shape_tool, output_shape_tool, label_map);
    if (cloned_root.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "An XDE shape tree could not be cloned into the destination document.");

    for (NCollection_DataMap<TDF_Label, TDF_Label>::Iterator label_iterator(label_map);
         label_iterator.More(); label_iterator.Next())
    {
      occ::handle<TDataStd_TreeNode> material_reference;
      const bool has_material_reference =
        label_iterator.Key().FindAttribute(XCAFDoc::MaterialRefGUID(), material_reference)
        && material_reference->HasFather();
      const occ::handle<NCollection_HSequence<TCollection_ExtendedString>> source_layers =
        source_layer_tool->GetLayers(label_iterator.Key());
      XCAFDoc_Editor::CloneMetaData(
        label_iterator.Key(), label_iterator.Value(), &visual_material_map,
        true, false, true, true, true);
      if (!source_layers.IsNull())
      {
        for (int32_t layer_index = 1; layer_index <= source_layers->Length(); ++layer_index)
          output_layer_tool->SetLayer(
            label_iterator.Value(), source_layers->Value(layer_index), false);
      }
      if (has_material_reference && label_iterator.Value() != cloned_root)
      {
        XCAFDoc_Editor::CloneMetaData(
          label_iterator.Key(), cloned_root, &visual_material_map,
          false, false, true, false, false);
      }
    }
    imported_roots.push_back(cloned_root);
  }
  return imported_roots;
}

OcctSharp_OcafDocumentHandle* CreateOwnedXdeDocument()
{
  opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
  BinXCAFDrivers::DefineFormat(application);
  XmlXCAFDrivers::DefineFormat(application);
  opencascade::handle<TDocStd_Document> document;
  application->NewDocument(TCollection_ExtendedString("BinXCAF"), document);
  if (document.IsNull())
  {
    throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null XDE document.");
  }
  document->SetUndoLimit(10);
  InitializeXdeTools(document);
  return AllocateValue(
    new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
    LiveOcafDocuments);
}

opencascade::handle<XCAFDoc_ShapeTool> GetXdeShapeTool(
  const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::ShapeTool(document->Document->Main());
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_create(
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&] { *out_document = CreateOwnedXdeDocument(); });
}

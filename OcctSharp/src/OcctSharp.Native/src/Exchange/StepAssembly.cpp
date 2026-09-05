// Native Exchange/StepAssembly implementation. Public contracts and ownership are unchanged.
#include "Exchange/XdeExchange.hxx"
#include "Geometry/Transforms.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Document.hxx"
#include <NCollection_DataMap.hxx>
#include <NCollection_Sequence.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TDocStd_Document.hxx>
#include <TopLoc_Location.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <gp_Trsf.hxx>

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_step_merge_xde(
  const OcctSharp_StepAssemblyInput* inputs,
  const int32_t input_count,
  const char* output_path)
{
  return Guard([&]
  {
    if (inputs == nullptr)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP input array is null.");
    }
    if (input_count <= 0)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_INVALID_ARGUMENT,
        "At least one STEP input is required.");
    }
    ValidatePath(output_path);

    occ::handle<TDocStd_Document> outputDocument = CreateXdeDocument();
    InitializeXdeTools(outputDocument);
    occ::handle<XCAFDoc_ShapeTool> outputShapeTool =
      XCAFDoc_DocumentTool::ShapeTool(outputDocument->Main());
    TDF_Label outputAssembly = outputShapeTool->NewShape();
    TDataStd_Name::Set(outputAssembly, TCollection_ExtendedString("OcctSharp Assembly"));

    int32_t rootCount = 0;
    for (int32_t inputIndex = 0; inputIndex < input_count; ++inputIndex)
    {
      const OcctSharp_StepAssemblyInput& input = inputs[inputIndex];
      ValidatePath(input.file_path);
      gp_Trsf transform = CreateTransform(input);

      occ::handle<TDocStd_Document> sourceDocument = CreateXdeDocument();
      InitializeXdeTools(sourceDocument);
      STEPCAFControl_Reader reader;
      ConfigureXdeReader(reader);
      if (ReadXdeStepFile(reader, input.file_path) != IFSelect_RetDone)
      {
        throw OperationFailure(
          OCCTSHARP_STATUS_FILE_IO_ERROR,
          "OCCT could not read a STEP input through STEPCAF.");
      }
      PreTransferStepStyleTargets(reader);
      if (!reader.Transfer(sourceDocument))
      {
        throw OperationFailure(
          OCCTSHARP_STATUS_TRANSFER_FAILED,
          "A STEP input could not be transferred into an XDE document.");
      }
      RecoverStepPresentationStyles(reader, sourceDocument);

      occ::handle<XCAFDoc_ShapeTool> sourceShapeTool =
        XCAFDoc_DocumentTool::ShapeTool(sourceDocument->Main());
      NCollection_Sequence<TDF_Label> sourceRoots;
      sourceShapeTool->GetFreeShapes(sourceRoots);
      if (sourceRoots.IsEmpty())
      {
        throw OperationFailure(
          OCCTSHARP_STATUS_TRANSFER_FAILED,
          "A STEP input produced no free XDE shape roots.");
      }

      NCollection_DataMap<occ::handle<XCAFDoc_VisMaterial>, occ::handle<XCAFDoc_VisMaterial>>
        visualMaterialMap;
      for (NCollection_Sequence<TDF_Label>::Iterator rootIterator(sourceRoots); rootIterator.More();
           rootIterator.Next())
      {
        NCollection_DataMap<TDF_Label, TDF_Label> labelMap;
        TDF_Label clonedRoot = XCAFDoc_Editor::CloneShapeLabel(
          rootIterator.Value(), sourceShapeTool, outputShapeTool, labelMap);
        if (clonedRoot.IsNull())
        {
          throw OperationFailure(
            OCCTSHARP_STATUS_TRANSFER_FAILED,
            "An XDE shape tree could not be cloned into the output document.");
        }

        for (NCollection_DataMap<TDF_Label, TDF_Label>::Iterator labelIterator(labelMap);
             labelIterator.More(); labelIterator.Next())
        {
          occ::handle<TDataStd_TreeNode> materialReference;
          const bool hasMaterialReference =
            labelIterator.Key().FindAttribute(XCAFDoc::MaterialRefGUID(), materialReference)
            && materialReference->HasFather();
          XCAFDoc_Editor::CloneMetaData(
            labelIterator.Key(),
            labelIterator.Value(),
            &visualMaterialMap,
            true,
            true,
            true,
            true,
            true);
          if (hasMaterialReference && labelIterator.Value() != clonedRoot)
          {
            // STEPCAF material export operates on top-level part labels. Preserve a
            // subshape assignment in its original cloned label and also promote it
            // to the corresponding part root for round-trip STEP material export.
            XCAFDoc_Editor::CloneMetaData(
              labelIterator.Key(),
              clonedRoot,
              &visualMaterialMap,
              false,
              false,
              true,
              false,
              false);
          }
        }

        TDF_Label component = outputShapeTool->AddComponent(
          outputAssembly,
          clonedRoot,
          TopLoc_Location(transform));
        if (component.IsNull())
        {
          throw OperationFailure(
            OCCTSHARP_STATUS_TRANSFER_FAILED,
            "A cloned XDE root could not be placed in the output assembly.");
        }
        ++rootCount;
      }
    }

    if (rootCount == 0)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_TRANSFER_FAILED,
        "No STEP roots were added to the output XDE assembly.");
    }
    outputShapeTool->UpdateAssemblies();

    STEPCAFControl_Writer writer;
    ConfigureXdeWriter(writer);
    if (!writer.Transfer(outputDocument, STEPControl_AsIs))
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_TRANSFER_FAILED,
        "The output XDE document could not be transferred to STEP.");
    }
    if (writer.Write(output_path) != IFSelect_RetDone)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_FILE_IO_ERROR,
        "OCCT could not write the XDE STEP assembly.");
    }
  });
}

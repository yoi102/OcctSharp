#include "../../generated/Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
#include "Documents/Lifecycle.hxx"
#include "Geometry/Transforms.hxx"
#include "Runtime/Error.hxx"
#include "Xde/Document.hxx"
#include <BRepBuilderAPI_Transform.hxx>
#include <NCollection_IndexedMap.hxx>
#include <NCollection_DataMap.hxx>
#include <NCollection_Sequence.hxx>
#include <TDataStd_Name.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <cmath>
#include <utility>
#include <vector>

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_copy_transformed_definition(
  OcctSharp_OcafDocumentHandle* destination, const OcctSharp_OcafDocumentHandle* source,
  const char* entry, const OcctSharp_TrsfHandle* transform,
  char* result_entry, int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    ValidateOcafDocument(destination); ValidateOcafDocument(source);
    RequireOpenOcafCommand(destination); ValidateTransformHandle(transform);
    const auto sourceLabel = ResolveOcafLabel(source, entry);
    if (!OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_IsSimpleShape(sourceLabel) || OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_IsReference(sourceLabel))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Transformed copy requires a simple reusable definition.");
    if (std::abs(std::abs(transform->Value.ScaleFactor()) - 1.0) > 1e-12)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Transformed copy supports rigid placements and exact reflections, not scaling.");

    const auto destinationTool = GetXdeShapeTool(destination);
    // Normalize the source's root location explicitly. XCAF AddShape turns a
    // located result into a reference label, which cannot own subshape styles.
    const auto original = OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetShape(sourceLabel);
    const auto inverseLocation = original.Location().Inverted();
    const auto local = original.Moved(inverseLocation);
    const auto effective = transform->Value.Multiplied(original.Location().Transformation());
    BRepBuilderAPI_Transform operation(local, effective, true, false);
    if (!operation.IsDone())throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "Exact definition transformation failed.");
    const auto result = operation.Shape();
    NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> resultShapes, used;
    TopExp::MapShapes(result, resultShapes);
    NCollection_Sequence<TDF_Label> subshapes; OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetSubShapes(sourceLabel, subshapes);
    std::vector<std::pair<TDF_Label, TopoDS_Shape>> mappings;
    for (const auto& label : subshapes)
    {
      const auto before = OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetShape(label).Moved(inverseLocation);
      const auto after = operation.ModifiedShape(before);
      if (after.IsNull() || before.ShapeType() != after.ShapeType() || !resultShapes.Contains(after) || used.Contains(after))
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "The transformation has no unique metadata correspondence.");
      used.Add(after); mappings.push_back({label, after});
    }
    const auto copy = destinationTool->NewShape();
    OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_SetShape_Bind(*(destinationTool))(copy, result);
    if (OcctSharpGenerated::DocumentAdapters::TDF_Label_IsNull(copy) || copy == sourceLabel)
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "XDE could not create an independent transformed definition.");
    NCollection_DataMap<occ::handle<XCAFDoc_VisMaterial>, occ::handle<XCAFDoc_VisMaterial>> materials;
    auto copyMetadata = [&](const TDF_Label& from, const TDF_Label& to)
    {
      // A transformed centroid/PMI or arbitrary source attribute is not valid
      // merely because its face has a correspondence. Copy only our contract.
      OcctSharpGenerated::XdeAdapters::XCAFDoc_Editor_CloneMetaData(from, to, &materials, true, true, true, true, false);
      XCAFDoc_DocumentTool::ColorTool(destination->Document->Main())->SetVisibility(to,
        XCAFDoc_DocumentTool::ColorTool(source->Document->Main())->IsVisible(from));
      occ::handle<TDataStd_Name> name;
      if (OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(from,OcctSharpGenerated::DocumentAdapters::TDataStd_Name_GetID(), name))OcctSharpGenerated::DocumentAdapters::TDataStd_Name_Set(to, OcctSharpGenerated::DocumentAdapters::TDataStd_GenericExtString_Get(*(name)));
    };
    copyMetadata(sourceLabel, copy);
    for (const auto& mapping : mappings)
    {
      const auto label = OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_AddSubShape_Bind(*(destinationTool))(copy, mapping.second);
      if (OcctSharpGenerated::DocumentAdapters::TDF_Label_IsNull(label))throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "XDE rejected transformed subshape metadata.");
      copyMetadata(mapping.first, label);
    }
    destinationTool->UpdateAssemblies();
    CopyLabelEntry(copy, result_entry, capacity, written);
  });
}

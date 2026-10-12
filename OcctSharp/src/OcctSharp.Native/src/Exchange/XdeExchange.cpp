#include "../../generated/DataExchange/OcctSharp.DataExchange.MeshExchangeAdapters.Generated.hxx"
#include "../../generated/DataExchange/OcctSharp.DataExchange.StepAdapters.Generated.hxx"
#include "../../generated/Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
// Native Exchange/XdeExchange implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Documents/NamedDataPersistence.hxx"
#include "Exchange/XdeExchange.hxx"
#include "Exchange/IgesExportCopy.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Document.hxx"
#include <BinXCAFDrivers.hxx>
#include <DEGLTF_Provider.hxx>
#include <DEOBJ_ConfigurationNode.hxx>
#include <DEOBJ_Provider.hxx>
#include <DEPLY_ConfigurationNode.hxx>
#include <DEPLY_Provider.hxx>
#include <DESTEP_Parameters.hxx>
#include <DEVRML_ConfigurationNode.hxx>
#include <DEVRML_Provider.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <IGESCAFControl_Reader.hxx>
#include <IGESCAFControl_Writer.hxx>
#include <IGESData_IGESModel.hxx>
#include <Interface_EntityIterator.hxx>
#include <Interface_Graph.hxx>
#include <NCollection_Sequence.hxx>
#include <NCollection_IndexedMap.hxx>
#include <Quantity_Color.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <STEPCAFControl_Reader.hxx>
#include <STEPCAFControl_Writer.hxx>
#include <STEPConstruct_Styles.hxx>
#include <STEPControl_ActorRead.hxx>
#include <STEPControl_StepModelType.hxx>
#include <Standard_Handle.hxx>
#include <Standard_Transient.hxx>
#include <StepRepr_RepresentationRelationshipWithTransformation.hxx>
#include <StepShape_ShapeRepresentation.hxx>
#include <StepVisual_Colour.hxx>
#include <StepVisual_StyledItem.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TDocStd_Application.hxx>
#include <TDocStd_Document.hxx>
#include <TopLoc_Location.hxx>
#include <TopExp.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS_Shape.hxx>
#include <TransferBRep.hxx>
#include <Transfer_TransientProcess.hxx>
#include <UnitsMethods.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XSControl_TransferReader.hxx>
#include <XSControl_WorkSession.hxx>
#include <XmlXCAFDrivers.hxx>
#include <gp_Trsf.hxx>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
void ConfigureXdeReader(
  STEPCAFControl_Reader& reader,
  const bool read_names,
  const bool read_colors,
  const bool read_layers,
  const bool read_validation_properties,
  const bool read_materials,
  const bool read_gdt,
  const bool read_views)
{
  reader.SetColorMode(read_colors);
  reader.SetNameMode(read_names);
  reader.SetLayerMode(read_layers);
  reader.SetPropsMode(read_validation_properties);
  reader.SetMetaMode(true);
  reader.SetProductMetaMode(true);
  reader.SetSHUOMode(true);
  reader.SetGDTMode(read_gdt);
  reader.SetMatMode(read_materials);
  reader.SetViewMode(read_views);
}

IFSelect_ReturnStatus ReadXdeStepFile(STEPCAFControl_Reader& reader, const char* file_path)
{
  DESTEP_Parameters parameters;
  OcctSharpGenerated::StepAdapters::DESTEP_Parameters_InitFromStatic_Bind(parameters)();
  return reader.ReadFile(file_path, parameters);
}

void PreTransferStepStyleTargets(STEPCAFControl_Reader& reader)
{
  const occ::handle<XSControl_WorkSession> work_session = reader.Reader().WS();
  if (work_session.IsNull() || work_session->Model().IsNull()) return;
  // Establish complete product geometry before transferring isolated styled edges
  // or faces. Otherwise their context-free cached binders can leave an assembly's
  // shell-based surface definition empty when the product root is transferred.
  OcctSharpGenerated::StepAdapters::XSControl_Reader_TransferRoots_Bind(reader.ChangeReader())();
  for (int32_t index = 1; index <= work_session->Model()->NbEntities(); ++index)
  {
    const occ::handle<StepVisual_StyledItem> style =
      occ::down_cast<StepVisual_StyledItem>(work_session->Model()->Value(index));
    if (style.IsNull() || style->ItemAP242().IsNull()) continue;
    reader.ChangeReader().TransferEntity(style->ItemAP242().Value());
  }
  // TransferEntity appends recovery-only style targets to the reader's result
  // list. Keep their cached binders, but let STEPCAF rebuild the list from the
  // product roots; otherwise styled solids are inserted as extra components.
  OcctSharpGenerated::StepAdapters::XSControl_Reader_ClearShapes_Bind(reader.ChangeReader())();
}

void RecoverStepPresentationStyles(
  STEPCAFControl_Reader& reader,
  const occ::handle<TDocStd_Document>& document)
{
  const occ::handle<XSControl_WorkSession> work_session = reader.Reader().WS();
  if (work_session.IsNull() || work_session->Model().IsNull()) return;
  const occ::handle<XSControl_TransferReader> transfer_reader = work_session->TransferReader();
  if (transfer_reader.IsNull()) return;
  const occ::handle<Transfer_TransientProcess> transfer = transfer_reader->TransientProcess();
  if (transfer.IsNull()) return;
  const Interface_Graph& graph = work_session->HGraph()->Graph();
  const occ::handle<STEPControl_ActorRead> actor =
    occ::down_cast<STEPControl_ActorRead>(transfer_reader->Actor());
  const occ::handle<XCAFDoc_ShapeTool> shape_tool =
    XCAFDoc_DocumentTool::ShapeTool(document->Main());
  const occ::handle<XCAFDoc_ColorTool> color_tool =
    XCAFDoc_DocumentTool::ColorTool(document->Main());

  struct RecoveredStyle
  {
    occ::handle<StepVisual_StyledItem> Style;
    TopoDS_Shape Shape;
  };
  std::vector<RecoveredStyle> recovered;
  for (int32_t index = 1; index <= work_session->Model()->NbEntities(); ++index)
  {
    const occ::handle<StepVisual_StyledItem> style =
      occ::down_cast<StepVisual_StyledItem>(work_session->Model()->Value(index));
    if (style.IsNull() || style->ItemAP242().IsNull()) continue;
    const occ::handle<Standard_Transient> target = style->ItemAP242().Value();
    const int32_t map_index = transfer->MapIndex(target);
    if (map_index <= 0) continue;
    TopoDS_Shape shape = TransferBRep::ShapeResult(transfer->MapItem(map_index));
    if (shape.IsNull()) continue;

    occ::handle<StepShape_ShapeRepresentation> representation =
      occ::down_cast<StepShape_ShapeRepresentation>(target);
    if (representation.IsNull())
    {
      Interface_EntityIterator parents = graph.Sharings(target);
      for (parents.Start(); parents.More(); parents.Next())
      {
        representation = occ::down_cast<StepShape_ShapeRepresentation>(parents.Value());
        if (!representation.IsNull()) break;
      }
    }
    if (!representation.IsNull() && !actor.IsNull())
    {
      Interface_EntityIterator relationships = graph.Sharings(representation);
      for (relationships.Start(); relationships.More(); relationships.Next())
      {
        const occ::handle<StepRepr_RepresentationRelationshipWithTransformation> relationship =
          occ::down_cast<StepRepr_RepresentationRelationshipWithTransformation>(
            relationships.Value());
        if (relationship.IsNull()) continue;
        gp_Trsf transformation;
        if (!actor->ComputeSRRWT(relationship, transfer, transformation)) break;
        if (relationship->Rep2() == representation) transformation.Invert();
        shape.Move(TopLoc_Location(transformation), false);
        break;
      }
    }
    recovered.push_back({style, shape});
  }

  std::stable_sort(
    recovered.begin(),
    recovered.end(),
    [](const RecoveredStyle& first, const RecoveredStyle& second)
    {
      return first.Shape.ShapeType() < second.Shape.ShapeType();
    });

  STEPConstruct_Styles style_reader(work_session);
  occ::handle<NCollection_HSequence<occ::handle<Standard_Transient>>> invisible_styles =
    new NCollection_HSequence<occ::handle<Standard_Transient>>;
  style_reader.LoadInvisStyles(invisible_styles);
  struct StyleTarget { TDF_Label Part; TopoDS_Shape Shape; };
  std::unordered_map<const void*, std::vector<StyleTarget>> exact_targets;
  bool targets_built = false;
  for (const RecoveredStyle& item : recovered)
  {
    TDF_Label label;
    bool found = shape_tool->SearchUsingMap(item.Shape, label, true, true);
    if (!found)
    {
      const TDF_Label main_label = shape_tool->FindMainShapeUsingMap(item.Shape);
      if (!OcctSharpGenerated::DocumentAdapters::TDF_Label_IsNull(main_label))
        found = shape_tool->AddSubShape(main_label, item.Shape, label) || !OcctSharpGenerated::DocumentAdapters::TDF_Label_IsNull(label);
    }
    if (!found)
    {
      // Style binders can have an occurrence location different from the
      // product definition. Recover only a unique exact native partner in
      // the transferred definitions; never add recovery-only scene geometry.
      if (!targets_built)
      {
        NCollection_Sequence<TDF_Label> definitions;
        shape_tool->GetShapes(definitions);
        for (const TDF_Label& part : definitions)
        {
          if (shape_tool->IsAssembly(part) || (static_cast<void>(shape_tool),OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_IsReference(part))) continue;
          const TopoDS_Shape part_shape = (static_cast<void>(shape_tool),OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetShape(part));
          if (part_shape.IsNull()) continue;
          NCollection_IndexedMap<TopoDS_Shape, TopTools_ShapeMapHasher> map;
          TopExp::MapShapes(part_shape, map);
          for (int32_t index = 1; index <= map.Extent(); ++index)
            exact_targets[map(index).TShape().get()].push_back({part, map(index)});
        }
        targets_built = true;
      }
      const auto targets = exact_targets.find(item.Shape.TShape().get());
      if (targets != exact_targets.end())
      {
        const StyleTarget* unique = nullptr;
        size_t matches = 0;
        for (const StyleTarget& target : targets->second)
          if (target.Shape.IsSame(item.Shape)) { unique = &target; ++matches; }
        if (matches == 0 && targets->second.size() == 1) unique = &targets->second.front();
        if (unique != nullptr && matches <= 1)
        {
          if (unique->Shape.IsSame((static_cast<void>(shape_tool),OcctSharpGenerated::XdeAdapters::XCAFDoc_ShapeTool_GetShape(unique->Part))))
            label = unique->Part;
          else shape_tool->AddSubShape(unique->Part, unique->Shape, label);
          found = !OcctSharpGenerated::DocumentAdapters::TDF_Label_IsNull(label);
        }
      }
    }
    if (!found) continue;

    occ::handle<StepVisual_Colour> surface_color;
    occ::handle<StepVisual_Colour> boundary_color;
    occ::handle<StepVisual_Colour> curve_color;
    STEPConstruct_RenderingProperties rendering;
    bool is_component = false;
    const bool has_color = style_reader.GetColors(
      item.Style, surface_color, boundary_color, curve_color, rendering, is_component);
    bool is_visible = true;
    for (int32_t index = 1; index <= invisible_styles->Length(); ++index)
    {
      if (invisible_styles->Value(index) == item.Style)
      {
        is_visible = false;
        break;
      }
    }
    if (!has_color && is_visible) continue;

    Quantity_Color decoded;
    if (!surface_color.IsNull() && STEPConstruct_Styles::DecodeColor(surface_color, decoded))
      color_tool->SetColor(label, Quantity_ColorRGBA(decoded), XCAFDoc_ColorSurf);
    if (rendering.IsDefined())
      color_tool->SetColor(label, rendering.GetRGBAColor(), XCAFDoc_ColorSurf);
    if (!boundary_color.IsNull() && STEPConstruct_Styles::DecodeColor(boundary_color, decoded))
      color_tool->SetColor(label, decoded, XCAFDoc_ColorCurv);
    if (!curve_color.IsNull() && STEPConstruct_Styles::DecodeColor(curve_color, decoded))
      color_tool->SetColor(label, decoded, XCAFDoc_ColorCurv);
    if (!is_visible) color_tool->SetVisibility(label, false);
  }
}

void ConfigureXdeWriter(
  STEPCAFControl_Writer& writer,
  const bool write_names,
  const bool write_colors,
  const bool write_layers,
  const bool write_validation_properties,
  const bool write_materials,
  const bool write_gdt)
{
  writer.SetColorMode(write_colors);
  writer.SetNameMode(write_names);
  writer.SetLayerMode(write_layers);
  writer.SetPropsMode(write_validation_properties);
  writer.SetMetadataMode(true);
  writer.SetSHUOMode(true);
  writer.SetDimTolMode(write_gdt);
  writer.SetMaterialMode(write_materials);
  writer.SetVisualMaterialMode(true);
}

std::vector<TDF_Label> ImportStepRootsIntoXdeDocument(
  const char* file_path,
  const occ::handle<TDocStd_Document>& output_document)
{
  ValidatePath(file_path);
  occ::handle<TDocStd_Document> source_document = CreateXdeDocument();
  InitializeXdeTools(source_document);
  STEPCAFControl_Reader reader;
  ConfigureXdeReader(reader);
  if (ReadXdeStepFile(reader, file_path) != IFSelect_RetDone)
    throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not read a STEP input through STEPCAF.");
  PreTransferStepStyleTargets(reader);
  if (!reader.Transfer(source_document))
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "A STEP input could not be transferred into an XDE document.");
  RecoverStepPresentationStyles(reader, source_document);

  return CloneXdeRootsIntoDocument(source_document, output_document, "STEP");
}

void ConfigureXdeIgesReader(
  IGESCAFControl_Reader& reader,
  const bool read_names,
  const bool read_colors,
  const bool read_layers)
{
  OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Reader_SetNameMode_Bind(reader)(read_names);
  OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Reader_SetColorMode_Bind(reader)(read_colors);
  OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Reader_SetLayerMode_Bind(reader)(read_layers);
}

occ::handle<TDocStd_Document> ReadIgesXdeDocument(
  const char* file_path,
  const bool read_names,
  const bool read_colors,
  const bool read_layers,
  OcctSharp_IgesReadReport* report)
{
  ValidatePath(file_path);
  occ::handle<TDocStd_Document> document = CreateXdeDocument();
  InitializeXdeTools(document);
  IGESCAFControl_Reader reader = OcctSharpGenerated::MeshExchangeAdapters::ConstructIGESCAFControlReader();
  ConfigureXdeIgesReader(reader, read_names, read_colors, read_layers);
  const IFSelect_ReturnStatus status = OcctSharpGenerated::StepAdapters::XSControl_Reader_ReadFile_Bind(reader)(file_path);
  if (status != IFSelect_RetDone)
  {
    const std::string message =
      "OCCT could not read IGES into XDE; status " + std::to_string(static_cast<int>(status)) + ".";
    throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, message.c_str());
  }
  if (report != nullptr)
  {
    const occ::handle<IGESData_IGESModel> model = OcctSharpGenerated::MeshExchangeAdapters::IGESControl_Reader_IGESModel_Bind(reader)();
    report->source_entity_count = model.IsNull() ? 0 : model->NbEntities();
    report->candidate_root_count = OcctSharpGenerated::MeshExchangeAdapters::IGESControl_Reader_NbRootsForTransfer_Bind(reader)();
    report->source_length_unit_meters = model.IsNull() ? 0.0 : OcctSharpGenerated::MeshExchangeAdapters::IGESData_GlobalSection_UnitValue_Bind(OcctSharpGenerated::MeshExchangeAdapters::IGESData_IGESModel_GlobalSection_Bind(*(model))())() * OcctSharpGenerated::MeshExchangeAdapters::IGESData_GlobalSection_CascadeUnit_Bind(OcctSharpGenerated::MeshExchangeAdapters::IGESData_IGESModel_GlobalSection_Bind(*(model))())() * 0.001;
    report->system_length_unit_millimeters = UnitsMethods::GetCasCadeLengthUnit();
  }
  if (!OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Reader_Transfer_Bind(reader)(document))
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer IGES into the XDE document.");
  NCollection_Sequence<TDF_Label> roots;
  XCAFDoc_DocumentTool::ShapeTool(document->Main())->GetFreeShapes(roots);
  if (report != nullptr) report->transferred_root_count = roots.Size();
  if (roots.IsEmpty())
    throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "An IGES input produced no free XDE shape roots.");
  // IGESCAF may leave color/layer references tied to its transfer document internals.
  // Clone the complete root trees and metadata into the caller-owned document so every
  // returned label remains valid after the reader/work session is destroyed.
  occ::handle<TDocStd_Document> owned_document = CreateXdeDocument();
  InitializeXdeTools(owned_document);
  CloneXdeRootsIntoDocument(document, owned_document, "IGES");
  return owned_document;
}

std::vector<TDF_Label> ImportIgesRootsIntoXdeDocument(
  const char* file_path,
  const occ::handle<TDocStd_Document>& output_document)
{
  const occ::handle<TDocStd_Document> source_document =
    ReadIgesXdeDocument(file_path, true, true, true, nullptr);
  return CloneXdeRootsIntoDocument(source_document, output_document, "IGES");
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_import_step(
  OcctSharp_OcafDocumentHandle* document, const char* file_path, int32_t* out_root_count)
{
  if (out_root_count == nullptr)
  {
    SetLastError("The imported STEP root-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_root_count = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    std::vector<TDF_Label> roots = ImportStepRootsIntoXdeDocument(file_path, document->Document);
    GetXdeShapeTool(document)->UpdateAssemblies();
    *out_root_count = static_cast<int32_t>(roots.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_import_iges(
  OcctSharp_OcafDocumentHandle* document, const char* file_path, int32_t* out_root_count)
{
  if (out_root_count == nullptr)
  {
    SetLastError("The imported IGES root-count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_root_count = 0;
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    std::vector<TDF_Label> roots = ImportIgesRootsIntoXdeDocument(file_path, document->Document);
    GetXdeShapeTool(document)->UpdateAssemblies();
    *out_root_count = static_cast<int32_t>(roots.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_open(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    ValidatePath(file_path);
    opencascade::handle<TDocStd_Application> application = new TDocStd_Application();
    ConfigureSafeNamedDataDrivers(application);
    opencascade::handle<TDocStd_Document> document;
    const PCDM_ReaderStatus status = application->Open(
      TCollection_ExtendedString(file_path, true), document);
    if (status != PCDM_RS_OK || document.IsNull())
    {
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not open the binary XDE document.");
    }
    document->SetUndoLimit(10);
    InitializeXdeTools(document);
    *out_document = AllocateValue(
      new OcctSharp_OcafDocumentHandle(std::move(application), std::move(document)),
      LiveOcafDocuments);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_step(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  return occtsharp_xde_document_read_step_options(
    file_path, 1, 1, 1, 1, 1, 1, 1, out_document);
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_iges(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  OcctSharp_IgesReadReport report{};
  return occtsharp_xde_document_read_iges_options(
    file_path, 1, 1, 1, &report, out_document);
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_iges_options(
  const char* file_path,
  const int32_t read_names,
  const int32_t read_colors,
  const int32_t read_layers,
  OcctSharp_IgesReadReport* out_report,
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr || out_report == nullptr)
  {
    SetLastError("The output IGES XDE document or report pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  *out_report = {};
  return Guard([&]
  {
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (!is_flag(read_names) || !is_flag(read_colors) || !is_flag(read_layers))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE IGES read option is not Boolean.");
    OcctSharp_OcafDocumentHandle* result = CreateOwnedXdeDocument();
    try
    {
      const occ::handle<TDocStd_Document> source = ReadIgesXdeDocument(
        file_path, read_names != 0, read_colors != 0, read_layers != 0, out_report);
      CloneXdeRootsIntoDocument(source, result->Document, "IGES");
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_gltf(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  occ::handle<DEGLTF_ConfigurationNode> node = new DEGLTF_ConfigurationNode();
  DEGLTF_Provider provider(node);
  return ReadXdeMeshDocument(file_path, out_document, provider, "OCCT could not transfer glTF/GLB into an XDE scene.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_obj(
  const char* file_path, OcctSharp_OcafDocumentHandle** out_document)
{
  occ::handle<DEOBJ_ConfigurationNode> node = new DEOBJ_ConfigurationNode();
  DEOBJ_Provider provider(node);
  return ReadXdeMeshDocument(file_path, out_document, provider, "OCCT could not transfer OBJ into an XDE scene.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_read_step_options(
  const char* file_path,
  const int32_t read_names,
  const int32_t read_colors,
  const int32_t read_layers,
  const int32_t read_validation_properties,
  const int32_t read_materials,
  const int32_t read_gdt,
  const int32_t read_views,
  OcctSharp_OcafDocumentHandle** out_document)
{
  if (out_document == nullptr)
  {
    SetLastError("The output XDE document pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_document = nullptr;
  return Guard([&]
  {
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (!is_flag(read_names) || !is_flag(read_colors) || !is_flag(read_layers)
        || !is_flag(read_validation_properties) || !is_flag(read_materials)
        || !is_flag(read_gdt) || !is_flag(read_views))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE STEP read option is not Boolean.");
    ValidatePath(file_path);
    OcctSharp_OcafDocumentHandle* result = CreateOwnedXdeDocument();
    try
    {
      STEPCAFControl_Reader reader;
      ConfigureXdeReader(
        reader,
        read_names != 0,
        read_colors != 0,
        read_layers != 0,
        read_validation_properties != 0,
        read_materials != 0,
        read_gdt != 0,
        read_views != 0);
      if (ReadXdeStepFile(reader, file_path) != IFSelect_RetDone)
      {
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not read STEP into the XDE document.");
      }
      if (read_colors != 0) PreTransferStepStyleTargets(reader);
      if (!reader.Transfer(result->Document))
      {
        throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer STEP into the XDE document.");
      }
      if (read_colors != 0) RecoverStepPresentationStyles(reader, result->Document);
      *out_document = result;
    }
    catch (...)
    {
      occtsharp_ocaf_document_release(result);
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  return occtsharp_xde_document_write_step_options(
    document, file_path, 0, 4, 1, 1, 1, 1, 1, 1);
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_iges(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  return occtsharp_xde_document_write_iges_options(document, file_path, 1, 1, 1);
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_iges_options(
  const OcctSharp_OcafDocumentHandle* document,
  const char* file_path,
  const int32_t write_names,
  const int32_t write_colors,
  const int32_t write_layers)
{
  return Guard([&]
  {
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (!is_flag(write_names) || !is_flag(write_colors) || !is_flag(write_layers))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE IGES write option is not Boolean.");
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE transaction must be closed before IGES export.");
    IGESCAFControl_Writer writer = OcctSharpGenerated::MeshExchangeAdapters::ConstructIGESCAFControlWriter();
    OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Writer_SetNameMode_Bind(writer)(write_names != 0);
    OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Writer_SetColorMode_Bind(writer)(write_colors != 0);
    OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Writer_SetLayerMode_Bind(writer)(write_layers != 0);
    const auto exportDocument = CreateIgesExportCopy(document->Document);
    if (!OcctSharpGenerated::MeshExchangeAdapters::IGESCAFControl_Writer_Perform_Bind(writer)(exportDocument, file_path))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT could not write the XDE document as IGES.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_step_options(
  const OcctSharp_OcafDocumentHandle* document,
  const char* file_path,
  const int32_t model_type,
  const int32_t schema,
  const int32_t write_names,
  const int32_t write_colors,
  const int32_t write_layers,
  const int32_t write_validation_properties,
  const int32_t write_materials,
  const int32_t write_gdt)
{
  return Guard([&]
  {
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (model_type < static_cast<int32_t>(STEPControl_AsIs)
        || model_type > static_cast<int32_t>(STEPControl_Hybrid))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP model type is outside the supported range.");
    if (schema < static_cast<int32_t>(DESTEP_Parameters::WriteMode_StepSchema_AP214CD)
        || schema > static_cast<int32_t>(DESTEP_Parameters::WriteMode_StepSchema_AP242DIS))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The STEP schema is outside the supported range.");
    if (!is_flag(write_names) || !is_flag(write_colors) || !is_flag(write_layers)
        || !is_flag(write_validation_properties) || !is_flag(write_materials)
        || !is_flag(write_gdt))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE STEP write option is not Boolean.");
    ValidateOcafDocument(document);
    ValidatePath(file_path);
    if (document->Document->HasOpenCommand())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE transaction must be closed before STEP export.");
    }
    STEPCAFControl_Writer writer;
    ConfigureXdeWriter(
      writer,
      write_names != 0,
      write_colors != 0,
      write_layers != 0,
      write_validation_properties != 0,
      write_materials != 0,
      write_gdt != 0);
    DESTEP_Parameters parameters;
    OcctSharpGenerated::StepAdapters::DESTEP_Parameters_InitFromStatic_Bind(parameters)();
    parameters.WriteSchema = static_cast<DESTEP_Parameters::WriteMode_StepSchema>(schema);
    if (!OcctSharpGenerated::XdeAdapters::STEPCAFControl_Writer_Transfer_Bind(writer)(document->Document, parameters, static_cast<STEPControl_StepModelType>(model_type)))
    {
      throw OperationFailure(OCCTSHARP_STATUS_TRANSFER_FAILED, "OCCT could not transfer the XDE document to STEP.");
    }
    const IFSelect_ReturnStatus writeStatus = writer.Write(file_path);
    if (writeStatus != IFSelect_RetDone)
    {
      const std::string message =
        "OCCT wrote the XDE STEP document with non-success status "
        + std::to_string(static_cast<int>(writeStatus)) + ".";
      throw OperationFailure(
        OCCTSHARP_STATUS_FILE_IO_ERROR,
        message.c_str());
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_gltf(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEGLTF_ConfigurationNode> node = new DEGLTF_ConfigurationNode();
  DEGLTF_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT glTF/GLB scene export failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_obj(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEOBJ_ConfigurationNode> node = new DEOBJ_ConfigurationNode();
  DEOBJ_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT OBJ scene export failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_ply(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEPLY_ConfigurationNode> node = new DEPLY_ConfigurationNode();
  DEPLY_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT PLY scene export failed.");
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_document_write_vrml(
  const OcctSharp_OcafDocumentHandle* document, const char* file_path)
{
  occ::handle<DEVRML_ConfigurationNode> node = new DEVRML_ConfigurationNode();
  DEVRML_Provider provider(node);
  return WriteXdeMeshDocument(document, file_path, provider, "OCCT VRML scene export failed.");
}

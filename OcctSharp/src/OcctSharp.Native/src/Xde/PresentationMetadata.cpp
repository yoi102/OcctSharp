#include "../../generated/Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
#include "../../generated/Documents/OcctSharp.Documents.DocumentAdapters.Generated.hxx"
#include "Documents/Lifecycle.hxx"
#include "Runtime/Error.hxx"
#include "Xde/Document.hxx"
#include <NCollection_DataMap.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <TDataStd_Name.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_Editor.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_MaterialTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFDoc_VisMaterialTool.hxx>
#include <cmath>

using namespace OcctSharp::Native;

namespace
{
XCAFDoc_ColorType ColorChannel(const int32_t channel)
{
  switch (channel)
  {
  case 0: return XCAFDoc_ColorGen;
  case 1: return XCAFDoc_ColorSurf;
  case 2: return XCAFDoc_ColorCurv;
  default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Invalid XDE color channel.");
  }
}
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_color_channel(
  const OcctSharp_OcafDocumentHandle* document,const char* entry,const int32_t channel,
  int32_t* assigned,OcctSharp_XdeColor* color,int32_t* visible)
{
  if(assigned==nullptr||color==nullptr||visible==nullptr)
  {SetLastError("Null XDE color channel output.");return OCCTSHARP_STATUS_INVALID_ARGUMENT;}
  *assigned=0;*color={};*visible=0;
  return Guard([&]
  {
    const auto kind=ColorChannel(channel);const auto label=ResolveOcafLabel(document,entry);
    const auto tool=XCAFDoc_DocumentTool::ColorTool(document->Document->Main());
    Quantity_ColorRGBA value;
    *visible=tool->IsVisible(label)?1:0;
    if(tool->GetColor(label,kind,value))
    {*assigned=1;*color={OcctSharpGenerated::XdeAdapters::Quantity_Color_Red_Bind(OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_GetRGB_Bind(value)())(),OcctSharpGenerated::XdeAdapters::Quantity_Color_Green_Bind(OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_GetRGB_Bind(value)())(),OcctSharpGenerated::XdeAdapters::Quantity_Color_Blue_Bind(OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_GetRGB_Bind(value)())(),OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_Alpha_Bind(value)()};}
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_color_channel(
  OcctSharp_OcafDocumentHandle* document,const char* entry,const int32_t channel,
  const int32_t assigned,const OcctSharp_XdeColor color)
{
  return Guard([&]
  {
    const auto kind=ColorChannel(channel);const auto label=ResolveOcafLabel(document,entry);
    RequireOpenOcafCommand(document);
    if(assigned!=0&&assigned!=1)throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Invalid XDE color presence.");
    const auto tool=XCAFDoc_DocumentTool::ColorTool(document->Document->Main());
    if(assigned==0){tool->UnSetColor(label,kind);return;}
    for(const auto value:{color.red,color.green,color.blue,color.alpha})
      if(!std::isfinite(value)||value<0.0||value>1.0)throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Invalid XDE color channel value.");
    tool->SetColor(label,OcctSharpGenerated::XdeAdapters::ConstructQuantityColorRGBA(static_cast<float>(color.red),static_cast<float>(color.green),static_cast<float>(color.blue),static_cast<float>(color.alpha)),kind);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_visibility(
  OcctSharp_OcafDocumentHandle* document,const char* entry,const int32_t visible)
{
  return Guard([&]
  {
    const auto label=ResolveOcafLabel(document,entry);RequireOpenOcafCommand(document);
    if(visible!=0&&visible!=1)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Invalid XDE visibility value.");
    XCAFDoc_DocumentTool::ColorTool(document->Document->Main())->SetVisibility(label,visible!=0);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_copy_presentation_metadata(
  OcctSharp_OcafDocumentHandle* destination,const char* destination_entry,
  const OcctSharp_OcafDocumentHandle* source,const char* source_entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(destination);ValidateOcafDocument(source);RequireOpenOcafCommand(destination);
    const auto from=ResolveOcafLabel(source,source_entry);const auto to=ResolveOcafLabel(destination,destination_entry);
    if(from==to)throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Presentation metadata requires distinct labels.");
    const auto colors=XCAFDoc_DocumentTool::ColorTool(destination->Document->Main());
    Quantity_ColorRGBA existing;
    const auto layers=XCAFDoc_DocumentTool::LayerTool(destination->Document->Main())->GetLayers(to);
    const auto material=XCAFDoc_DocumentTool::MaterialTool(destination->Document->Main());
    const auto visual=OcctSharpGenerated::XdeAdapters::XCAFDoc_DocumentTool_VisMaterialTool(destination->Document->Main());
    if(colors->GetColor(to,XCAFDoc_ColorGen,existing)||colors->GetColor(to,XCAFDoc_ColorSurf,existing)||
       colors->GetColor(to,XCAFDoc_ColorCurv,existing)||!colors->IsVisible(to)||
       (!layers.IsNull()&&!layers->IsEmpty())||material->IsMaterial(to)||
       to.IsAttribute(XCAFDoc::MaterialRefGUID())||!(static_cast<void>(visual),OcctSharpGenerated::XdeAdapters::XCAFDoc_VisMaterialTool_GetShapeMaterial(to)).IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT,"Destination label already has presentation assignments.");
    NCollection_DataMap<occ::handle<XCAFDoc_VisMaterial>,occ::handle<XCAFDoc_VisMaterial>> cloned;
    // XCAF copies Name even with generic attribute copying disabled. Preserve
    // the explicit destination name (including absence) across this operation.
    occ::handle<TDataStd_Name> targetName;
    const bool hadName=OcctSharpGenerated::DocumentAdapters::TDF_Label_FindAttribute(to,OcctSharpGenerated::DocumentAdapters::TDataStd_Name_GetID(),targetName);
    TCollection_ExtendedString name;
    if(hadName)name=OcctSharpGenerated::DocumentAdapters::TDataStd_GenericExtString_Get(*(targetName));
    OcctSharpGenerated::XdeAdapters::XCAFDoc_Editor_CloneMetaData(from,to,&cloned,true,true,true,true,false);
    if(hadName)OcctSharpGenerated::DocumentAdapters::TDataStd_Name_Set(to,name);else OcctSharpGenerated::DocumentAdapters::TDF_Label_ForgetAttribute(to,OcctSharpGenerated::DocumentAdapters::TDataStd_Name_GetID());
    const auto sourceColors=XCAFDoc_DocumentTool::ColorTool(source->Document->Main());
    colors->SetVisibility(to,sourceColors->IsVisible(from));
  });
}

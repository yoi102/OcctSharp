// Native Xde/Metadata implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Foundation/Text.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Metadata.hxx"
#include <Quantity_Color.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_HAsciiString.hxx>
#include <TDataStd_Name.hxx>
#include <TDataStd_TreeNode.hxx>
#include <TopLoc_Location.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_Area.hxx>
#include <XCAFDoc_Centroid.hxx>
#include <XCAFDoc_Color.hxx>
#include <XCAFDoc_ColorTool.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_LayerTool.hxx>
#include <XCAFDoc_MaterialTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFDoc_VisMaterialPBR.hxx>
#include <XCAFDoc_VisMaterialTool.hxx>
#include <XCAFDoc_Volume.hxx>
#include <XCAFPrs.hxx>
#include <XCAFPrs_Style.hxx>
#include <cmath>
#include <gp_Pnt.hxx>
#include <string>

namespace OcctSharp::Native
{
XdePresentationStyleMap CollectXdePresentationStyles(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry)
{
  XdePresentationStyleMap settings;
  XCAFPrs::CollectStyleSettings(
    ResolveOcafLabel(document, entry), TopLoc_Location(), settings);
  return settings;
}

OcctSharp_XdeColor CopyXdeColor(const Quantity_ColorRGBA& color)
{
  return {
    color.GetRGB().Red(), color.GetRGB().Green(), color.GetRGB().Blue(), color.Alpha()};
}

OcctSharp_XdeColor CopyXdeColor(const Quantity_Color& color, const double alpha)
{
  return {color.Red(), color.Green(), color.Blue(), alpha};
}

OcctSharp_XdePresentationStyle CopyXdePresentationStyle(const XCAFPrs_Style& style)
{
  OcctSharp_XdePresentationStyle result{};
  result.is_visible = style.IsVisible() ? 1 : 0;
  if (style.IsSetColorSurf())
  {
    result.has_surface_color = 1;
    result.surface_color = CopyXdeColor(style.GetColorSurfRGBA());
  }
  if (style.IsSetColorCurv())
  {
    result.has_curve_color = 1;
    result.curve_color = CopyXdeColor(style.GetColorCurv());
  }

  const opencascade::handle<XCAFDoc_VisMaterial>& material = style.Material();
  if (!material.IsNull() && material->HasPbrMaterial())
  {
    result.has_material_color = 1;
    result.material_color = CopyXdeColor(material->PbrMaterial().BaseColor);
  }
  else if (!material.IsNull() && material->HasCommonMaterial())
  {
    result.has_material_color = 1;
    const XCAFDoc_VisMaterialCommon& common = material->CommonMaterial();
    result.material_color = CopyXdeColor(common.DiffuseColor, 1.0 - common.Transparency);
  }
  return result;
}

bool GetAssignedMaterial(
  const TDF_Label& label,
  opencascade::handle<TCollection_HAsciiString>& name,
  opencascade::handle<TCollection_HAsciiString>& description,
  double& density,
  opencascade::handle<TCollection_HAsciiString>& densityName,
  opencascade::handle<TCollection_HAsciiString>& densityType)
{
  opencascade::handle<TDataStd_TreeNode> reference;
  if (!label.FindAttribute(XCAFDoc::MaterialRefGUID(), reference) || !reference->HasFather())
  {
    return false;
  }
  return XCAFDoc_MaterialTool::GetMaterial(
    reference->Father()->Label(), name, description, density, densityName, densityType);
}

std::string MaterialFieldUtf8(const TDF_Label& label, const int32_t field)
{
  opencascade::handle<TCollection_HAsciiString> name;
  opencascade::handle<TCollection_HAsciiString> description;
  opencascade::handle<TCollection_HAsciiString> densityName;
  opencascade::handle<TCollection_HAsciiString> densityType;
  double density = 0.0;
  if (!GetAssignedMaterial(label, name, description, density, densityName, densityType))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label has no material assignment.");
  }
  const opencascade::handle<TCollection_HAsciiString>* selected = nullptr;
  switch (field)
  {
    case 0: selected = &name; break;
    case 1: selected = &description; break;
    case 2: selected = &densityName; break;
    case 3: selected = &densityType; break;
    default:
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE material field index is invalid.");
  }
  return selected->IsNull() ? std::string() : std::string((*selected)->ToCString());
}

opencascade::handle<NCollection_HSequence<TCollection_ExtendedString>> GetXdeLayers(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry)
{
  const TDF_Label label = ResolveOcafLabel(document, entry);
  return XCAFDoc_DocumentTool::LayerTool(document->Document->Main())->GetLayers(label);
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_color(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_XdeColor color)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const double values[] = {color.red, color.green, color.blue, color.alpha};
    for (const double value : values)
    {
      if (!std::isfinite(value) || value < 0.0 || value > 1.0)
      {
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "XDE color channels must be finite values from zero through one.");
      }
    }
    const Quantity_ColorRGBA nativeColor(
      static_cast<float>(color.red),
      static_cast<float>(color.green),
      static_cast<float>(color.blue),
      static_cast<float>(color.alpha));
    const TDF_Label label = ResolveOcafLabel(document, entry);
    const opencascade::handle<XCAFDoc_ColorTool> colorTool =
      XCAFDoc_DocumentTool::ColorTool(document->Document->Main());
    colorTool->SetColor(label, nativeColor, XCAFDoc_ColorGen);
    colorTool->SetColor(label, nativeColor, XCAFDoc_ColorSurf);
    colorTool->SetColor(label, nativeColor, XCAFDoc_ColorCurv);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_get_color(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_color,
  OcctSharp_XdeColor* color)
{
  if (has_color == nullptr || color == nullptr)
  {
    SetLastError("An XDE color output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_color = 0;
  *color = {};
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    Quantity_ColorRGBA nativeColor;
    const auto try_color = [&](const XCAFDoc_ColorType type)
    {
      occ::handle<TDataStd_TreeNode> reference;
      if (!label.FindAttribute(XCAFDoc::ColorRefGUID(type), reference)
          || reference.IsNull() || !reference->HasFather()) return false;
      occ::handle<XCAFDoc_Color> attribute;
      const TDF_Label color_label = reference->Father()->Label();
      if (color_label.IsNull()
          || !color_label.FindAttribute(XCAFDoc_Color::GetID(), attribute)
          || attribute.IsNull()) return false;
      nativeColor = attribute->GetColorRGBA();
      return true;
    };
    if (try_color(XCAFDoc_ColorGen)
        || try_color(XCAFDoc_ColorSurf)
        || try_color(XCAFDoc_ColorCurv))
    {
      *has_color = 1;
      *color = {
        nativeColor.GetRGB().Red(), nativeColor.GetRGB().Green(),
        nativeColor.GetRGB().Blue(), nativeColor.Alpha()};
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_presentation_style_count(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE presentation-style count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    *count = CollectXdePresentationStyles(document, entry).Extent();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_presentation_style_snapshot(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_ShapeHandle** shapes,
  OcctSharp_XdePresentationStyle* styles,
  const int32_t capacity,
  int32_t* written)
{
  if (written == nullptr || capacity < 0
      || (capacity > 0 && (shapes == nullptr || styles == nullptr)))
  {
    SetLastError("The XDE presentation-style snapshot buffer is invalid.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *written = 0;
  for (int32_t index = 0; index < capacity; ++index)
  {
    shapes[index] = nullptr;
    styles[index] = {};
  }

  return Guard([&]
  {
    const XdePresentationStyleMap settings = CollectXdePresentationStyles(document, entry);
    if (settings.Extent() > capacity)
    {
      throw OperationFailure(
        OCCTSHARP_STATUS_INVALID_ARGUMENT,
        "The XDE presentation-style snapshot capacity is too small.");
    }

    int32_t allocated = 0;
    try
    {
      for (int32_t index = 1; index <= settings.Extent(); ++index)
      {
        shapes[index - 1] = AllocateShape(settings.FindKey(index));
        ++allocated;
        styles[index - 1] = CopyXdePresentationStyle(settings.FindFromIndex(index));
      }
    }
    catch (...)
    {
      for (int32_t index = 0; index < allocated; ++index)
      {
        UnregisterShape(shapes[index]);
        delete shapes[index];
        shapes[index] = nullptr;
      }
      throw;
    }
    *written = settings.Extent();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_layer(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* layer_utf8,
  const int32_t layer_length,
  const int32_t replace_existing)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    XCAFDoc_DocumentTool::LayerTool(document->Document->Main())->SetLayer(
      ResolveOcafLabel(document, entry), MakeExtendedUtf8(layer_utf8, layer_length), replace_existing != 0);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_count(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The XDE layer count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    const auto layers = GetXdeLayers(document, entry);
    *count = layers.IsNull() ? 0 : layers->Length();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  int32_t* length)
{
  if (length == nullptr)
  {
    SetLastError("The XDE layer-name length pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *length = 0;
  return Guard([&]
  {
    const auto layers = GetXdeLayers(document, entry);
    if (layers.IsNull() || index < 1 || index > layers->Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE layer index is out of range.");
    }
    *length = layers->Value(index).LengthOfCString();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_layer_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t index,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    const auto layers = GetXdeLayers(document, entry);
    if (layers.IsNull() || index < 1 || index > layers->Length())
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE layer index is out of range.");
    }
    CopyUtf8Result(ExtendedToUtf8(layers->Value(index)), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_material(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const char* name,
  const int32_t name_length,
  const char* description,
  const int32_t description_length,
  const double density,
  const char* density_name,
  const int32_t density_name_length,
  const char* density_type,
  const int32_t density_type_length)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateFinite(density, "XDE material density must be finite.");
    if (density < 0.0)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "XDE material density cannot be negative.");
    }
    ValidateUtf8Input(name, name_length);
    ValidateUtf8Input(description, description_length);
    ValidateUtf8Input(density_name, density_name_length);
    ValidateUtf8Input(density_type, density_type_length);
    auto makeString = [](const char* value, const int32_t length)
    {
      return opencascade::handle<TCollection_HAsciiString>(
        new TCollection_HAsciiString(MakeAsciiString(value, length)));
    };
    XCAFDoc_DocumentTool::MaterialTool(document->Document->Main())->SetMaterial(
      ResolveOcafLabel(document, entry),
      makeString(name, name_length),
      makeString(description, description_length),
      density,
      makeString(density_name, density_name_length),
      makeString(density_type, density_type_length));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_info(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int32_t* has_material,
  double* density)
{
  if (has_material == nullptr || density == nullptr)
  {
    SetLastError("An XDE material output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_material = 0;
  *density = 0.0;
  return Guard([&]
  {
    opencascade::handle<TCollection_HAsciiString> name;
    opencascade::handle<TCollection_HAsciiString> description;
    opencascade::handle<TCollection_HAsciiString> densityName;
    opencascade::handle<TCollection_HAsciiString> densityType;
    *has_material = GetAssignedMaterial(
      ResolveOcafLabel(document, entry), name, description, *density, densityName, densityType) ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_field_utf8_length(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t field,
  int32_t* length)
{
  if (length == nullptr)
  {
    SetLastError("The XDE material field length pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *length = 0;
  return Guard([&] { *length = static_cast<int32_t>(MaterialFieldUtf8(ResolveOcafLabel(document, entry), field).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_material_field_to_utf8(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const int32_t field,
  char* buffer,
  const int32_t capacity,
  int32_t* written)
{
  return Guard([&]
  {
    CopyUtf8Result(MaterialFieldUtf8(ResolveOcafLabel(document, entry), field), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_visual_material(
  OcctSharp_OcafDocumentHandle* document, const char* entry,
  const char* name, const int32_t name_length,
  const double red, const double green, const double blue, const double alpha,
  const double metallic, const double roughness,
  const double emissive_red, const double emissive_green, const double emissive_blue,
  const double refraction_index, const int32_t alpha_mode, const double alpha_cutoff)
{
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    ValidateUtf8Input(name, name_length);
    const double values[] = {
      red, green, blue, alpha, metallic, roughness,
      emissive_red, emissive_green, emissive_blue, refraction_index, alpha_cutoff };
    for (const double value : values)
      if (!std::isfinite(value))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A visual-material value is not finite.");
    const auto in_unit = [](const double value) { return value >= 0.0 && value <= 1.0; };
    if (!in_unit(red) || !in_unit(green) || !in_unit(blue) || !in_unit(alpha)
        || !in_unit(metallic) || !in_unit(roughness)
        || !in_unit(emissive_red) || !in_unit(emissive_green) || !in_unit(emissive_blue)
        || !in_unit(alpha_cutoff))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Visual-material colors, factors, and cutoff must be in [0,1].");
    if (refraction_index < 1.0 || refraction_index > 3.0)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Visual-material refraction index must be in [1,3].");
    if (alpha_mode < static_cast<int32_t>(Graphic3d_AlphaMode_BlendAuto)
        || alpha_mode > static_cast<int32_t>(Graphic3d_AlphaMode_MaskBlend))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The visual-material alpha mode is invalid.");

    occ::handle<XCAFDoc_VisMaterial> material = new XCAFDoc_VisMaterial();
    XCAFDoc_VisMaterialPBR pbr;
    pbr.BaseColor = Quantity_ColorRGBA(
      static_cast<float>(red), static_cast<float>(green),
      static_cast<float>(blue), static_cast<float>(alpha));
    pbr.Metallic = static_cast<float>(metallic);
    pbr.Roughness = static_cast<float>(roughness);
    pbr.EmissiveFactor = NCollection_Vec3<float>(
      static_cast<float>(emissive_red), static_cast<float>(emissive_green),
      static_cast<float>(emissive_blue));
    pbr.RefractionIndex = static_cast<float>(refraction_index);
    material->SetPbrMaterial(pbr);
    material->SetAlphaMode(
      static_cast<Graphic3d_AlphaMode>(alpha_mode), static_cast<float>(alpha_cutoff));
    const TCollection_AsciiString material_name = MakeAsciiString(name, name_length);
    material->SetRawName(new TCollection_HAsciiString(material_name));
    const occ::handle<XCAFDoc_VisMaterialTool> tool =
      XCAFDoc_DocumentTool::VisMaterialTool(document->Document->Main());
    const TDF_Label material_label = tool->AddMaterial(material, material_name);
    tool->SetShapeMaterial(ResolveOcafLabel(document, entry), material_label);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_info(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_material,
  double* red, double* green, double* blue, double* alpha,
  double* metallic, double* roughness,
  double* emissive_red, double* emissive_green, double* emissive_blue,
  double* refraction_index, int32_t* alpha_mode, double* alpha_cutoff)
{
  if (has_material == nullptr || red == nullptr || green == nullptr || blue == nullptr
      || alpha == nullptr || metallic == nullptr || roughness == nullptr
      || emissive_red == nullptr || emissive_green == nullptr || emissive_blue == nullptr
      || refraction_index == nullptr || alpha_mode == nullptr || alpha_cutoff == nullptr)
  {
    SetLastError("A visual-material output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_material = 0;
  *red = *green = *blue = *alpha = 0.0;
  *metallic = *roughness = 0.0;
  *emissive_red = *emissive_green = *emissive_blue = 0.0;
  *refraction_index = 0.0;
  *alpha_mode = 0;
  *alpha_cutoff = 0.0;
  return Guard([&]
  {
    const occ::handle<XCAFDoc_VisMaterial> material =
      XCAFDoc_VisMaterialTool::GetShapeMaterial(ResolveOcafLabel(document, entry));
    if (material.IsNull()) return;
    XCAFDoc_VisMaterialPBR pbr = material->HasPbrMaterial()
      ? material->PbrMaterial() : material->ConvertToPbrMaterial();
    if (!pbr.IsDefined) return;
    const Quantity_ColorRGBA base = pbr.BaseColor;
    *has_material = 1;
    *red = base.GetRGB().Red();
    *green = base.GetRGB().Green();
    *blue = base.GetRGB().Blue();
    *alpha = base.Alpha();
    *metallic = pbr.Metallic;
    *roughness = pbr.Roughness;
    *emissive_red = pbr.EmissiveFactor.r();
    *emissive_green = pbr.EmissiveFactor.g();
    *emissive_blue = pbr.EmissiveFactor.b();
    *refraction_index = pbr.RefractionIndex;
    *alpha_mode = static_cast<int32_t>(material->AlphaMode());
    *alpha_cutoff = material->AlphaCutOff();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_name_utf8_length(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  int32_t* has_material, int32_t* length)
{
  if (has_material == nullptr || length == nullptr)
  {
    SetLastError("A visual-material name output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *has_material = 0;
  *length = 0;
  return Guard([&]
  {
    TDF_Label material_label;
    if (!XCAFDoc_VisMaterialTool::GetShapeMaterial(
          ResolveOcafLabel(document, entry), material_label)) return;
    *has_material = 1;
    opencascade::handle<TDataStd_Name> name_attribute;
    if (material_label.FindAttribute(TDataStd_Name::GetID(), name_attribute))
      *length = name_attribute->Get().LengthOfCString();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_visual_material_name_to_utf8(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    TDF_Label material_label;
    if (!XCAFDoc_VisMaterialTool::GetShapeMaterial(
          ResolveOcafLabel(document, entry), material_label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label has no visual material.");
    opencascade::handle<TDataStd_Name> name_attribute;
    const std::string name = material_label.FindAttribute(TDataStd_Name::GetID(), name_attribute)
      ? ExtendedToUtf8(name_attribute->Get()) : std::string();
    CopyUtf8Result(name, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_validation_properties(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  OcctSharp_XdeValidationProperties* properties)
{
  if (properties == nullptr)
  {
    SetLastError("The XDE validation-properties output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *properties = {};
  return Guard([&]
  {
    const TDF_Label label = ResolveOcafLabel(document, entry);
    gp_Pnt centroid;
    properties->has_area = XCAFDoc_Area::Get(label, properties->area) ? 1 : 0;
    properties->has_volume = XCAFDoc_Volume::Get(label, properties->volume) ? 1 : 0;
    properties->has_centroid = XCAFDoc_Centroid::Get(label, centroid) ? 1 : 0;
    if (properties->has_centroid != 0)
      properties->centroid = {centroid.X(), centroid.Y(), centroid.Z()};
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_label_set_validation_properties(
  OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  const OcctSharp_XdeValidationProperties* properties)
{
  if (properties == nullptr)
  {
    SetLastError("The XDE validation-properties input pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateOcafDocument(document);
    RequireOpenOcafCommand(document);
    const auto is_flag = [](const int32_t value) { return value == 0 || value == 1; };
    if (!is_flag(properties->has_area) || !is_flag(properties->has_volume)
        || !is_flag(properties->has_centroid))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An XDE validation-property presence flag is not Boolean.");
    if ((properties->has_area != 0 && (!std::isfinite(properties->area) || properties->area < 0.0))
        || (properties->has_volume != 0 && (!std::isfinite(properties->volume) || properties->volume < 0.0)))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "XDE area and volume must be finite and non-negative.");
    if (properties->has_centroid != 0
        && (!std::isfinite(properties->centroid.x)
            || !std::isfinite(properties->centroid.y)
            || !std::isfinite(properties->centroid.z)))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE centroid must be finite.");

    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (properties->has_area != 0) XCAFDoc_Area::Set(label, properties->area);
    else label.ForgetAttribute(XCAFDoc_Area::GetID());
    if (properties->has_volume != 0) XCAFDoc_Volume::Set(label, properties->volume);
    else label.ForgetAttribute(XCAFDoc_Volume::GetID());
    if (properties->has_centroid != 0)
      XCAFDoc_Centroid::Set(
        label,
        gp_Pnt(properties->centroid.x, properties->centroid.y, properties->centroid.z));
    else label.ForgetAttribute(XCAFDoc_Centroid::GetID());
  });
}

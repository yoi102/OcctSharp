// Native Xde/Pmi implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Foundation/Text.hxx"
#include "Geometry/Conversions.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Pmi.hxx"
#include "Xde/SavedViews.hxx"
#include <NCollection_HArray1.hxx>
#include <NCollection_Sequence.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_HAsciiString.hxx>
#include <TDataStd_RealArray.hxx>
#include <TopoDS.hxx>
#include <TopoDS_Edge.hxx>
#include <TopoDS_Shape.hxx>
#include <XCAFDimTolObjects_DatumObject.hxx>
#include <XCAFDimTolObjects_DimensionObject.hxx>
#include <XCAFDimTolObjects_GeomToleranceObject.hxx>
#include <XCAFDoc.hxx>
#include <XCAFDoc_ClippingPlaneTool.hxx>
#include <XCAFDoc_Datum.hxx>
#include <XCAFDoc_DimTolTool.hxx>
#include <XCAFDoc_Dimension.hxx>
#include <XCAFDoc_DocumentTool.hxx>
#include <XCAFDoc_GeomTolerance.hxx>
#include <XCAFDoc_GraphNode.hxx>
#include <XCAFDoc_ViewTool.hxx>
#include <cmath>
#include <cstddef>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <string>
#include <vector>

namespace OcctSharp::Native
{
opencascade::handle<XCAFDoc_DimTolTool> GetDimTolTool(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::DimTolTool(document->Document->Main());
}

opencascade::handle<XCAFDoc_ViewTool> GetViewTool(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::ViewTool(document->Document->Main());
}

opencascade::handle<XCAFDoc_ClippingPlaneTool> GetClippingPlaneTool(const OcctSharp_OcafDocumentHandle* document)
{
  ValidateOcafDocument(document);
  return XCAFDoc_DocumentTool::ClippingPlaneTool(document->Document->Main());
}

opencascade::handle<TCollection_HAsciiString> MakePmiString(const char* value)
{
  return new TCollection_HAsciiString(value == nullptr ? "" : value);
}

std::string CopyPmiString(const opencascade::handle<TCollection_HAsciiString>& value)
{
  return value.IsNull() ? std::string() : std::string(value->ToCString());
}

std::vector<std::string> SplitEntries(const char* entries)
{
  std::vector<std::string> result;
  if (entries == nullptr || entries[0] == '\0') return result;
  std::string value(entries);
  size_t start = 0;
  while (start <= value.size())
  {
    const size_t end = value.find('\n', start);
    const std::string item = value.substr(start, end == std::string::npos ? std::string::npos : end - start);
    if (!item.empty()) result.push_back(item);
    if (end == std::string::npos) break;
    start = end + 1;
  }
  return result;
}

NCollection_Sequence<TDF_Label> ResolveEntries(
  const OcctSharp_OcafDocumentHandle* document, const char* entries)
{
  NCollection_Sequence<TDF_Label> result;
  for (const std::string& entry : SplitEntries(entries)) result.Append(ResolveOcafLabel(document, entry.c_str()));
  return result;
}

std::vector<TDF_Label> PmiLabels(const OcctSharp_OcafDocumentHandle* document, const int32_t kind)
{
  NCollection_Sequence<TDF_Label> labels;
  if (kind == 0) GetDimTolTool(document)->GetDimensionLabels(labels);
  else if (kind == 1) GetDimTolTool(document)->GetGeomToleranceLabels(labels);
  else if (kind == 2) GetDimTolTool(document)->GetDatumLabels(labels);
  else if (kind == 3) GetViewTool(document)->GetViewLabels(labels);
  else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI item kind is outside the supported range.");
  std::vector<TDF_Label> result;
  result.reserve(static_cast<size_t>(labels.Size()));
  for (NCollection_Sequence<TDF_Label>::Iterator iterator(labels); iterator.More(); iterator.Next())
    result.push_back(iterator.Value());
  return result;
}

opencascade::handle<XCAFDimTolObjects_DimensionObject> GetDimensionObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_Dimension> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_Dimension::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a semantic dimension.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The semantic dimension has no value object.");
  return object;
}

opencascade::handle<XCAFDimTolObjects_GeomToleranceObject> GetToleranceObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_GeomTolerance> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_GeomTolerance::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a geometric tolerance.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The geometric tolerance has no value object.");
  return object;
}

opencascade::handle<XCAFDimTolObjects_DatumObject> GetDatumObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_Datum> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_Datum::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a datum.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The datum has no value object.");
  opencascade::handle<TDataStd_RealArray> point;
  if (label.FindChild(17, false).FindAttribute(TDataStd_RealArray::GetID(), point)
      && point->Length() == 3)
  {
    const int32_t lower = point->Lower();
    object->SetPoint(gp_Pnt(point->Value(lower), point->Value(lower + 1), point->Value(lower + 2)));
  }
  return object;
}

void SetDimensionObject(
  const TDF_Label& label, const OcctSharp_PmiDimension& data,
  const double* values, const int32_t valueCount,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName)
{
  ValidateArray(values, valueCount, "The dimension value array is invalid.");
  ValidateArray(modifiers, modifierCount, "The dimension modifier array is invalid.");
  if (valueCount <= 0) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A semantic dimension requires at least one value.");
  auto object = new XCAFDimTolObjects_DimensionObject();
  object->SetType(static_cast<XCAFDimTolObjects_DimensionType>(data.type));
  auto valueArray = new NCollection_HArray1<double>(1, valueCount);
  for (int32_t index = 0; index < valueCount; ++index)
  {
    if (!std::isfinite(values[index])) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Dimension values must be finite.");
    valueArray->SetValue(index + 1, values[index]);
  }
  object->SetValues(valueArray);
  if (data.has_qualifier) object->SetQualifier(static_cast<XCAFDimTolObjects_DimensionQualifier>(data.qualifier));
  if (data.has_angular_qualifier) object->SetAngularQualifier(static_cast<XCAFDimTolObjects_AngularQualifier>(data.angular_qualifier));
  if (data.has_class_of_tolerance)
    object->SetClassOfTolerance(data.is_hole != 0,
      static_cast<XCAFDimTolObjects_DimensionFormVariance>(data.form_variance),
      static_cast<XCAFDimTolObjects_DimensionGrade>(data.grade));
  object->SetNbOfDecimalPlaces(data.left_decimal_places, data.right_decimal_places);
  NCollection_Sequence<XCAFDimTolObjects_DimensionModif> modifierValues;
  for (int32_t index = 0; index < modifierCount; ++index)
    modifierValues.Append(static_cast<XCAFDimTolObjects_DimensionModif>(modifiers[index]));
  object->SetModifiers(modifierValues);
  if (data.has_direction) object->SetDirection(ToDirection(data.direction));
  if (data.has_plane) object->SetPlane(ToAxis(data.plane));
  if (data.has_first_point) object->SetPoint(ToPoint(data.first_point));
  if (data.has_second_point) object->SetPoint2(ToPoint(data.second_point));
  if (data.has_text_point) object->SetPointTextAttach(ToPoint(data.text_point));
  object->SetSemanticName(MakePmiString(semanticName));
  object->SetPresentation(TopoDS_Shape(), MakePmiString(presentationName));
  if ((description != nullptr && description[0] != '\0') || (descriptionName != nullptr && descriptionName[0] != '\0'))
    object->AddDescription(MakePmiString(description), MakePmiString(descriptionName));
  XCAFDoc_Dimension::Set(label)->SetObject(object);
}

void SetToleranceObject(
  const TDF_Label& label, const OcctSharp_PmiTolerance& data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName)
{
  ValidateArray(modifiers, modifierCount, "The tolerance modifier array is invalid.");
  if (!std::isfinite(data.value) || !std::isfinite(data.zone_modifier_value) || !std::isfinite(data.maximum_value_modifier))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tolerance values must be finite.");
  auto object = new XCAFDimTolObjects_GeomToleranceObject();
  object->SetType(static_cast<XCAFDimTolObjects_GeomToleranceType>(data.type));
  object->SetTypeOfValue(static_cast<XCAFDimTolObjects_GeomToleranceTypeValue>(data.type_of_value));
  object->SetValue(data.value);
  object->SetMaterialRequirementModifier(static_cast<XCAFDimTolObjects_GeomToleranceMatReqModif>(data.material_requirement));
  object->SetZoneModifier(static_cast<XCAFDimTolObjects_GeomToleranceZoneModif>(data.zone_modifier));
  object->SetValueOfZoneModifier(data.zone_modifier_value);
  object->SetMaxValueModifier(data.maximum_value_modifier);
  NCollection_Sequence<XCAFDimTolObjects_GeomToleranceModif> modifierValues;
  for (int32_t index = 0; index < modifierCount; ++index)
    modifierValues.Append(static_cast<XCAFDimTolObjects_GeomToleranceModif>(modifiers[index]));
  object->SetModifiers(modifierValues);
  if (data.has_axis) object->SetAxis(ToAxis(data.axis));
  if (data.has_plane) object->SetPlane(ToAxis(data.plane));
  if (data.has_point) object->SetPoint(ToPoint(data.point));
  if (data.has_text_point) object->SetPointTextAttach(ToPoint(data.text_point));
  if (data.affected_plane_type != 0)
    object->SetAffectedPlane(ToPlane(data.affected_plane),
      static_cast<XCAFDimTolObjects_ToleranceZoneAffectedPlane>(data.affected_plane_type));
  object->SetSemanticName(MakePmiString(semanticName));
  object->SetPresentation(TopoDS_Shape(), MakePmiString(presentationName));
  XCAFDoc_GeomTolerance::Set(label)->SetObject(object);
}

void SetDatumObject(
  const TDF_Label& label, const OcctSharp_PmiDatum& data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName)
{
  ValidateArray(modifiers, modifierCount, "The datum modifier array is invalid.");
  auto object = new XCAFDimTolObjects_DatumObject();
  object->SetName(MakePmiString(name));
  object->SetSemanticName(MakePmiString(semanticName));
  object->SetPosition(data.position);
  object->IsDatumTarget(data.is_datum_target != 0);
  object->SetDatumTargetType(static_cast<XCAFDimTolObjects_DatumTargetType>(data.target_type));
  object->SetDatumTargetLength(data.target_length);
  object->SetDatumTargetWidth(data.target_width);
  object->SetDatumTargetNumber(data.target_number);
  NCollection_Sequence<XCAFDimTolObjects_DatumSingleModif> modifierValues;
  for (int32_t index = 0; index < modifierCount; ++index)
    modifierValues.Append(static_cast<XCAFDimTolObjects_DatumSingleModif>(modifiers[index]));
  object->SetModifiers(modifierValues);
  if (data.has_modifier_with_value)
    object->SetModifierWithValue(static_cast<XCAFDimTolObjects_DatumModifWithValue>(data.modifier_with_value), data.modifier_value);
  if (data.has_target_axis) object->SetDatumTargetAxis(ToAxis(data.target_axis));
  if (data.has_plane) object->SetPlane(ToAxis(data.plane));
  if (data.has_point) object->SetPoint(ToPoint(data.point));
  if (data.has_text_point) object->SetPointTextAttach(ToPoint(data.text_point));
  object->SetPresentation(TopoDS_Shape(), MakePmiString(presentationName));
  auto attribute = XCAFDoc_Datum::Set(label, MakePmiString(name), MakePmiString(description), MakePmiString(identification));
  attribute->SetObject(object);
}

std::string PmiText(const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry, const int32_t field)
{
  if (kind == 0)
  {
    const auto object = GetDimensionObject(document, entry);
    if (field == 0) return CopyPmiString(object->GetSemanticName());
    if (field == 1) return CopyPmiString(object->GetPresentationName());
    if (field == 2) return object->HasDescriptions() ? CopyPmiString(object->GetDescription(0)) : std::string();
    if (field == 3) return object->HasDescriptions() ? CopyPmiString(object->GetDescriptionName(0)) : std::string();
  }
  else if (kind == 1)
  {
    const auto object = GetToleranceObject(document, entry);
    if (field == 0) return CopyPmiString(object->GetSemanticName());
    if (field == 1) return CopyPmiString(object->GetPresentationName());
  }
  else if (kind == 2)
  {
    opencascade::handle<XCAFDoc_Datum> attribute;
    ResolveOcafLabel(document, entry).FindAttribute(XCAFDoc_Datum::GetID(), attribute);
    const auto object = GetDatumObject(document, entry);
    if (field == 0) return CopyPmiString(attribute->GetName());
    if (field == 1) return CopyPmiString(attribute->GetDescription());
    if (field == 2) return CopyPmiString(attribute->GetIdentification());
    if (field == 3) return CopyPmiString(object->GetSemanticName());
    if (field == 4) return CopyPmiString(object->GetPresentationName());
  }
  else if (kind == 3)
  {
    const auto object = GetSavedViewObject(document, entry);
    if (field == 0) return CopyPmiString(object->Name());
    if (field == 1) return CopyPmiString(object->ClippingExpression());
  }
  throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI text field is unsupported.");
}

NCollection_Sequence<TDF_Label> ReferenceLabels(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry)
{
  NCollection_Sequence<TDF_Label> first;
  NCollection_Sequence<TDF_Label> second;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (relation == 0 || relation == 1 || relation == 2 || relation == 4)
  {
    XCAFDoc_DimTolTool::GetRefShapeLabel(label, first, second);
    if (relation == 1) return second;
    return first;
  }
  if (relation == 3) XCAFDoc_DimTolTool::GetDatumOfTolerLabels(label, first);
  else if (relation == 5) GetDimTolTool(document)->GetTolerOfDatumLabels(label, first);
  else if (relation == 6)
  {
    const auto viewTool = GetViewTool(document);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    viewTool->GetRefShapeLabel(label, first);
  }
  else if (relation == 7)
  {
    const auto viewTool = GetViewTool(document);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    viewTool->GetRefGDTLabel(label, first);
  }
  else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI reference relation is unsupported.");
  return first;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_count(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, int32_t* count)
{
  if (count == nullptr) { SetLastError("The PMI count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&] { *count = static_cast<int32_t>(PmiLabels(document, kind).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_entry(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const int32_t index,
  char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const std::vector<TDF_Label> labels = PmiLabels(document, kind);
    if (index < 1 || index > static_cast<int32_t>(labels.size()))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI index is outside the valid 1-based range.");
    CopyLabelEntry(labels[static_cast<size_t>(index - 1)], buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiDimension* data,
  const double* values, const int32_t valueCount, const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr) { SetLastError("The dimension data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = GetDimTolTool(document)->AddDimension();
    SetDimensionObject(label, *data, values, valueCount, modifiers, modifierCount,
      semanticName, presentationName, description, descriptionName);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiDimension* data,
  const double* values, const int32_t valueCount, const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName)
{
  if (data == nullptr) { SetLastError("The dimension data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!GetDimTolTool(document)->IsDimension(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a semantic dimension.");
    const auto previous = GetDimensionObject(document, entry);
    SetDimensionObject(label, *data, values, valueCount, modifiers, modifierCount,
      semanticName, presentationName, description, descriptionName);
    const auto updated = GetDimensionObject(document, entry);
    updated->SetPath(previous->GetPath());
    updated->SetPresentation(previous->GetPresentation(), previous->GetPresentationName());
    XCAFDoc_Dimension::Set(label)->SetObject(updated);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiTolerance* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr) { SetLastError("The tolerance data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = GetDimTolTool(document)->AddGeomTolerance();
    SetToleranceObject(label, *data, modifiers, modifierCount, semanticName, presentationName);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiTolerance* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName)
{
  if (data == nullptr) { SetLastError("The tolerance data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!GetDimTolTool(document)->IsGeomTolerance(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a geometric tolerance.");
    const auto previous = GetToleranceObject(document, entry);
    SetToleranceObject(label, *data, modifiers, modifierCount, semanticName, presentationName);
    const auto updated = GetToleranceObject(document, entry);
    updated->SetPresentation(previous->GetPresentation(), previous->GetPresentationName());
    XCAFDoc_GeomTolerance::Set(label)->SetObject(updated);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_PmiDatum* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr) { SetLastError("The datum data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = GetDimTolTool(document)->AddDatum();
    SetDatumObject(label, *data, modifiers, modifierCount, name, description, identification, semanticName, presentationName);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_PmiDatum* data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName)
{
  if (data == nullptr) { SetLastError("The datum data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!GetDimTolTool(document)->IsDatum(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a datum.");
    const auto previous = GetDatumObject(document, entry);
    SetDatumObject(label, *data, modifiers, modifierCount, name, description, identification, semanticName, presentationName);
    const auto updated = GetDatumObject(document, entry);
    updated->SetDatumTarget(previous->GetDatumTarget());
    updated->SetPresentation(previous->GetPresentation(), previous->GetPresentationName());
    XCAFDoc_Datum::Set(label)->SetObject(updated);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_dimension_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_PmiDimension* data, int32_t* valueCount, int32_t* modifierCount)
{
  if (data == nullptr || valueCount == nullptr || modifierCount == nullptr)
  { SetLastError("A dimension snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *valueCount = 0; *modifierCount = 0;
  return Guard([&]
  {
    const auto object = GetDimensionObject(document, entry);
    data->type = static_cast<int32_t>(object->GetType());
    data->has_qualifier = object->HasQualifier() ? 1 : 0;
    if (data->has_qualifier) data->qualifier = static_cast<int32_t>(object->GetQualifier());
    data->has_angular_qualifier = object->HasAngularQualifier() ? 1 : 0;
    if (data->has_angular_qualifier) data->angular_qualifier = static_cast<int32_t>(object->GetAngularQualifier());
    bool isHole = false;
    XCAFDimTolObjects_DimensionFormVariance variance;
    XCAFDimTolObjects_DimensionGrade grade;
    data->has_class_of_tolerance = object->GetClassOfTolerance(isHole, variance, grade) ? 1 : 0;
    data->is_hole = isHole ? 1 : 0;
    data->form_variance = static_cast<int32_t>(variance);
    data->grade = static_cast<int32_t>(grade);
    object->GetNbOfDecimalPlaces(data->left_decimal_places, data->right_decimal_places);
    gp_Dir direction;
    data->has_direction = object->GetDirection(direction) ? 1 : 0;
    if (data->has_direction) data->direction = { direction.X(), direction.Y(), direction.Z() };
    data->has_plane = object->HasPlane() ? 1 : 0;
    if (data->has_plane) data->plane = CopyAxis(object->GetPlane());
    data->has_first_point = object->HasPoint() ? 1 : 0;
    if (data->has_first_point) data->first_point = CopyPoint(object->GetPoint());
    data->has_second_point = object->HasPoint2() ? 1 : 0;
    if (data->has_second_point) data->second_point = CopyPoint(object->GetPoint2());
    data->has_text_point = 1;
    data->text_point = CopyPoint(object->GetPointTextAttach());
    const auto values = object->GetValues();
    *valueCount = values.IsNull() ? 0 : values->Length();
    *modifierCount = object->GetModifiers().Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_tolerance_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_PmiTolerance* data, int32_t* modifierCount)
{
  if (data == nullptr || modifierCount == nullptr)
  { SetLastError("A tolerance snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *modifierCount = 0;
  return Guard([&]
  {
    const auto object = GetToleranceObject(document, entry);
    data->type = static_cast<int32_t>(object->GetType());
    data->type_of_value = static_cast<int32_t>(object->GetTypeOfValue());
    data->value = object->GetValue();
    data->material_requirement = static_cast<int32_t>(object->GetMaterialRequirementModifier());
    data->zone_modifier = static_cast<int32_t>(object->GetZoneModifier());
    data->zone_modifier_value = object->GetValueOfZoneModifier();
    data->maximum_value_modifier = object->GetMaxValueModifier();
    data->has_axis = object->HasAxis() ? 1 : 0;
    if (data->has_axis) data->axis = CopyAxis(object->GetAxis());
    data->has_plane = object->HasPlane() ? 1 : 0;
    if (data->has_plane) data->plane = CopyAxis(object->GetPlane());
    data->has_point = object->HasPoint() ? 1 : 0;
    if (data->has_point) data->point = CopyPoint(object->GetPoint());
    data->has_text_point = object->HasPointText() ? 1 : 0;
    if (data->has_text_point) data->text_point = CopyPoint(object->GetPointTextAttach());
    data->affected_plane_type = static_cast<int32_t>(object->GetAffectedPlaneType());
    if (object->HasAffectedPlane()) data->affected_plane = CopyPlane(object->GetAffectedPlane());
    *modifierCount = object->GetModifiers().Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_datum_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_PmiDatum* data, int32_t* modifierCount)
{
  if (data == nullptr || modifierCount == nullptr)
  { SetLastError("A datum snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *modifierCount = 0;
  return Guard([&]
  {
    const auto object = GetDatumObject(document, entry);
    data->position = object->GetPosition();
    data->is_datum_target = object->IsDatumTarget() ? 1 : 0;
    data->target_type = static_cast<int32_t>(object->GetDatumTargetType());
    data->target_length = object->GetDatumTargetLength();
    data->target_width = object->GetDatumTargetWidth();
    data->target_number = object->GetDatumTargetNumber();
    data->has_target_axis = object->HasDatumTargetParams() ? 1 : 0;
    if (data->has_target_axis) data->target_axis = CopyAxis(object->GetDatumTargetAxis());
    data->has_plane = object->HasPlane() ? 1 : 0;
    if (data->has_plane) data->plane = CopyAxis(object->GetPlane());
    data->has_point = object->HasPoint() ? 1 : 0;
    if (data->has_point) data->point = CopyPoint(object->GetPoint());
    data->has_text_point = object->HasPointText() ? 1 : 0;
    if (data->has_text_point) data->text_point = CopyPoint(object->GetPointTextAttach());
    XCAFDimTolObjects_DatumModifWithValue modifier;
    double modifierValue = 0.0;
    object->GetModifierWithValue(modifier, modifierValue);
    data->modifier_with_value = static_cast<int32_t>(modifier);
    data->modifier_value = modifierValue;
    data->has_modifier_with_value = std::abs(modifierValue) > 0.0 ? 1 : 0;
    *modifierCount = object->GetModifiers().Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_numeric_item(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t field, const int32_t index, double* realValue, int32_t* integerValue)
{
  if (realValue == nullptr || integerValue == nullptr)
  { SetLastError("A PMI numeric-item output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *realValue = 0.0; *integerValue = 0;
  return Guard([&]
  {
    if (index < 1) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "PMI numeric item indices are 1-based.");
    if (kind == 0 && field == 0)
    {
      const auto values = GetDimensionObject(document, entry)->GetValues();
      if (values.IsNull() || index > values->Length()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Dimension value index is out of range.");
      *realValue = values->Value(index); return;
    }
    if (kind == 0 && field == 1)
    {
      const auto values = GetDimensionObject(document, entry)->GetModifiers();
      if (index > values.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Dimension modifier index is out of range.");
      *integerValue = static_cast<int32_t>(values.Value(index)); return;
    }
    if (kind == 1 && field == 0)
    {
      const auto values = GetToleranceObject(document, entry)->GetModifiers();
      if (index > values.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Tolerance modifier index is out of range.");
      *integerValue = static_cast<int32_t>(values.Value(index)); return;
    }
    if (kind == 2 && field == 0)
    {
      const auto values = GetDatumObject(document, entry)->GetModifiers();
      if (index > values.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Datum modifier index is out of range.");
      *integerValue = static_cast<int32_t>(values.Value(index)); return;
    }
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI numeric field is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_text_utf8_length(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t field, int32_t* length)
{
  if (length == nullptr) { SetLastError("The PMI text-length pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *length = 0;
  return Guard([&] { *length = static_cast<int32_t>(PmiText(document, kind, entry, field).size()); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_text_to_utf8(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t field, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&] { CopyUtf8Result(PmiText(document, kind, entry, field), buffer, capacity, written); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_set_aux_shape(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t role, const OcctSharp_ShapeHandle* shape, const char* name)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document); ValidateUsableShape(shape);
    if (kind == 0)
    {
      const auto object = GetDimensionObject(document, entry);
      if (role == 0) object->SetPath(TopoDS::Edge(shape->Value));
      else if (role == 1) object->SetPresentation(shape->Value, MakePmiString(name));
      else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The dimension shape role is unsupported.");
      XCAFDoc_Dimension::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 1 && role == 1)
    {
      const auto object = GetToleranceObject(document, entry);
      object->SetPresentation(shape->Value, MakePmiString(name));
      XCAFDoc_GeomTolerance::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 2)
    {
      const auto object = GetDatumObject(document, entry);
      if (role == 0) object->SetDatumTarget(shape->Value);
      else if (role == 1) object->SetPresentation(shape->Value, MakePmiString(name));
      else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The datum shape role is unsupported.");
      XCAFDoc_Datum::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI shape role is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_clear_aux_shape(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry, const int32_t role)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    if (kind == 0)
    {
      const auto object = GetDimensionObject(document, entry);
      if (role == 0) object->SetPath(TopoDS_Edge());
      else object->SetPresentation(TopoDS_Shape(), object->GetPresentationName());
      XCAFDoc_Dimension::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 1)
    {
      const auto object = GetToleranceObject(document, entry);
      object->SetPresentation(TopoDS_Shape(), object->GetPresentationName());
      XCAFDoc_GeomTolerance::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else if (kind == 2)
    {
      const auto object = GetDatumObject(document, entry);
      if (role == 0) object->SetDatumTarget(TopoDS_Shape());
      else object->SetPresentation(TopoDS_Shape(), object->GetPresentationName());
      XCAFDoc_Datum::Set(ResolveOcafLabel(document, entry))->SetObject(object);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI shape role is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_get_aux_shape(
  const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const int32_t role, int32_t* hasShape, OcctSharp_ShapeHandle** shape)
{
  if (hasShape == nullptr || shape == nullptr) { SetLastError("A PMI shape output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *hasShape = 0; *shape = nullptr;
  return Guard([&]
  {
    TopoDS_Shape value;
    if (kind == 0) value = role == 0 ? GetDimensionObject(document, entry)->GetPath() : GetDimensionObject(document, entry)->GetPresentation();
    else if (kind == 1) value = GetToleranceObject(document, entry)->GetPresentation();
    else if (kind == 2) value = role == 0 ? GetDatumObject(document, entry)->GetDatumTarget() : GetDatumObject(document, entry)->GetPresentation();
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI shape role is unsupported.");
    if (!value.IsNull()) { *hasShape = 1; *shape = AllocateShape(value); }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_set_references(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry,
  const char* firstEntries, const char* secondEntries)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label item = ResolveOcafLabel(document, entry);
    const NCollection_Sequence<TDF_Label> first = ResolveEntries(document, firstEntries);
    const NCollection_Sequence<TDF_Label> second = ResolveEntries(document, secondEntries);
    if (kind == 0) GetDimTolTool(document)->SetDimension(first, second, item);
    else if (kind == 1) GetDimTolTool(document)->SetGeomTolerance(first, item);
    else if (kind == 2) GetDimTolTool(document)->SetDatum(first, item);
    else if (kind == 3)
    {
      opencascade::handle<XCAFDoc_GraphNode> toleranceNode;
      if (item.FindAttribute(XCAFDoc::DatumTolRefGUID(), toleranceNode))
      {
        while (toleranceNode->NbChildren() > 0)
          toleranceNode->UnSetChild(1);
      }
      item.ForgetAttribute(XCAFDoc::DatumTolRefGUID());
      for (NCollection_Sequence<TDF_Label>::Iterator iterator(first); iterator.More(); iterator.Next())
        GetDimTolTool(document)->SetDatumToGeomTol(iterator.Value(), item);
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI reference kind is unsupported.");
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_reference_count(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry, int32_t* count)
{
  if (count == nullptr) { SetLastError("The PMI reference count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *count = 0;
  return Guard([&] { *count = ReferenceLabels(document, relation, entry).Size(); });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_reference_entry(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry,
  const int32_t index, char* buffer, const int32_t capacity, int32_t* written)
{
  return Guard([&]
  {
    const NCollection_Sequence<TDF_Label> labels = ReferenceLabels(document, relation, entry);
    if (index < 1 || index > labels.Size()) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The PMI reference index is out of range.");
    CopyLabelEntry(labels.Value(index), buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_pmi_remove(
  OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    const bool valid = kind == 0 ? GetDimTolTool(document)->IsDimension(label)
      : kind == 1 ? GetDimTolTool(document)->IsGeomTolerance(label)
      : kind == 2 ? GetDimTolTool(document)->IsDatum(label) : false;
    if (!valid) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not the requested PMI kind.");
    label.ForgetAllAttributes(true);
  });
}

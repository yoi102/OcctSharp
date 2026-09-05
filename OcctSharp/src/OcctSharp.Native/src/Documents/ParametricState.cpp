#include "Documents/Parametric.hxx"
#include "Foundation/Text.hxx"
#include "Runtime/Validation.hxx"
#include <NCollection_HArray1.hxx>
#include <TDataStd_NamedData.hxx>
#include <cmath>
#include <cstring>

using namespace OcctSharp::Native;
using namespace OcctSharp::Native::Parametric;

OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_text_set(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const char* key,
  const char* text, int32_t length)
{
  return Guard([&] {
    Require(key && std::strlen(key) <= 256 && length >= 0 && length <= 4194304, "Invalid parametric metadata size.");
    const auto value = MakeExtendedUtf8(text, length);
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    TDataStd_NamedData::Set(label)->SetString(TCollection_ExtendedString(key, true), value);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_parametric_text_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const char* key,
  int32_t* found, char* text, int32_t capacity, int32_t* written)
{
  if (found) *found = 0; if (written) *written = 0;
  return Guard([&] {
    Require(key && found && written && capacity >= 0, "Invalid parametric metadata output.");
    const auto label = ResolveOcafLabel(document, entry); occ::handle<TDataStd_NamedData> data;
    const TCollection_ExtendedString name(key, true);
    if (!label.FindAttribute(TDataStd_NamedData::GetID(), data) || !data->HasString(name)) return;
    *found = 1;
    const auto value = ExtendedToUtf8(data->GetString(name));
    if (!text && capacity == 0) { *written = static_cast<int32_t>(value.size()) + 1; return; }
    CopyUtf8Result(value, text, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_parameter_set(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_ParameterInfo* info,
  const char* text, int32_t text_length, const int32_t* integers, const double* reals)
{
  return Guard([&] {
    Require(info && info->kind >= 0 && info->kind <= 5 && info->count >= 0 && info->count <= 1000000,
      "Invalid parameter kind or array count.");
    Require(info->reserved == 0 && text_length >= 0 && text_length <= 4194304, "Invalid parameter layout or text size.");
    if (info->kind == 2) ValidateFinite(info->real_value, "The parameter must be finite.");
    TCollection_ExtendedString value;
    if (info->kind == 3) value = MakeExtendedUtf8(text, text_length);
    if (info->kind == 4) ValidateArray(integers, info->count, "Missing integer array.");
    if (info->kind == 5) {
      ValidateArray(reals, info->count, "Missing real array.");
      for (int i = 0; i < info->count; ++i) ValidateFinite(reals[i], "Array values must be finite.");
    }
    const auto label = ResolveOcafLabel(document, entry); RequireOpenOcafCommand(document);
    auto data = TDataStd_NamedData::Set(label); data->Clear();
    if (info->kind == 0) return;
    data->SetInteger("kind", info->kind);
    if (info->kind == 1) data->SetInteger("value", info->integer_value);
    if (info->kind == 2) data->SetReal("value", info->real_value);
    if (info->kind == 3) data->SetString("value", value);
    if (info->kind >= 4) data->SetInteger("count", info->count);
    if (info->kind == 4 && info->count > 0) {
      occ::handle<NCollection_HArray1<int>> array = new NCollection_HArray1<int>(1, info->count);
      for (int i = 0; i < info->count; ++i) array->SetValue(i + 1, integers[i]);
      data->SetArrayOfIntegers("value", array);
    }
    if (info->kind == 5 && info->count > 0) {
      occ::handle<NCollection_HArray1<double>> array = new NCollection_HArray1<double>(1, info->count);
      for (int i = 0; i < info->count; ++i) array->SetValue(i + 1, reals[i]);
      data->SetArrayOfReals("value", array);
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_parameter_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry, OcctSharp_ParameterInfo* info,
  char* text, int32_t text_capacity, int32_t* written, int32_t* integers, double* reals, int32_t capacity)
{
  if (info) *info = {}; if (written) *written = 0;
  return Guard([&] {
    Require(info && written && capacity >= 0 && text_capacity >= 0, "Invalid parameter output.");
    const auto label = ResolveOcafLabel(document, entry); occ::handle<TDataStd_NamedData> data;
    if (!label.FindAttribute(TDataStd_NamedData::GetID(), data) || !data->HasInteger("kind")) return;
    info->kind = data->GetInteger("kind");
    Require(info->kind >= 1 && info->kind <= 5, "The stored parameter kind is invalid.");
    if (info->kind == 1) { Require(data->HasInteger("value"), "Stored integer is missing."); info->integer_value = data->GetInteger("value"); }
    if (info->kind == 2) { Require(data->HasReal("value"), "Stored real is missing."); info->real_value = data->GetReal("value"); }
    if (info->kind == 3) {
      Require(data->HasString("value"), "Stored string is missing.");
      const auto value = ExtendedToUtf8(data->GetString("value"));
      if (!text && text_capacity == 0) { *written = static_cast<int32_t>(value.size()) + 1; return; }
      CopyUtf8Result(value, text, text_capacity, written);
    }
    if (info->kind >= 4) {
      Require(data->HasInteger("count"), "Stored array count is missing."); info->count = data->GetInteger("count");
      Require(info->count >= 0 && info->count <= 1000000, "Stored array count is invalid.");
      if (info->count == 0) return;
      if (info->kind == 4) {
        Require(data->HasArrayOfIntegers("value"), "Stored integer array is missing.");
        const auto& array = data->GetArrayOfIntegers("value");
        Require(!array.IsNull() && array->Length() == info->count, "Stored integer array length differs.");
        if (!integers && capacity == 0) return;
        ValidateOutputCapacity(capacity, info->count, integers, "Integer output is too small.");
        for (int i = 0; i < info->count; ++i) integers[i] = array->Value(array->Lower() + i);
      } else {
        Require(data->HasArrayOfReals("value"), "Stored real array is missing.");
        const auto& array = data->GetArrayOfReals("value");
        Require(!array.IsNull() && array->Length() == info->count, "Stored real array length differs.");
        if (!reals && capacity == 0) return;
        ValidateOutputCapacity(capacity, info->count, reals, "Real output is too small.");
        for (int i = 0; i < info->count; ++i) reals[i] = array->Value(array->Lower() + i);
      }
    }
  });
}

#pragma once

// Private native Xde/Pmi contract; never a public ABI or a second owner.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.h"
#include <NCollection_Sequence.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_HAsciiString.hxx>
#include <XCAFDimTolObjects_DatumObject.hxx>
#include <XCAFDimTolObjects_DimensionObject.hxx>
#include <XCAFDimTolObjects_GeomToleranceObject.hxx>
#include <XCAFDoc_ClippingPlaneTool.hxx>
#include <XCAFDoc_DimTolTool.hxx>
#include <XCAFDoc_ViewTool.hxx>
#include <string>
#include <vector>

namespace OcctSharp::Native
{
opencascade::handle<XCAFDoc_DimTolTool> GetDimTolTool(const OcctSharp_OcafDocumentHandle* document);

opencascade::handle<XCAFDoc_ViewTool> GetViewTool(const OcctSharp_OcafDocumentHandle* document);

opencascade::handle<XCAFDoc_ClippingPlaneTool> GetClippingPlaneTool(const OcctSharp_OcafDocumentHandle* document);

opencascade::handle<TCollection_HAsciiString> MakePmiString(const char* value);

std::string CopyPmiString(const opencascade::handle<TCollection_HAsciiString>& value);

std::vector<std::string> SplitEntries(const char* entries);

NCollection_Sequence<TDF_Label> ResolveEntries(
  const OcctSharp_OcafDocumentHandle* document, const char* entries);

std::vector<TDF_Label> PmiLabels(const OcctSharp_OcafDocumentHandle* document, const int32_t kind);

opencascade::handle<XCAFDimTolObjects_DimensionObject> GetDimensionObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry);

opencascade::handle<XCAFDimTolObjects_GeomToleranceObject> GetToleranceObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry);

opencascade::handle<XCAFDimTolObjects_DatumObject> GetDatumObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry);

void SetDimensionObject(
  const TDF_Label& label, const OcctSharp_PmiDimension& data,
  const double* values, const int32_t valueCount,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName,
  const char* description, const char* descriptionName);

void SetToleranceObject(
  const TDF_Label& label, const OcctSharp_PmiTolerance& data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* semanticName, const char* presentationName);

void SetDatumObject(
  const TDF_Label& label, const OcctSharp_PmiDatum& data,
  const int32_t* modifiers, const int32_t modifierCount,
  const char* name, const char* description, const char* identification,
  const char* semanticName, const char* presentationName);

std::string PmiText(const OcctSharp_OcafDocumentHandle* document, const int32_t kind, const char* entry, const int32_t field);

NCollection_Sequence<TDF_Label> ReferenceLabels(
  const OcctSharp_OcafDocumentHandle* document, const int32_t relation, const char* entry);
}

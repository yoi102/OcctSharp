#pragma once

// Private native Xde/SavedViews contract; never a public ABI or a second owner.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.h"
#include <NCollection_Sequence.hxx>
#include <Standard_Handle.hxx>
#include <XCAFView_Object.hxx>

namespace OcctSharp::Native
{
void ValidateSavedView(const OcctSharp_SavedView& data);

void SetSavedViewObject(
  const TDF_Label& label, const OcctSharp_SavedView& data,
  const char* name, const char* clippingExpression);

NCollection_Sequence<TDF_Label> AddSavedViewPlanes(
  const OcctSharp_OcafDocumentHandle* document,
  const OcctSharp_PlaneEquation* planes, const int32_t planeCount);

void RemoveUnreferencedPlanes(
  const OcctSharp_OcafDocumentHandle* document,
  const NCollection_Sequence<TDF_Label>& labels);

opencascade::handle<XCAFView_Object> GetSavedViewObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry);
}

#pragma once

// Private native Xde/Metadata contract; never a public ABI or a second owner.
#include "Documents/Lifecycle.hxx"
#include "OcctSharp.Native.h"
#include <NCollection_HSequence.hxx>
#include <TCollection_ExtendedString.hxx>
#include <NCollection_IndexedDataMap.hxx>
#include <Quantity_Color.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_ExtendedString.hxx>
#include <TCollection_HAsciiString.hxx>
#include <TopTools_ShapeMapHasher.hxx>
#include <TopoDS_Shape.hxx>
#include <XCAFPrs_Style.hxx>
#include <string>

namespace OcctSharp::Native
{
using XdePresentationStyleMap =
  NCollection_IndexedDataMap<TopoDS_Shape, XCAFPrs_Style, TopTools_ShapeMapHasher>;

XdePresentationStyleMap CollectXdePresentationStyles(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry);

OcctSharp_XdeColor CopyXdeColor(const Quantity_ColorRGBA& color);

OcctSharp_XdeColor CopyXdeColor(const Quantity_Color& color, const double alpha = 1.0);

OcctSharp_XdePresentationStyle CopyXdePresentationStyle(const XCAFPrs_Style& style);

bool GetAssignedMaterial(
  const TDF_Label& label,
  opencascade::handle<TCollection_HAsciiString>& name,
  opencascade::handle<TCollection_HAsciiString>& description,
  double& density,
  opencascade::handle<TCollection_HAsciiString>& densityName,
  opencascade::handle<TCollection_HAsciiString>& densityType);

std::string MaterialFieldUtf8(const TDF_Label& label, const int32_t field);

opencascade::handle<NCollection_HSequence<TCollection_ExtendedString>> GetXdeLayers(
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry);
}

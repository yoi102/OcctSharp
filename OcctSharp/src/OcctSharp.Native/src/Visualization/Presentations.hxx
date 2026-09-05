#pragma once

// Private native Visualization/Presentations contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Runtime/Shape.hxx"
#include "Visualization/Context.hxx"
#include "Xde/Metadata.hxx"
#include <AIS_ColoredShape.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <Standard_Handle.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <XCAFPrs_Style.hxx>

namespace OcctSharp::Native
{
opencascade::handle<AIS_ColoredShape> FindPresentation(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t presentationId);

int64_t FindPresentationId(
  const OcctSharp_ViewerHandle* viewer,
  const opencascade::handle<AIS_InteractiveObject>& presentation);

void ValidateSubshape(
  const opencascade::handle<AIS_ColoredShape>& presentation,
  const OcctSharp_ShapeHandle* subshape);

bool TryGetXdeStyleColor(
  const XCAFPrs_Style& style,
  const TopAbs_ShapeEnum shapeType,
  Quantity_ColorRGBA& color);

void ApplyXdePresentationStyles(
  const opencascade::handle<AIS_ColoredShape>& presentation,
  const XdePresentationStyleMap& settings);
}

#pragma once

// Private native Visualization/Dimensions contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Visualization/Context.hxx"
#include <PrsDim_Dimension.hxx>
#include <Standard_Handle.hxx>

namespace OcctSharp::Native
{
opencascade::handle<PrsDim_Dimension> FindDimension(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t dimensionId);

void ConfigureDimension(
  const opencascade::handle<PrsDim_Dimension>& dimension,
  const char* modelUnits,
  const char* displayUnits,
  const int32_t hasCustomValue,
  const double customValue,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double lineWidth);
}

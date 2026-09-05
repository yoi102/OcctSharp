#pragma once

// Private native Visualization/Manipulators contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include "Visualization/Context.hxx"
#include <AIS_ManipulatorMode.hxx>

namespace OcctSharp::Native
{
OcctSharp_ViewerHandle::ManipulatorEntry& FindManipulator(
  OcctSharp_ViewerHandle* viewer,
  const int64_t manipulatorId);

AIS_ManipulatorMode ToManipulatorMode(const int32_t mode, const bool allowNone = false);

int32_t ManipulatorModeBit(const AIS_ManipulatorMode mode);

void ReattachManipulator(
  OcctSharp_ViewerHandle* viewer,
  OcctSharp_ViewerHandle::ManipulatorEntry& entry);

void DetachManipulatorsForPresentation(OcctSharp_ViewerHandle* viewer, const int64_t presentationId);
}

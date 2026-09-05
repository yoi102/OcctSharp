// Native Visualization/Manipulators implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "Geometry/Transforms.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Manipulators.hxx"
#include "Visualization/Presentations.hxx"
#include <AIS_Manipulator.hxx>
#include <AIS_ManipulatorMode.hxx>
#include <Standard_Handle.hxx>
#include <TopLoc_Location.hxx>
#include <cmath>

namespace OcctSharp::Native
{
OcctSharp_ViewerHandle::ManipulatorEntry& FindManipulator(
  OcctSharp_ViewerHandle* viewer,
  const int64_t manipulatorId)
{
  const auto iterator = viewer->Manipulators.find(manipulatorId);
  if (iterator == viewer->Manipulators.end())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer manipulator ID does not exist.");
  }
  return iterator->second;
}

AIS_ManipulatorMode ToManipulatorMode(const int32_t mode, const bool allowNone)
{
  if ((allowNone && mode == AIS_MM_None)
      || mode == AIS_MM_Translation || mode == AIS_MM_Rotation
      || mode == AIS_MM_Scaling || mode == AIS_MM_TranslationPlane)
  {
    return static_cast<AIS_ManipulatorMode>(mode);
  }
  throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The manipulator mode is outside the supported range.");
}

int32_t ManipulatorModeBit(const AIS_ManipulatorMode mode)
{
  return 1 << (static_cast<int32_t>(mode) - 1);
}

void ReattachManipulator(
  OcctSharp_ViewerHandle* viewer,
  OcctSharp_ViewerHandle::ManipulatorEntry& entry)
{
  AIS_Manipulator::OptionsForAttach options;
  options.SetAdjustPosition(false).SetAdjustSize(false).SetEnableModes(false);
  entry.Value->Attach(FindPresentation(viewer, entry.PresentationId), options);
  entry.Value->SetModeActivationOnDetection(entry.ActivationOnDetection);
  entry.Value->SetZoomPersistence(entry.ZoomPersistence);
  for (int32_t mode = AIS_MM_Translation; mode <= AIS_MM_TranslationPlane; ++mode)
  {
    const auto nativeMode = static_cast<AIS_ManipulatorMode>(mode);
    if ((entry.EnabledModes & ManipulatorModeBit(nativeMode)) != 0)
      entry.Value->EnableMode(nativeMode);
  }
}

void DetachManipulatorsForPresentation(OcctSharp_ViewerHandle* viewer, const int64_t presentationId)
{
  for (auto iterator = viewer->Manipulators.begin(); iterator != viewer->Manipulators.end();)
  {
    if (iterator->second.PresentationId != presentationId)
    {
      ++iterator;
      continue;
    }
    if (!iterator->second.Value.IsNull() && iterator->second.Value->IsAttached())
      iterator->second.Value->Detach();
    iterator = viewer->Manipulators.erase(iterator);
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_attach(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const int32_t adjust_position,
  const int32_t adjust_size,
  const int32_t enable_modes,
  int64_t* manipulator_id)
{
  if (manipulator_id == nullptr)
  { SetLastError("The viewer manipulator ID output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *manipulator_id = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    auto manipulator = opencascade::handle<AIS_Manipulator>(new AIS_Manipulator());
    AIS_Manipulator::OptionsForAttach options;
    options.SetAdjustPosition(adjust_position != 0)
      .SetAdjustSize(adjust_size != 0)
      .SetEnableModes(enable_modes != 0);
    manipulator->Attach(presentation, options);
    const int64_t id = viewer->NextManipulatorId++;
    viewer->Manipulators.emplace(
      id, OcctSharp_ViewerHandle::ManipulatorEntry{manipulator, presentation_id});
    *manipulator_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_part(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t axis,
  const int32_t mode, const int32_t enabled)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (axis < 0 || axis > 2)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Manipulator axes are zero-based from 0 through 2.");
    FindManipulator(viewer, manipulator_id).Value->SetPart(axis, ToManipulatorMode(mode), enabled != 0);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_enable_mode(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t mode)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    const AIS_ManipulatorMode nativeMode = ToManipulatorMode(mode);
    entry.Value->EnableMode(nativeMode);
    entry.EnabledModes |= ManipulatorModeBit(nativeMode);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_activation_on_detection(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t enabled)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    entry.Value->SetModeActivationOnDetection(enabled != 0);
    entry.ActivationOnDetection = enabled != 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_position(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const OcctSharp_Ax2* position)
{
  if (position == nullptr)
  { SetLastError("The manipulator position pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    FindManipulator(viewer, manipulator_id).Value->SetPosition(ToAxis(*position));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_appearance(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id,
  const double size, const double gap, const int32_t skin)
{
  if (!std::isfinite(size) || size <= 0.0 || size > 1000000.0
      || !std::isfinite(gap) || gap < 0.0 || gap > size || (skin != 0 && skin != 1))
  { SetLastError("Manipulator appearance values are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    const auto nativeSkin = static_cast<AIS_Manipulator::ManipulatorSkin>(skin);
    const bool mustReattach = entry.Value->IsAttached() && entry.Value->SkinMode() != nativeSkin;
    if (mustReattach) entry.Value->Detach();
    entry.Value->SetSkinMode(nativeSkin);
    entry.Value->SetSize(static_cast<float>(size));
    entry.Value->SetGap(static_cast<float>(gap));
    if (mustReattach)
      ReattachManipulator(viewer, entry);
    entry.Size = size;
    entry.Gap = gap;
    entry.Skin = skin;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_set_zoom_persistence(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t enabled)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    entry.Value->SetZoomPersistence(enabled != 0);
    entry.ZoomPersistence = enabled != 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_start(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t x, const int32_t y)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    entry.StartTransformation = viewer->Context->Location(
      FindPresentation(viewer, entry.PresentationId)).Transformation();
    entry.Value->StartTransform(x, y, viewer->View);
    entry.HasActiveTransformation = true;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_transform_mouse(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t x, const int32_t y,
  OcctSharp_TrsfHandle** transform)
{
  if (transform == nullptr)
  { SetLastError("The mouse transform output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *transform = nullptr;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    *transform = AllocateTransform(FindManipulator(viewer, manipulator_id).Value->Transform(x, y, viewer->View));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_transform_custom(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const OcctSharp_TrsfHandle* transform)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateTransformHandle(transform);
    auto& entry = FindManipulator(viewer, manipulator_id);
    if (!entry.HasActiveTransformation)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A custom manipulator preview requires StartTransform first.");
    entry.Value->Transform(transform->Value);
    viewer->Context->SetLocation(
      FindPresentation(viewer, entry.PresentationId),
      TopLoc_Location(entry.StartTransformation.Multiplied(transform->Value)));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_stop(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id, const int32_t apply)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    entry.Value->StopTransform(apply != 0);
    if (apply == 0)
      viewer->Context->SetLocation(
        FindPresentation(viewer, entry.PresentationId), TopLoc_Location(entry.StartTransformation));
    entry.HasActiveTransformation = false;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_get_state(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id,
  OcctSharp_ViewerManipulatorState* state)
{
  if (state == nullptr)
  { SetLastError("The manipulator state output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *state = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto& entry = FindManipulator(viewer, manipulator_id);
    state->attached = entry.Value->IsAttached() ? 1 : 0;
    state->active_mode = static_cast<int32_t>(entry.Value->ActiveMode());
    state->active_axis = entry.Value->ActiveAxisIndex();
    state->has_active_transformation = entry.HasActiveTransformation ? 1 : 0;
    state->activation_on_detection = entry.ActivationOnDetection ? 1 : 0;
    state->zoom_persistence = entry.ZoomPersistence ? 1 : 0;
    state->skin = entry.Skin;
    state->size = entry.Size;
    state->gap = entry.Gap;
    state->position = CopyAxis(entry.Value->Position());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_manipulator_detach(
  OcctSharp_ViewerHandle* viewer, const int64_t manipulator_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    auto& entry = FindManipulator(viewer, manipulator_id);
    if (entry.Value->IsAttached()) entry.Value->Detach();
    viewer->Manipulators.erase(manipulator_id);
  });
}

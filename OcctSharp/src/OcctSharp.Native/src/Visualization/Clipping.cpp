// Native Visualization/Clipping implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Clipping.hxx"
#include "Visualization/Context.hxx"
#include <Graphic3d_ClipPlane.hxx>
#include <Standard_Handle.hxx>
#include <cmath>
#include <gp_Pln.hxx>

namespace OcctSharp::Native
{
opencascade::handle<Graphic3d_ClipPlane> FindClipPlane(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t clipPlaneId)
{
  const auto iterator = viewer->ClipPlanes.find(clipPlaneId);
  if (iterator == viewer->ClipPlanes.end())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer clip-plane ID does not exist.");
  }
  return iterator->second;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create_clip_plane(
  OcctSharp_ViewerHandle* viewer, const double a, const double b, const double c, const double d,
  int64_t* clip_plane_id)
{
  if (clip_plane_id == nullptr)
  { SetLastError("The clip-plane ID output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *clip_plane_id = 0;
  if (!std::isfinite(a) || !std::isfinite(b) || !std::isfinite(c) || !std::isfinite(d)
      || a * a + b * b + c * c <= 1.0e-24)
  { SetLastError("A clip plane requires finite coefficients and a non-zero normal."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto plane = new Graphic3d_ClipPlane(gp_Pln(a, b, c, d));
    const int64_t id = viewer->NextClipPlaneId++;
    viewer->ClipPlanes.emplace(id, plane);
    viewer->View->AddClipPlane(plane);
    *clip_plane_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_update_clip_plane(
  OcctSharp_ViewerHandle* viewer, const int64_t clip_plane_id,
  const double a, const double b, const double c, const double d)
{
  if (!std::isfinite(a) || !std::isfinite(b) || !std::isfinite(c) || !std::isfinite(d)
      || a * a + b * b + c * c <= 1.0e-24)
  { SetLastError("A clip plane requires finite coefficients and a non-zero normal."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    FindClipPlane(viewer, clip_plane_id)->SetEquation(gp_Pln(a, b, c, d));
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_clip_plane_enabled(
  OcctSharp_ViewerHandle* viewer, const int64_t clip_plane_id, const int32_t enabled)
{
  if (enabled != 0 && enabled != 1)
  { SetLastError("Clip-plane enabled state must be Boolean."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    FindClipPlane(viewer, clip_plane_id)->SetOn(enabled != 0);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_clip_plane(
  OcctSharp_ViewerHandle* viewer, const int64_t clip_plane_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto plane = FindClipPlane(viewer, clip_plane_id);
    viewer->View->RemoveClipPlane(plane);
    viewer->ClipPlanes.erase(clip_plane_id);
  });
}

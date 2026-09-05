// Native Visualization/Navigation implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Navigation.hxx"
#include <AIS_Manipulator.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <Graphic3d_BufferType.hxx>
#include <Graphic3d_Camera.hxx>
#include <Quantity_Color.hxx>
#include <Standard_Handle.hxx>
#include <V3d_TypeOfOrientation.hxx>
#include <algorithm>
#include <cmath>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

namespace OcctSharp::Native
{
Aspect_TypeOfTriedronPosition ToTrihedronPosition(const int32_t position)
{
  switch (position)
  {
    case 0: return Aspect_TOTP_CENTER;
    case 1: return Aspect_TOTP_TOP;
    case 2: return Aspect_TOTP_BOTTOM;
    case 4: return Aspect_TOTP_LEFT;
    case 5: return Aspect_TOTP_LEFT_UPPER;
    case 6: return Aspect_TOTP_LEFT_LOWER;
    case 8: return Aspect_TOTP_RIGHT;
    case 9: return Aspect_TOTP_RIGHT_UPPER;
    case 10: return Aspect_TOTP_RIGHT_LOWER;
    default:
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The trihedron position is invalid.");
  }
}

V3d_TypeOfOrientation ToViewerProjection(const int32_t projection)
{
  switch (projection)
  {
    case 0: return V3d_TypeOfOrientation_Zup_Front;
    case 1: return V3d_TypeOfOrientation_Zup_Back;
    case 2: return V3d_TypeOfOrientation_Zup_Top;
    case 3: return V3d_TypeOfOrientation_Zup_Bottom;
    case 4: return V3d_TypeOfOrientation_Zup_Left;
    case 5: return V3d_TypeOfOrientation_Zup_Right;
    case 6: return V3d_TypeOfOrientation_Zup_AxoRight;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer projection is outside the supported range.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_fit_all(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const bool hasFlatManipulator = std::any_of(
      viewer->Manipulators.begin(), viewer->Manipulators.end(),
      [](const auto& item)
      {
        return item.second.Skin == AIS_Manipulator::ManipulatorSkin_Flat;
      });
    if (hasFlatManipulator)
      throw OperationFailure(
        OCCTSHARP_STATUS_INVALID_ARGUMENT,
        "OCCT 8.0.1 cannot safely fit a view while a flat-skin manipulator is attached; detach it or fit before attachment.");
    viewer->View->FitAll(0.01, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_redraw(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_resize(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->MustBeResized();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_projection(
  OcctSharp_ViewerHandle* viewer,
  const int32_t projection)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetProj(ToViewerProjection(projection));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_zoom(
  OcctSharp_ViewerHandle* viewer,
  const double factor)
{
  if (!std::isfinite(factor) || factor <= 0.0)
  {
    SetLastError("Viewer zoom factor must be finite and greater than zero.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetZoom(factor, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_pan(
  OcctSharp_ViewerHandle* viewer,
  const int32_t delta_x,
  const int32_t delta_y)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Pan(delta_x, delta_y, 1.0, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_start_rotation(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  const double z_rotation_threshold)
{
  if (!std::isfinite(z_rotation_threshold) || z_rotation_threshold < 0.0)
  {
    SetLastError("Viewer Z rotation threshold must be finite and non-negative.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->StartRotation(x, y, z_rotation_threshold);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_rotate(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Rotation(x, y);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_get_camera(
  OcctSharp_ViewerHandle* viewer, OcctSharp_ViewerCamera* camera)
{
  if (camera == nullptr)
  { SetLastError("The viewer camera output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *camera = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Eye(camera->eye.x, camera->eye.y, camera->eye.z);
    viewer->View->At(camera->target.x, camera->target.y, camera->target.z);
    viewer->View->Up(camera->up.x, camera->up.y, camera->up.z);
    viewer->View->Proj(camera->projection.x, camera->projection.y, camera->projection.z);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_camera(
  OcctSharp_ViewerHandle* viewer, const OcctSharp_ViewerCamera* camera)
{
  if (camera == nullptr || !IsFinite(camera->eye) || !IsFinite(camera->target)
      || !IsFinite(camera->up) || !IsFinite(camera->projection))
  { SetLastError("Viewer camera values must be non-null and finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const gp_Vec eye_to_target(
      gp_Pnt(camera->eye.x, camera->eye.y, camera->eye.z),
      gp_Pnt(camera->target.x, camera->target.y, camera->target.z));
    const gp_Vec projection(camera->projection.x, camera->projection.y, camera->projection.z);
    const gp_Vec up(camera->up.x, camera->up.y, camera->up.z);
    if (eye_to_target.SquareMagnitude() <= 1.0e-24 || projection.SquareMagnitude() <= 1.0e-24
        || up.SquareMagnitude() <= 1.0e-24)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer camera directions must be non-zero.");
    const gp_Dir direction(eye_to_target);
    const gp_Dir supplied_projection(projection);
    const gp_Dir supplied_up(up);
    if (std::abs(direction.Dot(supplied_projection)) < 0.999999)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer camera projection must agree with eye-to-target direction.");
    if (std::abs(direction.Dot(supplied_up)) > 0.999999)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer camera up direction cannot be parallel to its projection.");
    opencascade::handle<Graphic3d_Camera> updated = new Graphic3d_Camera();
    updated->Copy(viewer->View->Camera());
    updated->SetEyeAndCenter(
      gp_Pnt(camera->eye.x, camera->eye.y, camera->eye.z),
      gp_Pnt(camera->target.x, camera->target.y, camera->target.z));
    updated->SetUp(supplied_up);
    viewer->View->SetCamera(updated);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_screen_to_world(
  OcctSharp_ViewerHandle* viewer, const int32_t x, const int32_t y, OcctSharp_Xyz* point)
{
  if (point == nullptr)
  { SetLastError("The screen-to-world output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *point = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Convert(x, y, point->x, point->y, point->z);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_world_to_screen(
  OcctSharp_ViewerHandle* viewer, const OcctSharp_Xyz* point, int32_t* x, int32_t* y)
{
  if (point == nullptr || x == nullptr || y == nullptr || !IsFinite(*point))
  { SetLastError("World-to-screen inputs and outputs must be non-null and finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *x = 0; *y = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->Convert(point->x, point->y, point->z, *x, *y);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_pick_ray(
  OcctSharp_ViewerHandle* viewer, const int32_t x, const int32_t y, OcctSharp_ViewerPickRay* ray)
{
  if (ray == nullptr)
  { SetLastError("The viewer pick-ray output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *ray = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->ConvertWithProj(x, y, ray->origin.x, ray->origin.y, ray->origin.z,
                                  ray->direction.x, ray->direction.y, ray->direction.z);
    const gp_Dir direction(ray->direction.x, ray->direction.y, ray->direction.z);
    ray->direction = { direction.X(), direction.Y(), direction.Z() };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_window_fit(
  OcctSharp_ViewerHandle* viewer, const int32_t min_x, const int32_t min_y,
  const int32_t max_x, const int32_t max_y)
{
  if (min_x == max_x || min_y == max_y)
  { SetLastError("A viewer zoom rectangle must have non-zero width and height."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->WindowFit(std::min(min_x, max_x), std::min(min_y, max_y),
                            std::max(min_x, max_x), std::max(min_y, max_y));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_background_color(
  OcctSharp_ViewerHandle* viewer, const double red, const double green, const double blue)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(red, green, blue);
    viewer->View->SetBackgroundColor(Quantity_TOC_RGB, red, green, blue);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_computed_mode(
  OcctSharp_ViewerHandle* viewer, const int32_t enabled)
{
  if (enabled != 0 && enabled != 1)
  { SetLastError("Viewer computed-mode state must be Boolean."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetComputedMode(enabled != 0);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_show_trihedron(
  OcctSharp_ViewerHandle* viewer, const int32_t position,
  const double red, const double green, const double blue, const double scale)
{
  if (!std::isfinite(scale) || scale <= 0.0 || scale > 1.0)
  { SetLastError("Viewer trihedron scale must be finite, positive, and no greater than 1."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(red, green, blue);
    viewer->View->TriedronDisplay(ToTrihedronPosition(position),
      Quantity_Color(red, green, blue, Quantity_TOC_RGB), scale, V3d_WIREFRAME);
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_hide_trihedron(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->TriedronErase();
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dump(
  OcctSharp_ViewerHandle* viewer, const char* file_path, const int32_t buffer_type)
{
  if (file_path == nullptr || file_path[0] == '\0' || buffer_type < 0 || buffer_type > 2)
  { SetLastError("Viewer screenshot path or buffer type is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (!viewer->View->Dump(file_path, static_cast<Graphic3d_BufferType>(buffer_type)))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT failed to write the viewer screenshot.");
  });
}

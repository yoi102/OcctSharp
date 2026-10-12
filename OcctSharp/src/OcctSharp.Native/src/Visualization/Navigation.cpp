#include "../../generated/Visualization/OcctSharp.Visualization.ViewerAdapters.Generated.hxx"
// Native Visualization/Navigation implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Navigation.hxx"
#include <AIS_Manipulator.hxx>
#include <AIS_Shape.hxx>
#include <AIS_ViewCube.hxx>
#include <Aspect_TypeOfTriedronPosition.hxx>
#include <BRep_Builder.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <Graphic3d_BufferType.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_TransformPers.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Aspect_TypeOfHighlightMethod.hxx>
#include <NCollection_Vec2.hxx>
#include <Quantity_Color.hxx>
#include <Prs3d_Drawer.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <V3d_TypeOfOrientation.hxx>
#include <V3d.hxx>
#include <algorithm>
#include <cmath>
#include <cstring>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>
#include <TopoDS_Compound.hxx>

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

opencascade::handle<AIS_Shape> MakeViewCubeRollArrow(const bool left)
{
  constexpr double radius = 20.0;
  constexpr double pi = 3.14159265358979323846;
  const double cx = left ? 12.0 : 35.0;
  const double cy = left ? 30.0 : 10.0;
  const double start = (left ? 30.0 : 65.0) * pi / 180.0;
  const double sweep = (left ? 45.0 : -45.0) * pi / 180.0;
  BRep_Builder builder;
  TopoDS_Compound shape;
  builder.MakeCompound(shape);
  auto point = [&](const double angle)
  { return gp_Pnt(cx + radius * std::cos(angle), cy + radius * std::sin(angle), 0.0); };
  for (int index = 0; index < 22; ++index)
  {
    const gp_Pnt from = point(start + sweep * index / 22.0);
    const gp_Pnt to = point(start + sweep * (index + 1) / 22.0);
    builder.Add(shape, BRepBuilderAPI_MakeEdge(from, to).Edge());
  }
  const double end = start + sweep;
  const gp_Pnt tip = point(end);
  const double sign = left ? 1.0 : -1.0;
  const gp_Vec tangent(-std::sin(end) * sign, std::cos(end) * sign, 0.0);
  const gp_Vec normal(-tangent.Y(), tangent.X(), 0.0);
  builder.Add(shape, BRepBuilderAPI_MakeEdge(tip, tip.Translated(tangent * -8.0 + normal * 4.0)).Edge());
  builder.Add(shape, BRepBuilderAPI_MakeEdge(tip, tip.Translated(tangent * -8.0 - normal * 4.0)).Edge());
  opencascade::handle<AIS_Shape> arrow = new AIS_Shape(shape);
  arrow->SetTransformPersistence(new Graphic3d_TransformPers(
    Graphic3d_TMF_2d, Aspect_TOTP_RIGHT_UPPER, NCollection_Vec2<int>(82, 82)));
  arrow->SetZLayer(Graphic3d_ZLayerId_TopOSD);
  arrow->SetColor(Quantity_Color(0.96, 0.97, 1.0, Quantity_TOC_RGB));
  arrow->SetWidth(3.0);
  opencascade::handle<Prs3d_Drawer> hover = new Prs3d_Drawer();
  hover->SetMethod(Aspect_TOHM_COLOR);
  hover->SetColor(Quantity_Color(1.0, 0.72, 0.12, Quantity_TOC_RGB));
  hover->SetZLayer(Graphic3d_ZLayerId_TopOSD);
  arrow->SetDynamicHilightAttributes(hover);
  return arrow;
}

opencascade::handle<AIS_ViewCube> MakeViewCube()
{
  opencascade::handle<AIS_ViewCube> cube = new AIS_ViewCube();
  cube->SetAutoStartAnimation(false);
  cube->SetSize(48.0);
  cube->SetDrawAxes(false);
  cube->SetTransformPersistence(new Graphic3d_TransformPers(
    Graphic3d_TMF_TriedronPers, Aspect_TOTP_RIGHT_UPPER, NCollection_Vec2<int>(82, 82)));
  return cube;
}

void ApplyViewCubeHover(OcctSharp_ViewerHandle* viewer)
{
  if (viewer->ViewCube.IsNull()) return;
  const double transparency = viewer->ViewCubeHovered ? 0.0 : viewer->ViewCubeIdleTransparency;
  viewer->ViewCube->SetBoxTransparency(transparency);
  viewer->ViewCube->SetTextColor(viewer->ViewCubeHovered
    ? Quantity_Color(0.10, 0.16, 0.24, Quantity_TOC_RGB)
    : Quantity_Color(0.48, 0.53, 0.60, Quantity_TOC_RGB));
  for (const auto& arrow : {viewer->ViewCubeLeftArrow, viewer->ViewCubeRightArrow})
    if (!arrow.IsNull())
    {
      arrow->SetColor(viewer->ViewCubeHovered
        ? Quantity_Color(0.96, 0.97, 1.0, Quantity_TOC_RGB)
        : Quantity_Color(0.63, 0.68, 0.75, Quantity_TOC_RGB));
      arrow->SetTransparency(transparency);
    }
  if (viewer->Context->IsDisplayed(viewer->ViewCube))
  {
    viewer->Context->Redisplay(viewer->ViewCube, false);
    for (const auto& arrow : {viewer->ViewCubeLeftArrow, viewer->ViewCubeRightArrow})
      if (!arrow.IsNull() && viewer->Context->IsDisplayed(arrow))
        viewer->Context->Redisplay(arrow, false);
    viewer->View->Redraw();
  }
}

void SyncViewCubeRollArrows(OcctSharp_ViewerHandle* viewer)
{
  if (viewer->ViewCube.IsNull() || !viewer->Context->IsDisplayed(viewer->ViewCube)) return;
  double x = 0.0, y = 0.0, z = 0.0;
  OcctSharpGenerated::ViewerAdapters::V3d_View_Proj_Bind(*(viewer->View))(x, y, z);
  const double length = std::sqrt(x*x + y*y + z*z);
  const bool onFace = length > 0.0 &&
    std::max({std::abs(x), std::abs(y), std::abs(z)}) / length >= 0.9995;
  for (const auto& arrow : {viewer->ViewCubeLeftArrow, viewer->ViewCubeRightArrow})
  {
    if (onFace && !viewer->Context->IsDisplayed(arrow)) viewer->Context->Display(arrow, false);
    else if (!onFace && viewer->Context->IsDisplayed(arrow)) viewer->Context->Remove(arrow, false);
  }
}

OcctSharp_ViewerCamera TargetCameraWithoutDisplayChange(
  OcctSharp_ViewerHandle* viewer, const V3d_TypeOfOrientation orientation)
{
  // Work on a detached camera. V3d_View::SetProj touches the view's rendering
  // state, even if immediate updates are disabled; an unwindowed view is unsafe.
  const gp_Dir axis = V3d::GetProjAxis(orientation);
  const gp_Dir referenceUp = axis.Z() > 0.999 ? gp::DY()
    : axis.Z() < -0.999 ? -gp::DY() : gp::DZ();
  const gp_Vec projectedUp = gp_Vec(referenceUp) - gp_Vec(axis) * referenceUp.Dot(axis);
  opencascade::handle<Graphic3d_Camera> preview = new Graphic3d_Camera(OcctSharpGenerated::ViewerAdapters::V3d_View_Camera_Bind(*(viewer->View))());
  preview->SetDirection(-axis);
  OcctSharpGenerated::ViewerAdapters::Graphic3d_Camera_SetUp_Bind(*(preview))(gp_Dir(projectedUp));
  OcctSharp_ViewerCamera target{};
  const gp_Pnt eye = OcctSharpGenerated::ViewerAdapters::Graphic3d_Camera_Eye_Bind(*(preview))();
  const gp_Pnt center = preview->Center();
  const gp_Dir up = OcctSharpGenerated::ViewerAdapters::Graphic3d_Camera_Up_Bind(*(preview))();
  target.eye = {eye.X(), eye.Y(), eye.Z()};
  target.target = {center.X(), center.Y(), center.Z()};
  target.up = {up.X(), up.Y(), up.Z()};
  target.projection = {axis.X(), axis.Y(), axis.Z()};
  return target;
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
    SyncViewCubeRollArrows(viewer);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_get_projection_target_camera(
  OcctSharp_ViewerHandle* viewer, const int32_t projection, OcctSharp_ViewerCamera* camera)
{
  if (camera == nullptr)
  { SetLastError("Projection target camera output is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *camera = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    *camera = TargetCameraWithoutDisplayChange(viewer, ToViewerProjection(projection));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_view_cube_visible(
  OcctSharp_ViewerHandle* viewer, const int32_t visible)
{
  if (visible != 0 && visible != 1)
  { SetLastError("View cube visibility must be Boolean."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (visible != 0)
    {
      if (viewer->ViewCube.IsNull())
      {
        viewer->ViewCube = MakeViewCube();
        viewer->ViewCubeLeftArrow = MakeViewCubeRollArrow(true);
        viewer->ViewCubeRightArrow = MakeViewCubeRollArrow(false);
        ApplyViewCubeHover(viewer);
      }
      viewer->Context->Display(viewer->ViewCube, false);
      SyncViewCubeRollArrows(viewer);
    }
    else if (!viewer->ViewCube.IsNull())
    {
      viewer->Context->Remove(viewer->ViewCube, false);
      viewer->Context->Remove(viewer->ViewCubeLeftArrow, false);
      viewer->Context->Remove(viewer->ViewCubeRightArrow, false);
    }
    viewer->View->Redraw();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_view_cube_appearance(
  OcctSharp_ViewerHandle* viewer, const char* front, const char* back,
  const char* left, const char* right, const char* top, const char* bottom,
  const char* font, const double font_height, const double idle_transparency)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    for (const char* value : {front, back, left, right, top, bottom, font})
      if (value == nullptr || *value == '\0' || std::strlen(value) > 64)
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "View cube text must contain 1..64 UTF-8 bytes.");
    if (!std::isfinite(font_height) || font_height < 8.0 || font_height > 30.0
      || !std::isfinite(idle_transparency) || idle_transparency < 0.0 || idle_transparency > 0.9)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "View cube font height or idle transparency is invalid.");

    if (viewer->ViewCube.IsNull()) viewer->ViewCube = MakeViewCube();
    viewer->ViewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Front, TCollection_AsciiString(front));
    viewer->ViewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Back, TCollection_AsciiString(back));
    viewer->ViewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Left, TCollection_AsciiString(left));
    viewer->ViewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Right, TCollection_AsciiString(right));
    viewer->ViewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Top, TCollection_AsciiString(top));
    viewer->ViewCube->SetBoxSideLabel(V3d_TypeOfOrientation_Zup_Bottom, TCollection_AsciiString(bottom));
    viewer->ViewCube->SetFont(TCollection_AsciiString(font));
    viewer->ViewCube->SetFontHeight(font_height);
    viewer->ViewCubeIdleTransparency = idle_transparency;
    if (viewer->ViewCubeLeftArrow.IsNull()) viewer->ViewCubeLeftArrow = MakeViewCubeRollArrow(true);
    if (viewer->ViewCubeRightArrow.IsNull()) viewer->ViewCubeRightArrow = MakeViewCubeRollArrow(false);
    ApplyViewCubeHover(viewer);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_view_cube_hovered(
  OcctSharp_ViewerHandle* viewer, const int32_t hovered)
{
  if (hovered != 0 && hovered != 1)
  { SetLastError("View cube hover must be Boolean."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (viewer->ViewCubeHovered == (hovered != 0)) return;
    viewer->ViewCubeHovered = hovered != 0;
    ApplyViewCubeHover(viewer);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_hit_view_cube(
  OcctSharp_ViewerHandle* viewer, const int32_t x, const int32_t y, int32_t* orientation)
{
  if (orientation == nullptr)
  { SetLastError("View cube hit output is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  int32_t action = -1;
  const OcctSharp_Status status = occtsharp_viewer_hit_view_cube_control(viewer, x, y, &action);
  *orientation = action >= 0 && action <= 25 ? action : -1;
  return status;
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_hit_view_cube_control(
  OcctSharp_ViewerHandle* viewer, const int32_t x, const int32_t y, int32_t* action)
{
  if (action == nullptr)
  { SetLastError("View cube control hit output is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *action = -1;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (viewer->ViewCube.IsNull() || !viewer->Context->IsDisplayed(viewer->ViewCube)) return;
    int width = 0, height = 0;
    viewer->Window->Size(width, height);
    // Keep ordinary model clicks out of AIS detection; the cube occupies only this corner.
    if (x < std::max(0, width - 190) || x >= width || y < 0 || y >= std::min(height, 190)) return;
    viewer->Context->MoveTo(x, y, viewer->View, false);
    if (!viewer->Context->HasDetected()) return;
    const auto owner = opencascade::handle<AIS_ViewCubeOwner>::DownCast(viewer->Context->DetectedOwner());
    if (!owner.IsNull() && owner->Selectable() == viewer->ViewCube)
      *action = static_cast<int32_t>(owner->MainOrientation());
    else if (viewer->Context->DetectedInteractive() == viewer->ViewCubeLeftArrow)
      *action = 26;
    else if (viewer->Context->DetectedInteractive() == viewer->ViewCubeRightArrow)
      *action = 27;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_cube_orientation(
  OcctSharp_ViewerHandle* viewer, const int32_t orientation)
{
  if (orientation < 0 || orientation > 25)
  { SetLastError("View cube orientation is outside the 26 supported directions."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->View->SetProj(static_cast<V3d_TypeOfOrientation>(orientation));
    SyncViewCubeRollArrows(viewer);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_get_cube_target_camera(
  OcctSharp_ViewerHandle* viewer, const int32_t orientation, OcctSharp_ViewerCamera* camera)
{
  if (camera == nullptr || orientation < 0 || orientation > 25)
  { SetLastError("Cube target output or orientation is invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *camera = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    *camera = TargetCameraWithoutDisplayChange(viewer, static_cast<V3d_TypeOfOrientation>(orientation));
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
    SyncViewCubeRollArrows(viewer);
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
    OcctSharpGenerated::ViewerAdapters::V3d_View_Eye_Bind(*(viewer->View))(camera->eye.x, camera->eye.y, camera->eye.z);
    OcctSharpGenerated::ViewerAdapters::V3d_View_At_Bind(*(viewer->View))(camera->target.x, camera->target.y, camera->target.z);
    OcctSharpGenerated::ViewerAdapters::V3d_View_Up_Bind(*(viewer->View))(camera->up.x, camera->up.y, camera->up.z);
    OcctSharpGenerated::ViewerAdapters::V3d_View_Proj_Bind(*(viewer->View))(camera->projection.x, camera->projection.y, camera->projection.z);
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
    updated->Copy(OcctSharpGenerated::ViewerAdapters::V3d_View_Camera_Bind(*(viewer->View))());
    updated->SetEyeAndCenter(
      gp_Pnt(camera->eye.x, camera->eye.y, camera->eye.z),
      gp_Pnt(camera->target.x, camera->target.y, camera->target.z));
    OcctSharpGenerated::ViewerAdapters::Graphic3d_Camera_SetUp_Bind(*(updated))(supplied_up);
    viewer->View->SetCamera(updated);
    SyncViewCubeRollArrows(viewer);
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
    OcctSharpGenerated::ViewerAdapters::V3d_View_Convert_Bind(*(viewer->View))(x, y, point->x, point->y, point->z);
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
    OcctSharpGenerated::ViewerAdapters::V3d_View_Convert_Bind(*(viewer->View))(point->x, point->y, point->z, *x, *y);
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
    OcctSharpGenerated::ViewerAdapters::V3d_View_ConvertWithProj_Bind(*(viewer->View))(x, y, ray->origin.x, ray->origin.y, ray->origin.z,
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

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_background_gradient(
  OcctSharp_ViewerHandle* viewer,
  const double top_red, const double top_green, const double top_blue,
  const double bottom_red, const double bottom_green, const double bottom_blue)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(top_red, top_green, top_blue);
    ValidateColor(bottom_red, bottom_green, bottom_blue);
    viewer->View->SetBgGradientColors(
      Quantity_Color(top_red, top_green, top_blue, Quantity_TOC_RGB),
      Quantity_Color(bottom_red, bottom_green, bottom_blue, Quantity_TOC_RGB),
      Aspect_GradientFillMethod_Vertical, true);
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
    if (!OcctSharpGenerated::ViewerAdapters::V3d_View_Dump_Bind(*(viewer->View))(file_path, static_cast<Graphic3d_BufferType>(buffer_type)))
      throw OperationFailure(OCCTSHARP_STATUS_FILE_IO_ERROR, "OCCT failed to write the viewer screenshot.");
  });
}

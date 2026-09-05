#include "Visualization/Context.hxx"
#include "Runtime/Error.hxx"
#include <Graphic3d_Camera.hxx>
#include <gp_Vec.hxx>

using namespace OcctSharp::Native;
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_review_camera(OcctSharp_ViewerHandle* viewer,
  const OcctSharp_ReviewCamera* requested, OcctSharp_ReviewCamera* output) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(output != nullptr, "Null camera output.");
    if (requested) {
      const auto& r = *requested; RenderFlag(r.perspective); RenderFlag(r.auto_depth);
      for (int i = 0; i < 3; ++i) {
        RenderRange(r.eye[i], -1e12, 1e12, "Invalid camera eye.");
        RenderRange(r.target[i], -1e12, 1e12, "Invalid camera target.");
        RenderRange(r.up[i], -1e12, 1e12, "Invalid camera up.");
      }
      RenderRange(r.aspect, 1e-6, 1e6, "Invalid camera aspect.");
      RenderRange(r.scale, 1e-9, 1e12, "Invalid camera scale.");
      RenderRange(r.fov_y, 1e-3, 179, "Invalid vertical field of view in degrees.");
      RenderRange(r.near_plane, r.perspective ? 1e-9 : -1e15, 1e15, "Invalid near clipping plane.");
      RenderRange(r.far_plane, r.near_plane, 1e15, "Invalid far clipping plane.");
      RequireRender(r.far_plane > r.near_plane, "Camera clipping interval is empty.");
      const gp_Pnt eye(r.eye[0],r.eye[1],r.eye[2]), target(r.target[0],r.target[1],r.target[2]);
      const gp_Vec direction(eye,target), up(r.up[0],r.up[1],r.up[2]);
      RequireRender(direction.SquareMagnitude() > 1e-18 && up.SquareMagnitude() > 1e-18 &&
        direction.Crossed(up).SquareMagnitude() > 1e-12 * direction.SquareMagnitude() * up.SquareMagnitude(), "Degenerate camera frame.");
      opencascade::handle<Graphic3d_Camera> camera = new Graphic3d_Camera(viewer->View->Camera());
      camera->SetProjectionType(r.perspective ? Graphic3d_Camera::Projection_Perspective : Graphic3d_Camera::Projection_Orthographic);
      camera->SetEyeAndCenter(eye,target); camera->SetUp(gp_Dir(up)); camera->SetAspect(r.aspect);
      camera->SetFOVy(r.fov_y);
      // Perspective scale is derived from distance and FOV; changing it would move the eye.
      if (!r.perspective) camera->SetScale(r.scale);
      camera->SetZRange(r.near_plane,r.far_plane);
      viewer->View->SetAutoZFitMode(r.auto_depth != 0);
      viewer->View->SetCamera(camera);
    }
    const auto& c = viewer->View->Camera(); OcctSharp_ReviewCamera result{};
    for (int i = 0; i < 3; ++i) { result.eye[i] = c->Eye().Coord(i+1); result.target[i] = c->Center().Coord(i+1); result.up[i] = c->Up().Coord(i+1); }
    result.aspect = c->Aspect(); result.scale = c->Scale(); result.fov_y = c->FOVy();
    result.near_plane = c->ZNear(); result.far_plane = c->ZFar();
    result.perspective = c->IsOrthographic() ? 0 : 1; result.auto_depth = viewer->View->AutoZFitMode() ? 1 : 0;
    *output = result;
  });
}

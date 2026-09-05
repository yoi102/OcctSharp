#include "Visualization/Context.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Validation.hxx"
#include <Graphic3d_CubeMapSeparate.hxx>
#include <Graphic3d_CubeMapPacked.hxx>
#include <Graphic3d_CubeMapOrder.hxx>
#include <Graphic3d_CView.hxx>
#include <unordered_set>

using namespace OcctSharp::Native;
namespace {
void SetEnvironment(OcctSharp_ViewerHandle* viewer, int64_t id, bool background, bool lighting) {
  opencascade::handle<Graphic3d_CubeMap> cube;
  if (id != 0) {
    const auto found = viewer->Rendering.Environments.find(id);
    RequireRender(found != viewer->Rendering.Environments.end(), "Unknown or removed environment."); cube = found->second.Value;
  } else RequireRender(!background && !lighting, "An environment is required to enable its background or lighting.");
  RequireRender(!lighting || ReadRenderCaps(viewer).pbr, "Driver does not support image-based PBR lighting.");
  RequireRender(!lighting || viewer->View->ShadingModel() == Graphic3d_TypeOfShadingModel_Pbr, "Image-based lighting requires a PBR view profile.");
  auto& r = viewer->Rendering;
  const auto previousBackground = viewer->View->View()->BackgroundType();
  const auto oldCube = r.ActiveEnvironment == 0 ? opencascade::handle<Graphic3d_CubeMap>() : r.Environments.at(r.ActiveEnvironment).Value;
  try {
    viewer->View->SetBackgroundCubeMap(cube, lighting, false);
    viewer->View->SetImageBasedLighting(lighting, false); // ClearPBREnvironment is incorrect in OCCT 8.0.1.
    viewer->View->View()->SetBackgroundType(background ? Graphic3d_TOB_CUBEMAP : (r.ActiveEnvironment ? r.SavedBackground : previousBackground));
    viewer->View->ChangeRenderingParams().UseEnvironmentMapBackground = background;
  } catch (...) {
    viewer->View->SetBackgroundCubeMap(oldCube, r.EnvironmentLighting, false);
    viewer->View->SetImageBasedLighting(r.EnvironmentLighting, false);
    viewer->View->View()->SetBackgroundType(previousBackground); throw;
  }
  if (r.ActiveEnvironment == 0 && id != 0) r.SavedBackground = previousBackground;
  r.ActiveEnvironment = id; r.EnvironmentBackground = background; r.EnvironmentLighting = lighting; viewer->View->Invalidate();
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_environment_create(OcctSharp_ViewerHandle* viewer,
  const int64_t* images, int32_t count, const int32_t* order, int64_t* output) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(output && images && (count == 1 || count == 6), "Environment requires one packed or six separate images.");
    RequireRender(viewer->Rendering.Environments.size() < 16, "Environment budget exceeded.");
    ViewerEnvironmentEntry entry;
    for (int i = 0; i < count; ++i) entry.Images.push_back(FindTexture(viewer, images[i]));
    if (count == 6) {
      NCollection_Array1<opencascade::handle<Image_PixMap>> faces(0, 5);
      for (int i = 0; i < 6; ++i) {
        RequireRender(entry.Images[i]->SizeX() == entry.Images[i]->SizeY() && entry.Images[i]->SizeX() == entry.Images[0]->SizeX(), "Cubemap faces must be equally sized squares."); faces.SetValue(i, entry.Images[i]);
      }
      opencascade::handle<Graphic3d_CubeMapSeparate> cube = new Graphic3d_CubeMapSeparate(faces);
      RequireRender(cube->IsDone(), "Invalid six-face cubemap."); entry.Value = cube;
    } else {
      RequireRender(order != nullptr, "Packed cubemap needs six explicit side-to-tile indices.");
      std::unordered_set<int32_t> values; for (int i = 0; i < 6; ++i) RequireRender(order[i] >= 0 && order[i] < 6 && values.insert(order[i]).second, "Packed cubemap order is not a permutation.");
      const auto width = entry.Images[0]->SizeX(), height = entry.Images[0]->SizeY();
      bool valid = false; for (const size_t columns : { 1u, 2u, 3u, 6u }) if (width % columns == 0 && height % (6 / columns) == 0 && width / columns == height / (6 / columns)) valid = true;
      RequireRender(valid, "Packed cubemap must contain six equal square tiles without gaps.");
      const Graphic3d_CubeMapOrder layout(static_cast<unsigned char>(order[0]), static_cast<unsigned char>(order[1]), static_cast<unsigned char>(order[2]),
        static_cast<unsigned char>(order[3]), static_cast<unsigned char>(order[4]), static_cast<unsigned char>(order[5]));
      entry.Value = new Graphic3d_CubeMapPacked(entry.Images[0], layout.Validated());
    }
    entry.Value->SetColorMap(true);
    const int64_t id = viewer->Rendering.NextId; viewer->Rendering.Environments.emplace(id, std::move(entry)); ++viewer->Rendering.NextId; *output = id;
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_environment_set(OcctSharp_ViewerHandle* viewer, int64_t id, int32_t background, int32_t lighting) {
  return Guard([&] { ValidateViewerThread(viewer); RenderFlag(background); RenderFlag(lighting); SetEnvironment(viewer, id, background != 0, lighting != 0); });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_environment_remove(OcctSharp_ViewerHandle* viewer, int64_t id) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(viewer->Rendering.Environments.contains(id), "Unknown or removed environment.");
    if (viewer->Rendering.ActiveEnvironment == id) SetEnvironment(viewer, 0, false, false);
    viewer->Rendering.Environments.erase(id);
  });
}

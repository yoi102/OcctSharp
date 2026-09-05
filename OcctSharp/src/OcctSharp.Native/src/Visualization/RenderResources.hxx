#pragma once
#include "OcctSharp.Native.Rendering.h"
#include <Graphic3d_CLight.hxx>
#include <Graphic3d_CubeMap.hxx>
#include <Graphic3d_Texture2D.hxx>
#include <Image_PixMap.hxx>
#include <Prs3d_Drawer.hxx>
#include <Prs3d_ShadingAspect.hxx>
#include <Graphic3d_TypeOfBackground.hxx>
#include <unordered_map>
#include <vector>

// Parent-owned records only: no global registry and no GPU/context pointer in the ABI.
struct ViewerLightEntry {
  OcctSharp_Light Definition{};
  opencascade::handle<Graphic3d_CLight> Value;
};
struct ViewerAspectEntry {
  opencascade::handle<Prs3d_Drawer> Drawer;
  opencascade::handle<Prs3d_ShadingAspect> Original, Override;
};
struct ViewerAppearanceEntry {
  OcctSharp_Appearance Definition{};
  std::vector<ViewerAspectEntry> Aspects;
  opencascade::handle<Graphic3d_Texture2D> Texture;
};
struct ViewerEnvironmentEntry {
  opencascade::handle<Graphic3d_CubeMap> Value;
  std::vector<opencascade::handle<Image_PixMap>> Images;
};
struct ViewerRenderResources {
  int64_t NextId = 1;
  bool CustomLights = false;
  std::vector<ViewerLightEntry> Lights;
  std::unordered_map<int64_t, opencascade::handle<Image_PixMap>> Textures;
  std::unordered_map<int64_t, ViewerAppearanceEntry> Appearances;
  std::unordered_map<int64_t, ViewerEnvironmentEntry> Environments;
  std::unordered_map<int64_t, int32_t> Layers;
  int64_t ActiveEnvironment = 0;
  bool EnvironmentBackground = false, EnvironmentLighting = false;
  Graphic3d_TypeOfBackground SavedBackground = Graphic3d_TOB_NONE;
};

namespace OcctSharp::Native {
OcctSharp_RenderCaps ReadRenderCaps(OcctSharp_ViewerHandle* viewer);
void CaptureInitialReviewLights(OcctSharp_ViewerHandle* viewer);
void RequireRender(bool condition, const char* message);
void RenderRange(double value, double minimum, double maximum, const char* name);
void RenderFlag(int32_t value);
opencascade::handle<Image_PixMap> FindTexture(OcctSharp_ViewerHandle* viewer, int64_t id);
opencascade::handle<Graphic3d_Texture2D> MakeReviewTexture(OcctSharp_ViewerHandle* viewer,
  const OcctSharp_Appearance& definition, const opencascade::handle<Image_PixMap>& image);
void ResetReviewAppearance(OcctSharp_ViewerHandle* viewer, int64_t presentation);
void ReplaceReviewTexture(OcctSharp_ViewerHandle* viewer, int64_t id,
  const opencascade::handle<Image_PixMap>& image);
int32_t ReviewLayerId(OcctSharp_ViewerHandle* viewer, int64_t id);
}

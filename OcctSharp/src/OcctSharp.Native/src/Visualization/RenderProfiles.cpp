#include "Visualization/Context.hxx"
#include "Visualization/Presentations.hxx"
#include "Runtime/Error.hxx"
#include <OpenGl_Context.hxx>
#include <Graphic3d_RenderingParams.hxx>
#include <Graphic3d_TypeOfLimit.hxx>
#include <Graphic3d_ZLayerSettings.hxx>
#include <cmath>

namespace OcctSharp::Native {
void RequireRender(bool condition, const char* message) {
  if (!condition) throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, message);
}
void RenderRange(double value, double minimum, double maximum, const char* name) {
  RequireRender(std::isfinite(value) && value >= minimum && value <= maximum, name);
}
void RenderFlag(int32_t value) { RequireRender(value == 0 || value == 1, "Invalid Boolean render flag."); }
OcctSharp_RenderCaps ReadRenderCaps(OcctSharp_ViewerHandle* viewer) {
  ValidateViewerThread(viewer);
  const auto& driver = viewer->Driver;
  OcctSharp_RenderCaps c{};
  c.max_lights = driver->InquireLimit(Graphic3d_TypeOfLimit_MaxNbLights);
  c.max_texture = driver->InquireLimit(Graphic3d_TypeOfLimit_MaxTextureSize);
  c.max_dump_x = driver->InquireLimit(Graphic3d_TypeOfLimit_MaxViewDumpSizeX);
  c.max_dump_y = driver->InquireLimit(Graphic3d_TypeOfLimit_MaxViewDumpSizeY);
  c.max_texture_units = driver->InquireLimit(Graphic3d_TypeOfLimit_MaxCombinedTextureUnits);
  c.max_msaa = driver->InquireLimit(Graphic3d_TypeOfLimit_MaxMsaa);
  c.pbr = driver->InquireLimit(Graphic3d_TypeOfLimit_HasPBR);
  c.raytracing = driver->InquireLimit(Graphic3d_TypeOfLimit_HasRayTracing);
  c.srgb = driver->InquireLimit(Graphic3d_TypeOfLimit_HasSRGB);
  c.oit = driver->InquireLimit(Graphic3d_TypeOfLimit_HasBlendedOit);
  c.oit_msaa = driver->InquireLimit(Graphic3d_TypeOfLimit_HasBlendedOitMsaa);
  const auto& context = driver->GetSharedContext();
  c.max_anisotropy = context.IsNull() ? 1.0 : context->MaxDegreeOfAnisotropy();
  return c;
}
int32_t ReviewLayerId(OcctSharp_ViewerHandle* viewer, int64_t id) {
  if (id == 0) return Graphic3d_ZLayerId_Default;
  const auto found = viewer->Rendering.Layers.find(id);
  RequireRender(found != viewer->Rendering.Layers.end(), "Unknown or removed review layer.");
  return found->second;
}
}
using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_render_caps(OcctSharp_ViewerHandle* viewer, OcctSharp_RenderCaps* out) {
  return Guard([&] { RequireRender(out != nullptr, "Null capability output."); const auto value = ReadRenderCaps(viewer); *out = value; });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_render_profile(OcctSharp_ViewerHandle* viewer,
  const OcctSharp_RenderProfile* requested, OcctSharp_RenderProfile* effective) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(effective != nullptr, "Null effective profile output.");
    auto p = viewer->View->RenderingParams();
    if (requested != nullptr) {
      const auto& r = *requested; const auto caps = ReadRenderCaps(viewer);
      RequireRender(r.reserved == 0 && (r.shading == 0 || r.shading == 3 || r.shading == 4), "Invalid view shading pipeline.");
      RequireRender(r.shading != 4 || caps.pbr, "Driver does not support PBR shading.");
      if (r.shading != 4) {
        RequireRender(!viewer->Rendering.EnvironmentLighting, "Disable PBR environment lighting before switching the view pipeline.");
        for (const auto& item : viewer->Rendering.Appearances)
          RequireRender(item.second.Definition.shading != 4, "Reset PBR appearances before switching the view pipeline.");
      }
      RenderFlag(r.mode); RenderRange(r.msaa, 0, caps.max_msaa, "Unsupported MSAA sample count.");
      RequireRender(r.msaa == 0 || (r.msaa >= 2 && (r.msaa & (r.msaa - 1)) == 0), "MSAA must be zero or a power of two >= 2.");
      RenderRange(r.resolution_scale, .25, 4, "Resolution scale must be within .25..4.");
      RequireRender(r.msaa == 0 || r.resolution_scale == 1, "OCCT resolution scaling and MSAA cannot be combined.");
      RenderFlag(r.transparency); RenderFlag(r.tone_mapping);
      RequireRender(r.transparency == 0 || (caps.oit && (r.msaa == 0 || caps.oit_msaa)), "Driver does not support the requested weighted OIT mode.");
      RequireRender(r.mode == 0 || caps.raytracing, "Driver does not support path tracing.");
      RequireRender(r.mode == 1 || (r.tone_mapping == 0 && r.exposure == 0 && r.white_point == 1), "Tone mapping, exposure and white point apply only to path tracing.");
      RequireRender(r.mode == 0 || (r.msaa == 0 && r.transparency == 0), "Path tracing does not use raster MSAA or OIT.");
      RenderRange(r.oit_depth_factor, 0, 1, "Invalid OIT depth factor.");
      RenderRange(r.exposure, -20, 20, "Invalid exposure."); RenderRange(r.white_point, .001, 10000, "Invalid white point.");
      RenderRange(r.environment_power, 1, 10, "Environment power exceeds bake budget.");
      RequireRender((1 << r.environment_power) <= caps.max_texture, "Environment exceeds driver texture limit.");
      RenderRange(r.environment_levels, 2, r.environment_power + 1, "Invalid environment mip count.");
      RenderRange(r.diffuse_samples, 1, 4096, "Invalid diffuse bake sample count.");
      RenderRange(r.specular_samples, 1, 1024, "Invalid specular bake sample count.");
      RenderRange(r.bake_probability, .01, 1, "Invalid bake probability.");
      p.Method = r.mode ? Graphic3d_RM_RAYTRACING : Graphic3d_RM_RASTERIZATION;
      p.IsGlobalIlluminationEnabled = r.mode != 0;
      p.NbMsaaSamples = r.msaa; p.RenderResolutionScale = static_cast<float>(r.resolution_scale);
      p.TransparencyMethod = r.transparency ? Graphic3d_RTM_BLEND_OIT : Graphic3d_RTM_BLEND_UNORDERED;
      p.OitDepthFactor = static_cast<float>(r.oit_depth_factor);
      p.ToneMappingMethod = static_cast<Graphic3d_ToneMappingMethod>(r.tone_mapping);
      p.Exposure = static_cast<float>(r.exposure); p.WhitePoint = static_cast<float>(r.white_point);
      p.PbrEnvPow2Size = r.environment_power; p.PbrEnvSpecMapNbLevels = r.environment_levels;
      p.PbrEnvBakingDiffNbSamples = r.diffuse_samples; p.PbrEnvBakingSpecNbSamples = r.specular_samples;
      p.PbrEnvBakingProbability = static_cast<float>(r.bake_probability);
      viewer->View->ChangeRenderingParams() = p;
      viewer->View->SetShadingModel(static_cast<Graphic3d_TypeOfShadingModel>(r.shading)); viewer->View->Invalidate();
    }
    *effective = { p.Method == Graphic3d_RM_RAYTRACING ? 1 : 0, p.NbMsaaSamples,
      p.TransparencyMethod == Graphic3d_RTM_BLEND_OIT ? 1 : 0, static_cast<int32_t>(p.ToneMappingMethod),
      p.RenderResolutionScale, p.OitDepthFactor, p.Exposure, p.WhitePoint, p.PbrEnvPow2Size,
      p.PbrEnvSpecMapNbLevels, p.PbrEnvBakingDiffNbSamples, p.PbrEnvBakingSpecNbSamples, p.PbrEnvBakingProbability,
      static_cast<int32_t>(viewer->View->ShadingModel()), 0 };
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_layer_set(OcctSharp_ViewerHandle* viewer, int64_t id,
  const OcctSharp_ReviewLayer* input, int64_t* output) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(input && output, "Null layer input/output.");
    RenderFlag(input->depth_test); RenderFlag(input->depth_write); RenderFlag(input->clear_depth); RenderFlag(input->immediate);
    Graphic3d_ZLayerSettings settings; settings.SetEnableDepthTest(input->depth_test != 0);
    settings.SetEnableDepthWrite(input->depth_write != 0); settings.SetClearDepth(input->clear_depth != 0);
    settings.SetImmediate(input->immediate != 0);
    if (id != 0) viewer->Viewer->SetZLayerSettings(ReviewLayerId(viewer, id), settings);
    else {
      RequireRender(viewer->Rendering.Layers.size() < 64, "Review layer budget exceeded.");
      int nativeId = 0; RequireRender(viewer->Viewer->AddZLayer(nativeId, settings), "Could not allocate review layer.");
      id = viewer->Rendering.NextId;
      try { viewer->Rendering.Layers.emplace(id, nativeId); }
      catch (...) { viewer->Viewer->RemoveZLayer(nativeId); throw; }
      ++viewer->Rendering.NextId;
    }
    *output = id;
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_layer_assign(OcctSharp_ViewerHandle* viewer, int64_t presentation, int64_t layer) {
  return Guard([&] { ValidateViewerThread(viewer); const auto shape = FindPresentation(viewer, presentation);
    viewer->Context->SetZLayer(shape, ReviewLayerId(viewer, layer)); });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_layer_remove(OcctSharp_ViewerHandle* viewer, int64_t id) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(id != 0, "Cannot remove the default layer."); const int nativeId = ReviewLayerId(viewer, id);
    for (const auto& item : viewer->Presentations) if (item.second->ZLayer() == nativeId) viewer->Context->SetZLayer(item.second, Graphic3d_ZLayerId_Default);
    RequireRender(viewer->Viewer->RemoveZLayer(nativeId), "Could not remove review layer."); viewer->Rendering.Layers.erase(id);
  });
}

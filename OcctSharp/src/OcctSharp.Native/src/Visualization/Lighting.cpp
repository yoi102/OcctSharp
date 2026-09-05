#include "Visualization/Context.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Validation.hxx"
#include <Quantity_Color.hxx>
#include <unordered_set>

using namespace OcctSharp::Native;
namespace OcctSharp::Native {
void CaptureInitialReviewLights(OcctSharp_ViewerHandle* viewer) {
  for (const auto& light : viewer->View->ActiveLights()) {
    ViewerLightEntry entry; entry.Value = light; auto& d = entry.Definition;
    d.id = viewer->Rendering.NextId++; d.kind = static_cast<int32_t>(light->Type()); d.active = 1;
    d.headlight = light->IsHeadlight() ? 1 : 0; d.red = light->Color().Red(); d.green = light->Color().Green(); d.blue = light->Color().Blue();
    d.intensity = light->Intensity(); d.constant_attenuation = 1; d.dz = -1; d.angle = .523598775598; d.concentration = .5;
    if (d.kind == 1 || d.kind == 3) { const auto dir = light->Direction(); d.dx = dir.X(); d.dy = dir.Y(); d.dz = dir.Z(); }
    if (d.kind == 2 || d.kind == 3) { d.x = light->Position().X(); d.y = light->Position().Y(); d.z = light->Position().Z(); d.constant_attenuation = light->ConstAttenuation(); d.linear_attenuation = light->LinearAttenuation(); d.range = light->Range(); }
    if (d.kind == 3) { d.angle = light->Angle(); d.concentration = light->Concentration(); }
    viewer->Rendering.Lights.push_back(std::move(entry));
  }
}
}
namespace {
ViewerLightEntry MakeLight(const OcctSharp_Light& input) {
  RenderRange(input.kind, 0, 3, "Unknown light type."); RenderFlag(input.active); RenderFlag(input.headlight);
  RequireRender(input.reserved == 0, "Reserved light field must be zero.");
  RenderRange(input.red, 0, 1, "Invalid linear red."); RenderRange(input.green, 0, 1, "Invalid linear green.");
  RenderRange(input.blue, 0, 1, "Invalid linear blue."); RenderRange(input.intensity, .000001, 1000000, "Invalid light intensity.");
  ViewerLightEntry e; e.Definition = input; e.Value = new Graphic3d_CLight(static_cast<Graphic3d_TypeOfLightSource>(input.kind));
  e.Value->SetColor(Quantity_Color(input.red, input.green, input.blue, Quantity_TOC_RGB));
  e.Value->SetIntensity(static_cast<float>(input.intensity));
  // Ambient illumination has no camera-relative direction, and OCCT rejects even
  // SetHeadlight(false) for this light kind.
  if (input.kind == Graphic3d_TOLS_AMBIENT)
    RequireRender(input.headlight == 0, "Ambient lights cannot be headlights.");
  else
    e.Value->SetHeadlight(input.headlight != 0);
  if (input.kind == Graphic3d_TOLS_DIRECTIONAL || input.kind == Graphic3d_TOLS_SPOT) {
    RenderRange(input.dx, -1e12, 1e12, "Invalid light direction."); RenderRange(input.dy, -1e12, 1e12, "Invalid light direction."); RenderRange(input.dz, -1e12, 1e12, "Invalid light direction.");
    RequireRender(input.dx * input.dx + input.dy * input.dy + input.dz * input.dz > 1e-24, "Zero light direction.");
    const gp_Dir dir(input.dx, input.dy, input.dz); e.Value->SetDirection(dir);
    e.Definition.dx = dir.X(); e.Definition.dy = dir.Y(); e.Definition.dz = dir.Z();
  }
  if (input.kind == Graphic3d_TOLS_POSITIONAL || input.kind == Graphic3d_TOLS_SPOT) {
    RenderRange(input.x, -1e12, 1e12, "Invalid light position."); RenderRange(input.y, -1e12, 1e12, "Invalid light position."); RenderRange(input.z, -1e12, 1e12, "Invalid light position.");
    RenderRange(input.constant_attenuation, 0, 1e6, "Invalid attenuation."); RenderRange(input.linear_attenuation, 0, 1e6, "Invalid attenuation.");
    RequireRender(input.constant_attenuation + input.linear_attenuation > 0, "Both attenuation coefficients are zero.");
    RenderRange(input.range, 0, 1e12, "Invalid light range.");
    e.Value->SetPosition(gp_Pnt(input.x, input.y, input.z));
    e.Value->SetAttenuation(static_cast<float>(input.constant_attenuation), static_cast<float>(input.linear_attenuation)); e.Value->SetRange(static_cast<float>(input.range));
  }
  if (input.kind == Graphic3d_TOLS_SPOT) {
    RenderRange(input.angle, .000001, 3.141592, "Spot cone angle must be between zero and pi radians.");
    RenderRange(input.concentration, 0, 1, "Spot concentration must be within zero and one.");
    e.Value->SetAngle(static_cast<float>(input.angle)); e.Value->SetConcentration(static_cast<float>(input.concentration));
  }
  return e;
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_lights_replace(OcctSharp_ViewerHandle* viewer,
  const OcctSharp_Light* inputs, int32_t count, int64_t* ids, int32_t capacity) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(count >= 0 && count <= 128, "Light rig exceeds its budget.");
    ValidateArray(inputs, count, "Null light definitions."); ValidateOutputCapacity(capacity, count, ids, "Insufficient light ID capacity.");
    std::unordered_set<int64_t> oldIds, used; for (const auto& l : viewer->Rendering.Lights) oldIds.insert(l.Definition.id);
    std::vector<ViewerLightEntry> staged; staged.reserve(count); int64_t nextId = viewer->Rendering.NextId; int active = 0;
    for (int i = 0; i < count; ++i) {
      auto light = MakeLight(inputs[i]);
      if (light.Definition.id == 0) light.Definition.id = nextId++;
      else RequireRender(oldIds.contains(light.Definition.id), "Unknown or removed light ID.");
      RequireRender(used.insert(light.Definition.id).second, "Duplicate light ID."); active += light.Definition.active;
      staged.push_back(std::move(light));
    }
    RequireRender(active <= ReadRenderCaps(viewer).max_lights, "Active rig exceeds driver light limit.");
    const auto previous = viewer->View->ActiveLights();
    const auto previousGlobal = viewer->Viewer->ActiveLights();
    try {
      // The OCCT bulk overload removes global membership only after calling the
      // view, which rejects global lights. The single-light overload removes it first.
      for (const auto& light : previousGlobal) viewer->Viewer->SetLightOff(light);
      viewer->View->SetLightOff();
      for (const auto& light : staged) if (light.Definition.active) viewer->View->SetLightOn(light.Value);
    } catch (...) {
      viewer->View->SetLightOff(); for (const auto& light : previousGlobal) viewer->Viewer->SetLightOn(light);
      for (const auto& light : previous) if (!viewer->View->IsActiveLight(light)) viewer->View->SetLightOn(light); throw;
    }
    viewer->Rendering.Lights.swap(staged); viewer->Rendering.NextId = nextId; viewer->Rendering.CustomLights = true;
    for (int i = 0; i < count; ++i) ids[i] = viewer->Rendering.Lights[i].Definition.id;
    viewer->View->Invalidate();
  });
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_lights_snapshot(OcctSharp_ViewerHandle* viewer,
  OcctSharp_Light* output, int32_t capacity, int32_t* count) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(count != nullptr, "Null rig count output.");
    const auto& lights = viewer->Rendering.Lights;
    if (output == nullptr && capacity == 0) { *count = static_cast<int32_t>(lights.size()); return; }
    ValidateOutputCapacity(capacity, static_cast<int32_t>(lights.size()), output, "Insufficient light snapshot capacity.");
    for (size_t i = 0; i < lights.size(); ++i) output[i] = lights[i].Definition;
    *count = static_cast<int32_t>(lights.size());
  });
}

#include "Visualization/Context.hxx"
#include "Visualization/Presentations.hxx"
#include "Runtime/Error.hxx"
#include <BRep_Tool.hxx>
#include <TopoDS.hxx>
#include <TopExp_Explorer.hxx>
#include <Poly_Triangulation.hxx>
#include <Graphic3d_PBRMaterial.hxx>
#include <Graphic3d_Texture2Dplane.hxx>
#include <Graphic3d_TextureParams.hxx>
#include <AIS_ColoredDrawer.hxx>
#include <cmath>

using namespace OcctSharp::Native;
namespace {
Graphic3d_MaterialAspect Material(const OcctSharp_ReviewMaterial& m) {
  RenderRange(m.red, 0, 1, "Invalid material color."); RenderRange(m.green, 0, 1, "Invalid material color.");
  RenderRange(m.blue, 0, 1, "Invalid material color."); RenderRange(m.alpha, 0, 1, "Invalid material alpha.");
  RenderRange(m.metallic, 0, 1, "Invalid metallic value."); RenderRange(m.roughness, 0, 1, "Invalid roughness.");
  RenderRange(m.ior, 1, 3, "Invalid material refraction index."); RenderRange(m.emission, 0, 1000, "Invalid emission.");
  const Quantity_Color color(m.red, m.green, m.blue, Quantity_TOC_RGB);
  Graphic3d_PBRMaterial pbr; pbr.SetColor(Quantity_ColorRGBA(color, static_cast<float>(m.alpha)));
  pbr.SetMetallic(static_cast<float>(m.metallic)); pbr.SetRoughness(static_cast<float>(m.roughness)); pbr.SetIOR(static_cast<float>(m.ior));
  pbr.SetEmission(NCollection_Vec3<float>(static_cast<float>(m.red * m.emission), static_cast<float>(m.green * m.emission), static_cast<float>(m.blue * m.emission)));
  Graphic3d_MaterialAspect result; result.SetDiffuseColor(color); result.SetAmbientColor(color);
  result.SetTransparency(static_cast<float>(1 - m.alpha)); result.SetPBRMaterial(pbr); return result;
}
void ValidateUv(const TopoDS_Shape& shape, bool planar) {
  bool any = false;
  for (TopExp_Explorer e(shape, TopAbs_FACE); e.More(); e.Next()) {
    any = true; if (planar) continue;
    TopLoc_Location location; const auto face = TopoDS::Face(e.Current());
    const auto surface = BRep_Tool::Surface(face, location);
    if (!surface.IsNull()) continue;
    const auto mesh = BRep_Tool::Triangulation(face, location);
    RequireRender(!mesh.IsNull() && mesh->HasUVNodes(), "Presentation contains a discrete face without UV coordinates; use explicit planar mapping.");
  }
  RequireRender(any, "Texture mapping requires a surface or triangulated face.");
}
}
namespace OcctSharp::Native {
opencascade::handle<Graphic3d_Texture2D> MakeReviewTexture(OcctSharp_ViewerHandle* viewer,
  const OcctSharp_Appearance& a, const opencascade::handle<Image_PixMap>& image) {
  RenderFlag(a.planar); RenderFlag(a.repeat); RenderRange(a.filter, 0, 2, "Invalid texture filter.");
  RenderRange(a.anisotropy, 0, 3, "Invalid anisotropy policy.");
  RequireRender(a.anisotropy == 0 || ReadRenderCaps(viewer).max_anisotropy >= 2, "Driver has no anisotropic filtering.");
  RenderRange(a.scale_s, .000001, 1e6, "Invalid texture scale."); RenderRange(a.scale_t, .000001, 1e6, "Invalid texture scale.");
  RenderRange(a.translate_s, -1e6, 1e6, "Invalid texture translation."); RenderRange(a.translate_t, -1e6, 1e6, "Invalid texture translation.");
  RenderRange(a.rotation, -360000, 360000, "Invalid texture rotation (degrees).");
  opencascade::handle<Graphic3d_Texture2D> texture;
  if (a.planar) {
    for (int i = 0; i < 4; ++i) { RenderRange(a.plane_s[i], -1e6, 1e6, "Invalid S plane."); RenderRange(a.plane_t[i], -1e6, 1e6, "Invalid T plane."); }
    const gp_Vec s(a.plane_s[0], a.plane_s[1], a.plane_s[2]), t(a.plane_t[0], a.plane_t[1], a.plane_t[2]);
    RequireRender(s.Crossed(t).SquareMagnitude() > 1e-24, "Texture mapping planes must be independent.");
    opencascade::handle<Graphic3d_Texture2Dplane> plane = new Graphic3d_Texture2Dplane(image);
    plane->SetPlaneS(static_cast<float>(a.plane_s[0]), static_cast<float>(a.plane_s[1]), static_cast<float>(a.plane_s[2]), static_cast<float>(a.plane_s[3]));
    plane->SetPlaneT(static_cast<float>(a.plane_t[0]), static_cast<float>(a.plane_t[1]), static_cast<float>(a.plane_t[2]), static_cast<float>(a.plane_t[3]));
    texture = plane;
  } else texture = new Graphic3d_Texture2D(image);
  texture->SetColorMap(true); const auto& parameters = texture->GetParams();
  parameters->SetModulate(true); parameters->SetRepeat(a.repeat != 0);
  parameters->SetFilter(static_cast<Graphic3d_TypeOfTextureFilter>(a.filter));
  parameters->SetAnisoFilter(static_cast<Graphic3d_LevelOfTextureAnisotropy>(a.anisotropy));
  parameters->SetScale(NCollection_Vec2<float>(static_cast<float>(a.scale_s), static_cast<float>(a.scale_t)));
  parameters->SetTranslation(NCollection_Vec2<float>(static_cast<float>(a.translate_s), static_cast<float>(a.translate_t)));
  parameters->SetRotation(static_cast<float>(a.rotation)); return texture;
}
void ResetReviewAppearance(OcctSharp_ViewerHandle* viewer, int64_t id) {
  auto found = viewer->Rendering.Appearances.find(id); if (found == viewer->Rendering.Appearances.end()) return;
  const auto shape = FindPresentation(viewer, id);
  for (const auto& a : found->second.Aspects) a.Drawer->SetShadingAspect(a.Original);
  try { viewer->Context->Redisplay(shape, false); }
  catch (...) { for (const auto& a : found->second.Aspects) a.Drawer->SetShadingAspect(a.Override); throw; }
  viewer->Rendering.Appearances.erase(found);
}
void ReplaceReviewTexture(OcctSharp_ViewerHandle* viewer, int64_t id, const opencascade::handle<Image_PixMap>& image) {
  struct Change { ViewerAppearanceEntry* Entry; opencascade::handle<AIS_ColoredShape> Shape; opencascade::handle<Graphic3d_Texture2D> Texture; opencascade::handle<Graphic3d_TextureSet> Set; };
  std::vector<Change> staged;
  for (auto& item : viewer->Rendering.Appearances) if (item.second.Definition.texture == id) {
    opencascade::handle<Graphic3d_Texture2D> texture;
    opencascade::handle<Graphic3d_TextureSet> set;
    if (!image.IsNull()) { texture = MakeReviewTexture(viewer, item.second.Definition, image); set = new Graphic3d_TextureSet(texture); }
    staged.push_back({ &item.second, FindPresentation(viewer, item.first), texture, set });
  }
  // Every allocating step completed. The following handle swaps cannot partially allocate.
  for (const auto& change : staged) {
    change.Entry->Texture = change.Texture;
    if (image.IsNull()) change.Entry->Definition.texture = 0;
    for (const auto& a : change.Entry->Aspects) {
      a.Override->Aspect()->SetTextureSet(change.Set); a.Override->Aspect()->SetTextureMapOn(!change.Set.IsNull());
    }
    change.Shape->SynchronizeAspects();
  }
  viewer->View->Invalidate();
}
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_appearance(OcctSharp_ViewerHandle* viewer, int64_t id, const OcctSharp_Appearance* profile) {
  return Guard([&] {
    ValidateViewerThread(viewer); const auto shape = FindPresentation(viewer, id);
    if (!profile) { ResetReviewAppearance(viewer, id); return; }
    const auto& p = *profile;
    RequireRender(p.shading == 0 || p.shading == 3 || p.shading == 4, "Only Unlit, Phong and PBR review shading are supported.");
    RequireRender(p.shading != 4 || ReadRenderCaps(viewer).pbr, "Driver does not support PBR.");
    RequireRender(p.shading != 4 || viewer->View->ShadingModel() == Graphic3d_TypeOfShadingModel_Pbr, "PBR appearances require a PBR view profile.");
    RequireRender(!p.distinguish || p.shading != 0, "Unlit shading uses the front color on both sides; choose Phong or PBR for independent side materials.");
    RenderFlag(p.distinguish); RenderRange(p.culling, 0, 3, "Invalid face-culling policy.");
    RenderRange(p.alpha_mode, 0, 3, "Invalid alpha interpretation."); RenderRange(p.alpha_cutoff, 0, 1, "Invalid alpha cutoff.");
    const auto front = Material(p.front), back = Material(p.back);
    ViewerAppearanceEntry staged; staged.Definition = p;
    opencascade::handle<Graphic3d_TextureSet> set;
    if (p.texture != 0) { ValidateUv(shape->Shape(), p.planar != 0); staged.Texture = MakeReviewTexture(viewer, p, FindTexture(viewer, p.texture)); set = new Graphic3d_TextureSet(staged.Texture); }
    const auto previous = viewer->Rendering.Appearances.find(id);
    if (previous != viewer->Rendering.Appearances.end()) staged.Aspects = previous->second.Aspects;
    else {
      const auto add = [&](const opencascade::handle<Prs3d_Drawer>& drawer) {
        staged.Aspects.push_back({ drawer, drawer->HasOwnShadingAspect() ? drawer->ShadingAspect() : opencascade::handle<Prs3d_ShadingAspect>(), {} });
      };
      add(shape->Attributes());
      for (const auto& drawer : shape->CustomAspectsMap()) add(drawer);
    }
    for (auto& a : staged.Aspects) {
      opencascade::handle<Graphic3d_AspectFillArea3d> fill = new Graphic3d_AspectFillArea3d(*a.Drawer->ShadingAspect()->Aspect());
      fill->SetShadingModel(static_cast<Graphic3d_TypeOfShadingModel>(p.shading));
      fill->SetFrontMaterial(front); fill->SetBackMaterial(back); fill->SetDistinguish(p.distinguish != 0);
      fill->SetInteriorColor(Quantity_ColorRGBA(Quantity_Color(p.front.red, p.front.green, p.front.blue, Quantity_TOC_RGB), static_cast<float>(p.front.alpha)));
      fill->SetBackInteriorColor(Quantity_ColorRGBA(Quantity_Color(p.back.red, p.back.green, p.back.blue, Quantity_TOC_RGB), static_cast<float>(p.back.alpha)));
      fill->SetFaceCulling(static_cast<Graphic3d_TypeOfBackfacingModel>(p.culling));
      fill->SetAlphaMode(static_cast<Graphic3d_AlphaMode>(p.alpha_mode), static_cast<float>(p.alpha_cutoff));
      fill->SetTextureSet(set); fill->SetTextureMapOn(!set.IsNull()); a.Override = new Prs3d_ShadingAspect(fill);
    }
    std::vector<opencascade::handle<Prs3d_ShadingAspect>> old;
    for (const auto& a : staged.Aspects) old.push_back(a.Drawer->HasOwnShadingAspect() ? a.Drawer->ShadingAspect() : opencascade::handle<Prs3d_ShadingAspect>());
    const bool inserted = previous == viewer->Rendering.Appearances.end();
    auto [entry, unused] = viewer->Rendering.Appearances.try_emplace(id);
    (void)unused;
    for (const auto& a : staged.Aspects) a.Drawer->SetShadingAspect(a.Override);
    try { viewer->Context->Redisplay(shape, false); }
    catch (...) { for (size_t i = 0; i < old.size(); ++i) staged.Aspects[i].Drawer->SetShadingAspect(old[i]); if (inserted) viewer->Rendering.Appearances.erase(id); throw; }
    entry->second = std::move(staged);
  });
}

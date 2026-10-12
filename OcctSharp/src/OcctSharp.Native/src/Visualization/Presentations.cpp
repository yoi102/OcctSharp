#include "../../generated/Visualization/OcctSharp.ConsumerViewerSources.Generated.hxx"
#include "../../generated/Visualization/OcctSharp.ConsumerSharedPresentations.Generated.hxx"
#include "../../generated/Visualization/OcctSharp.Visualization.ViewerAdapters.Generated.hxx"
#include "../../generated/Xde/OcctSharp.Xde.XdeAdapters.Generated.hxx"
// Native Visualization/Presentations implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Geometry/Conversions.hxx"
#include "Geometry/Transforms.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Manipulators.hxx"
#include "Visualization/Presentations.hxx"
#include "Xde/Metadata.hxx"
#include <AIS_ColoredShape.hxx>
#include <Graphic3d_ZLayerId.hxx>
#include <Quantity_Color.hxx>
#include <Quantity_ColorRGBA.hxx>
#include <Standard_Handle.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopExp_Explorer.hxx>
#include <TopLoc_Location.hxx>
#include <TopoDS_Shape.hxx>
#include <XCAFDoc_ShapeTool.hxx>
#include <XCAFDoc_VisMaterial.hxx>
#include <XCAFPrs_Style.hxx>
#include <cmath>

namespace OcctSharp::Native
{
opencascade::handle<AIS_ColoredShape> FindPresentation(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t presentationId)
{
  const auto iterator = viewer->Presentations.find(presentationId);
  if (iterator == viewer->Presentations.end())
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer presentation ID does not exist.");
  }
  return iterator->second;
}

int64_t FindPresentationId(
  const OcctSharp_ViewerHandle* viewer,
  const opencascade::handle<AIS_InteractiveObject>& presentation)
{
  for (const auto& candidate : viewer->Presentations)
  {
    if (candidate.second == presentation) return candidate.first;
  }
  throw OperationFailure(
    OCCTSHARP_STATUS_OCCT_FAILURE,
    "The detected AIS object is outside the managed presentation registry.");
}

void ValidateSubshape(const opencascade::handle<AIS_ColoredShape>& presentation, const OcctSharp_ShapeHandle* subshape)
{ ValidateUsableShape(subshape); ValidateSubshape(presentation, subshape->Value); }

void ValidateSubshape(
  const opencascade::handle<AIS_ColoredShape>& presentation,
  const TopoDS_Shape& subshape)
{
  const TopoDS_Shape root = OcctSharpGenerated::ViewerAdapters::AIS_Shape_Shape_Bind(*(presentation))();
  bool contains = root.IsSame(subshape);
  if (!contains)
  {
    for (TopExp_Explorer explorer(root, subshape.ShapeType()); explorer.More(); explorer.Next())
    {
      if (explorer.Current().IsSame(subshape))
      {
        contains = true;
        break;
      }
    }
  }
  if (!contains)
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "The supplied topology is not a member of the presentation shape.");
  }
}

bool TryGetXdeStyleColor(
  const XCAFPrs_Style& style,
  const TopAbs_ShapeEnum shapeType,
  Quantity_ColorRGBA& color)
{
  const bool curveFirst = shapeType == TopAbs_EDGE || shapeType == TopAbs_WIRE;
  if (curveFirst && style.IsSetColorCurv())
  {
    color = Quantity_ColorRGBA(style.GetColorCurv());
    return true;
  }
  if (style.IsSetColorSurf())
  {
    color = style.GetColorSurfRGBA();
    return true;
  }

  const opencascade::handle<XCAFDoc_VisMaterial>& material = style.Material();
  if (!material.IsNull() && material->HasPbrMaterial())
  {
    color = OcctSharpGenerated::XdeAdapters::XCAFDoc_VisMaterial_PbrMaterial_Bind(*(material))().BaseColor;
    return true;
  }
  if (!material.IsNull() && material->HasCommonMaterial())
  {
    const XCAFDoc_VisMaterialCommon& common = material->CommonMaterial();
    color = Quantity_ColorRGBA(
      common.DiffuseColor, static_cast<float>(1.0 - common.Transparency));
    return true;
  }
  if (style.IsSetColorCurv())
  {
    color = Quantity_ColorRGBA(style.GetColorCurv());
    return true;
  }
  return false;
}

void ApplyXdePresentationStyles(
  const opencascade::handle<AIS_ColoredShape>& presentation,
  const XdePresentationStyleMap& settings)
{
  const TopoDS_Shape root = OcctSharpGenerated::ViewerAdapters::AIS_Shape_Shape_Bind(*(presentation))();
  for (int32_t index = 1; index <= settings.Extent(); ++index)
  {
    const TopoDS_Shape& styledShape = settings.FindKey(index);
    bool contains = root.IsSame(styledShape);
    if (!contains)
    {
      for (TopExp_Explorer explorer(root, styledShape.ShapeType()); explorer.More(); explorer.Next())
      {
        if (explorer.Current().IsSame(styledShape))
        {
          contains = true;
          break;
        }
      }
    }
    if (!contains) continue;

    const XCAFPrs_Style& style = settings.FindFromIndex(index);
    if (!style.IsVisible())
    {
      OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_SetCustomTransparency_Bind(*(presentation))(styledShape, 1.0);
      continue;
    }

    Quantity_ColorRGBA color;
    if (!TryGetXdeStyleColor(style, styledShape.ShapeType(), color)) continue;
    OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_SetCustomColor_Bind(*(presentation))(styledShape, OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_GetRGB_Bind(color)());
    if (OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_Alpha_Bind(color)() < 1.0f)
      OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_SetCustomTransparency_Bind(*(presentation))(styledShape, 1.0 - OcctSharpGenerated::XdeAdapters::Quantity_ColorRGBA_Alpha_Bind(color)());
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_display_shape(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_ShapeHandle* shape,
  int64_t* presentation_id)
{
  if (presentation_id == nullptr)
  {
    SetLastError("The viewer presentation ID output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *presentation_id = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateUsableShape(shape);
    opencascade::handle<AIS_ColoredShape> presentation = new AIS_ColoredShape(shape->Value);
    const int64_t id = viewer->NextPresentationId++;
    viewer->Presentations.emplace(id, presentation);
    viewer->Context->Display(presentation, false);
    *presentation_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_display_xde_label(
  OcctSharp_ViewerHandle* viewer,
  const OcctSharp_OcafDocumentHandle* document,
  const char* entry,
  int64_t* presentation_id)
{
  if (presentation_id == nullptr)
  {
    SetLastError("The XDE viewer presentation ID output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *presentation_id = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    TopoDS_Shape shape;
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!XCAFDoc_ShapeTool::GetShape(label, shape) || shape.IsNull())
      throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The XDE label contains no shape.");

    const opencascade::handle<AIS_ColoredShape> presentation = new AIS_ColoredShape(shape);
    ApplyXdePresentationStyles(presentation, CollectXdePresentationStyles(document, entry));
    const int64_t id = viewer->NextPresentationId++;
    viewer->Presentations.emplace(id, presentation);
    viewer->Context->Display(presentation, false);
    *presentation_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_visible(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const int32_t visible)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    if (visible != 0) viewer->Context->Display(presentation, false);
    else viewer->Context->Erase(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_color(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const double red,
  const double green,
  const double blue)
{
  if (!std::isfinite(red) || !std::isfinite(green) || !std::isfinite(blue)
      || red < 0.0 || red > 1.0 || green < 0.0 || green > 1.0 || blue < 0.0 || blue > 1.0)
  {
    SetLastError("Viewer RGB components must be finite values in the inclusive range 0 to 1.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    OcctSharpGenerated::ViewerAdapters::AIS_InteractiveContext_SetColor_Bind(*(viewer->Context))(presentation, Quantity_Color(red, green, blue, Quantity_TOC_RGB), false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_transparency(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const double transparency)
{
  if (!std::isfinite(transparency) || transparency < 0.0 || transparency > 1.0)
  {
    SetLastError("Viewer transparency must be a finite value in the inclusive range 0 to 1.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->SetTransparency(presentation, transparency, false);
    if(transparency==0.0 && dynamic_cast<ViewerSharedPresentation*>(presentation.get()))
    { presentation->SetTransparency(0.0); viewer->Context->RecomputePrsOnly(presentation,false); }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_display_mode(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const int32_t display_mode)
{
  if (display_mode < 0 || display_mode > 1)
  {
    SetLastError("Viewer display mode must be wireframe or shaded.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->SetDisplayMode(presentation, display_mode, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_overlay(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id, const int32_t enabled)
{
  if (enabled != 0 && enabled != 1)
  { SetLastError("Viewer overlay flag must be zero or one."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->SetZLayer(presentation,
      enabled != 0 ? Graphic3d_ZLayerId_Topmost : Graphic3d_ZLayerId_Default);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_presentation_selection_kind(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id, const int32_t shape_kind)
{
  if (shape_kind < -1 || shape_kind > 7)
  { SetLastError("Viewer selection kind must be whole-object or a TopAbs kind from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->Deactivate(presentation);
    const int mode = shape_kind < 0
      ? 0
      : AIS_Shape::SelectionMode(static_cast<TopAbs_ShapeEnum>(shape_kind));
    viewer->Context->Activate(presentation, mode, true);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_remove_presentation(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const opencascade::handle<AIS_ColoredShape> presentation = FindPresentation(viewer, presentation_id);
    DetachManipulatorsForPresentation(viewer, presentation_id);
    viewer->Context->Remove(presentation, false);
    viewer->Rendering.Appearances.erase(presentation_id);
    viewer->Presentations.erase(presentation_id);
    viewer->InstanceSources.erase(presentation_id);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_presentation_get_transform(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  OcctSharp_TrsfHandle** transform)
{
  if (transform == nullptr)
  { SetLastError("The presentation transform output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *transform = nullptr;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    *transform = AllocateTransform(OcctSharpGenerated::ViewerAdapters::AIS_InteractiveContext_Location_Bind(*(viewer->Context))(presentation).Transformation());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_presentation_set_transform(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id,
  const OcctSharp_TrsfHandle* transform)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateTransformHandle(transform);
    const auto presentation = FindPresentation(viewer, presentation_id);
    OcctSharpGenerated::ViewerAdapters::AIS_InteractiveContext_SetLocation_Bind(*(viewer->Context))(presentation, TopLoc_Location(transform->Value));
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_presentation_reset_transform(
  OcctSharp_ViewerHandle* viewer,
  const int64_t presentation_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->ResetLocation(presentation);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_color(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape, const double red, const double green, const double blue)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    ValidateColor(red, green, blue);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateUsableShape(subshape);
    const auto copied=ResolveSourceSubshape(viewer,presentation_id,subshape->Value);
    ValidateSubshape(presentation,copied);
    if(auto* shared=dynamic_cast<ViewerSharedPresentation*>(presentation.get())) shared->SetSourceSubshapeColor(copied,Quantity_Color(red, green, blue, Quantity_TOC_RGB));
    else OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_SetCustomColor_Bind(*(presentation))(copied, Quantity_Color(red, green, blue, Quantity_TOC_RGB));
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_transparency(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape, const double transparency)
{
  if (!std::isfinite(transparency) || transparency < 0.0 || transparency > 1.0)
  { SetLastError("Viewer subshape transparency must be from 0 through 1."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateUsableShape(subshape);
    const auto copied=ResolveSourceSubshape(viewer,presentation_id,subshape->Value);
    ValidateSubshape(presentation,copied);
    if(auto* shared=dynamic_cast<ViewerSharedPresentation*>(presentation.get())) shared->SetSourceSubshapeTransparency(copied,transparency);
    else OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_SetCustomTransparency_Bind(*(presentation))(copied, transparency);
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_subshape_width(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape, const double width)
{
  if (!std::isfinite(width) || width <= 0.0 || width > 1000.0)
  { SetLastError("Viewer subshape width must be finite, positive, and no greater than 1000."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateUsableShape(subshape);
    const auto copied=ResolveSourceSubshape(viewer,presentation_id,subshape->Value);
    ValidateSubshape(presentation,copied);
    if(auto* shared=dynamic_cast<ViewerSharedPresentation*>(presentation.get())) shared->SetSourceSubshapeWidth(copied,width);
    else OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_SetCustomWidth_Bind(*(presentation))(copied, width);
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_subshape_overrides(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id,
  const OcctSharp_ShapeHandle* subshape)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    ValidateUsableShape(subshape);
    const auto copied=ResolveSourceSubshape(viewer,presentation_id,subshape->Value);
    ValidateSubshape(presentation,copied);
    if(ClearSourceSubshapeOverrides(viewer,presentation_id,&copied)) return;
    MakePresentationIndependent(presentation);
    OcctSharpGenerated::ViewerAdapters::AIS_ColoredShape_UnsetCustomAspects_Bind(*(presentation))(copied, true);
    viewer->Context->Redisplay(presentation, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_all_subshape_overrides(
  OcctSharp_ViewerHandle* viewer, const int64_t presentation_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto presentation = FindPresentation(viewer, presentation_id);
    if(ClearSourceSubshapeOverrides(viewer,presentation_id,nullptr)) return;
    MakePresentationIndependent(presentation);
    presentation->ClearCustomAspects();
    viewer->Context->Redisplay(presentation, false);
  });
}

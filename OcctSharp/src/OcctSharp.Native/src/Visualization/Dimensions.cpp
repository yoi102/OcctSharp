// Native Visualization/Dimensions implementation. Public contracts and ownership are unchanged.
#include "Geometry/Conversions.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Dimensions.hxx"
#include <PrsDim_AngleDimension.hxx>
#include <PrsDim_DiameterDimension.hxx>
#include <PrsDim_Dimension.hxx>
#include <PrsDim_LengthDimension.hxx>
#include <PrsDim_RadiusDimension.hxx>
#include <Quantity_Color.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_AsciiString.hxx>
#include <cmath>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>

namespace OcctSharp::Native
{
opencascade::handle<PrsDim_Dimension> FindDimension(
  const OcctSharp_ViewerHandle* viewer,
  const int64_t dimensionId)
{
  const auto iterator = viewer->Dimensions.find(dimensionId);
  if (iterator == viewer->Dimensions.end())
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer dimension ID does not exist.");
  return iterator->second;
}

void ConfigureDimension(
  const opencascade::handle<PrsDim_Dimension>& dimension,
  const char* modelUnits,
  const char* displayUnits,
  const int32_t hasCustomValue,
  const double customValue,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double lineWidth)
{
  if ((hasCustomValue != 0 && hasCustomValue != 1) || !std::isfinite(customValue)
      || !std::isfinite(flyout) || !std::isfinite(lineWidth) || lineWidth <= 0.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer dimension style values are invalid.");
  if (!std::isfinite(red) || !std::isfinite(green) || !std::isfinite(blue)
      || red < 0.0 || red > 1.0 || green < 0.0 || green > 1.0 || blue < 0.0 || blue > 1.0)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Viewer dimension RGB values are invalid.");
  dimension->SetModelUnits(TCollection_AsciiString(modelUnits == nullptr ? "" : modelUnits));
  dimension->SetDisplayUnits(TCollection_AsciiString(displayUnits == nullptr ? "" : displayUnits));
  if (hasCustomValue != 0) dimension->SetCustomValue(customValue);
  else dimension->SetComputedValue();
  dimension->SetFlyout(flyout);
  dimension->SetColor(Quantity_Color(red, green, blue, Quantity_TOC_RGB));
  dimension->SetWidth(lineWidth);
  dimension->SetToUpdate();
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_create(
  OcctSharp_ViewerHandle* viewer,
  const int32_t kind,
  const OcctSharp_ShapeHandle* shape,
  const OcctSharp_Xyz* points,
  const int32_t point_count,
  const OcctSharp_PlaneEquation* plane,
  const char* model_units,
  const char* display_units,
  const int32_t has_custom_value,
  const double custom_value,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double line_width,
  int64_t* dimension_id)
{
  if (dimension_id == nullptr)
  { SetLastError("The viewer dimension ID output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *dimension_id = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    opencascade::handle<PrsDim_Dimension> dimension;
    if (kind == 0)
    {
      if (points == nullptr || point_count != 2 || plane == nullptr
          || !IsFinite(points[0]) || !IsFinite(points[1]))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A length dimension requires two finite points and one plane.");
      dimension = new PrsDim_LengthDimension(
        gp_Pnt(points[0].x, points[0].y, points[0].z),
        gp_Pnt(points[1].x, points[1].y, points[1].z),
        gp_Pln(plane->a, plane->b, plane->c, plane->d));
    }
    else if (kind == 1)
    {
      if (points == nullptr || point_count != 3
          || !IsFinite(points[0]) || !IsFinite(points[1]) || !IsFinite(points[2]))
        throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "An angle dimension requires three finite points.");
      dimension = new PrsDim_AngleDimension(
        gp_Pnt(points[0].x, points[0].y, points[0].z),
        gp_Pnt(points[1].x, points[1].y, points[1].z),
        gp_Pnt(points[2].x, points[2].y, points[2].z));
    }
    else if (kind == 2 || kind == 3)
    {
      ValidateUsableShape(shape);
      dimension = kind == 2
        ? opencascade::handle<PrsDim_Dimension>(new PrsDim_RadiusDimension(shape->Value))
        : opencascade::handle<PrsDim_Dimension>(new PrsDim_DiameterDimension(shape->Value));
    }
    else throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer dimension kind is unsupported.");
    ConfigureDimension(dimension, model_units, display_units, has_custom_value, custom_value,
      flyout, red, green, blue, line_width);
    const int64_t id = viewer->NextDimensionId++;
    viewer->Dimensions.emplace(id, dimension);
    viewer->Context->Display(dimension, false);
    *dimension_id = id;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_update_style(
  OcctSharp_ViewerHandle* viewer,
  const int64_t dimension_id,
  const char* model_units,
  const char* display_units,
  const int32_t has_custom_value,
  const double custom_value,
  const double flyout,
  const double red,
  const double green,
  const double blue,
  const double line_width)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    ConfigureDimension(dimension, model_units, display_units, has_custom_value, custom_value,
      flyout, red, green, blue, line_width);
    viewer->Context->Redisplay(dimension, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_set_visible(
  OcctSharp_ViewerHandle* viewer, const int64_t dimension_id, const int32_t visible)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    if (visible != 0) viewer->Context->Display(dimension, false);
    else viewer->Context->Erase(dimension, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_set_selected(
  OcctSharp_ViewerHandle* viewer, const int64_t dimension_id, const int32_t selected)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    const bool isSelected = viewer->Context->IsSelected(dimension);
    if (selected != 0 && !isSelected) viewer->Context->SetSelected(dimension, false);
    else if (selected == 0 && isSelected) viewer->Context->AddOrRemoveSelected(dimension, false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_dimension_remove(
  OcctSharp_ViewerHandle* viewer, const int64_t dimension_id)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const auto dimension = FindDimension(viewer, dimension_id);
    viewer->Context->Remove(dimension, false);
    viewer->Dimensions.erase(dimension_id);
  });
}

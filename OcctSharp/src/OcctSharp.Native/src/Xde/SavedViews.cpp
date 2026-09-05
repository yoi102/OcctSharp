// Native Xde/SavedViews implementation. Public contracts and ownership are unchanged.
#include "Documents/Lifecycle.hxx"
#include "Geometry/Conversions.hxx"
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Xde/Pmi.hxx"
#include "Xde/SavedViews.hxx"
#include <NCollection_Sequence.hxx>
#include <Standard_Handle.hxx>
#include <TCollection_HAsciiString.hxx>
#include <XCAFDoc_View.hxx>
#include <XCAFView_Object.hxx>
#include <cmath>
#include <gp_Dir.hxx>
#include <gp_Pln.hxx>
#include <gp_Pnt.hxx>

namespace OcctSharp::Native
{
void ValidateSavedView(const OcctSharp_SavedView& data)
{
  const auto finite_xyz = [](const OcctSharp_Xyz& value)
  {
    return std::isfinite(value.x) && std::isfinite(value.y) && std::isfinite(value.z);
  };
  const auto square_magnitude = [](const OcctSharp_Xyz& value)
  {
    return value.x * value.x + value.y * value.y + value.z * value.z;
  };
  if (data.projection_type < static_cast<int32_t>(XCAFView_ProjectionType_NoCamera)
      || data.projection_type > static_cast<int32_t>(XCAFView_ProjectionType_Central)
      || !finite_xyz(data.projection_point) || !finite_xyz(data.view_direction)
      || !finite_xyz(data.up_direction) || square_magnitude(data.view_direction) <= 1.0e-24
      || square_magnitude(data.up_direction) <= 1.0e-24
      || !std::isfinite(data.zoom_factor) || data.zoom_factor <= 0.0
      || !std::isfinite(data.window_horizontal_size) || data.window_horizontal_size <= 0.0
      || !std::isfinite(data.window_vertical_size) || data.window_vertical_size <= 0.0
      || !std::isfinite(data.front_clipping_distance)
      || !std::isfinite(data.back_clipping_distance))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Saved-view values are invalid or non-finite.");
  const gp_Dir view(data.view_direction.x, data.view_direction.y, data.view_direction.z);
  const gp_Dir up(data.up_direction.x, data.up_direction.y, data.up_direction.z);
  if (std::abs(view.Dot(up)) > 0.999999)
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Saved-view up and view directions cannot be parallel.");
}

void SetSavedViewObject(
  const TDF_Label& label, const OcctSharp_SavedView& data,
  const char* name, const char* clippingExpression)
{
  ValidateSavedView(data);
  auto object = new XCAFView_Object();
  object->SetName(MakePmiString(name));
  object->SetType(static_cast<XCAFView_ProjectionType>(data.projection_type));
  object->SetProjectionPoint(gp_Pnt(data.projection_point.x, data.projection_point.y, data.projection_point.z));
  object->SetViewDirection(gp_Dir(data.view_direction.x, data.view_direction.y, data.view_direction.z));
  object->SetUpDirection(gp_Dir(data.up_direction.x, data.up_direction.y, data.up_direction.z));
  object->SetZoomFactor(data.zoom_factor);
  object->SetWindowHorizontalSize(data.window_horizontal_size);
  object->SetWindowVerticalSize(data.window_vertical_size);
  object->SetClippingExpression(MakePmiString(clippingExpression));
  if (data.has_front_clipping) object->SetFrontPlaneDistance(data.front_clipping_distance);
  else object->UnsetFrontPlaneClipping();
  if (data.has_back_clipping) object->SetBackPlaneDistance(data.back_clipping_distance);
  else object->UnsetBackPlaneClipping();
  object->SetViewVolumeSidesClipping(data.has_view_volume_sides_clipping != 0);
  XCAFDoc_View::Set(label)->SetObject(object);
}

NCollection_Sequence<TDF_Label> AddSavedViewPlanes(
  const OcctSharp_OcafDocumentHandle* document,
  const OcctSharp_PlaneEquation* planes, const int32_t planeCount)
{
  if (planeCount < 0 || (planeCount > 0 && planes == nullptr))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The saved-view clipping-plane array is invalid.");
  NCollection_Sequence<TDF_Label> labels;
  const auto tool = GetClippingPlaneTool(document);
  for (int32_t index = 0; index < planeCount; ++index)
  {
    const auto& plane = planes[index];
    if (!std::isfinite(plane.a) || !std::isfinite(plane.b)
        || !std::isfinite(plane.c) || !std::isfinite(plane.d))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "Saved-view clipping-plane values must be finite.");
    labels.Append(tool->AddClippingPlane(
      gp_Pln(plane.a, plane.b, plane.c, plane.d),
      new TCollection_HAsciiString("OcctSharp saved view"), plane.capping != 0));
  }
  return labels;
}

void RemoveUnreferencedPlanes(
  const OcctSharp_OcafDocumentHandle* document,
  const NCollection_Sequence<TDF_Label>& labels)
{
  const auto tool = GetClippingPlaneTool(document);
  for (NCollection_Sequence<TDF_Label>::Iterator iterator(labels); iterator.More(); iterator.Next())
    tool->RemoveClippingPlane(iterator.Value());
}

opencascade::handle<XCAFView_Object> GetSavedViewObject(
  const OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  opencascade::handle<XCAFDoc_View> attribute;
  const TDF_Label label = ResolveOcafLabel(document, entry);
  if (!label.FindAttribute(XCAFDoc_View::GetID(), attribute))
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
  const auto object = attribute->GetObject();
  if (object.IsNull()) throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The saved view has no value object.");
  return object;
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_create(
  OcctSharp_OcafDocumentHandle* document, const OcctSharp_SavedView* data,
  const char* name, const char* clipping_expression,
  const char* shape_entries, const char* pmi_entries,
  const OcctSharp_PlaneEquation* planes, const int32_t plane_count,
  char* buffer, const int32_t capacity, int32_t* written)
{
  if (data == nullptr)
  { SetLastError("The saved-view data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = viewTool->AddView();
    SetSavedViewObject(label, *data, name, clipping_expression);
    const NCollection_Sequence<TDF_Label> shapes = ResolveEntries(document, shape_entries);
    const NCollection_Sequence<TDF_Label> pmi = ResolveEntries(document, pmi_entries);
    const NCollection_Sequence<TDF_Label> clippingPlanes = AddSavedViewPlanes(document, planes, plane_count);
    viewTool->SetView(shapes, pmi, clippingPlanes, label);
    CopyLabelEntry(label, buffer, capacity, written);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_update(
  OcctSharp_OcafDocumentHandle* document, const char* entry, const OcctSharp_SavedView* data,
  const char* name, const char* clipping_expression,
  const char* shape_entries, const char* pmi_entries,
  const OcctSharp_PlaneEquation* planes, const int32_t plane_count)
{
  if (data == nullptr)
  { SetLastError("The saved-view data pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    NCollection_Sequence<TDF_Label> previousPlanes;
    viewTool->GetRefClippingPlaneLabel(label, previousPlanes);
    SetSavedViewObject(label, *data, name, clipping_expression);
    const NCollection_Sequence<TDF_Label> shapes = ResolveEntries(document, shape_entries);
    const NCollection_Sequence<TDF_Label> pmi = ResolveEntries(document, pmi_entries);
    const NCollection_Sequence<TDF_Label> clippingPlanes = AddSavedViewPlanes(document, planes, plane_count);
    viewTool->SetView(shapes, pmi, clippingPlanes, label);
    RemoveUnreferencedPlanes(document, previousPlanes);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_get(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  OcctSharp_SavedView* data, int32_t* plane_count)
{
  if (data == nullptr || plane_count == nullptr)
  { SetLastError("A saved-view snapshot output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *data = {}; *plane_count = 0;
  return Guard([&]
  {
    const auto object = GetSavedViewObject(document, entry);
    data->projection_type = static_cast<int32_t>(object->Type());
    data->projection_point = CopyPoint(object->ProjectionPoint());
    const gp_Dir viewDirection = object->ViewDirection();
    data->view_direction = { viewDirection.X(), viewDirection.Y(), viewDirection.Z() };
    const gp_Dir upDirection = object->UpDirection();
    data->up_direction = { upDirection.X(), upDirection.Y(), upDirection.Z() };
    data->zoom_factor = object->ZoomFactor();
    data->window_horizontal_size = object->WindowHorizontalSize();
    data->window_vertical_size = object->WindowVerticalSize();
    data->has_front_clipping = object->HasFrontPlaneClipping() ? 1 : 0;
    if (data->has_front_clipping) data->front_clipping_distance = object->FrontPlaneDistance();
    data->has_back_clipping = object->HasBackPlaneClipping() ? 1 : 0;
    if (data->has_back_clipping) data->back_clipping_distance = object->BackPlaneDistance();
    data->has_view_volume_sides_clipping = object->HasViewVolumeSidesClipping() ? 1 : 0;
    NCollection_Sequence<TDF_Label> clippingPlanes;
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    viewTool->GetRefClippingPlaneLabel(label, clippingPlanes);
    *plane_count = clippingPlanes.Size();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_plane(
  const OcctSharp_OcafDocumentHandle* document, const char* entry,
  const int32_t index, OcctSharp_PlaneEquation* plane)
{
  if (plane == nullptr)
  { SetLastError("The saved-view plane output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *plane = {};
  return Guard([&]
  {
    NCollection_Sequence<TDF_Label> clippingPlanes;
    if (!GetViewTool(document)->GetRefClippingPlaneLabel(ResolveOcafLabel(document, entry), clippingPlanes)
        || index < 1 || index > clippingPlanes.Size())
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The saved-view clipping-plane index is outside the valid 1-based range.");
    gp_Pln value;
    opencascade::handle<TCollection_HAsciiString> nameValue;
    bool capping = false;
    if (!GetClippingPlaneTool(document)->GetClippingPlane(clippingPlanes.Value(index), value, nameValue, capping))
      throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The saved-view clipping plane could not be read.");
    value.Coefficients(plane->a, plane->b, plane->c, plane->d);
    plane->capping = capping ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_xde_saved_view_remove(
  OcctSharp_OcafDocumentHandle* document, const char* entry)
{
  return Guard([&]
  {
    ValidateOcafDocument(document); RequireOpenOcafCommand(document);
    const auto viewTool = GetViewTool(document);
    const TDF_Label label = ResolveOcafLabel(document, entry);
    if (!viewTool->IsView(label))
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The XDE label is not a saved view.");
    NCollection_Sequence<TDF_Label> clippingPlanes;
    viewTool->GetRefClippingPlaneLabel(label, clippingPlanes);
    viewTool->RemoveView(label);
    RemoveUnreferencedPlanes(document, clippingPlanes);
  });
}

// Native Visualization/Selection implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Shape.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Presentations.hxx"
#include "Visualization/Selection.hxx"
#include <AIS_SelectionScheme.hxx>
#include <Bnd_Box.hxx>
#include <NCollection_Array1.hxx>
#include <Standard_Handle.hxx>
#include <StdSelect_ShapeTypeFilter.hxx>
#include <TopAbs_ShapeEnum.hxx>
#include <TopoDS_Shape.hxx>
#include <algorithm>
#include <cmath>
#include <cstddef>
#include <gp_Pnt2d.hxx>
#include <utility>
#include <vector>

namespace OcctSharp::Native
{
AIS_SelectionScheme ToSelectionScheme(const int32_t selectionMode)
{
  switch (selectionMode)
  {
    case 0: return AIS_SelectionScheme_Replace;
    case 1: return AIS_SelectionScheme_Add;
    case 2: return AIS_SelectionScheme_Remove;
    case 3: return AIS_SelectionScheme_XOR;
    default: throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer selection mode is outside the supported range.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_move_to(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  int32_t* detected)
{
  if (detected == nullptr)
  {
    SetLastError("The viewer detection output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *detected = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->MoveTo(x, y, viewer->View, false);
    *detected = viewer->Context->HasDetected() ? 1 : 0;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_at(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  int32_t* selected_count)
{
  if (selected_count == nullptr)
  {
    SetLastError("The viewer selected-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *selected_count = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->MoveTo(x, y, viewer->View, false);
    viewer->Context->SelectDetected(AIS_SelectionScheme_Replace);
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_at_mode(
  OcctSharp_ViewerHandle* viewer,
  const int32_t x,
  const int32_t y,
  const int32_t selection_mode,
  int32_t* selected_count)
{
  if (selected_count == nullptr)
  {
    SetLastError("The viewer selected-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *selected_count = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->MoveTo(x, y, viewer->View, false);
    viewer->Context->SelectDetected(ToSelectionScheme(selection_mode));
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_selection(
  OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->ClearSelected(false);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_snapshot(
  OcctSharp_ViewerHandle* viewer,
  int64_t* presentation_ids,
  const int32_t capacity,
  int32_t* written)
{
  if (written == nullptr)
  {
    SetLastError("The viewer selection snapshot count pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *written = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    std::vector<int64_t> ids;
    for (viewer->Context->InitSelected(); viewer->Context->MoreSelected(); viewer->Context->NextSelected())
    {
      const opencascade::handle<AIS_InteractiveObject> selected = viewer->Context->SelectedInteractive();
      for (const auto& presentation : viewer->Presentations)
      {
        if (presentation.second == selected)
        {
          ids.push_back(presentation.first);
          break;
        }
      }
    }
    if (capacity < static_cast<int32_t>(ids.size()) || (!ids.empty() && presentation_ids == nullptr))
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer selection snapshot buffer is too small or null.");
    }
    for (size_t index = 0; index < ids.size(); ++index) presentation_ids[index] = ids[index];
    *written = static_cast<int32_t>(ids.size());
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_topology_snapshot(
  OcctSharp_ViewerHandle* viewer, int64_t* presentation_ids,
  OcctSharp_ShapeHandle** shapes, const int32_t capacity, int32_t* written)
{
  if (written == nullptr)
  { SetLastError("The viewer selected-topology count pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *written = 0;
  if (capacity < 0 || (capacity > 0 && (presentation_ids == nullptr || shapes == nullptr)))
  { SetLastError("The viewer selected-topology buffers or capacity are invalid."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const int32_t required = viewer->Context->NbSelected();
    if (capacity < required)
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "The viewer selected-topology buffers are too small.");
    int32_t index = 0;
    try
    {
      for (viewer->Context->InitSelected(); viewer->Context->MoreSelected(); viewer->Context->NextSelected())
      {
        const opencascade::handle<AIS_InteractiveObject> selected = viewer->Context->SelectedInteractive();
        int64_t presentation_id = 0;
        for (const auto& presentation : viewer->Presentations)
        {
          if (presentation.second == selected) { presentation_id = presentation.first; break; }
        }
        if (presentation_id == 0)
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "A selected AIS object is outside the managed presentation registry.");
        if (!viewer->Context->HasSelectedShape())
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "The selected AIS owner does not expose topology.");
        TopoDS_Shape selected_shape = viewer->Context->SelectedShape();
        if (selected_shape.IsNull())
          throw OperationFailure(OCCTSHARP_STATUS_OCCT_FAILURE, "OCCT returned a null selected topology shape.");
        presentation_ids[index] = presentation_id;
        shapes[index] = AllocateShape(std::move(selected_shape));
        ++index;
      }
      *written = index;
    }
    catch (...)
    {
      for (int32_t cleanup = 0; cleanup < index; ++cleanup) occtsharp_shape_release(shapes[cleanup]);
      *written = 0;
      throw;
    }
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selected_count(
  OcctSharp_ViewerHandle* viewer,
  int32_t* count)
{
  if (count == nullptr)
  {
    SetLastError("The viewer selected-count output pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *count = 0;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    *count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_detected_topology_snapshot(
  OcctSharp_ViewerHandle* viewer, int64_t* presentation_id, OcctSharp_ShapeHandle** shape)
{
  if (presentation_id == nullptr || shape == nullptr)
  { SetLastError("The viewer detected-topology output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *presentation_id = 0;
  *shape = nullptr;
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (!viewer->Context->HasDetected() || !viewer->Context->HasDetectedShape()) return;
    const TopoDS_Shape detected = viewer->Context->DetectedShape();
    if (detected.IsNull()) return;
    *presentation_id = FindPresentationId(viewer, viewer->Context->DetectedInteractive());
    *shape = AllocateShape(detected);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_rectangle(
  OcctSharp_ViewerHandle* viewer, const int32_t min_x, const int32_t min_y,
  const int32_t max_x, const int32_t max_y, const int32_t selection_mode,
  int32_t* selected_count)
{
  if (selected_count == nullptr)
  { SetLastError("The viewer selected-count output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *selected_count = 0;
  if (min_x == max_x || min_y == max_y)
  { SetLastError("A viewer selection rectangle must have non-zero width and height."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->SelectRectangle(
      NCollection_Vec2<int>(std::min(min_x, max_x), std::min(min_y, max_y)),
      NCollection_Vec2<int>(std::max(min_x, max_x), std::max(min_y, max_y)),
      viewer->View, ToSelectionScheme(selection_mode));
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_select_polygon(
  OcctSharp_ViewerHandle* viewer, const OcctSharp_Xy* points, const int32_t point_count,
  const int32_t selection_mode, int32_t* selected_count)
{
  if (selected_count == nullptr)
  { SetLastError("The viewer selected-count output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *selected_count = 0;
  if (points == nullptr || point_count < 3 || point_count > 4096)
  { SetLastError("A viewer selection polygon must contain between 3 and 4096 points."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  for (int32_t index = 0; index < point_count; ++index)
  {
    if (!std::isfinite(points[index].x) || !std::isfinite(points[index].y))
    { SetLastError("Viewer selection polygon coordinates must be finite."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    NCollection_Array1<gp_Pnt2d> polyline(1, point_count);
    for (int32_t index = 0; index < point_count; ++index)
      polyline.SetValue(index + 1, gp_Pnt2d(points[index].x, points[index].y));
    viewer->Context->SelectPolygon(polyline, viewer->View, ToSelectionScheme(selection_mode));
    *selected_count = viewer->Context->NbSelected();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_pixel_tolerance(
  OcctSharp_ViewerHandle* viewer, const int32_t tolerance)
{
  if (tolerance < 0 || tolerance > 100)
  { SetLastError("Viewer pixel tolerance must be from 0 through 100."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->SetPixelTolerance(tolerance);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_set_shape_filter(
  OcctSharp_ViewerHandle* viewer, const int32_t shape_kind)
{
  if (shape_kind < 0 || shape_kind > 7)
  { SetLastError("Viewer shape filters support TopAbs kinds from Compound through Vertex."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->RemoveFilters();
    viewer->ActiveFilter = new StdSelect_ShapeTypeFilter(static_cast<TopAbs_ShapeEnum>(shape_kind));
    viewer->Context->AddFilter(viewer->ActiveFilter);
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_clear_filters(OcctSharp_ViewerHandle* viewer)
{
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    viewer->Context->RemoveFilters();
    viewer->ActiveFilter.Nullify();
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_selection_bounds(
  OcctSharp_ViewerHandle* viewer, int32_t* has_bounds, OcctSharp_BoundingBox* bounds)
{
  if (has_bounds == nullptr || bounds == nullptr)
  { SetLastError("The viewer selection-bounds output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *has_bounds = 0;
  *bounds = {};
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    const Bnd_Box box = viewer->Context->BoundingBoxOfSelection(viewer->View);
    if (box.IsVoid()) return;
    box.Get(bounds->min_x, bounds->min_y, bounds->min_z,
            bounds->max_x, bounds->max_y, bounds->max_z);
    *has_bounds = 1;
  });
}

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_fit_selected(
  OcctSharp_ViewerHandle* viewer, const double margin, int32_t* fitted)
{
  if (fitted == nullptr)
  { SetLastError("The viewer fit-selected output pointer is null."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  *fitted = 0;
  if (!std::isfinite(margin) || margin < 0.0 || margin >= 1.0)
  { SetLastError("Viewer fit-selected margin must be finite and in the range 0 to less than 1."); return OCCTSHARP_STATUS_INVALID_ARGUMENT; }
  return Guard([&]
  {
    ValidateViewerThread(viewer);
    if (viewer->Context->NbSelected() == 0) return;
    viewer->Context->FitSelected(viewer->View, margin, true);
    *fitted = 1;
  });
}

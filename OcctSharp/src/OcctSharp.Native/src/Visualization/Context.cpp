// Native Visualization/Context implementation. Public contracts and ownership are unchanged.
#include "OcctSharp.Native.Internal.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Registry.hxx"
#include "Runtime/Validation.hxx"
#include "Visualization/Context.hxx"
#include <AIS_InteractiveContext.hxx>
#include <Aspect_DisplayConnection.hxx>
#include <OpenGl_GraphicDriver.hxx>
#include <V3d_Viewer.hxx>
#include <WNT_Window.hxx>
#include <algorithm>
#include <memory>
#include <thread>

namespace OcctSharp::Native
{
void ValidateViewer(const OcctSharp_ViewerHandle* viewer)
{
  if (viewer == nullptr)
  {
    throw OperationFailure(OCCTSHARP_STATUS_NULL_HANDLE, "The viewer handle is null.");
  }
  if (!IsLiveValue(viewer, LiveViewers))
  {
    throw OperationFailure(OCCTSHARP_STATUS_INVALID_HANDLE, "The viewer handle is invalid or already released.");
  }
}

void ValidateViewerThread(const OcctSharp_ViewerHandle* viewer)
{
  ValidateViewer(viewer);
  if (viewer->OwnerThread != std::this_thread::get_id())
  {
    throw OperationFailure(
      OCCTSHARP_STATUS_INVALID_ARGUMENT,
      "Viewer operations must run on the thread that created the viewer.");
  }
}
}

using namespace OcctSharp::Native;

OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_create(
  const intptr_t window_handle, OcctSharp_ViewerHandle** out_viewer)
{
  if (out_viewer == nullptr)
  {
    SetLastError("The output viewer pointer is null.");
    return OCCTSHARP_STATUS_INVALID_ARGUMENT;
  }
  *out_viewer = nullptr;
  return Guard([&]
  {
    if (window_handle == 0)
    {
      throw OperationFailure(OCCTSHARP_STATUS_INVALID_ARGUMENT, "A non-zero native window handle is required.");
    }
    std::unique_ptr<OcctSharp_ViewerHandle> viewer(new OcctSharp_ViewerHandle());
    viewer->OwnerThread = std::this_thread::get_id();
    viewer->Display = new Aspect_DisplayConnection();
    viewer->Driver = new OpenGl_GraphicDriver(viewer->Display);
    viewer->Viewer = new V3d_Viewer(viewer->Driver);
    viewer->Viewer->SetDefaultLights();
    viewer->Viewer->SetLightOn();
    viewer->Context = new AIS_InteractiveContext(viewer->Viewer);
    viewer->View = viewer->Viewer->CreateView();
    viewer->Window = new WNT_Window(reinterpret_cast<Aspect_Handle>(window_handle));
    viewer->View->SetWindow(viewer->Window);
    viewer->View->MustBeResized();
    *out_viewer = AllocateValue(viewer.release(), LiveViewers);
  });
}

void OCCTSHARP_CALL occtsharp_viewer_release(OcctSharp_ViewerHandle* viewer)
{
  if (viewer != nullptr && UnregisterValue(viewer, LiveViewers))
  {
    for (auto& manipulator : viewer->Manipulators)
      if (!manipulator.second.Value.IsNull() && manipulator.second.Value->IsAttached())
        manipulator.second.Value->Detach();
    viewer->Manipulators.clear();
    if (!viewer->Context.IsNull()) viewer->Context->RemoveFilters();
    if (!viewer->View.IsNull())
      for (const auto& plane : viewer->ClipPlanes) viewer->View->RemoveClipPlane(plane.second);
    if (!viewer->Context.IsNull()) viewer->Context->RemoveAll(false);
    viewer->ActiveFilter.Nullify();
    viewer->ClipPlanes.clear();
    viewer->Presentations.clear();
    viewer->Dimensions.clear();
    viewer->View.Nullify();
    viewer->Context.Nullify();
    viewer->Viewer.Nullify();
    viewer->Driver.Nullify();
    viewer->Window.Nullify();
    viewer->Display.Nullify();
    delete viewer;
  }
}

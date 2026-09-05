#include "OcctSharp.Native.Repair.h"
#include "Runtime/Error.hxx"
#include "Visualization/Context.hxx"
#include "Visualization/Presentations.hxx"

using namespace OcctSharp::Native;
OcctSharp_Status OCCTSHARP_CALL occtsharp_repair_viewer_select(
  OcctSharp_ViewerHandle* viewer, int64_t presentation_id) {
  return Guard([&] {
    ValidateViewer(viewer); ValidateViewerThread(viewer); auto presentation = FindPresentation(viewer, presentation_id);
    viewer->Context->ClearSelected(false); viewer->Context->SetSelected(presentation, false);
    viewer->Context->UpdateCurrentViewer();
  });
}

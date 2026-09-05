#pragma once

// Private native Visualization/Context contract; never a public ABI or a second owner.
#include "OcctSharp.Native.h"
#include <AIS_ColoredShape.hxx>
#include <AIS_InteractiveContext.hxx>
#include <AIS_Manipulator.hxx>
#include <Aspect_DisplayConnection.hxx>
#include <Graphic3d_ClipPlane.hxx>
#include <OpenGl_GraphicDriver.hxx>
#include <PrsDim_Dimension.hxx>
#include <SelectMgr_Filter.hxx>
#include <Standard_Handle.hxx>
#include <V3d_View.hxx>
#include <V3d_Viewer.hxx>
#include <WNT_Window.hxx>
#include <gp_Trsf.hxx>
#include <thread>
#include <unordered_map>

struct OcctSharp_ViewerHandle
{
  struct ManipulatorEntry
  {
    opencascade::handle<AIS_Manipulator> Value;
    int64_t PresentationId = 0;
    bool ActivationOnDetection = false;
    bool ZoomPersistence = false;
    bool HasActiveTransformation = false;
    int32_t EnabledModes = 0;
    int32_t Skin = 0;
    double Size = 0.0;
    double Gap = 0.0;
    gp_Trsf StartTransformation;
  };

  opencascade::handle<Aspect_DisplayConnection> Display;
  opencascade::handle<OpenGl_GraphicDriver> Driver;
  opencascade::handle<V3d_Viewer> Viewer;
  opencascade::handle<AIS_InteractiveContext> Context;
  opencascade::handle<V3d_View> View;
  opencascade::handle<WNT_Window> Window;
  std::unordered_map<int64_t, opencascade::handle<AIS_ColoredShape>> Presentations;
  std::unordered_map<int64_t, opencascade::handle<PrsDim_Dimension>> Dimensions;
  std::unordered_map<int64_t, opencascade::handle<Graphic3d_ClipPlane>> ClipPlanes;
  std::unordered_map<int64_t, ManipulatorEntry> Manipulators;
  opencascade::handle<SelectMgr_Filter> ActiveFilter;
  int64_t NextPresentationId = 1;
  int64_t NextDimensionId = 1;
  int64_t NextClipPlaneId = 1;
  int64_t NextManipulatorId = 1;
  std::thread::id OwnerThread;
};

namespace OcctSharp::Native
{
void ValidateViewer(const OcctSharp_ViewerHandle* viewer);

void ValidateViewerThread(const OcctSharp_ViewerHandle* viewer);
}

#include "Visualization/Context.hxx"
#include "Runtime/Error.hxx"
#include "Runtime/Validation.hxx"
#include <Graphic3d_Camera.hxx>
#include <V3d_ImageDumpOptions.hxx>
#include <Graphic3d_CView.hxx>
#include <Graphic3d_Structure.hxx>
#include <cstring>
#include <algorithm>
#include <unordered_set>

using namespace OcctSharp::Native;
namespace {
// Own the FBO: ToPixMap's normal cleanup is not exception-safe. Suppress implicit
// redraws until the captured camera and layer state have also been restored.
class CaptureBuffer final {
  opencascade::handle<V3d_View> view;
  opencascade::handle<Graphic3d_CView> driverView;
  opencascade::handle<Standard_Transient> original, buffer;
  int layer;
  bool singleLayer, immediateFront, immediateUpdate;
public:
  CaptureBuffer(const opencascade::handle<V3d_View>& v, int width, int height)
    : view(v), driverView(v->View()), original(driverView->FBO()),
      layer(driverView->ZLayerTarget()), singleLayer(driverView->ZLayerRedrawMode()) {
    buffer = driverView->FBOCreate(width, height);
    RequireRender(!buffer.IsNull(), "Cannot allocate an offscreen framebuffer; screen fallback is not supported.");
    immediateUpdate = view->SetImmediateUpdate(false);
    immediateFront = driverView->SetImmediateModeDrawToFront(false);
    driverView->SetFBO(buffer);
  }
  ~CaptureBuffer() {
    driverView->SetFBO(original);
    driverView->SetZLayerTarget(layer);
    driverView->SetZLayerRedrawMode(singleLayer);
    driverView->SetImmediateModeDrawToFront(immediateFront);
    driverView->FBORelease(buffer);
    view->SetImmediateUpdate(immediateUpdate);
    view->Invalidate();
  }
};
class CaptureCamera final {
  opencascade::handle<V3d_View> view;
  opencascade::handle<Graphic3d_Camera> original;
  bool autoDepth;
  double autoScale;
public:
  opencascade::handle<Graphic3d_Camera> Value;
  explicit CaptureCamera(const opencascade::handle<V3d_View>& v) : view(v), original(v->Camera()),
    autoDepth(v->AutoZFitMode()), autoScale(v->AutoZFitScaleFactor()), Value(new Graphic3d_Camera(original)) {
    view->SetAutoZFitMode(false); view->SetCamera(Value);
  }
  ~CaptureCamera() { view->SetCamera(original); view->SetAutoZFitMode(autoDepth, autoScale); }
};
// OCCT's multi-layer dump filters by numeric ID (>=), not drawing order.
// Temporarily mask the excluded structures and depth-clears to implement an actual
// through-layer capture, restoring both on success and exception.
class CaptureLayers final {
  OcctSharp_ViewerHandle* viewer;
  std::vector<opencascade::handle<Graphic3d_Structure>> hidden;
  std::vector<std::pair<int, Graphic3d_ZLayerSettings>> settings;
public:
  explicit CaptureLayers(OcctSharp_ViewerHandle* v) : viewer(v) {}
  void Through(int target) {
    NCollection_Sequence<int> sequence; viewer->Viewer->GetAllZLayers(sequence);
    std::unordered_set<int> excluded; bool passed = false;
    for (const int id : sequence) { if (passed) excluded.insert(id); if (id == target) passed = true; }
    RequireRender(passed, "Capture layer is not present in drawing order.");
    NCollection_Map<opencascade::handle<Graphic3d_Structure>> structures;
    viewer->View->View()->DisplayedStructures(structures);
    for (const auto& s : structures) if (s->IsVisible() && excluded.contains(s->GetZLayer())) hidden.push_back(s);
    for (int id : excluded) settings.emplace_back(id,viewer->Viewer->ZLayerSettings(id));
    for (const auto& s : hidden) s->SetVisible(false);
    for (const auto& [id, original] : settings) { auto value = original; value.SetClearDepth(false); viewer->Viewer->SetZLayerSettings(id,value); }
  }
  ~CaptureLayers() {
    for (const auto& [id, value] : settings) viewer->Viewer->SetZLayerSettings(id,value);
    for (const auto& s : hidden) s->SetVisible(true);
  }
};
}
OcctSharp_Status OCCTSHARP_CALL occtsharp_viewer_frame_capture(OcctSharp_ViewerHandle* viewer,
  const OcctSharp_FrameRequest* request, uint8_t* output, int32_t capacity, OcctSharp_FrameInfo* info) {
  return Guard([&] {
    ValidateViewerThread(viewer); RequireRender(request && info, "Null capture request/metadata."); const auto& r = *request;
    RenderFlag(r.depth); RenderFlag(r.adjust_aspect); RenderFlag(r.single_layer);
    RequireRender(r.width > 0 && r.height > 0 && r.width <= 16384 && r.height <= 16384 && static_cast<int64_t>(r.width) * r.height <= 16777216, "Capture exceeds its 16-megapixel budget.");
    const int length = r.width * r.height * 4; ValidateOutputCapacity(capacity, length, output, "Insufficient frame byte capacity.");
    const auto caps = ReadRenderCaps(viewer); const int maxTile = std::min(caps.max_dump_x, caps.max_dump_y);
    RequireRender(r.tile_size == 0 || (r.tile_size >= 16 && r.tile_size <= maxTile), "Unsupported capture tile size.");
    RequireRender(r.layer >= -1 && (r.layer != -1 || !r.single_layer), "Single-layer capture requires an explicit layer.");
    V3d_ImageDumpOptions options; options.Width = r.width; options.Height = r.height;
    options.BufferType = r.depth ? Graphic3d_BT_Depth : Graphic3d_BT_RGBA;
    options.TileSize = r.tile_size; options.ToAdjustAspect = r.adjust_aspect != 0;
    options.TargetZLayerId = r.layer == -1 ? Graphic3d_ZLayerId_BotOSD : ReviewLayerId(viewer, r.layer);
    options.IsSingleLayer = r.single_layer != 0;
    // A useful default depth describes model geometry, not the cleared OSD buffer.
    if (r.depth && r.layer == -1) { options.TargetZLayerId = Graphic3d_ZLayerId_Default; options.IsSingleLayer = true; }
    const int tile = r.tile_size == 0 ? maxTile : r.tile_size;
    CaptureBuffer buffer(viewer->View, std::min(r.width, tile), std::min(r.height, tile));
    CaptureLayers scope(viewer);
    if (r.layer != -1 && !r.single_layer) { scope.Through(options.TargetZLayerId); options.TargetZLayerId = Graphic3d_ZLayerId_BotOSD; }
    CaptureCamera camera(viewer->View);
    RequireRender(camera.Value->ProjectionType() == Graphic3d_Camera::Projection_Orthographic || camera.Value->ProjectionType() == Graphic3d_Camera::Projection_Perspective, "Capture supports monographic orthographic/perspective cameras only.");
    if (r.adjust_aspect) camera.Value->SetAspect(static_cast<double>(r.width) / r.height);
    OcctSharp_FrameInfo metadata{}; metadata.width = r.width; metadata.height = r.height; metadata.stride = r.width * 4;
    metadata.zero_to_one_depth = camera.Value->IsZeroToOneDepth() ? 1 : 0;
    metadata.near_plane = camera.Value->ZNear(); metadata.far_plane = camera.Value->ZFar();
    const auto inverse = (camera.Value->ProjectionMatrix() * camera.Value->OrientationMatrix()).Inverted();
    for (int row = 0; row < 4; ++row) for (int col = 0; col < 4; ++col) metadata.inverse_view_projection[row * 4 + col] = inverse.GetValue(row, col);
    Image_PixMap pixels; RequireRender(viewer->View->ToPixMap(pixels, options), "OCCT could not capture the requested framebuffer.");
    RequireRender(pixels.SizeX() == static_cast<size_t>(r.width) && pixels.SizeY() == static_cast<size_t>(r.height) &&
      pixels.Format() == (r.depth ? Image_Format_GrayF : Image_Format_RGBA), "Unexpected framebuffer dimensions or pixel format.");
    std::vector<uint8_t> copy(static_cast<size_t>(length));
    for (int row = 0; row < r.height; ++row) std::memcpy(copy.data() + static_cast<size_t>(row) * metadata.stride, pixels.Row(row), metadata.stride);
    std::memcpy(output, copy.data(), copy.size()); *info = metadata;
  });
}

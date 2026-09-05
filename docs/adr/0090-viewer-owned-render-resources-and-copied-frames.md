# ADR-0090: Viewer-owned render resources and copied frames

- Status: Accepted and locally validated for the complete forty-row Batch W; final delivery evidence in STATUS
- Date: 2026-09-05

## Decision and entry evidence

Keep all forty rows of the [W matrix](../BATCH_W_VIEWER_LIGHTING_FRAME_CAPTURE_GAP_INVENTORY.md).
Entry is committed V `2c76e10`, Preview.21, inventory `91357F17`. Frozen Preview.15
remains unchanged. The exact delta has 422 classification changes, fourteen in W
roots, no added/removed/identity-changed declarations. Two 54-root / 2,582-candidate
audits match `8563C417404B1ECA154442A4879282AD4C79CB0BBD1349ACDC140749B9C67495`.
The independent post-commit inventory matches the committed V inventory exactly.

Lighting, render profiles/layers, appearance, texture/environment input and capture
have independent Visualization translation units. Context owns their resources on
its existing creating thread; there is no second registry, managed project or DLL.
The existing facade viewer owns the friendly review controller and resource IDs.
Pure copied color/depth frame values belong in Visualization and never reference WPF.
Only the existing WPF sample creates WriteableBitmap snapshots; HwndHost stays live.

All input pixels are bounded owned copies. Local image loading accepts explicitly
provided local files, never URLs or implicit asset paths. Packed cubemaps retain their
backing image. Appearance profiles replace cloned shading aspects, including XDE
custom drawers, without mutating document material/geometry. Reset restores the saved
effective shading state. Texture replacement prepares every binding before swapping.
Cross-parent, stale, removed and wrong-thread resource use must reject.

Lights are staged as a whole rig with explicit per-view activation. Failed validation
or staging preserves the previous rig. Portable snapshots contain definitions, not
native IDs. Review replay resolves application asset keys in the same explicit scope;
no private paths or raw IDs are serialized by default.

Render profiles query actual driver capabilities and reject unsupported requests;
no silent MSAA/OIT/PBR fallback is accepted. Exposure/white point/tone mapping are
path-tracing settings, not claimed raster effects. OCCT 8.0.1 ClearPBREnvironment
calls SetImageBasedLighting(true); disabling uses SetImageBasedLighting(false)
explicitly. Visible environment background and IBL activation are separate settings.

Capture uses a valid existing HWND/context, bounded sized/tiled/layer ToPixMap calls,
and restores live view state. Pixel data carries row/channel/origin/alpha semantics.
Depth is a normalized framebuffer sample, not world distance; reconstruction uses
copied capture matrices rather than the current camera. Background and nonfinite
samples reject. Camera-dependent fixtures cover perspective and orthographic views.
Default depth capture selects the default model layer: OCCT clears depth in upper
overlay layers even when empty, so the final composited buffer is not model depth.
Explicit layer scopes remain available. Through-layer capture uses actual drawing
order and restores masked structure visibility/depth-clear settings; the SDK's numeric
ID threshold is not presented as drawing order. Copied cameras include projection,
scale/FOV, aspect, clipping planes and the automatic-depth policy.

## Alternatives, consequences and validation

Persistent GPU/driver pointers, arbitrary shaders, network image retrieval, D3DImage,
headless-service rendering and a new Native DLL are outside this batch. Per-family
commits do not satisfy the forty-row closure. Existing generated bindings are reused
where safe; exact newly called manual overloads alone are recorded in SC-060.

Require all forty named assertions, supported driver success paths, explicit negative
capabilities, lifetime/atomicity tests, real-HWND captures with numerical invariants,
WPF copied snapshots, full Release/Debug and actual Debug-native tests, strict private
headers/export parity, clean regeneration, exact accounting, both consumers, local
pack/release checks and one whole-batch local commit. No NuGet publication or push.

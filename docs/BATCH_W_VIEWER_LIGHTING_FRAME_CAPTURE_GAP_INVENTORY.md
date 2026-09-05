# Batch W: Viewer lighting, presentation materials and copied frame capture

- Status: scope prepared, implementation **0/40**. New API compile/runtime: **NOT RUN**.
- Decision: [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md).
- Preparation commit: `eacd0ed`; product baseline remains Preview.15 / OCCT 8.0.1.
- Planned local package slot: `8.0.1-preview.22`; current versions are unchanged.
- Frozen roots: [batch-w-viewer-lighting-frame-capture.json](../OcctSharp/config/batches/batch-w-viewer-lighting-frame-capture.json).
- Shared preparation: [U-W evidence](BATCH_U_W_PREPARATION.md).
- Delivery: [continuous Q-W runbook](BATCH_CONTINUOUS_EXECUTION.md); one complete batch per local commit.

## Product outcome and reuse boundary

D already supplies camera restore, clipping, backgrounds, subshape styles and RGB/RGBA/depth screenshot files; H supplies XDE PBR material data, scenes, LODs and exports. W adds managed viewer-owned light/material/texture programs, capability-aware rendering and copied in-memory frame data. Saving an existing screenshot or editing existing XDE material records is not a new W capability.

Load exact/discrete assembly -> configure bounded light rig/material/UV texture/environment -> render-profile validation -> capture RGBA/depth/tiled/layer frames -> copied WPF snapshot review -> clear/recreate resources on the creating thread.

All 40 rows are one delivery unit. A row is a newly observable workflow, not a getter,
test, overload or standalone family checkpoint. Acceptance below is future required
evidence, not a claim of implementation. Existing lower generated wrappers are reused
where ownership permits; candidate root membership alone does not mean a missing API.

## Frozen capability matrix

| ID | Root group | New capability | Required acceptance |
|---|---|---|---|
| W-01 | Render | Copied rendering capability report | Query current driver limits for lights, textures, samples and viewport/frame buffers without exposing the driver handle. |
| W-02 | Render | Capability-aware rendering profile | Validate a bounded copied quality profile and report effective settings versus unsupported requests; no silent quality downgrade. |
| W-03 | Light | Ambient lighting authoring | Create/update/remove viewer-owned ambient light entries with linear color and intensity semantics. |
| W-04 | Light | Directional lighting authoring | Create directional lights with validated normalized directions and copied state independent of input DTOs. |
| W-05 | Light | Positional lighting authoring | Create point lights with location/attenuation/range validation and clearly documented world-unit semantics. |
| W-06 | Light | Spot lighting authoring | Create spot lights with supported cone/exponent/direction controls and reject invalid angular ranges. |
| W-07 | Light | Atomic light-rig replacement | Replace a complete bounded light set or preserve the prior rig if allocation/validation fails. |
| W-08 | Light | Headlight versus world-space light behavior | Expose supported camera-relative lighting and verify its behavior during camera rotation versus fixed world lights. |
| W-09 | Light | Per-view light activation policy | Enable selected registered lights for the existing view while retaining inactive owned lights; validate effective light limits. |
| W-10 | Light | Portable light-rig snapshot and restore | Serialize copied light definitions and restore with fresh viewer IDs; never serialize native pointers or claim cross-view ID identity. |
| W-11 | Appearance | Presentation shading-model selection | Apply supported unlit/Phong/PBR shading choices at the appropriate viewer/presentation boundary and report unsupported modes. |
| W-12 | Appearance | Review-only physical material overrides | Apply copied appearance parameters to presentations without mutating the XDE document or its existing material records. |
| W-13 | Appearance | Independent front/back material appearance | Expose supported two-sided material behavior and verify oriented faces receive the intended side settings. |
| W-14 | Appearance | Back-face and two-sided rendering policy | Control face-culling/backface display for review with explicit distinction from changing shape orientation. |
| W-15 | Appearance | Material transparency and alpha interpretation | Apply supported blend/mask settings with an explicit transparency policy; preserve underlying source material values. |
| W-16 | Appearance | Atomic appearance-profile reset | Replace or clear a whole presentation appearance profile and restore effective document/default style without dangling resources. |
| W-17 | Appearance | Texture creation from copied pixels | Create viewer-owned Texture2D from bounded copied pixel buffers with explicit format/stride/origin and no borrowed PixMap memory. |
| W-18 | Appearance | Texture creation from explicit local image | Decode a user-supplied local image with dimension/path checks; no implicit network retrieval or side-loaded content. |
| W-19 | Appearance | Texture binding to eligible geometry | Bind a texture set to supported surface/mesh UVs; report absent UVs instead of inventing an arbitrary map. |
| W-20 | Appearance | Planar texture coordinate generation | Expose supported Texture2Dplane origin/scale mapping for eligible surfaces without modifying model UV geometry. |
| W-21 | Appearance | Texture-coordinate appearance transforms | Configure repeat/origin/scale/rotation on the visual mapping and verify model geometry remains unchanged. |
| W-22 | Appearance | Texture sampling policy | Apply supported filtering, wrapping and anisotropy settings constrained by driver capability; report effective values. |
| W-23 | Appearance | Atomic texture-content replacement | Replace a texture image and invalidate the correct presentation resources while preserving bindings on failure. |
| W-24 | Appearance | Texture lifetime and binding management | Reuse a viewer-owned texture across presentations, detach safely and reject cross-viewer/disposed IDs; no OpenGL object escapes. |
| W-25 | Appearance | Six-face cubemap environment input | Create a cube environment from six validated equally sized faces and explicit ordering/orientation. |
| W-26 | Appearance | Packed cubemap environment input | Accept supported six-tile image layouts with dimension checks and owned backing pixel memory. |
| W-27 | Light | Image-based lighting environment control | Enable/replace/remove eligible PBR environment lighting with capability evidence and bounded bake quality settings. |
| W-28 | Light | Background versus lighting environment separation | Control visible cubemap background independently from image-based illumination and verify restored defaults. |
| W-29 | Render | Antialiasing and render-resolution policy | Expose supported MSAA/resolution scale controls with effective-limit checks and unchanged camera/input coordinates. |
| W-30 | Render | Transparency rendering method policy | Choose supported blended/OIT modes and parameters; test capability rejection where the requested mode is unavailable. |
| W-31 | Render | Exposure and tone-mapping controls | Expose supported exposure/white-point/tone-mapping settings with defined linear-versus-display color semantics. |
| W-32 | Render | Review Z-layer profile | Place presentations in managed viewer-owned layers with depth/order policy, preserving existing picking and parent lifetime. |
| W-33 | Render | Copied RGBA frame capture | Return a fresh independent pixel array with width/height/stride/channel-order/origin/alpha semantics, not only D file dumps. |
| W-34 | Render | Copied float depth capture | Return depth samples with viewport/camera/range metadata and invalid/background values; distinguish depth-buffer values from world distance. |
| W-35 | Render | Depth-to-world reconstruction | Convert valid captured depth pixels using that capture camera/projection convention, with clipping/background rejection and numerical fixtures. |
| W-36 | Render | Sized offscreen-buffer capture | Use ToPixMap for a requested capture size/aspect policy, restoring view state; an existing valid HWND/OpenGL context remains required. |
| W-37 | Render | Tiled high-resolution capture | Use bounded ImageDumpOptions tiling with output-size/overflow limits and compare seam behavior on a deterministic scene. |
| W-38 | Render | Layer-scoped frame capture | Capture supported single/through-layer buffers with explicit included layer IDs, preserving live scene visibility afterwards. |
| W-39 | Integration | Self-contained review appearance recipe | Snapshot camera/light/material/texture reference settings for replay in the same asset scope; no embedded private paths or native IDs by default. |
| W-40 | Integration | WPF copied-frame review example | Add an opt-in WriteableBitmap snapshot/thumbnail path using copied buffers and live HwndHost review; do not promise D3DImage or continuous airspace-free rendering. |

## Root, dependency and source closure

| Root group | Exact decision roots |
|---|---|
| Light | `Graphic3d_CLight`, `V3d_AmbientLight`, `V3d_DirectionalLight`, `V3d_PositionalLight`, `V3d_SpotLight`, `Graphic3d_LightSet` |
| Appearance | `Graphic3d_MaterialAspect`, `Graphic3d_PBRMaterial`, `Graphic3d_BSDF`, `Graphic3d_Texture2D`, `Graphic3d_Texture2Dplane`, `Graphic3d_TextureParams`, `Graphic3d_TextureSet`, `Graphic3d_CubeMapSeparate`, `Graphic3d_CubeMapPacked`, `Graphic3d_CubeMap`, `AIS_TexturedShape`, `Prs3d_Drawer`, `Graphic3d_Aspects` |
| Render | `Graphic3d_RenderingParams`, `Graphic3d_Camera`, `Graphic3d_ZLayerSettings`, `V3d_Viewer`, `Image_PixMap`, `Image_AlienPixMap`, `Graphic3d_GraphicDriver` |

The 26 decision roots reuse 28 support roots:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `BRepTools_History`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `Quantity_Color`, `Quantity_ColorRGBA`, `XCAFDoc_VisMaterial`, `XCAFDoc_VisMaterialTool`, `XCAFPrs_Style`, `Aspect_GraphicsLibrary`, `Graphic3d_BufferType`, `V3d_ImageDumpOptions`, `Graphic3d_TypeOfLimit`, `Image_Format`.

Integration rows reuse the established or explicitly prepared public workflows rather
than requiring a second copy of their native bindings. Existing D/H viewer/material capabilities and R attributed mesh data are prerequisites. T/U/V outputs are review inputs, not a reason to make Visualization depend on parametric execution or new Modeling implementation details.
Delivery order alone is not an algorithm dependency. Every prerequisite must actually
pass its whole-batch gates before W starts.

Native: Visualization/Lighting.cpp, Visualization/Appearance.cpp and Visualization/FrameCapture.cpp, reusing Context.hxx and existing viewer/thread/registry ownership. Managed viewer contracts remain in Visualization or existing facade; XDE style adapters stay in higher owners. Pure copied frame DTOs do not depend on WPF; only the sample adapter uses WriteableBitmap.

Private headers stay acyclic and domain-owned. Native builders, iterators and temporary
arrays are local to a call. Recipes and diagnostic/index/history records are copied.
Owning topology reuses the current registry/release family and survives its inputs.
Document labels remain parent-bound; viewer objects remain parent-bound/thread-affine.
W light/texture IDs and U/V preview/result owners require concrete ownership and cleanup
evidence during implementation, not new independent registries. Do not expose native
session/GPU pointers. No new ownership category or binary split is created by preparation.

## OCCT limitations and non-goals

W retains the Windows x64 real-HWND/OpenGL context. ToPixMap offscreen buffers do not establish headless service rendering, a D3DImage bridge or native-to-WPF live compositing. Texture/cubemap pixels are copied/owned, and source PixMap lifetime is never borrowed. Render capabilities vary by driver; required capability rejection is testable, but a supported-success path still needs appropriate hardware evidence. No arbitrary shaders, GPU handle exposure, ray-tracing guarantee, network texture loading, or new native DLL. Screenshot bytes are not promised bit-identical across GPUs.

## Preparation evidence and baseline freshness

Inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 54 | 2582 | 1128 | 850 | 76 | 528 |

Root report SHA256:
`185C479CD79C5F8AE09269C72958CD693435655EFE7EE7E751483EBB18DAD3AC`.
Reports are regenerated by the existing exact-root auditor; the shared verifier checks
repeat determinism, baseline/input protection, SDK headers and representative toolkit
exports. Final executed results are in STATUS and the shared U-W preparation record.

Candidates include reused emitted/manual and non-callable or blocked declarations;
they are not 40 capabilities, expected new public methods or a complete bindable
denominator. Do not relabel all candidates Manual. SDK header/export availability
is not a new compile/link, lifetime or driver-success test.

The root scope is prepared on Preview.15, not on hypothetical completed Q-T/U/V code.
Before implementation, record the previous/new inventory commit/hash and exact
added/removed/reclassified IDs, callable signature and source/dependency changes.
Reaudit impacted rows and reuse newly completed prerequisites. Keep the 40-row product
denominator; a changed or unsupported outcome needs an explicit decision, not silent
deletion or replacement with an extra test.

## Whole-batch acceptance

The [shared implementation gates](BATCH_Q_T_PREPARATION.md) apply without reduction:
40-row assertion mapping; Release/Debug build and Generator/Runtime regression, including
actual Debug-native runtime; source/ownership/dependency closure; precise manual-ID and
ABI reconciliation; applicable real STEP/IGES/XDE/HWND workflows; clean regeneration,
compatibility, committed runtime notices/manifest, both clean consumers and complete
local pack/release checks; documentation and one local completion commit.

Use deterministic geometric/structural assertions and negative/foreign/disposed/stale
identity tests. For W, supported rendering paths need actual driver evidence; capability
rejection alone cannot stand in for all success-path tests. GPU-dependent pixel tests
use documented tolerance/invariants, not universal byte-identical screenshots.

After a verified completion commit, the authorized continuous run automatically
revalidates the next queued batch and continues without a new routine confirmation.
A compile/test failure stays in this batch for repair; it is not permission to skip
a gate. No NuGet publication, GitHub push, signing or unattended scheduler is created.

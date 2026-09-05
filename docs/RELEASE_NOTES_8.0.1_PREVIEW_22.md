# 8.0.1-preview.22: Batch W viewer review and copied capture

This local-only preview implements the original forty W capabilities. Validation and
commit status are in [STATUS](STATUS.md); no NuGet publication or GitHub push is part
of this run. Package 8.0.1-preview.22, ABI 1.66 and bridge 0.74.0 retain OCCT 8.0.1,
schema 1.13, assembly/file 0.1.0.0, twelve modules, the facade and one Native DLL.

## Review workflow

`viewer.Rendering` queries driver limits and applies bounded quality profiles; owns
ambient/directional/point/spot light rigs, textures, environments and review layers;
and applies review-only Phong/PBR/unlit material overrides. Original XDE colors and
materials remain unchanged and appearance reset restores effective custom drawers.

Texture input copies RGBA/BGRA with explicit stride and origin. Local image decoding
accepts an explicit local drive path, not URLs/UNC. Geometry UVs are required unless
planar visual mapping is explicitly requested. Filtering, wrapping, anisotropy and
mapping transforms do not modify model geometry. Replacement keeps bindings;
environment objects retain their creation-time pixel images after texture disposal.

Cubemaps use OCCT's six sides (+X, -X, +Y, -Y, +Z, -Z), top-down input images, and
an explicit side-to-tile permutation for equal-square packed layouts. Images must be
prepared for the SDK's cubemap orientation; there is no implicit face rotation.
Visible environment background and IBL are separate. IBL requires an ambient light
and a PBR view profile. Unlit with distinguished front/back colors rejects; use Phong
or PBR. Exposure/white point/filmic settings are path-tracing-only. Unsupported quality
requests fail rather than silently falling back. GPU pixel equality is not portable.

## Frames and recipes

`CaptureColor` owns fresh top-down RGBA8 pixels; RGB is display encoded and alpha is
composite coverage, not a straight-alpha asset. `CopyOpaqueBgra` provides a safe opaque
thumbnail. `CaptureDepth` owns float samples and inverse view/projection matrices.
Default depth selects the model layer, excluding OCCT's depth-clearing overlays.
`TryReconstruct` uses pixel centers and rejects background/nonfinite/clipped samples.
It remains valid after camera edits or viewer disposal. Explicit layer scopes report
copied context-local IDs; tiled/sized capture restores live camera and layer state.

`ViewerReviewRecipe` serializes copied camera/profile/light/material state and explicit
application asset keys, never implicit file paths or native IDs. Supply matching
asset scope and dictionaries on replay. All references resolve before mutation, but
the entire replay is not transactional. Individual updates reject invalid input before
replacement; callers requiring atomic scene publication should replay into a fresh view.

The WPF sample adds a frozen WriteableBitmap thumbnail beside the live HwndHost. It is
an opt-in copied snapshot, not D3DImage, continuous compositing or headless rendering.
See [sample README](../OcctSharp/samples/OcctSharpViewer.Wpf/README.md) and
[forty-row acceptance map](BATCH_W_VIEWER_LIGHTING_FRAME_CAPTURE_GAP_INVENTORY.md).

## Licensing

OcctSharp code is MIT. Bundled OCCT remains LGPL-2.1 with its additional exception;
other DLLs retain their own licenses. See [third-party notices](THIRD_PARTY_NOTICES.md) and the
runtime-local notices; the MIT package metadata does not relicense third-party code.

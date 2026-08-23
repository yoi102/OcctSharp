# ADR-0046: HWND-bound thread-affine viewer and presentation IDs

- Status: Accepted
- Date: 2026-08-23
- Scope: B17 visualization core on OCCT 8.0.1, Windows x64

## Decision

`OcctViewer` owns one native visualization graph created for a caller-provided Windows
HWND: `Aspect_DisplayConnection`, `OpenGl_GraphicDriver`, `V3d_Viewer`,
`AIS_InteractiveContext`, `V3d_View`, and `WNT_Window`. Creation and every subsequent
viewer operation must run on the creating managed thread. The application owns the HWND
and explicitly forwards resize and mouse coordinates; the bridge installs no callback
into managed code.

Displayed shapes become native-owned `AIS_Shape` presentations. Managed
`ViewerPresentation` objects are parent-bound values containing only a monotonic 64-bit
ID and a strong reference to their viewer. They never expose or release an AIS pointer.
Hide, show, and remove resolve the ID inside the parent registry. Removing a presentation
invalidates it deterministically; disposing the viewer invalidates all presentations.
Selection crosses the ABI only as a caller-owned copied array of presentation IDs.

The initial profile deliberately excludes camera manipulation, display modes, styles,
lights, clip planes, off-screen buffers, native-to-managed callbacks, and cross-thread
dispatch. These remain later classified declarations, not implicit B17 support.

## Validation

Release and Debug pass 32 generator and 68 runtime tests. The runtime test creates a
real off-screen HWND, establishes an OpenGL view, displays a box after disposing the
source `Shape`, exercises hide/show/resize/fit/redraw, rejects cross-thread redraw,
detects and selects the center presentation, snapshots copied IDs, and rejects use after
removal. The interactive sample creates a `CS_OWNDC` Win32 window and forwards size,
paint, mouse-move, and click messages. Freshness verifies 12 generated files. The
alpha.38 clean consumer repeats the HWND/display/selection path through ABI 1.30 and
bridge 0.38.0 with 45 DLLs under `occt`.

## Upgrade impact

Re-check WNT native-window construction, OpenGL driver options and dependencies,
default viewer/context/view initialization, AIS selection iteration, device-context
ownership, coordinate conventions, and render-thread requirements on every OCCT,
compiler, Windows SDK, or graphics-driver upgrade.

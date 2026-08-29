# ADR-0064: Implement production CAD viewport and model review as Batch D

- Status: Accepted
- Date: 2026-08-29
- Scope: Batch D product denominator, cross-family viewer closure, and ownership boundaries

## Context

Batch C closed the routine CAD workflow at alpha.54. The managed SDK can import or build
models, inspect and edit topology, preserve common XDE metadata, export, display shapes,
drive ordinary input, and copy selected topology. The next high-value gap is no longer
basic display: a production review viewport still needs stable model identity, exact
detection, area selection, per-subshape review styling, camera and coordinate state,
section review, and captured visual evidence.

These capabilities span XDE/STEP, TopoDS/BRep, AIS/SelectMgr, V3d, Graphic3d, and image
output. Implementing them as isolated methods would repeatedly reopen the same
thread-affine viewer graph and would leave applications with unusable partial workflows.
The complete inventory also shows a mixed root closure: a focused audit of 52 candidate
OCCT overloads found 19 already emitted, 31 blocked behind copied-value/container/
borrowed-result boundaries, and two deliberately skipped. Compilation of generated raw
types alone therefore cannot define the product API.

## Decision

Open Batch D as one product-scale implementation wave named **production CAD viewport
and model review**. Its finite denominator is the 24 capabilities recorded in
`BATCH_D_VIEWPORT_GAP_INVENTORY.md`. The workflow begins with STEP/XDE occurrences and
ends with a screenshot produced from a real HWND in both repository runtime and a clean
package consumer.

Batch D is one wave, not a family of `D01`, `D.1`, per-class, or per-method batches.
Focused checks may be used while implementing, but no partial capability group is a
Batch D checkpoint. The whole 24-capability denominator and its complete local gate are
accepted together.

The viewer boundary is extended as follows:

- `OcctViewer` remains the sole owning, creation-thread-affine owner of the display
  connection, driver, viewer, interactive context, view, window, filters, colored AIS
  presentations, and clip planes. The application still owns the HWND.
- `ViewerPresentation` and future filter/clip-plane references remain parent-bound IDs.
  No `AIS_InteractiveObject`, `SelectMgr_Filter`, `V3d_View`, `Graphic3d_ClipPlane`, or
  OCCT container handle becomes an independently owned public pointer.
- An XDE occurrence presentation stores copied source identity: occurrence path and
  stable label entries. It does not retain an `XdeOccurrence`, `XdeLabel`, XCAF tool, or
  document-native handle. Selection and detection return that copied identity with the
  parent-bound presentation.
- Detected and selected whole/subshape topology is copied into a new registered owning
  `Shape`. The borrowed result of `AIS_InteractiveContext::DetectedShape()` never crosses
  the C ABI.
- Rectangle and polygon points cross as validated caller-owned values. Native
  `NCollection_Vec2`/`NCollection_Array1<gp_Pnt2d>` objects are call-local.
- Per-subshape color, transparency, and width are presentation mutations backed by
  `AIS_ColoredShape`. The presentation owns the native colored AIS object; supplied
  subshapes are borrowed only during a call and are matched against the presentation's
  copied source topology.
- Camera state, screen/world positions, projection vectors, pick rays, bounds, colors,
  and plane equations cross only as fixed copied values. Invalid or degenerate camera,
  ray, rectangle, polygon, and plane inputs fail before native mutation.
- Clip-plane native handles and their sequence stay inside the viewer. A public clip
  plane reference is parent-bound and becomes invalid when removed or when its viewer is
  disposed.
- Screenshot output uses `V3d_View::Dump(path, Graphic3d_BufferType)` or an equivalent
  native-local path. No `Image_PixMap`, pixel pointer, image container, or borrowed image
  storage crosses the ABI. Managed path semantics must include an explicit Windows
  non-ASCII strategy rather than inheriting an untested narrow-path assumption.

Existing generated declarations are reused where their semantics and registry ownership
fit this parent-bound graph. Direct OCCT declarations used by the friendly native bridge
must be reconciled by one Batch D special case with exact stable IDs; already emitted or
previously manual declarations are not counted again. Generated output retains the
ADR-0061/ADR-0062 module/layer partition and must pass the semantic dependency-closure
gate after any selection change.

This decision does not split assemblies, native DLLs, or packages. Batch D keeps one
`OcctSharp.dll`, one `OcctSharp.Native.dll`, one package, existing public namespaces,
and creator-owned registries in the single native bridge.

## Locked non-goals

- IVtk/VTK, OpenGL ES, Draw/test, C++/CLI, or another optional dependency profile.
- Native-to-managed callbacks, application event-loop ownership, or custom input hooks.
- Custom shaders, rendering pipelines, GPU buffers, ray tracing, or renderer extension
  points.
- Arbitrary user-defined selection callbacks. Batch D supports only built-in,
  configuration-driven filters with no reverse callback.
- Exhaustive mesh attributes, low-frequency schema entities, animation, measurement
  annotation authoring, or a general scene graph.
- Physical managed-project, native-DLL, or NuGet package splitting.

## Alternatives considered

- Adding screenshot support alone was rejected because it captures a viewport that still
  cannot reliably identify, select, isolate, style, or section reviewed geometry.
- Exposing raw generated AIS/V3d/Graphic3d wrappers directly was rejected because their
  independent registries do not own the existing friendly viewer graph and would permit
  parent/thread lifetime violations.
- Returning borrowed detected topology was rejected because OCCT detection state changes
  on the next move and cannot outlive the interactive context safely.
- Returning `Image_PixMap` for screenshots was rejected because the requested product
  outcome is durable file evidence, not a new borrowed pixel/container lifetime family.
- Treating standard clip planes or computed hidden-line display as a custom rendering
  pipeline was rejected; both are stable OCCT viewer features contained by the existing
  V3d/Graphic3d owner graph.
- Splitting the work into selection, camera, clipping, and screenshot batches was rejected
  because the end-to-end review workflow requires all of them and shares one validation
  environment.

## Consequences

- Batch D has a stable 24-capability denominator before implementation starts.
- The public surface remains workflow-oriented while raw/generated declarations remain
  auditable implementation dependencies.
- Parent-bound viewer ownership expands, but no new standalone native pointer category
  or reverse callback is introduced.
- The implementation must reconcile the mixed emitted/blocked root closure and cannot
  declare success from generated method counts.
- Batch C evidence remains immutable. Batch D progress begins at 0/24 and cannot be used
  to revise alpha.54 validation claims.

## Validation required

- Focused tests for input validation, parent/viewer mismatch, removal/disposal,
  cross-thread rejection, source-document disposal, source-shape disposal, detection
  invalidation, selection schemes, filter reset, override reset, camera degeneracy,
  coordinate round trips, clip-plane update/enable, and screenshot failures.
- A real HWND runtime workflow that imports one real STEP/XDE assembly, displays located
  occurrences with copied identity, performs point and area selection, obtains owning
  detected/selected topology, applies and clears subshape styles, isolates/fits the
  selection, snapshots/restores the camera, converts coordinates, reviews a clip plane
  and hidden-line state, and writes a non-empty screenshot.
- The same end-to-end workflow from a clean package consumer with the application-local
  62-DLL closure, including output-path behavior.
- Release and Debug native/managed builds, generator and runtime tests, dependency
  profiles, generated freshness, byte-identical clean regeneration, dependency closure,
  API compatibility, full inventory, runtime manifest, SBOM/provenance/checksums, and
  local release gates.
- No validation above is currently claimed by this preparation ADR. Implementation,
  compile, runtime, package, and release checks are `NOT RUN` for Batch D.

## Related decisions

- ADR-0045: parent-bound XDE labels and copied occurrence metadata.
- ADR-0046: HWND/thread-affine viewer and parent-bound presentation IDs.
- ADR-0052: native-local common operations and stable-ID reconciliation.
- ADR-0059: committed Windows runtime and MIT licensing.
- ADR-0060: product-scale common API batches and large-wave cadence.
- ADR-0061: generated domain/layer partitioning.
- ADR-0062: generated cross-shard dependency closure and deferred physical split.
- ADR-0063: copied selected topology and parent-bound input.

## Implementation outcome

Alpha.55 completes the locked 24/24 denominator without splitting it into family
checkpoints. ABI 1.46 and bridge 0.54.0 implement the viewer-owned colored presentation,
filter, clip-plane, camera, coordinate, review-aid, and screenshot graph. SC-040
reconciles exactly 18 newly direct blocked stable IDs.

Release and Debug pass Generator 91/91, Runtime 115/115, and dependency profiles 6/6.
The repository runtime and clean 62-DLL package consumer both execute the real STEP/XDE
plus real-HWND review-to-screenshot workflow, including source-lifetime independence,
cross-thread/cross-viewer rejection, and Unicode screenshot output. Generated freshness,
byte-identical clean regeneration, dependency closure, API compatibility, complete
inventory classification, runtime hashes, SBOM/provenance/checksums, and the full local
release check pass. Hosted release execution, signing, and NuGet publication remain
separate and `NOT RUN`.

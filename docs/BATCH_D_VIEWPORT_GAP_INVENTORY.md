# Batch D production viewport and model-review gap inventory

This document locks the product denominator and records the completed cross-family
dependency closure for Batch D. It measures an end-to-end review workflow rather than
isolated OCCT declarations.

Preparation status: **COMPLETE**. Implementation status: **24/24 capabilities (100%);
COMPLETE**. Alpha.55 validates the API, ABI, repository runtime, real HWND, clean package,
inventory, and complete local release gate as one wave.

## Product outcome

A Windows x64 .NET application should be able to load an XDE assembly into an application-owned HWND,
retain occurrence identity through presentation and topology picks, make precise point/
rectangle/polygon selections, apply review-only subshape styling, isolate and fit the
result, inspect or restore the camera, convert between screen and model coordinates,
section the model, switch review display aids, and save screenshot evidence without
dipping into unmanaged OCCT.

```text
STEP/XDE import and occurrence traversal
  -> copied occurrence/presentation identity
  -> point detection plus owning topology snapshot
  -> rectangle/polygon selection and built-in filters
  -> per-subshape color/transparency/width review overrides
  -> selection bounds, fit, and isolate
  -> camera snapshot/restore and screen/world/pick-ray conversion
  -> window zoom, background, clip plane, hidden-line mode, and trihedron
  -> durable screenshot evidence
```

## Locked 24-capability denominator

| # | Family | Stable capability | Alpha.54 baseline | Batch D exit evidence |
|---:|---|---|---|---|
| 1 | XDE/AIS | Display a located XDE occurrence with copied occurrence path and stable label-entry identity | Shape display has only a presentation ID | Runtime identity survives occurrence/document disposal and is returned by detection/selection |
| 2 | AIS/TopoDS | Snapshot the exactly detected whole/subshape topology as an independent owning `Shape` | `MoveTo` returns only Boolean detection | Point detection returns presentation, copied source identity, and owning topology |
| 3 | AIS/SelectMgr | Rectangle selection with replace/add/remove/toggle schemes | Point selection only | Pixel rectangle selects expected presentations/subshapes for every scheme |
| 4 | AIS/SelectMgr | Polygon selection with replace/add/remove/toggle schemes | Point selection only | Validated polygon selects expected presentations/subshapes for every scheme |
| 5 | AIS/SelectMgr | Configure pixel tolerance with a bounded managed contract | No friendly option | Runtime boundary and detection behavior are tested |
| 6 | SelectMgr/TopoDS | Apply and clear built-in topology-kind filters without callbacks | Per-presentation active selection kind only | Filter lifetime, replacement/reset, and cross-presentation behavior are tested |
| 7 | AIS/Bnd/V3d | Copy selection bounds and fit selected geometry | `FitAll` only | Empty/non-empty bounds and selection fit are tested |
| 8 | AIS | Isolate selected presentations and restore the prior visibility set | Manual hide/show only | Reversible isolate works across removal and cleared selection |
| 9 | AIS/TopoDS/Quantity | Set a color override for one owned subshape | Whole-presentation color only | Face/edge override is visible and source-disposal independent |
| 10 | AIS/TopoDS | Set a transparency override for one owned subshape | Whole-presentation transparency only | Range, wrong-member, and reset behavior are tested |
| 11 | AIS/TopoDS | Set a width override for one owned subshape | No friendly width API | Range, wrong-member, and reset behavior are tested |
| 12 | AIS/TopoDS | Clear one or all subshape review overrides | No subshape override state | Individual and complete reset restore presentation defaults |
| 13 | V3d | Snapshot eye, target, up, and projection direction as copied camera state | Projection setters only | Snapshot contains finite, non-degenerate copied values |
| 14 | V3d | Restore one validated camera state atomically from the managed view | No camera-state restore | Round trip and invalid/degenerate failure paths are tested |
| 15 | V3d | Convert a client pixel to a world point on the current view plane | Missing | Known camera/viewport point is validated |
| 16 | V3d | Convert a world point to client pixels | Missing | World-to-screen-to-world tolerance is asserted |
| 17 | V3d | Produce a world-space pick ray from a client pixel | Missing | Origin/direction are finite, normalized, and camera-consistent |
| 18 | V3d | Zoom to a validated client rectangle | Zoom factor and pan only | Normalized/reversed/degenerate rectangle behavior is tested |
| 19 | V3d/Quantity | Set a copied linear-RGB background color | Missing friendly API | Range and real-HWND redraw behavior are tested |
| 20 | Graphic3d/V3d/gp | Create, update, enable/disable, and remove a parent-bound clip plane | No friendly clipping API | Plane validation, lifetime, removal, and viewer disposal are tested |
| 21 | V3d | Enable and disable computed hidden-line review | Wireframe/shaded presentation mode only | Computed-mode transition is tested on a real model/window |
| 22 | V3d/Quantity | Show, configure, and hide the orientation trihedron | Missing | Position/color/scale validation and redraw are tested |
| 23 | V3d/Image | Save a selected viewer buffer to a durable file path | Missing | Non-empty image, invalid path/buffer, overwrite, and Windows path behavior are tested |
| 24 | STEP/XDE through Image | Complete real-file review workflow in repository runtime and clean package consumer | Families exist only as separate partial paths | Both workflows pass with a real HWND, 62 DLLs, owning picks, styling, camera, clipping, and screenshot evidence |

The denominator is immutable for the Batch D implementation wave. An overload, enum,
option, error path, or convenience method needed to make one row complete belongs to that
row and cannot be deferred as a later micro-batch. New unrelated requests require a new
product decision; removing a difficult row merely to increase completion is forbidden.

## Root-declaration audit

The complete alpha.54 inventory was queried for the AIS, SelectMgr, V3d, Graphic3d, gp,
TopoDS, Quantity, and image roots needed by the workflow. The focused 52-overload set is:

| Inventory state | Count | Meaning for Batch D |
|---|---:|---|
| `Emitted` | 19 | Reuse when the generated wrapper can participate safely in the viewer-owned graph; emitted status alone is not friendly/runtime evidence |
| `Blocked` | 31 | Requires copied output, validated container/value input, native-local operation, or explicit parent-bound ownership |
| `Skipped` | 2 | Destructor/record or other declaration intentionally not exposed; it is not promoted merely for Batch D |
| **Total** | **52** | Audit roots only; the 24 product capabilities remain the completion denominator |

Decision-driving blocked roots include:

```text
AIS_ColoredShape::SetCustomColor
c:@S@AIS_ColoredShape@F@SetCustomColor#&1$@S@TopoDS_Shape#&1$@S@Quantity_Color#

AIS_ColoredShape::SetCustomTransparency
c:@S@AIS_ColoredShape@F@SetCustomTransparency#&1$@S@TopoDS_Shape#d#

AIS_ColoredShape::SetCustomWidth
c:@S@AIS_ColoredShape@F@SetCustomWidth#&1$@S@TopoDS_Shape#d#

AIS_ColoredShape::UnsetCustomAspects
c:@S@AIS_ColoredShape@F@UnsetCustomAspects#&1$@S@TopoDS_Shape#b#

AIS_InteractiveContext::DetectedShape
c:@S@AIS_InteractiveContext@F@DetectedShape#1

AIS_InteractiveContext::SelectRectangle
c:@S@AIS_InteractiveContext@F@SelectRectangle#&1$@S@NCollection_Vec2>#I#S0_#&1$@N@opencascade@S@handle>#$@S@V3d_View#$@E@AIS_SelectionScheme#

AIS_InteractiveContext::SelectPolygon
c:@S@AIS_InteractiveContext@F@SelectPolygon#&1$@S@NCollection_Array1>#$@S@gp_Pnt2d#&1$@N@opencascade@S@handle>#$@S@V3d_View#$@E@AIS_SelectionScheme#

AIS_InteractiveContext::BoundingBoxOfSelection
c:@S@AIS_InteractiveContext@F@BoundingBoxOfSelection#1

Graphic3d_ClipPlane::Graphic3d_ClipPlane(gp_Pln)
c:@S@Graphic3d_ClipPlane@F@Graphic3d_ClipPlane#&1$@S@gp_Pln#

Graphic3d_ClipPlane::SetEquation(gp_Pln)
c:@S@Graphic3d_ClipPlane@F@SetEquation#&1$@S@gp_Pln#

V3d_View::Convert(pixel -> world)
c:@S@V3d_View@F@Convert#I#I#&d#S0_#S0_#1

V3d_View::Convert(world -> pixel)
c:@S@V3d_View@F@Convert#d#d#d#&I#S0_#1

V3d_View::ConvertWithProj
c:@S@V3d_View@F@ConvertWithProj#I#I#&d#S0_#S0_#S0_#S0_#S0_#1

V3d_View::At / Eye / Proj / Up
c:@S@V3d_View@F@At#&d#S0_#S0_#1
c:@S@V3d_View@F@Eye#&d#S0_#S0_#1
c:@S@V3d_View@F@Proj#&d#S0_#S0_#1
c:@S@V3d_View@F@Up#&d#S0_#S0_#1

V3d_View::Dump
c:@S@V3d_View@F@Dump#*1C#&1$@E@Graphic3d_BufferType#
```

Important reusable emitted roots include `AIS_InteractiveContext::MoveTo`,
`FitSelected`, `SetPixelTolerance`, `AddFilter`, and `RemoveFilters`;
`StdSelect_ShapeTypeFilter(TopAbs_ShapeEnum)`; `V3d_View::WindowFit`, `SetAt`,
`SetEye`, `SetUp`, `SetBackgroundColor(Quantity_TypeOfColor,double,double,double)`,
`SetComputedMode`, `SetClipPlanes`, and `ZFitAll`; the default
`Graphic3d_ClipPlane` constructor plus `SetOn`/`IsOn`; and clip-plane sequence creation
and append/remove operations.

These states are evidence for implementation planning, not permission to expose raw
handles. Any direct blocked declaration actually used by the native bridge must be
recorded exactly in `SPECIAL_CASES.md`; unused audited candidates remain blocked.

## Cross-family dependency closure

### Identity and presentation

`XdeOccurrence.Path`, occurrence entry, and referred entry are copied before display.
The located shape is displayed through a colored AIS presentation owned by the viewer.
Detection/selection resolves the AIS object back to its parent-bound presentation and
copied source identity. Neither the source XDE document nor the temporary occurrence
object is retained.

### Detection, selection, and filters

Point detection, rectangle selection, polygon selection, selection schemes, pixel
tolerance, and built-in shape filters share one interactive context. Selection region
containers are created inside the native call. Filter handles remain in the viewer and
are cleared before destruction. Detected/selected subshapes are copied before another
viewer operation can invalidate OCCT's borrowed detection state.

### Review presentation

Presentations use `AIS_ColoredShape` so whole-object defaults and per-subshape overrides
share one source topology. Wrong-viewer, removed-presentation, non-member subshape,
disposed-shape, and invalid range inputs must fail without partial mutation. Isolate is
managed as a reversible viewer visibility snapshot; removal while isolated cannot
resurrect a stale presentation.

### Camera and coordinates

Camera getters return one immutable copied state. Restore validates the entire state
before ordered native setters and redraw. Pixel/world conversions and pick rays are
call-local value operations tied to the current view. Window zoom normalizes rectangle
orientation but rejects zero-area input.

### Clipping and visual evidence

Each clip plane is a parent-bound ID backed by a viewer-owned `Graphic3d_ClipPlane`.
The viewer owns the clip-plane sequence applied to the view. Hidden-line computation,
background, and trihedron state mutate the same owner-thread view. Screenshot writes a
durable file and returns no native image object.

### End-to-end closure

The final integration must use a real STEP/XDE assembly and a real HWND. It must prove
source-document/source-shape independence, identity preservation, exact owning picks,
area selection, filters, reversible isolate, custom subshape styling and reset, camera
round trips, coordinate conversion, clipping, hidden-line/trihedron state, and screenshot
output. The same workflow must execute from a clean package with all native assets loaded
from application-local `occt/`.

## Validation and completion gates

Batch D reaches 24/24 only when all of the following are current and pass together:

- exact stable-ID reconciliation for every direct manual declaration, with zero overlap
  against generated ownership;
- Release and Debug native/managed builds;
- focused generator/model tests for any generalized rule;
- focused runtime, ownership, failure, disposal, and thread-affinity tests for all 24
  capabilities;
- real STEP/XDE plus real-HWND end-to-end runtime evidence;
- clean package consumer of the same end-to-end workflow and 62-DLL closure;
- generated dependency closure with zero unresolved targets, graph violations, or cycles;
- generated freshness and byte-identical clean regeneration;
- additive API compatibility, complete inventory accounting, runtime hashes,
  SBOM/provenance/checksums, and complete local release check;
- updated status, migration, architecture, ownership, testing, special-case, release-note,
  and version/package documents that match the actual evidence.

The completed alpha.55 implementation ran the full gate as one wave:

| Check | Result |
|---|---|
| API/ABI implementation | PASS — package alpha.55, ABI 1.46, bridge 0.54.0 |
| Native/managed compile after Batch D changes | PASS — Release and Debug, 0 errors |
| Batch D runtime/lifetime tests | PASS — Runtime 115/115 in Release and Debug |
| Real-HWND/real-file integration | PASS — real STEP/XDE review workflow through Unicode screenshot output |
| Clean package consumer | PASS — 62-DLL application-local runtime executes the complete workflow |
| Full local release check | PASS — generation, clean regeneration, compatibility, inventory, SBOM/provenance/checksums, and Git whitespace gates |

`BatchDCompletionTests` covers the 24-capability owner/thread/lifetime and failure matrix.
The package consumer repeats the real STEP/XDE-to-HWND-to-screenshot workflow without a
developer OCCT installation. SC-040 reconciles exactly 18 newly direct blocked stable
IDs; the final classification is 16,353 emitted, 120 manual, 49,344 skipped, 50,455
blocked, and zero supported-unselected declarations.

## Explicit non-goals

IVtk/VTK, OpenGL ES, Draw/test, native callbacks, arbitrary managed selection callbacks,
custom shaders/rendering pipelines, GPU buffer exposure, exhaustive mesh attributes,
cold schema, and physical managed/native/package splitting are outside Batch D.

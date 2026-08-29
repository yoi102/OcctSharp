# Common CAD API gap inventory

This is the stable capability denominator and completion record for Batch C implementation
waves. It measures usable workflows rather than isolated OCCT declarations.

Completion status: **Batch C is complete at the alpha.54 local implementation
checkpoint**. Alpha.51, alpha.52, alpha.53, and the final alpha.54 closure are coherent
cross-family waves inside one Batch C product outcome, not independent numbered batches.

This file remains the immutable Batch C denominator. The next product batch is defined
separately in [the Batch D production viewport gap inventory](BATCH_D_VIEWPORT_GAP_INVENTORY.md);
its preparation and future progress must not rewrite Batch C results.

## First locked cross-family dependency closure

The wave is one dependency-closed product slice spanning `TopExp`/`BRep_Tool`/
`BRepCheck`, `BRepMesh`/`Poly`, `BRepTools`, OCAF/XDE, and `AIS`/`V3d`:

```text
create or read native BREP
  -> inspect unique and occurrence topology, closedness, validity, tolerances
  -> modify and transform the shape
  -> extract an orientation-correct detailed mesh with UVs and face mapping
  -> attach part name, color, layers, and material in one XDE operation
  -> STEPCAF write/read round trip
  -> display with appearance and standard camera controls
  -> replace/add/remove/toggle/clear interactive selection
```

The workflow is available from the friendly managed API, exercised by Release/Debug
runtime and clean package-consumer evidence, and shipped with matching clone-and-run
native assets.

## Capability matrix

| Family | Stable capability denominator | Before wave | Checkpoint result |
| --- | --- | --- | --- |
| Model/inspect | Unique and occurrence counts for all eight topology kinds | Missing | PASS: `Shape.GetTopologySummary()` |
| Model/inspect | Whole-shape closedness and validity | Partial: validity only | PASS: copied in the topology summary |
| Model/inspect | Vertex, edge, and face tolerance ranges | Missing | PASS: copied minimum/maximum ranges |
| Mesh/Poly | Positions and triangle indices | Available | PASS: existing `MeshSnapshot` preserved |
| Mesh/Poly | OCCT node normals, UV presence/values, face grouping, orientation | Missing | PASS: `DetailedMeshSnapshot` |
| Exchange | STEP, IGES, STL, OBJ, PLY, glTF, VRML | Available | PASS: existing providers preserved |
| Exchange | Native OCCT BREP read/write | Missing | PASS: `ReadBrep`/`WriteBrep` |
| XDE | Shape, name, color, layers, and material primitives | Available but multi-call | PASS: transaction-bound `AddPart` convenience |
| Viewer | Display, visibility, fit, redraw, resize, detect, replace-select | Partial | PASS: existing operations preserved |
| Viewer | Presentation color, transparency, display mode | Missing | PASS: parent-bound presentation controls |
| Viewer | Standard projection, zoom, pan | Missing | PASS: owner-thread-affine view controls |
| Viewer | Add/remove/toggle/clear selection | Missing | PASS: explicit selection schemes and clear |
| Integration | One common solid workflow crossing all non-window families | Missing | PASS: Release/Debug runtime and clean package consumer |
| Integration | Viewer appearance/camera/selection on a real HWND | Partial | PASS: Release/Debug real-HWND runtime path |

## Second locked cross-family dependency closure

The second wave is one import-diagnose-repair-interact slice spanning `STEPControl`,
`XSControl`, `BRepCheck`, `ShapeFix`, and `V3d`:

```text
read STEP geometry with read/transfer/unit report
  -> snapshot per-subshape validation status with geometry/exact options
  -> repair to an independently owned shape
  -> compare immutable validation reports before and after repair
  -> display and drive mouse rotation through the thread-affine viewer
```

| Family | Stable capability denominator | Before wave | Checkpoint result |
| --- | --- | --- | --- |
| STEPControl/XSControl | Typed read status, candidate/transferred roots, shape count, and system length unit | Geometry result only | PASS: `ReadStepWithReport` |
| BRepCheck | Per-unique-subshape copied issue kinds/statuses | Whole-shape Boolean only | PASS: `GetValidationReport` |
| BRepCheck | Geometry-check and exact-check options | Missing | PASS: explicit managed options |
| ShapeFix | Repaired owning shape with before/after validation comparison | Owning shape only | PASS: `RepairWithReport` |
| V3d | Mouse rotation start/continue input | Missing | PASS: `StartRotation`/`Rotate` |
| Integration | STEP import through diagnosis and repair | Missing | PASS: Release/Debug runtime workflow |
| Integration | Same import/repair and viewer rotation paths from a clean package | Missing | PASS: alpha.52 clean package consumer |

The denominator is 7/7 capabilities. Six direct declarations are reconciled by SC-037;
already generated V3d operations and the existing ShapeFix owning result are reused
without inflating the manual count.

## Third locked cross-family dependency closure

The third wave is one measure-annotate-compose-exchange slice spanning `BRepGProp`,
`XCAFDoc_Area`/`Volume`/`Centroid`, XDE assemblies, `TopLoc_Location`, and
`STEPCAFControl`:

```text
compute area, volume, and centroid from an owning shape
  -> store, read, replace, and clear optional XCAF validation attributes
  -> recursively flatten nested assembly occurrences
  -> compose root-to-leaf world locations and create independent located shapes
  -> write STEPCAF with explicit representation and metadata switches
  -> read STEPCAF with explicit metadata switches and verify filtered round trips
```

| Family | Stable capability denominator | Before wave | Checkpoint result |
| --- | --- | --- | --- |
| BRepGProp | Compute surface area, closed volume, and centroid from a label shape | Low-level property owner only | PASS: `UpdateValidationPropertiesFromShape` |
| XCAFDoc | Read/write optional area, volume, and centroid values | Missing | PASS: `ValidationProperties` |
| XCAFDoc | Preserve absent fields and clear attributes transactionally | Missing | PASS: nullable copied snapshot and setter |
| XDE assembly | Enumerate direct and recursively flattened occurrences | Direct components only | PASS: `GetOccurrences` |
| XDE/TopLoc | Compose world locations and return independent located shapes | Local occurrence location only | PASS: `XdeOccurrence` |
| STEPCAF read | Select name, color, layer, validation-property, and material import | All modes fixed on | PASS: `XdeStepReadOptions` |
| STEPCAF write | Select model type and common metadata output modes | As-is/all modes fixed on | PASS: `XdeStepWriteOptions` |
| Integration | Nested assembly property/options workflow from runtime and clean package | Missing | PASS: alpha.53 runtime and package consumer |

The denominator is 8/8 capabilities. Nine direct XCAF attribute declarations are
reconciled by SC-038; existing `BRepGProp`, location, assembly, and STEPCAF dependencies
are reused without counting them again.

## Final locked Batch C dependency closure

The final wave is one selective-import-edit-inspect-interact product slice spanning
`STEPControl`/`XSControl`, `BRepAdaptor`/`BRep_Tool`/`Geom2d`, `BRepBuilderAPI`/
`BRepTools_ReShape`/`TopExp`, and `AIS`/`V3d`:

```text
open one STEP reader session and inspect file units/transfer roots
  -> choose roots and an explicit target system length unit
  -> evaluate 3D curve/surface derivatives and edge-on-face 2D pcurves
  -> trim bounded edges/faces without leaking borrowed geometry
  -> construct wires and edit a copied topology graph by replace/remove
  -> navigate both directions of copied topology relationships
  -> display and activate whole-shape or subshape selection modes
  -> receive independently owned selected topology snapshots
  -> forward application mouse, wheel, and semantic keyboard input
  -> validate, export, and re-read the edited result
```

| Family | Stable capability denominator | Before wave | Checkpoint result |
| --- | --- | --- | --- |
| BRepAdaptor/Geom | Copied point, first derivative, and second derivative at an edge parameter | Unit tangent only | PASS: `EvaluateEdgeDerivatives` |
| BRepAdaptor/Geom | Copied U/V derivatives and oriented normal at bounded face parameters | Point/normal only | PASS: `EvaluateFaceDerivatives` |
| BRep_Tool/Geom2d | Edge-on-face pcurve bounds plus copied UV point/tangent evaluation | Missing | PASS: `GetPcurveSnapshot`/`EvaluatePcurve` |
| Geom/BRepBuilderAPI | Independently owned edge trimmed to a finite parameter interval | Missing | PASS: `TrimEdge` |
| Geom/BRepBuilderAPI | Independently owned rectangular face trimmed to finite UV bounds | Missing | PASS: `TrimFace` |
| BRepBuilderAPI | Owning wire construction from copied/borrowed edge inputs | Polygon-only | PASS: `ShapeFactory.CreateWire` |
| BRepTools_ReShape | Replace or remove a contained subshape and return an independent result | Missing | PASS: `ReplaceSubshape`/`RemoveSubshape` |
| TopExp | Navigate copied item-to-ancestor and ancestor-to-item relationships | Forward indices only | PASS: `TopologyAdjacencyMap.GetAncestorIndices`/`GetItemIndices` |
| STEPControl/XSControl | Owning reader session with read status, root count, effective unit, and file-unit names | One-shot summary | PASS: `StepReadSession.Open`/`Info` |
| STEPControl/XSControl | Selective one-root and multi-root transfer with independent shape owners | Transfer all roots only | PASS: `TransferRoot`/`TransferRoots` |
| STEPControl | Explicit finite target system length unit before transfer | Read-only system unit | PASS: `targetSystemLengthUnit` |
| AIS | Activate whole-object or one common topology selection kind per presentation | Whole-object only | PASS: `ViewerPresentation.SetSelectionKind` |
| AIS/TopoDS | Selected presentation plus independently owned selected subshape snapshots | Presentation IDs only | PASS: `OcctViewer.GetSelectedItems` |
| V3d/AIS | Parent-bound mouse press/move/release, wheel, and semantic keyboard forwarding | Rotation primitives only | PASS: `ViewerInputController` |
| Integration | Selective STEP import through geometry/topology edit, export, and interactive subshape selection | Missing | PASS: runtime and clean alpha.54 package workflow |

All 15 capabilities belong to one final Batch C checkpoint and all 15 pass. Focused
failure/disposal/ownership tests and the full Release/Debug, deterministic regeneration,
clean-package, real-file, real-HWND, inventory, provenance, and release-gate chain pass.
The denominator remains one product checkpoint rather than numbered C batches or
per-class completion claims.

## Checkpoint evidence

- Release and Debug native/managed builds: PASS, 0 errors.
- Generator tests: PASS, 91/91 in Release and Debug.
- Runtime/lifetime/integration tests: PASS, 114/114 in Release and Debug.
- Dependency profiles: PASS, 6/6 in Release and Debug.
- Generated freshness and clean source regeneration: PASS, 83/83 files current and
  byte-identical.
- Generated dependency closure: PASS, 27 direct edges, 0 unresolved references, 0
  target-graph violations, and 0 cycles.
- Clean NuGet consumer: PASS with alpha.54, ABI 1.45, bridge 0.53.0, OCCT 8.0.1, all
  62 application-local DLLs, and the final selective-import/edit/export/viewer workflow.
- Full classification: PASS, 116,272/116,272 declarations and 7,090/7,090 headers have
  final dispositions; 16,353 Emitted, 102 Manual, 0 SupportedUnselected, 49,344 Skipped,
  50,473 Blocked, 0 pending, and 0 HD099. Inventory SHA256 is
  `B885C13B4037AF79065143B204F715F85C533AB186337466D4DE1B0B25048770`.
- API compatibility: PASS, 37,018 additions and 0 removals against alpha.38, with no
  breaking change.
- Local release gate: PASS (`batchImplementationComplete: true`). The committed native
  bridge is 14,920,192 bytes with SHA256
  `57593BC8B66870DE0373BFBDEFF47B1731C20DF6066EFF22764254EB416E54AA`.
  Hosted CI execution, signing, and NuGet publication remain `NOT RUN`, so
  `publicReleaseReady` is false without keeping Batch C active.

## Batch C exit audit

Batch C is complete for its locked common-CAD workflow denominator. Advanced selection
filters, custom rendering pipelines, low-frequency schema entities, optional integrations,
and exhaustive mesh attributes are explicitly outside Batch C. They may become later
product work only after a new finite denominator and ownership/dependency closure are
accepted; they do not keep C active and are not silently counted as unfinished C work.

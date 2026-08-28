# Common CAD API gap inventory

This is the stable capability denominator and completion record for Batch C implementation
waves. It measures usable workflows rather than isolated OCCT declarations.

Checkpoint status: **alpha.51 first wave, alpha.52 second wave, and alpha.53 third wave
complete at their local checkpoints**. These are large waves inside Batch C, not a
claim that all Batch C common workflows are complete.

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

## Checkpoint evidence

- Release and Debug native/managed builds: PASS, 0 warnings and 0 errors.
- Generator tests: PASS, 62/62 in Release and Debug.
- Runtime/lifetime/integration tests: PASS, 108/108 in Release and Debug.
- Generated freshness and clean source regeneration: PASS, 13/13 files current and
  byte-identical.
- Clean NuGet consumer: PASS with alpha.53, ABI 1.44, bridge 0.52.0, OCCT 8.0.1, all
  62 application-local DLLs, and the nested XDE validation-property/occurrence/STEP-
  options workflow.
- Full classification: PASS, 116,272/116,272 declarations and 7,090/7,090 headers have
  final dispositions; 16,353 Emitted, 85 Manual, 0 SupportedUnselected, 49,344 Skipped,
  50,490 Blocked, 0 pending, and 0 HD099. Inventory SHA256 is
  `37A31B92034E1132AA46293BB42A50F4A9D88E3AEC10D98046E9A598F0F8676F`.
- API compatibility: PASS, 36,883 additions and 0 removals against alpha.38, with no
  breaking change.
- Local release gate: PASS (`batchImplementationComplete: true`). Hosted full release,
  signing, and NuGet publication remain `NOT RUN` and do not block this local checkpoint.

## Explicit non-goals for this wave

This wave does not claim all of Batch C complete. It does not add low-frequency schema
entities, custom rendering pipelines, advanced selection filters, or every OCCT mesh
attribute. Those remain in the Batch C denominator and are prioritized after this
coherent checkpoint.

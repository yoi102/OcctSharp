# Common CAD API gap inventory

This is the stable capability denominator and completion record for the first Batch C
implementation wave. It measures usable workflows rather than isolated OCCT declarations.

Checkpoint status: **COMPLETE locally for alpha.51**. This is one completed large wave
inside Batch C, not a claim that all Batch C common workflows are complete.

## Locked cross-family dependency closure

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

## Checkpoint evidence

- Release and Debug native/managed builds: PASS, 0 warnings and 0 errors.
- Generator tests: PASS, 62/62 in Release and Debug.
- Runtime/lifetime/integration tests: PASS, 107/107 in Release and Debug.
- Generated freshness and clean source regeneration: PASS, 13/13 files current and
  byte-identical.
- Clean NuGet consumer: PASS with alpha.51, ABI 1.42, bridge 0.50.0, OCCT 8.0.1, and
  all 62 application-local DLLs.
- Full classification: PASS, 116,272/116,272 declarations and 7,090/7,090 headers have
  final dispositions; 16,353 Emitted, 70 Manual, 0 SupportedUnselected, 49,344 Skipped,
  50,505 Blocked, 0 pending, and 0 HD099.
- API compatibility: PASS, 36,729 additions and 0 removals against alpha.38.
- Local release gate: PASS (`batchImplementationComplete: true`). Hosted full release,
  signing, and NuGet publication remain `NOT RUN` and do not block this local checkpoint.

## Explicit non-goals for this wave

This wave does not claim all of Batch C complete. It does not add low-frequency schema
entities, custom rendering pipelines, advanced selection filters, or every OCCT mesh
attribute. Those remain in the Batch C denominator and are prioritized after this
coherent checkpoint.

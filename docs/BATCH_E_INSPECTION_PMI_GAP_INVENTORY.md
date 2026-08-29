# Batch E engineering inspection, measurement, and PMI gap inventory

This document records the locked product denominator, cross-family dependency closure,
implementation, and completion evidence for Batch E. It measures one engineering-
inspection workflow rather than isolated OCCT classes or method counts.

Preparation status: **COMPLETE**. Implementation status: **24/24 capabilities (100%);
COMPLETE**. Preview.2, ABI 1.47, and bridge 0.55.0 pass the complete local gate.

## Product outcome

A Windows x64 .NET application should be able to open or create a STEP AP242/XDE model,
measure selected topology precisely, inspect and edit semantic dimensions, geometric
tolerances, datums, and saved model views, display review dimensions in the existing
HWND viewer, export the document, reopen it, and capture screenshot evidence without an
undocumented native escape hatch.

```text
STEP AP242/XDE import or generated inspection document
  -> stable occurrence and owning selected topology
  -> exact distance/support, length/area/volume, angle, radius, and units
  -> copied semantic dimensions, geometric tolerances, datums, and reference graph
  -> transaction-bound PMI creation/update/removal
  -> saved model view and parent-bound viewer measurement presentations
  -> STEP AP242 export/reimport
  -> durable inspection screenshot evidence
```

## Locked 24-capability denominator

| # | Family | Stable capability | Preview.1 baseline | Batch E exit evidence |
|---:|---|---|---|---|
| 1 | Viewer/XDE/TopoDS | Resolve a detected or selected review item to copied occurrence identity plus an independent owning measurement shape | Batch D returns identity and topology separately | Measurement target remains valid after source occurrence/document and selection-state disposal |
| 2 | BRepExtrema/TopoDS | Compute exact minimum distance and the corresponding point pair | `Shape.DistanceTo` copies one point pair | Finite value and points match analytic fixtures and wrong/disposed input fails |
| 3 | BRepExtrema/TopoDS | Copy every equivalent minimum-distance solution with owning support shapes and support kinds | Only solution count is exposed | Vertex/edge/face supports, edge parameters, and face UV values are copied and validated |
| 4 | BRepAlgo/BRepGProp | Classify separated, touching, contained, and volumetrically interfering shape pairs | Boolean Common and one distance value are separate calls | Classification and optional owning overlap shape agree for analytic solids |
| 5 | BRepAdaptor/GProp | Measure complete edge and wire length | Edge length exists; wire total is missing | Edge/wire results, invalid kinds, locations, and source independence are tested |
| 6 | BRepGProp/TopoDS | Measure face/shell area with centroid | General property accumulator exists | Located face/shell values and invalid-kind behavior are copied and tested |
| 7 | BRepGProp/TopoDS | Measure solid/compound volume, mass, centroid, and inertia tensor as one snapshot | `GPropProperties` is available but not inspection-composed | One immutable result is validated against analytic solids and assembly locations |
| 8 | BRepAdaptor/gp | Measure angle between linear edges and between planar faces | Curve/surface snapshots exist; no friendly pair-angle operation | Acute/obtuse/parallel/invalid cases return deterministic copied values |
| 9 | BRepAdaptor/gp | Measure radius/diameter for circular edges and cylindrical or conical faces | Individual adaptor snapshots expose some geometry | Kind-checked circle/cylinder/cone results and wrong-kind failures are tested |
| 10 | STEP/XDE/Units | Carry model length/angle units and explicit display units with every inspection value | STEP length unit is available only in reader workflows | Import units, conversion, precision, and formatting metadata round-trip without implicit global state |
| 11 | XCAFDoc/XCAFDimTolObjects | Enumerate semantic dimensions with stable document-parent-bound identities | Generated dimension value objects exist; document table traversal is missing | Empty/multiple labels, stable entries, order, and parent disposal are tested |
| 12 | XCAFDimTolObjects | Copy complete dimension semantics: type, value/range, plus-minus tolerance, qualifiers, ISO class, modifiers, and decimal places | Scalar generated members are fragmented | One immutable snapshot preserves all supported dimension semantics |
| 13 | XCAFDimTolObjects/TopoDS/gp | Copy dimension references, path, direction, annotation plane, points, text position, names, descriptions, and owning presentation topology | Borrowed/container/value results are incomplete | Optional fields and owning presentation survive source object disposal |
| 14 | XCAFDoc/XCAFDimTolObjects | Enumerate semantic geometric tolerances with stable identities | Generated tolerance value objects exist; document traversal is missing | Stable label enumeration and referenced-shape resolution are tested |
| 15 | XCAFDimTolObjects/gp | Copy tolerance type/value, material requirement, zone/modifiers, axis, affected plane, annotation placement, semantic name, and owning presentation | Scalar members exist but not a complete snapshot | One immutable snapshot covers optional fields, validation, and disposal independence |
| 16 | XCAFDoc/XCAFDimTolObjects | Enumerate datums and datum targets with stable identities | Generated datum value objects exist; document traversal is missing | Datum labels, target topology, ordering, and document-parent guards are tested |
| 17 | XCAFDimTolObjects/TopoDS/gp | Copy datum name, modifiers, target type/axis/size/number, position, annotation placement, and owning presentation | Fragmented generated accessors | One immutable snapshot preserves target and presentation semantics |
| 18 | XCAFDoc/TDF | Resolve the complete shape-dimension-tolerance-datum reference graph | Required label sequences are blocked/container-based | First/second shape sets, datum frames, tolerance links, and reverse lookups are copied without exposing TDF containers |
| 19 | XCAFDoc/OCAF | Transactionally create, update, attach, detach, and remove semantic dimensions | No friendly mutation workflow | Commit/abort, cross-document, invalid reference, and persistence behavior are tested |
| 20 | XCAFDoc/OCAF | Transactionally create, update, attach, detach, and remove geometric tolerances and datum frames | No friendly mutation workflow | Datum order/linkage, rollback, removal invalidation, and persistence are tested |
| 21 | STEPCAF/XCAF | Control GDT and saved-view import/export and write an AP242 model type explicitly | Current STEPCAF options cover common metadata but not the complete PMI outcome | Disabled/enabled modes and exported/reimported semantic counts are asserted |
| 22 | XCAFDoc_ViewTool/V3d/Graphic3d | Enumerate, copy, and apply saved model views containing shape visibility, PMI references, camera, and clipping planes | Viewer camera/clipping exists; XDE view graph is not exposed | Saved view survives document reload and applies only through copied values/parent IDs |
| 23 | PrsDim/AIS/Viewer | Create, style, show/hide, select, update, and remove parent-bound length, angle, radius, and diameter review dimensions | Generated PrsDim handles do not belong to the friendly viewer owner graph | Viewer-owned IDs, wrong-viewer/thread rejection, source independence, and redraw are tested |
| 24 | STEP AP242 through Image | Complete the generated-real-file inspection/PMI workflow in repository runtime and a clean package consumer | Batch D ends at general review screenshot | Create/import, measure, edit PMI, save/apply view, display dimensions, export/reimport, and screenshot pass with 62 DLLs |

The denominator is immutable for the Batch E implementation wave. Required overloads,
enums, validation, status, disposal, formatting, and convenience composition belong to
their row and cannot be deferred as numbered, dotted, per-class, or family sub-batches.

## Root-declaration audit

The alpha.55 final inventory was queried for the 14 OCCT root types that drive the
workflow: `BRepExtrema_DistShapeShape`, `BRepAdaptor_Curve`, `BRepAdaptor_Surface`,
`XCAFDoc_DimTolTool`, the three `XCAFDimTolObjects` value-object roots,
`XCAFDoc_ViewTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, and the four
`PrsDim` length/angle/radius/diameter roots.

| Inventory state | Count | Meaning for Batch E |
|---|---:|---|
| `Emitted` | 142 | Reuse where generated registry ownership and copied values fit |
| `Manual` | 2 | Existing adaptor special cases remain owned by their current records |
| `Blocked` | 604 | Requires copied results, container snapshots, document/viewer parent ownership, or native-local operations |
| `Skipped` | 242 | Destructors, operators, protected helpers, or non-callable declarations remain excluded |
| **Total** | **990** | Candidate dependency declarations only; product completion remains 24 capabilities |

Decision-driving blocked roots include `BRepExtrema_DistShapeShape::PointOnShape1/2`,
`SupportOnShape1/2`, `SupportTypeShape1/2`, edge/face parameter outputs, and
`InnerSolution`; `XCAFDoc_DimTolTool` label enumeration, reference graph, mutation, and
GDT presentation-map operations; `STEPCAFControl_Reader/Writer` GDT and view modes; and
`XCAFDoc_ViewTool` view enumeration/reference/application operations. `PrsDim` topology
constructors and geometry setters are also blocked because their generated registries do
not own the friendly viewer graph.

The generated `XCAFDimTolObjects` scalar/enum methods are useful implementation inputs,
but they do not by themselves provide document traversal, copied container snapshots,
reference topology, transaction semantics, or viewer ownership. Direct declarations
actually used by the friendly bridge are reconciled by SC-041 with exact stable IDs;
audited but unused candidates remain in their existing disposition.

Implementation reconciles exactly 102 direct blocked OCCT 8.0.1 stable IDs under
SC-041. The complete inventory therefore records 16,353 emitted declarations, 222
accepted manual stable IDs, 49,344 skipped declarations, 50,353 narrowly blocked
declarations, and zero supported-unselected/pending/HD099 declarations.

## Completed implementation evidence

- `Shape` exposes every exact-distance solution with copied points, support kinds,
  optional edge/face parameters, inner-solution state, and independent owning support
  topology. Pair classification covers separated, touching, contained, and interfering
  shapes with optional owning overlap topology.
- Inspection snapshots cover edge/wire length, face/shell area and centroid,
  solid/compound volume, mass, centroid and inertia, linear-edge/planar-face angles,
  circular/cylindrical/conical radius and diameter, and explicit `InspectionUnits`.
- `XdeDocument` owns stable-entry dimension, tolerance, datum, target, and saved-view
  identities. Complete copied snapshots, bidirectional reference graphs, transactional
  create/update/replace/detach/remove, rollback, cross-document guards, and removal
  invalidation are implemented without exposing TDF or XCAF containers.
- GDT and saved-view STEPCAF switches plus explicit AP242 schema selection round-trip
  authored inspection documents. Saved views copy and apply camera, visibility, PMI
  references, and clipping state through the live document/viewer boundary.
- `OcctViewer` owns parent-bound length, angle, radius, and diameter dimensions with
  style/update/show/hide/select/remove operations on the creating thread. The repository
  tests and clean package consumer exercise a real HWND and durable screenshot.
- The bridge corrects OCCT 8.0.1 datum-point X reconstruction from the persisted point
  array and explicitly replaces/removes the tolerance-datum graph when detaching links;
  both are documented in SC-041 and covered by focused regression tests.

## Cross-family dependency closure

### Measurement targets and results

The input boundary accepts existing owning `Shape` values and copied Batch D source
identity. BRepExtrema, adaptors, Boolean Common, and GProp objects remain call-local.
Results are immutable copied records; support topology and optional overlap topology are
new independent registered owning shapes. No native solver, iterator, support reference,
or indexed solution container crosses the ABI.

### Semantic PMI document ownership

`XdeDocument` owns the DimTol and View tools, TDF labels, reference graph, transactions,
and persistence. Public PMI identities are stable label entries parent-bound to that
document. Read operations copy strings, enums, scalars, arrays, placements, and owning
presentation/target topology. Mutation is allowed only inside the existing document
command boundary and must fail atomically for cross-document or invalid references.

### STEP AP242 exchange

STEPCAF reader/writer state remains native-local. GDT/view switches extend the existing
option records; AP242 model type is explicit. The integration fixture is created from
analytic geometry and transactionally authored PMI, then written to a real STEP file and
reopened, so Batch E does not depend on an unlicensed external fixture.

### Saved views and viewer dimensions

Saved model views cross as copied camera, visibility identity, PMI label identity, and
plane values. Applying one resolves identities against the live document and mutates the
existing thread-affine viewer. Review dimensions are viewer-owned parent-bound IDs backed
by native-local `PrsDim`/AIS objects; no generated shared handle is mixed into the
friendly viewer registry. Values and topology needed after removal are copied or owning.

### End-to-end closure

The final integration generates a real AP242/XDE inspection file, reopens it, resolves
PMI to located topology, measures selected geometry, mutates and persists dimension/
tolerance/datum data, applies a saved view, displays review dimensions in a real HWND,
exports and reopens the result, and writes a non-empty screenshot. The same workflow
must execute from the clean package and application-local 62-DLL runtime.

## Validation and completion gates

Batch E reached 24/24 when all of these passed together:

- exact stable-ID reconciliation for every direct manual declaration;
- Release and Debug native/managed builds and generator/runtime tests;
- focused measurement numeric, ownership, disposal, invalid-kind, and failure tests;
- OCAF transaction, rollback, cross-document, label invalidation, and persistence tests;
- real AP242/XDE export/reimport with semantic PMI/reference graph assertions;
- real-HWND parent/thread/viewer lifetime tests and screenshot evidence;
- the same complete workflow from the clean 62-DLL package consumer;
- generated dependency closure, freshness, and byte-identical clean regeneration;
- additive API compatibility, complete inventory classification, runtime hashes,
  SBOM/provenance/checksums, documentation, and the complete local release check.

| Check | Result |
|---|---|
| API/ABI implementation | PASS — package 8.0.1-preview.2, ABI 1.47, bridge 0.55.0 |
| Native/managed compile after Batch E changes | PASS — Release and Debug, 0 warnings/errors |
| Batch E runtime/lifetime/transaction tests | PASS — focused 4/4; full Runtime 119/119 in Release and Debug |
| Real AP242/XDE plus real-HWND integration | PASS — authored AP242, BinXCAF persistence, saved view, four viewer dimensions, non-empty screenshot |
| Clean package consumer for Batch E | PASS — clean restore/publish/runtime with the application-local 62-DLL closure |
| Full local release check after Batch E implementation | PASS — `eng/release-check.ps1 -PackageVersion 8.0.1-preview.2` |

The final inventory classifies 116,272/116,272 declarations and 7,090/7,090 headers;
7,058 headers are semantically scanned and the 32 named parse failures retain stable
header dispositions. Its SHA256 is
`2C8DE4940EAB609C5B24BCE45B50A473BF4120004DD337247E0172C0D1CAC3B1`.

## Explicit non-goals

CMM hardware/device integration, metrology uncertainty solvers, automatic GD&T
conformance judgment, arbitrary markup/note authoring, custom fonts/rendering pipelines,
native callbacks, IVtk/VTK, Draw/test, OpenGL ES, exhaustive STEP schema exposure, and
physical managed/native/package splitting are outside Batch E.

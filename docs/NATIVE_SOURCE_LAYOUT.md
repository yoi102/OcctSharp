# Native source responsibilities

## Batch U additions

Modeling adds LocalFeatureData.cpp, ContourFinishing.cpp, ChamferContours.cpp,
LocalDraft.cpp, LimitedPrisms.cpp, LimitedSweeps.cpp, RibSlotFeatures.cpp and
LocalHoles.cpp. LocalFeatures.hxx owns the copied input/history contract and checked
Form adapter; ContourPrograms.hxx owns ordered contour snapshots. The existing
FeatureResult storage has an optional copied local-feature payload; no registry is
added. Facade-only document/evaluation/viewer orchestration introduces no reverse
Documents dependency. The public LocalFeatures C companion adds eleven calls.
Current audit and binary evidence belong in STATUS, not in historical counts below.

## Batch T additions

Four Documents units add FunctionGraph.cpp, ParametricState.cpp, TopologyNaming.cpp
and ParametricRelocation.cpp; Modeling/ParametricTransform.cpp owns exact transform
correspondence. Documents/Parametric.hxx contains only local graph helper declarations
and ABI assertions. OcctSharp.Native.Parametric.h is a fixed C companion. No new
owner/registry/project/DLL or reverse Documents dependency is introduced. Source
layout is verified at 68 manual units, 585 C exports and 40 standalone strict private
headers; six negative cases pass. Binary compatibility verifies 29,459 identical
Release/Debug names, preserving all S exports and adding exactly sixteen C calls.
Final whole-batch validation is reported in STATUS.

## Batch S additions

Eight units add `Modeling/ScalarLaws.cpp`, `GuidedInputs.cpp`, `GuidedSweep.cpp`,
`GuidedLoft.cpp`, and `Surfaces/ConstraintResiduals.cpp`, `ConstrainedFilling.cpp`,
`PatchConversion.cpp`, `ConstraintContinuity.cpp`. Three small private headers own
law/input/residual helper contracts; the public Authoring companion owns fixed C
records. Existing FeatureResult private storage gains copied authoring history;
its registry/release owner is unchanged. Source audit passes with 63 units, 569
manual C exports and 23 unique storage definitions; all 39 private headers pass
standalone strict MSVC checks and six layout negatives pass. Release/Debug match
at 29,443 exports: R's 29,432 plus nine C entries and two OCCT construction helpers.
Full final validation is in STATUS. No manual PCH, unity, new project or DLL is added.

## Batch R additions (locally validated historical checkpoint)

R adds `Mesh/MeshAuthoring.cpp`, `Mesh/MeshEditing.cpp`, `Mesh/MeshTopology.cpp` and
`Exchange/MeshExchange.cpp`, with the private `Mesh/MeshAuthoring.hxx` and public
`OcctSharp.Native.Mesh.h` copied-buffer contract. These units own validation/authored
triangulation, call-local Poly editing, existing-cache/owning-topology adapters, and
direct editable import/no-remesh format delivery respectively. No new registry/live
set is introduced. Shared exact-face validation belongs to `Modeling/Topology`.

The R source audit passes with 55 independent units, 36 private headers,
560 manual C exports and 23 unique storage definitions; all six negative fixtures
pass. Every private header passes standalone MSVC `/Zs /W4 /WX`, with no PCH/unity.
Release and Debug have identical 29,432 export names, retaining all 29,416 Q names
and adding sixteen mesh exports. Managed comparison against Q adds 404 signatures
and removes none. Release/Debug and isolated actual Debug-native Runtime pass 229/229.
The following Q/ADR-0081 counts remain their historical checkpoints.

## Batch Q additions (locally validated historical checkpoint)

The Q implementation has 51 independent units, 35 private headers, 544 manual
C exports and 23 unique shared storage definitions. The ADR-0081 counts below remain
the historical extraction baseline. Q adds `RepairData`, `RepairDiagnostics`,
`RepairBoundaries`, `RepairFixers`, `RepairExecution`, `RepairSewing` and
`TopologyNormalization` under Modeling, `RepairPublication` under Xde and
`RepairReview` under Visualization. `Modeling/Repair.hxx` owns only private repair
contracts. `OcctSharp.Native.Repair.h` is the new public C companion; Xde/Visualization
consume its copied ABI values without including Modeling's private header.
`Runtime/Registry.cpp` alone owns `LiveRepairResults`. Explicit CMake registration,
no-PCH/unity and the source-size/unique-storage audit pass. The single native DLL and
the existing managed project graph are preserved.
All 35 private headers pass standalone MSVC checks. Release/Debug retain all 29,402
previous export names and expose the same 29,416-name additive surface. Complete
regression, actual Debug-native lifetime and fresh-source rebuild gates pass.

Status: fully implemented and locally validated under ADR-0081.

The delivered library remains one `OcctSharp.Native.dll`. Source folders describe
implementation responsibilities, not independent allocators, ABI packages or DLLs.

| Area | Responsibility |
|---|---|
| Runtime | ABI identity/layout assertions, error containment, one manual registry state and owning shape support |
| Foundation | Copied text, owning string/collection operations and shared transient support |
| Geometry | Copied geometric values, transforms and parameter conversions |
| Modeling | Construction, topology/geometry inspection, features/history, freeform, sketch, measurement and drawing |
| Mesh | Triangulation, copied/authored mesh snapshots, call-local Poly editing and owning discrete/cache adapters |
| Documents | OCAF lifecycle, attributes, references, trees, named shapes and history |
| Xde | Assembly structure, labels, metadata, presentation styles, PMI and saved views |
| Exchange | Shape/session exchange, XDE STEP/IGES/mesh exchange and transfer recovery |
| Visualization | Viewer lifecycle, presentations, selection, camera/input, clipping, dimensions and manipulators |
| Surfaces | Surface inspection, copied curves/topology, constrained filling, patch conversion and residual verification |

Private headers own native-only handle layouts and the small helper contracts needed by
other translation units. C++ helper visibility is internal to the bridge, while public
C declarations stay in `include/OcctSharp.Native.h` and the Surface, Repair, Mesh and Authoring companions.
Generated code remains generator-owned and uses the unchanged Internal support header.

## Scope and unchanged boundaries

The historical 13,510-line `OcctSharp.Native.cpp` is fully replaced by 39 independently
compiled `.cpp` files and 33 private `.hxx` files. Together with the three existing
Batch P surface implementations and their private header, the manual bridge has
42 translation units and 34 private headers across ten responsibility folders.
The largest implementation is `Modeling/Freeform.cpp` at 855 lines; the largest private
header is `Exchange/XdeExchange.hxx` at 132 lines. No replacement umbrella implementation
header, included `.cpp`/`.inc`, manual PCH, or unity build hides the old monolith.

The twelve managed module assemblies and compatibility facade are unchanged.
Preview.15, ABI 1.59, bridge 0.67.0, and binding-model schema 1.13 remain unchanged.
There are no new APIs, manual stable IDs, ownership categories, or algorithm changes.

## Complete implementation map

Paths below are relative to `OcctSharp/src/OcctSharp.Native/src/`. The C-export column
counts definitions in each manual source, not managed APIs or accepted OCCT stable IDs.
The 511 historical definitions plus 19 existing Batch P definitions total 530.
Generated source remains generator-owned in the separate `generated/` directory.

| Implementation | Responsibility | C exports |
|---|---|---:|
| `Runtime/Abi.cpp` | ABI size/alignment assertions and runtime version identity | 3 |
| `Runtime/Error.cpp` | Single TLS diagnostic buffer, error setter and generated adapter | 1 |
| `Runtime/Registry.cpp` | Unique manual live-set and mutex storage | 0 |
| `Runtime/Shape.cpp` | Shape allocation, validation, generated adapters and release | 1 |
| `Runtime/Validation.cpp` | Shared path, UTF-8 buffer, scalar and array validation | 0 |
| `Foundation/Collections.cpp` | Owning sequence/array/vector/maps and copied snapshots | 43 |
| `Foundation/Text.cpp` | Owning ASCII/extended strings and copied UTF-8 conversions | 16 |
| `Foundation/Transient.cpp` | Shared transient owners, reference counts, RTTI and casts | 10 |
| `Geometry/Conversions.cpp` | Private point/vector/axis/plane conversions and validation | 0 |
| `Geometry/Transforms.cpp` | Owning transforms, locations, vectors, axes and matrices; shape placement | 44 |
| `Geometry/Values.cpp` | Copied XYZ, line, circle, axis and plane operations | 28 |
| `Modeling/Construction.cpp` | Primitives, edges, wires, faces, compounds, basic loft/pipe/sewing | 21 |
| `Modeling/Topology.cpp` | Topology snapshots, validation, adjacency and history queries | 12 |
| `Modeling/GeometryInspection.cpp` | Adaptor snapshots, evaluation, derivatives, projection and trim | 13 |
| `Modeling/Inspection.cpp` | Bounds, distances, measurements, classification and DMU analysis | 11 |
| `Modeling/Properties.cpp` | GProp accumulators, mass, center, inertia and principal properties | 10 |
| `Modeling/Operations.cpp` | Extrude/revolve, fillet/chamfer, offset/thickness, Boolean/history and healing | 15 |
| `Modeling/Features.cpp` | Selected feature execution and owning result/history/diagnostic lifecycle | 7 |
| `Modeling/Freeform.cpp` | Curve/surface definitions, fitting, editing and profile-to-topology | 26 |
| `Modeling/Sketch.cpp` | Planar curves, intersections, wires, faces and containment | 7 |
| `Modeling/Drawing.cpp` | Hidden-line/section computations and copied drawing polylines | 4 |
| `Mesh/Triangulation.cpp` | Basic, detailed and advanced triangulation snapshots | 6 |
| `Documents/Lifecycle.cpp` | Document create/open/save/release, transactions, labels and names | 14 |
| `Documents/State.cpp` | Attributes, arrays, references, trees, named topology, undo/redo and history | 33 |
| `Xde/Document.cpp` | XDE document/tool initialization and copied root import support | 1 |
| `Xde/Structure.cpp` | Assemblies/occurrences, placement, cloning, references and SHUO | 28 |
| `Xde/Metadata.cpp` | Colors, layers, materials, styles and validation properties | 18 |
| `Xde/Pmi.cpp` | Dimensions, tolerances, datums, values, auxiliary shapes and references | 21 |
| `Xde/SavedViews.cpp` | Saved camera/view definitions and clipping planes in documents | 5 |
| `Exchange/ShapeExchange.cpp` | Geometry-only format I/O and selective STEP reader sessions | 21 |
| `Exchange/StepAssembly.cpp` | One-shot STEP assembly merge orchestration | 1 |
| `Exchange/XdeExchange.cpp` | Metadata-preserving STEP/IGES/mesh I/O and STEP style recovery | 17 |
| `Visualization/Context.cpp` | HWND/thread-affine display-driver/viewer/context lifecycle | 2 |
| `Visualization/Presentations.cpp` | Shape/XDE display, appearance, transforms and subshape overrides | 16 |
| `Visualization/Selection.cpp` | Detection/selection, copied topology, filters and fit-selected | 15 |
| `Visualization/Navigation.cpp` | Camera/input, coordinates, background, trihedron and screenshots | 19 |
| `Visualization/Clipping.cpp` | Viewer-parent-bound clip-plane lifecycle | 4 |
| `Visualization/Dimensions.cpp` | Viewer-parent-bound dimension presentations and styles | 5 |
| `Visualization/Manipulators.cpp` | Manipulator configuration, interaction and state | 13 |
| `Surfaces/SurfaceInspection.cpp` | Existing Batch P domain inspection, projection, sections and metrics | 6 |
| `Surfaces/SurfaceCurves.cpp` | Existing Batch P copied pcurves, lifting and sampled curves | 4 |
| `Surfaces/SurfaceTopology.cpp` | Existing Batch P repair/trim/split/analytic/offset topology | 9 |

## Private contracts and storage owners

Non-template implementations stay in their corresponding `.cpp`; only small templates
and inline support remain in headers. Domain helpers use `OcctSharp::Native`; opaque
handle definitions retain the global names of the unchanged C forward declarations.
The pre-existing Surface helper namespace and Internal adapter names are retained.
These C++ helpers are not DLL exports and are not a consumer contract.

| Handle or shared state | Definition owner | Lifecycle / consumers |
|---|---|---|
| `LastError` | `Runtime/Error.cpp` | One TLS buffer shared by manual guards, generated adapters and surface guards |
| `LiveShapesMutex` and 20 `Live*` sets | `Runtime/Registry.cpp` | Unique manual storage; generated per-type registries stay unchanged |
| Shape handle | `Runtime/Shape.hxx` | Allocate/validate/release in `Runtime/Shape.cpp`; manual and generated topology |
| Transient handle and RTTI probe | `Foundation/Transient.hxx` | Intrusive OCCT sharing |
| ASCII/extended string handles | `Foundation/Text.hxx` | Owning strings and caller-owned buffers |
| Sequence/array/vector/map handles | `Foundation/Collections.hxx` | Owning containers with bounded access, no borrowed iterators |
| Transform/location/vector/direction/axis/matrix handles | `Geometry/Transforms.hxx` | Owning native values and copied results |
| GProp handle | `Modeling/Properties.hxx` | Owning accumulator lifecycle |
| Feature result/history storage | `Modeling/Features.hxx` | Matching result release operation |
| OCAF/XDE document handle | `Documents/Lifecycle.hxx` | One owner reused by Xde/Exchange; labels stay parent-bound |
| STEP reader handle | `Exchange/ShapeExchange.hxx` | Owning selective transfer session |
| Viewer handle and parent-scoped IDs | `Visualization/Context.hxx` | One thread-affine graph; no second subfeature context |

All remaining private headers correspond to helper contracts in the implementation
map. `include/OcctSharp.Native.h` and `include/OcctSharp.Native.Surface.h` remain the
public C contract. `include/OcctSharp.Native.Internal.hxx` remains the shared generated
adapter contract. `Surfaces/SurfaceCommon.hxx` reuses that adapter plus the existing
`OcctSharp_Internal_BuildSketchCurve` implementation in `Modeling/Sketch.cpp`.

## Dependency direction

The actual private domain-header dependencies are acyclic:

- Runtime does not include domain headers. Foundation, Geometry and Mesh depend on Runtime.
- Modeling depends on Geometry and Runtime. Documents depends on Foundation and Runtime.
- Xde depends on Documents, Foundation, Geometry and Runtime.
- Exchange and Visualization depend on Xde, Documents, Geometry and Runtime.
- Surfaces uses the unchanged Internal support contract and the sketch adapter.

This is a source-maintenance boundary, not a claim that all OCCT toolkit dependencies
can be separated into independently deployable libraries. No new concurrent
release/use guarantee or cross-DLL allocation/release protocol is introduced.

## Preparation and verification

The move baseline is `5620ae5`. LF-normalized legacy source SHA256:
`B22F73FFD21546F35483708D39F16FB18E9E86EC38E86627318D929C2D132195`.
The 693 original function bodies, including all 511 complete historical C ABI
definitions, have been compared without rewriting their algorithms. A final source
ownership review moved all XYZ value functions into Geometry, not document lifecycle.
All 34 private headers also pass standalone MSVC `/TP /Zs` syntax checks. This found
and corrected an implicit `Standard_GUID.hxx` dependency in `Xde/Structure.hxx`.

`eng/verify-native-source-layout.ps1` runs from the normal build. It checks explicit
CMake registration, duplicate C entry points, unique registry/error storage owners,
the 1,000-line implementation ceiling, and the prohibition on included implementations,
manual PCH and unity builds. Its optional DLL comparison checks the complete native
export-name set, including generated exports. The JSON report is written under
`OcctSharp/artifacts/native-source-layout.json`. Six negative fixture checks in
`eng/test-native-source-layout.ps1` cover missing source registration, duplicate TLS,
implementation inclusion, unity/PCH and oversized files without mutating real source.

`NativeSourceBoundaryTests` verifies manual construction to generated topology to
manual inspection/release, shared cross-domain errors, and thread-local isolation.
Full regression must additionally run with the actual Debug native runtime, through
real STEP/IGES/XDE/HWND workflows, and from both facade/direct-module clean packages.
Same-baseline managed API comparison requires zero additions and zero removals.
Current validation results and final DLL/package hashes are in repository
`docs/STATUS.md`, which is intentionally excluded from package documentation.

## Prepared Q-T additions (not implemented)

ADR-0082 keeps this complete baseline map and the one-DLL boundary. Q's repair,
diagnosis and normalization belong in cohesive Modeling units; R authoring/editing/
discrete adapters in Mesh; S law/guided-sweep and filling/patch conversion in
Modeling/Surfaces; T graph/state/naming storage in Documents. Cross-family orchestration
must not create reverse private-header dependencies or duplicate Runtime owners.
In particular, do not expand the 855-line Freeform.cpp for all of S. Register future
units explicitly and retain the 1,000-line ceiling and standalone-header checks.
The four proposed placements and integration owners are detailed in
[Q-T preparation](BATCH_Q_T_PREPARATION.md); they do not change today's 42/34 source counts.

ADR-0083 additionally plans U contour finishing/local draft/limited features and V
partition regions/volume construction/region inspection as cohesive Modeling units;
W lighting/appearance/frame capture belongs in Visualization and reuses Context.hxx.
Do not enlarge the historical Features.cpp switch into another monolith. The source
counts above remain unchanged; see [U-W preparation](BATCH_U_W_PREPARATION.md).

## Rules for subsequent work

1. Put a new operation with its owning responsibility above; use a private contract
   only when another translation unit really needs it. Do not grow a universal header.
2. Register every manual implementation explicitly in CMake; generated files continue
   through the generator-owned collection. Keep independent no-PCH compilation.
3. Reuse existing runtime owners. A new handle category needs documented ownership and
   matching release semantics, not a local duplicate of a registry or error buffer.
4. Preserve generator/manual separation. This extraction is not Batch Q, does not
   reduce the remaining OCCT API inventory, and does not authorize native DLL splitting.

Related: [architecture](ARCHITECTURE.md), [ownership](OWNERSHIP.md),
[ADR-0081](adr/0081-native-source-responsibility-extraction.md).

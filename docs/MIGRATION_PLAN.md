# Complete OCCT Migration Plan

## Objective

OcctSharp targets complete, regeneratable C# coverage of the selected OCCT profile.
Complete coverage does not mean blindly emitting every AST declaration. It means every
catalogued public declaration is deterministically classified as generated, intentionally
manual, unavailable for the selected build, or blocked by a documented rule with an
owner and removal condition.

The plan keeps four percentages separate:

1. **Inventory completeness** — successfully parsed entry headers divided by catalogued
   headers for one named profile.
2. **Classification completeness** — declarations with a stable supported/manual/skipped/
   blocked disposition divided by discovered declarations.
3. **Binding coverage** — emitted plus accepted manual public declarations divided by
   bindable public declarations. Private, deleted, unavailable, and uninstantiated generic
   templates are reported but are not the public binding denominator.
4. **Validation coverage** — emitted declarations whose required compile/runtime/lifetime/
   integration gates pass divided by emitted declarations.

Roadmap progress is an engineering estimate and is never substituted for these metrics.

The current execution priority is not maximum raw OCCT coverage. ADR-0066 selects one
24-capability engineering-inspection, exact-measurement, and PMI/AP242 outcome across
BRepExtrema/BRepAdaptor/BRepGProp/TopoDS, XCAFDoc/XCAFDimTolObjects, STEPCAF/AP242,
PrsDim/AIS, V3d/Graphic3d, and screenshot evidence. Cold or optional declarations remain
classified, but they do not pre-empt that finite product workflow.

## Product and project structure

### Current structure

Keep one managed project, one native bridge, one NuGet package, and one application-local
`occt` runtime directory through the active Batch E inspection/PMI wave. Generated
files are organized by module immediately, even while they compile into the same assembly.

### Planned managed projects and packages

| Project/package | OCCT responsibility | Dependency direction |
|---|---|---|
| `OcctSharp.Runtime` | ABI identity, loader, errors, safe handles, common ownership contracts | None |
| `OcctSharp.Foundation` | Standard, NCollection, TCollection, Message, math, OS/process foundations | Runtime |
| `OcctSharp.Geometry` | gp, Geom/Geom2d, adaptors, approximation, extrema, properties | Foundation |
| `OcctSharp.MeshData` | Poly triangulations/polygons and copied mesh-data contracts | Geometry |
| `OcctSharp.Modeling` | TopAbs/TopLoc/TopoDS, BRep, algorithms, construction, healing | Geometry, MeshData |
| `OcctSharp.Mesh` | BRepMesh/IMesh/XBRepMesh algorithms and bulk meshing workflows | Modeling, MeshData |
| `OcctSharp.Documents` | OCAF/TDF/TData/TDocStd, persistence, binary/XML document drivers | Modeling, MeshData |
| `OcctSharp.Visualization` | Graphic3d, Prs3d, AIS, SelectMgr, V3d and window integration | Modeling, Mesh, Documents |
| `OcctSharp.DataExchange` | STEP, IGES, STL, OBJ, PLY, GLTF, VRML and exchange infrastructure | Modeling, MeshData, Mesh, Documents, Visualization |
| `OcctSharp.Xde` | XCAF, STEPCAF metadata, assemblies, and XDE persistence adapters | Documents, DataExchange, Visualization |
| `OcctSharp.IVtk` | Optional VTK integration | Visualization plus external VTK assets |
| `OcctSharp.OpenGles` | Optional OpenGL ES backend | Visualization plus GLES/EGL assets |
| `OcctSharp.Draw` | Optional Draw/test harnesses | Xde, Visualization |
| `OcctSharp` | Meta-package and convenience facade | All stable non-optional packages |

The first package split occurs only after common runtime types can move without public
API duplication and at least one of these triggers is met:

- a generated module exceeds 5,000 public members or materially slows normal builds;
- an optional external dependency such as VTK must remain absent for core consumers;
- the second RID requires a separate native asset package;
- independent module versioning or release validation becomes necessary.

Native assets later move to RID-specific packages such as
`OcctSharp.Native.win-x64`. Native code remains one bridge until a central cross-module
handle registry and creator-owned release contract prove that module bridge DLLs are safe.

ADR-0062 completes the generated-signature prerequisite: all 27 observed cross-shard
edges are resolved, compatible with this graph, and acyclic. It does not perform the
physical split. Managed projects still need public assembly-identity/type-forwarding and
manual-facade migration evidence; native DLLs still need cross-DLL registry, allocator,
and creator-routed release evidence. Until those separate decisions are accepted, the
current single assembly/DLL/package layout remains authoritative.

## Product-scale migration batches

Batch B is complete. The former B00-B20 and dotted Bxx.y labels are retired planning
labels, not current batches or commit boundaries.
Repository/toolchain, generator, ownership, foundation, modeling, mesh, exchange, XDE,
visualization, long-tail generation, upgrade, and release engineering all belong to B.
ADR-0060 opened product-scale batch `C` for common CAD API expansion and ADR-0063 closes
it at alpha.54. It used whole-product workflow scope comparable to B; numbered or dotted
batch fragments remain forbidden. ADR-0064 closes product-scale batch `D` with the same
large-wave rule and a finite production viewport/model-review denominator. ADR-0066 opens
Batch `E` as one finite 24-capability inspection/measurement/PMI/AP242 denominator.

| Batch | Status | Completed evidence | Remaining exit conditions |
|---|---|---|---|
| B | Complete (local implementation) | Reproducible .NET 10/native foundation; deterministic generation; safe value, shared, topology, document, metadata, exchange, modeling, mesh, and visualization profiles; 16,353 emitted plus 61 manual stable IDs; zero supported-unselected/LT001-LT004; complete observed classification; Release/Debug/runtime, 13-file freshness, byte-identical clean regeneration, 62-DLL package consumer, compatibility, provenance/SBOM/checksum, and local release gates passing | None inside Batch B. Signing credentials and NuGet publication remain independent release-readiness gates |
| C — Common CAD API Expansion | Complete (local implementation) | Alpha.51 validates topology/BREP/mesh/XDE/viewer editing; alpha.52 validates STEP reporting/BRepCheck/ShapeFix/V3d rotation; alpha.53 validates BRepGProp/XCAF properties/XDE occurrences/STEPCAF options; alpha.54 closes the final 15-capability geometry/topology/selective-STEP/viewer-input workflow. Release/Debug, Generator 91/91, Runtime 114/114, 83-file deterministic regeneration, 62-DLL clean consumer, inventory, dependency closure, API compatibility, provenance, and local release gates pass | None inside Batch C. Hosted CI execution, signing, and NuGet publication remain independent release-readiness gates; advanced filters, custom rendering, optional integrations, cold schema, and exhaustive mesh attributes require a new product denominator |
| D — Production CAD Viewport and Model Review | Complete (24/24, local implementation) | Alpha.55 implements the entire ADR-0064 denominator with copied XDE identity, owning detected/selected topology, area selection/filtering, subshape review styling, camera/conversions, clipping/review aids, and screenshot output. Release/Debug, Generator 91/91, Runtime 115/115, real STEP/XDE plus real-HWND, clean 62-DLL package, regeneration, inventory, compatibility, provenance, and local release gates pass | None inside Batch D. A future product batch requires a separately locked finite denominator; hosted release, signing, and NuGet publication remain independent release-readiness gates |
| E — Engineering Inspection, Exact Measurement, and PMI/AP242 | Complete (24/24, local implementation) | Preview.2 implements the entire ADR-0066 denominator: exact solution/support and engineering measurements, explicit units, complete dimension/tolerance/datum/saved-view snapshots and reference mutation, AP242/BinXCAF persistence, four viewer-owned dimension kinds, and screenshot output. Release/Debug, Generator 91/91, Runtime 119/119, focused 4/4, real AP242 plus real-HWND, clean 62-DLL package, regeneration, inventory, compatibility, provenance, and local release gates pass | None inside Batch E. The next product batch requires a separately locked finite denominator; hosted release, signing, and NuGet publication remain independent release-readiness gates |

Current B completion is not represented by counting retired planning labels. Engineering
progress, selected binding coverage, full-profile coverage, inventory completeness,
validation coverage, and publication readiness are reported independently. B through E
are 100% for their accepted local denominators because every local exit condition above
has evidence; this does not make the public package release-ready. New implementation
progress requires another locked product denominator and is never reported by
reopening B/C/D or inventing numbered or dotted E labels.

The 16,353 emitted plus 61 accepted manual stable IDs are Batch B's baseline binding
coverage. Batch C adds 41 accepted manual stable IDs across SC-036 through SC-039 and
locks every capability denominator in `COMMON_API_GAP_INVENTORY.md`; every declared
capability is runtime/package validated. The resulting alpha.54 inventory has 16,353
emitted plus 102 manual stable IDs. Completion follows the finite workflow and validation
contract, not declaration counts alone.

### Completed capability milestones inside B

- Deterministic generator, C ABI, .NET 10 workspace, package-local `occt/` runtime, and
  fresh-clone native bootstrap.
- Scalar/enums, intrusive shared handles, typed topology, transforms, strings and common
  scalar collections, geometry values/adaptors, construction, traversal, Boolean/healing,
  mesh snapshots, geometry exchange, OCAF/XDE metadata and assemblies, and Windows viewer.
- Generated StepBasic default-constructible shared-entity closure: 333 emitted
  declarations and 129 public generated types.
- Generated Geom/Geom2d shared-handle expansion: eight additional public types and 67
  additional emitted declarations.
- Generated common mesh/analysis/healing expansion: binding-model schema 1.2 excludes
  abstract records and adds 61 public BRepMesh/Poly/ShapeAnalysis/ShapeFix/ShapeUpgrade
  types plus 375 emitted declarations, bringing the manifest to 775 stable IDs.
- Accepted common-modeling bridge: 18 audited manual declarations for cone/torus,
  extrusion/revolution, fillet/chamfer, offset, section, bounds, validity, and counts.
- Current high-value bridge wave: 43 additional audited declarations for curve/surface
  construction and evaluation, projection, adjacency, loft/pipe/sewing/thick-solid,
  Boolean history summaries, wedge construction, and composable XDE STEP import.
- Alpha.48 IGES wave: generated IGESAppli, IGESBasic, IGESDefs, IGESDimen, IGESDraw,
  IGESGeom, IGESGraph, and IGESSolid shared entities add 984 emitted declarations and
  162 public wrappers. ABI 1.40/bridge 0.48.0, Generator 44/44, Runtime 147/147,
  clean regeneration, package consumer, and API diff (10,272 additions/0 removals)
  are validated on .NET SDK 10.0.400. The full observed inventory is classification-
  complete with 4,060 emitted, 61 manual, 11,144 supported-unselected, 27,310 skipped,
  and 73,639 blocked declarations; B remains in progress.
- Alpha.49 final long-tail wave: 16,353 emitted and 61 manual stable IDs; zero
  supported-unselected and zero LT001-LT004; 50,514 remaining candidates have narrow
  ABI/export/ownership/type dispositions. Release and Debug pass Generator 62/62,
  Runtime 105/105, and dependency profiles 6/6 with deterministic discovery/reports.

### Historical work sizing inside B

Work is selected by dependency closure, not raw header count. Each implementation wave
combines as many related packages, API families, and user workflows as can share a
truthful ownership contract and validation matrix. Tiny convenience methods are folded
into the active wave. Different lifetime categories or optional external dependencies
may be implemented and validated separately, but they do not create another batch.
Every material wave records source packages/toolkits, stable IDs, tests, package impact,
coverage change, and the next large workstream before work continues.

## Batch C: Common CAD API Expansion — complete

### Product outcome

A Windows x64 .NET application should be able to build or import a model, inspect and
edit its geometry/topology, mesh it, preserve document metadata, export it, and display
and select it without dropping to unmanaged OCCT for the routine path. Existing Batch B
APIs are the baseline; Batch C fills the high-frequency gaps and replaces narrow manual
bridges with generalized generated ownership/type-map rules where that is safer.

The three headings below are coverage lanes, not phases or independently completable
sub-batches. A large implementation wave deliberately takes connected work from several
lanes so that it finishes a user workflow rather than a class list.

### Model and inspect lane

- Complete commonly used `gp` values and transforms, `Geom`/`Geom2d` curves and surfaces,
  trimming/conversion/evaluation, projection/intersection, parameter and length helpers.
- Complete typed `TopoDS` creation/copy/orientation/location plus `TopExp` traversal,
  ancestor/descendant and adjacency maps, vertex/edge/wire/face extraction, and stable
  owning snapshots where native iterators or references cannot cross the ABI.
- Complete routine BRep queries: curve/surface access, UV/parameter ranges, continuity,
  bounds, mass properties, extrema/distance, validity, tolerance, closure, and diagnostics.

### Build, modify, and deliver lane

- Complete common construction/edit workflows: vertices, edges, wires, faces, shells,
  solids and compounds; primitives; transforms/copies; extrusion, revolution, loft,
  sweep/pipe, sewing, fillet, chamfer, offset, thick solid, section, Boolean operations,
  Boolean history, healing and upgrade operations.
- Complete meshing controls and status plus efficient face triangulation snapshots with
  positions, indices, normals, UVs, locations, orientation, and predictable remapping.
- Complete routine BREP/STEP/IGES/STL/OBJ/GLTF read/write status and options, multi-root
  transfer, unit/system diagnostics, and real-file failure reporting.
- Complete common OCAF/XDE document workflows for transactions, label trees, names,
  colors, layers, materials, properties, assemblies, occurrences, locations, persistence,
  and STEPCAF import/export round trips.

### Present and interact lane

- Complete viewer/view lifecycle, display/remove/redisplay, per-object color/material/
  transparency/display-mode state, fit-all, camera orientation/projection, zoom/pan/
  rotation, resize/redraw, and deterministic thread-affinity diagnostics.
- Complete common detection/selection modes, selection snapshots, clear/toggle behavior,
  and application-owned mouse/keyboard event forwarding without exposing AIS/V3d/
  SelectMgr pointers or reverse callbacks.

### First large implementation wave — complete local checkpoint

The first C wave is **common solid editing and inspection**. It is one cross-family wave,
not several tasks: expand geometry/topology query primitives, common BRep construction
and modification, mesh extraction, STEP/XDE round-trip state, and viewer presentation/
selection enough to run one end-to-end workflow:

```text
create or import
  -> inspect topology and underlying geometry
  -> transform and apply common modeling operations
  -> validate, measure, and mesh
  -> preserve names/colors/material/assembly placement
  -> export
  -> display and select
```

Alpha.51 completes this declared workflow through nine audited SC-036 manual stable IDs
and existing generated/friendly dependencies. Release and Debug, runtime/lifetime,
real-HWND, clean regeneration, clean package consumer, API compatibility, inventory,
runtime manifest, SBOM/provenance/checksum, and local release gates pass. The exact
capability denominator and evidence are recorded in `COMMON_API_GAP_INVENTORY.md`.
This checkpoint alone did not mark the whole Batch C exit contract complete.

### Second large implementation wave — import diagnostics and repair

The second C wave closes one dependency chain across STEPControl/XSControl, BRepCheck,
ShapeFix, and V3d: read STEP geometry with transfer/unit status, copy detailed validation
issues, repair to an owning result, compare immutable before/after reports, and drive
mouse rotation on the thread-affine viewer. Alpha.52 uses six audited SC-037 stable IDs
plus existing generated V3d and ShapeFix dependencies. Its exact 7-capability denominator
and complete local checkpoint evidence are recorded in `COMMON_API_GAP_INVENTORY.md`.

### Third large implementation wave — XDE validation properties and occurrences

The third C wave closes one dependency chain across BRepGProp, XCAF validation
attributes, XDE assembly/location traversal, and STEPCAF: compute area/volume/centroid,
store or clear optional document attributes, flatten nested occurrences with composed
world placement and independent located shapes, then control STEP metadata and model
representation on write/read. Alpha.53 uses nine audited SC-038 stable IDs and existing
owning/property/location infrastructure. Its exact 8-capability denominator and complete
local checkpoint evidence are recorded in `COMMON_API_GAP_INVENTORY.md`.

### Final large implementation wave — selective STEP, topology edit, and viewer input

The final C wave closes the remaining finite high-frequency chain across `BRepAdaptor`,
`BRep_Tool`/`Geom2d`, `BRepBuilderAPI`, `BRepTools_ReShape`, `TopExp`,
`STEPControl`/`XSControl`, and `AIS`/`V3d`. It adds copied edge/surface derivatives and
pcurves; owning edge/face trim, wire, replace, and remove results; bidirectional copied
adjacency; owning STEP reader sessions with unit metadata and selective root transfer;
whole/subshape selection modes; owning selected topology snapshots; and parent-bound
mouse, wheel, and semantic keyboard forwarding.

Alpha.54 uses 17 audited SC-039 stable IDs and existing generated/friendly dependencies.
Its 15/15 capability denominator passes focused ownership/failure/disposal checks and one
real STEP import/edit/export/re-read/real-HWND viewer workflow. The complete local chain
passes Release and Debug builds, Generator 91/91, Runtime 114/114, dependency profiles
6/6, 83/83 freshness and byte-identical regeneration, the 62-DLL clean consumer, API
compatibility, complete classification, dependency closure, provenance, and release
gates. This closes Batch C.

### Large-wave execution rules

- No `C01`, `C.1`, per-class batch, per-method milestone, or commit-as-progress counter.
- Normally include at least three connected API families and one end-to-end workflow in
  one implementation wave. A smaller fix is admitted only when it blocks several common
  families and remains part of the same active wave.
- Fold overloads, enums, options, diagnostics, disposal, and convenience methods into
  the owning family instead of scheduling them as later micro-work.
- Prefer parser/model/type-map/ownership/emitter generalization over a manual wrapper
  that solves one occurrence. Keep friendly facades for intentional workflow design.
- Use focused generation/compile/runtime checks during implementation. Run full
  Release/Debug, freshness, clean regeneration, package consumer, inventory, SBOM and
  release gates at the large-wave checkpoint, not after every small addition.
- Do not stop at generated or compiled. The wave checkpoint requires runtime semantics,
  ownership/failure paths, integration, and representative real-file evidence.
- Do not let optional IVtk, Draw/test, C++/CLI, OpenGL ES, platform backends, deprecated
  APIs, or allocator/compiler internals displace the Windows-core common workflow.

### Batch C exit criteria

- [x] Every declared common workflow above has an intentional public/raw boundary and no
  routine step requires an undocumented native escape hatch.
- [x] Every selected common family is emitted or accepted manual with an explicit ownership,
  error, lifetime, threading, and data-transfer contract.
- [x] End-to-end tests cover create/import, inspect, edit, validate/measure, mesh, metadata,
  export, display, and selection with representative real CAD files.
- [x] Release and Debug, deterministic regeneration, clean package consumer, API compatibility,
  runtime manifest, inventory accounting, provenance, and documentation gates pass.
- [x] Remaining blocked/cold APIs keep narrow dispositions but do not hide an unfinished
  high-frequency workflow.

Advanced selection filters, custom rendering pipelines, low-frequency schema entities,
optional integrations, and exhaustive mesh attributes are outside the accepted Batch C
denominator. They require a future product decision and do not keep C active.

## Batch D: Production CAD Viewport and Model Review — complete

### Product outcome and denominator

Batch D turns the alpha.54 viewer core into a production review workflow. One real
STEP/XDE assembly must flow through copied occurrence/presentation identity, exact owning
detection, point/rectangle/polygon selection, built-in filters, selection bounds/fit/
isolate, per-subshape review styles, camera snapshot/restore, screen/world/pick-ray
conversion, window zoom, background, clip plane, computed hidden-line mode, trihedron,
and durable screenshot evidence.

The complete 24-capability matrix, baseline gaps, decision-driving stable IDs, ownership
closure, and tests are locked in `BATCH_D_VIEWPORT_GAP_INVENTORY.md`. The inventory's 52
candidate OCCT overloads are implementation evidence, not the product denominator.

### Execution rules

- Implement the whole closure as one large wave. No `D01`, `D.1`, selection-only,
  camera-only, clipping-only, screenshot-only, per-class, or per-method checkpoint is a
  completed batch.
- Preserve the existing viewer owner thread and HWND contract. Presentations, filters,
  and clip planes are parent-bound; detection/selection topology is copied into an owning
  `Shape`; XDE identity and camera/coordinate data are copied values.
- Reuse generated declarations when their registries and lifetimes fit. Reconcile only
  direct manual OCCT declarations actually used, with exact stable IDs and no overlap.
- Keep one managed assembly, one native DLL, one package, stable public type full names,
  and the ADR-0061/ADR-0062 generated shard graph.
- Keep IVtk/VTK, OpenGL ES, Draw/test, callbacks, arbitrary managed filters, custom
  rendering/shaders, exhaustive mesh attributes, cold schema, and physical splitting out
  of this batch.

### Batch D exit criteria

- [x] All 24 declared capabilities have intentional friendly/raw boundaries with no
  undocumented unmanaged escape hatch.
- [x] Every direct declaration is emitted or accepted manual with explicit ownership,
  error, lifetime, threading, and data-transfer semantics.
- [x] Focused runtime tests cover validation, parent mismatch, removal/disposal,
  cross-thread calls, source lifetime independence, owning detected/selected topology,
  filter/override reset, camera degeneracy, coordinate conversion, clip planes, and file
  errors.
- [x] A real STEP/XDE assembly and real HWND complete the entire review-to-screenshot
  workflow in repository runtime and a clean 62-DLL package consumer.
- [x] Release/Debug, generator/runtime tests, generated freshness, byte-identical clean
  regeneration, dependency closure, package, API compatibility, inventory, runtime
  manifest, SBOM/provenance/checksums, documentation, and local release gates pass.

Current state: alpha.55 completes 24/24 capabilities (100%). SC-040 reconciles the 18
newly direct blocked stable IDs, Runtime 115/115 passes in Release and Debug, and the real
STEP/XDE plus real-HWND workflow passes both in repository runtime and the clean 62-DLL
package consumer. Generated freshness, byte-identical clean regeneration, dependency
closure, API compatibility, complete classification, runtime hashes, SBOM/provenance/
checksums, and the full local release check pass together.

## Batch E: Engineering Inspection, Exact Measurement, and PMI/AP242 — complete

ADR-0066 and `BATCH_E_INSPECTION_PMI_GAP_INVENTORY.md` lock one 24-capability product
outcome spanning exact distance/contact/interference inspection, length/area/volume/
centroid/inertia and angle/radius/diameter measurement, unit semantics, semantic
dimensions/tolerances/datums and their complete reference graph, transactional mutation,
AP242 GDT and saved-view round trips, viewer-owned annotations, and durable screenshot
evidence. The focused 14-root audit classifies 990 candidate declarations as 142 emitted,
2 manual, 604 blocked, and 242 skipped; these counts guide implementation and are not the
product denominator.

Preview.2 completes 24/24 as one cross-family wave; no family-only or numbered/dotted
checkpoint was used. Exact inspection, complete PMI snapshots/reference mutation,
AP242/BinXCAF persistence, saved views, viewer-owned dimensions, and screenshot evidence
pass in Release/Debug repository runtime and the clean 62-DLL package consumer. SC-041
reconciles 102 direct blocked stable IDs, and the complete local release gate passes while
retaining the single managed assembly, native DLL, NuGet package, application-local
runtime, public type names, and ADR-0061/ADR-0062 generated dependency graph.

## Generated output partitioning

Generated paths use module directories before project splitting:

```text
src/OcctSharp.Native/generated/<Module>/
src/OcctSharp/Generated/<Module>/
```

The manifest remains the only owner of generated paths. Moving a generated file between
modules is a generator change followed by regeneration, never a manual source move.

ADR-0061 implements this source partition and ADR-0062 closes its semantic dependencies.
Binding-model schema 1.3 and manifest schema 1.1 carry stable module/layer/shard
identities; Release generation currently produces 83 manifest-owned files for 16,353
stable IDs, including the separate MeshData shard. They still compile into one
managed assembly and one native DLL, and public `OcctSharp` type full names do not move.
Project/package/native-DLL splitting remains gated by the triggers above and needs a
separate compatibility and ownership decision. `dependency-closure.json` proves zero
unresolved targets, zero graph violations, and zero cyclic groups for the generated
surface; manual facade and binary-identity migration remain outside that claim.

## Dependency profiles

- `windows-core`: OCCT public APIs buildable from the pinned Windows bundle without VTK,
  C++/CLI-only, OpenGL ES-only, or Draw-only surfaces.
- `windows-full`: all Windows-native OCCT modules plus required optional third-party
  development headers.
- `ivtk`: IVtk declarations with a pinned compatible VTK development package.
- Future platform profiles are added only with a compatibility row and native tests.

The same header may be available in several profiles. Coverage is always reported with
the profile name; profile exclusions require stable reasons and do not disappear from the
global catalog.

## Required evidence used for D and each large work wave

- Deterministic discovery, generated source, coverage, and diagnostics.
- Deterministic generated-shard dependency closure with zero unresolved signature target,
  target-graph violation, or cyclic group.
- No unclassified declaration inside the selected common workflow closure.
- Native and managed Debug/Release builds.
- Focused ABI, runtime, ownership, failure, and disposal tests.
- Generated-source freshness and `git diff --check`.
- Clean NuGet consumer whenever public/runtime assets change.
- Updated `STATUS.md` and this plan; only factually affected topic documents and ADRs.

## Active product-batch execution boundary

1. Keep completed Batch B/C/D evidence immutable and report new implementation against E.
2. ADR-0059 resolves the MIT project license and bundled third-party notice layout;
   keep those files and the runtime manifest current for every distribution change.
3. Package signing, credentials, and NuGet publication require explicit authorization.
4. C is one completed common-API product batch and remains immutable evidence.
5. E is the active single 24-capability inspection/measurement/PMI/AP242 wave. Its
   connected families are not batches, version numbers, or permission to publish partial
   completion claims.
6. Exact inspection, semantic PMI/reference graphs, transactional mutation, AP242 GDT/
   saved views, viewer annotations, and screenshots are in E; arbitrary callbacks, custom
   rendering, optional integrations, cold schema, and physical splitting remain outside it.

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

The current execution priority is not maximum raw OCCT coverage. Under ADR-0060 it is
the largest safe expansion of APIs used together by ordinary CAD applications. Cold or
optional declarations remain classified, but they do not pre-empt common modeling,
query, mesh, exchange/XDE, and viewing workflows.

## Product and project structure

### Current structure

Keep one managed project, one native bridge, one NuGet package, and one application-local
`occt` runtime directory through the active Batch C common-workflow expansion. Generated
files are organized by module immediately, even while they compile into the same assembly.

### Planned managed projects and packages

| Project/package | OCCT responsibility | Dependency direction |
|---|---|---|
| `OcctSharp.Runtime` | ABI identity, loader, errors, safe handles, common ownership contracts | None |
| `OcctSharp.Foundation` | Standard, NCollection, TCollection, Message, math, gp, TopAbs, TopLoc | Runtime |
| `OcctSharp.Modeling` | Geom/Geom2d, topology, BRep, algorithms, construction, healing | Foundation |
| `OcctSharp.Mesh` | Poly, triangulation, meshing, bulk buffers | Modeling |
| `OcctSharp.DataExchange` | STEP, IGES, STL, OBJ, PLY, GLTF, VRML and exchange infrastructure | Modeling, Mesh |
| `OcctSharp.Xde` | OCAF, TDF/TDataStd/TDocStd, XCAF, STEPCAF metadata and assemblies | DataExchange |
| `OcctSharp.Visualization` | Graphic3d, Prs3d, AIS, SelectMgr, V3d and window integration | Modeling, Mesh |
| `OcctSharp.IVtk` | Optional VTK integration | Visualization plus external VTK assets |
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

## Product-scale migration batches

Batch B is complete. The former B00-B20 and dotted Bxx.y labels are retired planning
labels, not current batches or commit boundaries.
Repository/toolchain, generator, ownership, foundation, modeling, mesh, exchange, XDE,
visualization, long-tail generation, upgrade, and release engineering all belong to B.
ADR-0060 opens the next product-scale batch, `C`, for common CAD API expansion. It uses
whole-product workflow scope comparable to B; numbered or dotted batch fragments remain
forbidden.

| Batch | Status | Completed evidence | Remaining exit conditions |
|---|---|---|---|
| B | Complete (local implementation) | Reproducible .NET 10/native foundation; deterministic generation; safe value, shared, topology, document, metadata, exchange, modeling, mesh, and visualization profiles; 16,353 emitted plus 61 manual stable IDs; zero supported-unselected/LT001-LT004; complete observed classification; Release/Debug/runtime, 13-file freshness, byte-identical clean regeneration, 62-DLL package consumer, compatibility, provenance/SBOM/checksum, and local release gates passing | None inside Batch B. Signing credentials and NuGet publication remain independent release-readiness gates |
| C — Common CAD API Expansion | Active; implementation not started | ADR-0060 fixes the priority, large-wave sizing, common workflow matrix, and validation cadence | Deliver the complete high-frequency model/inspect, build/modify/deliver, and present/interact workflow contract below; pass focused evidence per family and one complete validation chain per large wave |

Current B completion is not represented by counting retired planning labels. Engineering
progress, selected binding coverage, full-profile coverage, inventory completeness,
validation coverage, and publication readiness are reported independently. B is 100%
because every local exit condition above now has evidence; this does not make the public
package release-ready. New implementation progress is reported against C, never by
reopening B or inventing B-derived labels.

The 16,353 emitted plus 61 accepted manual stable IDs are Batch B's baseline binding
coverage, not evidence that Batch C's workflows are complete. Batch C common-workflow
coverage remains `NOT ESTABLISHED` until the current public/generated surface is reconciled
against the workflow matrix below and a declared-workflow denominator is recorded.

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

## Batch C: Common CAD API Expansion

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

### First large implementation wave

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

Exact declarations and generated/manual ownership are selected from the pinned AST and
current public API inventory before code changes. Scope selection must favor generalized
rules that unlock several listed families. It must not shrink to a single easy class.

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

- Every declared common workflow above has an intentional public/raw boundary and no
  routine step requires an undocumented native escape hatch.
- Every selected common family is emitted or accepted manual with an explicit ownership,
  error, lifetime, threading, and data-transfer contract.
- End-to-end tests cover create/import, inspect, edit, validate/measure, mesh, metadata,
  export, display, and selection with representative real CAD files.
- Release and Debug, deterministic regeneration, clean package consumer, API compatibility,
  runtime manifest, inventory accounting, provenance, and documentation gates pass.
- Remaining blocked/cold APIs keep narrow dispositions but do not hide an unfinished
  high-frequency workflow.

## Generated output partitioning

Generated paths use module directories before project splitting:

```text
src/OcctSharp.Native/generated/<Module>/
src/OcctSharp/Generated/<Module>/
```

The manifest remains the only owner of generated paths. Moving a generated file between
modules is a generator change followed by regeneration, never a manual source move.

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

## Required evidence for C and each large work wave

- Deterministic discovery, generated source, coverage, and diagnostics.
- No unclassified declaration inside the selected common workflow closure.
- Native and managed Debug/Release builds.
- Focused ABI, runtime, ownership, failure, and disposal tests.
- Generated-source freshness and `git diff --check`.
- Clean NuGet consumer whenever public/runtime assets change.
- Updated `STATUS.md` and this plan; only factually affected topic documents and ADRs.

## Batch C execution boundary

1. Keep completed Batch B evidence immutable and report new implementation against C.
2. ADR-0059 resolves the MIT project license and bundled third-party notice layout;
   keep those files and the runtime manifest current for every distribution change.
3. Package signing, credentials, and NuGet publication require explicit authorization.
4. C is one common-API product batch. Its coverage lanes and large waves are not batches,
   version numbers, completion percentages, or permission to stop after a partial lane.

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

## Product and project structure

### Current structure

Keep one managed project, one native bridge, one NuGet package, and one application-local
`occt` runtime directory through the topology and basic modeling batches. Generated files
are organized by module immediately, even while they compile into the same assembly.

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

## Binding batches

Each batch uses the same sequence: inventory, semantic rules, generation, compile,
runtime/lifetime tests, package consumer, documentation, and coverage update.

| Batch | Status | Scope | Primary outcomes | Exit criteria |
|---|---|---|---|---|
| B00 | Complete | Repository and toolchain | .NET 10, C ABI, deterministic generator, local package | Validated baseline |
| B01 | Complete | Scalars and small values | Integers, reals, booleans, enums, `gp_Pnt`, configured static methods | Complete for selected declarations |
| B02 | Complete foundation | Shared transient foundation | `Handle<T>`, intrusive retention, RTTI, checked casts, first typed class | First generated class complete; broad hierarchy continues in B07 |
| B03 | Complete | Topology value foundation | `TopoDS_Shape` copy/null/type/orientation/reversal/equality semantics | Debug/Release, lifetime, freshness, and package consumer passed |
| B04 | Complete | Typed topology hierarchy | Compound, CompSolid, Solid, Shell, Face, Wire, Edge, Vertex; checked conversions | Eight casts validate `ShapeType`; no layout crossing; Debug/Release/package passed |
| B05 | Complete | Location and transformation values | `gp_Trsf`, `TopLoc_Location`, `gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` opaque value contracts plus transform conversion | Debug/Release, lifetime, freshness, and alpha.11 package gates passed |
| B06 | Complete | Foundation collections and strings | UTF-8/UTF-16 OCCT strings, scalar sequences/arrays/vectors, integer-key maps, and caller-owned snapshot enumeration | Debug/Release, stale-handle, double-dispose, empty, mutation, snapshot, and package gates |
| B07 | Complete (immutable profile) | Geometry primitives | `gp_Pnt`, `gp_XYZ`, `gp_Lin`, `gp_Circ`, `gp_Ax2`, `gp_Pln`, and `gp_Ax3` immutable value families | Debug/Release deterministic values, validation, geometry math, and package gates passed |
| B08 | Complete (safe value/owner profile) | Adaptors and properties | `GProp_GProps`/`BRepGProp` owned property accumulator; copied BRepAdaptor edge curve and face surface snapshots | Debug/Release numerics, layout, wrong-kind, disposal, freshness, and package gates passed; borrowed adaptor views remain excluded |
| B09 | Complete (basic construction profile) | BRep construction | Box/sphere/cylinder solids plus straight edge, polygon wire, and planar face owning builders | Debug/Release topology, invalid input/type, lifetime, and package gates passed |
| B10 | Complete (owning snapshot profile) | Topology traversal | Owning face/edge/wire/vertex `TopExp_Explorer` snapshots and child-kind maps | Parent/copy lifetime, empty/invalid kind, cleanup, and package gates passed |
| B11 | Complete (basic result profile) | Modeling algorithms | Owning Fuse/Common results plus copied `BRepExtrema_DistShapeShape` minimum-distance values | Release/Debug completion/failure, layout, null/disposed, source independence, freshness, and package gates passed |
| B12 | Complete (owning-result/no-history profile) | Boolean and healing | Owning Cut, ShapeFix, and same-domain-unification results with deterministic null rejection; BOP/ShapeFix/ShapeUpgrade history stays native-local | Release/Debug null/disposed/result lifetime plus alpha.34 package family checks passed; advanced history/modes remain later profiles |
| B13 | Complete (first bulk wave) | Mesh and bulk transfer | `BRepMesh_IncrementalMesh`, copied triangle vertices/normals/indices | Release/Debug, invalid-parameter, capacity, and package gates passed; Poly/RWMesh and benchmark profile remain pending |
| B14 | Complete (geometry-exchange profile) | Basic data exchange | STEP/IGES/STL geometry loops; OBJ/GLTF/VRML read/write; PLY write; existing STEPCAF/XDE assembly path | Release/Debug 65/65, freshness, alpha.35 clean consumer, 41-DLL closure; PLY read is upstream-unsupported, generated provider/options/metadata continue in B16/B19 |
| B15 | Complete (document/label profile) | OCAF document model | Owning TDocStd application/document, stable-entry parent-bound TDF labels, TDataStd names, transactions, BinOcaf persistence | Release/Debug 66/66, parent disposal, abort semantics, persistence, freshness, alpha.36 clean consumer, 43-DLL closure |
| B16 | Complete (metadata/assembly profile) | XDE and metadata | Parent-bound XDE labels; shapes, assemblies, occurrences, locations; copied names/RGBA/layers/materials; BinXCAF and STEPCAF | Release/Debug 67/67, memory/BinXCAF/STEPCAF round-trip, freshness, alpha.37 consumer, 44-DLL closure |
| B17 | Complete (Windows core profile) | Visualization core | HWND-bound Aspect/OpenGl/V3d/AIS owner, parent-bound presentations, explicit input forwarding, copied selection IDs | Release/Debug 68/68, real HWND display/selection/thread tests, freshness, interactive sample, alpha.38 consumer, 45-DLL closure |
| B18 | Complete (dependency-profile classification) | Optional integrations | Versioned audit for IVtk/VTK, OpenGL ES, Draw/test, WNT, Cocoa/X11, and C++/CLI; isolated future package boundaries | 6/6 profiles classified; Windows viewer available, named optional dependencies blocked/excluded without entering core; Release/Debug audit passes |
| B19 | In progress (B19.1-B19.3 complete) | Long-tail and templates | Classification is complete; B19.3 reconciles 333 emitted plus 18 accepted manual declarations, while 10,177 `SupportedUnselected` declarations and LT001-LT004 projection/ownership work remain | Close only when every bindable declaration is emitted or accepted manual and no safety-critical unknown projection remains |
| B20 | In progress (release engineering implemented) | Upgrade and release | 606-signature API baseline/diff, immutable-artifact CI, clean regeneration, notices, SBOM/provenance/checksums, explicit gates | Release tooling passes locally, but bindable-emission, license/notice, hosted CI, signing, and publication-scope gates remain open |

Current batch progress is 19 of 21 complete (90.5%). This measures execution batches,
not OCCT declaration coverage or public-release readiness.

### B19 emitted-binding sub-batches

| Sub-batch | Status | Scope | Evidence |
|---|---|---|---|
| B19.1 | Complete | Ten StepBasic scalar/shared entities plus typed enum emission and manifest-aware inventory reconciliation | 171/3,406 selected emitted; Release/Debug Generator 40/40 and Runtime 73/73; alpha.39 45-DLL clean consumer; 13-file freshness |
| B19.2 | Complete | StepBasic package-level default-constructible shared-entity closure | 333/5,503 selected emitted; 129 public generated StepBasic types; Release/Debug Generator 41/41 and Runtime 75/75; alpha.40 45-DLL clean consumer; 13-file freshness |
| B19.3 | Complete | High-frequency common modeling and topology operations | Cone/torus, extrusion/revolution, all/single-edge fillet/chamfer, offset, section, bounds, validity/count; 18 schema-1.6 Manual IDs; Release/Debug 44/44 + 81/81; alpha.41 47-DLL consumer |
| B19.4 | Next | High-frequency geometry/curve and advanced topology closure | Select a coherent Geom/Geom2d construction/evaluation/projection plus topology-map/history snapshot ownership family before low-value data entities |

## Batch sizing

Work is selected by dependency closure, not raw header count. Batches are intentionally
coarse: one batch should complete a coherent ownership family, several closely related
OCCT packages, or roughly 100–500 emitted public declarations. We only split when a
failure would otherwise hide the owning rule or make validation inseparable. B05 is the
model for this sizing: all transformation/value families were completed together behind
one ABI contract. Every batch records its source packages, toolkits, entry headers,
emitted stable IDs, test matrix, package impact, and next blocked rule.

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

## Per-batch required evidence

- Deterministic discovery, generated source, coverage, and diagnostics.
- No unclassified public declaration in the selected batch.
- Native and managed Debug/Release builds.
- Focused ABI, runtime, ownership, failure, and disposal tests.
- Generated-source freshness and `git diff --check`.
- Clean NuGet consumer whenever public/runtime assets change.
- Updated `STATUS.md`, this plan, topic documents, and ADRs for semantic changes.

## Immediate execution order

1. Execute B19.4 as a high-frequency Geom/Geom2d construction, evaluation, projection,
   and advanced topology-map/history snapshot closure.
2. Replace LT001-LT004 broad blockers with implemented enum/value/handle/borrowed-view
   projection rules or narrow evidence-backed unsupported reasons.
3. Re-run B20 evidence after B19 completion, then resolve license/notices/hosted-CI and
   signing/publication scope gates without publishing absent explicit authority.

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
| B06 | In progress | Foundation collections and strings | UTF-8/UTF-16 OCCT strings, `NCollection_Sequence<double>`, `NCollection_Array1<double>`, and the OCCT 8 dynamic-array-backed `NCollection_Vector<double>` alias; maps, richer elements, and iterators remain | B06-wide encoding, index/null, collection mutation, and iterator ownership gates |
| B07 | Planned | Geometry primitives | Broad `gp_*`, `Geom_*`, `Geom2d_*` values and shared curves/surfaces | Inheritance casts and representative evaluation tests |
| B08 | Planned | Adaptors and properties | Adaptor2d/3d, GeomAdaptor, BRepAdaptor, GProp | Borrowed/parent-bound rules and numerical tests |
| B09 | Planned | BRep construction | BRepBuilderAPI, BRepPrimAPI, Make* builders | Generated box/wire/face/solid closed loop replaces raw bridge |
| B10 | Planned | Topology traversal | TopExp, explorers, maps, child iteration | Parent/copy lifetime and early-exit stress tests |
| B11 | Planned | Modeling algorithms | intersections, projections, extrema, offsets, fillets, features | Failure/status mapping and representative CAD fixtures |
| B12 | Planned | Boolean and healing | BOPAlgo, BRepAlgoAPI, ShapeFix, ShapeUpgrade | History/result ownership and invalid-input tests |
| B13 | Planned | Mesh and bulk transfer | Poly, BRepMesh, RWMesh buffers | Bulk vertex/index/normal ABI and benchmarks |
| B14 | Planned | Basic data exchange | STEP/IGES/STL/OBJ/PLY/GLTF/VRML readers and writers | Generated APIs replace manual geometry exchange where equivalent |
| B15 | Planned | OCAF document model | TDF, TDataStd, TDocStd, persistence | Document/label parent-bound lifetime and transaction tests |
| B16 | Planned | XDE and metadata | XCAF, STEPCAF, colors, names, layers, materials, assemblies | Generated metadata round-trip matches one-shot bridge evidence |
| B17 | Planned | Visualization core | Aspect, Graphic3d, Prs3d, AIS, selection, V3d | Thread/window/callback ADRs and interactive smoke app |
| B18 | Planned | Optional integrations | IVtk, OpenGL ES, Draw/test toolkits, platform-specific adapters | Separate profiles/packages and dependency-provided builds |
| B19 | Planned | Long-tail and templates | Remaining packages, explicit template instantiations, special cases | 100% classification and no unowned public blocker |
| B20 | Planned | Upgrade and release | API baselines, OCCT version diff, CI, notices, SBOM, signing | Clean machine regeneration and approved publication |

Current batch progress is 6 of 21 complete (28.6%). This measures execution batches,
not OCCT declaration coverage or public-release readiness.

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

1. Continue B06 strings/collections with explicit encoding and ownership rules; add
   safe arrays, maps, and iterator contracts to this same coarse batch.
2. Begin B07 geometry alongside B06 only after their ownership rules
   stop unknown containers from leaking into generated signatures.

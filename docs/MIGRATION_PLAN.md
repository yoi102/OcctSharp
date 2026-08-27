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

## Single migration batch B

The current program has exactly one batch: `B`. The former B00-B20 and dotted Bxx.y
labels are retired planning labels, not current batches or commit boundaries.
Repository/toolchain, generator, ownership, foundation, modeling, mesh, exchange, XDE,
visualization, long-tail generation, upgrade, and release engineering all belong to B.
If a later product program truly requires another batch, it uses the next whole letter
and the same product-scale sizing; numbered or dotted batch fragments are forbidden.

| Batch | Status | Completed evidence | Remaining exit conditions |
|---|---|---|---|
| B | Complete (local implementation) | Reproducible .NET 10/native foundation; deterministic generation; safe value, shared, topology, document, metadata, exchange, modeling, mesh, and visualization profiles; 16,353 emitted plus 61 manual stable IDs; zero supported-unselected/LT001-LT004; complete observed classification; Release/Debug/runtime, 13-file freshness, byte-identical clean regeneration, 62-DLL package consumer, compatibility, provenance/SBOM/checksum, and local release gates passing | None inside Batch B. Public license/notice, hosted CI, signing, credentials, and publication remain independent release-readiness gates |

Current B completion is not represented by counting retired planning labels. Engineering
progress, selected binding coverage, full-profile coverage, inventory completeness,
validation coverage, and publication readiness are reported independently. B is 100%
because every local exit condition above now has evidence; this does not make the public
package release-ready.

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

### Work sizing inside B

Work is selected by dependency closure, not raw header count. Each implementation wave
combines as many related packages, API families, and user workflows as can share a
truthful ownership contract and validation matrix. Tiny convenience methods are folded
into the active wave. Different lifetime categories or optional external dependencies
may be implemented and validated separately, but they do not create another batch.
Every material wave records source packages/toolkits, stable IDs, tests, package impact,
coverage change, and the next large workstream before work continues.

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

## Required evidence for B and each material work wave

- Deterministic discovery, generated source, coverage, and diagnostics.
- No unclassified public declaration in the selected batch.
- Native and managed Debug/Release builds.
- Focused ABI, runtime, ownership, failure, and disposal tests.
- Generated-source freshness and `git diff --check`.
- Clean NuGet consumer whenever public/runtime assets change.
- Updated `STATUS.md` and this plan; only factually affected topic documents and ADRs.

## Post-B execution boundary

1. Keep the completed local Batch B result separate from public release readiness; do
   not convert legal, hosted-CI, signing, or publication gates to PASS.
2. Before any public release, project owners must select the project license and complete
   the non-OCCT third-party legal/notice review.
3. Hosted CI, package signing, credentials, and NuGet publication require explicit
   authorization and remain `NOT RUN`.
4. Any future product-scale migration uses the next whole-letter batch only when its
   scope is comparable; narrow API additions do not create dotted batches.

# Batch V: Exact partition, material regions and volume construction

- Status: scope prepared, implementation **0/40**. New API compile/runtime: **NOT RUN**.
- Decision: [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md).
- Preparation commit: `eacd0ed`; product baseline remains Preview.15 / OCCT 8.0.1.
- Planned local package slot: `8.0.1-preview.21`; current versions are unchanged.
- Frozen roots: [batch-v-partition-volume-workflows.json](../OcctSharp/config/batches/batch-v-partition-volume-workflows.json).
- Shared preparation: [U-W evidence](BATCH_U_W_PREPARATION.md).
- Delivery: [continuous Q-W runbook](BATCH_CONTINUOUS_EXECUTION.md); one complete batch per local commit.

## Product outcome and reuse boundary

J already implements SelectBooleanCells with one take/avoid/material request, multi-argument Boolean/split, robust options and history; Q prepares normalization/repair and L already provides assembly collision/clearance. V adds inspectable full partitions, ordered multi-material region programs, MakerVolume solid construction and assembly region provenance. None of those existing one-shot operations, repair helpers or collision counts is counted again.

Located assembly inputs -> full partition/membership snapshot -> material selection program -> shared/external interfaces -> MakerVolume and user-bounded voids -> T atomic region outputs -> R grouped mesh and XDE exchange/viewer review.

All 40 rows are one delivery unit. A row is a newly observable workflow, not a getter,
test, overload or standalone family checkpoint. Acceptance below is future required
evidence, not a claim of implementation. Existing lower generated wrappers are reused
where ownership permits; candidate root membership alone does not mean a missing API.

## Frozen capability matrix

| ID | Root group | New capability | Required acceptance |
|---|---|---|---|
| V-01 | Partition | Full unselected partition snapshot | Expose all split parts by dimension before any result selection, unlike J selected-result-only Cells helper. |
| V-02 | Partition | Partition input-membership map | Copy which original argument regions each part belongs to, with unknown/ambiguous membership explicitly represented. |
| V-03 | Partition | Revision-scoped cell identity | Bind selectable cell IDs to an immutable input/partition revision and reject IDs from another partition. |
| V-04 | Partition | Ordered multi-rule region selection | Execute multiple take/avoid add/remove rules in one native-local partition build and report rule-to-cell effects. |
| V-05 | Partition | Boolean-expression region programs | Compile a finite union/intersection/difference expression over input memberships into bounded selection rules, without evaluating user code. |
| V-06 | Partition | Disjoint multi-material cell assignment | Assign one integer region material per cell and reject conflicting assignments; separate region labels from XDE visual materials. |
| V-07 | Partition | Deferred internal-boundary removal | Apply removal only after the full compatible selection program; preserve material-zero boundaries and report failures. |
| V-08 | Partition | Protected inter-material interfaces | Retain interfaces between different region materials and expose owning boundary groups for downstream use. |
| V-09 | Partition | Typed container assembly | Build wires/shells/compsolids from finalized selected cells according to dimension and validate returned container composition. |
| V-10 | Partition | Region-program reselection | Rebuild a new native-local partition from a copied revised recipe; no hidden long-lived PaveFiller/CellsBuilder owner. |
| V-11 | Partition | Shared face and edge interface graph | Expose topological adjacency and oriented boundary uses between partition cells, not merely L geometric proximity. |
| V-12 | Partition | External region envelope extraction | Return boundary faces/edges used by only one selected region with explicit material and input provenance. |
| V-13 | Partition | Connected material-region extraction | Group connected selected cells into owning regions without merging different materials or dimensions. |
| V-14 | Partition | Per-region geometric measures | Compute dimension-appropriate measures and conservation checks for selected/unselected cells; surface area is not solid volume. |
| V-15 | Partition | Sliver-region selection policy | Identify/select small cells using explicit absolute/relative measures; removal is a requested modeling operation, never automatic repair. |
| V-16 | Partition | Mixed-dimensional partition results | Retain vertex/edge/face/solid components and report unsupported cross-dimensional internal-boundary removal. |
| V-17 | Partition | Batch region outputs from one partition build | Produce several independent output region sets plus correspondence in one call, with consistent cleanup if any required output fails. |
| V-18 | Validate | Partition argument-failure provenance | Map unsupported/invalid/self-interfering input combinations to copied argument/subshape diagnostics before selection. |
| V-19 | Validate | Partition precision acceptance policy | Measure validity/conservation/topology growth under chosen fuzzy settings; do not silently retry with larger tolerance. |
| V-20 | Partition | Unmapped-history disclosure | Return supported vertex/edge/face Modified/IsDeleted history and explicit unavailable solid lineage rather than fabricating mappings. |
| V-21 | Volume | Volume construction from intersecting face sets | Use MakerVolume to intersect eligible source shapes and return zero/one/many owning solids with clear cardinality. |
| V-22 | Volume | Pre-intersected volume construction mode | Permit SetIntersect(false) only with explicit verified non-interference preconditions; otherwise reject the fast path. |
| V-23 | Volume | Closed shell grouping from face sets | Use supported shell-splitting/solid-building stages to return connected closed candidates and unresolved face groups. |
| V-24 | Volume | Multiple-volume source-face correspondence | Map returned bounded volumes to their input/image faces and distinguish unassigned construction helpers. |
| V-25 | Volume | Nested-shell and cavity classification | Classify eligible nested shells and cavity orientation; ambiguity or invalid containment is reported, not assigned by volume sign alone. |
| V-26 | Volume | Internal vertex and edge inclusion policy | Configure MakerVolume internal-shape avoidance/inclusion and report retained internal topology independently of external boundaries. |
| V-27 | Volume | Open-boundary volume-build diagnostics | Return unresolved free boundaries and unused faces when no closed solid is produced, without claiming general hole filling. |
| V-28 | Volume | Construction-box exclusion evidence | Ensure MakerVolume helper bounding-box topology is not published as a product region and verify expected finite volumes. |
| V-29 | Volume | User-bounded void region construction | Build cavities/voids relative to an explicit owning envelope; no unbounded-space solid is invented. |
| V-30 | Volume | Selective volume extraction by geometric query | Select bounded volumes using point classification and explicit containment policy with on-boundary/unknown outcomes. |
| V-31 | Volume | Solid/compsolid adjacency delivery | Expose shared-face solids as compsolid groups where valid and preserve separate bodies otherwise. |
| V-32 | Integration | Repair-to-volume controlled workflow | Accept Q repaired face/shell sets into volume construction and compose supported provenance without in-place source mutation. |
| V-33 | Integration | Occurrence-aware assembly partition inputs | Resolve repeated XDE definitions into located independent partition inputs with occurrence-path provenance. |
| V-34 | Integration | Instance versus definition material-region policy | Choose whether identical definitions share region rules or are processed per located occurrence; never silently collapse instances. |
| V-35 | Integration | Region-to-XDE product authoring | Create output region products with explicit part/material keys, colors and unambiguous source metadata mapping. |
| V-36 | Integration | Partition and volume parametric features | Extend T built-ins with copied region/volume recipes and atomic multi-output publication. |
| V-37 | Integration | Partition reevaluation after placement edit | Recompute affected region recipes after existing assembly placement changes; scope cell IDs to the new revision. |
| V-38 | Integration | Region meshes with interface provenance | Feed R authored mesh contracts from selected exact regions while retaining region/interface IDs through triangle grouping. |
| V-39 | Integration | Multi-region exact STEP/IGES delivery | Export/reopen supported separate volumes and metadata; preserve region semantics in application/OCAF data when a format cannot encode them. |
| V-40 | Integration | Cell/interface/void review workflow | Select cells and shared interfaces, isolate voids in the user envelope and compare accepted outputs in the existing real-HWND viewer. |

## Root, dependency and source closure

| Root group | Exact decision roots |
|---|---|
| Partition | `BOPAlgo_CellsBuilder`, `BOPAlgo_PaveFiller`, `BOPAlgo_Builder`, `BOPAlgo_Section`, `BOPAlgo_Tools`, `BRepAlgoAPI_BuilderAlgo`, `BRepAlgoAPI_Splitter`, `BOPAlgo_BOP` |
| Volume | `BOPAlgo_MakerVolume`, `BOPAlgo_BuilderSolid`, `BOPAlgo_ShellSplitter`, `BOPAlgo_BuilderFace` |
| Validate | `BOPAlgo_ArgumentAnalyzer`, `BOPAlgo_CheckerSI`, `BRepAlgoAPI_Check`, `BRepClass3d_SolidClassifier`, `BRepClass_FaceClassifier` |

The 17 decision roots reuse 26 support roots:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `BRepTools_History`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `BRepTools`, `TopExp_Explorer`, `BRepTools_WireExplorer`, `BRepBuilderAPI_MakeSolid`, `BRepBuilderAPI_Sewing`, `Bnd_Box`, `BRepBndLib`, `ShapeUpgrade_UnifySameDomain`.

Integration rows reuse the established or explicitly prepared public workflows rather
than requiring a second copy of their native bindings. Q repair/protected-result policies, R mesh provenance and T multi-output parametric execution are required integration prerequisites. S/U results are valid modeling inputs but V does not depend on every U algorithm.
Delivery order alone is not an algorithm dependency. Every prerequisite must actually
pass its whole-batch gates before V starts.

Native: Modeling/PartitionRegions.cpp, Modeling/VolumeConstruction.cpp and Modeling/RegionInspection.cpp with small private DTO/history helpers. Do not enlarge the existing Features.cpp switch. Managed geometry contracts belong in Modeling; recipe/assembly/mesh orchestration in existing higher owners/facade; Documents never depends on XDE/Mesh.

Private headers stay acyclic and domain-owned. Native builders, iterators and temporary
arrays are local to a call. Recipes and diagnostic/index/history records are copied.
Owning topology reuses the current registry/release family and survives its inputs.
Document labels remain parent-bound; viewer objects remain parent-bound/thread-affine.
W light/texture IDs and U/V preview/result owners require concrete ownership and cleanup
evidence during implementation, not new independent registries. Do not expose native
session/GPU pointers. No new ownership category or binary split is created by preparation.

## OCCT limitations and non-goals

CellsBuilder material 0 retains boundaries; one part cannot have conflicting material assignments. Internal-boundary removal between different dimensions is not supported. MakeContainers is a finalization operation: later additions do not automatically refresh existing containers. Native history explicitly supports basic vertex/edge/face types, not guaranteed solid identity. MakerVolume with SetIntersect(false) requires non-interfering arguments, otherwise OCCT documents unpredictable output. No FEM solver, tetrahedral mesher, mesh Boolean engine, arbitrary native session or automatic tolerance escalation.

## Preparation evidence and baseline freshness

Inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 43 | 2075 | 803 | 414 | 120 | 738 |

Root report SHA256:
`D87D3206B4FD9F14E71D857D18077D7B53902EE45D6FF1FBEDA7B19A8DEF0AFF`.
Reports are regenerated by the existing exact-root auditor; the shared verifier checks
repeat determinism, baseline/input protection, SDK headers and representative toolkit
exports. Final executed results are in STATUS and the shared U-W preparation record.

Candidates include reused emitted/manual and non-callable or blocked declarations;
they are not 40 capabilities, expected new public methods or a complete bindable
denominator. Do not relabel all candidates Manual. SDK header/export availability
is not a new compile/link, lifetime or driver-success test.

The root scope is prepared on Preview.15, not on hypothetical completed Q-T/U/V code.
Before implementation, record the previous/new inventory commit/hash and exact
added/removed/reclassified IDs, callable signature and source/dependency changes.
Reaudit impacted rows and reuse newly completed prerequisites. Keep the 40-row product
denominator; a changed or unsupported outcome needs an explicit decision, not silent
deletion or replacement with an extra test.

## Whole-batch acceptance

The [shared implementation gates](BATCH_Q_T_PREPARATION.md) apply without reduction:
40-row assertion mapping; Release/Debug build and Generator/Runtime regression, including
actual Debug-native runtime; source/ownership/dependency closure; precise manual-ID and
ABI reconciliation; applicable real STEP/IGES/XDE/HWND workflows; clean regeneration,
compatibility, committed runtime notices/manifest, both clean consumers and complete
local pack/release checks; documentation and one local completion commit.

Use deterministic geometric/structural assertions and negative/foreign/disposed/stale
identity tests. For W, supported rendering paths need actual driver evidence; capability
rejection alone cannot stand in for all success-path tests. GPU-dependent pixel tests
use documented tolerance/invariants, not universal byte-identical screenshots.

After a verified completion commit, the authorized continuous run automatically
revalidates the next queued batch and continues without a new routine confirmation.
A compile/test failure stays in this batch for repair; it is not permission to skip
a gate. No NuGet publication, GitHub push, signing or unattended scheduler is created.

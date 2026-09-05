# Batch Q: Shape repair and topology normalization

- Status: **40/40 implemented and locally validated**, delivered as one whole-batch completion commit.
- Decision: [ADR-0082](adr/0082-broad-batch-q-through-t-preparation.md).
- Preparation baseline: commit `6b04bd9`, package `8.0.1-preview.15`, OCCT 8.0.1.
- Local package: `8.0.1-preview.16`, ABI 1.60, bridge 0.68.0, schema 1.13; not published.
- Execution contract and shared gates: [Q-T preparation](BATCH_Q_T_PREPARATION.md).
- Frozen configuration: [batch-q-shape-repair-topology.json](../OcctSharp/config/batches/batch-q-shape-repair-topology.json).

## Product outcome and existing coverage

`Shape.RepairWithReport`, `ShapeFactory.Sew`, `FreeformAuthoring.Heal`, Batch J preflight/repair/retry and basic history, and Batch P pcurve/SameParameter repair already exist. They are dependencies, not new Q capabilities.

Imported multi-solid model -> diagnose gaps/tolerances -> selected protected repair
plan -> sewing/division/unification on copies -> composed history and budget verdict ->
atomic XDE replacement -> STEP/IGES reopen -> viewer defect navigation. Fixtures must
cover a gapped wire, bad shell orientation, a small hole, a periodic face, repeated
assembly occurrences and an over-budget repair.

One row below is one independently observable workflow capability, not one getter,
native class, test case or commit. All 40 rows plus their dependencies and validation
form one batch. Internal source groups are implementation responsibilities, not smaller
delivery batches. The `Integration`/`Execution` groups compose the audited roots and
existing public operations; they do not imply new OCCT class roots.

## Frozen capability and acceptance matrix

The original acceptance statements below remain unchanged. Their implementation/test
mapping follows the matrix. Focused tests and all required whole-batch local gates pass.
Shared lifetime/negative/package gates are not extra capabilities.

| ID | Root group | New capability | Required observable acceptance |
|---|---|---|---|
| Q-01 | Diagnosis | Copied defect inventory with topology provenance | Identify wire, face, shell and solid defects on a deliberately broken compound; report scoped source indices, not pointers. |
| Q-02 | Diagnosis | Tolerance distribution and offending subshape selection | Return per-kind ranges and outliers in document units; distinguish inherited tolerance from measured geometric gaps. |
| Q-03 | Diagnosis | Wire connectivity and ordering diagnosis | Report disconnected chains, necessary reversals and start/end connectivity without modifying the input. |
| Q-04 | Diagnosis | Wire intersection and degeneracy diagnosis | Locate conflicting edge pairs and degenerate segments with copied evidence; distinguish unsupported checks from a clean result. |
| Q-05 | Diagnosis | Free-boundary extraction as owning wires | Separate open chains from closed loops and map their edges to the source; disposing the source preserves extracted wires. |
| Q-06 | Diagnosis | Free-boundary measurements and closure candidates | Measure contour length/area where defined and endpoint gaps; do not claim a nonplanar contour has a unique planar area. |
| Q-07 | Diagnosis | Shell connectivity and orientation defect map | Identify disconnected shells and incorrectly oriented edge uses with stable-in-snapshot provenance. |
| Q-08 | Diagnosis | Small-face and thin-face classification | Separate small area, strip and singular-face findings using explicit length/area thresholds. |
| Q-09 | Repair | Typed multi-stage repair plan | Freeze ordered stages, explicit auto/on/off controls and effective tolerance policy; reject contradictory stages before execution. |
| Q-10 | Repair | Repair restricted to selected topology | Resolve a selection against its source revision and alter only that closure; reject foreign or stale selections. |
| Q-11 | Repair | Protected-subshape repair policy | Preserve named protected boundaries or reject a stage whose result would remove them; never silently override the protection. |
| Q-12 | Repair | Bounded geometric-change policy | Limit tolerance growth and measured area/volume drift; mark unavailable checks and reject unverified required budgets. |
| Q-13 | Repair | Controlled wire reorder and reconnect | Produce an independently owned ordered wire with before/after edge correspondence, including reversed input edges. |
| Q-14 | Repair | Adjacent vertex-gap repair | Join eligible adjacent endpoints within the declared budget and report merges; leave over-budget endpoints unchanged or fail atomically. |
| Q-15 | Repair | Wireframe 2D/3D gap correction | Repair selected wireframe gaps with separate status and measured residuals; retain source geometry on failure. |
| Q-16 | Repair | Small-edge elimination with corner protection | Remove eligible small edges while preserving protected corners and report deleted/merged topology. |
| Q-17 | Repair | Face boundary and orientation normalization | Apply selected face-fixer modes, distinguish natural-bound completion from orientation correction and preserve holes unless requested. |
| Q-18 | Repair | Shell orientation normalization | Correct eligible inconsistent shell orientations and return copied evidence for unrepairable non-manifold shells. |
| Q-19 | Repair | Solid shell normalization | Normalize eligible shell membership/orientation in a solid and recheck enclosed volume and validity. |
| Q-20 | Repair | Small-face removal or replacement | Execute explicitly selected small-face repair with replacement history and reject unacceptable boundary changes. |
| Q-21 | Repair | Small-solid filtering | Remove explicitly eligible small solids by configured geometric thresholds and record every removed solid. |
| Q-22 | Repair | Scoped tolerance normalization | Adjust tolerances only for selected shape kinds within measured admissible bounds; verify rather than blindly reduce tolerances. |
| Q-23 | History | Configurable sewing with input provenance | Sew a face/shell set with explicit non-manifold/tolerance policy and preserve source-to-result correspondence beyond the basic Sew helper. |
| Q-24 | History | Sewing unresolved-boundary review | Return copied free/multiple/contiguous-edge findings and owning affected topology from the sewing result. |
| Q-25 | Normalize | Selective internal-wire removal | Remove only opted-in small holes or selected internal wires; report removed faces and reject protected boundary loss. |
| Q-26 | Normalize | Location normalization | Bake eligible placements into independent topology according to a level policy; preserve world-space geometry and occurrence mapping. |
| Q-27 | Normalize | Parametric-continuity face division | Split at requested supported C0/C1/C2/C3/CN continuity with history; reject G1/G2 as division criteria. |
| Q-28 | Normalize | Angular face subdivision | Apply ShapeDivideAngle with an explicit angular bound and verify result coverage and orientation. |
| Q-29 | Normalize | Area-bounded face subdivision | Apply ShapeDivideArea with area units and bounded output growth; retain the whole input on failure. |
| Q-30 | Normalize | Closed-surface face division | Split eligible periodic/closed faces into the requested supported parts and retain seam provenance. |
| Q-31 | Normalize | Closed-edge division | Divide eligible closed edges while preserving their support and face-loop consistency. |
| Q-32 | Normalize | Protected same-domain unification | Unify eligible adjacent faces/edges while keeping a protected shape set and preserving requested internal-edge policy. |
| Q-33 | History | Explicit topology replacement/removal plan | Apply a typed ReShape edit set to a copied source; validate type compatibility, cycles and conflicting edits before applying. |
| Q-34 | History | Composed multi-stage repair history | Compose modified/generated/deleted relations across all stages; represent unknown mapping explicitly instead of inventing one-to-one identity. |
| Q-35 | Repair | Preview and atomic result acceptance | Expose a disposable preview and its budget verdict; accepting it publishes the complete result, rejecting it retains the original. |
| Q-36 | Repair | Stage-level recovery result | Return failed/skipped/completed stage outcomes plus owning accepted output under an explicit all-or-nothing policy. |
| Q-37 | Integration | Transactional XDE definition repair | Update a shared definition once, preserve occurrence placements and map colors/names where history is unambiguous; report conflicts. |
| Q-38 | Integration | Repair-aware STEP/IGES delivery | Reopen repaired exact topology and compare validity, units, occurrence placement and supported metadata against the accepted preview. |
| Q-39 | Integration | Defect-to-viewer review navigation | Select source/result defects in the existing viewer with copied diagnostic identity; reject stale review selections after replacement. |
| Q-40 | Integration | Portable repair recipe and audit record | Serialize stage options, units, source revision and outcomes without native handles; reproducibly reload the recipe against the matching input. |

## Native decision roots and dependency closure

### Implementation and acceptance evidence

All named tests belong to `BatchQCompletionTests`, spread across
`BatchQCompletionTests.cs`, `BatchQRepairEdgeCaseTests.cs` and
`BatchQRepairContractTests.cs` in `OcctSharp/tests/OcctSharp.Runtime.Tests/`.
The integration fixture is shared verbatim with the clean facade package consumer in
`OcctSharp/tests/Shared/BatchQRepairWorkflow.cs`. The mapping is mechanically checked:
all 40 rows appear exactly once and all 25 named tests exist. Each row inherits the
complete shared local gates below; no row is accepted from a test count alone.

| Rows | Executable evidence | Checked outcome |
|---|---|---|
| 01, 02 | `SnapshotOwnsTopologyAndCopiedToleranceProvenance`; `BrokenWireShellDegeneracyAndThinFacesHaveScopedFindings` | Copied topology IDs, per-kind tolerance statistics and scoped defects; original disposal is safe. |
| 03, 04, 07, 08 | `BrokenWireShellDegeneracyAndThinFacesHaveScopedFindings`; `IntersectionsOpenChainsAndNonplanarBoundariesRemainExplicit`; `ReversedWireEdgesAndNaturalFaceBoundsHaveDistinctRepairControls` | Broken chains/shells, reversed edges, intersecting edge pairs, sphere degeneracies and strip/small/singular faces are distinguished. |
| 05, 06 | `FreeBoundariesMeasurePlanarLoopsAndSurviveSourceDisposal`; `IntersectionsOpenChainsAndNonplanarBoundariesRemainExplicit` | Owning closed/open wires, source edge maps, planar areas 4/100, length 48 and explicit nonplanar-area unavailability. |
| 09, 10 | `AtomicPreviewComposesHistoryAndRejectsForeignAndUnverifiedInputs`; `InvalidPortableRecordsAndContradictoryControlsAreRejected`; `SelectedSolidFilteringDoesNotTouchSiblingAndReportsRemoval` | Immutable controls, foreign/stale/cyclic/conflicting inputs and selected-only modifications. |
| 11, 12 | `HoleRemovalBudgetsAndProtectedWireAreAtomic`; `OverBudgetGapsAndProtectedSmallEdgesRejectAtomically` | Protected holes/edges reject atomically; area drift, tolerance and unavailable volume cannot bypass acceptance. |
| 13, 14, 15 | `WireRepairsConnectWithinBudgetWithoutMutatingTheDamagedSource`; `ReversedWireEdgesAndNaturalFaceBoundsHaveDistinctRepairControls`; `OverBudgetGapsAndProtectedSmallEdgesRejectAtomically` | Reorder/reconnect/gap correction, four result vertices and explicit many-to-one vertex history; source fingerprint unchanged and large gaps rejected. Actual 0.0001 UV gap measured before and below 1e-7 after repair; result-bound edge pairs and missing-support unavailability are verified. |
| 16, 22 | `SmallEdgesAndScopedTolerancesHaveVerifiedResults`; `OverBudgetGapsAndProtectedSmallEdgesRejectAtomically` | Real small-edge reduction with unchanged protected corner and changed/deleted edge history; selected vertex tolerance changes without sibling changes. |
| 17 | `ReversedWireEdgesAndNaturalFaceBoundsHaveDistinctRepairControls` | Natural-bound completion of a finite unbounded Bezier face, separate orientation-only control retaining both hole wires. |
| 18, 19 | `BrokenShellAndSolidOrientationsCanBeNormalized`; `NonmanifoldSewingReportsMultipleEdgesAndContinuitySplitsRealKinks` | Broken face-use orientation repaired; shell coherence and bounded-solid orientation distinguished; nonmanifold boundaries explicitly diagnosed. |
| 20 | `SelectedSmallFaceRepairRemovesOnlyTheEligibleFace` | Selected spot removed, sibling face/area retained, deleted history and protected-face rejection. |
| 21 | `SelectedSolidFilteringDoesNotTouchSiblingAndReportsRemoval` | Small solid removed explicitly, larger sibling unchanged and each removed solid mapped. |
| 23, 24 | `SewingReturnsBoundaryReviewAndExplicitEditsComposeDeletions`; `SewingContiguousEdgesAndProtectedUnificationHaveActualTopologyChanges`; `NonmanifoldSewingReportsMultipleEdgesAndContinuitySplitsRealKinks` | Actual sewing, free/multiple/contiguous boundaries and owning copies with provenance. |
| 25, 26 | `SelectedHoleRemovalAndLocationBakingPreserveTheirScopes`; `HoleRemovalBudgetsAndProtectedWireAreAtomic` | Selected hole removed; placed solid retains world bounds/volume while serialized location count becomes zero and history records changes. |
| 27 | `AllParametricContinuityModesAndClosedEdgesAreChecked`; `NonmanifoldSewingReportsMultipleEdgesAndContinuitySplitsRealKinks` | All C0/C1/C2/C3/CN modes, G1/G2 rejection, real C0 kink split into two faces with preserved area. |
| 28, 29, 30, 31 | `DivisionPreservesAreaAndBoundsGrowth`; `AllParametricContinuityModesAndClosedEdgesAreChecked` | Cylinder subdivision creates extra faces/edges with preserved area/validity; resource failure retains original and skips later stages. |
| 32 | `SewingContiguousEdgesAndProtectedUnificationHaveActualTopologyChanges` | Adjacent same-domain faces actually unify; protected shared edge prevents merging and retains two faces. |
| 33, 34 | `ReplacementHistoryAndConflictingSelectionsAreExplicit`; `SewingReturnsBoundaryReviewAndExplicitEditsComposeDeletions`; `AtomicPreviewComposesHistoryAndRejectsForeignAndUnverifiedInputs` | Replacement/deletion and composed history; type mismatch, cycles and nested/conflicting edits rejected without invented identity. |
| 35, 36 | `AtomicPreviewComposesHistoryAndRejectsForeignAndUnverifiedInputs`; `DivisionPreservesAreaAndBoundsGrowth`; `InvalidPortableRecordsAndContradictoryControlsAreRejected`; `NativeRepairBuffersHandlesAndRepeatedOwnershipAreChecked` | One-time atomic acceptance, failed/skipped outcomes, independent accepted owners and 48 native create/release lifecycles. |
| 37, 38, 39 | `SharedDefinitionRepairReopensStepIgesAndSelectsRealViewerDefects`; `MetadataConflictsForeignAndStaleSessionsNeverPublishPartialChanges` | One shared definition/two placements, names/colors, undo/redo, valid STEP/IGES reopen with area 200, real HWND selection/screenshots; conflicts and stale/foreign inputs reject. |
| 40 | `PortableRecipesRebindOnlyMatchingGeometryUnitsAndRevision`; `InvalidPortableRecordsAndContradictoryControlsAreRejected` | JSON contains no handles; fingerprint/unit/revision binding, deterministic re-execution and malformed/inconsistent record rejection. |

Final `artifacts/batch-q-final-focused.log` passes 25/25. The complete
`artifacts/batch-q-release-check.log` passes Release/Debug Generator 91/91 and Runtime
205/205, fresh-source build with 94 byte-identical generated files, both clean consumers,
14 package audits, inventory, compatibility and local release gates. The isolated actual
Debug-native run passes 205/205; all 35 private headers compile standalone. Native exports
retain the old 29,402 names and add fourteen; managed signatures add 540 against
Preview.15 and remove none. Full final inventory has 16,353 Emitted, 815 Manual,
49,760 Blocked, 49,344 Skipped and zero pending. Final hashes/evidence are in STATUS.

SC-054 records exactly 106 directly used formerly Blocked stable IDs in
`config/batches/batch-q-manual-calls.json`. Their dependency refinement includes native-
local inherited divider methods and wire/shape/naming helpers; it does not change the
frozen 52 roots, 40-row denominator or previous audit. No unused overload is accepted
merely because its class appears in the candidate set.
The final per-edge residual checks reuse four already Emitted `ShapeAnalysis_Wire`
gap/distance methods; they do not inflate the 106-ID Manual delta. Reconcile with
`eng/verify-batch-manual-accounting.ps1` against the preserved Preview.15 full inventory;
the verifier also rejects unaudited state/reason/identity changes outside the exact list.

### Audited root closure

| Root group | Exact inventory roots |
|---|---|
| Diagnosis | `ShapeAnalysis_ShapeContents`, `ShapeAnalysis_ShapeTolerance`, `ShapeAnalysis_FreeBounds`, `ShapeAnalysis_FreeBoundsProperties`, `ShapeAnalysis_Wire`, `ShapeAnalysis_WireOrder`, `ShapeAnalysis_Shell`, `ShapeAnalysis_CheckSmallFace` |
| Repair | `ShapeFix_Root`, `ShapeFix_Shape`, `ShapeFix_Wire`, `ShapeFix_Wireframe`, `ShapeFix_Edge`, `ShapeFix_Face`, `ShapeFix_Shell`, `ShapeFix_Solid`, `ShapeFix_FixSmallFace`, `ShapeFix_FixSmallSolid`, `ShapeFix_ShapeTolerance` |
| Normalize | `ShapeUpgrade_RemoveInternalWires`, `ShapeUpgrade_RemoveLocations`, `ShapeUpgrade_ShapeDivideContinuity`, `ShapeUpgrade_ShapeDivideAngle`, `ShapeUpgrade_ShapeDivideArea`, `ShapeUpgrade_ShapeDivideClosed`, `ShapeUpgrade_ShapeDivideClosedEdges`, `ShapeUpgrade_UnifySameDomain` |
| History | `ShapeBuild_ReShape`, `BRepTools_ReShape`, `BRepBuilderAPI_Sewing` |

These 30 decision roots are a candidate audit, not a commitment to expose
every declaration. Reused support roots (22) are:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `ShapeExtend_Status`, `BRepTools_History`, `BRepLib`, `BRepTools`, `BRepBuilderAPI_MakeSolid`.

Additional header-only/template dependencies: `NCollection_IndexedMap.hxx`, `TopTools_ShapeMapHasher.hxx`.
They are intentionally outside exact-root declaration counts. See the alias explanation
in the shared preparation document.

Dependencies close through copied value/definition inputs, native-local algorithm and
container use, registered owning topology results, parent-bound documents and viewer
objects, and existing exchange providers. OCCT toolkit dependencies reuse the existing
explicit CMake core closure; availability evidence is not link/runtime proof for new code.

### Baseline audit evidence

Full inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 52 | 2306 | 1031 | 604 | 91 | 580 |

Two audit runs are byte-identical. Report SHA256:
`DD1DAEE0D8D22F99EC8E3242FCF3136FB8326B39F243A8C9D5C68226D5A18F9F`.
Regenerate with `eng/audit-batch-roots.ps1` using the linked config and the pinned
inventory; report path is `artifacts/generator-reports/batch-q-root-audit.json`
inside the code workspace. Reused support accounts for much of these counts.
Candidates are neither 40 capabilities nor an implementation/API denominator.
Do not mark unrelated blocked/template/unsupported IDs manual merely because their
root appears here.

## Implementation ownership and source placement

Native: seven cohesive Modeling units (`RepairData`, `RepairDiagnostics`,
`RepairBoundaries`, `RepairFixers`, `RepairExecution`, `RepairSewing` and
`TopologyNormalization`), plus Xde `RepairPublication` and Visualization `RepairReview`.
`Modeling/Repair.hxx` contains the private contract; the public C companion is
`OcctSharp.Native.Repair.h`. The existing Runtime registry owns the result live set.
Managed: Modeling-owned copied contracts/shape operations and facade workflow orchestration;
XDE/exchange/viewer integration stays with its existing owner. Reuse Runtime/Shape,
Modeling/Features history conventions and the one registry. No new project or DLL.

Builders, adaptors, iterators and temporary arrays remain native-call-local; copied
results contain no borrowed pointers. Any owning result container needs a matching
release path and source-disposal tests. Shape owners reuse the current registration
and release family. Document labels and viewer IDs remain parent-bound and thread rules
remain unchanged. Concurrent release/use is not newly supported. OWNERSHIP, NATIVE_ABI,
SPECIAL_CASES and ADR-0084 record the implemented result owner, copied layouts,
matching release and 106 exact direct manual calls.

## Constraints and non-goals

Q does not replace the existing basic repair APIs. Hole removal, small-feature removal,
location flattening and topology simplification change modeling intent and are opt-in.
Tolerance increases cannot silently hide a failed geometric check. ShapeDivideContinuity
accepts parametric continuity, not G1/G2. General perfect healing and persistent identity
through arbitrary splits/merges are not promised. A failed required mapping or budget
blocks publication of that result, not just its diagnostic flag.

## Entry and completion gates

Use the shared [entry/delta protocol and validation gates](BATCH_Q_T_PREPARATION.md).
Q was implemented against completed B-P and ADR-0081; all local gates now pass.
The capability count stays 40 when the baseline changes; already delivered capabilities
are prerequisites, not a reason to pad the denominator. A substantive unsupported
capability or changed product outcome requires an explicit documented scope decision,
not silent deletion or a smaller completion claim.

Completion requires a 40-row test mapping, Release/Debug builds and regression with the
actual Debug native DLL, source-layout/dependency checks, exact stable-ID reconciliation,
applicable real-file/HWND workflows, both clean package consumers, clean regeneration,
API/ABI compatibility, runtime manifest and local release evidence, documentation and
one local batch commit. No automatic NuGet publication or GitHub push.

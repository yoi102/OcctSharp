# Batch T: Parametric document recompute and persistent topology selection

- Status: all original **40/40** rows implemented and locally validated; full release-check passes. Final document-package and local commit evidence is in STATUS.
- Decision: [ADR-0082](adr/0082-broad-batch-q-through-t-preparation.md).
- Preparation baseline: commit `6b04bd9`, package `8.0.1-preview.15`, OCCT 8.0.1.
- Local package version: `8.0.1-preview.19`; no publication.
- Execution contract and shared gates: [Q-T preparation](BATCH_Q_T_PREPARATION.md).
- Frozen configuration: [batch-t-parametric-document-recompute.json](../OcctSharp/config/batches/batch-t-parametric-document-recompute.json).

## Product outcome and existing coverage

Batch I already implements copied attributes, reference/tree graphs and topological ordering, named transactions, undo/redo/savepoints, Get/SetNamedShape and four OCAF/XDE persistence formats. Those graphs do not execute features. T adds a finite typed execution model, dirty propagation and persistent topology selection; it does not count existing graph getters or save APIs again.

Persist parameter/expression -> execute primitive/profile/Boolean/Q/R/S feature DAG ->
dirty one branch -> incremental atomic recompute -> re-resolve named edge/face ->
save/reopen -> undo/redo -> update repeated XDE occurrences -> export/review. Include
a cycle, missing parameter versus zero, failed middle node, ambiguous selector, dedicated
selector children, cancellation between calls and a document disposed before access.

One row below is one independently observable workflow capability, not one getter,
native class, test case or commit. All 40 rows plus their dependencies and validation
form one batch. Internal source groups are implementation responsibilities, not smaller
delivery batches. The `Integration`/`Execution` groups compose the audited roots and
existing public operations; they do not imply new OCCT class roots.

## Frozen capability and acceptance matrix

All forty rows remain the frozen scope. Each acceptance statement is a required
test, not by itself a report that it has passed. Shared lifetime/negative/package gates apply to
every applicable row and are not counted as additional capabilities.

| ID | Root group | New capability | Required observable acceptance |
|---|---|---|---|
| T-01 | State | Versioned parametric feature schema | Persist feature kind/version, parameter/result labels and source revision; reject unknown schema versions without mutation. |
| T-02 | State | Typed named parameter map | Persist integer/real/string/array values with Has checks so missing values remain distinct from zero or empty. |
| T-03 | State | Unit-aware scalar parameter references | Store declared units/dimensions and validate feature parameter compatibility before execution. |
| T-04 | State | Declarative scalar expression parameters | Support a bounded whitelist of arithmetic/min/max and parameter references with typed units; never evaluate arbitrary code. |
| T-05 | State | Expression dependency and cycle analysis | Resolve copied variable references and report expression cycles or missing inputs without changing persisted results. |
| T-06 | Graph | Persistent feature registration and removal | Allocate TFunction scope IDs and dedicated feature labels; remove references/result ownership atomically. |
| T-07 | Graph | Typed feature input/result bindings | Bind shape, mesh or scalar inputs to feature outputs using persisted references and explicit type checks. |
| T-08 | Graph | Atomic dependency rewiring | Update both graph directions and verify cycles before publication, distinct from querying I dependency snapshots. |
| T-09 | Graph | Executable plan construction | Produce a deterministic executable DAG with named blocked nodes and topological order, not just a copied reference graph. |
| T-10 | Graph | Dirty propagation from parameter edits | Mark directly touched features and transitive impacted dependants while preserving valid independent branches. |
| T-11 | Graph | Incremental recompute execution | Run only affected eligible nodes and demonstrate unchanged independent result revisions across repeated edits. |
| T-12 | Graph | Explicit full recompute | Rebuild all eligible nodes in dependency order and compare results/status with the incremental path. |
| T-13 | Graph | Targeted result recompute | Recompute the required ancestor closure for selected outputs and leave unrelated dirty branches pending. |
| T-14 | Graph | Persisted execution-state transitions | Keep not-executed/executing/succeeded/failed/blocked states coherent; recover interrupted persisted states on reopen. |
| T-15 | Graph | Touched/impacted/valid logbook semantics | Track separate sets and expose copied recompute evidence proving that merely touched does not mean successfully valid. |
| T-16 | Graph | Generation-safe result cache invalidation | Invalidate cached results on relevant definition/version changes; refuse stale shapes even when label entries are unchanged. |
| T-17 | Execution | Parametric primitive features | Execute typed box/cylinder features with scalar parameters and owning shape results; reuse existing modeling kernels. |
| T-18 | Execution | Parametric rigid-placement features | Execute supported placement/transform definitions with referenced input results and unchanged source owners. |
| T-19 | Execution | Parametric profile extrusion/revolution | Execute existing profile features from referenced planar topology and dimensioned parameters. |
| T-20 | Execution | Parametric Boolean features | Execute supported cut/fuse/common with typed inputs, copied diagnostics and explicit result-history support. |
| T-21 | Execution | Parametric Batch Q repair features | Execute a frozen Q repair recipe on referenced topology and publish only its accepted budget/history result. |
| T-22 | Execution | Parametric Batch S guided-sweep features | Execute persisted supported guide/law definitions with referenced profiles and source-result history. |
| T-23 | Execution | Parametric Batch S constrained-fill features | Execute persisted typed constraints and withhold results when required residual checks fail. |
| T-24 | Execution | Parametric Batch R mesh-output features | Persist supported meshing/edit recipes and output-kind metadata; never treat a discrete result as exact solid topology. |
| T-25 | Graph | Atomic multi-feature recompute commit | Compute into temporary owners and publish one document transaction only after all required targets pass. |
| T-26 | Graph | Failure propagation and last-good result policy | Mark dependent nodes blocked; expose last-good results only as explicitly stale and release failed candidates. |
| T-27 | Graph | Bounded cancellation between feature calls | Cancel before the next synchronous native operation and roll back pending work; do not claim interruptibility inside arbitrary OCCT calls. |
| T-28 | Naming | Generated/modified/deleted evolution recording | Write TNaming evolution from actual algorithm history and distinguish missing history from a guaranteed identity relation. |
| T-29 | Naming | Versioned evolution-history snapshot | Copy old/new shape relationships for a selected document revision without leaking TNaming iterators. |
| T-30 | Naming | Dedicated persistent subshape selection creation | Use a dedicated selector label; preserve feature/user attribute children that TNaming_Selector.Select would otherwise clear. |
| T-31 | Naming | Selection re-resolution after recompute | Solve supported selections against updated context and expose resolved topology as independent owning shapes. |
| T-32 | Naming | Deleted and ambiguous selection outcomes | Return explicit missing/ambiguous/unsupported results and require rebinding instead of choosing a nearest-looking subshape. |
| T-33 | Naming | Selection context and type constraints | Validate expected vertex/edge/face kind and context revision; reject foreign-document or unrelated result selections. |
| T-34 | State | Feature subgraph duplication with relocation | Copy a closed subgraph with TDF relocation and rewrite internal references; explicitly retain or reject external dependencies. |
| T-35 | State | Feature subgraph deletion policy | Delete a selected feature set using explicit cascade/reject-dependants policy and clear stale selector references atomically. |
| T-36 | State | Persistent recompute save/reopen | Reopen Bin/XML OCAF and XDE variants with functions, parameter schema and selections intact, then perform a real recompute. |
| T-37 | State | Recompute-aware undo/redo | Undo/redo a parameter edit plus published result transaction and restore dirty state, selections and visible result consistently. |
| T-38 | Integration | Parametric XDE definition replacement | Publish recomputed definitions once for repeated occurrences while retaining placements and reporting metadata-mapping conflicts. |
| T-39 | Integration | Recomputed exact-result STEP/IGES delivery | Export/reopen supported exact geometry and metadata; explicitly exclude the parametric graph and discrete-only outputs from format guarantees. |
| T-40 | Integration | Parametric result and failed-feature viewer review | Refresh result presentations on recompute and undo, highlight failed feature inputs and reject stale parent-bound selection IDs. |

## Implementation-to-acceptance evidence

The following tests are in OcctSharp.Runtime.Tests; each row appears exactly once.
The shared public-only workflow is also compiled into the clean package consumer.
Passing a focused case does not replace full exit gates, whose results are in STATUS.

| Capability | Executable evidence |
|---|---|
| `T-01` | UnknownSchemaAndEscapedFeaturePathsRejectWithoutMutationAndAttachedOwnerRemainsOwnedByCaller |
| `T-02` | NativeParameterStoragePreservesEmptyUnicodeTypedValuesAndAbort |
| `T-03` | PrimitiveExpressionUnitsScalarAndDisposedDocumentAreEnforced |
| `T-04` | ExpressionsEnforceUnitsBoundedArithmeticAndMissingInputs |
| `T-05` | PlansResolveCrossFeatureParametersAndRejectExpressionCycles |
| `T-06` | SubgraphRelocationRewritesIdsExpressionsAndNativeDependencies |
| `T-07` | PlansRejectMissingInputsAndIncompatibleOutputKinds |
| `T-08` | NativeGraphRewireRejectsCyclesBeforeChangingEitherDirection |
| `T-09` | PlansResolveCrossFeatureParametersAndRejectExpressionCycles |
| `T-10` | IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo |
| `T-11` | IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo |
| `T-12` | IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo |
| `T-13` | IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo |
| `T-14` | MalformedPersistedPathsFailBeforeMutationAndInterruptedStatesRecoverDownstream |
| `T-15` | IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo; FailedMiddleNodeKeepsAllLastGoodResultsAndBlocksDependants |
| `T-16` | NativeDependencyAndCurrentResultRevisionCorruptionRejectBeforeUse |
| `T-17` | PrimitiveExpressionUnitsScalarAndDisposedDocumentAreEnforced |
| `T-18` | IncrementalFullTargetedRecomputePreserveIndependentGenerationsAndUndo |
| `T-19` | ExtrusionAndRevolutionUseTypedParametersAndReferencedProfiles |
| `T-20` | TypedBooleanRepairAndMeshRecipesExecuteWithoutChangingExactInputs (four formats) |
| `T-21` | TypedBooleanRepairAndMeshRecipesExecuteWithoutChangingExactInputs (four formats) |
| `T-22` | PersistedGuidedLawAndConstrainedFillRecipesReexecuteRealKernels (four formats) |
| `T-23` | PersistedGuidedLawAndConstrainedFillRecipesReexecuteRealKernels (four formats) |
| `T-24` | TypedBooleanRepairAndMeshRecipesExecuteWithoutChangingExactInputs (four formats) |
| `T-25` | LaterKernelFailureRollsBackEarlierSuccessfulCandidates |
| `T-26` | FailedMiddleNodeKeepsAllLastGoodResultsAndBlocksDependants |
| `T-27` | CancellationAfterSuccessfulCandidateAbortsBeforeTheNextKernelAndPreservesRevisions |
| `T-28` | ActualExtrusionEvolutionHasOwningGeneratedAndTransactionSelectedHistory; NativeNamingReportsMultipleSuccessorsAsAmbiguousAndMissingContextAsDeleted |
| `T-29` | HistorySelectsDurableResultGenerationsAcrossUndoRedoAndReopen |
| `T-30` | DedicatedSelectionTracksActualTransformHistoryAndRejectsForeignStaleContexts |
| `T-31` | DedicatedSelectionTracksActualTransformHistoryAndRejectsForeignStaleContexts |
| `T-32` | NativeNamingReportsMultipleSuccessorsAsAmbiguousAndMissingContextAsDeleted |
| `T-33` | RelocatedSelectionsStayIndependentAndTamperedTokensAreRejected |
| `T-34` | SubgraphRelocationRewritesIdsExpressionsAndNativeDependencies; RelocatedSelectionsStayIndependentAndTamperedTokensAreRejected |
| `T-35` | SubgraphRelocationRewritesIdsExpressionsAndNativeDependencies |
| `T-36` | FourFormatsReopenFunctionsParametersAndSelectionsThenReallyRecompute; both four-format recipe tests |
| `T-37` | HistorySelectsDurableResultGenerationsAcrossUndoRedoAndReopen; shared BatchTParametricWorkflow |
| `T-38` | SubshapeMetadataConflictsPreventPublicationWithoutChangingOccurrences; shared BatchTParametricWorkflow |
| `T-39` | RecomputedSharedDefinitionsExchangeAndRealHwndReview |
| `T-40` | RecomputedSharedDefinitionsExchangeAndRealHwndReview |

Additional raw boundary, copied-array/schema and failure tests apply across the matrix.
Exact SC-057 accounting reconciles 65 new Manual IDs and zero other identity/disposition
changes against frozen S. Exit inventory SHA256:
`1A4B9369EA89E89F9A71BC12E190196FD010B716681939E935BEAC840A69FDBA`.
Three negative accounting cases reject wrong baseline, input overwrite and missing
implementation; original inputs remain unchanged.

## Native decision roots and dependency closure

| Root group | Exact inventory roots |
|---|---|
| Graph | `TFunction_Function`, `TFunction_GraphNode`, `TFunction_Scope`, `TFunction_Logbook`, `TFunction_Iterator`, `TFunction_ExecutionStatus` |
| Naming | `TNaming_Builder`, `TNaming_Selector`, `TNaming_Tool`, `TNaming_NamedShape`, `TNaming_Naming`, `TNaming_Iterator`, `TNaming_NewShapeIterator`, `TNaming_OldShapeIterator`, `TNaming_Evolution` |
| State | `TDataStd_NamedData`, `TDataStd_Expression`, `TDataStd_Variable`, `TDataStd_ReferenceArray`, `TDF_Reference`, `TDF_CopyTool`, `TDF_RelocationTable`, `TDF_DataSet`, `TDocStd_Application` |

These 24 decision roots are a candidate audit, not a commitment to expose
every declaration. Reused support roots (28) are:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `TDF_Label`, `TDF_ChildIterator`, `TDF_AttributeIterator`, `TDataStd_Name`, `TDataStd_Integer`, `TDataStd_Real`, `BinDrivers`, `XmlDrivers`, `BinXCAFDrivers`, `XmlXCAFDrivers`, `Standard_GUID`.

There are no additional header-only exceptions in this batch configuration.

Dependencies close through copied value/definition inputs, native-local algorithm and
container use, registered owning topology results, parent-bound documents and viewer
objects, and existing exchange providers. OCCT toolkit dependencies reuse the existing
explicit CMake core closure; availability evidence is not link/runtime proof for new code.

### Baseline audit evidence

Full inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 52 | 1981 | 866 | 536 | 101 | 478 |

Two audit runs are byte-identical. Report SHA256:
`9D26EC4CCCB68A7960B888023D8285FD6E682BBFB1A47960224F5C366508E428`.
Regenerate with `eng/audit-batch-roots.ps1` using the linked config and the pinned
inventory; report path is `artifacts/generator-reports/batch-t-root-audit.json`
inside the code workspace. Reused support accounts for much of these counts.
Candidates are neither 40 capabilities nor an implementation/API denominator.
Do not mark unrelated blocked/template/unsupported IDs manual merely because their
root appears here.

## Implementation ownership and source placement

### Post-S entry (2026-09-05)

Baseline `580bb22` / Preview.18; original Preview.15 evidence and config are retained.
The separate `batch-t-entry.json` and fresh 128-header inventory establish
SHA256 `78F5F2380209C17EC0A2C5A164B485B821563757EE073ABC23598F5CB76CE0D1`.
The exact delta contains 222 prior Blocked-to-Manual transitions, 11 in T roots,
zero added/removed/identity changes. Current 52 roots: 1,981 candidates, 855 Blocked,
536 Emitted, 112 Manual and 478 Skipped. Repeated root reports match SHA256
`61E5C27D2A14AB593A6636E0D244D26C2D1F81B85E15DA2692DEE857831497C7`.
New ownership/execution decisions are in ADR-0087. This entry evidence does not validate
new runtime operations. Expanded focused evidence includes four-format Q/R/S reexecution, exact source transforms,
relocated selectors, real HWND and durable result-generation history. Final counts and
full exit gates are recorded in STATUS; completion is not inferred from entry evidence.

Native: new `Documents/FunctionGraph.cpp`, `Documents/ParametricState.cpp` and
`Documents/TopologyNaming.cpp`, `Documents/ParametricRelocation.cpp` and
`Modeling/ParametricTransform.cpp`. Documents owns storage/naming only and must not include
higher Mesh/Xde/Visualization domain headers. Managed Documents owns copied schema and
low-level persistence; built-in Q/R/S execution and XDE/viewer coordination belong in
the existing facade (or appropriate higher existing owner), not in a new project and
not in a lower module that would introduce a dependency cycle.

Builders, adaptors, iterators and temporary arrays remain native-call-local; copied
results contain no borrowed pointers. Any owning result container needs a matching
release path and source-disposal tests. Shape owners reuse the current registration
and release family. Document labels and viewer IDs remain parent-bound and thread rules
remain unchanged. Concurrent release/use is not newly supported. Before introducing an
actual handle/layout/manual binding exception, update OWNERSHIP, NATIVE_ABI and
SPECIAL_CASES with exact directly invoked stable IDs; SC-057 and ADR-0087 record this implementation boundary.

## Constraints and non-goals

The expression language is deliberately finite (arithmetic, min/max, literal constants
and typed parameter references); no scripts, reflection, user assemblies or arbitrary
native-to-managed callbacks. Built-in feature evaluation is managed orchestration over
existing explicit operations, not a new virtual TFunction_Driver proxy system.
TNaming_Selector.Select clears descendants: use a dedicated private selector label.
Solve can fail; stable TDF entries do not guarantee persistent subshape identity.
No general constraint solver or promise of naming through arbitrary topology changes.

## Entry and completion gates

Use the shared [entry/delta protocol and validation gates](BATCH_Q_T_PREPARATION.md).
T has real execution dependencies on completed Q repair, R mesh and S guided/filling contracts. Those are not implemented by this preparation.
The capability count stays 40 when the baseline changes; already delivered capabilities
are prerequisites, not a reason to pad the denominator. A substantive unsupported
capability or changed product outcome requires an explicit documented scope decision,
not silent deletion or a smaller completion claim.

Completion requires a 40-row test mapping, Release/Debug builds and regression with the
actual Debug native DLL, source-layout/dependency checks, exact stable-ID reconciliation,
applicable real-file/HWND workflows, both clean package consumers, clean regeneration,
API/ABI compatibility, runtime manifest and local release evidence, documentation and
one local batch commit. No automatic NuGet publication or GitHub push.

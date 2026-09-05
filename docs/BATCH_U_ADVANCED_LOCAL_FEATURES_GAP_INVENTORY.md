# Batch U: Advanced contour finishing and limit-driven local features

- Status: original **40/40 rows implemented and locally validated**, including final documentation-package verification. Focused **96/96**, ten repeats and actual Debug-native **409/409 PASS**. Local commit recorded in STATUS/runbook.
- Decision: [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md).
- Historical preparation commit: `eacd0ed` / Preview.15. Implementation entry is T `bfb8811` / Preview.19; OCCT remains 8.0.1.
- Locally validated package: `8.0.1-preview.20`, ABI 1.64, bridge 0.72.0; full release-check and final documentation delivery PASS.
- Frozen roots: [batch-u-advanced-local-features.json](../OcctSharp/config/batches/batch-u-advanced-local-features.json).
- Shared preparation: [U-W evidence](BATCH_U_W_PREPARATION.md).
- Delivery: [continuous Q-W runbook](BATCH_CONTINUOUS_EXECUTION.md); one complete batch per local commit.

## Product outcome and reuse boundary

FeatureModeling already implements selected constant/start-end-radius fillets, symmetric/two-distance chamfers, a shared-angle Draft helper, vector-depth boss/pocket, basic revolved/pipe features, holes and copied history. U extends contour programs, advanced chamfer modes, limit-driven BRepFeat operations and native failure evidence; it does not count those basic operations again.

A supported CAD part -> tangent-contour law finishing and simulated sections -> tapered/limited prism or rib -> protected acceptance -> T recompute -> shared XDE definition replacement -> STEP/IGES and viewer failure/result review.

All 40 rows are one delivery unit. A row is a newly observable workflow, not a getter,
test, overload or standalone family checkpoint. The original acceptance criteria below
remain required; current named runtime evidence follows the matrix. Existing lower generated wrappers are reused
where ownership permits; candidate root membership alone does not mean a missing API.

## Frozen capability matrix

| ID | Root group | New capability | Required acceptance |
|---|---|---|---|
| U-01 | Finish | Tangent-contour discovery from selected seed edges | Copy ordered contour membership, closure, length and normalized abscissa; reject seeds outside the source revision. |
| U-02 | Finish | Law-driven contour fillets | Use S copied scalar-law definitions for supported fillet radius laws, not only J endpoint interpolation. |
| U-03 | Finish | Sampled radius-profile fillets | Accept normalized parameter/radius samples with strict ordering and positive admissible values; report invalid contour domains. |
| U-04 | Finish | Multiple contour radius programs | Assign independent programs to disjoint contours in one request and reject conflicting assignments to the same tangent chain. |
| U-05 | Finish | Vertex-anchored contour radius constraints | Bind supported radii at contour vertices with explicit correspondence and report incompatible junctions. |
| U-06 | Finish | Fillet surface-representation control | Select supported rational/quasi-angular/polynomial representation with numeric shape checks, not a representation-quality guarantee. |
| U-07 | Finish | Fillet continuity and approximation policy | Expose supported internal continuity and tolerance controls as a validated bundle; report achieved validity separately. |
| U-08 | Finish | Fillet section simulation | Copy simulated section circle/parameter data per contour into independent DTOs; no Sect array or builder handle escapes. |
| U-09 | Finish | Fillet surface-patch provenance | Map generated patch groups and input contour identity to owning results; distinguish unsupported history from an empty patch set. |
| U-10 | Finish | Detailed faulty contour and vertex report | Copy stripe status, faulty contours and vertices with revision-scoped IDs and actionable failure locations. |
| U-11 | Finish | Explicit partial-result inspection | Return BadShape only when HasResult permits, clearly marked invalid/partial and never promoted to accepted output automatically. |
| U-12 | Finish | Editable finishing recipe replay | Replace/remove contour assignments in an immutable recipe and rebuild from the original source, without retaining native builder state. |
| U-13 | Finish | Distance-angle chamfers | Apply AddDA with correct support-face orientation and angle units; verify unequal supporting-face geometry. |
| U-14 | Finish | Constant-throat chamfers | Use the supported constant-throat mode and verify throat measurements against an eligible analytic section. |
| U-15 | Finish | Constant-throat penetration chamfers | Expose the penetration mode with its asymmetric support semantics; do not treat its dimensions as ordinary two-distance chamfers. |
| U-16 | Finish | Per-contour chamfer dimension programs | Set independent supported dimensions per contour under one explicit mode; reject conflicting mode requests before build. |
| U-17 | Finish | Chamfer tangent-chain correspondence | Return contour/member/vertex correspondence to support recipe editing beyond J result-only history. |
| U-18 | Draft | Per-face draft programs | Accept individually defined angles/neutral-plane/pull settings, validating propagated face conflicts rather than one global angle. |
| U-19 | Draft | Controlled tangent propagation | Allow explicit propagation policy and expose the effective affected-face set before accepting a drafted result. |
| U-20 | Draft | Draft problem-face diagnosis | Map failed draft addition/build to affected faces and status without guessing an angle that succeeds. |
| U-21 | Draft | Draft shell by length | Use MakeDraft on an eligible edge/wire/shell with length-driven extent, distinct from changing angles on selected solid faces. |
| U-22 | Draft | Draft shell up to a support surface | Limit an eligible draft to supported surface geometry and verify intended side and intersection. |
| U-23 | Draft | Draft shell up to a shape | Limit the draft against an owning stop shape and return stop-contact provenance with source isolation. |
| U-24 | Local | Profile feature support and sliding contract | Validate base/profile/support membership and explicit sliding edge-to-face constraints before a BRepFeat operation. |
| U-25 | Local | Prismatic feature up to a limiting shape | Create/cut a prism with an explicit stop shape instead of only a vector depth; report unreachable or ambiguous stops. |
| U-26 | Local | Prismatic feature between two limits | Apply supported From/Until boundaries and verify retained starting/ending topology and orientation. |
| U-27 | Local | Prismatic until-end and from-end feature modes | Expose supported semi-infinite modes with finite owning output and clearly defined base-side behavior. |
| U-28 | Local | Drafted prism by height | Create additive/subtractive tapered features with explicit profile, draft angle and height using MakeDPrism. |
| U-29 | Local | Drafted prism between shape limits | Use supported From/Until or stop-plus-height modes and report failed limiter intersections independently of repair. |
| U-30 | Local | Local revolved feature with limiting shapes | Use BRepFeat limits and support membership beyond J angle-only Boolean-composed revolve. |
| U-31 | Local | Local pipe feature with limiting shapes | Use eligible BRepFeat pipe support/stop behavior, separate from S general guide/law sweep authoring. |
| U-32 | Local | Linear rib and slot features | Build supported rib/slot forms from a wire/plane and thickness directions; verify additive/subtractive results. |
| U-33 | Local | Revolution rib and slot features | Build supported rotational forms with axis/angular limits and source-to-feature correspondence. |
| U-34 | Local | Cylindrical hole between explicit bounds | Use supported local-hole axial bound/up-to modes with result checking, extending J simple cylinder-cut holes. |
| U-35 | Local | Feature top/lateral/contact grouping | Return supported cap, lateral and glued/sliding interface groups as owning shapes with named provenance. |
| U-36 | Integration | Protected local-feature acceptance | Apply Q tolerance/geometric-change/protected-boundary policy to a U result; retain original topology on rejected acceptance. |
| U-37 | Integration | U feature programs in parametric recompute | Extend T typed built-ins with finishing and supported limit-driven recipes; failed programs preserve explicit last-good state. |
| U-38 | Integration | Occurrence-aware feature definition replacement | Edit one XDE definition through an explicit occurrence context and preserve repeated placements and unambiguous metadata mapping. |
| U-39 | Integration | Advanced-feature exact exchange delivery | STEP/IGES reopen representative law-fillet and rib/limited-prism results with geometric and supported metadata assertions. |
| U-40 | Integration | Contour and stop-surface viewer review | Review simulated sections, faulty chains and limiting surfaces alongside the accepted shape using real parent-bound viewer identities. |


## Implementation assertion map

All original forty rows remain unchanged. Focused current-source tests pass 96/96;
and the full release-check passes separately. Final documentation delivery is recorded
in STATUS. Named tests below live in
[Runtime tests](../OcctSharp/tests/OcctSharp.Runtime.Tests/) and reuse the public-only
[shared workflow](../OcctSharp/tests/Shared/BatchULocalFeatureWorkflow.cs).
RawSnapshotsValidateEveryCapacityBeforeWritingAndClearFailedOutputs additionally
checks ABI buffer sentinels and failed output initialization.

| Row | Named assertions | Observable evidence |
|---|---|---|
| U-01 | `TangentChainLawUsesTheWholeOrderedContourDomain`; `ClosedCircularContourHasExplicitSeamCorrespondence` | ordered membership, closure, abscissae and duplicate/foreign rejection |
| U-02 | `NonconstantRadiusProfilesChangeActualGeometry`; `TangentChainLawUsesTheWholeOrderedContourDomain` | actual simulated radii, measured adapter error and changed solid |
| U-03 | `NonconstantRadiusProfilesChangeActualGeometry`; `ClosedLawsRejectValueAndDerivativeSeamsBeforeKernelBuild` | sampled interpolation, full domain and seam checks |
| U-04 | `IndependentContoursAndVertexProgramsRejectAmbiguousAssignments` | independent dimensions and exact numeric material removal |
| U-05 | `LawAnchorsAreConsistencyConstraintsAndSharedJunctionsCannotConflict`; `IndependentContoursAndVertexProgramsRejectAmbiguousAssignments` | constant overrides, law consistency and shared junction rejection |
| U-06 | `RepresentationAndContinuityHaveIndependentNumericValidation` | all three representations and volume tolerance |
| U-07 | `RepresentationAndContinuityHaveIndependentNumericValidation`; `RealCornerPlateFailureReturnsOnlyAnOwningDiagnosticPartial` | all continuities, numeric result and over-coarse approximation failure |
| U-08 | `ContourDiscoverySimulationAndOwningPatchResults` | copied circle sections, radius and post-source-disposal lifetime |
| U-09 | `ContourDiscoverySimulationAndOwningPatchResults` | owning surface-patch groups and source evolution |
| U-10 | `FailureCannotProduceAnAcceptedRoot`; `RealCornerPlateFailureReturnsOnlyAnOwningDiagnosticPartial` | faulty contour and actual corner fault, no successful root |
| U-11 | `RealCornerPlateFailureReturnsOnlyAnOwningDiagnosticPartial` | real HasResult/BadShape, partial lifetime, no acceptance |
| U-12 | `RecipesReplayOriginalSourceAndRejectForeignSelections` | replace/remove replay and original-source validation |
| U-13 | `ChamferModesMeasureTheActualSupportingSection` | distance-angle setbacks and section area |
| U-14 | `ChamferModesMeasureTheActualSupportingSection` | perpendicular-support throat measurement |
| U-15 | `ChamferModesMeasureTheActualSupportingSection` | native penetration setbacks and volume |
| U-16 | `IndependentChamferContoursKeepTheirOwnDimensions` | two contour programs with separate dimensions |
| U-17 | `ChamferModesMeasureTheActualSupportingSection`; `IndependentChamferContoursKeepTheirOwnDimensions` | seed/member/vertex contour correspondence |
| U-18 | `PerFaceDraftPreflightAndIndependentAnglesPreserveTheOriginal` | different angles and unchanged source |
| U-19 | `TangentDraftReportsTheEffectiveChainAndRejectsSelectedFaceOnly` | rounded-part tangent chain, preflight and propagation rejection |
| U-20 | `DraftFailurePreservesTheActualProblemFace` | actual Add failure, original problem-face index and SDK status |
| U-21 | `DraftShellExtentAndStopsCreateIndependentShells` | slanted length and independent shell |
| U-22 | `DraftShellExtentAndStopsCreateIndependentShells`; `AnalyticEdgeShellLimitsReportMeasuredGeometryAndEdgeOnlyHistory`; `CorneredShellLimitProfilesRejectBeforeEnteringTheUnsafeSdkPath` | analytic surface stop and measured height; invalid open-line results and unsafe cornered profiles explicitly reject |
| U-23 | `DraftShellExtentAndStopsCreateIndependentShells`; `AnalyticEdgeShellLimitsReportMeasuredGeometryAndEdgeOnlyHistory` | circle/line shape stops, owning limit history, exact final lateral membership and source disposal |
| U-24 | `SlidingProfileEdgesRetainSharedMembershipAndRejectForeignPairs` | real sliding pocket, duplicate/foreign graph rejection |
| U-25 | `LocalPrismsUseRealSupportAndLimits`; `UnreachableLimiterIsNotReplacedByAnUnboundedFeature` | numeric Until success and parallel unreachable stop |
| U-26 | `LocalPrismsUseRealSupportAndLimits` | From/Until volume, valid shape and final group indices |
| U-27 | `SemiInfinitePrismModesHaveFiniteCutResults`; `DraftedUntilEndInsufficientAxialReachRemainsADiagnosticFailure` | eight modes and disclosed insufficient-reach failure |
| U-28 | `LocalPrismsUseRealSupportAndLimits`; `DraftedCutHeightMeasuresTheActualTaper` | additive/subtractive drafted height and exact taper integral |
| U-29 | `LocalPrismsUseRealSupportAndLimits`; `SemiInfinitePrismModesHaveFiniteCutResults`; `UnreachableLimiterIsNotReplacedByAnUnboundedFeature` | drafted limits, stop-plus-height and failures |
| U-30 | `RevolvedFeaturesUseRadialProfileAndPlanarStop` | Extent/Until/FromUntil volume, negative-Y side and single solid |
| U-31 | `PipeFeatureStopsUseTheSharedSupportGraph`; `LimitedLineSpineReportsThePinnedSdkCurveConversionFailure` | bounded Bezier success and disclosed untrimmed-line failure |
| U-32 | `LinearRibsAndSlotsChangeMaterialThroughNativeForms` | add/cut material changes and native history |
| U-33 | `RevolutionRibsAndSlotsUseLinearThickness`; `AngularRibClippingPreservesBaseOutsideTheSelectedInterval` | linear thickness, material-only angular clipping and base preservation |
| U-34 | `LocalHolesUseNativeBoundedConstruction` | bounded/next/end/blind hole volume |
| U-35 | `LocalPrismsUseRealSupportAndLimits`; `LinearRibsAndSlotsChangeMaterialThroughNativeForms`; `AnalyticEdgeShellLimitsReportMeasuredGeometryAndEdgeOnlyHistory` | final cap/lateral/contact correspondence; unavailable cap remains absent and pre-limit shell is not a final lateral |
| U-36 | `ProtectedAcceptanceEnforcesExactHistoryAndBudgetsBeforeSingleConsumption` | exact protected mapping, budgets, rejection, independent single acceptance |
| U-37 | `PersistedContourRecipeReexecutesAndFailedParametersKeepLastGood`; `LimitedRecipeReopenRebuildsWithExactSupportSharing`; `AdditionalFinishingRecipesExecuteThroughTheFacade` | four-format reopen actually reruns kernels; explicit last-good and rebind |
| U-38 | `SharedDefinitionExchangeAndRealHwndReview`; `ChangedOccurrenceOrDefinitionCannotPublishAStaleCandidate`; `AmbiguousMetadataForeignSourcesAndDisposedParentsRejectPublication` | shared placements, metadata, undo/redo and atomic rejection |
| U-39 | `SharedDefinitionExchangeAndRealHwndReview` | fillet/rib/limited prism STEP+IGES area, name, color and disclosure |
| U-40 | `SharedDefinitionExchangeAndRealHwndReview` | real HWND sections/limits/fault context, atomic replacement, foreign/disposed/thread guards |


## Root, dependency and source closure

| Root group | Exact decision roots |
|---|---|
| Finish | `BRepFilletAPI_MakeFillet`, `BRepFilletAPI_MakeChamfer`, `BRepFilletAPI_LocalOperation`, `ChFi3d_FilletShape`, `ChFiDS_ChamfMode` |
| Draft | `BRepOffsetAPI_DraftAngle`, `BRepOffsetAPI_MakeDraft` |
| Local | `BRepFeat_MakePrism`, `BRepFeat_MakeDPrism`, `BRepFeat_MakeRevol`, `BRepFeat_MakePipe`, `BRepFeat_MakeLinearForm`, `BRepFeat_MakeRevolutionForm`, `BRepFeat_MakeCylindricalHole`, `BRepFeat_Form`, `BRepFeat_RibSlot` |

The 16 decision roots reuse 28 support roots:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `BRepTools_History`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `Law_Function`, `Law_Interpol`, `Law_BSpline`, `BRepBuilderAPI_MakeEdge`, `BRepBuilderAPI_MakeWire`, `BRepBuilderAPI_MakeFace`, `gp_Ax1`, `gp_Pln`, `gp_Dir`, `GeomAbs_Shape`.

Integration rows reuse the established or explicitly prepared public workflows rather
than requiring a second copy of their native bindings. S provides copied scalar laws; Q provides acceptance/history policies; T provides the typed recompute integration. These are real future prerequisites, not already delivered APIs.
Delivery order alone is not an algorithm dependency. Every prerequisite must actually
pass its whole-batch gates before U starts.

Native implementation uses eight cohesive Modeling units: LocalFeatureData,
ContourFinishing, ChamferContours, LocalDraft, LimitedPrisms, LimitedSweeps,
RibSlotFeatures and LocalHoles. Reuse Modeling/Features result/history helpers and
Runtime owners; no large operation switch is appended to Features.cpp. Managed
contracts/operations stay in Modeling; T integration, XDE and viewer orchestration
stay in the existing facade above Documents.

Private headers stay acyclic and domain-owned. Native builders, iterators and temporary
arrays are local to a call. Recipes and diagnostic/index/history records are copied.
Owning topology reuses the current registry/release family and survives its inputs.
Document labels remain parent-bound; viewer objects remain parent-bound/thread-affine.
W light/texture IDs and U/V preview/result owners require concrete ownership and cleanup
evidence during implementation, not new independent registries. Do not expose native
session/GPU pointers. No new ownership category or binary split is created by preparation.

## OCCT limitations and non-goals

KI-034 narrows the previously assumed shell-limit eligibility after actual Debug
evidence: limit-driven profiles require one analytic line/circle boundary edge;
cornered/multi-edge inputs reject before the unsafe SDK path. Length-only cornered
drafts remain supported. `AnalyticEdgeShellLimitsReportMeasuredGeometryAndEdgeOnlyHistory`
checks both transitions, independent geometry and final lateral membership;
`CorneredShellLimitProfilesRejectBeforeEnteringTheUnsafeSdkPath` preserves the former
cornered-limit cases as explicit rejections. A completed but invalid open-line/surface
result is diagnostic-only and RequireShape rejects it. U-21/22/23 still require and
have positive length/surface/shape-limit evidence; no row is removed. PreLimitShape
is distinct from final laterals, and only source-edge generation history is supported.

Arbitrary callback laws and persistent native fillet/feature builders remain excluded. Radius samples use normalized contour parameters; discontinuities, incompatible tangent chains and junctions may fail. BadShape is a diagnostic partial result, not a success fallback. Throat and penetration chamfers have different geometry from ordinary chamfers. BRepFeat support/sliding/limit preconditions are validated, not hidden by a generic Boolean fallback.

## Preparation evidence and baseline freshness

Inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 44 | 2045 | 943 | 431 | 113 | 558 |

Root report SHA256:
`1F1780A57E636DECE9EA0C6BC786EECB2F80054A95086373917FB5110D22710A`.
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

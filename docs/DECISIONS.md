# Architecture Decisions

- [ADR-0108](adr/0108-generated-private-modeling-builders.md): accepted private modeling builders and copied diagnostics.

Accepted decisions live in individual ADR files and are append-only in spirit.
Superseded ADRs remain available and point to their replacements.

## Accepted

| ADR | Decision | Status |
|---|---|---|
| [ADR-0139](adr/0139-generated-exact-consumer-prerequisites.md) | Exact generated consumer support with existing result owners and viewer-owned shared source IDs | Accepted for CH; delivery separately gated |
| [ADR-0137](adr/0137-private-located-assembly-normalization.md) | Exact private shared-clone normalization of located assembly definitions and semantic XDE value delegates | Accepted for CC; delivery separately gated |
| [ADR-0101](adr/0101-generated-coupled-repair-workspaces.md) | Closed coupled repair workspaces, private deep repair receivers and exact replacement evidence | Accepted |
| [ADR-0100](adr/0100-generated-topology-and-exchange-collections.md) | Exact owning topology collections and retained exchange entities with private cursors and one transfer epoch | Accepted |
| [ADR-0099](adr/0099-generated-bulk-analysis-and-typed-curve-collections.md) | Exact sized bulk operations and independent typed spline collection ownership | Accepted |
| [ADR-0098](adr/0098-generated-surface-grids-and-retained-interpolation.md) | Exact Array2 grids and retained HArray interpolation with independent snapshots and finite flat-height results | Accepted |
| [ADR-0097](adr/0097-generated-copied-arrays-and-sequences.md) | Exact copied Array1/Sequence profiles with immutable snapshots and transactional result publication | Accepted |
| [ADR-0096](adr/0096-generated-owned-nontransient-values.md) | Audited owning non-transient instances with stable storage and leased non-reused token handles | Accepted |
| [ADR-0095](adr/0095-generated-reference-operations.md) | Audited copied mutable-reference operations with atomic managed publication | Accepted |
| [ADR-0094](adr/0094-generated-geometric-value-operations.md) | Copied geometric instance operations and transform coefficients | Accepted |
| [ADR-0093](adr/0093-generated-geometric-value-projections.md) | Broad generated geometric value copies with preserved overload epochs | Accepted |
| [ADR-0092](adr/0092-generated-numeric-and-copied-reference-expansion.md) | Broad generated numeric and copied-reference expansion with stable overload identities | Accepted |
| [ADR-0001](adr/0001-repository-layout.md) | Separate root documentation from the inner code workspace | Accepted |
| [ADR-0002](adr/0002-native-c-abi.md) | Use a versioned native C ABI boundary | Accepted |
| [ADR-0003](adr/0003-canonical-binding-model.md) | Normalize AST facts through a canonical binding model | Accepted |
| [ADR-0004](adr/0004-initial-windows-dotnet-occt-baseline.md) | Use .NET 10, Windows x64, VS 2026, and OCCT 8.0.1 as the first baseline | Accepted |
| [ADR-0005](adr/0005-native-status-and-shape-handle.md) | Use stable statuses, thread-local diagnostics, and an owning shape handle | Accepted |
| [ADR-0006](adr/0006-clangsharp-ast-frontend.md) | Use pinned ClangSharp/libClangSharp for initial semantic discovery | Accepted |
| [ADR-0007](adr/0007-generated-source-and-raw-naming.md) | Commit deterministic generated source and separate internal raw naming from the friendly API | Accepted |
| [ADR-0008](adr/0008-initial-nuget-and-native-runtime-layout.md) | Use one initial NuGet package and an application-local `occt` runtime directory | Accepted |
| [ADR-0009](adr/0009-native-shape-handle-registry.md) | Register native shape handles and reject stale handles before dereference | Accepted |
| [ADR-0010](adr/0010-standard-transient-shared-handle.md) | Preserve OCCT intrusive sharing through an experimental `Standard_Transient` handle probe | Accepted |
| [ADR-0011](adr/0011-shared-handle-runtime-type-identity.md) | Validate shared-handle runtime type identity through OCCT RTTI names and `IsKind` | Accepted |
| [ADR-0012](adr/0012-checked-shared-handle-cast.md) | Require OCCT RTTI validation before creating a typed shared wrapper | Accepted |
| [ADR-0013](adr/0013-generated-typed-shared-handle.md) | Generate configured typed wrappers that preserve OCCT intrusive handles | Accepted |
| [ADR-0014](adr/0014-batched-full-occt-inventory.md) | Measure full OCCT through a separate batched semantic inventory | Accepted |
| [ADR-0015](adr/0015-staged-managed-package-modularity.md) | Stage module projects and packages without duplicating the current native closure | Accepted |
| [ADR-0016](adr/0016-generated-topods-shape-value-semantics.md) | Generate `TopoDS_Shape` value-copy, identity, and orientation semantics | Accepted |
| [ADR-0017](adr/0017-generated-typed-topology-casts.md) | Generate checked typed `TopoDS_*` wrappers without crossing native layouts | Accepted |
| [ADR-0018](adr/0018-opaque-gp-trsf-value-bridge.md) | Preserve `gp_Trsf` transformation values without crossing C++ layout | Accepted |
| [ADR-0019](adr/0019-opaque-toploc-location-value-bridge.md) | Preserve `TopLoc_Location` semantics without crossing C++ layout | Accepted |
| [ADR-0020](adr/0020-opaque-gp-vector-axis-matrix-value-bridge.md) | Preserve `gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` as opaque values | Accepted |
| [ADR-0021](adr/0021-opaque-occt-strings-and-real-sequence.md) | Preserve OCCT UTF-8/UTF-16 strings and `NCollection_Sequence<double>` behind explicit buffer/index contracts | Accepted |
| [ADR-0022](adr/0022-opaque-occt-real-array-and-vector.md) | Preserve `NCollection_Array1<double>` and the OCCT 8 dynamic-array-backed vector alias behind opaque bound/index contracts | Accepted |
| [ADR-0023](adr/0023-opaque-occt-integer-key-maps.md) | Preserve scalar integer-key maps behind opaque lookup/index contracts | Accepted |
| [ADR-0024](adr/0024-b06-snapshot-enumeration.md) | Use caller-owned one-shot snapshots instead of native iterators across the ABI | Accepted |
| [ADR-0025](adr/0025-gp-point-friendly-value-facade.md) | Expose generated `gp_Pnt` through an immutable validated `GpPoint` facade | Accepted |
| [ADR-0026](adr/0026-opaque-gp-xyz-value-bridge.md) | Preserve `gp_XYZ` algebra behind an explicitly sized opaque value-copy ABI | Accepted |
| [ADR-0027](adr/0027-opaque-gp-line-value-bridge.md) | Preserve `gp_Lin` origin/direction and core geometry operations behind an opaque value ABI | Accepted |
| [ADR-0028](adr/0028-opaque-gp-circle-value-bridge.md) | Preserve `gp_Circ` center/normal/radius and core measurements behind an opaque value ABI | Accepted |
| [ADR-0029](adr/0029-opaque-gp-ax3-value-bridge.md) | Preserve `gp_Ax3` coordinate-system values behind an opaque value ABI | Accepted |
| [ADR-0030](adr/0030-opaque-gprop-properties-bridge.md) | Keep `GProp_GProps` and BRepGProp computations behind an owning opaque handle | Accepted |
| [ADR-0031](adr/0031-primitive-solid-builders.md) | Keep BRep primitive builders native-local and expose safe owning Shape results | Accepted |
| [ADR-0032](adr/0032-topology-face-snapshot.md) | Snapshot topology faces as owning copies without exposing native explorers | Accepted |
| [ADR-0033](adr/0033-opaque-boolean-shape-operations.md) | Keep BRepAlgoAPI Fuse/Cut state native-local and return owning shape results | Accepted |
| [ADR-0034](adr/0034-brep-mesh-bulk-snapshot.md) | Transfer triangulated mesh data through caller-owned copied buffers | Accepted |
| [ADR-0035](adr/0035-opaque-shapefix-result.md) | Keep ShapeFix_Shape native-local and return an owning fixed shape | Accepted |
| [ADR-0036](adr/0036-opaque-unify-same-domain-result.md) | Keep ShapeUpgrade_UnifySameDomain native-local and return an owning result | Accepted |
| [ADR-0037](adr/0037-iges-read-bridge.md) | Keep initial IGES read transfer native-local and return one owning shape | Accepted |
| [ADR-0038](adr/0038-null-shape-failure-contract.md) | Reject null topology values at modeling/healing operation boundaries | Accepted |
| [ADR-0039](adr/0039-basic-brep-edge-wire-face-builders.md) | Return owning topology from native-local edge, wire, and face builders | Accepted |
| [ADR-0040](adr/0040-brep-adaptor-value-snapshots.md) | Copy BRep adaptor curve/surface values without exposing borrowed geometry | Accepted |
| [ADR-0041](adr/0041-basic-modeling-algorithm-results.md) | Return owning Common/Fuse topology and copied minimum-distance values | Accepted |
| [ADR-0042](adr/0042-boolean-healing-owning-results-without-history.md) | Close Boolean/healing result ownership while keeping history native-local | Accepted |
| [ADR-0043](adr/0043-native-local-mesh-format-providers.md) | Keep mesh-format providers/configuration native-local and expose geometry-only owning results | Accepted |
| [ADR-0044](adr/0044-ocaf-document-and-stable-entry-labels.md) | Own OCAF documents and represent parent-bound labels by stable TDF entries | Accepted |
| [ADR-0045](adr/0045-parent-bound-xde-metadata-and-assemblies.md) | Keep XCAF tools native-local and expose parent-bound labels plus copied metadata/occurrences | Accepted |
| [ADR-0046](adr/0046-hwnd-thread-affine-viewer-and-presentation-ids.md) | Own the HWND-bound visualization graph on one thread and expose presentations/selection as IDs | Accepted |
| [ADR-0047](adr/0047-optional-dependency-profiles-and-package-isolation.md) | Classify optional SDK/toolkit profiles reproducibly and isolate their future packages from core | Accepted |
| [ADR-0048](adr/0048-final-long-tail-and-header-classification.md) | Give every full-inventory declaration and entry header a deterministic final disposition | Accepted |
| [ADR-0049](adr/0049-release-evidence-and-publication-gates.md) | Use one reproducible release evidence pipeline and keep batch completion separate from publication readiness | Superseded in part by ADR-0050 |
| [ADR-0050](adr/0050-completion-gates-are-not-classification-gates.md) | Keep classification separate from completion; its numbered-batch model is superseded by ADR-0054 | Superseded in part by ADR-0054 |
| [ADR-0051](adr/0051-repository-native-bootstrap.md) | Bootstrap repository native runtime from pinned local or immutable OCCT inputs | Superseded in part by ADR-0059 |
| [ADR-0052](adr/0052-native-local-common-modeling-operations.md) | Keep common modeling algorithms native-local and reconcile audited manual stable IDs | Accepted |
| [ADR-0053](adr/0053-composable-xde-step-import.md) | Import STEP roots into an owned XDE document for composable assembly workflows | Accepted |
| [ADR-0054](adr/0054-single-complete-migration-batch.md) | Merge the former B00-B20 plan into one complete migration batch B | Accepted |
| [ADR-0055](adr/0055-generated-operation-namespaces-and-placement-allocation.md) | Separate generated operation namespaces and retain placement allocators | Accepted |
| [ADR-0056](adr/0056-generated-translation-unit-completion-headers.md) | Complete forward-declared template elements before generated scope headers | Accepted |
| [ADR-0057](adr/0057-core-toolkit-closure-and-auto-package-exclusions.md) | Link one explicit core toolkit closure and give excluded automatic packages stable dispositions | Accepted |
| [ADR-0058](adr/0058-narrow-long-tail-dispositions-and-local-completion-gate.md) | Replace LT001-LT004 with narrow evidence and require all local implementation gates for Batch B completion | Accepted |
| [ADR-0059](adr/0059-committed-windows-runtime-and-mit-license.md) | Commit the verified Windows x64 runtime and license OcctSharp project code under MIT | Accepted |
| [ADR-0060](adr/0060-common-cad-api-product-batch.md) | Make Batch C one large common-CAD-API product batch instead of small per-class work | Accepted |
| [ADR-0061](adr/0061-domain-layered-generated-output.md) | Partition generated output by product module and API layer without changing assembly, DLL, or public type identity | Accepted |
| [ADR-0062](adr/0062-generated-shard-dependency-closure.md) | Close every emitted cross-shard signature edge, add MeshData, and defer physical project/DLL splitting behind compatibility and cross-DLL ownership work | Accepted |
| [ADR-0063](adr/0063-final-batch-c-selective-session-topology-viewer-closure.md) | Close Batch C with selective STEP sessions, owning topology edits, copied selected subshapes, and parent-bound application input | Accepted |
| [ADR-0064](adr/0064-production-cad-viewport-review-batch.md) | Open Batch D as one 24-capability production CAD viewport/model-review wave with copied identity/topology and parent-bound filters/clip planes | Accepted |
| [ADR-0065](adr/0065-occt-aligned-nuget-preview-version.md) | Align NuGet numeric versions with OCCT and retain an independent OcctSharp preview counter | Accepted |
| [ADR-0066](adr/0066-engineering-inspection-measurement-pmi-batch.md) | Define and close Batch E as one 24-capability engineering inspection, exact measurement, and PMI/AP242 wave | Accepted |
| [ADR-0067](adr/0067-freeform-curve-surface-authoring-batch.md) | Define and close Batch F as one 24-capability freeform curve, surface, and profile-to-solid authoring wave | Accepted |
| [ADR-0068](adr/0068-technical-drawing-hidden-line-vector-output-batch.md) | Define and close Batch G as one 24-capability technical-drawing, hidden-line, section, and vector-output wave | Accepted |
| [ADR-0069](adr/0069-advanced-mesh-scene-material-interchange-batch.md) | Define and close Batch H as one 24-capability advanced-mesh, scene, material, LOD, and interchange wave | Accepted |
| [ADR-0070](adr/0070-document-state-attribute-graph-history-persistence-batch.md) | Define Batch I as one 24-capability OCAF/XDE document-state, attribute-graph, history, and persistence wave | Accepted for implementation |
| [ADR-0071](adr/0071-advanced-feature-modeling-history-recovery-batch.md) | Define Batch J as one 24-capability advanced feature-modeling, robust-Boolean, copied-history, and recovery wave | Accepted and implemented |
| [ADR-0072](adr/0072-assembly-authoring-bom-occurrence-batch.md) | Define Batch K as one 24-capability assembly-authoring, BOM, reference, occurrence-metadata, and review wave | Accepted and implemented |
| [ADR-0073](adr/0073-digital-mockup-interference-clearance-batch.md) | Define Batch L as one 24-capability occurrence-aware interference, clearance, containment, and review wave | Accepted and implemented |
| [ADR-0074](adr/0074-physical-managed-modules-and-shared-native-package.md) | Split managed assemblies/packages with type-forwarded compatibility, one facade, and one shared native runtime package | Accepted and implemented |
| [ADR-0075](adr/0075-interactive-assembly-placement-editing-batch.md) | Define Batch M as one 24-capability interactive presentation and XDE occurrence-placement editing wave | Accepted and implemented |
| [ADR-0076](adr/0076-xcaf-disconnected-step-presentation-style-recovery.md) | Recover disconnected STEP styles into copied XDE snapshots and viewer-native presentation overrides | Accepted and implemented |
| [ADR-0077](adr/0077-iges-xde-metadata-path-interoperability-batch.md) | Define Batch N as one 24-capability metadata-aware IGES/XDE, Unicode-path, viewer, and package wave | Accepted and implemented |
| [ADR-0078](adr/0078-2d-sketch-planar-modeling-batch.md) | Define Batch O as one 24-capability copied 2D sketch, planar topology, feature, exchange, and viewer wave | Accepted and implemented |
| [ADR-0079](adr/0079-surface-uv-curve-on-surface-batch.md) | Define Batch P as one surface/UV, pcurve projection and lifting, seam topology, repair, exchange, and viewer wave; expanded to 32 capabilities by ADR-0080 | Accepted and implemented |
| [ADR-0080](adr/0080-expanded-surface-wave-and-native-source-boundaries.md) | Expand Batch P to 32 capabilities and separate new native source responsibilities while retaining the existing managed modules and one native DLL | Accepted and implemented |
| [ADR-0081](adr/0081-native-source-responsibility-extraction.md) | Extract all historical native source responsibilities, preserving one DLL and the complete existing ABI/ownership contract | Accepted and implemented |
| [ADR-0082](adr/0082-broad-batch-q-through-t-preparation.md) | Prepare Q-T as four 40-capability whole waves with exact-root audits, preserved source boundaries and explicit per-batch baseline revalidation | Accepted; Q-T 160/160 locally validated |
| [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md) | Add U-W and prepare continuous Q-W execution with full per-batch validation/local commits and automatic advancement after explicit start | Active continuous run; 240/280 locally validated |
| [ADR-0084](adr/0084-source-bound-repair-previews.md) | Source-bound copied repair snapshots, selected/protected stages, verified budgets and atomic publication | Accepted and locally validated |
| [ADR-0085](adr/0085-immutable-authored-mesh-revisions.md) | Immutable attributed mesh revisions, exact index correspondence, call-local Poly editing and discrete delivery | Accepted and locally validated |
| [ADR-0086](adr/0086-guided-authoring-laws-constraints-and-provenance.md) | Copied laws, owning guided/constraint graphs, independent residuals and exact provenance | Accepted and locally validated; final packaging/commit in STATUS |
| [ADR-0087](adr/0087-parametric-recompute-and-persistent-selection.md) | Typed parametric DAG execution, atomic publication, durable generation history and fallible persistent selection | Accepted and locally validated; final package-content/commit evidence in STATUS |
| [ADR-0088](adr/0088-source-bound-contour-and-local-feature-programs.md) | Source-bound copied contour programs, explicit local-feature limits and diagnostic-only partial results | Accepted and locally validated; final documentation delivery in STATUS |
| [ADR-0089](adr/0089-exact-partition-and-volume-programs.md) | Exact partition/material programs and bounded volume construction with copied revisions and atomic named outputs | Accepted and locally validated |
| [ADR-0090](adr/0090-viewer-owned-render-resources-and-copied-frames.md) | Viewer-owned rendering resources, explicit asset recipes and copied color/depth frames | Accepted; complete exit evidence in STATUS |
| [ADR-0091](adr/0091-proportionate-development-workflow.md) | Flexible work/commit sizing, proportionate validation, current-state separation and bounded source-size exceptions | Accepted |

## Cadoryx private viewer-source design

[ADR-0102](adr/0102-cadoryx-shared-viewer-sources.md) records source snapshots,
explicit style revisions and preserved instance/selection owners. Frozen .12
Release gates pass; complete A5, Debug, packaging and application acceptance
remain pending.

## Pending decisions

| ID | Decision needed | Required by |
|---|---|---|
| PD-010 | Large test data, Git LFS, and fixture licensing policy | Real-file tests |
| PD-012 | Project license and bundled third-party notice layout | Resolved by ADR-0059: MIT plus runtime-local notices and license texts |

## ADR template

New ADRs should include:

- Title, status, and date.
- Context and constraints.
- Decision.
- Alternatives considered and why they were rejected.
- Consequences and migration impact.
- Validation required.
- Links to related decisions, issues, and documents.

- [ADR-0103: Generated private transfer records](adr/0103-generated-private-transfer-records.md): exact index domains, one transfer epoch, detached records/checks and functional shape binders.

- [ADR-0104: Copied numeric matrices and private solvers](adr/0104-copied-numeric-matrices-and-private-solvers.md): bounded copied matrices, fresh decomposition state and immutable spline factors.

- [ADR-0105: Private spline workspaces](adr/0105-private-spline-workspaces.md): typed curve/surface caches, canonical coefficient copies and atomic interpolation.
- [ADR-0106: Private writer diagnostics](adr/0106-private-writer-diagnostics.md): one writer epoch, copied checks and exact source/map/model domains.
- [ADR-0107: Reproducible local delivery inputs](adr/0107-reproducible-local-delivery-inputs.md): shared native build lock, frozen package input identities and precise SDK exclusions.

- [ADR-0109](adr/0109-generated-copied-curved-hatching.md): TM022 copied curved hatching, gated optional domains and independent 2D/3D trimmed curves.

- [ADR-0110](adr/0110-generated-poly-data-and-controlled-meshing.md): TM023 copied Poly channels/bounds/conditional connectivity and TM024 copied meshing parameters/status with independently owned topology.

- [ADR-0111](adr/0111-generated-document-and-xde-attribute-projections.md): TM025/026 parent-bound document operations, atomic NamedData replacement and copied XDE layer/note projections.

- [ADR-0112](adr/0112-generated-private-exchange-header-sessions.md): TM027 private exchange sessions, copied headers and domain-scoped selections with restored metadata export.

- [ADR-0113](adr/0113-generated-parent-bound-selection-workspaces.md): TM028 copied selecting volumes, strict source stamps, world scene hits and actual live selection repaint.

- [ADR-0114](adr/0114-generated-call-private-foundation-values.md): TM029 bounded call-private Foundation values, UTF16 edit boundaries and proven explicit unit dimensions.

- [ADR-0115](adr/0115-private-filling-kernels-and-controlled-topology-assembly.md): TM030 copied spline/filling workflows and TM031 initialized validated new topology.

- [ADR-0116](adr/0116-private-xde-metadata-and-stepcaf-workspaces.md): TM032 / TM033 exact copied AX workflows.

- [ADR-0117](adr/0117-copied-polygons-and-unambiguous-native-loops.md): TM034 exact copied AY workflows.

- [ADR-0118](adr/0118-audited-foundation-function-diagnostics.md): TM035 exact header-function provenance and call-private copied diagnostics.

- [ADR-0119](adr/0119-initialized-assembly-and-copied-validation.md): TM036 copied geometry graphs, initialized assembly and identity-scoped validation.

- [ADR-0120](adr/0120-bounded-native-curve-computations.md): TM037 private bounded curve sampling, arc length and copied projections.

- [ADR-0121](adr/0121-identity-bound-document-association-diagnostics.md): TM038 exact document/context association diagnostics and atomic edits.

- [ADR-0122: Private identity-bound selected-root transfer](adr/0122-private-identity-bound-selected-root-transfer.md) — Accepted 2026-10-10.

- [ADR-0123: Exact copied mesh boundary diagnostics](adr/0123-exact-copied-mesh-boundary-diagnostics.md) — Accepted 2026-10-10.

- [ADR-0124](adr/0124-finite-copied-numeric-results-and-sdk-supplements.md): Accepted; TM041 finite copied aggregates and hash-pinned SDK include fragments.

- [ADR-0125: copied modeling source diagnostics](adr/0125-copied-modeling-source-diagnostics.md) — Accepted.

- [ADR-0126: coupled intersection domains](adr/0126-coupled-intersection-result-domains.md) — Accepted.
- [ADR-0127: direct collection transactions/persistence](adr/0127-direct-collection-transaction-and-persistence.md) — accepted.
- [ADR-0128: local STEPCAF assembly epochs](adr/0128-local-step-assembly-epochs.md) — accepted.
- [ADR-0129: configured mesh document exchange](adr/0129-configured-mesh-document-exchange.md) — accepted.
- [ADR-0130: private finite parameterized construction](adr/0130-private-parameterized-construction.md) — accepted within the authorized BL–BQ scope.
- [ADR-0131: finite extrema with coupled copied output](adr/0131-private-finite-extrema.md) — accepted within the authorized BL–BQ scope.
- [ADR-0132: bounded text batch publication](adr/0132-bounded-text-batch-publication.md) — accepted within the authorized BL–BQ scope.
- [ADR-0133: private mesh proximity](adr/0133-private-mesh-proximity.md) — accepted within the authorized BL–BQ scope.
- [ADR-0134: private canonical STEP export](adr/0134-private-canonical-step-export.md) — accepted within the authorized BL–BQ scope.

- [ADR-0135](adr/0135-generated-compatible-legacy-adapters.md): TM053 compatible whole-sequence copy, generator-owned legacy adapters and embedded-input provenance within authorized BR–CI.

- [ADR-0136](adr/0136-call-private-coupled-constraint-copies.md): TM054 initialized nested3D/2D constraint copies, bounded enumeration and atomic detached publication.
- [ADR-0138](adr/0138-owned-mesh-resource-import.md): CE nested local mesh resources and native material-owned copied encoded textures; existing flat profile preserved.

- [ADR-0140](adr/0140-public-consumer-copy-history-and-exact-exchange.md): public generator-owned copy/history, initial display and exact STEP/distance consumer support; ABI 1.118 admission.
- [ADR-0141](adr/0141-pinned-vcpkg-dependency-provider.md): pinned independent source dependency recovery and exact provider-specific runtime closure.

# Architecture Decisions

Accepted decisions live in individual ADR files and are append-only in spirit.
Superseded ADRs remain available and point to their replacements.

## Accepted

| ADR | Decision | Status |
|---|---|---|
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

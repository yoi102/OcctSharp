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

## Pending decisions

| ID | Decision needed | Required by |
|---|---|---|
| PD-009 | Canonical API manifest schema and stable symbol ID | Upgrade diff implementation |
| PD-010 | Large test data, Git LFS, and fixture licensing policy | Real-file tests |
| PD-011 | Automated/CI acquisition of the pinned OCCT artifact | Initial CI |
| PD-012 | Project license and bundled third-party notice layout | First package |

## ADR template

New ADRs should include:

- Title, status, and date.
- Context and constraints.
- Decision.
- Alternatives considered and why they were rejected.
- Consequences and migration impact.
- Validation required.
- Links to related decisions, issues, and documents.

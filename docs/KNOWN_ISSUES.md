# Known Issues

## KI-026: Full-selection 16,017-binding wave is not yet compile-accepted

- Status: Open; batch B exit blocker.
- The generated manifest contains 16,017 IDs from a 116,190-declaration selection, but
  the last accepted Release wave remains 6,555 bindings.
- The first native compile exposed collisions between static and shared exports,
  constructor and instance `Create` exports, normalized case variants, and invalid
  ordinary allocation of `BRepMeshData_Curve`.
- Generator fixes reserve `_static_` and `_method_` namespaces, assign ordinals across
  normalized member groups, disambiguate duplicate managed signatures, and implement
  allocator-retaining placement construction. Generator 53/53 passes.
- Full regeneration and native compilation are still `NOT RUN` after those fixes; more
  compile or link gaps may appear. Do not report the 16,017 IDs as accepted coverage.
- The next compile reached the RWGltf family and exposed an incomplete template element
  in an OCCT artifact header. The generator now emits the exact completion header through
  `generatedPreambleHeaders`; the full retry is pending.

Issues remain in this document after resolution, with their status and resolution
evidence updated.

## KI-001: No Git repository initialized

- Status: Resolved
- Severity: Medium
- Area: Repository
- Problem: The current root is not yet a Git repository, so documentation is not yet
  versioned or pushable.
- Resolution: Initialized one Git repository on branch `main` at the outer root on
  2026-08-21. The inner `OcctSharp/` directory is not a nested repository.

## KI-002: OCCT baseline is not selected

- Status: Resolved
- Severity: Blocking
- Area: Dependencies
- Problem: Generation, parsing, compatibility, and runtime work cannot produce a
  reproducible baseline without an exact OCCT version/build identity.
- Resolution: ADR-0004 selects the OCCT 8.0.1 combined VC14 x64 Debug/Release
  distribution. A committed manifest records expected layout and representative hashes.

## KI-003: AST implementation is not selected

- Status: Resolved
- Severity: High
- Area: Generator
- Problem: The architecture requires a semantic C++ AST parser, but the concrete
  Clang/libclang integration and version have not been chosen.
- Resolution: ADR-0006 selects pinned ClangSharp/libClangSharp. Controlled and real
  OCCT header parsing tests pass with deterministic output.

## KI-004: Initial target matrix is not selected

- Status: Resolved
- Severity: High
- Area: Compatibility
- Problem: Initial OS, architecture, compiler, .NET target, and NativeAOT commitments
  are unresolved.
- Resolution: ADR-0004 selects .NET 10, Windows x64, VS 2026/MSVC, and OCCT 8.0.1.
  Other matrices remain not evaluated.

## KI-005: Native ABI error result is not finalized

- Status: Resolved
- Severity: High
- Area: Native ABI
- Problem: Exception containment is required, but the concrete error/result mechanism
  and diagnostic-buffer lifetime are not selected.
- Resolution: ADR-0005 defines stable status values, out parameters, thread-local UTF-8
  diagnostics, and native exception containment for ABI 1.0.

## KI-006: Existing background documents duplicate policy

- Status: Open
- Severity: Low
- Area: Documentation
- Problem: The two original long-form documents repeat architecture and process rules,
  which can drift from topic documents.
- Current mitigation: `docs/DOCUMENTATION_INDEX.md` defines topic documents and accepted ADRs as
  sources of truth while preserving the original documents as background.
- Planned resolution: Consolidate or annotate duplicated sections only after explicit
  approval; do not delete historical guidance silently.

## KI-007: OCCT acquisition is machine-local

- Status: Open
- Severity: High
- Area: Dependencies/CI
- Problem: The validated OCCT bundle exists in a local Downloads directory and is not
  automatically acquired on a clean machine or CI agent.
- Current mitigation: The local path is ignored; a committed manifest records the
  expected version, layout, and representative hashes.
- Planned resolution: Define a licensed artifact source or controlled source-build
  pipeline before CI claims reproducible clean acquisition.

## KI-008: Generator emitter coverage remains narrow

- Status: Open
- Severity: High
- Area: Generator
- Problem: Deterministic native/managed emission now owns 775 stable IDs across
  value-copy scopes, nine generated Geom/Geom2d types, 129 generated StepBasic shared
  types, 61 generated mesh/Poly/analysis/healing types, typed enums, base topology
  values, and checked typed topology casts. Schema 1.6
  additionally reconciles 61 audited modeling declarations as Manual, but accepted binding
  coverage is only 836/16,633 (5.0262%) of the expanded selected dependency closure and
  is not full OCCT coverage. The
  validated shape and exchange bridges remain manual.
- Current mitigation: `TM001`–`TM007`, explicit generated/manual scopes, support diagnostics,
  manifest-aware inventory, and compile/runtime/lifetime tests prevent unknown ownership
  cases from being emitted merely to increase counts.
- Planned resolution: Expand coherent generated package scopes after each required
  value/shared/topology/borrowed lifetime rule is proven, then replace manual raw
  functions only when generated equivalents have equal evidence.

## KI-009: Windows non-ASCII exchange paths are not validated

- Status: Open
- Severity: Medium
- Area: Native file exchange
- Problem: Managed paths are marshalled as UTF-8, while the selected OCCT file APIs
  accept narrow `char*` paths and their Windows non-ASCII behavior has not been proven.
- Current mitigation: Validation and samples use ASCII paths; failures return explicit
  file I/O status and diagnostics.
- Planned resolution: Add non-ASCII path tests and, if required, a controlled temporary
  ASCII-path or stream-based strategy without changing public path semantics.

## KI-010: XDE material placement has an OCCT writer limitation

- Status: Open
- Severity: Medium
- Area: STEPCAF/XDE exchange
- Problem: OCCT 8.0.1 STEPCAF material output is evaluated on top-level part labels.
  A material attached solely to a subshape would otherwise disappear from the written
  STEP relationship.
- Current mitigation: The XDE merge preserves the cloned subshape assignment and also
  promotes it to that input root's part label before export. Material name and density
  records survive, but the original subshape-only placement is not represented with full
  fidelity in the written STEP.
- Planned resolution: Add varied licensed XDE fixtures and revisit the mapping after
  generated document/label bindings and a broader OCCT upgrade matrix exist.

## KI-011: Experimental package is not ready for public publication

- Status: Open
- Severity: High
- Area: Packaging/release
- Problem: Local NuGet creation and clean-consumer execution pass, but the repository
  has no selected project license and does not yet carry a complete reviewed notice and
  provenance set for every bundled OCCT third-party runtime.
- Current mitigation: The package is explicitly experimental and remains under ignored
  local artifacts. It includes the OCCT LGPL 2.1 text and OCCT linking exception and is
  not uploaded by any script.
- Planned resolution: Resolve PD-012 with the user, audit every redistributed DLL,
  include complete notices and provenance, then add CI, SBOM, signing, and publication
  approval gates.

## KI-012: Full OCCT semantic inventory is incomplete with the supplied bundle

- Status: Blocked
- Severity: High
- Area: Generator/dependencies
- Problem: The catalog contains 7,090 public entry headers, but the current OCCT bundle
  semantically scans only 7,058. The 32 isolated failures comprise 19 IVtk headers that
  need VTK headers, ten headers that reference generated OCCT files absent from the
  bundle, one RapidJSON-dependent header, one C++/CLI-only header, and one OpenGL ES
  platform header. Therefore the 116,272 declarations found so far are a partial
  denominator, not full OCCT coverage.
- Current mitigation: The inventory recursively isolates failures, writes a normalized
  partial report, returns a distinct non-zero exit code, and keeps normal generation
  independent. Versioned preamble headers already reduced the initial failure count
  from 58 to 32 without hand-editing OCCT headers.
- Planned resolution: Supply the matching OCCT generated source/header set plus the VTK
  and RapidJSON development headers used by this build. Then define explicit inventory
  profiles for optional IVtk, C++/CLI, and OpenGL ES surfaces and rerun the audit to a
  complete profile-specific denominator.

## KI-013: StepData declarations are not uniformly linkable from the supplied binary

- Status: Open
- Severity: Medium
- Area: Generator/STEP exchange infrastructure
- Problem: Package-level construction of `StepData` found public header declarations for
  `StepData_FreeFormEntity::StepData_FreeFormEntity()` and
  `StepData_UndefinedEntity::Super()` whose symbols are absent from the supplied
  TKXSBase/TKDESTEP import libraries. Treating every `StepData_` record as an ordinary
  entity therefore compiles but fails native linking.
- Current mitigation: `StepData_*.hxx` remains in semantic discovery and classification,
  but `StepData` is not a package-level shared-entity emission scope. No per-class
  blacklist or generated-source edit is used. Seven related STEP entity packages remain
  generated and link-validated.
- Planned resolution: Verify the matching OCCT source/build export configuration and
  design a dedicated StepData interface/session ownership profile before enabling this
  infrastructure package.

## KI-014: Abstract shared-handle classification is incomplete

- Status: Open
- Severity: High
- Area: Generator/shared handles
- Problem: The current semantic model's `IsAbstract` fact does not include every class
  made abstract by inherited pure virtual members. Broadly emitting constructor-less
  handle bases therefore selected `BRepMesh_BaseMeshAlgo` and related types as if they
  were constructible. Nested helper records in `Poly_MakeLoops` also demonstrate that
  name-prefix selection alone is not a valid transient-type test.
- Current mitigation: Package shared-handle scopes require a public, supported
  constructor and close intrusive-handle constructor dependencies only over generated
  target scopes. The failed abstract-base expansion was regenerated away; Release
  native compilation remains clean for the active generated output.
- Planned resolution: Build an inheritance-complete pure-virtual classifier and retain
  only `Standard_Transient` descendants before enabling constructor-less abstract
  shared-handle scopes.

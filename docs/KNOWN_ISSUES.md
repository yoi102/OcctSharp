# Known Issues

## KI-028: Batch F freeform curve/surface authoring closure was not implemented

- Status: Resolved in Preview.3 at 24/24 under ADR-0067.
- Severity: Product gap
- Area: Geom/GeomAPI, BRepBuilderAPI/BRepOffsetAPI/BRepFill, split/repair, STEP/XDE
- Problem: Preview.2 has basic Bezier/interpolate/loft/pipe/sew entry points but does not
  provide the complete rational definition, immutable edit, freeform surface/profile,
  offset/fill/split/controlled-sweep, repair, exchange, and viewer-evidence workflow.
- Resolution: `BATCH_F_FREEFORM_AUTHORING_GAP_INVENTORY.md` locks one dependency and
  ownership closure plus a 24-root/1,122-candidate audit. All 24 capabilities were
  implemented as one wave, 94 direct blocked declarations were reconciled by SC-042,
  and Release/Debug repository runtime plus the clean 62-DLL package consumer pass.

## KI-027: Batch E engineering inspection and PMI closure was not implemented

- Status: Resolved in Preview.2 at 24/24 under ADR-0066.
- Severity: Product gap
- Area: BRepExtrema/BRepGProp, XCAFDimTolObjects, STEPCAF/AP242, PrsDim/AIS
- Original problem: The alpha.55 implementation baseline did not provide the complete
  24-capability exact-inspection, measurement, semantic PMI/reference graph, transactional
  mutation, AP242 GDT/saved-view, viewer-annotation, and screenshot workflow.
- Resolution: Preview.2 implements and validates all 24 capabilities as one Batch E wave.
  Exact inspection, complete PMI/reference mutation, AP242/BinXCAF persistence, saved
  views, four viewer-owned annotation kinds, and real-HWND screenshot output pass in
  Release/Debug repository runtime and the clean 62-DLL package consumer. SC-041 records
  102 direct stable IDs; Generator 91/91, Runtime 119/119, full inventory, compatibility,
  regeneration, runtime identity, provenance, and the local release check pass.

## KI-026: Full-selection 16,017-binding wave was not compile-accepted

- Status: Resolved in the alpha.49 through alpha.55 evidence chain.
- The original 16,017-ID intermediate selection exposed static/instance naming
  collisions, normalized overload collisions, placement-allocation requirements, missing
  template completion headers, and native toolkit-link gaps.
- Resolution: generalized operation namespaces/ordinals, allocator-retaining placement
  construction, configured completion headers, toolkit closure, and narrow long-tail
  dispositions were implemented in the generator and regenerated. The accepted surface
  is now 16,353 emitted stable IDs plus 316 accepted manual stable IDs.
- Evidence: alpha.55 Release and Debug native/managed builds pass; Generator 91/91,
  Runtime 115/115, dependency profiles 6/6, 83-file freshness and byte-identical clean
  regeneration, dependency closure, clean package consumer, inventory, API compatibility,
  provenance, and complete local release gates pass. The obsolete 16,017 intermediate
  count is not current coverage.

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

- Status: Resolved for Sample/package consumption; remains a contributor rebuild input
- Severity: Medium
- Area: Dependencies/CI
- Problem: The validated OCCT bundle exists in a local Downloads directory and is not
  automatically acquired on a clean machine or CI agent.
- Resolution: ADR-0059 commits the verified 62-DLL Windows x64 runtime and exact hashes,
  so an ordinary clone can run the Sample without acquiring an SDK. Generator/native
  rebuilds still require the pinned local or immutable ADR-0051 SDK input.

## KI-008: Generated surface remains selective rather than full OCCT coverage

- Status: Open
- Severity: High
- Area: Generator
- Problem: Deterministic native/managed generation now owns 16,353 stable IDs and the
  accepted friendly/manual layer reconciles 316 additional stable IDs. This is a broad,
  validated selected surface, not full OCCT API coverage. The complete classification
  still contains 49,344 skipped and 50,259 narrowly blocked declarations, while 32 of
  7,090 entry headers cannot be semantically scanned with the supplied optional/artifact
  inputs. A single generated/total percentage would therefore mix different denominators
  and overstate support.
- Current mitigation: `TM001`–`TM007`, explicit generated/manual scopes, support diagnostics,
  manifest-aware inventory, and compile/runtime/lifetime tests prevent unknown ownership
  cases from being emitted merely to increase counts.
- Planned resolution: Expand only newly accepted finite product dependency closures;
  generalize value/shared/topology/borrowed lifetime rules where safe, and replace manual
  raw functions only when generated equivalents have equal compile/runtime/lifetime/
  package evidence. Continue reporting inventory, classification, binding, and validation
  denominators separately.

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
- Problem: Local NuGet creation and clean-consumer execution pass, but package signing,
  hosted release execution, credentials, and NuGet publication authorization have not
  been provided or run.
- Current mitigation: ADR-0059 resolves the MIT project license and commits notices,
  license texts, and SHA256 provenance for the 62-DLL runtime. No script publishes the
  package automatically.
- Planned resolution: Run the hosted release matrix and establish signing/publication
  policy only after explicit owner authorization and credentials exist.

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

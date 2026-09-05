# Known Issues

## KI-035: Viewer rendering and capture semantics depend on OCCT and the driver

- W explicitly validates PBR, MSAA, OIT, anisotropy and path-tracing requests; there is
  no silent quality downgrade. Success tests require an appropriate real OpenGL driver.
- Exposure/white point/filmic mapping affect path tracing, not raster output. IBL needs
  an active ambient light and the PBR view pipeline; disabling IBL restores SDK defaults,
  not necessarily black lighting. Unlit cannot distinguish independent front/back colors.
- Default depth selects the model layer because upper OCCT layers clear depth even
  when empty. Explicit single/through-layer captures disclose their copied scope.
  Depth is normalized buffer data; only capture-time matrices reconstruct world points.
- SDK multilayer dumps compare numeric IDs. W instead masks structures by drawing
  order and restores visibility/depth clears. Explicit FBO ownership guards screenshot
  cleanup and rejects unavailable offscreen buffers; a live HWND/context is still needed.
- Copied RGBA alpha is renderer composite coverage, not a straight-alpha texture.
  The WPF thumbnail uses opaque Bgr32; HwndHost airspace is unchanged. No D3DImage,
  headless service or universal cross-GPU screenshot byte identity is promised.
- Recipe replay resolves all asset keys first but is not a multi-setting transaction.
  A later unsupported setting can leave earlier settings applied; apply to a fresh
  review view when all-or-nothing application-level presentation is required.

## KI-034: Debug shell-draft limits assert in OCCT internal history

- Status: bridge precondition and edge-only history adaptation locally validated
  in Release/Debug and actual Debug-native 409/409. Upstream SDK/DLLs are not patched.
- Evidence: actual Debug-native testhost asserted in TKBRep `BRepTools_History.cxx:165`.
  `artifacts/diagnostics/batch-u-crt.dmp` and `batch-u-crt-stack.log` identify
  `DraftShellExtentAndStopsCreateIndependentShells` -> native ShellDraft. Source
  `BRepFill_Draft::Fuse` queries unsupported sweep-section shapes. Both right and
  round closed-polygon limit probes assert; this is not a catchable OCCT exception.
- Limit-driven modes now explicitly require one analytic line/circle boundary edge,
  identically in Release and Debug. Cornered/multi-edge profiles are rejected before
  the kernel. Length-only cornered profiles remain supported. This is a disclosed
  eligibility restriction, not an upstream fix or a claim of arbitrary shell support.
- `BRepFill_Draft::Generated` also casts every input to Edge. Only source edges are
  queried; no vertex cast, fabricated deletion, or unavailable Modified mapping is used.
- The open straight-edge/unbounded-surface case can finish but produce invalid
  topology. Diagnostics retain that distinction and RequireShape rejects it. Circle
  surface limits and circle/line shape limits have independent numeric success tests.
- `Shell()` is the pre-restriction sweep, not necessarily a final lateral group.
  It is labelled PreLimitShape; final laterals require native-generated face membership.
- No assertion is disabled/ignored, no Boolean fallback, new DLL, or vendored LGPL
  implementation is introduced. Original U-21/22/23 outcomes retain positive geometry
  tests and explicit cornered-limit rejection tests.

## KI-033: Pinned BRepFeat limiter preconditions are not uniform

- Status: exposed and covered by Batch U positive/negative regressions; no SDK patch.
- MakeDPrism UntilEnd derives slanted length from the largest base-box dimension.
  On a cube it can stop short of the far cap and throw a map-lookup failure. An
  adequately wide base succeeds. The bridge returns diagnostic failure, not a fallback.
- Limited MakePipe can fail converting untrimmed Geom_Line to BSpline. The supported
  success fixture uses a bounded Bezier spine; a line-spine failure has an explicit test.
- Planar stop faces may denote unbounded support surfaces. Revolved limits use native
  base-side selection; they are not guaranteed to stop at the first positive angle.
- Public options and ADR-0088 disclose these semantics; no feature may silently replace
  a failed local builder with a global Boolean.

## KI-032: OCCT 8.0.1 fillet Law_Function radius setter loses the supplied law

- Status: U workaround locally validated, including actual Debug-native 409/409.
- Severity: High (native access violation).
- Evidence: `artifacts/batch-u-first-runtime.log` records a test-host `0xC0000005`
  abort in nonconstant-law fillet simulation. In the pinned SDK's source,
  `ChFiDS_FilSpine::SetRadius(Law_Function, ...)` populates only a temporary composite
  then clears the persistent radius sequence. Compute also clears post-simulation laws.
- Workaround: source-bound copied laws become checked native sample programs, with
  explicit interpolation/probe diagnostics and per-edge global arc-length mapping.
  Do not retry the unsafe setter or present sampled agreement as an exact/global bound.
- Decision: [ADR-0088](adr/0088-source-bound-contour-and-local-feature-programs.md).

## KI-031: OCCT 8.0.1 per-constraint filling residual getters corrupt temporary storage

- Status: resolved in the Preview.18 bridge; final whole-batch evidence is in STATUS.
- Severity: High; native heap overwrite/uninitialized residual read.
- Evidence: an earlier focused 40/40 pass was followed by intermittent failure in
  `artifacts/batch-s-repeat-8.log`; the pre-fix isolation run also crashed later.
- Cause: GeomPlate_BuildPlateSurface per-index G0Error/G1Error/G2Error allocate the
  initial per-curve count, while EcartContraintesMil writes refined curve intervals.
  The exposed index space also does not accept point constraints.
- Resolution: do not call these SDK getters. Independently measure position, normal
  angle and curvature-tensor residuals on the final approximated surface with explicit
  bounded samples and derivative availability. The three unsafe overloads stay Blocked.
  No SDK DLL/source patch or disabled constraint-acceptance test is used.
- Regression: twelve consecutive 40-case runs and ten expanded 44-case runs pass;
  each expanded run includes 48 low-sampling/high-iteration G2 lifetime solves. This
  is evidence for the exercised bridge path, not a claim that upstream OCCT is fixed.

## KI-030: Debug native history queries asserted for container topology

- Status: Resolved in Preview.14; final Release-native and Debug-native Runtime 164/164 pass.
- Area: Feature/Boolean/freeform copied history.
- Reproduction: The Debug OCCT TKBRep runtime asserted in BRepTools_History.cxx:165
  (`IsSupportedType(theInitial)`) during Batch J split/defeature/cell/recovery tests.
  Catching Standard_Failure cannot contain this CRT assertion.
- Resolution: Native history queries accept only vertex, edge, face, and solid inputs,
  matching OCCT's contract. Feature and freeform container workflows inspect supported
  descendants; basic container-kind summaries keep source counts without unsupported
  change/deletion queries. No borrowed history data or new ownership category is exposed.
- Regression: ContainerHistoryUsesSupportedDescendantsWithoutNativeAssertions exercises
  a compound Boolean, wire-kind history, and freeform splitting under both native builds.

## KI-029: IGES exchange is geometry-only and does not preserve XDE metadata

- Status: Resolved in Preview.13 at 24/24 under ADR-0077
- Severity: Medium
- Area: IGESCAF/XDE exchange and WPF viewer
- Original problem: The managed IGES path used `IGESControl` and returned one owning shape. It did
  not project IGES names, colors, layers, or visibility into an owned XDE document, so
  the WPF sample displays IGES with the neutral fallback color.
- Resolution: Preview.13 implements ADR-0077's complete metadata-aware IGES/XDE read/
  import/write, independent name/color/layer options, copied diagnostics/units, mixed-
  format composition, XDE-label viewer display, round-trip, lifetime, Unicode paths, and
  clean-package evidence. Focused Batch N 4/4 and full Release/Debug Runtime 156/156 pass.

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
  accepted friendly/manual layer reconciles 557 additional stable IDs. This is a broad,
  validated selected surface, not full OCCT API coverage. The complete classification
  still contains 49,344 skipped and 50,018 narrowly blocked declarations, while 32 of
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

- Status: Resolved in Preview.13 under ADR-0077
- Severity: Medium
- Area: Native file exchange
- Original problem: Managed paths were marshalled as UTF-8, while the selected OCCT file APIs
  accept narrow `char*` paths and their Windows non-ASCII behavior has not been proven.
- Resolution: ASCII paths retain their direct behavior. Non-ASCII input is copied to a
  unique ASCII staging file; non-ASCII output is written to staging and promoted only
  after success. Preview.13 tests successful read/write, failure and exception cleanup,
  diagnostics retaining the public path, package consumption, and round-trip behavior.

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

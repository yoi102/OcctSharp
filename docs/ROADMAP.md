# Roadmap

The roadmap is outcome-based. A phase completes only when all exit criteria have
evidence; creating files or generating a large method count is not sufficient.

The retired B00–B20 capability milestones and future package boundaries are documented
in [the complete migration plan](MIGRATION_PLAN.md). They all belong to the completed
product-scale batch `B`; none is a separate current batch or commit boundary.
Batch B is complete. ADR-0060 opens Batch `C` as the active common-CAD-API product batch.

## Current priority: Batch C common CAD workflows

The active roadmap is one large product outcome: make the routine CAD path usable from
.NET without unmanaged escape hatches. Work advances model/inspect, build/modify/deliver,
and present/interact coverage together. These are lanes inside C, not phases or batches.

Priority order inside each large wave:

1. High-frequency end-to-end workflow gaps and generalized generator/type-map/ownership
   rules that unlock several related API families.
2. Complete family semantics: overloads, options, status/diagnostics, ownership,
   disposal, failure paths, bulk transfer, and friendly composition.
3. Runtime, integration, real-file, viewer, and package evidence for the whole workflow.
4. Only then additional cold schema entities, optional integrations, or low-frequency
   infrastructure declarations.

The first active wave is common solid editing and inspection across geometry/topology
queries, BRep construction/modification, mesh extraction, STEP/XDE round trip, and viewer
presentation/selection. It is not split into per-class or numbered tasks. See
[the complete migration plan](MIGRATION_PLAN.md) for the exact Batch C contract.

The phase record below is retained as Batch B history and architecture context; it is not
the active execution sequence for C.

## Phase D0: Documentation and decisions

Goal: establish one coherent source of truth before implementation.

Tasks:

- [x] Record repository/code separation.
- [x] Record C ABI and canonical binding model decisions.
- [x] Establish architecture, generation, ABI, ownership, type-map, test, version,
  compatibility, issue, and status documents.
- [x] Select the exact OCCT baseline and dependency acquisition strategy.
- [x] Select the AST front end and parser toolchain.
- [x] Select the initial platform/compiler/.NET matrix.
- [x] Decide the initial generated-output commit and raw naming policy.
- [x] Decide the initial NuGet package and native-runtime delivery policy.

Exit criteria:

- All decisions required for the Phase 0 skeleton are accepted ADRs.
- `STATUS.md` contains no unowned Phase 0 blocker.

## Phase 0: Reproducible foundation

Goal: create a buildable workspace and prove one managed-to-OCCT call.

Planned outcomes:

- [x] One Git repository at the outer root.
- [x] Inner solution and native CMake workspace.
- [x] Pinned dependency and toolchain manifests.
- [x] CI skeleton for the initial platform, including clone-only bundled-runtime smoke.
- [x] Minimal manually written C ABI probe with ABI/build version query.
- [x] Managed invocation of the probe with clear load diagnostics.
- [x] OCCT-backed box creation and topology face-count runtime path.

Exit criteria:

- Clean checkout can restore dependencies and build using documented commands.
- Native and managed smoke tests pass on the initial CI target.
- Loaded OCCT/native identity is reported and matches the pinned input.

## Phase 1: Generator skeleton

Goal: parse controlled C++ plus the selected OCCT scope into a canonical model and
generate a small deterministic native/managed binding set.

Planned outcomes:

- [x] AST front end with explicit compiler arguments.
- [x] Versioned discovery configuration and initial binding-model schema.
- [x] Initial primitive, enum, and `gp_Pnt` projection rules.
- [x] First selected simple constructor emission (`gp_Pnt` value copy).
- [x] Generalized value-copy eligibility for simple constructors and static methods.
- [x] Generalized native/managed emission for configured `gp_Pnt`, `Precision`,
  `TopAbs`, `Standard`, `TopLoc`, and `gp` value-copy scopes, with deterministic
  overload naming and explicit exclusion of side-effect-sensitive `Standard::Purge`.
- [x] Initial ordered support classification and stable skip reasons.
- [x] Initial native, managed, and generated-file manifest emitters.
- [x] Package/toolkit coverage and per-declaration diagnostic emitters.
- [x] Separate full-header catalog and failure-isolating batched semantic inventory.
- [x] Staged output replacement and manifest-owned stale-file removal.

Exit criteria:

- Regeneration is byte-stable for the same normalized inputs.
- Generated native and managed outputs compile.
- Discovery totals and every skip reason are reported.

## Phase 2: Lifetime foundation

Goal: establish safe ownership semantics before expanding coverage.

Planned outcomes:

- Opaque typed handle and ABI error contracts.
- `Handle<T>` and `Standard_Transient` semantics.
- `TopoDS_*` representation and subtype/copy behavior.
- Candidate small `gp_*` value mappings.
- Disposal, invalid-handle, cast, parent/child, and exception-path tests.
- [x] Native live-handle registry and invalid-handle diagnostics for the owning shape category.
- [x] Experimental `Standard_Transient` shared-handle probe with clone/null/reference-count tests.
- [x] Shared-handle runtime type identity and base-kind checks through OCCT RTTI.
- [x] Checked derived shared-handle cast with explicit type-mismatch status.
- [x] `TM006` plus the first generated real typed shared handle (`Geom_CartesianPoint`).
- [x] `TM007` plus generated base `TopoDS_Shape` copy/null/kind/orientation/reversal
  and partner/same/equal semantics.
- [x] Typed topology hierarchy and checked conversions for the eight `TopoDS_*` value
  wrappers (B04).
- [x] B05 opaque transformation values: `gp_Trsf`, `TopLoc_Location`, `gp_Vec`, `gp_Dir`,
  `gp_Ax1`, and `gp_Mat` use registry-validated opaque handles with clone, validation,
  composition/conversion, matrix access, and topology placement/transform entry points.
  B05 is deliberately closed as one coarse ownership batch; these manual bridges remain
  pending replacement by generalized generated value rules.
- [x] B06 foundation strings/collections: UTF-8/UTF-16 OCCT strings,
  `NCollection_Sequence<double>`, `NCollection_Array1<double>`, and the OCCT 8
  dynamic-array-backed `NCollection_Vector<double>` alias use explicit opaque
  buffer/index contracts; integer-key maps use caller-owned snapshot buffers and no
  native iterator crosses the ABI.
- [ ] Broader memory diagnostics and stress tooling.

Exit criteria:

- Ownership model validation rejects unknown unsafe cases.
- Required lifetime stress tests pass with no known critical lifetime defect.

## Phase 3: STEP closed loop

Goal: prove one useful, end-to-end CAD workflow.

Planned outcomes:

- [x] Box or equivalent shape creation.
- [x] Topology traversal.
- [x] Manual STEP geometry write and read.
- [x] Local real STEP fixture workflow; commit licensing remains PD-010.
- [x] Interim friendly API for the selected geometry-only closed loop.

The manual path satisfies the requested runnable samples but does not complete Phase 3:
generated bindings and packaging still do not participate in this workflow.

Exit criteria:

- Generator, native ABI, raw managed layer, friendly API, packaging layout, lifetime,
  and real-file tests all participate in the workflow.
- Claims are limited to ordinary STEP geometry unless STEPCAF/XDE is separately tested.

## Phase 4: Modeling and geometry expansion

Goal: generalize rules across selected geometry and BRep packages.

Planned outcomes include geometry/adaptors, BRep construction, transformations,
boolean operations, and module-level coverage reporting.

Current evidence includes the complete B08 safe profile: owned `GProp_GProps` property
accumulators and value-copy `BRepAdaptor_Curve`/`BRepAdaptor_Surface` snapshots. General
borrowed adaptor objects and underlying curve/surface handles remain outside this profile.
The B11/B12 basic algorithm profiles are also complete for owning/value results without
cross-ABI history: Fuse/Common/Cut, minimum distance, ShapeFix, and same-domain unification.

Exit criteria are set after Phase 3 evidence identifies the next safe scope.

## Phase 5: Mesh and bulk transfer

Goal: support triangulation without chatty interop.

Planned outcomes include bulk vertex/index/normal transfer, correctness tests, and
native-versus-managed benchmarks before any zero-copy design.

## Phase 6: XDE and metadata

Goal: support document/assembly workflows and preserve names, colors, layers, and
materials through explicitly tested STEPCAF/XDE paths.

Initial evidence: a manual, one-shot XDE assembly operation is validated in ABI 1.2.
It is deliberately outside the generated binding scope and does not complete this phase;
document/label ownership, generated bindings, broader metadata fixtures, and lifecycle
tests remain required.

## Phase 7: Visualization

Goal: add viewer, selection, window integration, callbacks, and thread-affinity rules
after core ownership and platform contracts are stable.

B17 completes the Windows visualization-core profile: an application-owned HWND is
bound to a thread-affine OpenGL/V3d/AIS owner, presentations are parent-bound IDs,
selection is copied, and applications explicitly forward input/resize events. Native
callbacks and broad generated visualization declarations remain B19 long-tail scope.

## Phase 8: Distribution

Goal: publish validated NuGet packages for the declared compatibility matrix.

Implemented evidence: one experimental package now carries the managed assembly and
the complete Windows x64 native closure, copies it below the consumer's `occt` directory,
and passes a clean restore/publish/runtime consumer. The release workstream also implements
API baselines, clean-source regeneration, immutable-artifact CI configuration,
SBOM/provenance/checksums, release notes, and explicit machine-readable gates. MIT,
bundled notices, clone-only hosted CI, and the committed runtime pass. This does not
authorize signing or NuGet publication.

The former B00-B20 plan is the completed product-scale migration batch `B`. Foundation,
generated sharing, modeling, classification, and release engineering are historical
capability milestones inside B, not active batches.

Batch C now prioritizes the common workflow matrix defined by ADR-0060. Numbered or
dotted fragments, per-class batches, and small completion percentages are not used.
Focused checks may run during implementation, but full Release/Debug/package/inventory
evidence is amortized at a coherent large-wave checkpoint rather than repeated after
each method.

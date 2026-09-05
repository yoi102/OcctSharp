# ADR-0086: Guided authoring with copied laws, constraints and provenance

- Status: Accepted and implemented; 40/40 local capability validation passes; final packaging/commit evidence in STATUS
- Date: 2026-09-05

## Context and entry

Implement all unchanged 40 S capabilities after R's `86e069c` completion commit.
The separate entry config pins Preview.17 and inventory SHA256
`4E90AB503456D7617CE81E21116CBAA0119042B2E63EEAD9A5C06CD20DE807E6`.
The original Preview.15 configuration stays frozen. The exact delta is 154 prior
Blocked-to-Manual IDs, 21 within S roots, and zero identity/addition/removal changes.
Repeated audits agree at `71D65197222B212B36D6FDC8D11ECD9D4E35A7ADB510A2D4923D4AB0DAAFD9CD`:
52 roots, 2,432 candidates (1,042 Blocked, 611 Emitted, 179 Manual, 600 Skipped).

## Decision and source boundaries

Geometry owns immutable scalar laws and copied samples. Modeling owns guided sweep,
loft and constrained-fill definitions/results. Existing facade Freeform definitions
retain their public assembly identity; conversion and XDE/viewer composition using
them stay in the facade. No reverse dependency or new managed project/Native DLL.
Use cohesive ScalarLaws/GuidedSweep/GuidedLoft and constrained-fill/conversion units,
not the historical Freeform monolith. Native algorithms and OCCT law handles stay
call-local; caller buffers carry copied numeric data. Law_BSpline is not Law_Function:
Law_BSpFunc is an explicit dependency refinement, not a widened frozen root audit.

Map domains by transforming the actual leaf parameters/knots and chain-rule endpoint
derivatives; trimming retains the original definition and limits its active domain.
Composite laws retain ordered spans and expose join discontinuities. Sampling bounds
are not a proof of global positivity. A positive B-spline control hull can supply a
conservative global bound; a sweep's scale acceptance must state which policy it uses.
No arbitrary procedural callbacks or managed virtual proxies are introduced.

Sweep plans copy input topology into a single dependency graph, preserving relationships
between spine vertices, profile subshapes and supporting face pcurves. Independently
deep-copying related edge/face arguments is not an acceptable isolation strategy.
Exact copy/history mappings identify provenance; counts, nearest geometry and native
addresses cannot substitute for identity. The same applies to constrained filling.
Plans own their private source snapshots, reject use after disposal and outlive the
original arguments. Results contain independent owning Shapes and copied diagnostics.
Reuse the existing bounded FeatureResult native lifetime for temporary result extraction;
do not add a registry/allocator family. No borrowed shape or native algorithm escapes.

Auxiliary guide and homothetic law conflict before build. KeepContact reports C0 only.
Support framing requires actual pcurve support for every spine edge. Simulation exposes
the SDK's equally spaced section count, not arbitrary requested stations. Algorithm
status, approximation error, topology validity and requested/achieved continuity are
separate facts. Failed solidification preserves a valid shell only under explicit policy.

Filling uses per-constraint IDs, boundary/interior distinction, support face/UV inputs,
seed surface and bounded solver controls. IsDone is not fulfilment. Validate each
required constraint's applicable residuals; unavailable/ignored residuals cannot pass
acceptance. Derivative/curvature singularities are reported, never converted to zero.
Do not use OCCT 8.0.1's per-index G*Error getters: their initial-size temporary
buffers are incompatible with refined curve samples. Verify the final surface with
bounded independent position, normal and curvature-tensor samples, explicitly not
a global error proof. The three unsafe overloads stay Blocked. Temporary graph
assembly restores source TShape.Free; compatible loft wires and their generated
maps come from an explicit BRepFill_CompatibleWires operation.
Bezier pieces/patches are copied with parameter-span provenance. Recipe references stay
in OCAF; STEP/IGES promises cover tested geometry/metadata, not arbitrary recipe data.

## Alternatives and consequences

Avoid persistent native builder handles and extra registries; immutable snapshots make
source isolation and failure atomicity explicit. Retain existing public Freeform types
rather than moving them into a lower assembly. Do not infer exact history from geometric
proximity or treat a requested constraint as satisfied merely because it was added.
Preview.18 is reserved. Actual C contract additions increment ABI/bridge; schema and
assembly identity change only if required. Record direct manual calls under SC-056.

## Required validation

All 40 named acceptance rows, positive and malformed laws, chain-rule derivatives,
supported frame/contact success and incompatible-mode failure, actual simulation,
history/end sections/solidification, constrained G0/G1/G2 success and ignored-constraint
failure, copied conversion/continuity singularities, source/result lifetime loops,
real XDE/STEP/IGES/HWND workflow, strict standalone private headers, source/dependency
closure, exact manual accounting, additive API/ABI, Release/Debug/actual Debug-native,
both clean consumers, clean regeneration, runtime/package bytes and full local gates.
Only then commit S locally and immediately proceed to T without routine confirmation.

Validation: focused 44/44 and ten repeats; Release/Debug Generator 91/91 and Runtime
273/273; isolated actual Debug-native 273/273; 39 strict private headers, source
and exact 68-ID accounting, additive compatibility, both consumers, 94-file clean
regeneration and full local release-check pass. Package content/provenance and local
commit are recorded in STATUS and the continuous journal. No scope row was removed.

Related: [S matrix](../BATCH_S_GUIDED_SWEEP_CONSTRAINED_SURFACE_GAP_INVENTORY.md),
[continuous runbook](../BATCH_CONTINUOUS_EXECUTION.md), ADR-0074, ADR-0081, ADR-0082, ADR-0083.

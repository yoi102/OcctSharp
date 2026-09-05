# ADR-0078: Implement 2D sketch and planar modeling as Batch O

- Status: Accepted and implemented
- Date: 2026-09-03
- Scope: Batch O product denominator, 2D geometry, planar topology/features, ownership, and validation

## Context

OcctSharp can author rich 3D/freeform topology, but applications still lack one coherent
2D sketch contract. Generated `Geom2d` wrappers expose selected intrusive objects while
common `gp_*2d` values, mixed analytic/freeform definitions, copied projection and
intersection results, loop diagnostics, hole-aware faces, and profile features remain
fragmented or inaccessible from the friendly API.

Adding individual points, lines, or circles would not close a CAD workflow. The useful
boundary starts with copied 2D definitions in an explicit plane and ends with owning
topology, XDE/exchange preservation, viewer evidence, and package consumption.

## Decision

Open Batch O as the indivisible 24-capability wave in
`BATCH_O_2D_SKETCH_PLANAR_MODELING_GAP_INVENTORY.md`.

Keep all OCCT 2D values, curves, builders, adaptors, projectors, intersectors, wire
explorers, classifiers, and feature algorithms native-local. Managed code receives only
immutable copied definitions/solutions/diagnostics, explicit plane values, and independent
owning topology results. Existing generated `Geom2d` wrappers remain available, but the
friendly Batch O contract does not retain or mutate them.

Compose this boundary with existing `Shape`, freeform, XDE, STEP/IGES, and viewer APIs.
Loops and hole nesting are validated before face/feature creation; XDE labels and viewer
objects retain their established parent/thread lifetimes. Only exact blocked declarations
directly called by the implementation may enter SC-052.

The additive wave targets package `8.0.1-preview.14`, native ABI 1.58, bridge 0.66.0,
and schema 1.13. It retains ADR-0074's managed module graph, facade compatibility, one
native bridge, and the shared 62-DLL runtime package.

## Alternatives considered

- Extending only generated `Geom2d` coverage was rejected because mutable shared objects
  do not provide the copied definition, loop, topology, feature, and failure contract.
- A point/line/circle mini-batch was rejected because it cannot create or validate a
  mixed closed profile or produce a solid.
- Implementing a managed parametric constraint solver was rejected because OCCT 8.0.1
  does not supply that complete product boundary and it would dominate this migration wave.
- Reusing 3D edges without an explicit plane/2D inspection model was rejected because it
  loses curve parameters, local coordinates, and deterministic planar diagnostics.
- Splitting 2D geometry into another managed or native module was rejected because the
  accepted Geometry/Modeling/facade ownership graph already represents the dependency.

## Consequences

- Batch O implements all 24 capabilities following a 24-root, 849-candidate baseline audit.
- Curves and algorithms remain native-local; definitions and diagnostics are copied;
  topology and features are independent owners.
- Parametric constraints, DXF/DWG, D3DImage, cross-platform rendering, and physical
  native splitting remain explicit non-goals.
- Batch B-N evidence is immutable and is not revised by Batch O progress.

## Validation required

The complete 24-row matrix; exact SC-052 accounting; value/array/tolerance validation;
analytic/freeform numeric fixtures; source-disposal and multi-result ownership; mixed
loop, self-intersection, nesting, hole, face, extrusion, revolution, and add/cut behavior;
real XDE plus STEP/IGES; real HWND selection/screenshot; clean facade/direct-module
consumers; Release/Debug; generator/runtime suites; deterministic regeneration;
compatibility/inventory/runtime hashes; SBOM/provenance/checksums; documentation; and
Git whitespace gates.

Preview.14 implements this decision. Focused Batch O tests pass 7/7, Generator 91/91,
and Runtime 164/164 against both Release and Debug native bridges. SC-052 reconciles
52 exact directly used blocked stable IDs. The 94-file clean regeneration, real XDE/
STEP/IGES/HWND workflow, and local release checks pass; STATUS records the final package
refresh, hashes, and commit boundary. Publication is excluded from batch delivery.

## Related decisions

- ADR-0002: fixed native C ABI.
- ADR-0039: owning edge/wire/face builders.
- ADR-0046: HWND/thread-affine viewer ownership.
- ADR-0052: native-local algorithms and exact stable-ID accounting.
- ADR-0054: whole-letter product batches and no numbered fragments.
- ADR-0067: copied freeform definitions and owning profile-to-solid results.
- ADR-0074: managed modules and one shared native package.
- ADR-0077: STEP/IGES format-neutral exchange and Unicode path staging.

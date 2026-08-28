# ADR-0063: Close Batch C with selective sessions, owning topology edits, and parent-bound input

- Status: Accepted
- Date: 2026-08-29
- Scope: Final Batch C common API dependency closure and ownership boundaries

## Context

The first three Batch C waves covered common topology/mesh/BREP/XDE/viewer controls,
import diagnosis and repair, and validation properties/occurrence composition/STEPCAF
options. The remaining high-frequency gap was not one class: applications still needed
selective STEP transfer, curve/surface/pcurve inspection, bounded topology construction
and editing, bidirectional adjacency, subshape selection, and ordinary window input in
one workflow.

The native OCCT objects behind those capabilities have incompatible lifetimes. Adaptors,
builders, and reshapers are call-local; a STEP reader must live across several calls;
AIS presentations and input are parent/viewer/thread-bound; selected topology must not
escape as an AIS-owned borrowed reference.

## Decision

Complete Batch C through one API/ABI/package checkpoint with these boundaries:

- Copy curve derivatives, surface derivatives, and 2D pcurve values into fixed ABI records.
- Keep trim builders, wire builders, and `BRepTools_ReShape` native-local and return new
  registered owning shapes.
- Own a parsed `STEPControl_Reader` behind `StepReadSession`; copy its unit metadata and
  return an independent owning shape for each selected root transfer.
- Keep one managed assembly, one native DLL, and the ADR-0061/0062 source shards. This
  closure does not authorize physical project or DLL splitting.
- Keep selection modes and `ViewerInputController` parent-bound to `OcctViewer` and its
  creation thread. Copy every selected whole/subshape topology into a new owning shape.
- Reconcile only the 17 newly direct blocked declarations through SC-039. Existing
  emitted or previously reconciled declarations remain in their original accounting.

## Alternatives considered

- Exposing adaptors, curves, builders, reader work sessions, or AIS owners as raw managed
  handles was rejected because their borrowed/session/parent lifetimes differ.
- Reopening and retransferring the STEP file for each selected root was rejected because
  it loses the purpose and state boundary of a reader session.
- Returning selected presentation IDs without topology was rejected because subshape
  editing workflows require an independently owned selected shape.
- Installing native window callbacks was rejected because application frameworks already
  own their event loops; explicit forwarding keeps teardown and thread ownership clear.
- Physically splitting managed projects or native DLLs was rejected for the compatibility
  and cross-DLL allocator/registry reasons established by ADR-0062.

## Consequences

- ABI 1.45 and bridge 0.53.0 are additive, and package identity advances to alpha.54.
- Session, call-local, parent-bound, thread-affine, copied-value, and owning-shape contracts
  remain distinct in the public API.
- Batch C has a finite 15-capability exit denominator and a single real STEP
  import-edit-export-viewer integration workflow.
- Advanced filters, custom rendering pipelines, low-frequency schemas, and exhaustive
  mesh attributes remain outside Batch C; they do not keep the common API batch open.

## Validation

- Release and Debug native/managed builds pass with 0 errors; Generator 91/91,
  Runtime 114/114, and dependency profiles 6/6 pass.
- Invalid range, wrong kind, non-member edit, duplicate/invalid root, disposed session,
  disposed viewer, cross-thread input, and selected-shape ownership paths are exercised.
- A real STEP file passes selective import, topology edit, export, re-read, display, face
  selection, and owning selected-shape snapshot validation.
- Generated freshness and byte-identical regeneration pass for 83/83 files. The clean
  alpha.54 62-DLL package consumer, full inventory, API compatibility, bundled runtime
  hashes, SBOM/provenance, and local release gates pass.

## Related decisions

- ADR-0046: HWND/thread-affine viewer and presentation IDs.
- ADR-0059: committed Windows runtime and MIT license.
- ADR-0060: one common-CAD-API Batch C.
- ADR-0061: generated domain/layer source partition.
- ADR-0062: generated cross-shard closure and deferred physical splitting.

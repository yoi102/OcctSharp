# ADR-0079: Implement surface UV and curve-on-surface workflows as Batch P

- Status: Accepted and implemented
- Date: 2026-09-05
- Scope: Batch P denominator, projection/lifting, seam topology, repair, and ownership

## Context

Preview.14 completes Batch O's copied 2D sketch and planar-feature boundary. The earlier
Shape and Batch F APIs already evaluate surfaces and author freeform topology, but they
do not close a hole-aware, periodic, seam-preserving UV curve workflow. A nearest point
projection is not a full multi-solution/domain contract; an edge pcurve snapshot is not
a complete copied curve definition or both branches of a seam.

## Decision

Freeze the 32 capabilities in
[the Batch P gap inventory](../BATCH_P_SURFACE_UV_CURVE_GAP_INVENTORY.md) as one wave.
Preparation audits 32 exact roots and 1,178 declarations over the hash-pinned Preview.14
inventory: 608 blocked, 204 emitted, 41 manual, and 325 skipped.

Compose existing Shape, freeform, sketch, XDE, exchange, and viewer APIs through copied
surface/UV records and a cross-family facade. Native adaptors, projectors, classifiers,
interpolators and healers remain call-local. Face locations, orientation, bounded/native
UV parameters, singularity flags, approximation diagnostics, and seam branch identities
are explicit. Pcurve and 3D geometry consistency must be verified, not inferred.

Snapshots retain no native parent; returned topology owns registered wrappers. Repair
must copy underlying geometry/topology before mutation, because independent wrappers can
share TShape or geometry. Multi-result failure releases all partially created owners.
XDE labels and viewer objects retain document-parent and viewer-parent/thread lifetimes.

Only exact newly invoked blocked stable IDs, including support classes outside the root
audit, may enter SC-053 after implementation. Existing Manual/Emitted/Skipped attribution
is unchanged. Reserve Preview.15/ABI 1.59/bridge 0.67.0/schema 1.13, but do not change
current runtime or package identity during preparation. Preserve ADR-0074's twelve
managed modules, facade, one native DLL and shared runtime package.

## Alternatives considered

- Reimplementing existing scalar evaluation was rejected: it adds duplicated contracts
  without closing seam, domain, representation or repair behavior.
- Treating UV bounds or a sampled polygon as the face domain was rejected: holes,
  periodic seams and singularities require native geometry/topology classification.
- Exposing mutable native surface/pcurve handles was rejected: it bypasses the copied
  data and independently owning result contract and makes repair affect shared inputs.
- Point/projection/seam/repair mini-batches were rejected: they cannot complete the
  surface-supported authoring and exchange workflow.
- A new managed project or native DLL was rejected: the existing closed module graph
  suffices and cross-DLL ownership has not been established.

## Consequences and migration impact

Preparation was completed before implementation. Batch P now implements the expanded
32-capability scope under ADR-0080; final validation and identities are recorded in STATUS.
The complete dependency closure, baseline reuse and actual preparation evidence are
recorded in the gap inventory. A repeatable audit rejects silent baseline drift.
General UV atlases, geodesics, constraints, native splitting and renderer work remain
out of scope. Batch completion means local validation and commit, not NuGet publication.

## Validation required

All 32 rows must pass together across analytic/freeform, located/reversed, holed,
periodic/seam and singular fixtures. Include copied/owning lifetime, failed-result
cleanup, unchanged input topology after repair, exact SC-053 reconciliation, Release/
Debug and a real native Debug test run, STEP/IGES metadata, real HWND, both clean package
consumers, generation/compatibility/inventory/runtime and package hashes, release
metadata, documentation, and Git whitespace. All local implementation gates now pass:
32/32 capabilities, focused 13/13, Generator 91/91, Runtime 177/177 in Release/Debug
and an actual Debug-native sweep, clean consumers and 94-file clean regeneration.
STATUS and the gap inventory record final identities, hashes and validation boundaries.

## Related decisions

- ADR-0002 and ADR-0052: fixed C ABI and native-local algorithms.
- ADR-0039 and ADR-0063: owning topology and copied surface/pcurve inspection.
- ADR-0067: immutable freeform definitions and owning surface/split results.
- ADR-0074: managed modules and one shared native package.
- ADR-0077: XDE/STEP/IGES metadata and path staging.
- ADR-0078: copied 2D curve definitions and planar topology.

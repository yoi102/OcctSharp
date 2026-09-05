# ADR-0093: Broad generated geometric value projections

- Status: Accepted and implemented; actual validation is recorded in STATUS.
- Date: 2026-09-06.
- Entry: Batch X `2cbe55e`, Preview.23; 19,682 generated and 1,217 manual IDs.

## Decision

Batch Y adds a central TM009 table for copied geometric value parameters and returns:
2D/3D coordinates, vectors and directions; axes and coordinate systems; matrices and
quaternions; lines, conics, planes and elementary analytic surfaces. Value and const
lvalue-reference signatures may use this projection. Mutable/output references,
pointers, arrays and general transformation internals remain outside this scope.

Generate explicit C records and immutable blittable managed records in OcctSharp.Values,
owned by the existing Foundation module. Use OCCT constructors/accessors to translate
each field; never reinterpret C++ object storage. A reference return is copied during
the call and survives receiver mutation/disposal. Input scalars must be finite, native
constructors validate domain restrictions, and complete coordinate-system values must
preserve orthogonality and handedness. Invalid frames reject rather than silently losing
orientation. All conversions run inside the existing checked native exception boundary.

Existing opaque/friendly facade geometry types and their assembly identities remain.
The new lightweight records serve generated signatures without moving facade types or
adding a reverse module dependency. A scalar value type's ordinary native constructors
are not automatically counted as emitted merely because its parameters/returns map.

Extend immutable overload epochs with Preview.23's added IDs. Keep Preview.22 ordering
first, X ordering second and Y additions third. The epoch used to preserve ordinals is
separate from the pre-X direct-return ABI distinction: X static functions stay checked.

## Accepted outcome and validation

Implement the reusable mapping table and native/managed emitters across all eligible
selected modules; audit returned references and exact link exceptions. Require geometry
round trips, derivatives/geometry operations through the newly available value inputs
and returns, invalid input/error containment, copied-return lifetime, and both fresh
facade/direct-module consumers. Record exact manifest and full-inventory transitions,
including reason refinements that remain Blocked and do not count as migration.

Preserve every old generated native signature/body and managed public signature. Run
Release/Debug builds and suites, actual Debug-native runtime, strict C record layout,
deterministic clean regeneration, dependency closure and the complete local release
check. Target Preview.24 / ABI 1.68 / bridge 0.76.0; preserve the single native DLL,
module graph and separately versioned schemas. Make cohesive local checkpoints when
validated, and complete all accepted delivery gates before declaring Y complete.
No GitHub push or NuGet publication is authorized by the local-commit instruction.

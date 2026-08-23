# ADR-0019: Opaque `TopLoc_Location` Value Bridge

- Status: Accepted
- Date: 2026-08-22

## Context

`TopLoc_Location` is a composite OCCT value backed by datum/power lists. Its public
API returns references to internal state and composes locations with OCCT-specific
rules. B05 needs location semantics for topology placement without exposing that
representation or borrowing a native reference across the C ABI.

## Decision

- Add a registry-validated opaque native location handle and managed `TopLocLocation`.
- Expose identity, construction from an owned `gp_Trsf`, clone, inversion,
  multiplication, identity query, conversion to an independent `GpTrsf`, and shape
  `Located`/`Moved` operations.
- Keep `Located` (absolute location) and `Moved` (compose with existing location)
  as distinct operations. Return independent native values and preserve the existing
  status/error and SafeHandle ownership contracts.

## Alternatives considered

- Projecting the datum list or `TopLoc_Location` C++ layout was rejected because it is
  implementation-specific and not a stable ABI.
- Returning a borrowed `gp_Trsf&` from `Transformation()` was rejected because its
  lifetime is tied to the native location wrapper.
- Exposing only `ShapeTransform` scalars was rejected because it cannot represent
  location composition or absolute-versus-relative placement semantics.

## Consequences

- ABI advances from 1.12 to 1.13; bridge version advances from 0.13.0 to 0.14.0 and
  the package advances to `0.1.0-alpha.10`.
- B05.2 remains an intentional manual bridge under SC-006. Axes, vectors, directions,
  matrices, and generated location members remain subsequent work.
- The single bridge and package remain unchanged structurally under ADR-0015.

## Validation

- Debug and Release builds and runtime tests must pass for all new operation families.
- Package consumer must load ABI 1.13 from the application-local `occt` directory and
  verify location composition.
- Generated freshness and documentation/link checks remain required.

## Current batch note

The remaining B05 value families are recorded in ADR-0020. The historical B05.2
wording above is retained to preserve the decision trail; B05 is now closed as one
coarse migration batch.

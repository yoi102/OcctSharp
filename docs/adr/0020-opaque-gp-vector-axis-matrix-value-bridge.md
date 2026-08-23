# ADR-0020: Opaque `gp` Vector, Direction, Axis, and Matrix Values

- Status: Accepted
- Date: 2026-08-22
- Scope: B05 completion on OCCT 8.0.1, Windows x64

## Context

`gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` are small mathematical values, but their
C++ layouts, validation behavior, and exception paths are OCCT implementation details.
Projecting them as managed structs would couple the package ABI to one OCCT build and
would make future regeneration unsafe. B05 already established opaque registry-backed
contracts for `gp_Trsf` and `TopLoc_Location`; the remaining transformation values need
the same lifetime and error guarantees.

## Decision

Expose each value through an opaque, registry-validated native handle and an owning
.NET `SafeHandle` wrapper:

- `GpVec`: finite construction, clone, components, magnitude, dot product, cross product.
- `GpDir`: finite/non-zero construction, clone, components, dot product, reversal.
- `GpAx1`: point-plus-direction construction, clone, components, reversal, and rotation
  conversion to `GpTrsf`.
- `GpMat`: nine-value construction, identity, clone, 1-based value reads, determinant.
- `GpVec.ToTranslation` and `GpAx1.ToRotation` create independent `GpTrsf` values.

No C++ object pointer or layout crosses the C ABI. Every successful result is a new
owning value. Native exceptions and invalid handles use the existing status/diagnostic
contract. Registries are safety guards for stale handles, not a concurrency guarantee.

## Alternatives considered

- Blittable managed structs: rejected because OCCT layout and invariants are not a
  stable public ABI.
- Borrowed references into another value: rejected because source disposal would make
  the reference invalid and complicate regeneration.
- Generate these classes immediately: deferred until the generator can express their
  value constructors and operation-specific validation without unsafe fallbacks.

## Consequences

The bridge adds four handle families and six native registry operations while preserving
the established ownership model. The friendly managed surface is usable now, but these
bindings remain manual special cases until generalized value emission supports the same
rules. B06 may reuse the registry/error patterns for strings and collections but must
define encoding and index ownership separately.

## Validation required

Debug and Release native/managed builds, 32 generator tests, 32 runtime tests, generated
freshness, and a clean alpha.11 NuGet consumer with the application-local `occt` closure.
Every OCCT upgrade must rerun finite/zero validation, matrix indexing, transform
conversion, disposal, and package tests.

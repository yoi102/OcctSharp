# ADR-0018: Opaque `gp_Trsf` Value Bridge

- Status: Accepted
- Date: 2026-08-22

## Context

The B05 transformation scope needs a safe managed representation of OCCT's
`gp_Trsf`. The class stores matrix, translation, scale, and form state whose layout
and implementation details are not an ABI contract. Existing scalar shape transforms
also need to share one validated OCCT transformation path.

## Decision

- Add an opaque, registry-validated native `gp_Trsf` handle and a managed `GpTrsf`
  owner. No native C++ layout is copied into managed memory.
- Expose identity and finite checked translation/rotation construction, clone,
  inversion, multiplication, 1-based 3x4 matrix reads, disposal, and application to
  an owned `Shape`.
- Return independent native values for every operation that produces a transform or
  shape. Keep all OCCT exceptions inside the C ABI and map invalid handles/arguments
  through the existing status contract.
- Keep the older `ShapeTransform` record as a compatibility convenience and add an
  explicit `ToGpTrsf()` conversion. Do not remove the scalar ABI until consumers have
  migrated.

## Alternatives considered

- Projecting `gp_Trsf` as a managed sequential struct was rejected because the C++
  layout is not a stable cross-version contract.
- Passing raw `gp_Trsf*` without a registry was rejected because stale or arbitrary
  pointers could be dereferenced.
- Reusing only the existing scalar shape-transform function was rejected because it
  cannot support composition, inversion, cloning, or matrix inspection.

## Consequences

- ABI advances additively from 1.11 to 1.12; bridge version advances from 0.12.0 to
  0.13.0 and the package advances to `0.1.0-alpha.9`.
- B05.1 is intentionally manual and recorded in `SPECIAL_CASES.md`; axes, vectors,
  directions, matrices, and generated `gp_Trsf` members remain later B05 work.
- One native bridge and one managed assembly remain in place under ADR-0015.

## Validation

- Debug and Release native/managed builds must pass with the focused runtime suite.
- Runtime tests must cover identity, composition, inverse, clone independence,
  finite validation, invalid matrix indices, shape application, and disposal.
- Generated freshness remains required because the repository still regenerates the
  existing configured scopes in the same build.

## Current batch note

The later B05 work is recorded in ADR-0020. The historical B05.1 wording above is
retained to preserve the decision trail; B05 is now closed as one coarse migration batch.

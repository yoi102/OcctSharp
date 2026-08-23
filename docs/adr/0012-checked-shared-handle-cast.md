# ADR-0012: Checked Shared-Handle Cast Boundary

- Status: Accepted
- Date: 2026-08-21

## Context

Runtime type identity is useful only if a typed wrapper can be created without
turning an unchecked native pointer into a claimed cast. The shared probe already
exposes OCCT RTTI names and `IsKind`, but it did not yet define a cast result or a
managed type boundary.

## Decision

- Add one experimental cast target, `OcctSharp_TransientDerived`, rather than
  generalizing every OCCT `Handle<T>` at once.
- The native bridge validates the live wrapper, rejects null or incompatible
  dynamic types with `OCCTSHARP_STATUS_TYPE_MISMATCH`, then copies the opaque
  `opencascade::handle<Standard_Transient>` wrapper. No C++ object pointer, layout,
  or `Standard_Type` pointer crosses the ABI.
- The managed API exposes `SharedTransient.TryCastDerived` and `CastDerived`, plus
  a `SharedTransientDerived` wrapper. A successful cast retains one additional
  intrusive reference; a failed `Try` returns `false`, while `CastDerived` throws
  `InvalidCastException`.
- The registry continues to guard ABI wrapper lifetime. It is not a concurrent
  access guarantee and does not replace OCCT reference counting.
- General generated `Handle<T>`, borrowed handles, and parent-bound projections
  remain pending until their type descriptors and lifetime tests are designed.

## Alternatives considered

- Returning the source wrapper as a different managed type was rejected because
  it would allow an unchecked cast contract.
- Returning a raw native pointer was rejected because it bypasses retained-handle
  ownership and exposes an unstable layout boundary.
- Generating all typed handles now was rejected because only one runtime type probe
  and one ownership category are validated.

## Consequences

- The additive ABI status and export require ABI minor version 1.8 and bridge
  version 0.9.0. The package advances to `0.1.0-alpha.5`.
- The first typed wrapper is intentionally experimental and not a general OCCT
  handle-generation promise.

## Validation

- Release and Debug runtime tests verify successful derived casts retain the object,
  wrong/null casts fail without a wrapper, and the throwing cast reports a clear
  managed exception.

# ADR-0010: Standard_Transient Shared Handle Probe

- Status: Accepted
- Date: 2026-08-21

## Context

OCCT `opencascade::handle<T>` is an intrusive reference-counted smart handle.
Copying a handle retains the same `Standard_Transient` object and destroying a
copy releases one reference. Passing the raw `T*` across the C ABI would lose this
contract and make managed disposal unsafe.

## Decision

- Add an experimental C ABI for `Handle(Standard_Transient)` only as a lifetime
  probe: create, create-null, clone, null-state, reference-count query, and release.
- The ABI wrapper owns one OCCT `opencascade::handle<Standard_Transient>` value; the
  OCCT object and its intrusive counter never cross the boundary.
- Each ABI wrapper is registered separately so stale wrapper pointers return
  `InvalidHandle` before access and repeated release is a no-op.
- Expose the probe as `SharedTransient` in the managed API. It supports `Clone`,
  `IsNull`, `ReferenceCount`, and `Dispose`; broader typed `Handle<T>` generation,
  casts, and borrowed/parent-bound wrappers remain pending.
- The additive surface increments native ABI minor version to 1.6 and bridge version
  to 0.7.0. The package advances to `0.1.0-alpha.3`.

## Alternatives considered

- Mapping `Handle<T>` to `IntPtr` was rejected because it loses retain/release and
  null semantics.
- Exposing `Standard_Transient*` directly was rejected because callers could bypass
  the intrusive counter.
- Generating every OCCT handle type now was deferred until canonical type mapping and
  dynamic-cast rules are complete.

## Validation

- Release and Debug builds execute shared clone/reference-count and null-handle tests.
- The package consumer reports ABI 1.6, bridge 0.7.0, and OCCT 8.0.1.

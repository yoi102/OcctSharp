# ADR-0009: Native Shape Handle Registry and Invalid-Handle Contract

- Status: Accepted
- Date: 2026-08-21

## Context

The initial `OcctSharp_ShapeHandle` was an owning opaque pointer. Managed
`SafeHandle` made normal disposal idempotent, but a stale native pointer could
still be dereferenced if a low-level caller used it after release. Phase 2 needs
a deterministic invalid-handle result before broader shared or borrowed handle
projection is attempted.

## Decision

- Every native shape handle allocated by the bridge is registered in a process-local
  live-handle set protected by a mutex.
- Shape operations validate both null and live-registration state before accessing
  `TopoDS_Shape` storage.
- Release unregisters first and deletes only a registered handle; null, repeated, or
  stale releases are no-ops.
- Invalid non-null handles return `OCCTSHARP_STATUS_INVALID_HANDLE` (8) and a stable
  diagnostic instead of dereferencing the pointer.
- The registry is a safety guard, not a general concurrency guarantee. A single shape
  must not be released concurrently with an operation using it; broader thread-safety
  requires a separate ownership design.
- The additive status contract increments the native ABI minor version to 1.5 and the
  bridge implementation version to 0.6.0.

## Alternatives considered

- Relying only on managed `SafeHandle` was rejected because raw/native consumers could
  still pass stale pointers.
- Keeping freed addresses forever without a registry was rejected because it cannot
  distinguish a live handle from an arbitrary pointer.
- Reference-counting every shape operation was deferred until shared and parent-bound
  handle semantics are designed.

## Validation

- Release and Debug native/managed builds pass.
- Release and Debug runtime tests cover repeated release and stale-handle rejection.
- Package consumer validation reports ABI 1.5 and bridge 0.6.0.

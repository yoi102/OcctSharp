# ADR-0005: Native Status, Diagnostics, and Shape Handle

- Status: Accepted
- Date: 2026-08-21

## Context

The first native API must contain C++ exceptions and prove deterministic ownership
without prematurely defining every future OCCT handle category.

## Decision

- Fallible C ABI calls return a stable `OcctSharp_Status` integer enum.
- The native bridge catches `Standard_Failure`, `std::exception`, and unknown C++
  exceptions.
- Diagnostic text is UTF-8 in thread-local storage and remains valid until the next
  bridge call on the same thread.
- `OcctSharp_ShapeHandle` is an opaque owning handle containing a copied/moved
  `TopoDS_Shape` value.
- Managed code wraps it in `SafeHandle`; native allocation and release remain in the
  same bridge module.
- Native ABI version 1.0 is encoded as `0x00010000`; managed initialization rejects an
  incompatible major version.

## Alternatives

- C++ exceptions across P/Invoke were rejected as unsafe and unsupported.
- Returning raw `TopoDS_Shape*` was rejected because ownership and representation would
  be ambiguous.
- A global last-error buffer was rejected because concurrent callers would race.

## Consequences

- Managed repeated disposal is safe through `SafeHandle`.
- The current handle is specific to owning shapes; shared `Handle<T>`, borrowed,
  parent-bound, and typed runtime-cast designs still need additional rules.
- Every managed failure must read diagnostic text before making another native call.

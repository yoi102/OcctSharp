# ADR-0013: Generated Typed Shared Handles

- Status: Accepted
- Date: 2026-08-21

## Context

The experimental `SharedTransient` probe established intrusive retain/release,
registry validation, OCCT RTTI checks, and a checked cast. The generator still could
not project a real OCCT class returned or accepted as `Handle<T>`, so increasing the
header scope did not increase usable ownership-bearing API coverage.

## Decision

- Type-map rule `TM006` recognizes an OCCT `opencascade::handle<T>` value and projects
  it as an opaque shared wrapper. Pointer and reference layers around the handle remain
  rejected until their distinct ownership contracts are defined.
- A generated type owns one native wrapper containing one OCCT intrusive handle. The
  C ABI exposes only the opaque wrapper, never the OCCT object pointer or C++ layout.
- Every generated typed wrapper has its own live-wrapper registry. All operations
  validate the wrapper before dereference; release is idempotent. This is a stale-handle
  safety boundary, not a concurrent-access guarantee.
- The shared-handle eligibility pass promotes only public constructors and public
  non-pure instance methods whose complete parameter and return projections are proven
  value copies or `void` under the current type map.
- Generation configuration schema 1.3 adds explicit `sharedHandleScopes`. Each scope
  fixes the source package, native type, native header, export prefix, and managed name.
- `Geom_CartesianPoint` is the first generated real typed shared handle, exposed as
  `GeomCartesianPoint`. It validates construction, coordinate reads/writes, retained
  clone behavior, RTTI, disposal, and source-disposal independence.
- Borrowed pointers, parent-bound views, general downcasts, thread safety, and arbitrary
  ownership-bearing parameters remain unsupported.

## Alternatives considered

- Reusing one untyped `SharedTransient` for all APIs was rejected because it discards
  the declared target type and makes friendly APIs depend on runtime strings.
- Passing an OCCT object pointer across the ABI was rejected because it bypasses
  intrusive ownership and exposes unstable C++ layout and casting behavior.
- Enabling every discovered transient class automatically was rejected because member
  parameters, return ownership, callbacks, containers, and parent lifetimes still need
  separate rules.

## Consequences

- The generated set expands from four to eight files and from 31 to 42 emitted
  declarations in the selected scope.
- The additive exports require ABI 1.9 and bridge 0.10.0. The package advances to
  `0.1.0-alpha.6`.
- Adding a new generated typed class requires a reviewed configuration scope and
  compile/runtime/lifetime evidence; it does not require a handwritten wrapper.

## Validation

- Generator tests cover `TM006`, shared-handle eligibility, deterministic emission,
  and generated XML documentation.
- Runtime tests cover both constructors, coordinates, mutation, clone reference counts,
  shared mutation visibility, disposal order, RTTI, and access after disposal.
- Release and Debug builds, generated-source freshness, and the clean NuGet consumer
  must pass for this decision's package baseline.

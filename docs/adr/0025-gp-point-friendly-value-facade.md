# ADR-0025: Friendly `gp_Pnt` value facade

- Status: Accepted
- Date: 2026-08-22
- Scope: geometry primitive workstream inside B on OCCT 8.0.1, Windows x64

## Context

The generator already emits a layout-safe 24-byte value-copy ABI for the selected
`gp_Pnt` constructors. Exposing only the internal raw struct would force consumers to
depend on generated names and would not provide coordinate validation or value helpers.

## Decision

Add the public immutable `GpPoint` record struct. `Create`, `Origin`, and `Copy` call the
generated `gp_Pnt` value-copy exports; `DistanceTo` is a managed value operation. Inputs
must be finite and no native pointer or C++ layout is exposed.

## Consequences

- The first B07 public geometry value is source-compatible and allocation-free on the
  managed side.
- The generated declaration count remains unchanged because the facade is not a new
  native declaration; it is a curated API over the existing generated contract.
- Future `gp_XYZ` and axis/curve values must follow the same finite-value and copy rules.

## Validation

Release build and runtime tests pass (32 generator, 41 runtime). Package publication is
not authorized; local alpha.16 packaging and clean-consumer validation are required for
the next checkpoint.

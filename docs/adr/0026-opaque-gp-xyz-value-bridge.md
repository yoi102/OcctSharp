# ADR-0026: Opaque `gp_XYZ` value bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: B07 geometry primitive first value family on OCCT 8.0.1, Windows x64

## Context

`gp_XYZ` is a 24-byte C++ value with checked normalization and vector algebra. Its
layout must not be treated as a stable managed ABI, while its core operations are safe
value-copy candidates.

## Decision

Expose an explicitly sized C ABI value (`OcctSharp_Xyz`) and native operations for
default/create/copy/add/cross/dot/modulus/normalize. The managed `GpXyz` record validates
finite inputs and maps OCCT normalization failures to the existing status/exception
contract. No native pointer, reference, or C++ layout crosses the boundary.

## Consequences

- ABI minor 1.19 and bridge 0.20.0 are required; package alpha.17 carries the additive
  managed/native surface.
- The manual bridge remains separate from generated declaration coverage until the
  generator can prove trivial value layout and operation exception behavior.
- Future `gp_Lin`, `gp_Circ`, `gp_Pln`, and axis values must not reuse this ABI without
  their own semantic and ownership review.

## Validation

Release runtime tests cover construction, copy, add, cross, dot, modulus, normalization,
zero-normalization failure, and non-finite input. Debug validation is required before
accepting the geometry-value workstream; clean package consumer validation is required for alpha.17.

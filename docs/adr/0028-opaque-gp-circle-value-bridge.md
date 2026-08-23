# ADR-0028: Opaque `gp_Circ` value bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: B07 geometry primitive circle sub-batch on OCCT 8.0.1, Windows x64

## Decision

Expose an explicitly sized 56-byte circle value containing copied center, normal, and
radius. Native construction delegates to `gp_Circ(gp_Ax2,gp_Real)` and preserves
negative-radius and zero-normal failures; area, circumference, and point distance are
computed by OCCT. Managed `GpCircle` is immutable and layout-independent.

## Validation

Debug/Release runtime tests cover radius, area, length, point distance, and construction
failures. ABI 1.21/bridge 0.22.0 and package alpha.19 are additive.

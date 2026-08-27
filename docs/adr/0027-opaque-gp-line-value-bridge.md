# ADR-0027: Opaque `gp_Lin` value bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: geometry primitive line workstream inside B on OCCT 8.0.1, Windows x64

## Decision

Expose an explicitly sized 48-byte line value containing copied origin and unit
direction coordinates. Native construction uses `gp_Lin(gp_Pnt,gp_Dir)` so OCCT's
zero-direction construction failure is preserved; reversal, distance, and angle are
performed by OCCT. Managed `GpLine` is immutable and never exposes a native reference.

## Validation

The required Debug/Release runtime tests cover default direction, construction failure,
distance, angle, reversal, and package loading. ABI 1.20/bridge 0.21.0 and package
alpha.18 are additive to the prior B07 values.

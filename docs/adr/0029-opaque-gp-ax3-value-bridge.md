# ADR-0029: Opaque `gp_Ax3` value bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: B07 geometry primitive axis sub-batch on OCCT 8.0.1, Windows x64

## Decision

Expose an explicitly sized 96-byte `OcctSharp_Ax3` value containing copied origin,
X/Y directions, and main direction coordinates. Native construction delegates to
`gp_Ax3(gp_Pnt,gp_Dir,gp_Dir)` and retains OCCT's zero/parallel direction failures.
The `Direct` invariant is evaluated by OCCT and returned as a normalized `int32_t`.
Managed `GpAx3Value` is immutable and never exposes C++ layout or references.

## Validation

Release native/managed builds, runtime tests, generated freshness, and a clean alpha.21
package consumer pass with ABI 1.24, bridge 0.25.0, and the 36-DLL application-local
`occt` closure.

## Upgrade impact

Re-check `gp_Ax3` direction normalization, directness calculation, value size/alignment,
and construction exception behavior on every OCCT/compiler upgrade.

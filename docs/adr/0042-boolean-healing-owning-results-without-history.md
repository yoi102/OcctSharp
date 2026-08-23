# ADR-0042: Boolean and healing owning results without history

- Status: Accepted
- Date: 2026-08-23
- Scope: B12 safe Boolean/healing profile on OCCT 8.0.1, Windows x64

## Decision

Close B12 around result operations whose ownership is proven: `Shape.Cut`,
`Shape.Fixed`, and `Shape.UnifiedSameDomain`. Each operation validates live, non-null
topology, keeps its `BRepAlgoAPI`, `ShapeFix_Shape`, or
`ShapeUpgrade_UnifySameDomain` state native-local, and returns a new registered owning
shape that does not retain the input.

This profile deliberately has no cross-ABI history contract. `BRepTools_History`, BOP
generated/modified/deleted maps, ShapeFix status/mode internals, ShapeUpgrade history,
and configurable modes remain native-local and are not represented by borrowed pointers
or placeholder handles. They require a later explicit profile and ownership decision.

## Validation

Release and Debug runtime tests cover successful topology, both-input/source disposal,
null and disposed failures, and deterministic diagnostics. The alpha.34 clean consumer
executes Cut, Fixed, UnifiedSameDomain, and null Boolean/healing rejection with 36
application-local native DLLs. Existing full Release/Debug builds, generated freshness,
and ABI 1.26/bridge 0.34.0 evidence remain current because this closure adds no export.

## Upgrade impact

Re-check null behavior, completion and null-result states, default unification flags,
ShapeFix execution, TKShHealing/TKBO closure, and input/result independence on upgrades.

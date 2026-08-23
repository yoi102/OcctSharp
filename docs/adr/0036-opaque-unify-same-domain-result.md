# ADR-0036: Keep ShapeUpgrade_UnifySameDomain native-local

- Status: Accepted
- Date: 2026-08-22

## Context

Same-domain unification has mutable algorithm state and optional history. A first
safe migration step must not expose `BRepTools_History`, callbacks, or native
topology maps before their ownership rules are defined.

## Decision

Expose `Shape.UnifiedSameDomain`, using OCCT's default edge/face unification and
no BSpline concatenation. The bridge runs `Build`, checks for a non-null result,
and returns an independent owning `Shape`; failures remain status/diagnostic values.

## Consequences

The result operation is useful for healing workflows but intentionally excludes
history and mode controls. It can be replaced by generated ShapeUpgrade bindings
after those contracts are modeled.

## Validation

Debug/Release runtime tests and a package consumer must verify non-null topology,
source-disposal independence, and native linkage to TKShHealing.

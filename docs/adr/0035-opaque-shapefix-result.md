# ADR-0035: Keep ShapeFix_Shape native-local

- Status: Accepted
- Date: 2026-08-22

## Context

`ShapeFix_Shape` owns mutable healing state and exposes a large set of mode and
history controls. Crossing that state through the C ABI before parent/lifetime and
diagnostic semantics are defined would create an unsafe partial wrapper.

## Decision

Expose one operation, `Shape.Fixed`, that validates an input shape, runs
`ShapeFix_Shape::Perform` in the bridge, and returns a new owning shape. A null
result or OCCT exception becomes a status and thread-local diagnostic. The source
shape is not retained and remains independently disposable.

## Consequences

This is a deliberately narrow healing contract. It proves result ownership and
failure containment but does not claim coverage of ShapeFix modes, ShapeUpgrade
history, or generated declarations.

## Validation

Debug/Release runtime and clean package consumers must verify topology preservation,
source-disposal independence, and native failure mapping.

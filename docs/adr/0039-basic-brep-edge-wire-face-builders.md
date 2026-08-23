# ADR-0039: Keep basic BRep builders native-local

- Status: Accepted
- Date: 2026-08-23

## Context

Edge, polygon-wire, and planar-face builders own mutable OCCT construction state.
Only their completed topology values need to cross the stable C ABI.

## Decision

Expose copied point inputs for a straight edge and polygon wire, plus an owning wire
input for planar-face construction. Validate finite/distinct points, minimum polygon
cardinality, input topology kind, and builder completion. Return a new registered
owning `Shape` for every successful result.

## Consequences

B09 now covers the basic solid, edge, wire, and planar-face construction profile.
Curves, non-planar surfaces, constraints, and advanced builders remain later profiles.

## Validation

Debug/Release runtime and clean package gates cover shape kinds, face count, invalid
endpoints/cardinality/type, source disposal independence, and native loading.

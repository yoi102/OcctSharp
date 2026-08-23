# ADR-0038: Reject null topology values at operation boundaries

- Status: Accepted
- Date: 2026-08-22

## Context

`TopoDS_Shape` has a valid null value, but BRep booleans and healing algorithms
require actual topology. Letting null values reach OCCT produces version-dependent
exceptions and weak diagnostics.

## Decision

Keep null shapes representable for diagnostics, but validate `IsNull` in Fuse, Cut,
ShapeFix, and UnifySameDomain before invoking OCCT. Return `InvalidArgument` with
the stable message `The topology shape is null.`.

## Consequences

Invalid-input tests are deterministic and do not depend on OCCT exception wording.

## Validation

Release/Debug runtime tests and a clean package consumer verify null inspection,
algorithm rejection, and no native dereference.

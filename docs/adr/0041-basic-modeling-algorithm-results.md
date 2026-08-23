# ADR-0041: Basic modeling algorithm results

- Status: Accepted
- Date: 2026-08-23
- Scope: B11 basic modeling algorithm profile on OCCT 8.0.1, Windows x64

## Decision

Complete the B11 safe profile with three result-oriented operations. `Shape.Fuse` and
`Shape.Common` keep `BRepAlgoAPI` builders native-local and return new registered owning
shape values. `Shape.DistanceTo` keeps `BRepExtrema_DistShapeShape` native-local and
copies its minimum distance, first corresponding point pair, and total solution count
into a fixed 64-byte value structure.

Inputs are validated live, non-null topology values borrowed only for the call. Results
do not retain inputs. Algorithm builder state, support topology, parameter details,
progress objects, and history are not exposed by this profile.

## Validation

Release and Debug pass 32 generator and 64 runtime tests. Tests cover overlapping
Common topology, separated-box distance and points, null/disposed failures, result and
value independence after source disposal, and native/managed ABI layout. Generated
freshness and the alpha.34 clean package consumer pass with 36 native DLLs.

## Upgrade impact

Re-check Boolean completion/null-result behavior, distance execution state, 1-based
solution access, point ordering, fixed layout, and native toolkit closure on upgrades.

# ADR-0030: Opaque `GProp_GProps` property bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: B08 adaptor/property first sub-batch on OCCT 8.0.1, Windows x64

## Decision

Expose `GProp_GProps` as an owned registry-validated handle. The bridge computes
linear, surface, or volume properties from an owned `TopoDS_Shape`, and exposes only
mass, centre of mass, selected inertia-matrix values, clone, and density-weighted add.
`BRepGProp` remains native-local; no C++ object layout, references, or native iterators
cross the ABI. Mode and matrix indices are validated at the boundary.

## Validation

Release runtime tests cover a 10x20x30 box (volume 6000 and centre (5,10,15)), clone,
composition, inertia symmetry, invalid density/index, disposal, and a clean alpha.22
package consumer with 36 native DLLs under `occt`.

## Upgrade impact

Re-check BRepGProp integration tolerances, only-closed behavior, density errors, and
GProp matrix semantics on each OCCT upgrade.

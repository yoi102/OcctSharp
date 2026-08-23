# ADR-0034: Transfer BRep mesh data through caller-owned snapshots

- Status: Accepted
- Date: 2026-08-22

## Context

`BRepMesh_IncrementalMesh` creates face-local `Poly_Triangulation` objects whose
nodes, triangles, and locations are native-owned. Exposing those objects or their
arrays would make managed lifetime depend on OCCT topology and remeshing state.
The mesh phase also needs a bulk path; one interop call per vertex would be too
chatty for useful models.

## Decision

Expose a two-call native contract: `occtsharp_shape_mesh_count` computes required
vertex/index capacities and `occtsharp_shape_mesh_snapshot` copies positions,
face normals, and 32-bit triangle indices into caller-owned buffers. Each triangle
owns three copied vertices. Face reversal is reflected in both winding and normals.
Deflections are finite and strictly positive, and undersized buffers fail before
any copy. `Shape.CreateMesh` owns the resulting managed arrays and does not retain
the source shape.

## Alternatives rejected

- Native iterator wrappers: parent-bound and invalid after remeshing or disposal.
- Zero-copy views: require a pinned native ownership and mutation contract that is
  not yet available.
- Deduplicated global vertices: would add topology identity and smoothing semantics
  before a stable Poly/RWMesh model exists.

## Consequences

The first mesh wave is safe and predictable but intentionally duplicates vertices
per triangle and does not promise stable vertex identity or zero-copy performance.
Poly algorithms, RWMesh formats, and benchmark gates remain later work.

## Validation

Release and Debug native/managed builds, runtime tests for non-empty meshes, finite
normals, index bounds, invalid deflections, disposed sources, and a package consumer
are required before this sub-batch is marked complete.

# ADR-0085: Immutable authored mesh revisions and discrete topology adapters

- Status: Accepted and locally validated; Batch R 40/40, focused 24/24, Release/Debug/actual Debug-native Runtime 229/229 and full local exit gates pass
- Date: 2026-09-05

## Context and decision

Implement the complete unchanged 40-row R matrix. Entry is Q's `1a3662a` at Preview.16;
the explicit entry config preserves the original Preview.15 preparation config. The
case-sensitive inventory delta is 106 Blocked-to-Manual transitions, nine within R's
46 roots; no declarations or identities changed. These are reused prerequisites.

MeshData owns immutable `AuthoredMesh` copies, explicit optional normals/UV channels,
polylines, opaque material/group keys, coordinate metadata and revision-scoped index
correspondence. Reuse Geometry's `GpPoint` for positions; use mesh-specific copied
normal/UV/affine values rather than moving existing public Modeling/facade DTO identities.
MeshData has no Modeling/XDE dependency. Mesh owns editing and graph orchestration;
Modeling owns adapters returning the existing registered Shape lifetime. The facade
owns XDE material resolution, exchange and viewer revision replacement.

Edits create a unique revision and exact source/result maps, including deletion and
one-to-many seam splitting. Composition requires the same intermediate revision and
cardinalities. No native address, geometric nearest-match or equal cardinality proves
identity. Position/connectivity edits invalidate derived normals explicitly. Optional
channels are absent or full-cardinality, with explicit undefined normals; never pretend
that missing UV/normal information is a valid zero-valued measurement.

Native Poly algorithms remain call-local. Use coherent triangulation for patch editing
and degenerate removal, Poly connectivity plus full copied edge incidence for boundary/
non-manifold distinction, and Poly_MergeNodesTool indices for actual welding. Attribute
preservation partitions allowed merges; verify the resulting double-precision distances
because OCCT's welding hash uses float positions. Do not invent a mesh geometry engine.

Discrete faces reuse Shape registration/release, with no new native result owner. Their
snapshot path reads existing triangulations without invoking a mesher. Exact-face cache
replacement/remeshing works on independent copies and preserves source caches. Discrete
delivery must not promise exact STEP/IGES, surface-backed validity or exact solid volume.
Keep one DLL, existing projects and independent cohesive Native translation units.

## Alternatives and consequences

Mutable native Poly handles would require broader lifetime/concurrency semantics and
make edit correspondence less reproducible. Moving Modeling DTOs downwards would change
assembly identities unnecessarily. Reusing shape meshing for authored data would erase
the authored triangulation and is rejected. Material objects remain above MeshData.

Keep every existing public API/export. Reserve Preview.17; increment ABI/bridge only
when implementing the new C contract. Record exact directly used blocked IDs under a
new manual exception; never mark an entire audited root migrated.

## Required validation

All 40 acceptance rows, immutable/foreign/stale/overflow/invalid-channel tests, actual
topology changes and exact correspondence, manifold/branched/non-orientable fixtures,
mirror normals/winding, exact-versus-discrete cache isolation, real formats/XDE/HWND,
Release/Debug and actual Debug-native regression, headers/source/dependency closure,
manual accounting, additive API/ABI, clean regeneration, runtime refresh, both clean
consumers and full local release checks. Only then commit R and immediately enter S.

Related: [R matrix](../BATCH_R_MESH_AUTHORING_EDITING_GAP_INVENTORY.md),
[runbook](../BATCH_CONTINUOUS_EXECUTION.md), ADR-0074, ADR-0081, ADR-0082, ADR-0083.

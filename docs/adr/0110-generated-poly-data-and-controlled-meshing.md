# ADR-0110: generated Poly data and controlled meshing workspaces

Status: Accepted. Date: 2026-10-07.

## Decision

AR uses exact closed workflow catalogues TM023 in MeshData and TM024 in Mesh.
MeshData has no Modeling dependency. Mesh reuses its existing Modeling dependency.
Neither rule changes the global projection of borrowed native arrays/references.

Call-private Poly node/UV arrays configure precision before allocating, initialize
every slot and bound zero-based access. Float conversion rejects overflow. Copy
triangulations before mapping nodes/UV/normals/triangles; pinned Map* methods can
alias storage, so no mapped handle escapes. Copy scalar channels before releasing
maps. Missing normals/UV remain absent; zero normals are explicitly undefined.
Retain caller handles during copying; callers must initialize data and synchronize
mutation. Copied nodes, metadata and diagnostic arrays survive all owner disposal.
Each owning triangulation query deep-copies nodes, channels and initial parameters.
Invalidate the private copied bounds cache before deriving actual bounds.

Native Poly connectivity is conditional. Validate all indices before Load; reject
degenerate, non-manifold or inconsistently oriented edges from that projection.
Bound incident traversal and compare it with full indexed incidence, rejecting
disconnected vertex fans or incomplete traversal. Report its unavailability; reuse
existing MeshConnectivity for complete incidence, editing and revision guards.
Never claim Poly's single-neighbor representation retains all non-manifold uses.

Controlled meshing copies exact topology/geometry without its source mesh caches,
then assigns a fully initialized private IMeshTools_Parameters value to a private
mesher. Only a default inactive Message_ProgressRange is admitted; its source copy
constructor disarms the source and remains outside the rule. Copy effective
parameters after SDK normalization, preserving requested versus effective values,
IsDone, IsModified, status flags and exact missing-face ordinals. No callback or
mutable mesher/parameter reference escapes. Do not change global parallel defaults.

IsDone alone does not prove complete tessellation. Shape queries require faces,
no missing triangulation and no error bits; ReMesh/Outdated/Reused remain diagnostic
informational bits. Each query makes an independent geometry/mesh copy. Reuse
existing SnapshotExisting, editing, discrete topology and actual format readers;
do not implement another LOD/editor or publish partial success as complete.

## Validation

Precision conversion, missing/undefined channels, empty and point-only data,
stale bounds, independent mutations/disposal, invalid indices/capacity/wrong types,
coherent and disconnected/non-manifold adjacency, SDK parameter normalization,
real meshing failures and independent source caches. Runtime and both clean
consumers compose meshing, copied data, editing, discrete topology and real file
write/readback. Retain full local delivery gates; external publication is separate.

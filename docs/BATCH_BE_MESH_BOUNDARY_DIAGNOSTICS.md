# BE: Exact copied mesh boundary diagnostics

State: IMPLEMENTED / LOCAL GATES PASS; final freeze remains delivery authority.
Development identity: Preview.55 / ABI 1.98 / bridge 0.106.0.
See actual local evidence below.

[Admission](BATCH_BE_MESH_BOUNDARY_DIAGNOSTICS_ADMISSION.json): 8 admitted, 195 deferred,
0 new prerequisites and 13 reused operations; generated delta 8,
Manual delta 0. [Candidate input](BATCH_BE_MESH_BOUNDARY_DIAGNOSTICS_CANDIDATES.json) is an audit pool.
TM040 follows [ADR-0123](adr/0123-exact-copied-mesh-boundary-diagnostics.md).

A copied Poly_Polygon2D preserves the input-bound metadata separately from the
SDK's one-based normalized native bounds. Inputs are finite and bounded; closure
is explicit. Nodes and triangles of one private deep triangulation copy define
the only index domain used for quality, incident-edge, boundary, nonmanifold,
orientation and unreferenced-node diagnostics. No external topology is inferred.

Native coherent free-node diagnostics are used only for a proved manifold,
consistent-orientation, distinct-index profile. Native zero-based indices convert
to the private one-based domain and must match copied triangle references;
unsupported adjacency returns null. Each CopyTriangulation is independent and
preserves the verified node/triangle ordinal order.

Exact private copied faces expose BRepMesh native 2D/3D ranges, UV ends and maximum
tolerance. Bounds are 4096 polygon points/faces/edges, 16384 mesh nodes/triangles,
16 MiB output and finite coordinates at most 1e12. Synchronize source mutation
while copying. This profile belongs to Mesh, which already references MeshData
and Modeling; no reverse module dependency is introduced.

The full workflow executes BD root selection, controlled meshing, exact quality
and UV diagnostics, BC reference persistence, saved occurrence identity, actual
HWND mouse selection and more than 100 changed DXGI client pixels, then visibility
and transform stale-volume checks. The direct-module consumer retains the accepted
raw native Viewer workflow. Model-only assertions do not substitute for pixels.

Both clean consumers execute the shared workflow. Full Release/Debug, actual
Debug-native paths/hashes, cold regeneration, API/ABI/lifetime/source/payload and
14-package gates remain required. Only batch-be/delivery-result.json PASS
completes this batch.

Native Poly_Triangulation carries geometry, UV/normal channels and triangle node ordinals. Managed AuthoredMesh face-group metadata is outside that native copy domain. The whole-chain test verifies the original node/triangle mapping on entry, then compares both independent outputs to that exact native input snapshot.

Local evidence:

8 admitted / 195 deferred, 0 new prerequisites / 13 reused; generated +8 / Manual +0.
Release/Debug/cold Generator 286, Runtime 1171, all passed.
Actual Debug-native 1171, 62 observed DLL paths/hashes. Exports
41398 (+6/-0), Debug parity; API 57932
(+108/-0); 4347 facade forwarders. All 264 baseline
generated files unchanged; current files 267. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; BE counts complete only when delivery-result.json is PASS.

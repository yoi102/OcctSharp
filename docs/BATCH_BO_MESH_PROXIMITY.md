# Batch BO: private face-mesh proximity

State: IMPLEMENTED / LOCAL GATES PASS; frozen delivery authority: batch-bo/delivery-result.json.

BO admits 12 primary declarations and 10 required private prerequisites; 39 family candidates remain deferred and 32 exact dependencies are reused. The generated MeshProximity/TM051 profile keeps triangle sets, maps, BVH and native tools private. It copies an independently owned geometry/triangulation graph and checks exact zero-based face ordering and reciprocal overlap relationships.

Overlap accepts a finite tolerance and returns the exact SDK relation, including empty relations. WasPerformed means the private Perform call returned normally; IsDone preserves the SDK flag. The pinned OverlapTool sets IsDone from BVH Select > 0, so a zero-candidate separation returns false while visited disjoint triangles can return true with no overlap. Neither case is normalized or confused with an absent layer reference. LayerThickness uses the SDK proximity-value algorithm with exactly one face on each side. It is an approximate mesh diagnostic, not a certified BRep shortest distance. Native reference points and status ordinals are copied only after ValueTool.IsDone; the original ShapeProximity value-mode failure path can read uninitialized status, so it is not invoked. The finite overlap loader supplies audited triangle sets to a separate private value tool.

Resident complete triangulations are required. Face count <=256, nodes/triangles <=100000 per side, pair count <=65536, sampling <=copied node count. Degenerate triangles, missing/deferred meshes and Polygon3D edge primitives are rejected before solver execution. Geometry nesting/degree/poles are bounded; layer refinement is conservatively preflighted against the pinned SDK area/longest-edge algorithm, at most 100000 splits and depth 32. These budgets do not provide a timeout. Stored deflection is provenance, with no certified geometric error bound.

Immutable metadata survives disposal; CopyFace creates another independent geometry/mesh copy. No topology history is inferred. Tests cover finite tolerance, multi-face exact indices, differing density/refinement, source closure/replacement, budgets, invalid meshes, concurrent queries/disposal and real BL finite faces with controlled meshing. Both clean consumers execute this same native workflow.

Pinned SDK source commit: b8f597c677811d1f9f4d8a97f5ae2825c0353a42. See [admission](BATCH_BO_MESH_PROXIMITY_ADMISSION.json), [ownership](OWNERSHIP.md) and [ADR-0133](adr/0133-private-mesh-proximity.md).

The native delta contains six bounded C exports plus one MSVC-generated SDK default-constructor closure (??_FBRepExtrema_ShapeProximity@@QEAAXXZ). The compiler helper is not a managed API and receives no additional migration credit; all seven exact names and Debug parity are checked.

Only OcctSharp/artifacts/batch-bo/delivery-result.json PASS completes BO. Full delivery, actual Debug DLL observation, two clean consumers and package/source freeze are NOT RUN until their recorded gates pass. Hosted CI, external CAD interoperability, full downstream acceptance, signing, push and public publication: NOT RUN.

Local evidence:

12 admitted / 39 deferred, 10 new prerequisites / 32 reused; generated +22 / Manual +0.
Release/Debug/cold Generator 310, Runtime 1335: PASS.
Actual Debug-native 1335, 62 actually observed DLL paths/hashes: PASS.
Exports 41456 (+7/-0), Debug parity; API 58763 (+76/-0).
All 301 baseline generated files unchanged; current generated files 304.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final frozen authority: OcctSharp/artifacts/batch-bo/delivery-result.json PASS. Only that authority closes BO; BP–BQ remain authorized.

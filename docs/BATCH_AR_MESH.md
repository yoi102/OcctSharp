# Batch AR: copied Poly data and controlled meshing diagnostics

Baseline: delivered Preview.41. Delivery: Preview.42 / ABI 1.85 / bridge 0.93.0.
Full local gates PASS; final identities are frozen in
OcctSharp/artifacts/batch-ar/delivery-result.json.

[75 exact candidates](BATCH_AR_MESH_CANDIDATES.json) are audited in
[admission](BATCH_AR_MESH_ADMISSION.json): 35 admitted, 40 deferred and one new
default inactive progress-range prerequisite. 29 existing prerequisites are listed
separately. Expected exact Blocked→Emitted delta: 36.
[ADR-0110](adr/0110-generated-poly-data-and-controlled-meshing.md) defines the rules.

The useful gaps are copied Poly arrays/maps/bounds and conditional native incident
diagnostics, plus private mesh parameters/effective normalization and status.
Existing immutable authored revisions, complete incidence/non-manifold diagnostics,
editing, LOD, discrete topology and exchange are reused. Caller data must be
initialized and source mutation synchronized while copying. Native arrays, mutable
receivers, aliasing map handles and external progress callbacks remain private.

All public indices are zero-based; absent native adjacency uses -1. Empty data is
valid. Missing and explicitly undefined channels differ. Unsafe Poly adjacency is
unavailable, retaining the copied mesh for existing full incidence analysis.
Source and queried triangulation/shape copies must survive independent disposal.
Meshing distinguishes requested/effective parameters, flags, completion and missing
faces; no incomplete mesh is promoted through a shape query.

Required: precision/channel/bounds/adjacency/parameter/error/lifetime tests; real
shape→controlled mesh→copied data→existing edits→discrete topology→file readback in
Runtime and both clean consumers; complete Release/Debug/cold/debug identity,
ABI/API/exports, inventory, 14 packages and final frozen local delivery evidence.
Hosted CI, signing, push, public NuGet and whole downstream acceptance: NOT RUN.

## Local delivery evidence

Release, Debug and cold-source Release each pass 251 Generator / 978 Runtime,
with zero failures/skips; isolated actual Debug-native passes 978/978 and observes
62 loaded DLL paths/hashes. All 216 generated files reproduce, including 210
unchanged baseline files. Exact migration delta: 36. Exports: 41,294
(+14/-0), Debug parity; managed API 56,630 (+72/-0); 4,236 current
facade forwarders. Both configurations pass 57 C11 headers, 34 layouts,
250 release races / 16,000 acquisitions; all 45 private headers compile.
Twenty-four AR Runtime cases cover channels, quantization, cache independence,
metadata copies, native incident completeness, genuine self-intersecting/open-wire
error flags, and raw capacity/index/type/stale/failure publication guards. Native
IsDone can coexist with errors, or triangles with an open-wire error; both keep
HasCompleteTriangulation=false and shape queries unavailable.
Fourteen packages and both clean consumers compose controlled meshing, copied
Poly data, exact existing editing and discrete topology. Runtime/facade use
coordinate-aware authored OBJ/STL readback; direct modules use BREP/STL readback
with triangle/coordinate equality and explicit tessellation of imported STL facets.
The direct consumer contains no facade DLL. The geometry-only OBJ probe exposes
its existing default unit conversion and is not treated as coordinate-preserving.
Final source inputs, payloads, packaged documents, provenance, SBOM and checksums
are checked before freezing PASS. The same 32 parser failures remain.

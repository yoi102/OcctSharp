# CA: private generated mesh data, adjacency and meshing adapters

Baseline: BZ frozen Preview.75 / ABI1.113 / bridge0.124.0.
State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.

[Admission](BATCH_CA_MESH_ADMISSION.json) selects24 existing Manual signatures
across MeshData/Mesh. Six already-emitted support identities are reused with no
new credit. Twenty fixed SDK files govern initialization, array copying,
coherent pointer/storage lifetime, float welding and mesher constructors.
TM060 emits one native-private header, no public mutable SDK receiver or C ABI.

Nine original callers change receiver/helper typing; two related files remain
byte-identical. All original input validation,5-million-element budgets,
normal/UV presence, transforms/orientation/index conversion, source-isolation or
intentional mutation, partial/failure policy and publication remain. Triangles
and coherent triangle readbacks copy scalar node indices before owner closure.
SDK mutation pointers never leave the original call/owner. Weld output clearing
is generated; no mutable SDK output reference escapes. Polygon arrays retain
original paired1-based normalization; unequal lower bounds are not supported by
this private profile. Float normal/weld precision is not upgraded by generation.
Parameter mesher profiles expose only IsDone, not an unproved IsModified state.

Rule regressions, exact caller/configuration audits, Release/Debug native/runtime
and independent copied storage are required. Shared public consumers must retain
exact model/document and mesh revisions through quality, edit/remesh, failure,
XBF/STEP reopen and real HWND selection/invalidation. Exact CAD geometry and
mesh approximation/proximity remain distinct. Full local binding delivery is
required before CB. Hosted CI, signing, push, publication, external CAD and full
downstream application/device acceptance remain NOT RUN without evidence.

Local evidence:

24 admitted / 0 deferred, 0 new prerequisites / 6 reused; generated +24 / Manual -24 (new SDK declarations 0, Manual to Emitted 24).
Release/Debug/cold Generator 342, Runtime 1403: PASS.
Actual Debug-native 1403, 62 actually observed DLL paths/hashes: PASS.
Exports 41464 (+0/-0), Debug parity; API 58938 (+0/-0).
All 324 baseline generated files unchanged; current generated files 325.
Explicitly audited modified generated files: 0.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-ca/delivery-result.json PASS completes CA.

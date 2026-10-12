# CE: multi-format exchange and owned resource import

Baseline CD frozen Preview.79 / ABI1.114 / bridge0.128.0 PASS.
Candidate Preview.80 / ABI1.115 / bridge0.129.0; full gates passed, final frozen authority is batch-ce/delivery-result.json.
See [exact admission](BATCH_CE_EXCHANGE_ADMISSION.json) and
[owned resource contract](adr/0138-owned-mesh-resource-import.md).

TM064 converts34 existing Manual operations to exact private SDK factories and
delegates in two original native policy callers, preserving38 semantic call sites.
Six mutable XSControl_WorkSession provider overloads remain deferred: their owner
and initialization are not demonstrated by independent no-session workflows.
Eleven STEP operations reuse CD. TM065 adds4 exact blocked image/buffer operations
and reuses5 emitted dependencies. New SDK discovery0; new generated IDs38.

ConfiguredMeshFiles.ReadResources imports explicit local nested glTF JSON/OBJ.
It captures128-file/64MiB hashes, rejects escapes/reparse/unencoded unsafe paths,
copies bounded files, locks and rechecks the graph, then publishes complete owners.
The old flat Read and textureless WriteNew restrictions remain. All material PNG
slots are copied into native NCollection_Buffer/Image_Texture owners before
staging is deleted. Limit8MiB per texture,32MiB encoded total,64MiB decoded total,
16M pixels per texture and8-bit complete CRC-checked PNG. JPEG/compressed profiles
remain deferred. ReadTexture copies encoded bytes and decoded corner pixels from
one exact material/slot; snapshots survive parent closure. A retained native
document alias survives disposal of the result and both source/staging deletion.

The pinned OBJ reader resolves MTL texture paths against the OBJ directory and
converts Unicode texture tokens to ASCII. Explicit references are normalized only
inside an unpublished SDK copy using deterministic ASCII names. Each native path
maps back to an admitted original hash/path; geometry/material values are retained.
This adaptation is disclosed, not an SDK Unicode support claim. Multi-file IGES
assembly linkage is not admitted; separate IGES files remain independent inputs.

The real fixture ledger covers nested Unicode glTF external/embedded PNG and OBJ,
exact encoded hash and four decoded colors, two-triangle geometry, source deletion,
result/document/independent-shape lifetimes, malformed/missing resources and cleanup.
The existing IGES surface-mode writer retains6 surfaces/area52 and ASCII names,
colors/layers but loses the source solid topology (volume24 ->0). The subsequent
STL retains12 facets and2x3x4 bounds, losing metadata as defined by the format.
Geometry-only STL Unicode paths now reuse the existing ASCII path staging helper
in Runtime; this is IO policy infrastructure, not another binding exception.

Full Release/Debug/cold, actual Debug, ABI/API/export/lifetime and14-package
consumers passed; final source/package identity freeze is tracked by delivery-result.json. Hosted CI, external CAD, full downstream
application acceptance, physical GPU faults, signing, push and public NuGet: NOT RUN.

The exact unit audit also corrects the existing IGES copied diagnostic. Although
the SDK header says meters, pinned UnitValue() returns UnitFlagValue()/CascadeUnit()
and UnitFlagValue() returns millimeters. Report source meters as the captured model
ratio * captured model CascadeUnit * .001. CascadeUnit is an already-emitted exact
dependency; no new declaration credit is taken. The real millimeter fixture now
requires .001 rather than accepting any positive value. Caller equivalence records
this one explicit policy correction separately from34 exact delegate migrations.

Local evidence:

34 admitted / 6 deferred, 4 new prerequisites / 17 reused; generated +38 / Manual -34 (new SDK declarations 4, Manual to Emitted 34).
Release/Debug/cold Generator 353, Runtime 1434: PASS.
Actual Debug-native 1434, 62 actually observed DLL paths/hashes: PASS.
Exports 41467 (+2/-0), Debug parity; API 59003 (+37/-0).
All 326 baseline generated files unchanged; current generated files 331.
Explicitly audited modified generated files: 3.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-ce/delivery-result.json PASS completes CE.

# Batch AP: private modeling builders and copied diagnostics

Baseline: locally delivered Preview.39. Delivery: Preview.40 / ABI 1.83 / bridge
0.91.0. Full local gates PASS; final identities are frozen in
OcctSharp/artifacts/batch-ap/delivery-result.json.

[Admission](BATCH_AP_MODELING_ADMISSION.json) selects 65/140 candidates, defers 75
and adds one inherited BRepLib_Command::IsDone prerequisite. The 59 Emitted/two
Manual existing wire dependencies are reused, not added again.
[ADR-0108](adr/0108-generated-private-modeling-builders.md) defines TM021.

## Public outcome

ModelingBuilders builds a segment or a finite interval on an owning curve, creates
a wire from ordered copied edges, captures all 33 initialized content counts/six
shape groups and performs three bounded face-member wire checks. Independent
topology composes with existing face construction, repair and STEP/IGES readback.
No partial builder wire is exposed when a later edge disconnects.

The pinned SDK source at b8f597c677811d1f9f4d8a97f5ae2825c0353a42 is frozen under
OcctSharp/artifacts/batch-ap/sdk with source hashes. ShapeContents allocates groups
and clears flags in its constructor but initializes counters only in Clear/Perform;
the adapter explicitly initializes before access. Native traversal counts can repeat
subshapes: NbEdges and NbSharedEdges have different meanings.

Builders stay call-private. Typed frozen registry owners retain completed output;
every query deep-copies geometry. An unfinished result preserves native error status
and disallows topology access. A wire's input graph is copied together, keeping
shared vertex/curve relations without caller mutation. Disconnected input stops at
the first failed Add. Input shape mutation during copying must be synchronized.

Wire checks require exact membership in the source face, copy face/wire together,
use native indices 1..NbEdges, and retain LastCheckStatus independently of Found.
Absent outputs are nullable; native false must not manufacture zero-valued points.
Content group indices 0..5 have the order documented in the public API. Returned
groups and shape values remain usable after source/snapshot disposal.

## Validation tracking

Generator tests cover exact signatures/access, incomplete catalogues, deterministic
output and unchanged global type mapping. Runtime cases cover completed/unfinished
builders, disconnected and empty wires, curve copying, source/output release,
statistics semantics, group/index bounds, false diagnostics, real notch outputs,
foreign membership, release races and wrong typed tokens. Shared public workflow
runs from Runtime and both clean consumers, with face repair and real STEP/IGES.

Generation, compile, runtime, actual Debug-native, regeneration, ABI/API/export,
fresh inventory and packages are separate gates. Only executed results will be
recorded in STATUS and the final frozen delivery report.

## Local delivery evidence

Release, Debug and cold-source Release pass 243 Generator / 938 Runtime each,
with zero failures or skips. Isolated actual Debug-native passes 938/938, with
all 62 loaded DLL paths/hashes observed. Cold regeneration reproduces all 207
generated files, including 204 unchanged baseline files. Exact delta: 66 IDs.
Exports: 41,274 (+7/-0), Debug parity. Managed API: 56,456 (+43/-0), no breaking
change. All 4,219 facade forwarders are current. Both configurations pass 54 C11
headers, 34 layouts and 250 release races / 16,000 acquisitions. All 45 private
headers compile. Fresh inventory retains the same 32 parser failures.

Fourteen Preview.40 packages and both clean consumers pass the AP workflow.
Final package documents, source inputs, native/managed payloads, provenance, SBOM
and checksums are independently checked before the delivery collector can write
PASS. Hosted CI, signing, push, public NuGet and full downstream acceptance: NOT RUN.

# Batch BJ: selected STEPCAF roots and explicit local external assemblies

State: FROZEN LOCAL DELIVERY PASS at 8.0.1-preview.60; authority: OcctSharp/artifacts/batch-bj/delivery-result.json.
Baseline Preview.59 / ABI 1.102 / bridge 0.110.0, frozen BI delivery required.
Development target Preview.60 / ABI 1.103 / bridge 0.111.0.
[Admission](BATCH_BJ_LOCAL_STEP_ASSEMBLY_ADMISSION.json): 4 candidate declarations,
47 deferred, 9 necessary same-SDK external-reference graph prerequisites. Existing
STEPCAF modes, document ownership, model/root queries and shape copies are reused.
No WorkSession, mutable map, stream or native reader reference escapes.

## Initialized private graph and source identities

The specialized STEPCAFControl_Reader constructor initializes the STEP controller.
ReadFile alone does not transfer or read external files. STEPConstruct_ExternRefs
attached to each file's own reader WorkSession inspects the same associations used
by the SDK's recursive Transfer implementation. The pinned Format() lookup accepts only ShapeRepresentation and misses plain
Representation emitted by the pinned writer. A bounded fallback queries exact
DocumentFile/property/representation associations, compares any duplicate format
claims and copies the actual data-format item before native recursive reads.
A new tool is used per model because
LoadExternRefs does not clear prior state. Every graph edge is checked before transfer.
The SDK caches an external file only after recursive Transfer returns, so its cache
does not prove cycle prevention. Native and managed preflights independently reject
cycles, missing files, path-bearing names, unsupported formats and case/key ambiguity.

The explicit manifest admits 1–128 flat ASCII basenames under one canonical source
directory, with SHA256 identities captured before opening. Windows reserved names
and reparse inputs are rejected. Total bytes <= 64 MiB, entities <= 100,000,
references <= 4,096 and dependency depth <= 16. Root selections use the issuing
workspace identity and exact rank/model number/type, <= 4,096 distinct roots.
These are input/result budgets; they are not interruptible computation/time limits.

Each input is copied using a bounded stream into an exclusively owned temporary
directory. Staged hashes must match the manifest. FileShare.Read locks prevent
staged writes/deletes until the epoch is disposed. Source changes after opening
cannot alter the private staged epoch; a new open against the old hash fails.
Cleanup verifies the exact canonical temporary parent and deletes only owned flat
files before a non-recursive directory removal. Original source paths/hashes appear
in detached diagnostics; native loading operates on their verified staged copies.

## Transfer, copied failure state and exact occurrences

Transfer is one use. Native selected roots are processed into a close-safe private
XCAF document. Each root has an independent copied success/failure record. Further
roots are not attempted after failure. Loaded external-file status, transfer status
and imported label entries are copied while the native session is alive. Any failed
root or actually loaded external transfer prevents document publication; detached
diagnostics can still be returned. A preflight error throws before transfer.

Success publishes an owned document plus exact free-root and occurrence/definition/
parent relations queried from that document. Occurrence strings are diagnostic
identities in this exact result, never a cross-file topology-history binding.
CopyFreeRoot and CopyOccurrence deep-copy native geometry/triangulation; their shapes
survive workspace/document closure. Direct BI projections use the same imported
document/context, with inherited caller-command/BeforeClose restrictions.

Private STEPCAF read modes are applied per initialized reader. The SDK's
ReadExternFile creates a new STEPControl_Reader with default basic read/repair
parameters; this API does not claim that BD's custom model repair settings propagate
to recursive external files. Those parameter overloads remain deferred. No global
parameter mutation or silent mode fallback is added.

## Required evidence

Native-generated real two-instance/multi-file assembly, selected independent roots,
actual external load/transfer/definition/color and occurrence placement, copied
geometry/source lifetime; missing/cycle/out-directory/case/format/rewrite failures;
private modes and lazy limits. Fixed source, installed-header identities, exact new
StableIds, unchanged baseline outputs, full Release/Debug/cold/actual-Debug suites,
ABI/API/exports, both clean consumers, HWND/DXGI/stale queries and final payload
freeze remain required. Only batch-bj/delivery-result.json PASS completes BJ.
See [ADR-0128](adr/0128-local-step-assembly-epochs.md).

Local evidence:

4 admitted / 47 deferred, 9 new prerequisites / 28 reused; generated +13 / Manual +0.
Release/Debug/cold Generator 300, Runtime 1234: PASS.
Actual Debug-native 1234, 62 actually observed DLL paths/hashes: PASS.
Exports 41429 (+5/-0), Debug parity; API 58400 (+104/-0).
All 285 baseline generated files unchanged; current generated files 288.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze PASS is recorded in batch-bj/delivery-result.json.

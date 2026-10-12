# BD: Private identity-bound selected-root transfer

State: IMPLEMENTED / LOCAL GATES PASS; final freeze remains delivery authority.
Development identity: Preview.54 / ABI 1.97 / bridge 0.105.0.
See actual local evidence below.

[Admission](BATCH_BD_EXCHANGE_ROOTS_ADMISSION.json): 10 admitted, 163 deferred,
0 new prerequisites and 34 reused operations; generated delta 10,
Manual delta 0. [Candidate input](BATCH_BD_EXCHANGE_ROOTS_CANDIDATES.json) is an audit pool.
TM039 follows [ADR-0122](adr/0122-private-identity-bound-selected-root-transfer.md).

A private XSControl reader is explicitly initialized to STEP or IGES. The private
STEP/IGES actor is bound to the exact model; shape-fix parameters belong to that
actor. No Interface_Static values, WorkSession, model/entity handles, mutable maps,
streams or callbacks are exposed. Existing AK/AT/AN/AX workflows are reused.

Root IDs bind to one workspace GUID and one hash-verified source file. Root-rank,
model-number and entity routes retain selection order and duplicates. Invalid
selection does not consume the one-transfer epoch; native exceptions retire it.
Native false, null and empty roots publish no partial topology. Each GetShape
returns an independent geometry-deep copy. Detached root/load/transfer diagnostics
and repair parameters survive disposal. External-file traversal is not implemented.

Source files are bounded to 128 MiB, models to 1048576 entities, selections/roots/
published shapes to 4096 and copied text to 16 MiB. Lazy selections stop at 4097
and dispose the enumerator. Synchronize calls per workspace and source-file edits.
Acceptance uses real STEP/IGES selection, all three transfer routes, different
private configurations, exact foreign/file identity rejection, a real empty-root
STEP fixture, copied lifetime and selected export with independent readback.

Both clean consumers execute the shared workflow. Full Release/Debug, actual
Debug-native paths/hashes, cold regeneration, API/ABI/lifetime/source/payload and
14-package gates remain required. Only batch-bd/delivery-result.json PASS
completes this batch.

The private XSControl interface owns a specialized STEPControl_Reader or IGESControl_Reader. Preserve virtual format-specific loading and root discovery: the generic STEP ReadFile route omits the initialized STEP model protocol and failed the native probe. Reuse five additional already-covered loader operations; retain private per-model actors.

Local evidence:

10 admitted / 163 deferred, 0 new prerequisites / 34 reused; generated +10 / Manual +0.
Release/Debug/cold Generator 284, Runtime 1160, all passed.
Actual Debug-native 1160, 62 observed DLL paths/hashes. Exports
41392 (+6/-0), Debug parity; API 57824
(+84/-0); 4337 facade forwarders. All 261 baseline
generated files unchanged; current files 264. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; BD counts complete only when delivery-result.json is PASS.

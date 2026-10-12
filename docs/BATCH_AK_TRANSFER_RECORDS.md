# Batch AK: private transfer records and diagnostics

Locally validated engineering candidate: Preview.35 / ABI 1.79 / bridge 0.87.0.
Baseline: locally delivered Preview.34 / Batch AJ.

[ADR-0103](adr/0103-generated-private-transfer-records.md) defines lifetime and
index domains. The [admission matrix](BATCH_AK_TRANSFER_RECORDS_ADMISSION.json)
audits all 163 planned candidates: 78 admitted and 85 deferred. Eleven generated
prerequisites join 25 reused Emitted and one reused Manual supporting operation.
Exact fresh inventory delta: 89 Blocked-to-Emitted IDs; all other classifications
remain unchanged. Full local delivery passed; external/public-release gates remain
separate. The aggregate is OcctSharp/artifacts/batch-ak/delivery-result.json.

`TransferRecordSession.Read(path, format)` loads a private file model.
`TransferRoots()` or `Transfer(session.ModelNumber(n), ...)` runs one transfer and
returns a detached `TransferProcessSnapshot`. Explicit selections preserve order
and duplicates. Snapshot mappings distinguish missing binder, no result, no shape,
execution status and diagnostic messages. Records retain native level-zero and
level-one counts, including the duplicate main record at level one. Root, complete
and abnormal lists contain transfer-map indices, never model numbers.

`snapshot.Shapes`, `GetShape(mapping.MapIndex)` and `GetEntity(mapping.ModelNumber)`
produce retaining wrappers. `FindShape` compares exact native identity and does
not infer matches from geometry. Repeated snapshots read frozen metadata.

`TransferShapeBinder.Create(shapes)` copies ordered storage. `Append`, `Replace`
and `WithMessage` return independent binders. Shape identity/location/orientation
and duplicates survive. Empty/no-result/failure are distinct: the SDK's AddResult
does not set HasResult. Typed access checks TopAbs kind before casting.

No arbitrary mutable process, actor, callback, graph or binder handle escapes.
Writer finder-process operations require another explicitly justified consumer
scope and are deferred. No next migration batch is implicitly created by AK.

Evidence is retained under `OcctSharp/artifacts/batch-ak/`. The shared public-only
STEP/IGES workflow is wired into Runtime and both package consumers. See STATUS
for checks actually passed and outstanding gates.

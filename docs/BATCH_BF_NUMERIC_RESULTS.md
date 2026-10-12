# BF: Finite copied numeric results and parser-source audit

State: FROZEN LOCAL DELIVERY PASS at 8.0.1-preview.56; authority: OcctSharp/artifacts/batch-bf/delivery-result.json.
Last delivered baseline: Preview.55 / ABI 1.98 / bridge 0.106.0.
Development target: Preview.56 / ABI 1.99 / bridge 0.107.0.

[Admission](BATCH_BF_NUMERIC_RESULTS_ADMISSION.json): eleven primary functions
admitted, 44 primary candidates deferred; supporting operations are recorded
separately as the implementation/source audit is completed. No declaration count
is claimed delivered until the batch freeze is PASS.

TM041 and [ADR-0124](adr/0124-finite-copied-numeric-results-and-sdk-supplements.md)
implement private bounded LU/SVD aggregates, ordinary/SVD vector solves, multiple
RHS, inverse/pseudoinverse, determinant and copied vector diagnostics. Native result
fields never escape. All outputs are staged and validated before publication;
noncompletion has no partial data. Independent result grids preserve public bounds;
pivots are one-based private row ordinals. SVD values preserve actual SDK order.

Reusable generated support provides atomic buffer publication, finite values,
exact result counts and budget+1 lazy-copy limits. Existing AF/AG/AL/AZ APIs are
reused. Matrices are at most 64 by 64; vectors at most 4096. Thresholds are finite
and positive, at most one. There is no strict execution-time guarantee.

The 32 baseline parser failures are audited separately. A fixed-source, hash-bound
GeomBndLib include fragment targets the four affected header parse paths without
editing the installed SDK. Actual repair and full inventory refresh remain
required; other optional/missing/unsupported SDK headers retain explicit exclusions.
Source snapshots and evidence live under `OcctSharp/artifacts/batch-bf`.

Required acceptance: numeric reconstruction and residuals; singular/incomplete
absence; nonfinite arithmetic; budgets and parser path errors; lazy input and
concurrent copy semantics; fitted geometry/file readback and both consumers;
full Release/Debug/cold, actual Debug-native, ABI/API/source/package gates.
BF is complete only after `artifacts/batch-bf/delivery-result.json` is PASS.

Local evidence:

11 admitted / 44 deferred, 0 new prerequisites / 3 reused; generated +11 / Manual +0.
Release/Debug/cold Generator 290, Runtime 1182: PASS.
Actual Debug-native 1182, 62 actually observed DLL paths/hashes: PASS.
Exports 41400 (+2/-0), Debug parity; API 58015 (+83/-0).
All 267 baseline generated files unchanged; current generated files 272.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze PASS is recorded in batch-bf/delivery-result.json.

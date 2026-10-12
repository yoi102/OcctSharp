# CD: configured STEP and exact field fidelity

Baseline: CC frozen Preview.78 / ABI1.113 / bridge0.127.0 PASS.
Candidate: Preview.79 / ABI1.114 / bridge0.128.0; frozen local delivery PASS.
See [exact admission](BATCH_CD_STEP_ADMISSION.json).

TM063 generates11 existing Manual operations in private STEP/XSControl readers
and parameter initialization;23 semantic calls retain all original validation,
registry, virtual SDK dispatch, unit/root mappings, owning shape copy and Guard.
Two existing policy callers change only semantic calls and one generated include.
131 XDE declarations are reused from CC;40 exchange operations await CE owner
audit;unused default XML registration remains deferred. SDK additions:0.

The existing STEPCAF generated workspace gains WriteConfigured, accepting both
document domains, explicit AP203/AP214/AP242 and finite positive session resolution.
It transfers the complete original document, including supported semantic PMI;
no shape-only clone stands in for semantic attributes. Diagnostics read back the
actual writer model's schema/resolution. Managed output stages beside destination,
validates payload and hash, then replaces; failures leave destination intact and
release payload/remove staging. Existing Write signatures/behavior are retained.

Duplicate cleaning is opt-in for the new configuration. The pinned SDK serializes
the PMI fixture successfully with cleaning disabled but fails with it enabled.
An explicit request is preserved and reports the actual IFSelect failure; no
silent mode fallback occurs. The original failure logs remain in batch-cd.

The same fixture independently reads actual schemas, geometry, Unicode names/
layers, colors, physical and visual materials and semantic PMI definitions and
association geometry. AP242 is additionally imported through the native document
domain, rewritten, and independently read through the facade. Source files and
source owners are removed before validating copied outputs. Fields lost by the
fixed writer/reader are reported individually, not called fully preserved.
See [field matrix](BATCH_CD_STEP_FIDELITY.md). The matrix is fixture-specific; external CAD and arbitrary PMI/PBR are NOT RUN.

Full Release/Debug/cold, actual Debug-native, ABI/API/export/lifetime, clean
consumers, exact package contents and final source freeze: PASS.
Hosted CI/signing/push/public NuGet/full downstream acceptance are NOT RUN.

Local evidence:

11 admitted / 1 deferred, 0 new prerequisites / 131 reused; generated +11 / Manual -11 (new SDK declarations 0, Manual to Emitted 11).
Release/Debug/cold Generator 350, Runtime 1415: PASS.
Actual Debug-native 1415, 62 actually observed DLL paths/hashes: PASS.
Exports 41465 (+1/-0), Debug parity; API 58966 (+28/-0).
All 325 baseline generated files unchanged; current generated files 329.
Explicitly audited modified generated files: 3.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-cd/delivery-result.json PASS completes CD.

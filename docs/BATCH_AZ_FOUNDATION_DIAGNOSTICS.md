# AZ Foundation diagnostics

State: IMPLEMENTED / LOCAL GATES PASS; final freeze remains delivery authority. Last delivered baseline is Preview.49.
Admission: 27 primary declarations, 223 deferred, zero new prerequisites, six reused.
Exact records: [admission](BATCH_AZ_FOUNDATION_DIAGNOSTICS_ADMISSION.json) and
[candidate audit](BATCH_AZ_FOUNDATION_DIAGNOSTICS_CANDIDATES.json).

TM035 implements copied optional symmetric matrix powers and spectral decomposition,
bounded polynomial evaluation/refinement and root normalization, independent SDK
quadrature, inclusive UTF16/ASCII set searches and private Unicode Message_Msg
diagnostics. [ADR-0118](adr/0118-audited-foundation-function-diagnostics.md) specifies
source admission, numeric and format limits. AV strings/units and AL solvers remain
reused; additional redundant overloads are not double-counted.

Message arguments are ordered within each type. Missing arguments become SDK UNKNOWN;
extra arguments fail. Original/Value/Get strings are detached. Unsupported formats
are rejected before entering SDK sprintf. Percent escapes remain deferred because
the pinned SDK parser does not provide a safely proven literal-escape workflow.
The SDK truncates UTF16 code units while detecting percent signs. Format bodies
with non-ASCII units whose low byte is 0x25 fail closed; the same Unicode text
is supported as a copied string argument because inserted text is not reparsed.

Matrices use immutable copied grids, finite exactly symmetric data and at most 128
rows/columns. Eigenvectors are columns. A spectral result or optional matrix power
is captured only after all outputs are finite. Absence contains no partial values.
Polynomial refinement is an estimate with residual; it does not prove convergence.
Descending refinement has at most five coefficients due to the pinned SDK work array.
Gauss and Kronrod rules return independent points/weights on [-1,1]; SDK noncompletion
has no samples. All lazy inputs stop at budget+1 and dispose their enumerators.

The installed public headers match the pinned SDK source. Header functions have exact
package/toolkit contracts; discovery provenance remains visible, including null values.
Native linking and actual export audits validate external calls separately.

Focused and full delivery results are recorded under OcctSharp/artifacts/batch-az.
Full Release/Debug/cold, actual Debug-native, ABI/API, inventory and package/freeze
checks are required before this batch is counted complete. See actual local evidence below.
The authorized BA–BE batches remain queued after AZ freeze.

Local evidence:

27 admitted / 223 deferred, 0 new prerequisites / 6 reused; generated +27 / Manual +0.
Release/Debug/cold Generator 276, Runtime 1108, all passed.
Actual Debug-native 1108, 62 observed DLL paths/hashes. Exports
41362 (+8/-0), Debug parity; API 57504
(+69/-0); 4310 facade forwarders. All 249 baseline
generated files unchanged; current files 252. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; AZ counts complete only when delivery-result.json is PASS.

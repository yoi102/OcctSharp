# Batch BM: finite extrema and coupled copied results

State: IMPLEMENTED / LOCAL GATES PASS; frozen delivery authority: batch-bm/delivery-result.json.

BL frozen local delivery PASS is the baseline. This batch admits 23 of the 54 audited
Blocked family candidates, defers 31, and reuses 27 exact dependencies without
new migration credit. The pinned SDK commit is
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`.
The [admission record](BATCH_BM_FINITE_EXTREMA_ADMISSION.json) owns exact signatures.

## Contract

TM049 initializes finite-domain Extrema_ExtCC/ExtCS/ExtSS privately. Copied
geometry and bounded adaptors remain alive until the solver output is copied;
solver/adaptor references never cross the ABI. The immutable result holds deep
copied input geometry for parameter back-evaluation and detached rows for use
after disposal. Each row preserves its native one-based ordinal, square distance,
and one coupled pair of points with curve parameters or surface UV.

IsDone precedes all result queries. Parallel results report square distance and
a missing point pair because the SDK provides no unique pair in that state.
An empty completed result differs from a failed computation. Curve/curve also
copies the four native endpoint combinations separately; these are neither all
extrema nor proof of a global minimum. The single-solution flag is a configuration
request applied by parametric branches; analytic branches can return multiple rows.

The Geometry module owns computations. The Modeling adapter
`FiniteExtremaGeometry.CopyCurve` independently copies edge geometry and applies
its native location, preserving the existing dependency direction. It infers no
topology history. BL analytic edges/faces are real input fixtures.

Inputs require increasing finite intervals of width at most 1e6, finite scalar
magnitude at most 1e12 and tolerances from 1e-12 through .1. Natural nonperiodic
domains are enforced; periodic domains can cross the seam. Nested trim/offset
geometry is checked for cycles with maximum depth eight before copying. Curve
complexity is at most 256 poles/degree 14; surface complexity is at most 256 poles
in total and degrees eight. Unsupported geometry is rejected. Results are limited
to 4096 rows with a caller-selected lower budget. These checks bound inputs and
copied output storage, not native execution time. SDK numerical extrema do not
prove completeness or global optimality.

## Validation and delivery

Planned checks cover CC stationary/parallel/endpoint/multiple/periodic/general
cases, CS/SS states, coupled point/parameter/distance residuals, invalid domains,
budgets/tolerances, detached lifetime, disposal races and C buffer canaries.
Both clean consumers execute BL-created CC/CS/SS geometry and copied lifetimes.
Full Release/Debug/cold/actual Debug, DLL identities, ABI/exports/API, inventory,
14 packages and real Viewer checks remain required. Only
`OcctSharp/artifacts/batch-bm/delivery-result.json` PASS closes this delivery.

Hosted CI, external CAD interoperability, human/full downstream acceptance,
signing, push and public publication: NOT RUN.

Local evidence:

23 admitted / 31 deferred, 0 new prerequisites / 27 reused; generated +23 / Manual +0.
Release/Debug/cold Generator 306, Runtime 1304: PASS.
Actual Debug-native 1304, 62 actually observed DLL paths/hashes: PASS.
Exports 41446 (+6/-0), Debug parity; API 58634 (+74/-0).
All 294 baseline generated files unchanged; current generated files 298.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final frozen authority: OcctSharp/artifacts/batch-bm/delivery-result.json PASS. Only that authority closes BM; BN–BQ remain authorized.

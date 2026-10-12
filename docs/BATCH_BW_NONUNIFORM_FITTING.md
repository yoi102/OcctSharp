# BW: copied nonuniform fitting and generated fitting adapters

Baseline: frozen BV Preview.71 / ABI1.111 / bridge0.120.0.
Candidate: Preview.72 / ABI1.112 / bridge0.121.0.
State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.

## Contracts

TM056 emits18 exact initialized SDK fitting/patch calls as stack-only native
adapters. Owning handle getters return by value. Two audited callers retain
input validation/copies, options, topology construction, errors and publication.
The [admission](BATCH_BW_NONUNIFORM_FITTING_ADMISSION.json) converts18 Manual
identities to Emitted; newly implemented SDK identities and coverage gain are0.
Three admitted existing generated outputs change;317 stay unchanged, one header is new.

`SurfaceFilling.LoftWithParameters` couples2..128 sections with copied finite,
strictly ascending parameters, absolute value<=1e12 and width>=1e-9. The private
SDK section generator receives a1-based owning array. The managed result exposes
a separate readonly `RequestedSectionParameters` snapshot; legacy/sweep is empty.
No fallback changes the user's strategy or requested parameter mapping.

The pinned smoothing strategy2 ignores the explicit mapping and is rejected.
Exactly two sections require spline strategy1. Direct SDK probes confirm
ordinary0 and bounded-poles3 throw StdFail_NotDone for uniform and explicit
domains under default C2 and linear C0 profiles; spline1 succeeds for both.
Four-section strategies0/1/3 retain exact parameter position/derivative tests.
Legacy uniform/smoothing behavior is unchanged.

The first Debug full suite failed the newly added two-section legacy smoothing
fixture with `NCollection_Array1::Value`. An independent consumer loaded the
frozen Preview.71 actual Debug bridge (hash checked) and reproduced the same
failure, while the previously supported four-section midpoint remained1.5.
The new regression fixture now exercises that original four-section contract;
completion, empty explicit-parameter snapshot and midpoint assertions remain.
No production legacy fallback, SDK patch or changed count admission is introduced.
Two-section legacy smoothing is not newly promised; the fixed SDK limit remains.
The failed full run and baseline compiler/output-path fixture corrections are retained.

## AppSweep decision

Direct pinned SDK Release probe reproduces21-section straight-path final
translation Z0 instead of expected Z3. SweepSectionGenerator's ordinary tangent
branch omits the composed translation. The fixed SDK is not patched; AppSweep
remains DEFERRED and receives no migration credit. The existing safe AppSurf
workflow is retained. This scope delivers nonuniform fitting independently.

## Evidence

Runtime and both clean consumers execute explicit mapping/derivatives, dispose
source sections/result/surface, retain independent cache/face and parameter
snapshots, and save actual STEP with readback area9. Native probes compare all18
adapters with direct SDK calls; C11 probes check rejected outputs/buffer canaries.
Initial SDK and fixture failures remain in batch-bw logs; assertions are retained.
Required Release/Debug/cold, actual Debug-native, ABI/lifetime, package, final
Runtime and source/package freeze are incomplete until independently recorded.
Hosted CI, signing, push, public NuGet, external CAD and full downstream: NOT RUN.

Local evidence:

18 admitted / 0 deferred, 0 new prerequisites / 1 reused; generated +18 / Manual -18 (new SDK declarations 0, Manual to Emitted 18).
Release/Debug/cold Generator 330, Runtime 1380: PASS.
Actual Debug-native 1380, 62 actually observed DLL paths/hashes: PASS.
Exports 41462 (+1/-0), Debug parity; API 58927 (+2/-0).
All 317 baseline generated files unchanged; current generated files 321.
Explicitly audited modified generated files: 3.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bw/delivery-result.json PASS completes BW.

# BV: periodic finite projection and generated private adapters

Baseline: frozen BU Preview.70 / ABI1.111 / bridge0.119.0.
Candidate: Preview.71 / ABI1.111 / bridge0.120.0. Delivery authority is
`OcctSharp/artifacts/batch-bv/delivery-result.json` PASS; implementation alone
does not complete this batch.

## Generated behavior

The curve-sampling template now admits finite ascending U/V rectangles on periodic
surfaces across their canonical seam. Each periodic direction is limited to one
SDK period; a nonperiodic direction must remain inside its original domain.
Finite coordinates, tolerance, source complexity and result budgets remain enforced.
The pinned SDK `Extrema_ExtPS::TreatSolution` maps candidates into the requested
branch. No additional parameter normalization or endpoint guessing is introduced.
The copied point, U/V and distance remain coupled. These are orthogonal extrema,
not proof of a global minimum on the trimmed rectangle or face interior.

TM055 emits a native internal projection helper header. Four bounded/unbounded
constructor profiles and copied scalar/point/UV getters replace direct SDK calls
in the five audited manual caller files. Helpers live only on the native stack;
no projector owner or borrowed SDK output crosses the ABI. Callers keep validation,
location transforms, face clipping, sorting, failure text and output publication.

The exact [admission](BATCH_BV_PERIODIC_PROJECTION_ADMISSION.json) converts17 shared
Manual identities to Emitted once. This adds0 newly implemented SDK identities,
reduces Manual by17, and leaves Emitted+Manual coverage unchanged. Six previously
generated periodic queries are reused.318 previous outputs remain unchanged;
one existing native output changes under exact admission and one helper is new.

## Verification and consumer

The original cylinder seam request failed against BU with the documented domain
exception. Focused tests cover positive/negative shifted windows, torus U+V seams,
sphere nonperiodic latitude rejection, over-period/nonfinite/empty inputs,
no orthogonal solution, coupled back-evaluation, concurrent private calls and
disposed owners. An independent native probe compares all17 helper calls with
the SDK in Release and Debug, including no-solution and invalid-index behavior.
Existing projection/constraint/surface regression assertions remain unchanged.
An additional public-API probe passed8 exact cylinder/torus window endpoint cases
and observed62 matching loaded Release DLLs. Partial rectangular trimming remains
nonperiodic according to the pinned SDK; full-period trimming reports periodicity.

Both package consumers use copied seam U/V to build an independent circular edge,
dispose sources, save actual STEP, and verify the readback edge length1.8.
The same public workflow is exercised by the runtime suite.
The initial consumer fixture incorrectly assumed a typed spline pcurve overload
and a top-level edge STEP result. Corrected code uses the existing exact circle
builder and examines actual imported subshapes; the length assertion is retained.

Full suites, actual Debug-native identities, cold regeneration, ABI/lifetime,
14 packages and final source/package freeze remain required before delivery PASS.
The complete current Release/Debug suites passed328 Generator and1373 Runtime
tests each. The unified run then stopped because sample and facade-consumer
guards still expected bridge0.119.0. Their exact expectations were corrected to
0.120.0; all assertions remain. Original failure logs are preserved. Continuation
validates unchanged tested binaries, executes the original unified suffix, and
records its source hash; old suite results are explicitly reused, not replayed.
The standalone ABI/layout/private-header/adapter checks also passed in both
configurations. Final delivery remains governed by the authority above.
Hosted CI, signing, push, public NuGet, external CAD and full downstream application
acceptance are NOT RUN unless independently evidenced.

Local evidence:

17 admitted / 0 deferred, 0 new prerequisites / 6 reused; generated +17 / Manual -17 (new SDK declarations 0, Manual to Emitted 17).
Release/Debug/cold Generator 328, Runtime 1373: PASS.
Actual Debug-native 1373, 62 actually observed DLL paths/hashes: PASS.
Exports 41461 (+0/-0), Debug parity; API 58925 (+0/-0).
All 318 baseline generated files unchanged; current generated files 320.
Explicitly audited modified generated files: 1.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bv/delivery-result.json PASS completes BV.

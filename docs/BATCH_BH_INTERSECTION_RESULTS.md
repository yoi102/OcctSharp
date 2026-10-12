# Batch BH: finite ray and coupled surface intersection results

State: FROZEN LOCAL DELIVERY PASS at 8.0.1-preview.58; authority: OcctSharp/artifacts/batch-bh/delivery-result.json.
Delivered baseline: Preview.57 / ABI 1.100 / bridge 0.108.0 (BG frozen PASS).
Development target: Preview.58 / ABI 1.101 / bridge 0.109.0.
[Admission](BATCH_BH_INTERSECTION_RESULTS_ADMISSION.json): 16 admitted, 10 deferred,
0 new prerequisites. Existing GeomInt computation/output methods are reused; their
existing migration is not counted again.

## Finite hit domain

TM043 creates a private IntCurvesFace_ShapeIntersector per call, then Load/Perform
on one geometry-deep-copied shape. The SDK Load appends internal face intersectors;
therefore a mutable loaded receiver is never reused or exposed. Queries use finite
line/curve intervals, positive tolerance, at most 256 face occurrences and bounded
topology/spline complexity before copying. Curve inputs are separately deep-copied.
At most 4,096 hits can be published.

Results retain point, UV, line/curve parameter, native transition/state, SDK ordinal
and exact face ordinal in the private copied shape. Parameter sorting is stable and
keeps equal/coincident/seam hits distinct. No topology history or automatic rebinding
to the original shape is inferred. Face queries return another geometry-deep copy.
CopySurface extracts an independent world-coordinate surface in the Modeling module,
which already depends on Geometry; no reverse module dependency is introduced.

## Coupled geometry domain

TM044 initializes the existing GeomInt solver on two
private surface copies. Completed output retains each 3D line with its two associated
pcurves, plus 3D boundaries, isolated points and paired UV coordinates. HasLineOnS1/S2
is checked before accessing a pcurve; missing output remains null. Both source surfaces
and all curves are copied. Managed tuple publication cleans up earlier copies if any
later field fails; callers never receive a partial coupled tuple.

The fixed SDK resets myNbrestr to zero and never increments it. Thus NbBoundaries
remains zero; restriction outputs are reported in the native Lines domain. The wrapper
preserves that classification and does not invent boundary provenance. A real restricted
BSpline fixture verifies line geometry, bounds and independent lifetime; finite UV
boundary-crossing events are verified separately. Positive Boundary() output is
unavailable in this pinned SDK.

The SDK boundary API supplies 3D restriction curves without corresponding public
boundary pcurve access. This distinction remains explicit. Reached tolerances are SDK
diagnostics, not a certified global geometric error. Finite sampling in tests provides
observed residual evidence only. Returned analytic curves may have infinite native
parameter domains; callers choose finite evaluation/build intervals.

Separate explicit helpers build a pcurve on a copied surface over a finite 3D curve
interval, optionally constrained by a finite native UV rectangle, and report copied
boundary-crossing parameters from two existing coupled pcurves. Missing pcurves are
not silently reconstructed by the intersection operation. Periodic charts retain
their native UV branches; helpers do not silently unwrap or merge seam events.

The two tangent tolerance accessors are declared but have no definitions in the fixed
SDK source and fail the installed-library link. They remain deferred. The solver uses
its pinned constructor defaults; no callable custom tangent configuration is claimed.

## Failure and validation

Unfinished solvers publish no hit/geometry output, only initialized completion state.
Argument, budget and SDK exceptions do not publish a partial result. Scalar snapshot
buffers are staged and finite-validated before commit. Independent queried objects
and detached records survive source/result disposal. Synchronize source mutation
while copying. Limits constrain input/copy/output profiles; SDK computation and
internal sorting have no strict timeout or interruptibility guarantee.

Required validation covers no/multiple/boundary/tangent hits, stable equal-parameter
ordering, exact face UV back-evaluation, finite curve queries, periodic surfaces,
missing pcurves, isolated/boundary results, paired residuals, reconstruction/trim,
budget failures, independent lifetime, real topology/file workflows and both consumers.
Keep full Release/Debug/cold, actual Debug-native, ABI/API/export, inventory, package,
HWND/DXGI identity/stale-query and final freeze gates. Only batch-bh/delivery-result.json
PASS completes BH. See [ADR-0126](adr/0126-coupled-intersection-result-domains.md).

Local evidence:

16 admitted / 10 deferred, 0 new prerequisites / 16 reused; generated +16 / Manual +0.
Release/Debug/cold Generator 296, Runtime 1209: PASS.
Actual Debug-native 1209, 62 actually observed DLL paths/hashes: PASS.
Exports 41422 (+14/-0), Debug parity; API 58248 (+114/-0).
All 275 baseline generated files unchanged; current generated files 281.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze PASS is recorded in batch-bh/delivery-result.json.

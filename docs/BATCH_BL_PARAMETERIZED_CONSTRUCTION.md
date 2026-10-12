# Batch BL: private finite parameterized construction

State: IMPLEMENTED / LOCAL GATES PASS; frozen delivery authority: batch-bl/delivery-result.json.
Baseline Preview.61; target Preview.62 / ABI 1.105 / bridge 0.113.0.
The [exact admission](BATCH_BL_PARAMETERIZED_CONSTRUCTION_ADMISSION.json) selects
15/102 constructor candidates, defers 87 and requires two copied native representation/range prerequisites.
SDK source is pinned to b8f597c677811d1f9f4d8a97f5ae2825c0353a42; installed headers match.
Existing AP builders, geometric owners, topology copying and file exchange are reused.

## Contract

Line/circle/ellipse/hyperbola/parabola intervals, curve endpoint points/vertices,
pcurve+surface intervals and plane/cylinder/cone/sphere/torus finite faces stay private.
Frames require nonzero orthogonal directions, finite bounds and bounded magnitudes.
Intervals preserve actual native normalization; native ranges and orientation
are copied diagnostics. Face UV ranges must be increasing. This profile excludes infinite
intervals, uninitialized builders, implicit casts, stream and arbitrary mutation paths.

Curve/surface inputs are deep copied. Endpoint vertices are copied together in one graph,
preserving native sharing within that graph without guessing coordinate-based identity.
The SDK verifies endpoint/parameter consistency. Native failed constructors publish diagnostics
only; no partial shape or fabricated range is accessible. Completed topology is frozen,
and every queried shape is a new geometry copy independent of the result and sources.

For a pcurve edge, the exact stored representation on the private copied surface is verified
before BuildCurve3d completes its independent 3D representation. Pcurve UV and 3D samples
come from this frozen construction domain. This does not infer topology history or bind
old subshape consumers to a new result. External source mutation must be synchronized while copying.

The fixed SDK's 3D MakeEdge implementation normalizes descending parameters without reversing
the edge, despite the general header prose; its nonperiodic pcurve implementation explicitly
reverses. A descending periodic pcurve instead wraps its end forward across the seam.
The wrapper reports these actual distinct native orientations and does not silently alter them.
Full-circle endpoint sharing is verified by native IsSame within the frozen result, rather
than by the legacy subshape traversal count (which visits the shared vertex twice).

## Acceptance

Five analytic curve and five surface families, reversed line intervals, periodic seam,
all admitted point/vertex/pcurve variants, native failure, invalid frames/domains/endpoints,
source/result disposal and concurrent query/release require actual native tests.
The shared public workflow builds ordered parameterized edges and a finite face, evaluates
actual coupled pcurve representations and verifies STEP/IGES readback in both consumers.
Generator signature/closure/determinism, Release/Debug/cold/actual Debug, 62 loaded DLL
identities, ABI/API/exports, native buffer canaries, source audit, 14 packages and final
freeze remain required. Only batch-bl/delivery-result.json PASS completes BL.

See [ADR-0130](adr/0130-private-parameterized-construction.md) and the
[authorized BL–BQ plan](NEXT_ROUND_BL_BQ_PLAN.md). No public release is claimed.

Local evidence:

15 admitted / 87 deferred, 2 new prerequisites / 14 reused; generated +17 / Manual +0.
Release/Debug/cold Generator 304, Runtime 1280: PASS.
Actual Debug-native 1280, 62 actually observed DLL paths/hashes: PASS.
Exports 41440 (+6/-0), Debug parity; API 58560 (+69/-0).
All 291 baseline generated files unchanged; current generated files 294.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final frozen authority: OcctSharp/artifacts/batch-bl/delivery-result.json PASS. Only that authority closes BL; BM–BQ remain authorized.

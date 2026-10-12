# AW private geometry and controlled assembly

State: IMPLEMENTED / LOCAL GATES PASS; delivered only after final freeze PASS. Baseline Preview.46 / ABI 1.89 / bridge 0.97.0 delivered.
Current development identity Preview.47 / ABI 1.90 / bridge 0.98.0.
Exact admission is in BATCH_AW_GEOMETRY_MODELING_ADMISSION.json: 208 primary,
52 admitted / 156 deferred, 4 new prerequisites / 22 reused; provisional generated
delta 56 / Manual 0 until compiled/runtime/delivery proof. Fixed SDK source is
b8f597c677811d1f9f4d8a97f5ae2825c0353a42, including AppBlend template bodies.

TM030 privately evaluates BSplCLib D0–D3 for scalar/2D/3D clamped nonperiodic
splines and 2D/3D Bezier data. Pole/weight/knot buffers are bounded copied inputs;
finite in-domain samples publish independent immutable derivative rows.
Periodic and unclamped raw evaluation are deferred. Zero/invalid weights,
inconsistent multiplicities, out-of-domain parameters and excess capacity fail.

SurfaceFilling initializes AppSurf receivers and their private section generators.
Lofts use copied/harmonized owning BSpline curves and explicit uniform section
parameters. Radius sweeps retain a private path copy and build bounded private
circular sections on checked transported frames through complete computation.
AppSweep is explicitly deferred: the pinned SweepSectionGenerator's ordinary
tangent branches omit the TR = Rot * Trans assignment, collapsing straight-path
sections. Three actual approximation modes failed; the completing mode did not
prove path translation. It is not emulated or marked as covered. Radius sweeps
use AppSurf and test actual endpoint positions in all four strategies.
Input curves are nonperiodic, degree 1–25, at most 256 poles / 64 knots, bounded
coordinates/parameters, positive rational weights. All source mutation must be
synchronized by the caller during input capture. Strategies expose ordinary,
spline, smoothing and bounded-pole SDK modes. Failed approximation publishes
diagnostics only; every completed surface query deep copies geometry.
2D filling channels, general callbacks and raw scratch kernels remain deferred.

TM031 privately initializes vertex geometry, edge geometry, endpoint vertices and
their parameters. SameParameter/SameRange are asserted only on a newly created
edge containing one copied 3D representation and no pcurves. They are never used
to certify an input edge. BRepCheck_Analyzer must accept the final shape before
publication. There is no guessed topology history or mutation of input topology.
Tolerance is positive and at most one; numeric magnitudes are at most 1e12.

All required local checks passed: full suites 269 Generator / 1064
Runtime in Release, Debug and cold source; actual Debug-native 1064; cold
bytes 238, unchanged old bytes 232; inventory/ABI/export/API/private headers;
two clean consumers and 14 packages. Final package/source freeze is authoritative.
Continuous authorization proceeds AX and AY after AW freeze.

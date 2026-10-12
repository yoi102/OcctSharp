# BB copied curve computations

State: IMPLEMENTED / LOCAL GATES PASS; final freeze remains delivery authority.
Development identity: Preview.52 / ABI 1.95 / bridge 0.103.0.
See actual local evidence below.

[Admission](BATCH_BB_CURVE_SAMPLING_ADMISSION.json) records 26 admitted and
59 deferred primary candidates, no new prerequisite and 20 reused operations.
[Candidate input](BATCH_BB_CURVE_SAMPLING_CANDIDATES.json) is an audit pool.
Fixed SDK source is commit b8f597c677811d1f9f4d8a97f5ae2825c0353a42;
the installed headers must match it. TM037 emits the complete private workflow.

`CurveComputations` copies 2D/3D curves before CPnts length/abscissa,
GCPnts uniform-abscissa, quasi-uniform C0 deflection and tangential deflection.
Only bounded line/circle/ellipse/Bezier/BSpline source complexity is accepted:
256 poles, degree 25 and 64 C2 intervals. Intervals are finite and ascending;
periodic intervals may cross an unwrapped seam but span at most one period.
Uniform output preserves SDK parameters within its requested tolerance;
parameters are never silently clamped. Results are strictly ordered.

Native-only compositional adaptor overrides count Value/D0/D1/D2 evaluations through the base 3D EvalD0/EvalD1/EvalD2 virtual methods and 2D virtual methods and reject
nonfinite points/derivatives. GetType reports OtherCurve to keep audited generic
paths from allocating an uncontrolled analytic distribution. No managed
callback is exposed. The budget is 65536 evaluations, 4096 requested/published
points. NotDone, BudgetExceeded and OutsideDomain publish no partial points.
Deflection/Step are native diagnostics, not a proof of global approximation error.

Curve/surface projections operate on independent copies in explicit finite
domains. Surface profiles admit analytic surfaces and splines/Bezier with at most
4096 poles and degree 25. Only orthogonal candidates returned by SDK are copied;
no endpoint candidate is guessed. Equal nearest-distance ties remain ambiguous.
Result tokens are consumed and released immediately, leaving detached read-only
managed lists usable after source disposal. Callers synchronize source mutation
while the copy is made.

Focused acceptance: Generator 2/2 and Runtime 15/15 PASS against current Release-native; full gates still NOT RUN.

Acceptance includes 2D/3D curves, exact line arc intervals, signed abscissa,
periodic seam, midpoint chord quality, finite projections, absent orthogonal
solutions, degenerate/invalid input, budgets, independent lifetime and concurrency.
The shared workflow connects samples to topology assembly and both consumers.
Full delivery follows [WORKFLOW](WORKFLOW.md) and
[ADR-0120](adr/0120-bounded-native-curve-computations.md).
BB is complete only when artifacts/batch-bb/delivery-result.json is PASS.

Local evidence:

26 admitted / 59 deferred, 0 new prerequisites / 20 reused; generated +26 / Manual +0.
Release/Debug/cold Generator 280, Runtime 1136, all passed.
Actual Debug-native 1136, 62 observed DLL paths/hashes. Exports
41377 (+6/-0), Debug parity; API 57625
(+69/-0); 4320 facade forwarders. All 255 baseline
generated files unchanged; current files 258. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; BB counts complete only when delivery-result.json is PASS.

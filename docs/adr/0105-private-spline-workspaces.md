# ADR-0105: private spline workspaces

Status: Accepted for implementation, 2026-10-06.

AM uses exact generated signatures to build immutable private coefficient and
span-cache workspaces from copies of owning 2D/3D curves and 3D surfaces. Native
arrays remain one-based. Curve results contain position followed by derivatives;
surface results contain P, DU, DV, DUU, DUV, DVV. Results are detached copied grids.
Workspace publication and every coupled result are atomic and finite-checked.

Caches admit evaluation only within their source knot span. Periodic parameters
are normalized to the original geometry domain before the span check. Callers
synchronize mutation while the source is copied; subsequent source mutation or
disposal cannot affect a workspace. SafeHandle and typed registry leases protect
workspace disposal during calls. No native buffer, mutable receiver or callback
escapes. Each allocation is capped at 1,048,576 numeric elements.

Surface coefficient grids use U rows/V columns; native span caches instead use
max-degree rows/min-degree columns. Conversion is explicit, including equal
degrees. The SDK's inline surface CoefsD functions infer reversed degrees from
canonical coefficients; these signatures remain deferred. Surface evaluation
uses the separately audited CacheD functions with explicit degrees/layout.

Typed interpolation copies inputs and validates every band write before invoking
the native algorithm. It initially accepts clamped nonperiodic knots, bounded
contact orders, and optional weights (null absent, empty invalid). Exceptions or
unfinished inversion publish no values. Weighted curve interpolation follows the
SDK's Euclidean input/output contract for point constraints only (all contact
orders zero). Unweighted interpolation also accepts derivative constraints;
weighted homogeneous derivative constraints require a separate contract.
Weighted surface interpolation solves a
homogeneous tensor grid, so the adapter explicitly homogenizes and dehomogenizes.
All resulting weights must be finite and strictly positive before publication.

Packed caches, raw mutation buffers, sentinel addresses, callbacks and retained
multiple-knot outputs remain deferred with exact candidate reasons. This avoids
adding overlapping public APIs without a proven lifetime/layout contract.

Validation compares position and derivatives with owning evaluators for rational
and polynomial, periodic and nonperiodic curves/surfaces, both unequal degree
orders, repeated evaluation, independent lifetime, invalid layouts and failure
atomicity. Interpolated geometry composes with real CAD/STEP round trips in both
consumers. Full delivery follows WORKFLOW; implementation alone is not delivery.

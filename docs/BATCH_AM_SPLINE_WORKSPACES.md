# Batch AM: private spline workspaces

Baseline: locally delivered Preview.36. Candidate: Preview.37 / ABI 1.81 / bridge
0.89.0. Implementation and delivery are tracked separately in [STATUS](STATUS.md).

The [frozen candidate input](BATCH_AM_SPLINE_WORKSPACES_CANDIDATES.json) contains
136 AH deferrals, excluding the six already completed by AL. The
[admission matrix](BATCH_AM_SPLINE_WORKSPACES_ADMISSION.json) selects 31 exact
signatures and defers 105. No new supporting declaration is claimed. Existing
owning geometry, copied grids, matrix snapshots, basis evaluation and type registry
are reused. Native implementation dependencies are separately listed: LocateParameter
and Bezier weight-copy helpers operate only on private/owning data. Their general
raw bindings remain Blocked and are not counted as new migrations. The original
198 generated files remain byte-identical.

[ADR-0105](adr/0105-private-spline-workspaces.md) defines Geometry's TM019 adapter.
`SplineWorkspace` owns immutable private typed arrays. `Cache` copies an owning
2D/3D BSpline curve or 3D surface and freezes one knot span. `Coefficients` copies
an owning Bezier geometry. Snapshots and evaluation results are independent
`CopiedGrid<double>` values. Curve evaluation orders are 0..3, surface orders 0..2.
The surface row order is P, DU, DV, DUU, DUV, DVV. Caches normalize periodic input
then require it to lie within the frozen span; they never extrapolate silently.
At a span endpoint, derivatives belong to that frozen polynomial. At repeated
interior knots they can differ from an owning evaluator that chooses the adjacent
span; no cross-knot smoothness is inferred.

Snapshots use one-based rows and columns, coordinates followed by optional weight
coefficients. Curve rows follow increasing powers. Surface rows flatten U powers
then V powers, regardless of the private max/min-degree native cache layout.
The expansion origin is exposed; span-cache scale is half its interval length.
Bezier coefficient origins/scales are 0/1. Weight coefficients need not be positive;
the original geometry weights are positive and evaluation is restricted to its span.

`SplineWorkspaceInterpolation` copies clamped nonperiodic input. Every knot,
parameter, contact order, array capacity and native band write is checked before
calling the native algorithm. Curve weighted interpolation uses Euclidean point
samples with contact order zero; unweighted interpolation also supports derivative
constraints. Native homogeneous derivative constraints are not admitted here.
Surface weighted interpolation explicitly constructs homogeneous samples and divides
only after validating all solved weights. Null means no weights; an empty option is
invalid. Returned pole/weight rows are coupled. Kernel exceptions or unfinished
inversion throw without publishing any output; inputs are unchanged.

Known scope limits: native band factorization does not pivot, so some valid dense
systems cannot use this interpolation entry point; AL dense solvers remain available.
Surface inline CoefsD methods infer reversed degrees from canonical coefficients and
remain deferred. The adapter evaluates converted coefficients through explicit
CacheD calls. Packed scalar caches, callback evaluators, sentinel addresses and other
raw mutation/retained output families keep their candidate-specific deferred reasons.
SDK noexcept allocation behavior retains the process-wide OOM limitation.

Validation includes rational/polynomial 2D and 3D curves, non-square surface layouts
in both degree orders, periodic normalization, analytical/contact interpolation,
input immutability, source disposal, workspace disposal races, and actual surface
STEP readback. The shared public workflow is compiled into Runtime and both clean
package consumers. Full local delivery evidence is bound under
`OcctSharp/artifacts/batch-am/`; no public release is implied.

## Local validation

Release, Debug and cold-source Release each pass 234 Generator / 911 Runtime tests,
zero failures or skips. Isolated actual Debug-native passes 911 tests with 62 loaded
DLL paths/hashes matched. All 201 generated files regenerate byte-identically;
198 baseline files remain unchanged. Export delta is +10/-0 (41,263 total), with
Debug parity; API delta is +26/-0 (56,329 total), with 4,205 facade forwarders.
Both configurations pass 52 C11 headers, 34 geometric layouts and 250 registry
release races / 16,000 acquisitions. All 45 private headers compile independently.

The release entry point passed both product suites and smoke before two concurrent
forwarder checks collided on a shared temporary path. The check now uses a unique
temporary file, verified by two concurrent invocations. Hash-bound unchanged product
source/tests allow continuation from verify-generated; all remaining original gates
run, including cold source suites, exact inventory, 14 packages and both consumers.
The failed log is retained; it is not reported as a clean uninterrupted run.

Inventory remains 116,272 declarations: 26,395 Emitted, 1,218 Manual, 39,312 Blocked,
49,347 Skipped and zero Pending. Exactly 31 Blocked declarations become Emitted;
32 parser failures remain unchanged. These counts do not imply full migration.
Final document/package/payload identity is recorded in delivery-result.json.
Hosted CI, signing, push, public NuGet and full downstream acceptance are NOT RUN.

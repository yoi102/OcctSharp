# Batch AL: copied numeric matrices and private solvers

Candidate: Preview.36 / ABI 1.80 / bridge 0.88.0. Implementation and local build/runtime/release gates pass;
final package content auditing follows this document update.
Contract: [ADR-0104](adr/0104-copied-numeric-matrices-and-private-solvers.md).

## Exact migration accounting

[54 frozen candidates](BATCH_AL_NUMERIC_MATRICES_CANDIDATES.json) are all classified
in the [admission matrix](BATCH_AL_NUMERIC_MATRICES_ADMISSION.json): 41 admitted,
13 deferred, plus one newly admitted FactorBandedMatrix prerequisite. The existing
25 emitted matrix operations remain reused, with unchanged generated files.
The new exact generated stable-ID delta is 42. No global pointer/reference rule
is broadened. The closed TM018 projection belongs to Foundation.

The adapter calls the four typed point band solvers; their pinned template helper
delegates to the SDK's raw numerical buffer routines internally. This implementation
detail does not admit those raw routines as public bindings. Likewise math_VectorBase
and collection storage are internal native support, without a new public vector owner.

## Public workflow

MathMatrix.FromValues accepts CopiedGrid<double>, preserving row/column bounds.
Snapshot, row and column snapshots are immutable copies. Functional products,
transpose, scalar operations and row/column replacement preserve the inputs.
TransposeMultiplied computes A-transpose times B. Native TMultiplied(double)
scales without transposing. Inverse preserves the native matrix's original domains;
GaussInverse/PseudoInverse swap equation and unknown domains in their result.

Solve accepts one or more RHS columns and produces a detached NumericSolveResult.
Rows use the input matrix's column domain; columns use the RHS column domain.
The residual is the Frobenius norm of A*X-B, including rectangular least squares.
Gauss returns its determinant. Inverse operations have no RHS residual. Singular
Gauss/Householder decomposition returns IsDone=false with no values or scalars.
SVD supports rank deficiency and underdetermined systems; each public operation
constructs fresh state so a previous threshold cannot affect a later call.
Tolerance must be finite and strictly between zero and one. Gauss and Householder
use absolute pivot/column thresholds; SVD uses a threshold relative to its largest
singular value. Residual norms retain the numerical units of the input system.

SplineMatrixSystem accepts clamped nonperiodic flat knots, in-domain parameters
and contact orders from zero through degree. Degree is 1 through 25. It normalizes
all native storage to one-based indices, checks each band's write interval, keeps
unfactored coefficients and factors only private storage. No pivoting is promised;
zero or tiny pivots produce explicit noncompletion. The last diagonal is checked
separately because FactorBandedMatrix does not check it.

EvaluateBasis returns local support columns indexed by pole number; row zero is
position and subsequent rows are derivatives. Solve accepts 2D/3D coordinate rows.
Null weights mean unweighted; empty weights are invalid. With weights, coordinate
and weight columns publish together. Nonhomogeneous inputs are converted privately,
solved using the homogeneous SDK path and divided only after checking solved weights.
Status 4 denotes a zero solved weight; no solution is published. Homogeneous results
retain homogeneous coordinates. Numeric factor/solve noncompletion is separate from
invalid arguments and exceptions; no partial caller buffer is returned.

## Source proof and limits

Pinned source commit: b8f597c677811d1f9f4d8a97f5ae2825c0353a42.
Artifacts under OcctSharp/artifacts/batch-al/sdk record installed header and source
SHA256 identities. The audit follows math_Matrix.lxx/cxx, math_DoubleTab.hxx,
math_Gauss.cxx, math_Householder.cxx, math_SVD.cxx, BSplCLib_2.cxx,
BSplCLib.cxx and BSplCLib_CurveComputation.pxx. Householder has one-based private
column accesses; SVD thresholds mutate Diag; BuildBSpMatrix and the raw band solver
use unchecked pointer offsets. Those boundaries drive the adapter's normalization,
private copies and exact dimensions.

Finite values and storage arithmetic are checked; every private matrix has at most
1,048,576 elements, including SVD's padded U and square V. This is an allocation
bound, not a latency guarantee. Existing mutable MathMatrix inputs still require
caller synchronization. SafeHandle and typed registry leases protect disposal.
Immutable band systems support concurrent independent solves. SDK noexcept allocation
failure can terminate the process; no allocator-failure recovery is claimed.

## Acceptance evidence

The shared public-only BatchALNumericWorkflow interpolates an analytical 3D polynomial,
compares band and dense Gauss/Householder/SVD results and residuals, samples curve
positions/derivatives and round-trips an edge through STEP. It is included in Runtime
and both package consumers. Focused tests cover shifted domains, independent snapshots,
singular and underdetermined systems, SVD thresholds, weighted 2D/3D outputs, Hermite
contacts, invalid layouts, raw ABI publication failures and disposal races.

Release, Debug and fresh-source Release each pass **230 Generator / 888 Runtime**
tests, zero failures or skips. Isolated actual Debug-native passes **888/888**;
all **62 loaded DLL paths/hashes** match the Debug build. All 198 generated files
and the manifest regenerate byte-identically; all 195 previous files remain unchanged.

Native exports: **41,253**, +11/-0, Debug parity.
Managed API: **56,303 signatures**, +59/-0;
4,203 facade forwarders are current. Both configurations pass 51 C11 headers,
34 geometric layouts and 250 release races / 16,000 acquisitions. All 45 private
headers compile independently.

Fresh inventory: 116,272 declarations; 26,364 Emitted, 1,218 Manual,
39,343 Blocked and 49,347 Skipped; zero Pending, the same 32 parser exclusions.
Exactly 42 declarations change from Blocked to Emitted. This is not full migration.

The unified local release check creates 14 packages and runs both clean public-only
consumers. The shared workflow composes basis evaluation, dense/banded solving,
sampled spline position/derivative checks and independent STEP readback.
Final package payload/document/provenance/SBOM/checksum auditing follows this
stable-document update. Evidence: OcctSharp/artifacts/batch-al/.

Hosted CI, signing, push, public NuGet and full downstream application acceptance:
**NOT RUN**.

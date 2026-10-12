# ADR-0104: copied numeric matrices and private solvers

Status: Accepted for implementation, 2026-10-06.

## Context

AL extends the existing owning math_Matrix type. The pinned SDK's Householder
implementation indexes private columns from one, SVD mutates its singular-value
threshold state, and BuildBSpMatrix writes band storage through unchecked pointers.
Raw pointers, borrowed return references and mutable solver handles are unsuitable
public contracts. The existing owning matrix operations remain compatible.

## Decision

Generate a closed signature catalogue and source templates, preserving previous
generated files. Add initialized bulk matrix construction, copied matrix/vector
operations and immutable managed numeric snapshots. A mutable MathMatrix still
requires caller synchronization; disposal is protected by registry leases/SafeHandle.
New functional operations never mutate caller inputs and publish only after success.

Solvers copy and normalize all working matrices/vectors to one-based native ranges,
restore meaningful row/column domains in result metadata, and construct a fresh native
solver for each operation. Householder supports complete and explicitly bounded
submatrix solves. Gauss and SVD expose solve/inverse results, not internal receiver
state. Numerical noncompletion is a distinct result with no solution; invalid inputs
fail before calls. Matrix outputs must be finite before publication. Results own copied
managed data and survive all native input/result owners.

Spline matrix construction is initially clamped, nonperiodic and one-based. Validate
degree, knot multiplicities, parameter domain, derivative orders and every row's band
write interval before BuildBSpMatrix. Factor only an independent copy. A private
immutable system retains the original band storage and factorization status. Solves
copy all point/weight data; validate every resulting weight before rational division.
Neither matrix factor buffers nor output references escape. Periodic basis wrapping,
external storage, move constructors, stream dumps and mutable receiver aliases remain
outside this contract. AM may admit additional separately proved spline layouts.

Allocation sizes and index arithmetic are checked before native allocation. Existing
SDK noexcept operations retain the SDK's process-wide out-of-memory limitation; no
allocator-failure recovery is claimed. Ordinary argument/kernel exceptions stay in the
C ABI. Public callbacks and progress implementations are not introduced.

## Alternatives

Exposing native solver owners would permit SVD threshold history to affect later calls.
Returning raw matrix references would couple results to those owners. Reimplementing
all solvers in managed code would not migrate the audited OCCT declarations. The closed
copying adapter retains native algorithms while avoiding those lifetime ambiguities.

## Validation

Audit exact candidate/supporting IDs and pinned source hashes. Check shifted bounds,
rectangular products, singular/rank-deficient/underdetermined systems, multiple right
hand sides, residuals, inverse identities, repeated SVD thresholds, band writes,
rational weights, input immutability, disposal races and raw token/capacity failures.
Compose basis -> solve -> spline -> sampled geometry -> STEP workflows in Runtime and
both clean consumers. Follow all local delivery gates in [WORKFLOW](../WORKFLOW.md).

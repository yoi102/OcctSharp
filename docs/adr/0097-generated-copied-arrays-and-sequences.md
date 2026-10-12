# ADR-0097: Generated copied arrays and sequences

- Status: Accepted for Batch AF; delivery evidence is recorded separately.
- Date: 2026-10-02.
- Baseline: Preview.29 / ABI 1.73 / bridge 0.81.0.
- Related: [ADR-0095](0095-generated-reference-operations.md),
  [ADR-0096](0096-generated-owned-nontransient-values.md).

## Decision

Select exact stable IDs and native signatures from an audited operation catalogue.
The general type map stays closed. This first profile accepts Array1 of double, int,
gp_Pnt and gp_Pnt2d, and Sequence of double. Arrays have one dimension, a signed
32-bit lower bound, a positive count and kernel-safe upper/lower bounds. Both signed
integer endpoints are reserved: lower > INT_MIN and lower + count <= INT_MAX,
checked in 64-bit arithmetic. The pinned Array1::Upper adds lower + size before
subtracting one, and indexed kernel loops increment/decrement endpoints. Managed
construction and raw factories reject those unsafe bounds before allocation. Sequences
start at one and may be empty. Array2, HArray, arbitrary templates, pointers,
borrowed adaptors and retained external buffers require further contracts.

Managed CopiedArray<T> and CopiedSequence<T> hold independent immutable managed
copies. Their constructors and ToArray copy the data. Native indices are explicit;
operations requiring one-based data reject other lower bounds before kernel access.
Only the closed element catalogue can cross the ABI, even though the managed
snapshot containers can hold any unmanaged T. Coordinates3d projects gp_Pnt through
its constructors/accessors; no native geometric layout is marshaled.

Generated bridge factories copy every input element into native owning containers.
Fixed input addresses are used only within those factory calls. Algorithm calls
obtain typed registry leases and deep-copy all containers to local storage. The
audited fitting algorithms consume their input during construction/Init and retain
their own result handles. Stable heap ownership and leases follow ADR-0096.

Mutable collection references are copy-in/copy-out. Native output containers are
fully copied before registration; publication guards roll back registrations when
any subsequent registration fails. Managed code converts all collection outputs
before assigning any ref replacement. False kernel values still constitute success;
exceptions publish no ref changes. Native receiver mutation is not rolled back.
Concurrent mutation still requires caller synchronization.

Returned const references are copied while the receiver remains alive. A private
typed token owns each result snapshot; info/copy/release operate on that immutable
snapshot, preventing a count/read race with the original receiver. Capacity and null
checks precede output writes, and conversion completes in private storage before
copying to the caller buffer. Raw result tokens are released in finally blocks even
if managed conversion fails. Type tags, stale-token rejection and token non-reuse
reuse the single existing native registry. No native address reaches product APIs.

Counts are never inferred from arbitrary references. Per-operation guards validate
array lengths, matching bounds, knot ordering and multiplicities, spline degree,
weights, fitting parameters, output capacity and index ranges. The pinned Release
SDK can compile out range checks, so bridge preconditions are required. Numeric and
geometric inputs reject non-finite values during copying.

Emit a separate collection export/API family. Export identities derive from stable
IDs rather than catalogue order. Preserve prior bodies, signatures and stable IDs.
Fourteen compatible curve/fitting entries retain their existing Manual dispositions and do
not count as migrated declarations. Keep twelve modules, facade and one native DLL.
Shared receivers retain their existing lifetime contract; SafeHandle marshaling
retains managed receivers during each call.

## Validation

Require exact selection and signature drift tests, rejected pointer/unlisted element
profiles, copy independence, lower-bound and empty-sequence semantics, malformed
dimensions/capacity, failure publication, stale/wrong-type tokens, repeated disposal,
fit/construct/edit/read workflows in both 2D and 3D, Release/Debug and actual Debug
native runs, old-body/export/API preservation, cold regeneration, exact inventory,
clean facade/direct-module consumers and the unified local release gate.

See [Batch AF](../BATCH_AF_COPIED_COLLECTIONS.md) and [STATUS](../STATUS.md).

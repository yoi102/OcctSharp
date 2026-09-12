# ADR-0096: Generated owning non-transient values with leased token handles

- Status: Accepted; Batch AB implemented and locally validated.
- Date: 2026-09-08.
- Entry: Batch AA c65b480, Preview.26 / ABI 1.70 / bridge 0.78.0.
- Related: [ADR-0009](0009-native-shape-handle-registry.md),
  [ADR-0091](0091-proportionate-development-workflow.md),
  [ADR-0095](0095-generated-reference-operations.md).

## Context

Non-transient instance receivers and constructors remain a large migration gap after
Batch AA. Many useful OCCT classes have independent native ownership but cannot safely
be represented by an intrusive Handle<T>, a copied geometric record, or a temporary
receiver. Some preserve references to their own members, so moving or copying their
storage after construction can invalidate internal state. A raw allocation-address
registry also leaves a validation-to-use race and can accept an old address after
allocator reuse. This new category requires its own explicit lifetime contract.

## Decision

Generate owning wrappers only for an explicitly audited catalogue matching stable IDs
and exact native signatures. Accepted concrete types require an accessible, supported
construction and destruction path. Construct each native instance directly in stable
heap storage and destroy it through that same typed owner. Published instances are
never copied or moved; this scope does not infer Clone, copy constructors, move
constructors, borrowed-member ownership, or layout-based marshaling.

Each managed owner contains a typed SafeHandle with deterministic Dispose and a
finalization fallback. New C ABI handle values are opaque tokens, not object addresses.
The native bridge issues nonzero monotonically increasing tokens from one process-local
sequence for this category. Tokens are never reused, including after release; exhaustion
fails rather than wrapping. They have no persistence, cross-process, pointer-dereference
or arithmetic meaning. No C++ object layout or shared_ptr crosses the ABI.

One type-tagged registry in the existing Native DLL retains a shared_ptr owner for each
live token. An operation validates token liveness and the exact native type tag and
obtains a shared_ptr lease while holding the registry lock. It retains that lease until
all native access and result conversion finish. Resolve every accepted owned input in
the same way. Release removes the registry's owner under the lock; it neither reuses
the token nor destroys an object still held by an operation lease. Actual destruction
occurs after the last lease ends. Repeated or stale release is harmless and must not
remove another type's entry. Registry locking does not span arbitrary OCCT execution.

This closes concurrent release versus operation use-after-free and allocation-address
ABA for the new category. If acquisition wins, the call may finish after release; if
release wins, acquisition fails before native access. SafeHandle marshaling also keeps
managed calls alive across their P/Invoke. These are lifetime guarantees, not general
OCCT object thread safety: callers must synchronize concurrent operations on mutable
state and preserve all applicable native thread restrictions. Existing shape/shared
handle categories keep their established contracts and are not silently upgraded.

Scalar, enum and approved geometric inputs/results retain their accepted copy mappings.
Audited mutable copied-reference parameters follow ADR-0095: initialized independent
snapshots enter the call, all copied outputs and returns convert before successful
publication, and managed ref assignments follow a successful status in parameter
order. A false kernel result still publishes successful output copies. An exception
leaves managed refs unchanged but may leave the owned native object mutated; object
mutation rollback is not promised. Geometric inputs must start as valid records.

Use independent generated files and a new export/API family. Preserve Manual and
Skipped classifications, all old exports, public identities and generated native
function bodies. The general type map must not convert arbitrary non-transient classes,
mutable references, buffers or pointers to untyped handles. Borrowed storage, retained
external addresses, template instances and unsupported construction/destruction remain
outside the rule unless a separate applicable contract is accepted. Keep the existing
twelve modules, facade and single native bridge; allocation and release remain in
that bridge.

## Alternatives considered

- Stack receivers or copied value snapshots do not preserve stateful object identity
  and can invalidate classes that retain references to their own storage.
- A raw address plus a live set does not close concurrent release after validation,
  and allocator address reuse permits stale-handle ABA.
- Holding the registry mutex through OCCT calls would serialize unrelated owners and
  complicate reentrant calls. A lease protects storage without that global call lock.
- Intrusive Handle<T> ownership does not apply to non-transient classes; inventing
  inheritance would change the kernel's type and ownership model.

## Validation required

The exact supporting catalogue also supplies inherited maker status queries and
compatible projections of existing Manual methods without changing their dispositions.
For audited GC_Root/gce_Root descendants, generated `Value` and `Operator` calls check
`IsDone` explicitly. The pinned Release kernel did not enforce the documented
StdFail_NotDone exception for an invalid gce_MakeDir; the wrapper rejects that query
before copying a result. This guard is part of the generator and requires no manual ABI.

Before delivery, run generator selection/signature-drift and construction-path
regressions; native/managed Release and Debug builds; ABI and exact old export/body/API
audits; semantic operations on several accepted native families; raw null/stale/wrong-type
handle and repeated-release probes; concurrent acquisition/release stress and non-reuse
tests; destructor/lifetime and failure-publication checks; actual Debug-native runtime;
deterministic and cold regeneration; exact full-inventory reconciliation; all packages,
clean facade/direct-module consumers and the full local release check. Record the tested
race ordering and repetition count without claiming that lifetime leasing serializes
object mutation. Executed evidence is recorded in Batch AB and STATUS.

[Batch AB](../BATCH_AB_OWNED_NONTRANSIENT_VALUES.md) owns scope and evidence;
[STATUS](../STATUS.md) retains the last validated product and current progress. Local
commits are authorized. GitHub push and NuGet publication remain outside this work.

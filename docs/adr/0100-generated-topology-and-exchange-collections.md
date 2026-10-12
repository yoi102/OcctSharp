# ADR-0100: Generated topology and exchange collections

- Status: Accepted
- Date: 2026-10-03

## Context

AF-AH copied numeric and spline collections do not establish topology identity,
intrusive entity ownership, cursor independence or model/transfer epochs. AI audits
the frozen 88 signatures against pinned OCCT headers and source commit
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`. Existing Manual prerequisites keep
their classification and original special-case records.

## Decision

Add a closed exact-ID/signature catalogue and a generator emitter for Modeling
and DataExchange. Containers use the existing typed registry, leased ownership,
exception guard and rollback publication. Managed SafeHandles retain every input
through synchronous calls, including each raw pointer-array element. General
templates and the global InitialTypeMap remain closed.

`OwnedShapeSet` copies ordered TopoDS values, including duplicates, preserving
TShape, location and orientation as defined by ADR-0016. It does not clone the
geometry graph. Private algorithms run serially; Boolean/build/split operations
use non-destructive mode and history. History lookup requires exact input
membership. All native modified/generated paths are returned without selection,
geometric matching or automatic rebinding. Removal faces and kept edges/vertices
must belong to the exact source. Independent returned values outlive algorithms
and containers.

`RetainedEntitySet` copies container storage and retains Standard_Transient
references with original RTTI; it does not deep-clone entities. Iterator creation
detaches input storage and creates its own cursor. The implicit native copy
constructor is excluded because it shares mutable cursor/list handles. Calls on
owned mutable receivers are serialized. Native lazy preparation and GetOneItem's
append behavior remain explicit; filtering restarts the private cursor.

`ExchangeCollectionSession` reads a private STEP/IGES file model, exposes no model
or session mutators, validates every transfer input against that model, and allows
one transfer epoch. Validation failures leave the epoch open. A transfer exception
retires the session, requiring a new read. This prevents cache reuse across later
transfers. Model numbers and queries require a live session and exact membership.
Entity containers survive session disposal; retained references do not imply
continued model membership elsewhere. External mutation of retained entities is
unsupported while they participate in these private sessions/graphs.

Owned direct graph data retain the complete private native session and its model,
and survive disposal of the managed session wrapper through that retention. A
real-file lifetime probe showed that retaining only the model permits graph query
invalidation after releasing the SDK WorkSession. Graph/transfer calls serialize
through the retained parent's mutex. Only direct adjacency/root queries are
admitted. Native TypedSharings recursion lacks a visited set and is excluded.
LastTransferList accepts only roots=false. EntitiesFromShapeList accepts mode 1
only and preserves its first-matching-entity early exit. The excluded root branch
mixes model numbers and transfer-map indices. Its recorded branches (modes 2..4)
index an inner result sequence using the outer model number (`list->Value(i)`
instead of `list->Value(ir)`); actual Debug native exposed this as
NCollection_Sequence::Value. Reject those modes before invoking the SDK. Recorded
entity/shape snapshots and the safe mapped lookup remain available together.

Sequence append adapters mutate detached call-local sequences and publish a new
container. Shared sewing/binder receivers retain their existing thread/mutation
restrictions. A caller must not mutate shared receivers concurrently.

## Alternatives and consequences

A generic NCollection map would hide element, order, pointer and epoch contracts.
Borrowed lists/cursors would retain invalidation risks. Deep entity cloning would
change identity and require model/protocol copy machinery. The selected contract
keeps shared identity explicit and defers coupled repair maps, actor callbacks,
mutable contexts and recursive graph traversal to dedicated future workflows.

## Validation

Require exact admission accounting, preserved prior output/API/exports, generated
native/managed compilation, location/orientation/duplicate identity tests, source
disposal, cursor independence, errors/wrong/stale tokens, all-input membership
checks, real STEP/IGES RTTI and indices, transfer/cache state, and disposal races.
Run the public workflow in Runtime and both package consumers. Full Release/Debug,
actual Debug native, cold regeneration/build, ABI/header checks, packaging and local
release gates remain required before declaring delivery. STATUS owns check results.

Related: [ADR-0016](0016-generated-topods-shape-value-semantics.md),
[AI plan](../BATCH_AI_TOPOLOGY_EXCHANGE_PLAN.md),
[admission matrix](../BATCH_AI_TOPOLOGY_EXCHANGE_ADMISSION.json),
[WORKFLOW](../WORKFLOW.md).

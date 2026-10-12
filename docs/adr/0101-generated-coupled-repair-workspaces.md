# ADR-0101: Generated coupled repair workspaces

- Status: Accepted
- Date: 2026-10-03

## Context

AJ requires private mutable repair receivers, coupled map/list outputs and exact
replacement evidence. Copying TopoDS values does not detach geometry; implicit
WireData and WireSegment copies share mutable sequence handles. Header declarations
alone do not establish linked SDK support.

## Decision

Generate a closed exact-signature catalogue, typed registry leases and managed
SafeHandles in Modeling. WireData snapshots copy sequence storage, retaining native
shape identity. Functional edits create new sequence storage. Segments privately
own their wire and four patch-index sequences, validate all indices and ranges
before mutation and return copied values. Their implicit copy constructors and the
unimplemented TopoDS_Wire constructor remain excluded.

Repair sessions deep-copy topology and geometry once with BRepBuilderAPI_Copy;
the copy algorithm supplies the exact original-to-private correspondence. Receivers
own a private ShapeBuild_ReShape context with location consideration enabled. No
external context or borrowed mutable mode is exposed. Mode adapters copy scalar
settings. Native calls serialize; failed mutation retires the receiver. Returned
shape snapshots are deep-copied so subsequent repair cannot mutate prior results.

The four CheckSmallEdges collections form one immutable workspace. Face orientation
maps likewise belong to the private face receiver. Workspaces retain that receiver
and capture its epoch; consume operations require the same receiver and epoch.
Validation errors do not advance the epoch. Native mutation and publication failures
retire the receiver. Only success publishes outputs and advances the epoch. Map key
order is private snapshot traversal order, not a persistent identifier. Shape keys
use native IsSame semantics, including location and excluding orientation; ordered
list values preserve orientation and duplicates.

Replacement queries require exact original membership. Preserve all native
modified/generated paths, signed direct status and removal. Unsupported history
types have unavailable relation evidence. Never infer correspondence by geometry
or select one split/merge result. Explicit replacement inputs are deep-copied;
native ReShape records the requested relation, not an inferred repair. Detached
history snapshots own their containers and shapes independently of sessions.

## Alternatives and consequences

Generic public maps or shared native receivers would hide coupling and invalidation.
Value-only copying would allow healing to mutate the caller's shape. Deep copies
cost memory but provide source preservation. Composite-surface continuation and
advanced pcurve/intersection outputs require additional audited dependency paths;
they remain explicitly deferred when not established by the admission matrix.

## Validation

Require changed and unchanged repairs, ordered/reversed and nonmanifold wires,
coupled outputs, stale/foreign workspaces, exact lookalike rejection, complete
split/merge relations, independent results, errors, disposal races and real-file
roundtrips in Runtime and both public package consumers. Full delivery gates in
WORKFLOW remain applicable. STATUS records actual results.

Related: [AJ plan](../BATCH_AJ_REPAIR_WORKSPACES_PLAN.md),
[OWNERSHIP](../OWNERSHIP.md), [WORKFLOW](../WORKFLOW.md).

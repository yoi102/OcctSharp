# ADR-0108: generated private modeling builders and copied diagnostics

Status: Accepted. Date: 2026-10-07.

## Context

AP needs initialized BRepLib builders, copied content statistics and bounded wire
reference outputs. Native failure can retain partial topology. The pinned
ShapeContents constructor allocates groups but does not initialize counters.

## Decision

TM021 applies only to exact AP catalogue signatures. A reusable closed-workflow
emitter verifies the entire catalogue before publishing. InitialTypeMap is unchanged.
Modeling owns the adapter in the existing bridge. Builders remain call-private.
Deep-copy curves and input graphs; preserve a common graph for ordered wire edges.
Publish completion/error separately; unfinished builders expose no partial topology.

Frozen results use existing typed leased tokens and SafeHandles. Each topology/group
query makes a new geometry copy. Contents explicitly Clear/ClearFlags before setting
six capture flags and Perform. Copy all 33 counts with native occurrence semantics
and names; mutable native sequences never escape.

Wire checks require exact source face membership, copy face/wire together, bound
one-based indices and initialize coupled outputs. Preserve LastCheckStatus including
FAIL when a check returns false. Optional data exists only on true. Exceptions
publish no result owners. Source mutation must be synchronized during copying.

## Consequences

No new DLL, callback or borrowed-reference category. Registry leases provide release
safety, not mutation synchronization. Alternate constructors, implicit copies,
curve sequence diagnostics and RTTI stay explicitly deferred. Complete delivery
retains actual Debug-native, both consumers and all applicable WORKFLOW gates.

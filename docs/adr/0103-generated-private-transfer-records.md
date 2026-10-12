# ADR-0103: Generated private transfer records and detached diagnostics

Status: Accepted for Batch AK implementation, 2026-10-06.

## Decision

Use a closed exact-signature generator contract to connect standard STEP/IGES
readers to a private single transfer epoch. Callers supply session-bound model
numbers or select default native roots. Model numbers and process-map indices are
distinct public value types; default and foreign values fail before mutation.
Only built-in native controllers run. External process mutation, actors, callbacks,
contexts and recursive graph queries do not escape.

Materialize records, process root/complete/abnormal iterators, eight type-statistic
buckets and check/message text under the session lock. A frozen snapshot owns
copied metadata, copied shape-container storage and retained entity/shape values.
Repeated queries do not revisit native mutable checks. Reader disposal does not
invalidate snapshots. Acquired shapes/entities outlive their snapshot. Retained
entities and TShapes preserve identity; they are not deep immutable geometry clones.

Native mutation or publication failure retires the session; input validation
does not consume the epoch. Registry leases and matching type-tagged releases
protect concurrent disposal. Shape-list binder edits copy list and check storage;
no implicit native copy constructor is used. Native HasResult remains independent
of list length. Typed shape casts require exact topology kind first.

Messages are copied immediately to UTF-8 and serialized with escaped control bytes
inside an immutable native snapshot. A bounded two-call C ABI copies its text into
managed immutable records. Native entity/shape access uses separate typed exports.
Neither a JSON view nor an unmanaged pointer is exposed in the public API.

## Pinned SDK constraints

The audited SDK is OCCT 8.0.1, source commit
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`. Binder chains must be acyclic and at most
4096 nodes before SDK traversal. Result records must have no children before SDK
recursive helpers: this is the current Fill implementation's behavior, checked
at runtime rather than assumed. Native Results(1) duplicates the main record;
its count and transferred-entity order are preserved.

ResultOne/CheckListOne use unpopulated zero maps. SelectResult uses the current
cursor instead of each loop item. Filtered Next recurses across rejected items.
BRepCheck has no active source implementation. AI's failed root/recorded lookup
paths and TypedSharings stay excluded. Exact snapshot matching provides a useful
replacement without counting the unsafe SDK declarations as migrated.

## Scope and verification

The AK admission matrix owns exact counts and reasons. Supporting Manual/Emitted
operations do not inflate the migration delta. Generate, compile and validate
native Release/Debug, runtime ownership, cold regeneration, preserved IDs/API/
exports, ABI, packages and both clean public consumers before local delivery.
Hosted CI, signing and publication are separate gates.

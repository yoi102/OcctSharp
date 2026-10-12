# Batch AI: topology and exchange collections

Baseline: verified Preview.32 / ABI 1.76 / bridge 0.84.0. This batch adds the
[ADR-0100 contract](adr/0100-generated-topology-and-exchange-collections.md) through
the generator; generated files are never patched manually. The frozen candidate
inventory contains 88 audit rows. The [exact admission matrix](BATCH_AI_TOPOLOGY_EXCHANGE_ADMISSION.json)
admits 47, defers 41 with concrete source/contract reasons, and separately lists
30 supporting prerequisites. Actual Blocked-to-Emitted accounting is recorded by
the fresh inventory delta, without counting reused Manual/already-emitted support.

## Usable scope

- Ordered owning shape value sets; Boolean Fuse/Cut/Common, Splitter and general
  fuse arguments/tools/section edges; exact modified/generated/deleted history.
- Exact-source face removal, copied requested faces, guarded sewing couples, and
  safe same-domain unification with exact kept edge/vertex maps.
- Independent retaining entity sets/cursors, RTTI selection, detached sequence
  inputs/outputs and shared binder adapters.
- Private STEP/IGES readers, model-order entity snapshots and positive indices,
  both selection overloads, one transfer epoch, statistics and recorded/mapped
  entity/shape lookup, plus independent direct graph/root queries.

Shape copies retain shared TShape and preserve location/orientation; entity copies
retain intrusive references. Neither is a deep geometry/entity clone. Set access
uses zero-based indices; native sequence ShapeValue and sewing couples retain
one-based indices. Empty general sets and duplicates are supported; individual
algorithms enforce their required nonempty groups and bounded counts. Native
algorithm failures publish no result owner. Query failures zero single outputs;
multi-value stats are written only after successful completion.

Session creation and algorithm factories publish only complete owners. Transfer
input validation happens before sealing the epoch. A transfer exception leaves
native state unproved and retires the session. Independent results survive source
disposal; transfer/index operations still require a live session and exact model
membership. Graphs retain the complete private native session because the pinned
SDK invalidates graph query state if only the model handle survives session teardown.
External mutation of retained model entities and concurrent mutation
of shared sewing/binder receivers are unsupported.

Native iterator copies, recursive TypedSharings, root branches that mix model and
transfer-map indices, nested maps, repair workspaces, actor/protocol callbacks,
mutable contexts, string/integer sequence extensions and process-bound record
receivers remain explicit deferred scope. Native ShapeResultList caching is used
only within the session's single transfer epoch.

EntitiesFromShapeList admits only mode 1, including its native first-match exit.
Recorded modes 2..4 use the outer model index for an inner result sequence in the
pinned SDK. Actual Debug native exposed an out-of-range failure that Release
checks missed. Those modes are rejected before the SDK call; recorded entity and
shape snapshots remain usable with safe mapped lookup. The real-file regression
checks rejected modes, native first-match behavior, empty and foreign shapes.

## Shared workflow and evidence

`BatchAITopologyExchangeWorkflow` runs multiple input shapes through Boolean
history, owning result/argument sets, repair inspection, STEP serialization,
entity/model index/RTTI queries, graph/iterator contents, recorded transfer and
independent entity/shape inspection after source disposal. The same public-only
code is linked into Runtime and the facade/direct-module package consumers.
Focused tests add IGES files, errors, exact lookalike rejection, independent
cursors, native exhaustion/destruction, typed filters, binder/sequence aliasing,
raw publication/wrong/stale token checks and SafeHandle disposal races.

`OcctSharp/artifacts/batch-ai/` keeps source/SDK hashes, failed attempts, exact
preservation/inventory checks and local delivery evidence. STATUS is the current
verification authority. Native/managed Release and Debug, actual Debug native,
cold source regeneration/build, ABI/lifetime/compatibility, exact inventory,
both package consumers and unified local engineering gates passed. The final
aggregate binds those results to source/runtime/package hashes. Hosted CI,
signing, publication and whole downstream application acceptance remain separate.

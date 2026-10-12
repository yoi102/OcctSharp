# 8.0.1-preview.33: topology and exchange collections

Local engineering candidate for pinned OCCT 8.0.1 / Windows x64. Native ABI is
1.77; bridge version is 0.85.0. This is an additive candidate, with external release
and downstream acceptance gates separate from local validation in STATUS.

AI adds 56 exact generated declarations: 47 admitted audit candidates and nine
newly generated prerequisites. Twenty existing Manual prerequisites and one
already-emitted prerequisite are reused without changing their classification.
Forty-one audit candidates remain deferred with source/contract reasons. The
generated manifest contains 26,096 IDs
in 182 files; the previous 26,040 IDs and 175 complete files are preserved.

New public owning APIs connect ordered shape sets to Boolean/general fuse/split,
exact history, removal faces, sewing couples and protected same-domain unification.
Shape values keep TShape/location/orientation identity. Retained entity sets and
private cursors preserve entity RTTI without deep clones. Private STEP/IGES sessions
provide model indices, selections, a single transfer epoch, statistics, recorded
results and direct graph queries. Model membership is checked before transfer.
Independent sets/results outlive source owners; model/transfer calls still require
the corresponding live private session. Native cursor copies, recursive graph
queries, unsafe root-map branches, mutable repair/context maps and actor callbacks
remain excluded.

Shape-to-entity lookup admits the native mapped first-match branch only. Recorded
lookup modes are excluded after actual Debug native exposed a pinned SDK sequence
index bug; recorded entity/shape snapshots remain available. Graphs retain the
complete private native session to preserve query lifetime after managed disposal.

The public-only multi-input Boolean/history -> repair inspection -> STEP/entity
transfer workflow is shared by Runtime and both package consumers. Focused cases
cover IGES, identity/orientation/location, duplicates, foreign lookalikes, cursor
independence, typed filtering, publication errors, binder/sequence aliasing and
release races. Full gate results and source/runtime/package hashes are owned by
[STATUS](STATUS.md) and `OcctSharp/artifacts/batch-ai/delivery-result.json`.

See [AI contract](BATCH_AI_TOPOLOGY_EXCHANGE.md),
[admission matrix](BATCH_AI_TOPOLOGY_EXCHANGE_ADMISSION.json),
[ADR-0100](adr/0100-generated-topology-and-exchange-collections.md).

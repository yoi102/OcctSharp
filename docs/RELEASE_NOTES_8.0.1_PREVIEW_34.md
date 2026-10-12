# 8.0.1-preview.34: coupled repair workspaces and exact history

Local engineering candidate for pinned OCCT 8.0.1 / Windows x64. Native ABI is
1.78; bridge version is 0.86.0. [STATUS](STATUS.md) owns actual delivery results;
external release and downstream acceptance remain separate.

AJ adds 137 generated declarations: 136 admitted audit candidates and one new
prerequisite. It reuses 25 Manual and 43 already-emitted prerequisites without
changing those dispositions. Fifty-two audit candidates remain deferred for
specific SDK or ownership reasons. The manifest has 26,233 IDs in 192 files;
all prior 26,096 IDs and 182 complete generated files are preserved.

New generated Modeling APIs connect ordered wire storage/diagnostics, private
segments and patch sequences, coupled small-edge/orientation maps, private deep
wire/face/shell/solid repair and exact replacement snapshots. All four collections
stay coupled across small-edge consume. Foreign/stale epochs are rejected before
mutation. Results and all native history paths survive managed receiver disposal.
One-to-many edge/face/solid replacement uses native history-compatible carriers;
multi-vertex replacement is rejected. No geometric correspondence or automatic
selected-topology rebinding is performed.

The shared public-only acceptance repairs an actual tiny edge, splits disjoint
outer bounds into two valid faces, preserves source geometry, disposes receivers
and roundtrips STEP/IGES through private AI exchange sessions. Focused cases also
exercise reordered/reversed/nonmanifold wires, real loops, shell orientation,
unresolved gap evidence, exact split/merge/removal, invalid inputs and release races.
Native DONE is a status, not proof that all topology defects are repaired.

Implicit mutable WireData/WireSegment copies, the missing native wire constructor,
shared seam-pcurve reversal, composite-surface continuation and advanced external
analysis remain deferred. See the [AJ contract](BATCH_AJ_REPAIR_WORKSPACES.md),
[admission matrix](BATCH_AJ_REPAIR_WORKSPACES_ADMISSION.json) and
[ADR-0101](adr/0101-generated-coupled-repair-workspaces.md).

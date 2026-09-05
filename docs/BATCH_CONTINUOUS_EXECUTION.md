# Continuous implementation runbook: Q through W

- Status: prepared for the next explicitly started implementation run; **not running**.
- Decision: [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md).
- Queue: **Q -> R -> S -> T -> U -> V -> W**, 40 capabilities each, **280 total**.
- Current implementation: **0/280**; completed product baseline remains B-P / Preview.15.
- Machine-readable queue: [continuous-plan.json](../OcctSharp/config/batches/continuous-plan.json).

## Start and continuation contract

The user requested planning now and uninterrupted sequential implementation next time.
This checkpoint does not start feature code, a scheduler, a background process or a new
task. On the next instruction to begin this continuous run, start the first incomplete
batch and continue through the queue without asking routine permission after each
successful local commit. No exact magic phrase is required.

A completed batch is a durable checkpoint, **not a reason to end the authorized run**.
After its full gates and local commit, record the next baseline delta and proceed.
Do not ask the user to type “continue” merely because one family/batch is finished.
Do not substitute sleeps, reminders or a polling automation for implementation.
User interruption/stop or a required external decision still takes precedence.
Platform/process/session interruptions cannot be promised away; persist enough evidence
to resume the same work safely, without claiming background execution after a turn ends.

## Queue and actual prerequisites

Every row inherits completed B-P, ADR-0074/0081, the pinned SDK and local build tooling.
Prerequisites below are real planned contracts, not artificial dependency on every
earlier algorithm.

| Batch | Scope | Additional required queue contracts | Local preview slot |
|---|---|---|---|
| Q | [Repair/topology](BATCH_Q_SHAPE_REPAIR_TOPOLOGY_GAP_INVENTORY.md) | None | 8.0.1-preview.16 |
| R | [Mesh authoring/editing](BATCH_R_MESH_AUTHORING_EDITING_GAP_INVENTORY.md) | None | 8.0.1-preview.17 |
| S | [Guided sweeps/constraints](BATCH_S_GUIDED_SWEEP_CONSTRAINED_SURFACE_GAP_INVENTORY.md) | None | 8.0.1-preview.18 |
| T | [Parametric recompute/naming](BATCH_T_PARAMETRIC_DOCUMENT_RECOMPUTE_GAP_INVENTORY.md) | Q repair, R mesh, S guide/constraint results | 8.0.1-preview.19 |
| U | [Advanced local features](BATCH_U_ADVANCED_LOCAL_FEATURES_GAP_INVENTORY.md) | Q acceptance, S laws, T recipe execution | 8.0.1-preview.20 |
| V | [Partition/volume workflows](BATCH_V_PARTITION_VOLUME_GAP_INVENTORY.md) | Q repair, R grouped mesh, T multi-output publication | 8.0.1-preview.21 |
| W | [Lighting/material/frame review](BATCH_W_VIEWER_LIGHTING_FRAME_CAPTURE_GAP_INVENTORY.md) | R attributed mesh, existing viewer/XDE material contracts | 8.0.1-preview.22 |

Preview slots are reservations, not a version update now. ABI/bridge/schema increments
are based on actual contract changes, not inferred from preview counters. If another
authorized fix uses a slot, record the renumbering before the relevant implementation.

## Per-batch loop

1. **Recover and revalidate preparation.** Inspect Git state, STATUS, this journal,
   matrix, ADRs, source owners and actual prerequisite commits. Preserve unrelated
   user changes. Compare the frozen/current inventory, SDK and callable signatures.
   Record exact added/removed/reclassified IDs, affected rows and the new baseline hash.
   Routine in-scope baseline reconciliation is done by the agent, not a new user approval.
2. **Implement the entire closure.** Complete all 40 capabilities and supporting
   validation/interop/dependency work. Cohesive source units and internal work ordering
   do not become smaller delivery batches. Fix parser/model/rules/emitters and regenerate
   rather than editing generated output. Record exact manual exceptions and lifetimes.
3. **Repair failures inside the batch.** A compiler, test, package or reproducibility
   failure stays in this batch for diagnosis and correction. Do not skip the gate or
   move on because a demo passes, a timer expires or many files were changed.
4. **Run the complete exit gates.** Use the
   [shared gate list](BATCH_Q_T_PREPARATION.md) and the batch's 40-row acceptance map:
   Release/Debug plus actual Debug-native runtime, focused/full regression, ownership,
   real-file/HWND and applicable driver-success checks, source/dependency closure,
   generated freshness/clean regeneration, compatibility/inventory, runtime manifest,
   both clean consumers and complete local pack/release checks.
5. **Document and commit locally.** Record actual counts, artifacts/hashes and any
   NOT RUN items. Required NOT RUN gates mean incomplete, not an exception to completion.
   Update STATUS and the matrix. Stage only batch-owned changes; make one complete-batch
   commit without amending/rebasing prior user history.
6. **Advance immediately.** Record completion commit and next baseline in this journal;
   continue to the next queued batch without routine confirmation. Finish the run only
   after W passes, the user stops/changes scope, or safe progress truly needs outside input.

Fresh-build/runtime/package evidence belongs to the batch being completed. Earlier
Generator 91/91 and Runtime 180/180 baseline evidence cannot validate future Q-W APIs.
Likewise classification completeness and candidate counts never establish completion.

## Resume journal

Update this table and STATUS at meaningful progress boundaries. Tests have named report
paths/hash evidence; a short comment such as “tested” is insufficient.

| Batch | State | Implementation | Entry baseline/delta | Exit evidence | Local completion commit |
|---|---|---|---|---|---|
| Q | PREPARED | 0/40 | Preview.15 frozen; revalidate at start | NOT RUN | None |
| R | PREPARED | 0/40 | Preview.15 frozen; pending post-Q delta | NOT RUN | None |
| S | PREPARED | 0/40 | Preview.15 frozen; pending post-R delta | NOT RUN | None |
| T | PREPARED | 0/40 | Preview.15 frozen; pending post-S delta | NOT RUN | None |
| U | PREPARED | 0/40 | Preview.15 frozen; pending post-T delta | NOT RUN | None |
| V | PREPARED | 0/40 | Preview.15 frozen; pending post-U delta | NOT RUN | None |
| W | PREPARED | 0/40 | Preview.15 frozen; pending post-V delta | NOT RUN | None |

Suggested implementation state transitions:
`PREPARED -> REVALIDATED -> IMPLEMENTING -> VERIFYING -> COMMITTED -> next batch`.
If a turn/session is interrupted, record current batch/row set, precise failing command,
remaining gates and next safe action. Resume at the first incomplete batch; do not
reimplement committed work or create an extra micro-batch for leftover tests.
Set active-loop state CONTINUE while authorized work remains, not COMPLETE at every
intermediate local commit. Preparation-only completion remains a distinct state.

## Genuine stop conditions and authority boundary

Pause only when required for user stop/reprioritization, conflicting user edits that
cannot be preserved, missing access/toolchain/hardware after safe local alternatives,
a material changed/unsupported product outcome, or a new external/destructive action
outside the accepted workflow. Report exact evidence, exhausted in-scope alternatives
and the smallest required decision. Do not silently drop difficult rows or claim a
capability-failure test covers an untested required success path.

Ordinary in-scope choices, source placement, overload completion, expected manual binding
exceptions and baseline/document updates do not require repeated user confirmation.
“Continuous” does not authorize NuGet publication, GitHub push, signing/credentials,
unrelated refactors, data deletion, new projects/native DLLs, unsafe ownership, or
arbitrary native callbacks. Packing/checking is local validation only.

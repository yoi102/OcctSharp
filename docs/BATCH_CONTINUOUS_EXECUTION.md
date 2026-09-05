# Continuous implementation runbook: Q through W

- Status: **CONTINUE — Batch Q-T COMMITTED; Batch U VERIFYING**, explicitly started by the user on 2026-09-05.
- Decision: [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md).
- Queue: **Q -> R -> S -> T -> U -> V -> W**, 40 capabilities each, **280 total**.
- Current complete implementation, validation and local commits: **160/280 (57.1%)**; Q `1a3662a`, R `86e069c`, S `580bb22`, T `bfb8811`. U retains its original forty rows, has passed post-T entry, and is completing final-source exit gates under ADR-0088.
- Machine-readable queue: [continuous-plan.json](../OcctSharp/config/batches/continuous-plan.json).

## Start and continuation contract

The user explicitly started uninterrupted sequential implementation on 2026-09-05.
Continue the first incomplete batch through full validation and local commit, then
advance without routine permission. This is active task work, not a scheduler,
background service or new task.

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
| T | [Parametric recompute](BATCH_T_PARAMETRIC_DOCUMENT_RECOMPUTE_GAP_INVENTORY.md) | Q repair, R mesh and S authoring recipes | 8.0.1-preview.19 |
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
| Q | COMMITTED | 40/40 | `3491c1e`; zero product delta from `6b04bd9`; frozen 52-root audit revalidated | Focused 25/25; Release/Debug Generator 91/91 and Runtime 205/205; actual Debug-native 205/205; full release-check, 14 packages/two consumers, 106-ID exact reconciliation; final documents and package bytes verified | `1a3662a` |
| R | COMMITTED | 40/40 | `1a3662a` / Preview.16; fresh 128-header inventory matches `7917A78F`; 106 prior Blocked-to-Manual changes, nine within R roots, zero identity changes; entry reports match `1C8F1B3E` | Focused 24/24; final Release/Debug Generator 91/91 and Runtime 229/229; actual Debug-native 229/229; 36 standalone headers; 29,432 matching native exports, 404 additive managed signatures/no removals; exact 48-ID accounting; final full release-check PASS after OBJ hardening; 94-file clean regeneration and both consumers PASS | `86e069c` |
| S | COMMITTED | 40/40 | `86e069c` / Preview.17; 154 prior transitions, 21 in S roots, zero identity changes; 52 roots / 2,432 candidates; repeat hash `71D65197` | Focused 44/44 and ten repeats; Release/Debug Generator 91/91 and Runtime 273/273; actual Debug-native 273/273; 39 strict headers, six layout negatives; 500 additive managed signatures/no removals; 68-ID exact accounting; 94-file cold regeneration, both consumers and full release-check PASS. Final 172 stable docs, 73 runtime/license files and five checksums match | `580bb22` |
| T | COMMITTED | 40/40 | `580bb22` / Preview.18; 222 prior transitions, 11 in T roots; zero identity changes; repeat root hash `61E5C27D` | Focused 40/40 and ten repeats; Release/Debug Generator 91/91 and Runtime 313/313; actual Debug-native 313/313; 40 strict headers, six layout negatives; 464 additive managed signatures/no removals; 65 exact Manual transitions. Both consumers, 94-file cold regeneration and full release-check PASS. Exit hash `1A4B9369`; final 174 docs, 73 runtime/license files and five checksums match | `bfb8811` |
| U | VERIFYING | 40/40; every local gate passes, completion commit next | `bfb8811` / Preview.19; fresh inventory matches `1A4B9369`; 287 prior transitions, 16 affecting U roots, zero identity changes; 44 roots / 2,045 candidates; repeat root hash `43FF988F`; frozen Preview.15 config retained | Focused 96/96 and ten repeats; Release/Debug Generator 91/91 and Runtime 409/409; actual Debug-native 409/409; 42 strict headers and environment-restoration checks; 592 additive APIs and 11 native exports/no removals. Exact 108-ID accounting; exit hash `77900ED8`; both consumers, 94-file cold regeneration, full release-check and final 176-doc/73-runtime/five-checksum package verification PASS | Pending immediate completion commit |
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

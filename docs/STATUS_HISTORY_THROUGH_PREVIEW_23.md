# Current Status

- Last updated: 2026-09-06.
- Current work: [Batch X](BATCH_X_GENERATED_PROJECTION_EXPANSION.md), accepted under
  [ADR-0092](adr/0092-generated-numeric-and-copied-reference-expansion.md).
- Locally validated: `8.0.1-preview.23`, ABI 1.67, bridge 0.75.0; configuration schema 1.13,
  binding-model schema 1.3 and assembly/file identity 0.1.0.0 are unchanged.
- Batch X is locally complete: implementation, full configuration tests, complete local
  release check, final documentation repack and package-content validation all pass.
- Entry: W product `15fd671` / Preview.22 plus workflow maintenance `f84e869`.
  [WORKFLOW](WORKFLOW.md) / ADR-0091 remain applicable. Q-W's original 280/280 accepted
  capabilities stay complete; X does not reopen or alter that historical queue.
- Twelve managed modules, the facade, one Native DLL and fourteen local packages remain.
  This document accompanies the completed X local checkpoint; GitHub push and NuGet publication remain NOT RUN.

## Current full-library accounting

| Disposition | Preview.22 entry | Batch X exit |
|---|---:|---:|
| Emitted | 16,353 | 19,682 |
| Accepted manual | 1,217 | 1,217 |
| Blocked | 49,358 | 46,028 |
| Skipped | 49,344 | 49,345 |
| SupportedUnselected / pending / HD099 | 0 / 0 / 0 | 0 / 0 / 0 |
| Total inventoried | 116,272 | 116,272 |

Generated plus accepted manual coverage rises from 15.11% to 17.97% of all inventoried
IDs. This is declaration coverage, not functional completeness. Semantic discovery is
still 7,058/7,090 headers, with the same 32 explicit dependency/artifact exclusions.
The finite Q-W queue being complete does not mean full OCCT migration is complete.

X adds exactly 3,329 generated IDs across eleven modules and removes none. Standard
source-package attribution accounts for 2,587 additions, predominantly macro-origin
DynamicType handles; do not describe these as thousands of independent CAD features.
The module breakdown and remaining contract gaps are in the Batch X record.

Full exit inventory SHA256:
`B714207D4224DFAAD3A34CEBB9CFFE7353211779276AC084CA330A7F06A480DB`.
`eng/verify-batch-x-accounting.ps1` passes against the hash-pinned W baseline: 3,329
Blocked-to-Emitted transitions, one Cocoa Blocked-to-Skipped artifact exception,
FlatBezierKnots remaining Blocked/BL209, and 57 audited Blocked-to-Blocked reason
refinements. No other disposition, identity or header classification changes.

## Executed X validation

- PASS: Release and Debug native/managed builds; Generator 110/110 and Runtime 481/481
  in each configuration (`artifacts/batch-x-release-check.log`). Focused X is 12/12,
  covering numeric boundaries, old/new overloads, copied values, retained/null handles,
  source disposal and static null-output rejection/error clearing.
- PASS: isolated actual Debug-native Runtime 481/481, all 62 DLLs hash-verified
  (`artifacts/batch-x-actual-debug.log` and `batch-x-actual-debug/result.json`).
- PASS: all 28,747 old generated native signatures and bodies preserved; both native
  configurations expose 32,820 names, exactly 3,329 added and zero removed. Managed
  API against W adds 3,287 signatures, zero removed. The 3,461 facade forwarders remain
  current. Evidence: `batch-x-source-compatibility.log`, `batch-x-native-exports.json`,
  `batch-x-api-diff.json` under `OcctSharp/artifacts/`.
- PASS: native source-layout and bounded-size exception fixtures; all twenty generated
  C ABI headers compile together as strict C11. All 94 generated files pass in-place
  freshness/determinism. A clean source copy rebuild passes Generator 110/110 and
  Runtime 481/481; all 94 generated files are byte-identical.
- PASS: four accounting rejection cases (lost denominator, unrelated reclassification,
  a Blocked refinement promoted to Skipped and header drift), without mutating source
  inputs (`artifacts/batch-x-accounting-negatives.log`).
- PASS: fourteen local Preview.23 packages and fresh facade/direct-module consumers,
  including the shared X public numeric/lifetime workflow; no facade in the direct
  module consumer. Both consumers pass again after final documentation repacking.
  Package content verifies 187 stable documents, 73 runtime/license files, five
  checksums and final provenance (`artifacts/batch-x-document-package-final.log` and
  `artifacts/release/package-content.json`). Final facade package SHA256:
  `AD649C5AD9B60810116E93B12A7214004A7C21A0B9C46AF352C5CD440B3EF35D`.
- PASS: bundled Release bridge matches the built runtime, all 62 DLL and eleven
  license/notice hashes verify. Release bridge is 17,345,024 bytes, SHA256
  `A6BFAA92EAA23488AAA9C09D5A4596E7278DBB4D9FDC5A47F4DCB63118D1B642`.
  Debug bridge SHA256: `7BA57D0DA94A70B6A612A0BF154B70850E585BB97F28490F513C04B1DC9407A2`.
- Hosted CI, signing, NuGet publication and GitHub push: NOT RUN, outside this task.
- PASS: complete local release check (`artifacts/batch-x-release-check.log`), with
  `batchImplementationComplete=true`; its independent final full inventory reproduces
  the exact exit hash above. Public-release readiness remains false for the external
  NOT RUN gates; this does not change the completed local implementation result.

## Next action

No Batch X implementation or local delivery gate remains. Final documentation QA passes
188 documents and 423 local links, paired fences and `git diff --check`. STATUS is a
live local record and is excluded from the packaged stable-document set. Further migration can audit explicit pointer/output, array/value and nested
handle-target gaps; no new fixed-size queue is selected or started by this checkpoint.

## Historical evidence

Preview.22's detailed product results and earlier batch narratives remain in
[STATUS history](STATUS_HISTORY_THROUGH_PREVIEW_22.md). Workflow maintenance evidence
is preserved in `f84e869` and [ADR-0091](adr/0091-proportionate-development-workflow.md).
Historical counts and prompts do not create a new active migration queue.

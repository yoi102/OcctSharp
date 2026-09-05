# Current Status

- Last updated: 2026-09-06
- Current policy: [WORKFLOW](WORKFLOW.md), accepted in
  [ADR-0091](adr/0091-proportionate-development-workflow.md).
- Product baseline: `15fd671`, Batch W, `8.0.1-preview.22`; ABI 1.66,
  bridge 0.74.0, schema 1.13. Twelve managed modules plus the facade and one Native DLL.
- B-W are locally complete for their accepted scopes. Q-W is 280/280 (100%),
  seven original forty-capability batches. No product implementation remains in that queue.
- This is not full OCCT migration or public-release readiness. Current work is the
  authorized workflow/documentation and source-layout-verifier adjustment, not a new
  product batch or a new package. Product/generated code and runtime identities are unchanged.
- Historical checkpoint narratives and their original validation evidence are retained
  in [STATUS history](STATUS_HISTORY_THROUGH_PREVIEW_22.md). Do not resume its old loop states.

## Current coverage baseline

| Disposition | Declaration IDs |
|---|---:|
| Emitted | 16,353 |
| Accepted manual | 1,217 |
| Blocked | 49,358 |
| Skipped | 49,344 |
| SupportedUnselected / pending / HD099 | 0 / 0 / 0 |
| Total inventoried | 116,272 |

7,058 of 7,090 headers were semantically scanned; 32 retain explicit dependency/artifact
exclusions. Classification completeness does not mean complete semantic discovery or
API migration. Full inventory SHA256:
`260AE9A603E7B68F717C38C3FAF9C68A83F866D0154E84E1CD7FD811DB0C6A2C`.
All counts above belong to the last validated product baseline, unchanged by ADR-0091.

## Current maintenance validation

ADR-0091 introduces flexible task/commit sizing, impact-based development validation,
one current progress summary and bounded source-size exceptions with rationale. It
retains generated-code, ABI/ownership and complete product-delivery validation rules.

- PASS: source-layout verification of 86 independent native units, 617 manual C exports
  and 23 unique shared-state definitions; no current size exceptions.
- PASS: six existing rejection fixtures; one bounded-size acceptance/reporting case
  and seven new rejection cases, including shared-state validation in an excepted file.
- PASS: two PowerShell syntax checks; 185 Markdown documents and 413 local file links,
  paired fences, exact LF-normalized preservation of historical STATUS/archived prompt,
  current-state consistency and `git diff --check`. Product/generated/runtime files and
  frozen batch configuration have no diff.
- Product builds, runtime suites, regeneration and package delivery: NOT RUN for this
  maintenance change. The product evidence below was recorded at Preview.22, not rerun.
- No package version change, repack, local commit, NuGet publication or GitHub push has
  been performed for this maintenance change. Existing package bytes contain their
  historical documents, not these updated workflow documents.

## Latest product validation evidence (Preview.22)

### Preview.22 Batch W local validation

- Original forty rows are implemented and mapped to named assertions. Final-source
  focused 23/23 and ten repeats pass. Release/Debug Generator 91/91 and Runtime 469/469
  pass. The isolated actual Debug-native run also passes 469/469 with all 62 DLLs
  hash-verified (`artifacts/batch-w-actual-debug.log`). Full release-check passes in
  `artifacts/batch-w-release-check.log`, including a clean-source build with the same
  91/91 + 469/469 tests and all 94 generated files byte-identical.
- Source gates pass: 86 independent native units, 617 manual C exports, 23 unique
  storage definitions, 44/44 strict standalone headers, six layout negatives and
  header-verifier environment restoration. New C record sizes/offsets pass the
  standalone layout probe, pure C header compilation and ten managed Marshal sizes.
  Release/Debug exports match 29,491 names: sixteen added,
  none removed. Managed API comparison against V adds 318 signatures, removes zero.
  The facade regenerates 3,461 type forwarders; frame DTOs have no WPF dependency.
- Bundled Release bridge is 15,994,368 bytes, SHA256
  `72239B4632872D26BA5F742A765A0E535C154E3618287342A20FC1BDDAF73E0D`.
  Actual Debug bridge SHA256 is
  `7C16B78641A1040F0E730FABC3145789FEAE8FCFC92C5C2821D20EA8D5BC0EF4`.
  The runtime manifest checks all 62 DLLs and eleven license/notice files.
- Final WPF build and `--snapshot-smoke` pass; `artifacts/batch-w-wpf-final.png` is
  visually inspected. Final real STEP/XDE source/review captures in
  `artifacts/batch-w-viewer-final/` are inspected; exact reset and source material/
  topology invariance pass. All images remain local test artifacts.
- Frozen Q-W preparation still passes 280 rows, 223 SDK headers, 7,668 unique candidates,
  fourteen negative cases and sixteen representative SDK symbols. Original scope and
  baseline are unchanged. Independent W exit inventory/accounting and three negatives
  pass: exactly 86 Blocked-to-Manual transitions, no other changes. Hash:
  `260AE9A603E7B68F717C38C3FAF9C68A83F866D0154E84E1CD7FD811DB0C6A2C`.
  Emitted 16,353 / Manual 1,217 / Blocked 49,358 / Skipped 49,344; no pending states.
  Exit inventory/API/DLL copies are in `artifacts/preparation-baselines/preview22-batch-w/`.
  The complete release-check independently reproduces the same inventory hash.
- Fourteen local packages and both fresh-cache consumers pass, including W's shared
  public XDE workflow and direct Visualization frame use without facade/WPF. Final
  documentation repack and both consumers pass again in
  `artifacts/batch-w-document-package-final.log`. Package-content validation verifies
  180 stable documents, 73 runtime/license files and five checksums/provenance.
  Final facade package SHA256:
  `0B4DCB376CC28F427E14E937765297E0557EE05D545DB53345B47908AD629E28`.
  All 397 local Markdown links, paired fences and whitespace checks pass. This
  whole-batch checkpoint closes the authorized Q-W run at 280/280 (100%).
  Hosted CI, signing, NuGet publication and GitHub push are NOT RUN and outside scope.

## Next action

Apply WORKFLOW to the next user-authorized task. For further full-library migration,
audit remaining blocked projection/ownership/dependency groups and select a useful
generator or product scope; do not reopen the completed Q-W queue or invent a fixed
forty-row successor. Hosted CI, signing and publication remain separate NOT RUN work.

## Current execution state

```text
PRODUCT_BASELINE: 15fd671 / PREVIEW.22 / B THROUGH W LOCALLY VALIDATED
PRODUCT_QUEUE: Q-W COMPLETE; 280/280 ACCEPTED CAPABILITIES
CURRENT_WORK: ADR-0091 MAINTENANCE COMPLETE; CHANGES UNCOMMITTED
MIGRATION_SCOPE: PARTIAL OCCT API COVERAGE; 16353 GENERATED PLUS 1217 MANUAL
NEXT_PRODUCT_SCOPE: NOT SELECTED
PUBLICATION: NOT RUN
```

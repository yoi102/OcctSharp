# Current Status

- Last updated: 2026-09-12.
- Current work: [Batch AC](BATCH_AC_OWNED_UTILITY_EXPANSION.md). Implementation and
  Release tests pass; full local delivery checks are in progress.
- Candidate: 8.0.1-preview.28 / ABI 1.72 / bridge 0.80.0.
- Entry: c55a969, completed Preview.27;
  [its evidence is archived](STATUS_HISTORY_THROUGH_PREVIEW_27.md).
- Twelve modules, one facade, one Native DLL and fourteen packages remain the product layout.
- GitHub push, hosted CI, signing and NuGet publication: NOT RUN, outside this task.

## Implemented migration

AC adds 1,924 exact generated IDs and 225 owned classes across 53 OCCT packages and
nine modules. The generated manifest currently contains 25,627 IDs and 155 files.
The final full-inventory reconciliation is still pending; entry inventory has 116,272
declarations, 1,217 accepted Manual IDs and 7,058/7,090 headers with 32 exclusions.

The new catalogue covers analytic spline conversion/projection, surface construction,
intersection and interval workflows, HLR, wire ordering, initialized numeric/color/date
values, 88 STEP SELECT containers and independent exchange/mesh/material settings.
Registry lifetime and copied-reference publication reuse ADR-0096. New const receivers
resolve const overloads; a frozen owner epoch preserves AB bodies. Supporting inherited
operations, typed SELECT setters, factories and releases are counted separately from
migrated declarations. Full-library migration is incomplete.

## Executed checks

- PASS: Release native and managed build/link; Generator 177/177; Runtime 568/568,
  including 26 new AC tests.
- PASS: deterministic generation and complete, acyclic module dependency closure.
- PASS: 36,408 previous generated native functions and header declarations preserved,
  including 122 old owned release functions; no changed bodies/prototypes or removals.
- PASS: 3,633 managed API additions and zero removals, for 55,076 signatures and
  4,097 facade type forwarders. New native source exports: 3,408.
- PASS: Release/Debug C11 checks for 40 headers and registry fixtures with 250 release
  races / 16,000 acquisitions per configuration; final-header recheck follows delivery.
- PASS: bundled 62-DLL runtime matches the current Release build.
- IN PROGRESS: full release-check (Release/Debug, clean regeneration, package consumers,
  fresh full inventory and release metadata).
- NOT RUN: actual Debug-native AC runtime suite and final package-content verification.

The current Release bridge is 21,829,632 bytes with SHA256
`FBB48038700D99DDDA301CF68DD35A1C5560DDA2DD4E2DED0AAE4BA62F538A98`.
These checks do not yet mark the AC local delivery complete.

## Remaining migration

Borrowed adaptors, retained external buffers, general sequences and mutable owning
outputs remain future work. Four declared SDK methods lack both Release and Debug
symbols and remain Blocked; see the batch record. Follow [WORKFLOW](WORKFLOW.md)
for impact-based verification and useful batch sizing without a fixed capability count.

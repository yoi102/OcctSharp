# Current Status

- Last updated: 2026-09-12.
- Current work: [Batch AC](BATCH_AC_OWNED_UTILITY_EXPANSION.md). Implementation and
  Release/Debug tests pass; local delivery checks are complete. Public-release
  readiness remains false because hosted CI, signing and publication are not run,
  and 32 SDK headers remain explicit Clang parse exclusions.
- Candidate: 8.0.1-preview.28 / ABI 1.72 / bridge 0.80.0.
- Entry: c55a969, completed Preview.27;
  [its evidence is archived](STATUS_HISTORY_THROUGH_PREVIEW_27.md).
- Twelve modules, one facade, one Native DLL and fourteen packages remain the product layout.
- GitHub push, hosted CI, signing and NuGet publication: NOT RUN, outside this task.

## Implemented migration

AC adds 1,924 exact generated IDs and 225 owned classes across 53 OCCT packages and
nine modules. The generated manifest currently contains 25,627 IDs and 155 files.
The final coverage manifest classifies 116,263 parsed declarations: 61,571 Pending,
32,008 Skipped, 21,467 Supported and 1,217 accepted Manual IDs. It emits 25,627 IDs
across 155 files. The full inventory scanned 7,058/7,090 headers; the remaining 32
are explicit parser exclusions. This is an accounting snapshot, not a claim that all
OCCT APIs are migrated.

The new catalogue covers analytic spline conversion/projection, surface construction,
intersection and interval workflows, HLR, wire ordering, initialized numeric/color/date
values, 88 STEP SELECT containers and independent exchange/mesh/material settings.
Registry lifetime and copied-reference publication reuse ADR-0096. New const receivers
resolve const overloads; a frozen owner epoch preserves AB bodies. Supporting inherited
operations, typed SELECT setters, factories and releases are counted separately from
migrated declarations. Full-library migration is incomplete.

## Executed checks

- PASS: Release/Debug native and managed build/link; Generator 177/177; Runtime 568/568,
  including 26 new AC tests.
- PASS: deterministic generation and complete, acyclic module dependency closure.
- PASS: 36,408 previous generated native functions and header declarations preserved,
  including 122 old owned release functions; no changed bodies/prototypes or removals.
- PASS: 3,633 managed API additions and zero removals, for 55,076 signatures and
  4,097 facade type forwarders. Native export audit: 40,569 exports, with 3,408 generated
  C ABI additions and eight exact MSVC constructor closures; zero removals and Debug parity.
- PASS: final Release/Debug C11 checks for 40 headers, geometric ABI fixtures and registry
  fixtures with 250 release races / 16,000 acquisitions per configuration.
- PASS: actual Debug-native Runtime 568/568 in an isolated directory; all 62 DLL hashes checked.
- PASS: positive and 11 rejection accounting fixtures using an isolated synthetic inventory.
- PASS: bundled 62-DLL runtime matches the current Release build.
- PASS: clean source rebuild and full suites, with all 155 generated files byte-identical.
- PASS: fourteen Preview.28 packages and both clean facade/direct-module consumers.
- PASS: full release-check final inventory/classification, release metadata, checksums
  and provenance; 32 parser-limited headers are explicitly excluded.
- PASS: final package-content verification (203 stable documents, 73 runtime/license
  files, five checksums and final provenance).

The current Release bridge is 21,829,632 bytes with SHA256
`FBB48038700D99DDDA301CF68DD35A1C5560DDA2DD4E2DED0AAE4BA62F538A98`.
These checks mark the AC local engineering delivery complete. They do not make this a
full OCCT migration or a public release.

## Remaining migration

Borrowed adaptors, retained external buffers, general sequences and mutable owning
outputs remain future work. Four declared SDK methods lack both Release and Debug
symbols and remain Blocked; see the batch record. Follow [WORKFLOW](WORKFLOW.md)
for impact-based verification and useful batch sizing without a fixed capability count.

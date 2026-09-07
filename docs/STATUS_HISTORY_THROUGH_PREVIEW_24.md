# Current Status

- Last updated: 2026-09-06.
- Current batch: [Y](BATCH_Y_GEOMETRIC_VALUE_EXPANSION.md), implemented under
  [ADR-0093](adr/0093-generated-geometric-value-projections.md) and WORKFLOW.
- Locally validated: `8.0.1-preview.24`, ABI 1.68, bridge 0.76.0. Configuration 1.13,
  binding model 1.3 and assembly/file 0.1.0.0 are unchanged.
- Entry: Batch X `2cbe55e`; implementation checkpoint: `dda4c3b`.
  [Preview.23 status history](STATUS_HISTORY_THROUGH_PREVIEW_23.md) retains X evidence.
- Batch Y is locally complete: implementation, full product checks, final documentation
  repack and package-content verification pass. This document accompanies its local
  completion checkpoint.
- Twelve modules, one facade, one Native DLL and fourteen local packages remain.
  GitHub push and NuGet publication are NOT RUN, outside this task.

## Full-library accounting

| Disposition | Preview.23 entry | Batch Y exit |
|---|---:|---:|
| Emitted | 19,682 | 20,650 |
| Accepted manual | 1,217 | 1,217 |
| Blocked | 46,028 | 45,058 |
| Skipped | 49,345 | 49,347 |
| SupportedUnselected / pending / HD099 | 0 / 0 / 0 | 0 / 0 / 0 |
| Total inventoried | 116,272 | 116,272 |

Generated plus accepted manual declaration coverage rises from 17.97% to 18.81%.
This is declaration coverage, not functional completeness. Semantic discovery remains
7,058/7,090 headers, with the same 32 explicit exclusions. Full OCCT migration remains
incomplete; no retired Q-W capability count is reused as a full-library percentage.

Y adds 968 generated IDs across eight modules and thirty geometric value records in
Foundation. There are no generated removals or new manual IDs. The exact audit validates
968 Blocked-to-Emitted transitions, two artifact exceptions and 223 Blocked-to-Blocked
reason refinements. Refined reasons and the incidental SDK C++ export do not count as
migration. SC-062 records the two remaining projection contracts and exact exclusions.
Final inventory SHA256:
`CCCD42562A55D0345518F446D8D0B5746CC047B88CAC849F3E0CE337793EE5FC`.

## Executed validation

- PASS: full Release and Debug native/managed builds; Generator 144/144 and Runtime
  493/493 in each configuration. Focused Y is 12/12. Evidence:
  `artifacts/batch-y-release-check.log`, `batch-y-focused.log` and `batch-y-generator-tests.log`.
- PASS: isolated actual Debug-native Runtime 493/493, all 62 DLLs hash-verified
  (`artifacts/batch-y-actual-debug.log` and `batch-y-actual-debug/result.json`).
- PASS: every one of 32,076 X generated native signatures/bodies preserved. Native
  Release/Debug sets match at 33,789 names, zero removed: 968 new C bindings plus one
  exact SDK inline C++ export audited in SC-062. Managed API against X adds 1,085
  signatures, zero removed, 47,638 total. All 3,491 facade forwarders are current.
  Evidence: `batch-y-source-compatibility.log`, `batch-y-native-exports.json`,
  `batch-y-native-exports.log`, `batch-y-api-diff.json` under `OcctSharp/artifacts/`.
- PASS: thirty native/managed size and field-offset contracts, non-symmetric matrix
  semantics, and all 21 generated headers as strict C11 in both configurations
  (`artifacts/batch-y-native-layout.log`). Geometry input/output, large directions,
  handedness, derivatives, copied lifetime, Documents/XDE/IGES and error handling pass.
- PASS: all 96 generated files pass freshness/determinism; a clean source copy builds
  and passes Generator 144/144 and Runtime 493/493, with all 96 files byte-identical.
  Generated dependencies remain resolved, acyclic and within the existing module graph.
- PASS: fourteen Preview.24 local packages; clean facade and direct-module consumers,
  including the shared Y geometric workflow. The direct consumer receives no facade DLL.
  Both pass again after final stable-document repacking. Package content verifies
  191 stable documents, 73 runtime/license files, five checksums and final provenance
  (`artifacts/batch-y-document-package-final.log`, `artifacts/release/package-content.json`).
  Final facade package SHA256:
  `AD11D910FC99DC93A796B52DBF396C0F554EF02A9AB5F11F5FDDCD9966F920E6`.
- PASS: exact accounting against the hash-pinned X baseline (`eng/verify-batch-y-accounting.ps1`
  and `artifacts/batch-y-accounting.json`), with unchanged declaration identities/denominator
  and all header classifications. Four negative fixtures reject a lost denominator,
  unrelated reclassification, Blocked-to-Skipped refinement and header drift while
  preserving the source input (`artifacts/batch-y-accounting-negatives.log`).
- PASS: bundled Release closure matches the built bridge; all 62 DLLs and eleven
  notices/licenses verify. Bridge size: 17,865,728 bytes; Release SHA256:
  `7B84DBD5565AF41030E5E037A3921197B493A3FA36E52C0368FDF9E0DBE0338E`.
  Debug SHA256: `8838247CA8820079B3BC3395727DCC230605BEE252492EBFFB7965DC70DEC34D`.
- PASS: complete local release check, `batchImplementationComplete=true`.
  Hosted CI, signing, NuGet publication and GitHub push are NOT RUN; public release
  readiness remains false for these external gates.

## Next action

No Y implementation or local delivery gate remains. Documentation QA verifies 192
documents, 432 local links and paired fences; whitespace checks pass. STATUS is excluded
from the stable package documents to avoid a self-referential package hash. Future
migration can address explicit output/array and unselected handle/constructor contracts;
no further fixed-size migration queue is active.

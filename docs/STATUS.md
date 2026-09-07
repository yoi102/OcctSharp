# Current Status

- Last updated: 2026-09-07.
- Active batch: [Z](BATCH_Z_VALUE_OPERATIONS.md), implemented under
  [ADR-0094](adr/0094-generated-geometric-value-operations.md).
- Target: 8.0.1-preview.25 / ABI 1.69 / bridge 0.77.0. Full delivery checks are in progress.
- Entry product: Y 2083f99, plus README update 845e3d6. Its validated evidence remains in
  [Preview.24 history](STATUS_HISTORY_THROUGH_PREVIEW_24.md).
- Twelve modules, one facade, one Native DLL and fourteen local packages remain.
  GitHub push and NuGet publication are NOT RUN, outside this task.

## Full-library accounting

| Disposition | Preview.24 entry | Batch Z |
|---|---:|---:|
| Emitted | 20,650 | 22,038 |
| Accepted manual | 1,217 | 1,217 |
| Blocked | 45,058 | 43,670 |
| Skipped | 49,347 | 49,347 |
| SupportedUnselected / pending / HD099 | 0 / 0 / 0 | 0 / 0 / 0 |
| Total inventoried | 116,272 | 116,272 |

Generated plus accepted manual declaration coverage rises from 18.81% to 20.00%.
This is declaration coverage, not functional completeness. Semantic discovery remains
7,058/7,090 headers with the same 32 explicit exclusions. Full migration is incomplete.

Z adds 1,388 generated IDs and removes none. There are no new manual IDs or exclusions.
The exact audit validates 1,388 Blocked-to-Emitted transitions and seven audited
Blocked-to-Blocked reason refinements; refinements do not count as migration.
Inventory SHA256: `C81FD10258895F765187AEC809FD3ECB337ED2776E70EF39669F27CF6EF1E75B`.

## Executed validation

- PASS: Release native/managed build and deterministic generation/discovery; 99 files.
- PASS: Generator Release 153/153 and full Runtime Release 517/517 (including 24 Z tests).
- PASS: Release and Debug geometric ABI fixtures, 22 strict C11 headers, 34 record layouts.
- PASS: 33,044 old generated native signatures/bodies unchanged; 1,487 managed API
  additions with zero removals; 3,530 facade forwarders current.
- PASS: 14 local packages and clean facade/direct-module consumers, including Z workflow.
- PASS: exact inventory accounting and four rejection fixtures; unchanged denominator, header and manual-ID sets.
- Pending: complete final Release/Debug suites, actual Debug-native, final export parity,
  cold regeneration and full local release-check.
  These delivery gates are NOT RUN to completion; Z is not yet a completed product batch.

## Implemented scope

35 copied geometric operation families cover constructors, queries and ref mutations.
Four new 2D/3D similarity/affine coefficient records support translation, rotation,
reflection, scale, composition and inverse workflows. Failed mutations preserve the
managed receiver. SC-063 records explicit singular-inverse guards. Mutable output
references, arrays, pointers, operators and native form-cache editing remain outside
this rule. See [Batch Z](BATCH_Z_VALUE_OPERATIONS.md) and [type mapping](TYPE_MAPPING.md).

## Next action

Finish Debug/runtime/export and cold/package release checks, record final evidence,
repack current documentation and create the local completion commit.

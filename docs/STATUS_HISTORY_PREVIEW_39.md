# Current Status

- Last updated: 2026-10-07 (Asia/Tokyo).
- **All four authorized AL-AO batches are locally delivered: 4/4, none remaining.**
- Current local delivery: **8.0.1-preview.39 / ABI 1.82 / bridge 0.90.0**.
- Previous delivery: [Preview.38 / AN](STATUS_HISTORY_PREVIEW_38.md).
- Source remains uncommitted; no push, signing or publication occurred.

## Completed round

| Batch | Closed workflow | Local delivery |
|---|---|---|
| AL | Copied matrices, private numeric solves and spline composition | Preview.36, PASS |
| AM | Private spline caches, coefficients and coupled interpolation | Preview.37, PASS |
| AN | Private STEP/IGES writers, copied diagnostics and independent file readback | Preview.38, PASS |
| AO | SDK exclusion evidence, concurrent native bootstrap and frozen package inputs | Preview.39, PASS |

The [AL-AO plan](NEXT_ROUND_AL_AO_PLAN.md) is complete. Contracts remain in
[AL](BATCH_AL_NUMERIC_MATRICES.md), [AM](BATCH_AM_SPLINE_WORKSPACES.md),
[AN](BATCH_AN_WRITER_DIAGNOSTICS.md), and [AO](BATCH_AO_REPRODUCIBLE_DELIVERY.md).
Each batch has a frozen `OcctSharp/artifacts/batch-<id>/delivery-result.json`.
These engineering deliveries do not establish full SDK migration or public release.

## Current validation and coverage

Release, Debug and cold-source Release each pass **238 Generator / 924 Runtime**,
with zero failures or skips. Isolated actual Debug-native passes **924/924**, with
all 62 actually loaded DLL paths/hashes verified. All 204 generated files and the
manifest are byte-identical to AN and clean regeneration. Dependency closure is
complete and acyclic. Exports remain 41,267 with Debug parity; managed API remains
56,413 and 4,214 facade forwarders are current. Both configurations pass 53 C11
headers, 34 layouts and 250 release races / 16,000 acquisitions; all 45 private
headers compile independently.

Fresh inventory: 116,272 declarations; 26,411 Emitted, 1,218 Manual, 39,296 Blocked,
49,347 Skipped and zero Pending. The round adds 89 emitted IDs (42 AL + 31 AM + 16
AN); AO changes no declaration states. The same 32 parser failures remain. The
[SDK ledger](BATCH_AO_SDK_BLOCKERS_LEDGER.json) distinguishes 33 configured header
exclusions from those failures, retains 17 absent-symbol exclusions and two prior
symbol deferrals, and proves the distinct available overloads.

Thirteen package snapshot fixtures pass. Real concurrent native bootstrap proves
nonoverlapping critical sections, missing bridge recovery and failure lock release.
A real concurrent document addition is rejected with zero candidate packages promoted.
All **14 final packages and both clean consumers pass**. All 62 native and 13 managed
payload identities match build, bundle, packages and consumers. Frozen source and
document identities, package-input manifest, provenance, SBOM and all fourteen
package checksums pass. Final evidence: `OcctSharp/artifacts/batch-ao/delivery-result.json`.

Hosted CI, signing, push, public NuGet and full downstream application acceptance:
**NOT RUN**. Completed historical plans do not implicitly queue more implementation.

## Progress review and next planning round (2026-10-07)

The [Preview.39 progress review](PROGRESS_PREVIEW_39.md) and
[reconciled module counts](PROGRESS_PREVIEW_39_COUNTS.json) account for every final
declaration: 27,629 Emitted + Manual, 39,296 Blocked and 49,347 Skipped. Coverage is
23.76% of inventoried declarations, or 41.28% after excluding Skipped; neither is a
product-completion percentage. Full semantic scan remains incomplete despite final
Pending=0, because 32 headers fail parsing and undiscovered declarations are uncounted.

The user-requested [AP-AU plan](NEXT_ROUND_AP_AU_PLAN.md) proposes six substantial
workflow batches. All six are now **AUTHORIZED / IN PROGRESS**; AP is implementing,
0/6 delivered. [AP's exact audit input](BATCH_AP_MODELING_CANDIDATES.json)
contains 140 Blocked candidates, with 59 Emitted and two Manual reused dispositions;
these are not admission promises. [AP admission](BATCH_AP_MODELING_ADMISSION.json)
selects 65 candidates and defers 75, with one inherited done-gate prerequisite.
AQ-AU have family/workflow plans, not frozen
declaration sets. This planning revision is documentation/accounting maintenance;
it does not change or rerun the frozen Preview.39 delivery. New documents are not
contained in those packages and must enter the next package-content audit.

## Concurrent Cadoryx work

The separate [Cadoryx A5 contract](CADORYX_A5_PACKAGE_CONTRACT.md),
[source tessellation](CADORYX_A5_SOURCE_TESSELLATION.md) and
[incoming snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md) remain authoritative.
This SDK delivery does not establish whole-application or physical display acceptance.

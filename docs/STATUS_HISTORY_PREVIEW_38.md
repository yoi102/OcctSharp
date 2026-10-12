# Preview.38 / Batch AN delivery history

- Last updated: 2026-10-07 (Asia/Tokyo).
- **Batch AN local engineering delivery is complete**.
- Last delivered: **8.0.1-preview.38 / ABI 1.82 / bridge 0.90.0**.
- Previous delivery: [Preview.37 / AM](STATUS_HISTORY_PREVIEW_37.md).
- Source remains uncommitted; no push, signing or publication occurred.

## Scope and evidence

[AN writer diagnostics](BATCH_AN_WRITER_DIAGNOSTICS.md) admits 6 of 7 candidates,
retains 1 deferral and adds 10 writer prerequisites. Six already emitted projection
helpers are reused separately. The exact delta is +16/-0 IDs: 26,411 IDs / 204
files, with all 201 baseline generated files byte-identical. Private one-epoch
STEP/IGES sessions deep-copy source geometry and publish immutable managed snapshots
with separate transfer/write statuses and exact source/map/model identity domains.

Release, Debug and cold-source Release each pass **238 Generator / 924 Runtime**,
zero failures or skips. Isolated actual Debug-native passes **924/924**, with all
62 actually loaded DLL paths/hashes verified. Regeneration is byte-identical. Both
configurations pass 53 C11 headers, 34 geometric layouts and 250 release races /
16,000 acquisitions; all 45 private headers compile independently. Template-bound
native probes additionally validate binder lists/chains and cloned diagnostics.
Exports: 41,267 (+4/-0), with Debug parity. Managed API: 56,413 (+84/-0), with
4,214 facade forwarders. Dependency closure is complete and acyclic.

Fresh inventory: 116,272 declarations; 26,411 Emitted, 1,218 Manual, 39,296 Blocked,
49,347 Skipped and zero Pending. Exactly 16 Blocked -> Emitted transitions; the same
32 parser failures remain. No full SDK migration claim follows.

All **14 final packages and both clean consumers pass**. Their shared AN workflow
writes real STEP/IGES files and independently reads back detached geometry. All 62
native and 13 managed payloads match build, bundle, packages and consumers. Final
package documents, inventories, notices, provenance, SBOM and checksums pass.
Evidence: `OcctSharp/artifacts/batch-an/delivery-result.json`.
Hosted CI, signing, push, public NuGet and full downstream acceptance: **NOT RUN**.

## Remaining authorized work

The [AL-AO round](NEXT_ROUND_AL_AO_PLAN.md) has **three delivered batches (AL-AM-AN)**
and **one remaining (AO)**. AO starts now: precise SDK exclusion ledger,
concurrent native bootstrap and source-bound package inputs, followed by complete
local delivery. Read-only audits reproduce 32 parser failures, 17 configured missing
symbol exclusions and two AK symbol deferrals. The 33 configured excluded headers
also contain a parseable ExprIntrp interface header; this is not a 33rd parser failure.

## Concurrent Cadoryx work

The separate [Cadoryx A5 contract](CADORYX_A5_PACKAGE_CONTRACT.md),
[source tessellation](CADORYX_A5_SOURCE_TESSELLATION.md) and
[incoming snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md) remain authoritative.
This SDK delivery does not establish whole-application or physical display acceptance.

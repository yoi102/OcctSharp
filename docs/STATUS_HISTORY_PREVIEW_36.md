# Preview.36 / Batch AL delivery history

- Last updated: 2026-10-06 (Asia/Tokyo).
- **Batch AL local engineering delivery is complete**, including final package
  source-content auditing. [AL contract](BATCH_AL_NUMERIC_MATRICES.md).
- Candidate: 8.0.1-preview.36 / ABI **1.80** / bridge **0.88.0**.
- Previous delivery: [Preview.35 / Batch AK](STATUS_HISTORY_PREVIEW_35.md).
- Source remains uncommitted. No push, signing or publication occurred.

## Scope

AL audits all 54 matrix/solver/banded candidates: **41 admitted / 13 deferred**,
plus one new generated factorization prerequisite. The 25 existing matrix declarations
are reused. Exact generated delta: **42**, no removals. The manifest contains
**26,364 IDs / 198 files**; managed dependency closure is complete and acyclic.

## Verification

Release, Debug and fresh-source Release each pass **230 Generator / 888 Runtime**
tests, zero failures or skips. Isolated actual Debug-native passes **888/888**;
all **62 loaded DLL paths/hashes** match the Debug build. All 198 generated files
and the manifest regenerate byte-identically; all 195 previous files remain unchanged.

Native exports: **41,253**, +11/-0, Debug parity.
Managed API: **56,303 signatures**, +59/-0;
4,203 facade forwarders are current. Both configurations pass 51 C11 headers,
34 geometric layouts and 250 release races / 16,000 acquisitions. All 45 private
headers compile independently.

Fresh inventory: 116,272 declarations; 26,364 Emitted, 1,218 Manual,
39,343 Blocked and 49,347 Skipped; zero Pending, the same 32 parser exclusions.
Exactly 42 declarations change from Blocked to Emitted. This is not full migration.

The unified local release check creates 14 packages and runs both clean public-only
consumers. The shared workflow composes basis evaluation, dense/banded solving,
sampled spline position/derivative checks and independent STEP readback.
All 62 native and 13 managed payloads match build, bundle, packages and consumers.
The final packages contain 269 stable Markdown documents, 11 JSON inventories and
11 notices, with exact source identity, provenance, SBOM and checksums verified.
Evidence: OcctSharp/artifacts/batch-al/delivery-result.json and hash-bound reports.

Hosted CI, signing, push, public NuGet and full downstream application acceptance:
**NOT RUN**.

## Remaining authorized work

The AF-AK round has six local deliveries and zero remaining batches.
The [AL-AO round](NEXT_ROUND_AL_AO_PLAN.md) has **one delivered batch (AL)**,
and **three remaining batches**: AM spline workspaces, AN private writer
diagnostics and AO SDK/build/package reproducibility. All are authorized.
Finish each local delivery before expanding implementation; deferred candidates
retain explicit reasons and are not counted as migrated.

## Concurrent Cadoryx work

The incoming main-checkout Cadoryx A5 status is preserved verbatim in
[its integration snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md).
AK delivery does not change the independent application acceptance decisions.
Later concurrent progress remains in [the A5 contract](CADORYX_A5_PACKAGE_CONTRACT.md)
and [source tessellation record](CADORYX_A5_SOURCE_TESSELLATION.md). Main integration
does not establish the separate A5 performance or whole-application acceptance gates.

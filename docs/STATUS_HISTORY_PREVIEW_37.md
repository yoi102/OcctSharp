# Preview.37 / Batch AM delivery history

- Last updated: 2026-10-06 (Asia/Tokyo).
- **Batch AM local engineering delivery is complete**, including final package
  content and payload identity. [AM contract](BATCH_AM_SPLINE_WORKSPACES.md).
- Last delivered: **8.0.1-preview.37 / ABI 1.81 / bridge 0.89.0**.
- Previous delivery: [Preview.36 / AL](STATUS_HISTORY_PREVIEW_36.md).
- Source remains uncommitted. No push, signing or publication occurred.

## Scope and evidence

AM admits 31 of 136 candidates; 105 retain explicit deferral reasons. Three native
implementation helpers are tracked separately and are not counted as new migration.
The exact manifest delta is +31/-0: 26,395 IDs / 201 files. All 198 prior generated
files remain byte-identical. Dependency closure is complete and acyclic.

Release, Debug and cold-source Release each pass **234 Generator / 911 Runtime**,
zero failures or skips. Isolated actual Debug-native passes **911/911**, with all
62 actually loaded DLL paths/hashes verified. All 201 generated files and manifest
regenerate byte-identically. ABI checks pass in both configurations: 52 C11 headers,
34 geometric layouts, 250 release races / 16,000 acquisitions. All 45 private
headers compile independently. Native exports: 41,263 (+10/-0), Debug parity.
Managed API: 56,329 (+26/-0); 4,205 facade forwarders are current.

The unified entry point passed both suites and smoke, then hit a concurrent shared
forwarder-check-file lock. Unique temporary filenames fix the tool; two concurrent
checks pass. Hash-bound unchanged product inputs allow continuation from
verify-generated through all remaining gates, including cold suites and inventory.
Both the failed log and successful continuation remain in the evidence.

Fresh inventory: 116,272 declarations; 26,395 Emitted, 1,218 Manual, 39,312 Blocked,
49,347 Skipped; zero Pending and the same 32 parser failures. Exactly 31 declarations
change Blocked -> Emitted. This does not establish full SDK migration.

All **14 final packages and both clean consumers pass**. The shared AM workflow
composes typed interpolation, curve/surface caches, analytical and owning-evaluator
position/derivative checks, CAD construction and independent STEP readback.
All 62 native and 13 managed payloads match build, bundle, packages and consumers.
Final packages contain 272 stable Markdown documents, 13 JSON inventories and 11
notices; source identities, provenance, SBOM and checksums pass.
Evidence: `OcctSharp/artifacts/batch-am/delivery-result.json`.

Hosted CI, signing, push, public NuGet and full downstream application acceptance:
**NOT RUN**.

## Remaining authorized work

The [AL-AO round](NEXT_ROUND_AL_AO_PLAN.md) now has **two delivered batches (AL/AM)**
and **two remaining (AN/AO)**, both authorized. AN writer-source auditing is complete
and implementation begins next. AO read-only audits reproduce all 32 parser failures
and retain 17 configured missing-symbol exclusions plus two AK missing-symbol
 deferrals. The 33 configured excluded headers also include one parseable
ExprIntrp interface header; configured exclusion count differs from parser failures.
No AN/AO implementation or delivery is claimed yet. Finish each complete local
candidate delivery before expanding implementation.

## Concurrent Cadoryx work

The independent Cadoryx A5 records remain authoritative:
[contract](CADORYX_A5_PACKAGE_CONTRACT.md),
[source tessellation](CADORYX_A5_SOURCE_TESSELLATION.md), and
[incoming integration snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md).
This SDK delivery does not establish whole-application or physical display acceptance.

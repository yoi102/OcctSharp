# Current Status

- Last updated: 2026-10-08 (Asia/Tokyo).
- AP–AU authorized: AP/AQ/AR/AS delivered; AT full local checks pass; **5/6 after AT final freeze, one remains**.
- Candidate: **8.0.1-preview.44 / ABI 1.87 / bridge 0.95.0**.
- Final freeze: OcctSharp/artifacts/batch-at/delivery-result.json; AT delivered only when state is PASS.
- Previous delivered baseline: [Preview.43 / AS](STATUS_HISTORY_PREVIEW_43.md).
- Source remains uncommitted; no push, signing or public release occurred.

## Current round

[AT](BATCH_AT_EXCHANGE_SESSIONS.md) adds TM027 private STEP/IGES file sessions,
detached header/entity/check projections, bounded same-session all/root/pointed/
exemplar selections, ambiguity-preserving label resolution and restored metadata
export. AK/AN geometry transfer workflows are reused. STEP schema and IGES units/
scale stay read-only. Clear the private default modifiers; prepare copied header
data; restore original attributes after success/failure. GiveListCombined remains
deferred for a pinned SDK null sequence defect. Bound all effective IGES global
strings to 195 UTF-8 bytes before its fixed-buffer serialization.
182 primary: 63 admitted / 119 deferred; 4 new prerequisites, 30 reused;
generated delta67, Manual delta0. Round AP–AT: 258 generated and 9 Manual IDs.

Release, Debug and cold-source Release each pass **259 Generator / 1015 Runtime**;
isolated actual Debug-native **1015/1015**, 62 loaded DLL identities observed.
All 226 generated files reproduce; 223 baseline files unchanged. Acyclic resolved
dependency closure; exports 41,315 (+10/-0), Debug parity; managed API
56,925 (+163/-0); 4,263 facade forwarders. Both configurations:
60 C11 headers, 34 layouts, 250 release races / 16,000 acquisitions;
46 private headers compile. Fourteen packages and both consumers
pass; final source/package/provenance identities remain required for freeze.

Inventory: **116,272 total; 26,669 Emitted; 1,227 Manual; 39,029 Blocked;
49,347 Skipped; Pending=0**. Same 32 parser failures; semantic scanning incomplete.
[Overall/module progress](PROGRESS_PREVIEW_44.md) and
[counts](PROGRESS_PREVIEW_44_COUNTS.json) separate coverage/product completion.

## Next action

Freeze AT final identities, then complete [AU](NEXT_ROUND_AP_AU_PLAN.md): actual
selection/input gaps, real HWND visible frame evidence and round integration.
Do not use event counters as visual proof or infer all Blocked will be migrated.
AF–AK and AL–AO are complete. External CAD/AS extension interoperability,
Hosted CI, signing, push, public NuGet and full downstream application acceptance:
**NOT RUN**.

## Concurrent Cadoryx work

[Cadoryx A5 contract](CADORYX_A5_PACKAGE_CONTRACT.md),
[source tessellation](CADORYX_A5_SOURCE_TESSELLATION.md) and
[incoming snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md) remain authoritative.
SDK delivery does not establish whole-app or physical display acceptance.

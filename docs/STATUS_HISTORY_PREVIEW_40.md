# Current Status

- Last updated: 2026-10-07 (Asia/Tokyo).
- AP–AU are authorized: AP is locally delivered; **1/6, five batches remain**. AQ is implementing.
- Current local delivery: **8.0.1-preview.40 / ABI 1.83 / bridge 0.91.0**.
- Frozen PASS: OcctSharp/artifacts/batch-ap/delivery-result.json.
- Previous delivered baseline: [Preview.39 / AO](STATUS_HISTORY_PREVIEW_39.md).
- Source is uncommitted; no push, signing or public release occurred.

## Current round

[AP](BATCH_AP_MODELING.md) delivers private edge/wire builders, 33 initialized
shape counts/six copied groups and three bounded face-member wire diagnostics.
It composes owning curves/topology, repair and actual STEP/IGES readback from
Runtime and both clean consumers. Admission: 65 of 140 candidates plus one
prerequisite, 75 deferred; 59 Emitted/two Manual reused. Exact generated delta: 66.

Release, Debug and cold-source Release pass **243 Generator / 938 Runtime** each;
isolated actual Debug-native passes **938/938**, observing all 62 DLL paths/hashes.
All 207 generated files reproduce exactly; 204 baseline files are unchanged.
Dependency closure remains complete/acyclic. Exports 41,274 (+7/-0), Debug parity;
managed API 56,456 (+43/-0), no breaking changes; facade forwarders 4,219.
Both configurations pass 54 C11 headers, 34 layouts, 250 release races / 16,000
acquisitions; all 45 private headers compile. Fourteen packages and both clean
consumers pass. The frozen report verifies final payload, document/source input,
provenance, SBOM and checksum identities before recording delivery PASS.

Fresh inventory: **116,272 total; 26,477 Emitted; 1,218 Manual; 39,230 Blocked;
49,347 Skipped; Pending=0**. The same 32 header parser failures remain; full
semantic scanning is incomplete. [Module/overall progress](PROGRESS_PREVIEW_40.md)
and [machine counts](PROGRESS_PREVIEW_40_COUNTS.json) keep declaration coverage
separate from product completion. The old Preview.39 report remains historical.

AQ audited 50 primary candidates: 20 admitted / 30 deferred, seven new inherited
event prerequisites, ten reused prerequisites; expected generated delta 27.
Its [contract](adr/0109-generated-copied-curved-hatching.md) keeps inputs/results copied.
AQ candidate: Preview.41 / ABI 1.84 / bridge 0.92.0. Generation and native/managed
Release compilation pass; four focused Generator and 16 focused Runtime cases
pass. A native IsDone=true/error-status fixture now keeps overall completion
false and exposes no domains. Raw failed-domain buffers are covered in the
ongoing full suites. All 207 baseline generated files remain byte-identical;
three new files add exactly 27 IDs. Full local release-check is RUNNING;
AQ delivery is not yet complete.

## Next action

Proceed through [AQ–AU](NEXT_ROUND_AP_AU_PLAN.md): geometric hatching, mesh data
and diagnostics, document/XDE attributes, exchange headers and session selection,
then real HWND selection/integration. Freeze each batch's exact useful gaps and
admission from its delivered baseline; finish all five without routine confirmation.
The previous AF–AK and AL–AO rounds are complete. Remaining Blocked declarations
are a candidate pool, not a promise that these five batches finish the entire SDK.

Hosted CI, signing, push, public NuGet and full downstream application acceptance:
**NOT RUN**. Local delivery does not establish those separate gates.

## Concurrent Cadoryx work

The separate [Cadoryx A5 contract](CADORYX_A5_PACKAGE_CONTRACT.md),
[source tessellation](CADORYX_A5_SOURCE_TESSELLATION.md) and
[incoming snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md) remain authoritative.
This SDK delivery does not establish whole-application or physical display acceptance.

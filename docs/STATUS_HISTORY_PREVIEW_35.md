# Current Status

- Last updated: 2026-10-06 (Asia/Tokyo).
- **Batch AK local engineering delivery is complete; all six planned AF-AK batches are locally delivered.**
- **Main-checkout integration and final package verification also pass**, preserving concurrent Cadoryx implementation and documentation.
- Candidate: `8.0.1-preview.35` / ABI **1.79** / bridge **0.87.0**.
- Previous baseline: [Preview.34 / Batch AJ](STATUS_HISTORY_PREVIEW_34.md).
- Source remains uncommitted. No push, signing or publication occurred.
- The subsequent planning-only revision selects four new AL-AO batches; none is
  implemented. The validated Preview.35 artifacts remain the prior delivery snapshot.

## Scope

[AK contract](BATCH_AK_TRANSFER_RECORDS.md) and [ADR-0103](adr/0103-generated-private-transfer-records.md)
connect private STEP/IGES transfer, explicit model/map domains, immutable records,
process iterators, copied checks/messages/statistics, exact result identity and
functional shape binders. All 163 candidates are audited: 78 admitted, 85 deferred;
11 new generated prerequisites, 25 reused Emitted and one reused Manual operation.
Exact delta is 89 IDs. The manifest has 26,322 IDs / 195 files and passes
the acyclic managed dependency closure. Fresh inventory: 116,272 declarations;
26,322 Emitted, 1,218 Manual, 39,385 Blocked, 49,347 Skipped, zero Pending and
32 parser exclusions. Every other classification is unchanged. This is not full OCCT migration.

## Verification

### Integrated main checkout

Release, Debug and a fresh source-copy Release build each pass **225 Generator /
865 Runtime** tests, zero failures or skips. Isolated actual Debug-native passes
**865/865**; all **62 actually loaded DLL paths and SHA256 hashes** match the
integrated Debug build. All **195 generated files and the manifest** are
byte-identical after clean regeneration and to the independently delivered AK output.

The integrated API retains every delivered AK signature: **56,244 signatures**,
+26/-0 relative to the isolated AK candidate; **4,199 facade forwarders** are current.
Native exports total **41,242**, +6/-0 relative to isolated AK, with exact Debug/Release
parity. Both configurations pass 50 C11 headers, 34 geometric layouts and 250 release
races / 16,000 acquisitions; **45 private headers** pass independent compilation.

The unified release check, fresh inventory, **14 final packages** and both clean
public-only consumers pass. All 62 native DLLs and 13 managed payloads match the
build/bundle/packages/consumers. Final package auditing binds **265 stable Markdown
documents, 9 JSON inventories, 11 notices**, provenance, SBOM and checksums to source.
The exact inventory classifications and 32 parser exclusions match isolated AK.

Integration preserves 1,314 untouched incoming files byte-for-byte, the explicitly
merged AK changes and five concurrent Cadoryx document updates/additions. Two relative
links in one incoming historical document were repaired without changing its body.
The initial parallel native bootstrap hit an MSVC intermediate-file lock; the retained
failure log is superseded by the successful serial bootstrap and complete unified check.
Evidence: `OcctSharp/artifacts/batch-ak/integration-result.json` and its hash-bound reports.

### Independent AK delivery baseline

Full Release/Debug pass **225 Generator / 803 Runtime** each; isolated actual
Debug-native passes **803/803**, with all **62 actually loaded DLL paths/hashes**
verified. All 192 prior generated files are byte-identical; all prior IDs remain.
Native exports: **41,236**, +13/-0, Debug parity. Managed API: +139/-0.
Both configurations pass 50 C11 headers, 34 geometric layouts and registry lifetime
checks; 44 private headers pass standalone compilation. Fourteen packages and
both clean public-only consumers pass with exact native/managed payload identity.
Cold source build passes 225 Generator / 803 Runtime tests; all 195 generated
files and manifest are byte-identical. Fresh inventory and unified local release
checks pass. Managed API totals 56,218 signatures; 4,198 facade forwarders are current.
Final package documents, provenance, SBOM and checksums are hash-bound in
OcctSharp/artifacts/batch-ak/delivery-result.json. Its `deliveryRepository` explicitly
identifies the independent worktree; its counts are not the main-checkout counts.
Evidence: `OcctSharp/artifacts/batch-ak/`.

The batch-ai-delivery worktree retains the Preview.34 baseline and independent AK
delivery evidence. Main integration uses the baseline-relative merge and validates
the actual integrated result separately, including concurrent Cadoryx A5 changes.

Hosted CI, signing, push and public NuGet: **NOT RUN**.

## Remaining planned work

All six AF-AK batches are locally delivered, with **zero remaining in that round**.
The new [AL-AO planning round](NEXT_ROUND_AL_AO_PLAN.md) contains **four planned,
zero implemented and zero delivered batches**. AL is next: 54 exact currently
Blocked matrix/solver/banded declarations are frozen as audit candidates, not admissions.
AM covers spline workspaces, AN private writer diagnostics, and AO SDK/tooling blockers.
The 85 deferred AK audit candidates retain concrete reasons and do not count as migrated.
Full downstream application acceptance remains NOT RUN. This planning revision changes
documents only; it does not rerun tests or update the previously validated package contents.

## Concurrent Cadoryx work

The incoming main-checkout Cadoryx A5 status is preserved verbatim in
[its integration snapshot](STATUS_HISTORY_CADORYX_AT_AK_INTEGRATION.md).
AK delivery does not change the independent application acceptance decisions.
Later concurrent progress remains in [the A5 contract](CADORYX_A5_PACKAGE_CONTRACT.md)
and [source tessellation record](CADORYX_A5_SOURCE_TESSELLATION.md). Main integration
does not establish the separate A5 performance or whole-application acceptance gates.

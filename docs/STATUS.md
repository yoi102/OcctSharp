# Current Status

- Last updated: 2026-09-09.
- Active batch: [AB](BATCH_AB_OWNED_NONTRANSIENT_VALUES.md), in progress under
  [ADR-0096](adr/0096-generated-owned-nontransient-values.md). Its new ownership
  mechanism and migrated scope are not yet validated for delivery.
- Completed local batch: [AA](BATCH_AA_REFERENCE_OPERATIONS.md), implementing
  [ADR-0095](adr/0095-generated-reference-operations.md).
- Validated product: 8.0.1-preview.26 / ABI 1.70 / bridge 0.78.0.
  All local product-delivery gates and final package-content checks pass.
- Entry product: Batch Z c80f09c. Its completed evidence is preserved in
  [Preview.25 history](STATUS_HISTORY_THROUGH_PREVIEW_25.md).
- Twelve modules, one facade, one Native DLL and fourteen local packages remain.
  GitHub push and NuGet publication are NOT RUN, outside this task.

## Batch AB in progress

The completed AA baseline is frozen in the inner workspace at
`artifacts/preparation-baselines/preview26-batch-ab/`: manifest, full inventory,
Release native DLL and a freshly read 49,943-signature API snapshot, with per-file
hashes in `baseline-evidence.json`. The baseline source commit is c65b480; generated
declaration and file counts match AA at 22,636 and 126. AB is implementing audited
owning non-transient wrappers with stable native storage, typed non-reused tokens
and shared_ptr operation leases. The 122-owner catalogue now generates 1,067 added
IDs (23,703 total) in 137 files; its native build passes. The 34-header C11 and registry
fixtures pass, including 250 concurrent release races. All 35,030 old generated native
bodies/signatures remain unchanged. Preview.27 / ABI 1.71 / bridge 0.79.0 is the candidate;
Release native/managed builds, deterministic discovery/generation, Generator 163/163
and Runtime 542/542 pass (14 new runtime tests). Debug and the remaining product-delivery
gates are NOT RUN for AB yet. The following AA counts and PASS results describe the
completed Preview.26 baseline, not AB completion.

## Full-library accounting (validated Batch AA baseline)

| Disposition | Preview.25 entry | Batch AA |
|---|---:|---:|
| Emitted | 22,038 | 22,636 |
| Accepted manual | 1,217 | 1,217 |
| Blocked | 43,670 | 43,072 |
| Skipped | 49,347 | 49,347 |
| SupportedUnselected / pending / HD099 | 0 / 0 / 0 | 0 / 0 / 0 |
| Total inventoried | 116,272 | 116,272 |

Generated plus accepted manual declaration coverage rises from 20.00% to 20.51%.
This is declaration coverage, not functional completeness. Semantic discovery remains
7,058/7,090 headers with the same 32 explicit exclusions. Full migration is incomplete.

AA adds exactly the 598 audited catalogue IDs and removes none. There are no new
manual IDs or header exclusions. Every addition is Blocked-to-Emitted; there are no
other disposition or discovery identity changes. Inventory batch size remains 128.
Inventory SHA256: `AE0C2EA053355D9D94E2DD20F9FB0B2EF41A10E22610EF745C59EF7DA887966E`.

## Executed validation (Batch AA)

- PASS: Release/Debug native/managed builds and deterministic generation/discovery; 126 files.
- PASS: Generator 154/154 and Runtime 528/528 in both Release and Debug (11 AA tests).
- PASS: Release and Debug geometric ABI fixtures, 31 strict C11 headers and 34 record layouts.
- PASS: 34,432 previous generated native signatures/bodies unchanged; 818 managed API
  additions with zero removals; 49,943 public signatures and 3,750 facade forwarders.
- PASS: 35,775 native exports, +598, zero removals; Debug and Release match.
- PASS: actual Debug-native Runtime 528/528, with all 62 DLLs hash-verified.
- PASS: 14 local packages and clean facade/direct-module consumers, including AA workflow.
- PASS: exact inventory accounting and four rejection fixtures; unchanged denominator,
  header and manual-ID sets, with the exact 598-entry catalogue reconciled.
- PASS: cold source copy builds and reproduces all 126 generated files byte-for-byte.
- PASS: all local release-check gates; batchImplementationComplete=true. Execution resumed
  at the final inventory after interruption, retaining completed build/consumer/cold-run evidence.
- PASS: independent read-only reference-emitter review found no confirmed blocking defect.
- PASS: final documentation repack and content validation; 198 stable documents,
  73 runtime/license files, five checksums and matching final provenance.
- Implementation commit: 581b887. This document accompanies the local completion commit.
- Final facade package SHA256: 8BB1E3AE9BBDA4067BB0C8044212C21E6F7BE8B35F6C52933DFEAD7E8B6DE77D.

Release DLL: 19,127,296 bytes,
`CE2A270D75B91CF4FE0F82BFA746929DE912E9067AE5CED2E4BC8E5AD9D5D8E8`.
Debug DLL: `F22663AAAC95272E8A937940F4B871A8233CE1291E6E25113EA2986CD1A4255D`.

## Implemented scope

598 copied reference operations cover 81 OCCT packages across nine product modules.
220 operation classes expose curve/surface derivatives, parameter conversion, geometry
coefficients, display attributes, and exchange/document queries. Native inputs are
initialized snapshots; all copied outputs publish after success. Managed refs retain
their original values on exceptions. Typed SafeHandles retain shared inputs during calls.
See [Batch AA](BATCH_AA_REFERENCE_OPERATIONS.md), [type mapping](TYPE_MAPPING.md) and
[ownership](OWNERSHIP.md) for initialization, aliasing and thread restrictions.

## Next action

Continue Batch AB's audited owning non-transient implementation and lifetime validation,
then reconcile the exact inventory/API/export delta and run product delivery gates.
The validated version remains Preview.26 until new product evidence is complete.
Remote CI, signing, GitHub push and NuGet publication are NOT RUN.

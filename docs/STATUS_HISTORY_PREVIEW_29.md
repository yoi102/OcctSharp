# Current Status

- Last updated: 2026-10-02 (Asia/Tokyo).
- **P0 local engineering delivery validation is complete for Preview.29.**
- Identity: `8.0.1-preview.29` / native ABI **1.73** / bridge **0.81.0**.
- Scope: consolidate and validate the existing AD/AE and Cadoryx development work.
  No new migration scope was added; the generated manifest is unchanged from task entry.
- Earlier checkpoints remain in
  [the status archive through 2026-10-01](STATUS_HISTORY_THROUGH_2026_10_01.md).

## Product and delivery baselines

| Baseline | Current interpretation |
|---|---|
| Preview.29 | Current completed local engineering candidate: fourteen packages, matching built/bundled/runtime payloads, complete Release/Debug checks and clean source regeneration. See [release notes](RELEASE_NOTES_8.0.1_PREVIEW_29.md). |
| Batch AC / Preview.28 | Previous completed local engineering baseline at `f43f388`, ABI 1.72 / bridge 0.80.0. Its packages and historical evidence retain their own identities. |
| Suffixed Preview.28 prerequisites | Existing immutable mirror.2, sweep.1 and other development packages were preserved; they are not relabeled as Preview.29. |
| Platform and layout | OCCT 8.0.1, Windows x64, .NET 10; twelve managed modules, one facade and one native bridge. Fourteen packages share one 62-DLL native runtime. |
| Public release | Hosted CI, signing, GitHub push and public NuGet publication: NOT RUN. Local engineering completion does not establish public release readiness. |

Source changes remain uncommitted. HEAD is `2f7ba46`; no commit or push was made
during this delivery-validation task. The local source identity and final package
evidence are retained under `OcctSharp/artifacts/delivery-preview29/`.

## Migration accounting

The fresh **2026-10-02** full-inventory classification reports:

| Disposition | Declarations |
|---|---:|
| Emitted | 25,875 |
| Accepted Manual | 1,218 |
| Blocked | 39,832 |
| Skipped | 49,347 |
| Pending classification | 0 |
| Total classified | 116,272 |

The manifest has **25,875 exact stable IDs in 155 files**. Relative to Preview.28,
AD/AE added exactly 248 IDs (203 + 45); no prior generated IDs were removed.
This delivery added no further IDs. The scan parsed 7,058 of 7,090 headers;
the 32 explicit parser exclusions are unchanged from the AE checkpoint.

Zero pending means every discovered item has a disposition. It is not full OCCT
migration. Skipped includes operators, non-public declarations and type metadata.
The separate discovery `coverage.json` reports 116,263 declarations and 61,570
raw Pending entries; it is an eligibility-stage report, not the final-classification
backlog. Do not combine the two accounting stages.

The main blocked contracts remain receiver ownership, construction, unmapped values,
borrowed/output references, raw-pointer lifetime and unselected handles. Borrowed
adaptors, retained external buffers and unaudited sequence/array projections remain
outside the current safe profiles.

## Delivered development work

- [AD owned data workflows](BATCH_AD_OWNED_DATA_WORKFLOWS.md) and
  [AE exchange scalar values](BATCH_AE_EXCHANGE_SCALAR_VALUES.md), including generated
  index preconditions, owning wrappers, facade forwarding and package workflows.
- [Copied Boolean topology history](BOOLEAN_TOPOLOGY_HISTORY.md) and SC-061's bounded,
  copied B-spline knot buffer.
- Exact source-topology and unique-partner indexing, retaining caller-owned
  asset/revision/fingerprint/instance checks and ambiguity rejection.
- Viewer gradient, ViewCube directions/roll/detached camera/appearance and manipulator
  presentation/selection corrections.
- Plane reflection, styled STEP/XDE duplicate-solid correction and copied
  self-interference diagnostics using a deep private geometry copy.

## Executed delivery checks

| Check | Result and evidence |
|---|---|
| Release and Debug native/managed builds | PASS in the unified `delivery-preview29/release-check.log`. SDK 10.0.401, using the pinned 10.0.400 feature band's latest-patch policy. |
| Full Generator and Runtime suites | PASS: Generator **189/189**, Runtime **609/609** in each configuration. |
| Actual isolated Debug-native runtime | PASS: **609/609**, all 62 DLL hashes checked; `delivery-preview29/actual-debug/result.json` and its retained TRX. |
| Fresh source rebuild/regeneration | PASS: Release Generator 189/189 and Runtime 609/609 against its own new native bridge; all **155 generated files byte-identical**. |
| ABI and lifetime/concurrency fixtures | PASS in both configurations: strict C11 for 40 headers, 34 geometric layout assertions, registry errors/disposal, 250 release races and 16,000 acquisitions per configuration. Supplemental logs are in `delivery-preview29/`. |
| Native exports | PASS: **40,872**, **303 additions**, zero removals versus Preview.28; Debug names equal Release. `delivery-preview29/native-export-compatibility.json`. |
| Generated-source preservation | PASS: all **39,816** previous generated function bodies and header declarations preserved, including releases; no removed stable IDs. `delivery-preview29/generated-preservation.json`. |
| Managed API | PASS: **55,475 signatures**, **399 additions**, zero removals versus the Preview.28 API snapshot. `delivery-preview29/api-diff-preview28.json`. The release entry's alpha.38 comparison also passes. |
| Dependency/source structure | PASS: complete acyclic 28-edge generated closure, source-layout positive/negative checks, and **44/44** standalone private headers. |
| Samples and affected real-file/Viewer workflows | PASS: console smoke plus the complete Runtime suites' exchange, mirror/style, topology, self-interference and actual-driver scenarios. |
| Clean package consumers | PASS: fourteen packages; facade and direct-module consumers; all **62 native DLLs** and **13 managed payloads** matched to the build. `delivery-preview29/package-payloads.json`. |
| Isolated downstream capability probe | PASS: **44/44** against Preview.29 with locked restore, copied Cadoryx probe source and restricted runtime environment. All 62 DLLs match the package/build. `delivery-preview29/cadoryx-probe/evidence/result.json`; this is not a full Cadoryx application delivery. |
| Full inventory and unified local release check | PASS, with the unchanged 32 exclusions. `artifacts/release/release-gates.json` identifies Preview.29 and local completion; public readiness remains false. |
| Package content/provenance | PASS: 208 stable documents, 73 runtime/license files, five checksums and package provenance. Final documentation repack is verified before the task closes. |
| Documentation/whitespace | PASS: local document links and fences; `git diff --check`. Reports are retained in `delivery-preview29/`. |

Artifact paths in the table are relative to `OcctSharp/artifacts/`, except the
explicit `artifacts/release/` path, which is relative to the inner workspace.

Release bridge SHA256:
`92A56FC80D5DB2AE77759BCB2C488FACE538AD0F39C2096A151EE4A5090A2994`.
Debug bridge SHA256:
`CDED0AF305EC0466BA6BCDEED46BD89953D1CA7653ACA68F333A371C804193FB`.

Mixed-DPI/RDP visual confirmation and the physical display buffer before repaint
remain **NOT RUN**. Hosted CI, signing, push, publication and a full Cadoryx application
run against Preview.29 are **NOT RUN**. These remain separate platform/application/
public-release evidence; no overall OCCT migration or public readiness is claimed.

## Next action

P0's local SDK validation is closed. The recommended next migration is the audited
copied-array/sequence generator workstream, with exact declarations and a useful
workflow selected before implementation. [ROADMAP](ROADMAP.md#active-priorities)
owns that order and its contract boundaries.

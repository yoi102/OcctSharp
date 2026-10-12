# Current Status

- Last updated: 2026-10-02 (Asia/Tokyo).
- **Batch AH local delivery is complete; Batch AI is planned and not admitted.**
- Current validated local candidate: `8.0.1-preview.32` / ABI **1.76** / bridge **0.84.0**.
- Previous local baseline: [Preview.31 / Batch AG](STATUS_HISTORY_PREVIEW_31.md).
- Source remains uncommitted; no push, signing or public publication has occurred.

## Scope and accounting

[Batch AH](BATCH_AH_BULK_ANALYSIS.md) and
[ADR-0099](adr/0099-generated-bulk-analysis-and-typed-curve-collections.md) admit
**42 exact Blocked declarations** from the 184-candidate audit: 23 BSplCLib, nine
BSplSLib, five GeomConvert and five Geom2dConvert. These comprise 32 sized
numeric/grid operations and ten typed spline collection conversions. All **142**
deferred rows have concrete source and contract reasons in the
[admission matrix](BATCH_AH_BULK_ANALYSIS_ADMISSION.json). There are no newly counted
Manual supporting declarations.

The manifest contains **26,040** stable IDs in **175** files. All previous **25,998**
IDs and **169** complete generated files are preserved byte-for-byte. Fresh full
inventory classifies **116,272** declarations: **26,040 Emitted**, **1,218 Manual**,
**39,667 Blocked**, **49,347 Skipped**, zero Pending. Exactly the admitted 42 IDs
changed from Blocked to Emitted; every other declaration disposition, all header
classifications and **32** parser exclusions remain unchanged. Classification
completion is not full OCCT migration.

Bulk numeric/grid calls deep-copy one-based inputs, check exact dimensions and
overflow before the kernel, and publish mutable outputs only after success. Typed
2D/3D curve sets and concat results retain independent intrusive copies through
leased registry owners. Returned curves survive source/set/result disposal. The
shared workflow measures sampled position/derivative error, performs representation
edits and approximation, constructs edges/faces and roundtrips STEP through Runtime
and both public-only consumers. General templates, raw workspaces and borrowed
buffers remain excluded.

## Verification

- Final-source native/managed Release and Debug builds/suites: PASS **210/210
  Generator**, **702/702 Runtime** per configuration, .NET SDK 10.0.401. AH adds
  five Generator and **31 Runtime** cases. The **16** concurrently developed
  adaptive-inspection/sketch-intersection cases are counted separately from AH.
- Isolated actual Debug-native: PASS **702/702**; all **62** DLL hashes verified.
- Cold source build/suites: PASS **210/210**, **702/702**; all **175** generated
  files and manifest are byte-identical. Native compiler identities are retained.
- ABI/lifetime: PASS **46** strict C11 headers, **34** geometric layouts; each
  configuration passed **250** release races / **16,000** acquisitions. Private
  headers **44/44** pass standalone strict compilation.
- Compatibility versus Preview.31: PASS **41,128** native exports (+53 AH,
  +2 companion, -0), with Debug/Release parity; **55,756** managed signatures
  (+67 AH, +17 companion, -0). No removals against the alpha.38 baseline.
- **4,158** facade forwarders, generated freshness and acyclic module closure: PASS.
- **14** packages, both clean public-only consumers and payload identity: PASS;
  **62** shared native DLLs, **13** managed assemblies, zero native duplication.
  Both consumers execute the bulk-analysis/curve-conversion-to-CAD/STEP workflow;
  the direct module consumer receives no OcctSharp.dll.
- Bundled runtime, console smoke, exact inventory delta, stable packaged
  documents/licenses, provenance/SBOM/checksums and unified local release check:
  PASS. Local `batchImplementationComplete=true`; `publicReleaseReady=false`.
- Hosted CI, signing, push, public NuGet and full downstream application acceptance:
  **NOT RUN**. Existing mixed-DPI/RDP and physical pre-repaint viewer observations
  remain **NOT RUN**.

Delivery is bound to the attached `batch-ah-delivery` worktree and its recorded
source hashes. `OcctSharp/artifacts/batch-ah/` retains the aggregate result, final
source/input identities, exact inventory delta, actual Debug closure, compatibility,
package/document checks and all failed attempts. `OcctSharp/artifacts/release/`
contains unified gates and metadata. Current shared-checkout work must match those
hashes before inheriting these results.

Full Debug exposed the pinned continuity divider's invalid partial partition and
premature ordinary-Gauss convergence on conic volume boundaries. The reviewed
SC-067 fixes were adopted, analytical tolerances were retained, and the final full
gates rerun. A copied source timestamp also exposed a stale Release test assembly;
it was rebuilt from source before the accepted gate run. Earlier logs remain as
failure/recovery evidence. Stable final documents and all four candidate/admission
JSON files are included in the refreshed package; volatile STATUS stays source-only.

## Companion scope

The frozen snapshot includes the separately developed adaptive shape inspection
and copied sketch intersection classification APIs: **two native exports**, **17
managed signatures** and **16 Runtime cases** beyond Preview.31. These additions
and the SC-067 numerical corrections are included in this local engineering
snapshot, and are not counted as AH migration. Their Cadoryx suffixed-package,
visual and whole-application acceptance remains separate. The immutable
`8.0.1-preview.31.cadoryx.curve-inspection.2` package passed its own 48/48 clean
consumer cases and 62 DLL hashes, followed by Cadoryx B9's 1192/1192 tests and
13 production window gates with empty binding logs and 27 inspected screenshots
(`Cadoryx/artifacts/b9-final-verification.json`). Physical pointer and external
CAD are still unverified; these results do not replace Preview.32/AH or its
downstream delivery gates.

Earlier XdeDocument.CopyTransformedDefinition work is already part of the AG
baseline; its one export/method and six Runtime cases are not new AH additions.
Historical [AG delivery](STATUS_HISTORY_PREVIEW_31.md) remains unchanged.

## Next work

Execute [Batch AI](BATCH_AI_TOPOLOGY_EXCHANGE_PLAN.md) from this verified Preview.32
baseline: **88 exact audit candidates across 31 families**, bound to the fresh AH
exit inventory. First prove typed shape/entity collection ownership; then close
Boolean/history, inspection/repair and STEP/IGES transfer/iteration together in one
useful batch. All candidates are PLANNED_CANDIDATES_NOT_ADMITTED. Counts are audit
scope, not promised migrations. Preserve explicit split/merge/ambiguous history
results and parent/epoch restrictions. Complete each batch's local delivery gates
before expanding implementation.

# Current Status

- Last updated: 2026-10-02 (Asia/Tokyo).
- **Batch AG implementation and local engineering delivery validation are complete.**
- Current validated local candidate: `8.0.1-preview.31` / ABI **1.75** / bridge **0.83.0**.
- Previous local baseline: [Preview.30 / Batch AF](STATUS_HISTORY_PREVIEW_30.md).
- Source remains uncommitted; no push, signing or public publication has occurred.

## Scope and accounting

[Batch AG](BATCH_AG_SURFACE_INTERPOLATION.md) and
[ADR-0098](adr/0098-generated-surface-grids-and-retained-interpolation.md) add copied
surface grids, surface fitting and retained constrained 2D/3D interpolation across
five families. Exactly **63** Blocked declarations are newly generated; **18**
supporting Manual entries retain their classification and are counted separately.
Two nullable Weights pointers remain excluded; use independent WeightsArray snapshots.

The manifest contains **25,998** stable IDs in **169** files. All previous **25,935**
IDs and **162** complete generated files are preserved byte-for-byte. Fresh full
inventory classifies **116,272** declarations: **25,998 Emitted**, **1,218 Manual**,
**39,709 Blocked**, **49,347 Skipped**, zero Pending. Exactly the admitted 63 IDs
changed from Blocked to Emitted; all other declaration dispositions, supporting
Manual entries, header classifications and **32** parser exclusions remain unchanged.
Classification completion is not full OCCT migration.

CopiedGrid owns row-major snapshots with checked dimensions and offsets. Retained
HArray data uses independent intrusive copies; results outlive algorithms. SC-066
handles the pinned constant-height surface fitter's NaN result with an exact plane.
Arbitrary templates, borrowed/static addresses and external buffers remain closed.
Seventeen existing freeform/exchange public types now belong to Modeling/DataExchange;
generated facade forwarders preserve their public API and binary compatibility.

## Verification

- Final-source native/managed Release and Debug builds/suites: PASS **205/205 Generator**,
  **655/655 Runtime** per configuration, .NET SDK 10.0.401. AG contributes eight
  Generator and 24 Runtime cases; the separately developed XDE companion contributes
  six Runtime cases and is not counted in AG's 63-declaration migration.
- Isolated actual Debug-native: PASS **655/655**; all **62** DLL hashes verified.
- Cold source build/suites: PASS **205/205**, **655/655**; all **169** generated files
  and manifest are byte-identical. Native toolchain identities are retained in evidence.
- ABI/lifetime: PASS **44** strict C11 headers, **34** geometric layouts; each
  configuration passed **250** release races / **16,000** acquisitions. Private
  headers **44/44** and standalone generated grid support passed strict compilation.
- Compatibility: PASS **41,073** native exports (+104 AG, +1 companion, -0), with
  Debug/Release parity; **55,672** managed signatures (+100 AG, +1 companion, -0)
  versus Preview.30. No removals against the alpha.38 baseline.
- **4,150** facade forwarders and acyclic module closure: PASS. An unchanged binary
  compiled against Preview.30 resolves all 17 relocated types and completes owning
  freeform and STEP workflows on Preview.31.
- **14** packages, both clean public-only consumers and payload identity: PASS;
  **62** shared native DLLs, **13** managed assemblies, zero native duplication.
  The direct module consumer executes the same grid-to-face/STEP and curve-to-edge
  workflows without receiving OcctSharp.dll.
- Bundled runtime, console smoke, generated freshness, full classification,
  exact inventory delta, stable packaged documents/licenses, provenance/SBOM/checksums
  and unified local release check: PASS. Local `batchImplementationComplete=true`;
  `publicReleaseReady=false`.
- Hosted CI, signing, push, public NuGet and full downstream application acceptance:
  **NOT RUN**. Existing mixed-DPI/RDP and physical pre-repaint viewer observations
  remain **NOT RUN**.

Evidence is retained in `OcctSharp/artifacts/batch-ag/`: the aggregate result,
final source hashes, exact inventory delta, actual Debug closure, unchanged old-binary
compatibility, package payload/content checks and original failed attempts.
`OcctSharp/artifacts/release/` owns the unified gate report and release metadata.
Earlier concurrent native build conflicts and clean-consumer defects were corrected
and followed by a complete final gate run. Stable delivery documents and AG/AH
candidate JSON are included in the final package refresh; volatile STATUS stays source-only.

## Companion scope

### Cadoryx B9 adaptive inspection (2026-10-02, in progress)

A native independent probe reproduced inaccurate area/volume integration across
nonuniform knot spans. Deep-copy CN partition plus adaptive Gauss matches the
independent area 40.208 and prism volume 120.624 (about 1 ms for this sample).
The existing inspection path and additive copied InspectAdaptiveProperties API
are implemented in manual source, with SC-067 and ownership records. Actual
Release native build/runtime, centroid/inertia/lifetime tests, Debug and immutable
local-package consumption are still running or NOT RUN. This does not revise
AG's independent delivery evidence or complete Cadoryx A–F.

The working tree also contains the separately developed
`XdeDocument.CopyTransformedDefinition`. Its one native export, one managed method
and six Runtime cases are included in this tested source/package snapshot and
accounted separately from AG. Downstream Cadoryx suffixed packages and visual/app
acceptance are separate evidence and do not establish whole-app acceptance of this
base Preview.31 package.

## Next work

Execute the [AH bulk analysis plan](BATCH_AH_BULK_ANALYSIS_PLAN.md) from this validated
AG baseline: **184 exact audit candidates**, comprising 138 BSplCLib, 36 BSplSLib,
five GeomConvert and five Geom2dConvert declarations. The candidate JSON is hash-bound
to the AG exit inventory. Audit counts and aliasing first; then close analysis,
evaluation, bulk edits, conversion, sampled-error data and geometry-to-exchange
as one broad useful workflow in both consumers. Candidates are not admitted or
implemented declarations. AI follows with typed topology and exchange collections.
Complete each batch's local gates before expanding implementation.

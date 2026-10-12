# Current Status

- Last updated: 2026-10-02 (Asia/Tokyo).
- **Batch AF copied-array/sequence rules and local engineering delivery validation are complete.**
- Current validated candidate: `8.0.1-preview.30` / native ABI **1.74** / bridge **0.82.0**.
- Previous completed baseline: [Preview.29](STATUS_HISTORY_PREVIEW_29.md).
- Source work remains uncommitted; no push or publication has been performed.

## Implemented scope and accounting

[Batch AF](BATCH_AF_COPIED_COLLECTIONS.md) and
[ADR-0097](adr/0097-generated-copied-arrays-and-sequences.md) define one copied
collection workflow across eight families: 2D/3D Bezier and B-spline definition,
bulk reads/edits, point fitting, knot reversal/reparameterization and interval merging.
Exactly **60** Blocked declarations are newly generated. Fourteen compatible Manual
constructor/status/result entries are separately supported and retain their disposition.
Five closed container/element profiles reuse one typed native registry and immutable
managed snapshots. Arbitrary templates, Array2, HArray, borrowed addresses and
retained external buffers remain excluded.

The manifest has **25,935** stable IDs in **162** files. All previous 25,875 IDs and
155 generated files are preserved byte-for-byte. Fresh full inventory contains
**116,272** declarations: **25,935 Emitted**, **1,218 Manual**, **39,772 Blocked** and
**49,347 Skipped**, with zero Pending. Exactly the selected 60 IDs changed from
Blocked to Emitted; all other dispositions, the 14 supporting Manual entries and
32 parser exclusions remain unchanged. Inventory completion is not full OCCT migration.

## Verification

- Focused Generator copy-contract tests: PASS **8/8**; Runtime copy tests: PASS **16/16**.
- Full native/managed Release and Debug builds and suites: PASS **197/197 Generator**,
  **625/625 Runtime** per configuration, .NET SDK 10.0.401.
- Isolated actual Debug-native: PASS **625/625**, all 62 DLL hashes verified.
- Cold source rebuild: PASS **197/197**, **625/625**; all **162** generated files and
  manifest are byte-identical.
- ABI/lifetime: PASS **42** strict C11 headers, **34** geometric layout assertions;
  each configuration passed **250** release races / **16,000** acquisitions. Private
  headers **44/44** and standalone generated copied support passed strict compilation.
- Compatibility: PASS **40,968** native exports (+96/-0) with Debug/Release parity;
  **55,571** managed API signatures (+96/-0 versus Preview.29). No API removals against
  the alpha.38 baseline; all 40,103 prior generated function bodies/prototypes remain.
- Acyclic generated module closure and **4,127** facade forwarders: PASS.
- **14** packages, facade/direct-module clean consumers and payload identity: PASS;
  **62** shared native DLLs, **13** managed assemblies, no native duplication.
- Bundled runtime, console clone-and-run smoke, fresh inventory, package content,
  source-matched documents/licenses, provenance/SBOM/checksums and unified local
  release gate: PASS. Local `batchImplementationComplete=true`; `publicReleaseReady=false`.
- Hosted CI, signing, push, public NuGet and downstream full-application acceptance: NOT RUN.
- Existing mixed-DPI/RDP and physical pre-repaint viewer observations remain NOT RUN.

Evidence is retained under `OcctSharp/artifacts/batch-af/`, including `result.json`,
the final source hashes, exact inventory delta, actual Debug closure and final
package validation. The release gate report is in `OcctSharp/artifacts/release/`.
Final package refresh includes the stable batch/ADR/release documents and the AG
candidate JSON. Volatile STATUS stays source-only under the existing packaging policy.

## Next work

AG is planned as one surface-grid and interpolation closure: 83 exact candidates
across five families, including 65 Blocked and 18 Manual supporting entries. Dimension
products, storage order, retained HArray ownership, tangent/bool elements and output
capacity must be audited before admission. See the
[exact candidate list](BATCH_AG_SURFACE_INTERPOLATION_CANDIDATES.json) and
[batch plan](BATCH_AF_COPIED_COLLECTIONS.md).

After AF delivery, proceed with AG's contract audit; then AH bulk analysis/approximation
and AI topology/exchange collections. Each batch closes its own local delivery gates
before scope expands. Counts measure actual admitted declarations and complete
workflows, not arbitrary fixed targets.

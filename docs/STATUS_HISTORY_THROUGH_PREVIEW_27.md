# Status history through Preview.27

- Last updated: 2026-09-12.
- Completed local batch: AB, with implementation and delivery checks passing under
  [ADR-0096](adr/0096-generated-owned-nontransient-values.md).
- Product: 8.0.1-preview.27 / ABI 1.71 / bridge 0.79.0.
- Implementation commit: f58a580. Entry product: c65b480, Preview.26;
  [its completed evidence is archived](STATUS_HISTORY_THROUGH_PREVIEW_26.md).
- Twelve modules, one facade, one Native DLL and fourteen local packages remain.
- PASS: final documentation and package content verified: 201 packaged documents,
  73 runtime/license files and five release checksums.
- GitHub push, remote CI, signing and NuGet publication: NOT RUN, outside this task.

## Full-library accounting

| Disposition | Preview.26 entry | Batch AB |
|---|---:|---:|
| Emitted | 22,636 | 23,703 |
| Accepted manual | 1,217 | 1,217 |
| Blocked | 43,072 | 42,005 |
| Skipped | 49,347 | 49,347 |
| SupportedUnselected / pending / HD099 | 0 / 0 / 0 | 0 / 0 / 0 |
| Total inventoried | 116,272 | 116,272 |

Generated plus accepted manual declaration coverage rises from 20.51% to 21.43%.
This is declaration coverage, not functional completeness. Semantic discovery remains
7,058/7,090 headers with the same 32 explicit exclusions. Full migration is incomplete.

AB adds exactly 1,067 audited catalogue IDs: 446 constructors and 621 instance
operations, all Blocked-to-Emitted. No other declaration disposition, discovery identity,
manual ID or header exclusion changes. Inventory uses BatchSize 128.
Inventory SHA256: `14B0D49AB1929D51D2E9BA17240641C6537C11DDEAF89F7F6F0AFAA0A9A24BE2`.

## Executed validation

- PASS: Release/Debug native and managed builds; deterministic generation/discovery,
  137 generated files, complete acyclic module dependency closure.
- PASS: Generator 163/163; Runtime 542/542 in Release and Debug, including 14 AB tests.
- PASS: actual Debug-native Runtime 542/542 in an isolated directory; all 62 DLL hashes checked.
- PASS: Release/Debug strict C11 checks for 34 generated headers and geometric ABI fixtures.
- PASS: registry null/wrong-type/stale/repeated release, token non-reuse, stable addresses,
  250 concurrent release rounds and 16,000 lease attempts per fixture configuration;
  final destruction and managed Dispose/finalizer tests.
- PASS: 35,030 previous generated native signatures/bodies unchanged; 1,500 managed API
  additions and zero removals, for 51,443 signatures and 3,872 facade forwarders.
- PASS: 37,153 native exports, 1,378 additions and zero removals; Debug matches Release.
- PASS: 14 packages and clean facade/direct-module consumers with AB workflows; sample smoke.
- PASS: cold source copy builds and tests successfully; all 137 generated files are byte-identical.
- PASS: exact final inventory accounting and seven rejection fixtures. The fixtures use
  an isolated synthetic inventory; the final accounting checks the actual full scan.
- PASS: all local release-check gates; batchImplementationComplete=true. Interrupted
  execution resumed with source-identified Release/Debug evidence retained.

Final facade package SHA256:
`1A622EF34D46885191383824580C7AF23567EFD855DC42C80E4CCF483A3B2372`.

The latest rebuilt Release bridge also passes all 542 Runtime tests; its 62-DLL closure
matches the bundled runtime. Release bridge size: 20,033,536 bytes.
Release SHA256: `FE5AE1330F6951EF2560E90AA97967BD0CC8A5B818DC00519111F58327E0498A`.
Debug SHA256: `451780944BA0C259C1786174B988383BB93D5FA9C4928CFEF8C8F4682020EEC0`.
Release started with SDK 10.0.400; resumed and cold checks use 10.0.401 under latestPatch,
with identical generated/discovery hashes.

## Implemented scope

122 owning classes cover 15 OCCT packages across Geometry, Modeling and Foundation:
Bnd, analytic Extrema, IntAna/IntAna2d, GccAna/GccEnt, GC/gce makers, GProp,
GeomAPI/Geom2dAPI, LProp/GeomLProp, quaternion interpolation and selected math utilities.
OwnedValues exposes deterministic disposal, fixed native object addresses and type-checked
non-reused tokens with operation leases. Only 24 audited data types allow independent
copied owning results. Mutable copied refs publish after success; native object mutation
is not rolled back on failure. Callers synchronize concurrent mutation.

187 supporting method projections, two default factories and 122 release exports are
accounted separately from migrated declaration IDs. Existing Manual APIs remain intact.
See [Batch AB](BATCH_AB_OWNED_NONTRANSIENT_VALUES.md), [OWNERSHIP](OWNERSHIP.md) and
[TYPE_MAPPING](TYPE_MAPPING.md) for contracts and usage.

## Remaining migration

Broader non-transient owners, explicit buffers/sequences, borrowed member lifetimes,
mutable owning outputs and unsupported native types remain future work. There is no
fixed per-batch capability count; follow [WORKFLOW](WORKFLOW.md).

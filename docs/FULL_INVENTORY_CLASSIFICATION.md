# Full Inventory Classification

The long-tail workstream inside B provides complete classification for the pinned OCCT
8.0.1 public entry-header catalog. Generated coverage and classification remain separate:
a blocked declaration is not a package API.

## Current metrics

| Metric | Result |
|---|---:|
| Catalogued entry headers | 7,090 |
| Semantically parsed headers | 7,058 (99.5487%) |
| Isolated header failures | 32 |
| Unique declarations from parsed headers | 116,272 |
| Finally classified declarations | 116,272 (100%) |
| Finally classified headers | 7,090 (100%) |
| Final declaration/header pending | 0 / 0 |
| Unowned header fallback `HD099` | 0 |
| Generated manifest stable IDs | 16,353 |
| Accepted manual stable IDs | 863 |
| `SupportedUnselected` | 0 |
| Broad `LT001`-`LT004` reasons | 0 |

The successful declarations have these final dispositions:

| State | Count | Meaning |
|---|---:|---|
| `Emitted` | 16,353 | The generated manifest owns the declaration stable ID (`EM001`) |
| `Manual` | 863 | Schema 1.13 links the stable ID to accepted manual behavior through SC-032/SC-033 and subsequent product-wave special cases through SC-055 (`MN001`) |
| `SupportedUnselected` | 0 | No declaration accepted by the active safe generator rules remains outside the manifest |
| `Skipped` | 49,344 | Non-public/language-level exclusions or narrow accepted non-callable declarations |
| `Blocked` | 49,712 | Public declarations with a specific unresolved ABI, export, type, or ownership boundary |
| `Pending` | 0 | No unowned declaration disposition remains |

The former broad LT001-LT004 buckets are eliminated. Narrow blocker counts are:

| Code | Category | Count |
|---|---|---:|
| `BL002` | Missing toolkit provenance | 445 |
| `BL003` | Unverified free-function export | 117 |
| `BL102` | Non-transient receiver ownership | 18,857 |
| `BL103` | Non-transient value construction | 6,562 |
| `BL202` | Raw pointer lifetime | 3,548 |
| `BL203` | Rvalue-reference transfer | 1,277 |
| `BL204` | Borrowed or output reference | 8,722 |
| `BL205` | Unselected intrusive-handle target | 2,709 |
| `BL206` | Template-instantiation projection | 600 |
| `BL208` | Unmapped value type | 6,875 |

New accepted skip reasons are `SK012 TypeMetadata` 11,603, `SK013
InternalHeaderFunction` 4, `SK014 DestructorLifecycleBoundary` 4,182, `SK015
PureVirtualDispatch` 1,050, `SK016 AbstractTypeConstruction` 458, and `SK017
AnonymousOrUnnameableEnum` 41. Existing deterministic SK002-SK011 reasons remain in the
machine-readable report.

## Header dispositions

| Code | State/category | Count |
|---|---|---:|
| `HD000` | Parsed | 7,058 |
| `HD001` | Missing VTK | 19 |
| `HD002` | Missing EGL/GLES context | 1 |
| `HD003` | Missing RapidJSON | 1 |
| `HD004` | C++/CLI-only | 1 |
| `HD005` | Missing generated OCCT header in the artifact | 10 |

The manifest-aware BatchSize=128 report is
`OcctSharp/artifacts/generator-reports/full-inventory.json`, SHA256
`4E90AB503456D7617CE81E21116CBAA0119042B2E63EEAD9A5C06CD20DE807E6`.
R reconciles exactly 48 Blocked-to-Manual SC-055 transitions against Q, with zero
other declaration, identity or classification changes. Full classification is not
a claim that all 116,272 declarations are callable managed APIs.

## Interpretation

Classification completeness answers whether every observed surface has an accountable
state. Binding coverage answers how much safe API is actually emitted and validated.
The 32 failed headers contribute header dispositions rather than invented declarations.
Batch implementation completion additionally requires local build, runtime, freshness,
package-consumer, compatibility, and release-engineering gates; public release readiness
still has independent legal, hosted-CI, signing, and publication gates.

# Full Inventory Classification

B19 provides complete classification for the pinned OCCT 8.0.1 public entry-header
catalog. It does not claim complete generated C# bindings.

## Current metrics

| Metric | Result |
|---|---:|
| Catalogued entry headers | 7,090 |
| Semantically parsed headers | 7,058 (99.5487%) |
| Isolated header failures | 32 |
| Unique declarations from parsed headers | 116,214 |
| Finally classified declarations | 116,214 (100%) |
| Finally classified headers | 7,090 (100%) |
| Final declaration/header pending | 0 / 0 |
| Unowned header fallback `HD099` | 0 |
| Selected generated bindings | 333/9,567 (3.4807% of selected scope) |
| Selected emitted plus accepted manual bindings | 351/9,567 (3.6689% of selected scope) |

The successful declarations have these final dispositions:

| State | Count | Meaning |
|---|---:|---|
| `Emitted` | 333 | The generated manifest owns the declaration stable ID (`EM001`) |
| `Manual` | 18 | Schema 1.6 links the stable ID to accepted SC-032 behavior (`MN001`) |
| `SupportedUnselected` | 10,177 | Initial value-copy rules consider the declaration eligible, but no full-profile emitter selection/validation exists |
| `Skipped` | 27,310 | Deleted, non-public, variadic, template declaration, or operator exclusion with existing `SK` code |
| `Blocked` | 78,376 | Public candidate needs a declaration, receiver ownership, return, or parameter projection rule |
| `Pending` | 0 | No unowned declaration disposition remains |

Blocked reason counts are `LT001 DeclarationProjection` 13,140,
`LT002 InstanceOwnership` 43,459, `LT003 ReturnProjection` 20,349, and
`LT004 ParameterProjection` 1,428. `LT000 EligibleUnselected` accounts for 10,177,
while `EM001 GeneratedBinding` accounts for 333 manifest-reconciled declarations and
`MN001 ManualBinding` accounts for 18 stable-ID-reconciled declarations.
Skipped counts remain `SK002` 856, `SK003` 4,577, `SK004` 6, `SK005` 119, and
`SK006` 21,752.

## Header dispositions

| Code | State/category | Count |
|---|---|---:|
| `HD000` | Parsed | 7,058 |
| `HD001` | Missing VTK | 19 |
| `HD002` | Missing EGL/GLES context | 1 |
| `HD003` | Missing RapidJSON | 1 |
| `HD004` | C++/CLI-only | 1 |
| `HD005` | Missing generated OCCT header in the artifact | 10 |

The machine-readable report is generated under
`OcctSharp/artifacts/generator-reports/full-inventory.json`. The current BatchSize=128
manifest-aware report has SHA256
`A6E86542CE4538EA63F14B6A58F35F628D793E4098C86BC17CDCA935EFF7257D`.

## Interpretation

Classification completeness answers “does every observed surface have an accountable
state?” Binding coverage answers “how much safe API is actually emitted and validated?”
They must never be merged. `SupportedUnselected` and `Blocked` are not package APIs, and
the 32 failed headers contribute header dispositions rather than invented declarations.

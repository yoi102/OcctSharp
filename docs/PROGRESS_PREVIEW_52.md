# Preview.52 local delivery progress

Identity Preview.52 / ABI 1.95 / bridge 0.103.0. BB authority:
OcctSharp/artifacts/batch-bb/delivery-result.json state PASS.
After freeze: AZ–BE 3/6 delivered, 3 remaining.
BC → BD → BE remain continuously authorized.

26 admitted / 59 deferred, 0 new prerequisites / 20 reused; generated +26 / Manual +0.
Release/Debug/cold Generator 280, Runtime 1136, all passed.
Actual Debug-native 1136, 62 observed DLL paths/hashes. Exports
41377 (+6/-0), Debug parity; API 57625
(+69/-0); 4320 facade forwarders. All 255 baseline
generated files unchanged; current files 258. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; BB counts complete only when delivery-result.json is PASS.

Inventory 116272: {'Blocked': 38617, 'Emitted': 27081, 'Manual': 1227, 'Skipped': 49347}; non-Skipped coverage
42.2981%. Declaration coverage is not product
completion. Pending 0; same 32 parser exclusions; semantic scan incomplete.

| Module | Emitted + Manual | Blocked | Non-Skipped percent |
|---|---:|---:|---:|
| Foundation | 3,707 | 7,188 | 34.02 |
| Geometry | 6,426 | 5,300 | 54.8 |
| Modeling | 3,570 | 11,460 | 23.75 |
| MeshData | 171 | 128 | 57.19 |
| Mesh | 179 | 311 | 36.53 |
| Documents | 1,599 | 2,339 | 40.6 |
| DataExchange | 7,679 | 7,997 | 48.99 |
| Xde | 724 | 644 | 52.92 |
| Visualization | 4,253 | 3,231 | 56.83 |
| IVtk | 0 | 0 | None |
| Draw | 0 | 19 | 0.0 |

Source uncommitted. Hosted CI, signing, push, public NuGet, external CAD
interoperability and full downstream application acceptance: NOT RUN.

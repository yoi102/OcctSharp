# Preview.50 local delivery progress

Identity Preview.50 / ABI 1.93 / bridge 0.101.0. AZ authority:
OcctSharp/artifacts/batch-az/delivery-result.json state PASS.
After freeze: AZ–BE 1/6 delivered, BA–BE 5 remaining and continuously authorized.

27 admitted / 223 deferred, 0 new prerequisites / 6 reused; generated +27 / Manual +0.
Release/Debug/cold Generator 276, Runtime 1108, all passed.
Actual Debug-native 1108, 62 observed DLL paths/hashes. Exports
41362 (+8/-0), Debug parity; API 57504
(+69/-0); 4310 facade forwarders. All 249 baseline
generated files unchanged; current files 252. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; AZ counts complete only when delivery-result.json is PASS.

Inventory 116272: {'Blocked': 38666, 'Emitted': 27032, 'Manual': 1227, 'Skipped': 49347}; non-Skipped coverage
42.2249%. Declaration coverage is not product
completion. Pending 0; same 32 parser exclusions; semantic scan incomplete.

| Module | Emitted + Manual | Blocked | Non-Skipped percent |
|---|---:|---:|---:|
| Foundation | 3,707 | 7,188 | 34.02 |
| Geometry | 6,400 | 5,326 | 54.58 |
| Modeling | 3,547 | 11,483 | 23.6 |
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

# Preview.75 local delivery progress

Development identity Preview.75 / ABI 1.113 / bridge 0.124.0.
Delivery authority: OcctSharp/artifacts/batch-bz/delivery-result.json state PASS.
BR–CI: 9/18 after final freeze, 9 remaining (CA, CB, CC, CD, CE, CF, CG, CH, CI).

111 admitted / 0 deferred, 0 new prerequisites / 5 reused; generated +111 / Manual -111 (new SDK declarations 0, Manual to Emitted 111).
Release/Debug/cold Generator 338, Runtime 1401: PASS.
Actual Debug-native 1401, 62 actually observed DLL paths/hashes: PASS.
Exports 41464 (+0/-0), Debug parity; API 58938 (+0/-0).
All 323 baseline generated files unchanged; current generated files 324.
Explicitly audited modified generated files: 0.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bz/delivery-result.json PASS completes BZ.

Inventory 116366: {'Blocked': 38345, 'Emitted': 27565, 'Manual': 1049, 'Skipped': 49407}; implemented declarations 28614.
Non-Skipped coverage 42.7336%; Pending 0.
Parser failures 28; repaired this batch 0.
Newly discovered declarations 0 are separate from migration delta 111.
These counts are not product-completion percentages; full semantic scan complete: False.

| Module | Emitted + Manual | Blocked | Non-Skipped percent |
|---|---:|---:|---:|
| Foundation | 3,732 | 7,163 | 34.25 |
| Geometry | 6,472 | 5,288 | 55.03 |
| Modeling | 3,649 | 11,381 | 24.28 |
| MeshData | 175 | 124 | 58.53 |
| Mesh | 183 | 307 | 37.35 |
| Documents | 1,666 | 2,272 | 42.31 |
| DataExchange | 7,715 | 7,961 | 49.22 |
| Xde | 769 | 599 | 56.21 |
| Visualization | 4,253 | 3,231 | 56.83 |
| IVtk | 0 | 0 | None |
| Draw | 0 | 19 | 0.0 |

Source uncommitted. Hosted CI, signing, push, public NuGet, external CAD interoperability and full downstream application acceptance: NOT RUN.

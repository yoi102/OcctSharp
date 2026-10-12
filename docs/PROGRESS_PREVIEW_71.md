# Preview.71 local delivery progress

Development identity Preview.71 / ABI 1.111 / bridge 0.120.0.
Delivery authority: OcctSharp/artifacts/batch-bv/delivery-result.json state PASS.
BR–CI: 5/18 after final freeze, 13 remaining (BW, BX, BY, BZ, CA, CB, CC, CD, CE, CF, CG, CH, CI).

17 admitted / 0 deferred, 0 new prerequisites / 6 reused; generated +17 / Manual -17 (new SDK declarations 0, Manual to Emitted 17).
Release/Debug/cold Generator 328, Runtime 1373: PASS.
Actual Debug-native 1373, 62 actually observed DLL paths/hashes: PASS.
Exports 41461 (+0/-0), Debug parity; API 58925 (+0/-0).
All 318 baseline generated files unchanged; current generated files 320.
Explicitly audited modified generated files: 1.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bv/delivery-result.json PASS completes BV.

Inventory 116366: {'Blocked': 38345, 'Emitted': 27405, 'Manual': 1209, 'Skipped': 49407}; implemented declarations 28614.
Non-Skipped coverage 42.7336%; Pending 0.
Parser failures 28; repaired this batch 0.
Newly discovered declarations 0 are separate from migration delta 17.
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

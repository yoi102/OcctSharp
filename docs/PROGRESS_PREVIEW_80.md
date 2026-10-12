# Preview.80 local delivery progress

Development identity Preview.80 / ABI 1.115 / bridge 0.129.0.
Delivery authority: OcctSharp/artifacts/batch-ce/delivery-result.json state PASS.
BR–CI: 14/18 after final freeze, 4 remaining (CF, CG, CH, CI).

34 admitted / 6 deferred, 4 new prerequisites / 17 reused; generated +38 / Manual -34 (new SDK declarations 4, Manual to Emitted 34).
Release/Debug/cold Generator 353, Runtime 1434: PASS.
Actual Debug-native 1434, 62 actually observed DLL paths/hashes: PASS.
Exports 41467 (+2/-0), Debug parity; API 59003 (+37/-0).
All 326 baseline generated files unchanged; current generated files 331.
Explicitly audited modified generated files: 3.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-ce/delivery-result.json PASS completes CE.

Inventory 116366: {'Blocked': 38341, 'Emitted': 27900, 'Manual': 718, 'Skipped': 49407}; implemented declarations 28618.
Non-Skipped coverage 42.7396%; Pending 0.
Parser failures 28; repaired this batch 0.
Newly discovered declarations 0 are separate from migration delta 38.
These counts are not product-completion percentages; full semantic scan complete: False.

| Module | Emitted + Manual | Blocked | Non-Skipped percent |
|---|---:|---:|---:|
| Foundation | 3,734 | 7,161 | 34.27 |
| Geometry | 6,472 | 5,288 | 55.03 |
| Modeling | 3,649 | 11,381 | 24.28 |
| MeshData | 175 | 124 | 58.53 |
| Mesh | 183 | 307 | 37.35 |
| Documents | 1,666 | 2,272 | 42.31 |
| DataExchange | 7,715 | 7,961 | 49.22 |
| Xde | 769 | 599 | 56.21 |
| Visualization | 4,255 | 3,229 | 56.85 |
| IVtk | 0 | 0 | None |
| Draw | 0 | 19 | 0.0 |

Source uncommitted. Hosted CI, signing, push, public NuGet, external CAD interoperability and full downstream application acceptance: NOT RUN.

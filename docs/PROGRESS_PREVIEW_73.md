# Preview.73 local delivery progress

Development identity Preview.73 / ABI 1.113 / bridge 0.122.0.
Delivery authority: OcctSharp/artifacts/batch-bx/delivery-result.json state PASS.
BR–CI: 7/18 after final freeze, 11 remaining (BY, BZ, CA, CB, CC, CD, CE, CF, CG, CH, CI).

12 admitted / 0 deferred, 0 new prerequisites / 8 reused; generated +12 / Manual -12 (new SDK declarations 0, Manual to Emitted 12).
Release/Debug/cold Generator 332, Runtime 1392: PASS.
Actual Debug-native 1392, 62 actually observed DLL paths/hashes: PASS.
Exports 41464 (+2/-0), Debug parity; API 58938 (+11/-0).
All 318 baseline generated files unchanged; current generated files 322.
Explicitly audited modified generated files: 3.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bx/delivery-result.json PASS completes BX.

Inventory 116366: {'Blocked': 38345, 'Emitted': 27435, 'Manual': 1179, 'Skipped': 49407}; implemented declarations 28614.
Non-Skipped coverage 42.7336%; Pending 0.
Parser failures 28; repaired this batch 0.
Newly discovered declarations 0 are separate from migration delta 12.
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

# Batch AA: Copied reference and multi-result operations

- State: implementing ADR-0095; full delivery checks NOT RUN.
- Entry product: c80f09c, Preview.25 / ABI 1.69 / bridge 0.77.0.
- Entry: 22,038 generated, 1,217 manual, 43,670 Blocked, 49,347 Skipped.
- Total: 116,272 declarations; 7,058/7,090 headers, 32 exclusions.
- Entry inventory SHA256: C81FD10258895F765187AEC809FD3ECB337ED2776E70EF39669F27CF6EF1E75B.
- Target: Preview.26 / ABI 1.70 / bridge 0.78.0.
- Development evidence: 598 generated additions, zero removed declarations, 126 generated
  files across the product. Existing 34,432 generated native functions retain exact
  signatures/bodies. Generator tests: 154/154 PASS. Dependency closure: PASS.
  Release native/managed compile and runtime 528/528 PASS. Strict C11 compilation of
  all 31 generated headers PASS. Native exports: 35,775, +598 / -0, Debug equals Release.
  Managed public signatures: +818 / -0 (598 methods and 220 operation classes).
  Debug runtime, cold regeneration and package/product delivery gates remain in progress.

## Generated scope

| Module | Methods | Operation classes |
|---|---:|---:|
| Foundation | 6 | 3 |
| Geometry | 344 | 97 |
| MeshData | 2 | 1 |
| Modeling | 98 | 46 |
| Mesh | 5 | 4 |
| Documents | 7 | 1 |
| Visualization | 84 | 42 |
| DataExchange | 44 | 23 |
| Xde | 8 | 3 |
| Total | 598 | 220 |

The 598-entry catalogue matches stable IDs and native signatures. The 21 explicitly
unselected candidates retain their old classification: fifteen dimensioned array-start
interfaces and six platform-specific interfaces. No manual binding was added.
Generated source lives in nine independent native header/source and managed file sets.
The facade forwards 3,750 public types. Existing 34,432 generated native bodies remain
unchanged, including both old generated and value-operation export families.

## Runtime evidence

The new eleven runtime regressions cover analytic circle third derivatives; scalar
coordinate extraction and initialized coordinate transforms; plane and cylinder
coefficient sets; RGB/HLS round trips at the pinned SDK's internal float precision;
XDE enum outputs; boolean false and true query behavior; invalid-input failure atomicity;
disposed receivers; raw null/invalid handles; null output pointers, zeroed failed raw
outputs, and cleared error state after success. The shared workflow is also compiled
into both package consumers. Raw C outputs are initialized on failure after every
output pointer has been validated; managed ref values remain unchanged on exceptions.

The accepted scope covers scalar/enum/geometric in/out references across existing
const shared receivers, copied geometric receivers and static helpers. Curve/surface
derivatives, parameter conversion, geometry coefficients, mesh/display attributes and
exchange/document queries are selected by audited stable IDs. A generated candidate is
not counted as migrated until emission, compilation and validation complete.

See [ADR-0095](adr/0095-generated-reference-operations.md) for ownership, aliasing,
publication and full delivery contracts. [STATUS](STATUS.md) owns current evidence.

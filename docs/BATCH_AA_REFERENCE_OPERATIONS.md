# Batch AA: Copied reference and multi-result operations

- State: local implementation and product delivery gates PASS under ADR-0095.
- Entry product: c80f09c, Preview.25 / ABI 1.69 / bridge 0.77.0.
- Entry: 22,038 generated, 1,217 manual, 43,670 Blocked, 49,347 Skipped.
- Total: 116,272 declarations; 7,058/7,090 headers, 32 exclusions.
- Entry inventory SHA256: C81FD10258895F765187AEC809FD3ECB337ED2776E70EF39669F27CF6EF1E75B.
- Product: Preview.26 / ABI 1.70 / bridge 0.78.0.
- Validated evidence: 598 generated additions across 81 OCCT packages, zero removed declarations, 126 generated
  files across the product. Existing 34,432 generated native functions retain exact
  signatures/bodies. Generator tests: 154/154 PASS. Dependency closure: PASS.
  Release native/managed compile and runtime 528/528 PASS. Strict C11 compilation of
  all 31 generated headers PASS. Native exports: 35,775, +598 / -0, Debug equals Release.
  Managed public signatures: +818 / -0 (598 methods and 220 operation classes).
  Debug runtime and actual Debug-native 528/528 PASS (62 DLL hashes verified).
  Cold source-copy build reproduces all 126 generated files byte-for-byte. Fourteen
  local packages and both clean package consumers PASS, including the AA workflow.
  Full local release-check reports batchImplementationComplete=true. The check resumed
  at its final inventory step after an execution interruption; original and resumed
  evidence are in artifacts/batch-aa-release-check.log and batch-aa-release-tail.log.
- Exit: 22,636 generated, 1,217 manual, 43,072 Blocked, 49,347 Skipped; coverage 20.51%.
- Exact audit: 598 Blocked-to-Emitted transitions; zero unrelated disposition, discovery
  identity or header changes. Inventory uses the baseline's batch size of 128.
- Exit inventory SHA256: AE0C2EA053355D9D94E2DD20F9FB0B2EF41A10E22610EF745C59EF7DA887966E.
- Implementation commit: 581b887. STATUS records final package-content evidence.
- Remote CI, signing, GitHub push and NuGet publication: NOT RUN.

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

## Usage

```csharp
using OcctSharp;
using OcctSharp.ReferenceOperations;
using OcctSharp.ValueOperations;

double x = 1, y = 2, z = 3;
var translation = Transform3dOperations.Create();
Transform3dOperations.SetTranslation(ref translation, new OcctSharp.Values.Vector3d(3, -2, 5));
GpTrsfReferences.Transforms(translation, ref x, ref y, ref z);
// x = 4, y = 0, z = 8. A failing call leaves all three variables unchanged.
```

These are initialized in/out references, so geometric arguments need valid initial
records. Use the copied-value Create methods for directions, frames and transforms;
their default zero records may be invalid. The wrappers retain the kernel's documented
domain, indexing and numerical precision; they do not infer output-only directions.

See [ADR-0095](adr/0095-generated-reference-operations.md) for ownership, aliasing,
publication and full delivery contracts. [STATUS](STATUS.md) owns current evidence.

# Batch Z: Geometric value operations and transformation migration

- State: all local product gates passed. Final artifact evidence is maintained in STATUS.
- Entry: 20,650 generated, 1,217 manual, 45,058 Blocked and 49,347 Skipped IDs.
- Entry inventory: 116,272 IDs, SHA256
  `CCCD42562A55D0345518F446D8D0B5746CC047B88CAC849F3E0CE337793EE5FC`.
- Entry product: 2083f99; preserve README update 845e3d6.
- Target: Preview.25 / ABI 1.69 / bridge 0.77.0.

The accepted scope adds a reusable copied-receiver operation emitter and geometric
transform projections across existing modules. Constructors, queries and successful
mutations must work as complete public value workflows, with failed mutations leaving
the original receiver unchanged. Mapping support is not claimed as native declaration
coverage until the selected output is emitted, compiled and validated.

See [ADR-0094](adr/0094-generated-geometric-value-operations.md) for contracts and full
delivery gates. [STATUS](STATUS.md) records current execution and verified counts.

## Implementation checkpoint

```csharp
using OcctSharp;
using OcctSharp.ValueOperations;
using OcctSharp.Values;

var move = Transform3dOperations.Create(); // Identity, not default zero coefficients.
Transform3dOperations.SetTranslation(ref move, new Vector3d(3, -2, 5));
var point = new Point3d(1, 2, 3);
Point3dOperations.Transform(ref point, move); // (4, 0, 8)
Transform3dOperations.Invert(ref move);
Point3dOperations.Transform(ref point, move); // (1, 2, 3)
```

These public APIs are exercised by `tests/Shared/BatchZValueWorkflow.cs` in Runtime
tests and both clean package consumers. Existing owning types remain available;
the Values namespace contains the independent immutable coefficient records.

Local implementation commit: d75082a. Entry product and README remain preserved.
Generated plus accepted manual declaration coverage: 18.81% -> 20.00%; this is not
functional completeness. Full migration remains incomplete.

| Generated additions | Count |
|---|---:|
| Geometry value operations | 1,236 |
| Geometry existing shared/static paths | 105 |
| Visualization | 29 |
| DataExchange | 9 |
| Modeling | 8 |
| MeshData | 1 |
| Total | 1,388 |

All 1,388 IDs move Blocked -> Emitted. Seven other IDs remain Blocked with audited
narrower pointer/output/sequence reasons. There are no removed generated or manual IDs,
new manual IDs, changed header exclusions or denominator changes. The accounting script
and four deliberately corrupted fixtures validate this boundary.

## Validation evidence

- PASS: Release/Debug native/managed builds and deterministic generation; 99 generated files.
- PASS: Release/Debug Generator 153/153 and Runtime 517/517, including 24 Batch Z tests.
- PASS: actual Debug-native Runtime 517/517; all 62 DLLs verified by hash.
- PASS: Release/Debug geometric ABI, 22 C11 headers and 34 native/managed value layouts.
- PASS: all 33,044 old generated native signatures/bodies unchanged.
- PASS: 1,487 public managed API additions, zero removals; 3,530 facade forwarders.
- PASS: 35,177 native exports, +1,388, zero removals; Debug and Release names match.
- PASS: first fourteen-package verification, clean facade and direct module consumers.
- PASS: exact full inventory accounting and four rejection fixtures.
- PASS: cold source copy builds; 99 generated files are byte-identical.
- PASS: full local release-check, batchImplementationComplete=true.
Final documentation-package verification is recorded in STATUS after repacking.

Full inventory SHA256:
`C81FD10258895F765187AEC809FD3ECB337ED2776E70EF39669F27CF6EF1E75B`.
Release bridge: 18,670,592 bytes, SHA256
`A8C44A2DF06E0583DE88DA3B4393EBE4FA9DE62A360B9AB1781650C6AC236EDD`.
Debug bridge SHA256:
`5BBCA9DBCE1FE59B7445911724B4DB047987659F871418F7CB845E2B05BCBB63`.

The native fixture exposed nonfinite results from OCCT similarity reconstruction of
some singular matrices. Conversion checks finiteness and coefficient round-trip;
general affine reconstruction retains original singular coefficients. SC-063 records
explicit IsSingular guards for inverse and negative powers where the pinned SDK may
otherwise return instead of throwing. Failed managed mutations retain their receiver.

The user-supplied original icon is retained. NuGet packages use the separately generated
256x256 occtsharp-package-icon.png (72,358 bytes), stored in each package as
occtsharp-icon.png to retain existing package metadata. This resolves NU5047 without
replacing the full-resolution source image.

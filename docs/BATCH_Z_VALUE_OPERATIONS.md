# Batch Z: Geometric value operations and transformation migration

- State: implementing ADR-0094; Y is the validated baseline.
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

## Development evidence (not product completion)

- Emitted manifest: 22,038 IDs, +1,388 versus frozen Y, zero removals; 99 files.
- Source audit: all 33,044 old generated native functions preserve signatures/bodies.
- Generator: 153/153 Release tests passed.
- Geometric ABI: Release and Debug pass, 22 pure C11 headers and 34 value layouts;
  matrix ordering, transforms, similarity rejection, affine shear/singular handling.
- Facade: regenerated 3,530 forwarders; 1,487 additions and zero removals in the rebuilt API diff.
- Full Release/Debug suites, actual Debug-native, final export comparison, fresh inventory
  accounting, cold regeneration and package consumers: pending, NOT RUN to completion.

The native fixture exposed nonfinite results from OCCT similarity reconstruction of
some singular matrices. Similarity conversion now checks both finiteness and coefficient
round-trip; general affine reconstruction retains its original singular coefficients.
Release runtime passes 517/517, including 24 Z tests. Both clean package consumers pass.
Exact accounting passes: +1,388 emitted, seven Blocked refinements, unchanged manual,
skipped and header sets; all four deliberately corrupted accounting fixtures reject.
Full inventory SHA256: C81FD10258895F765187AEC809FD3ECB337ED2776E70EF39669F27CF6EF1E75B.
The Release bridge is 18,670,592 bytes, SHA256
A8C44A2DF06E0583DE88DA3B4393EBE4FA9DE62A360B9AB1781650C6AC236EDD.
Final Debug/cold/local release completion remains pending at this implementation checkpoint.

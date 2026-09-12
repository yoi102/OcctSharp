# Batch AB: Owning non-transient receivers and operations

- State: implemented and locally validated under [ADR-0096](adr/0096-generated-owned-nontransient-values.md).
- Product: Preview.27 / ABI 1.71 / bridge 0.79.0; implementation commit f58a580.
- Entry product: c65b480, Preview.26 / ABI 1.70 / bridge 0.78.0 (Batch AA).
- Entry: 22,636 generated, 1,217 accepted manual, 43,072 Blocked, 49,347 Skipped.
- Entry total: 116,272 declarations; 7,058/7,090 headers and 32 explicit exclusions.
- Entry declaration coverage: 20.51%; this is not functional completeness.
- Exit: 23,703 generated, 1,217 accepted manual, 42,005 Blocked, 49,347 Skipped;
  21.43% declaration coverage. Final package-content evidence is recorded in STATUS.

## Frozen entry evidence

The inner workspace contains the immutable input copies in
`artifacts/preparation-baselines/preview26-batch-ab/`. Source is the completed AA
product at c65b480, before AB generation. `baseline-evidence.json` records each file's
size/hash and entry counts. The API snapshot was read from the existing Release
facade and contains 49,943 signatures; no rebuild was used to prepare it.

| File | SHA256 |
|---|---|
| manifest.json | 71A004577A602C76BD07C2731C4BE049E976BB81FCA012C0B2A7B784A3FE2056 |
| full-inventory.json | AE0C2EA053355D9D94E2DD20F9FB0B2EF41A10E22610EF745C59EF7DA887966E |
| OcctSharp.Native.dll | CE2A270D75B91CF4FE0F82BFA746929DE912E9067AE5CED2E4BC8E5AD9D5D8E8 |
| api.json | FA12CD6F2E4CD5377A4C706F2A73BB2F9DF1336D89E7CDA5FF2A614C70201232 |

The entry manifest records 22,636 generated declaration IDs and 126 generated files.
The Release native DLL is 19,127,296 bytes. Preserve the 35,775 entry exports and all
35,030 generated native operation signatures/bodies, including AA's 598 ref exports.
The entry facade has 3,750 forwarded types. These are AA baseline facts, not AB results.

## Accepted implementation scope

Introduce a reusable generated ownership category for independently constructible and
destructible non-transient classes. Select exact audited constructor/method identities
and signatures; report actual emitted additions after validation rather than using a
target method count. Published native objects remain in stable heap storage and are
neither copied nor moved. Managed typed SafeHandles own opaque tokens with matched
release in the existing Native DLL.

A single type-tagged token registry and shared_ptr operation leases prevent native
storage from disappearing after validation. Tokens monotonically increase and are
never recycled, so a released token cannot resolve to a later allocation. Release
removes the registry owner; already-acquired leases keep their instance alive until
the call ends. An operation racing after removal fails without accessing that instance.
Callers still synchronize concurrent object mutation and obey native thread rules.

Only existing approved copied projections and specifically audited owned-input
relationships participate. Mutable copied ref arguments are initialized snapshots;
publish all conversions after success and preserve managed refs on exceptions.
Owned object changes and internal cache updates are not transactional or rolled back.
Manual/Skipped IDs and old generated exports/bodies remain unchanged. Borrowed members,
external-buffer lifetimes, unsupported constructors/destructors and unsafe array-start
references stay excluded. No module, facade or native-DLL split is introduced.

## Implemented catalogue and validation

The implementation catalogue currently freezes 122 owners and 1,067 migration IDs:
446 constructors formerly BL103 and 621 instance operations formerly BL102;
891 Geometry, 149 Modeling and 27 Foundation. It covers 15 OCCT packages: Bnd, analytic Extrema,
IntAna/IntAna2d, GccAna/GccEnt, GC/gce makers, GProp, GeomAPI/Geom2dAPI, LProp,
quaternion interpolation and selected math utilities. `OwnedValues` is the managed
namespace; `occtsharp_owned_` is the separate native export family. TM010 applies only
inside the exact catalogue, with copied owning results restricted to 24 audited
data types. Algorithms are constructed in place and never copied.

Supporting operations are listed separately: 187 inherited or existing Manual method
projections and two default factories (Bnd_Box2d and Bnd_OBB). They make the new wrapper
usable without changing those declarations' previous disposition. Release functions
and supporting operations are native/API additions, not migrated declaration IDs.
The discovery denominator and all 1,217 accepted Manual IDs remain the accounting
baseline. No wildcard owner rule applies to future SDK declarations.

For example, the new owned range supports mutation, leased arguments and independent
returned copies (the same behavior is exercised by BatchABOwnedTests):

```csharp
using OcctSharp.OwnedValues;

using BndRange first = BndRange.Create(1, 5);
using BndRange second = BndRange.Create(3, 8);
first.Common(second);
using BndRange shifted = first.Shifted(10);
double minimum = 0, maximum = 0;
shifted.GetBounds(ref minimum, ref maximum); // 13, 15
```

Development evidence: all 1,067 IDs have generated successfully in 137 files with a
complete, acyclic managed dependency closure. Native compilation/linking has passed.
All 35,030 previous generated native signatures/bodies are unchanged. Strict C11 checks
pass for 34 headers. The standalone registry fixture passes null/wrong-type/stale and
repeated release, token non-reuse, stable addresses, 250 release races with 16,000 lease
attempts, and final destruction, in both Release and Debug fixture configurations.
Release and Debug native/managed builds, deterministic generation/discovery, Generator
163/163 and Runtime 542/542 pass, including 14 AB runtime tests. The new public API has 1,500
additions and zero removals. The bundled 62-DLL runtime matches the Release build.
Seven accounting rejection fixtures pass against an isolated synthetic inventory;
the final real inventory confirms precisely 1,067 Blocked-to-Emitted IDs with no other
disposition or header changes. Implementation is saved at f58a580.
Release used SDK 10.0.400; resumed Release/Debug and cold builds use 10.0.401 under the
repository's latestPatch policy and reproduces the identical generated/discovery hashes.
Actual Debug-native Runtime 542/542 passes with all 62 DLL hashes checked. Native
export comparison passes with 37,153 exports, 1,378 additions and zero removals;
Debug and Release sets match. The final read-only audit also checked all 187 support entries: there are no matching
const/nonconst overload pairs and no module edges outside the accepted graph. Support
entries retain separate catalogue evidence; they are not added to migrated stable IDs.

Fourteen local packages and clean facade/direct-module consumers pass with the AB
workflow. A cold source copy builds, passes Generator 163/163 and Runtime 542/542,
and reproduces all 137 generated files byte-for-byte. Local release-check gates pass;
batchImplementationComplete=true. Execution resumed after interruption with evidence
in artifacts/batch-ab-release-check.log, batch-ab-release-resume.log and
batch-ab-release-tail.log. Final inventory SHA256:
`14B0D49AB1929D51D2E9BA17240641C6537C11DDEAF89F7F6F0AFAA0A9A24BE2`.

The final rebuilt Release bridge was synchronized to the bundled runtime and rerun
against all 542 Runtime tests. Its SHA256 is
`FE5AE1330F6951EF2560E90AA97967BD0CC8A5B818DC00519111F58327E0498A`;
the tested Debug bridge SHA256 is
`451780944BA0C259C1786174B988383BB93D5FA9C4928CFEF8C8F4682020EEC0`.

See [OWNERSHIP](OWNERSHIP.md) and [TYPE_MAPPING](TYPE_MAPPING.md) for contracts.
[STATUS](STATUS.md) owns live progress. Commits remain local; push/publication are NOT RUN.

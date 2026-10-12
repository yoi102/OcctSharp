# Preview.85 public delivery

Version8.0.1-preview.85 / ABI1.118 / bridge0.133.0 is published on nuget.org.
All14 package downloads, catalog SHA512 identities, exact payloads and NuGet
repository signatures passed. Two independent consumer projects restored into
fresh package/HTTP caches using only nuget.org with .NET SDK10.0.401, compiled
and passed their original runtime workflows. The direct-module project has no
facade assembly. Both consumers contain the exact published62-DLL runtime.

Author signing and hosted CI remain NOT RUN. NuGet repository signing was
actually verified with `dotnet nuget verify --all`; it is a separate result.

## Frozen public identities and evidence

Local receipts are under `OcctSharp/artifacts/cadoryx-public-sdk85/`.
`public-delivery.json` binds the final local delivery, package-envelope/payload
equivalence, actual14 HTTP201 submissions, remote identities, signatures and
fresh public-source consumer results by SHA256. The original submission receipt
remains `SUBMITTED`; public availability and consumption are independently
established by the later remote receipts.

The final precompiled Release bridge SHA256 is
`D44F3F272386EA2CC87E7107AB2E3375D8100283872372F1FBC234A3B4E38CB3`.
The actual Debug bridge SHA256 is
`00219F979EC348F4B1682DC43DB7B64D262C81A8F9626C208644A0D01A4A5B2B`.
The generation manifest SHA256 is
`EB97291E0667FB9EE9AFE33E8383BFD92F797E52A1A91D880CE071121E4002A6`.
Earlier bridge hashes identify historical builds, not the published payload.

| Gate | Result | Authoritative receipt |
|---|---|---|
| Complete Release build/Generator/Runtime | PASS,410/2255 | `template-fix-release-full.json` |
| Complete Debug build/Generator/Runtime | PASS,410/2255 | `template-fix-debug-full.json` |
| Actual Debug-native Runtime | PASS,2255 and62 exact loaded DLLs | `template-fix-actual-debug/result.json`, `template-fix-actual-debug-loaded-modules.json` |
| Corrected-source cold regeneration/build/test | PASS,399 exact outputs/410/2255 | `template-fix-cold.json` |
| Native export compatibility | PASS,41509 names,+9 public C/+2 compiler symbols,-0 | `template-fix-native-exports.json` |
| Public managed API compatibility | PASS,59287 baseline,+104/-0 | `api-frozen-public-diff.json` |
| Final local14 packages and two consumers | PASS | `local-delivery.json`, `template-fix-package-consumers.json`, `final-payload-equivalence.json` |
| Package content and checksum rejection fixtures | PASS,439 documents/73 runtime-license files/19 checksums,6 fixtures | `template-fix-package-content.json`, `final-checksum-tests/result.json` |
| Remote payload/catalog identities | PASS,14/14 | `remote-package-identities.json` |
| NuGet repository signatures | PASS,14/14 | `remote-signatures.json`, `remote-packages/*.verify.log` |
| Fresh public-only consumer restore/build/runtime | PASS,two consumers/14 packages/62 DLLs | `remote-consumers.json`, `remote-consumers-4.log` |

The local gates were independently executed in full. The unified
`release-check.ps1` entry was not replayed as one invocation. Native SDK
deprecation warnings remain in the native build logs.

NuGet's full signed archive SHA512 (`remoteSha512`) and authenticated package
content hash (`nugetContentSha512`) have distinct meanings. Catalog/download
bytes and cache `.nupkg.sha512` match the archive hash. Restore metadata,
assets and lock content hashes match the authenticated content hash. Both are
recorded, and the verified content hash matches the original unsigned package.
Actual assets resolved sources and every family package's metadata source are
nuget.org; implicit SDK library-packs sources were explicitly disabled.

Published packages are immutable. Subsequent source/build-tool/documentation
maintenance in this checkout is not retrospectively included in those packages.

## Independent source dependency provider

The [pinned vcpkg route](DEPENDENCY_RECOVERY.md) is for OcctSharp contributors.
NuGet consumers, including Cadoryx, do not run vcpkg. Its source-recovery,
configuration-specific bridge/runtime, provenance and license results are
qualified separately and do not replace the published precompiled provider.
The original no-FFmpeg profile's2253/2255 failure remains historical evidence;
the revised profile preserves the original Media calls and test assertions.
The revised provider's empty/no-binary-cache source recovery passed, with7478
exact headers and399 exact generated outputs. Release/Debug bridges compile
with84 DLLs and22 copyright texts each; recursive closure checks pass6 cases,
dependency rejection checks pass8 cases, and41509 exports match the public
bridge with no additions/removals. Actual configuration Runtime suites each
passed2255/2255 using SDK10.0.401. The retained first profile failed2253/2255.

`vcpkg-media-qualification.json` binds source inputs and independent receipts.
Both isolated actual bridges were observed loaded with exact configuration
hashes; Release sampled84 matching modules and Debug sampled40. The Debug
snapshot is partial, so it does not establish that all84 DLLs were observed
loaded. Every copied native DLL was independently checked against its manifest
and installed port. Native SDK deprecation/macro warnings remain in logs.

Git retains the qualified embedded template and identity manifests as exact
bytes. This prevents checkout newline conversion from changing embedded
resource fingerprints or invalidating frozen runtime identities.

## Coverage and remaining product boundaries

The generator emits399 outputs and4472 facade forwarders;220 authored fixtures
exercise the added consumer contracts. No new raw declaration credit is claimed.
Overall inventory stays116366 declarations:28020 Emitted,598 Manual,38341 Blocked,
49407 Skipped,0 Pending. Non-Skipped support is28618/66959=42.7396%; full migration
is not complete. Classification is complete, with28 explicit semantic header
exclusions; that is not successful semantic scanning of every SDK header.

Cadoryx's24 strict STEP rejections and A-F product acceptance belong to its
separate application work. Their tolerances/assertions remain unchanged; G is
user-deferred. Physical GPU removal, display/input latency, mixed-DPI/RDP and
human acceptance remain independent gates. This SDK publication does not close
those application or physical-device boundaries.

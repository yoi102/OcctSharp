# Dependency Management

Optional integration prerequisites are separately declared in
`OcctSharp/config/dependency-profiles.json` and checked by
`OcctSharp/eng/audit-dependency-profiles.ps1`. They are not silently inherited from a
developer machine and do not enter the core NuGet closure. See
[`OPTIONAL_INTEGRATIONS.md`](OPTIONAL_INTEGRATIONS.md) and ADR-0047.

## Goal

Every supported build and generation run must be reproducible from an immutable,
reviewable dependency definition. A local OCCT installation discovered by chance is
not a release input.

## OCCT dependency record

Each supported OCCT baseline must record:

- Upstream version and immutable source commit/tag or artifact checksum.
- Acquisition method and authoritative URL or package identity.
- Applied patches, with reason and patch hash.
- Build system and complete configuration options.
- Enabled and disabled OCCT modules/features.
- Compiler, standard library, architecture, runtime linkage, and build type.
- Third-party dependencies and versions.
- Header, library, and runtime artifact layout.
- License and redistribution evidence.

The initial machine-readable record is
`OcctSharp/config/occt-8.0.1-windows-x64.json`. It records the distribution identity,
expected layout, supported configurations, representative binary/header hashes, and
upstream license-file hashes. The local absolute path remains ignored.

## Toolchain lock

Generation depends on both OCCT and the C++ parser toolchain. Pin and record:

- Clang/libclang or equivalent parser version.
- Parser managed/native package versions.
- CMake and build-generator versions used in CI.
- .NET SDK version.
- Native compiler and platform SDK version.
- Package manager and lockfile versions, if used.

Compiler arguments that affect parsing are part of the generator input, not local
developer preferences.

## Acquisition strategies under consideration

| Strategy | Development | Release | Current state |
|---|---|---|---|
| Controlled OCCT source build | Reproducible but slower | Strong provenance | Candidate |
| Verified prebuilt OCCT artifacts | Fast | Requires trusted build metadata | Selected for local Phase 0 |
| vcpkg | Convenient | Must not require package manager on user machines | Candidate |

ADR-0004 selects the supplied prebuilt combined distribution for the first local
baseline. B20 configures CI acquisition through an immutable archive URL plus SHA256;
hosted execution still requires repository variables and has not been run.

## Native redistribution

NuGet consumers should not manually locate OCCT runtime libraries. Published packages
must either carry the complete allowed native dependency closure for a RID or declare
an explicit supported external-runtime contract. ADR-0008 selects one self-contained
package for the initial Windows x64-only matrix. Its transitive build asset copies the
bridge and 44 dependent runtime DLLs into the application's `occt` directory.

Runtime packaging validation must inspect actual binary dependencies, not only the
presence of the OcctSharp native bridge.

The current local package includes the OCCT LGPL 2.1 text and OCCT linking exception.
Complete notices and redistribution review for every bundled third-party DLL remain a
public-release gate; creating a local package is not evidence that publication is ready.

## Supply-chain requirements

- Verify downloaded artifact hashes.
- Preserve third-party license and notice files.
- Do not commit unlicensed CAD fixtures or binaries.
- Regenerate the implemented CycloneDX SBOM, provenance, and fixed-order SHA256 evidence
  for every release candidate.
- Resolve all `unknown` non-OCCT component versions/licenses and define signing policy
  before public release.

The current local evidence records 45 native files, including the bridge, in
`artifacts/release/`. This is technical provenance only; it does not replace legal review
or make `publicReleaseReady` true.

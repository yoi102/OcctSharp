# ADR-0051: Bootstrap Repository Native Runtime from Pinned OCCT Inputs

- Status: Superseded in part by ADR-0059
- Date: 2026-08-24
- Scope: Repository samples and developer builds

## Context

The NuGet package already carries the complete native runtime below the consumer's
`occt/` output directory. Repository projects cannot commit the native build artifacts,
however, so a fresh Git clone previously failed while building the Sample project unless
the developer manually ran the full generation and test pipeline first. The ignored
`config/local.settings.json` also cannot provide another computer with an OCCT SDK path.

The repository needs a repeatable native-only bootstrap without making managed builds
recursively invoke `eng/build.ps1`, and without silently downloading an unverified SDK.

## Decision

ADR-0059 now makes a verified committed Windows x64 Release runtime the default Sample
input. The bootstrap below remains accepted for contributors who set
`OcctSharpUseBundledNativeRuntime=false` to rebuild native output from pinned inputs.

- `eng/ensure-native.ps1` is the repository-native bootstrap entry point. It validates
  the pinned OCCT 8.0.1 manifest, configures CMake, builds only the requested Debug or
  Release native bridge, and verifies the application-local runtime closure.
- The Sample project invokes this entry point incrementally through
  `Directory.Build.targets` when `artifacts/native/<Configuration>/OcctSharp.Native.dll`
  is missing or older than native source/generator output. The existing copy target then
  places the complete closure below the Sample output's `occt/` directory.
- OCCT input resolution is ordered: explicit parameter, `OCCTSHARP_OCCT_ROOT`, ignored
  local settings, or an immutable HTTPS archive URL plus SHA256. Archive downloads and
  extraction stay below ignored `OcctSharp/artifacts/dependencies/`.
- No unverified or floating download URL is committed. If neither a local SDK nor an
  approved URL/SHA256 pair is configured, the build fails with exact configuration
  instructions.
- Installed NuGet consumers remain independent of this bootstrap and do not require an
  OCCT SDK, CMake, or Visual Studio because ADR-0008 package assets already carry `occt/`.

## Alternatives

- Committing `artifacts/native/` was rejected because build products and third-party
  binaries do not belong in Git and would obscure provenance.
- Calling `eng/build.ps1` from MSBuild was rejected because that script rebuilds the
  managed solution, regeneration, and tests, creating recursion from a Sample build.
- Falling back to a machine-wide OCCT installation was rejected because it can select
  an incompatible binary set.

## Consequences

- A configured fresh clone can build or run the Sample project directly; the first run
  pays the native CMake build cost and subsequent builds are incremental.
- A new computer still needs either the pinned OCCT SDK or an approved immutable archive
  location and SHA256. Git clone alone cannot supply uncommitted third-party binaries.
- Repository contributors need the .NET 10 SDK and Visual Studio C++/CMake workload.

## Validation

- Rename the local Debug bridge out of the expected artifact path.
- Build the Sample project normally and verify the native-only bootstrap recreates the
  45-DLL runtime, copies it below Sample output `occt/`, and does not recurse.
- Run the entity-creation Sample to prove the rebuilt bridge loads and calls OCCT.
- Verify missing dependency configuration produces an actionable error.

# Build and Release

This document defines the build and release process. Local packaging and reproducible
Release evidence tooling is implemented inside batch B; public publication remains externally gated.

## Validated baseline

- Managed target: .NET 10 only (`net10.0`, SDK 10.0.400).
- Platform: Windows x64.
- Native compiler: Visual Studio 2026/MSVC 19.51.
- OCCT: 8.0.1 combined VC14 x64 distribution with Debug and Release libraries.
- AST: ClangSharp 21.1.8.4 with libClangSharp 21.1.8.2.

## Clone-and-run sample

No native developer configuration is required for the committed examples:

```powershell
git clone https://github.com/yoi102/OcctSharp.git
cd OcctSharp\OcctSharp
.\eng\verify-bundled-runtime.ps1
dotnet run --project .\samples\OcctSharp.Samples -- --smoke
```

The repository supplies the 62-DLL Windows x64 Release closure. Both Debug and Release
managed configurations use it. The smoke command verifies runtime identity and creates
an OCCT box, without reading local settings or building C++.

## Native contributor configuration

Copy `OcctSharp/config/local.settings.example.json` to
`OcctSharp/config/local.settings.json` and set:

- `occtRoot` to the directory containing `inc/`, `cmake/`, and `win64/`.
- `visualStudioRoot` to the Visual Studio installation containing CMake and C++ tools.

`local.settings.json` is ignored because it contains machine-specific absolute paths.
The committed `OcctSharp/config/occt-8.0.1-windows-x64.json` records the expected
distribution layout and verification hashes without local paths.

As an alternative to a pre-extracted SDK path, set both
`OCCTSHARP_OCCT_ARTIFACT_URL` and `OCCTSHARP_OCCT_ARTIFACT_SHA256`, or set the matching
`occtArtifactUrl`/`occtArtifactSha256` local settings. Only an absolute HTTPS URL with a
64-digit SHA256 is accepted. The verified archive is cached and extracted below ignored
`artifacts/dependencies/`; no artifact URL is currently committed or implicitly trusted.

To deliberately rebuild the native bridge instead of using the committed runtime:

```powershell
dotnet run --project .\samples\OcctSharp.Samples --configuration Debug `
  -p:OcctSharpUseBundledNativeRuntime=false
```

With the bundled runtime disabled, if the Debug native runtime is missing or stale, MSBuild invokes
`eng/ensure-native.ps1`, builds only the native bridge and current 62-DLL closure, and copies it
to the Sample output's `occt/` directory. It does not call `eng/build.ps1` and cannot
recurse into the managed build. This override requires one of the documented pinned
OCCT inputs; the default clone-and-run path does not.

## Current local commands

From the inner `OcctSharp/` workspace:

```powershell
.\eng\build.ps1 -Configuration Release
.\eng\build.ps1 -Configuration Debug
```

The machine-specific OCCT and Visual Studio roots come from ignored
`config/local.settings.json`, environment variable/parameters, and committed examples.
`global.json` locks the .NET SDK to 10.0.400.

The script validates dependency and toolchain identities, restores and bootstraps the
generator, regenerates committed native/managed raw source, configures and builds the
native bridge, copies the OCCT/TBB/jemalloc runtime closure, builds the .NET solution,
runs generation twice to verify coverage/diagnostics report hashes, runs deterministic
model and real-header discovery twice, and executes the configured generator and runtime
tests. Build output stays under ignored inner
workspace `artifacts/`, `build/`, `bin/`, and `obj/` directories.

ADR-0081 adds native source-boundary verification to this entry point. With Batch Q, all 51 manual
translation units compile without the generated PCH or unity builds. Source-list,
implementation-size, unique manual registry/TLS ownership, and six negative fixture
checks guard against reintroducing the historical monolith. See
[the complete source map](NATIVE_SOURCE_LAYOUT.md).

For an architecture-only extraction, preserve the pre-change DLL and compare the
complete export-name set as well as the ordinary API compatibility baseline:

```powershell
.\eng\verify-native-source-layout.ps1 `
  -NativeLibraryPath artifacts/native/Release/OcctSharp.Native.dll `
  -BaselineNativeLibraryPath artifacts/native-layout/baseline-native.dll
```

The baseline path above is an example local evidence file, not a distributed runtime.
Ordinary Debug managed tests still load the committed Release runtime; an actual
Debug-native lifetime regression must separately use the rebuilt Debug DLL closure.

To verify that committed generated output is current, run:

```powershell
.\eng\verify-generated.ps1 -Configuration Release
```

The verification build regenerates without tests, confirms every manifest-owned output
is tracked by Git, and requires no working-tree diff for those files.

Run the full-library inventory separately from the normal build:

```powershell
.\eng\inventory.ps1 -CatalogOnly
.\eng\inventory.ps1 -BatchSize 64
```

The fast command catalogs every public entry header. The semantic command parses
deterministic batches, isolates individual failing headers, and writes
`artifacts/generator-reports/full-inventory.json`. An incomplete scan writes its partial
report but exits non-zero so automation cannot mistake it for a full denominator.

Create and verify the package from the committed runtime without a native rebuild with:

```powershell
.\eng\pack.ps1 -SkipBuild
.\eng\verify-package.ps1 -SkipBuild
```

The package verifier first audits all 14 local packages: the 13 managed packages must
contain zero native DLLs and `OcctSharp.Native.win-x64` must contain exactly 62. It then
publishes and executes both the `OcctSharp` compatibility consumer and a direct
`OcctSharp.Modeling` consumer. The direct consumer must not receive `OcctSharp.dll`.
Both load the shared `occt/` runtime and execute topology/ABI checks. See
[NuGet packaging](NUGET_PACKAGING.md) for the exact layout.

The current file-exchange bridge also copies the transitive OCCT Data Exchange,
Application Framework, Visualization, and required third-party DLL closure. See
[Console samples](SAMPLES.md) for the interactive menu workflow after the build.

## Current implementation scope

- Native ABI and runtime identity queries.
- Native exception containment and thread-local UTF-8 diagnostics.
- Opaque owned topology shape handle with native live-registration and stale-handle rejection.
- Experimental `SharedTransient` and checked `SharedTransientDerived` wrappers
  preserving OCCT intrusive `Handle(Standard_Transient)` clone, release, RTTI
  identity, and validated cast semantics.
- Generated `GeomCartesianPoint` typed shared wrapper preserving one intrusive OCCT
  handle per native wrapper, with generated constructors, value/member operations,
  clone, RTTI, registry validation, and disposal.
- Generated `TopoDS_Shape` value wrapper semantics for null/kind/orientation, clone,
  reversal, and partner/same/equal comparisons, partitioned under the Topology module.
- Generated checked typed topology casts for the eight configured `TopoDS_*` wrappers,
  with ABI TypeMismatch handling and source-disposal independence tests.
- OCCT box construction and face enumeration through the C ABI.
- Managed `LibraryImport`, `SafeHandle`, ABI validation, `Shape`, and `ShapeFactory`.
- Clang-based semantic discovery into a deterministic canonical declaration model.
- Generated native/managed `gp_Pnt` value-copy constructors, 20 `Precision` methods,
  three `TopAbs` enum methods, and five additional ownership-neutral scalar methods with
  a committed generated-file manifest and manifest-owned stale cleanup.
- General value-copy eligibility classification for simple constructors and static
  methods; source emission is intentionally limited to configured scopes and excludes
  ownership-bearing or side-effect-sensitive APIs.
- Deterministic package/toolkit coverage and per-declaration diagnostics reports under
  ignored `artifacts/generator-reports/`.
- Separate catalog and batched semantic inventory for the complete OCCT public-header
  surface; it is intentionally excluded from normal build latency.
- Physical managed module/facade NuGet layout with one shared native runtime package,
  automatic application-local loading from `occt`, and compatibility plus direct-module
  clean consumers.
- Geometry and metadata-aware STEP/IGES read, import, compose, round-trip, and export;
  OCAF/XDE documents, stable parent-bound labels, transactions, history, persistence,
  assemblies, PMI, scene/mesh exchange, and the Windows HWND viewer.
- Fourteen OCCT-aligned Preview.16 packages: 12 managed modules, the compatibility/facade
  package, and one shared native package containing the manifest-verified 62-DLL runtime
  plus 11 third-party notice/license files. Local SBOM, provenance, checksums, isolation,
  and clean facade/direct-module consumers pass.

The generated surface remains deliberately selective rather than full OCCT coverage,
and unknown ownership still fails closed. Hosted full release execution, package signing,
Preview.16 NuGet publication/indexing, and a public-source consumer are not implemented
or run; local package creation and the complete local release evidence pipeline are.

## Build principles

- A clean checkout must not depend on an unrecorded machine-wide OCCT installation.
- Repository-native bootstrap must use a manifest-validated SDK or an immutable
  HTTPS archive plus SHA256 and must never recursively invoke the managed build.
- Dependency resolution, generation, native build, managed build, tests, packaging,
  and package-consumer validation are distinct stages.
- Build output and downloaded dependencies stay under ignored inner-workspace paths.
- Generated output is produced in staging and validated before replacement.
- Release artifacts are produced by CI from a tagged, reviewable source state.
- Local and CI builds use the same versioned presets or configuration inputs.

## Release pipeline stages

1. Validate dependency and toolchain locks.
2. Acquire or build the pinned OCCT baseline.
3. Configure the AST parsing environment.
4. Run or verify deterministic binding generation.
5. Configure and build the native bridge.
6. Build managed raw bindings and friendly SDK.
7. Run the required test layers for the target matrix.
8. Inspect native exports and runtime dependency closure.
9. Create packages in an isolated staging directory.
10. Restore packages into a clean consumer project and run smoke/integration tests.
11. Produce provenance, checksums, reports, notices, and release metadata.

## Package requirements

A released package set must make these relationships unambiguous:

- Managed SDK version.
- Required native ABI major/minor version.
- Supported OCCT build identity.
- Supported runtime identifiers and architectures.
- Complete native runtime dependency closure or explicit external-runtime contract.
- Third-party license and notice content.

ADR-0008 selects the application-local runtime contract. ADR-0015 stages modularity;
ADR-0074 now implements 12 managed modules, one compatibility/facade package, and one
`OcctSharp.Native.win-x64` package. Every managed package converges on that same native
package and contains no copied native assets.

## Release gates

No public release is ready unless all applicable gates have current evidence:

- Reproducible clean generation.
- No unreviewed generated diff.
- Native and managed builds pass for the declared matrix.
- ABI, runtime, lifetime, integration, real-file, and packaging tests meet the release
  policy.
- Compatibility and upgrade reports are updated.
- Native dependencies are inspected on each platform.
- License, notice, SBOM/provenance, checksum, and signing requirements are satisfied.
- A clean consumer can restore and run without a developer OCCT installation unless
  the release explicitly documents that unsupported model.

## Version and release records

Each release should preserve:

- Source commit and tag.
- Dependency and toolchain lock identities.
- Generator/configuration/model schema versions.
- Native ABI and OCCT identities.
- Generated API manifest and coverage summary.
- Compatibility matrix and upgrade report.
- Package hashes and native dependency inspection results.
- Known issues and migration notes.

## Current status

Native configure/build, managed restore/build, deterministic model/discovery runs, and
tests are implemented in `eng/build.ps1`. Local package creation and clean-consumer
restore/publish/runtime validation are implemented. `eng/release-check.ps1` additionally
runs Release and Debug, generated freshness, a fresh full inventory, clean-source
regeneration, the 606-signature API compatibility diff, SBOM/provenance/gate generation,
fixed-order SHA256 checksums, and Git whitespace validation.

The root CI workflow has a dependency-free generator job and a complete Windows job that
runs this same entry point after acquiring an archive from configured immutable URL and
SHA256 variables. The `8.0.1-preview.9` local release check covers Release/Debug,
Generator 91/91, Runtime 147/147, the inherited real STEP/XDE plus real-HWND Batch D-K
workflows, and the complete Batch L occurrence/bounds/interference/clearance/incremental/review workflow,
the clean 62-DLL package consumer, deterministic generation/regeneration, inventory, API
compatibility, runtime hashes, SBOM/provenance/checksums, and Git whitespace. MIT project
licensing and bundled third-party notices pass. Hosted full release execution, package
signing, and NuGet publication are `NOT RUN`; therefore local implementation may be
complete while public release readiness remains false. Preview.10 additionally checks
the managed module graph, 3,233 facade forwarders, aggregate API compatibility, 14-package
asset isolation, and a direct Modeling-package consumer.

Preview.13 additionally validates Batch N IGESCAF/XDE metadata read/import/write,
format-neutral routing, Unicode-path staging and cleanup, mixed STEP/IGES composition,
round-trip, lifetime, and real-HWND display. Release and Debug pass Generator 91/91 and
Runtime 156/156; focused Batch N is 4/4; 94-file clean regeneration, 14-package isolation,
clean facade/direct-module consumers, full inventory, API, SBOM, provenance, checksums,
and Git whitespace pass.

```powershell
cd OcctSharp
.\eng\release-check.ps1 -PackageVersion 8.0.1-preview.16
```

Release evidence is written below `OcctSharp/artifacts/release/`: `api-diff.json`,
`sbom.cdx.json`, `provenance.json`, `release-gates.json`, and `checksums.sha256`.

Preview.14 adds Batch O's complete sketch/planar-feature, STEP/IGES, and viewer consumer.
Preview.16 additionally validates Batch Q's source-bound repair, transactional shared-
definition publication, protected metadata, STEP/IGES and real-viewer review workflow.
The release-check command builds Release and Debug, verifies the committed runtime,
regenerates a clean source copy, checks the 14 local packages with both consumers, and
generates inventory/API/SBOM/provenance/checksum evidence. STATUS records the final
run results. Completing a batch requires a local commit; NuGet upload and GitHub push
are not part of the batch workflow.

When final documentation changes package contents after the code gates pass, rerun
`verify-package.ps1 -SkipBuild`, then `generate-release-metadata.ps1` and
`update-release-checksums.ps1` for that exact package version. The checksum updater
requires matching package, provenance, and gate identities and does not alter gate states.

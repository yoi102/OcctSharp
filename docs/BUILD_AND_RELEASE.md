# Build and Release

This document defines the build and release process. Local packaging and reproducible
B20 release evidence are implemented; public publication remains externally gated.

## Validated baseline

- Managed target: .NET 10 only (`net10.0`, SDK 10.0.400).
- Platform: Windows x64.
- Native compiler: Visual Studio 2026/MSVC 19.51.
- OCCT: 8.0.1 combined VC14 x64 distribution with Debug and Release libraries.
- AST: ClangSharp 21.1.8.4 with libClangSharp 21.1.8.2.

## Local configuration

Copy `OcctSharp/config/local.settings.example.json` to
`OcctSharp/config/local.settings.json` and set:

- `occtRoot` to the directory containing `inc/`, `cmake/`, and `win64/`.
- `visualStudioRoot` to the Visual Studio installation containing CMake and C++ tools.

`local.settings.json` is ignored because it contains machine-specific absolute paths.
The committed `OcctSharp/config/occt-8.0.1-windows-x64.json` records the expected
distribution layout and verification hashes without local paths.

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

Create and verify the experimental NuGet package with:

```powershell
.\eng\pack.ps1
.\eng\verify-package.ps1 -SkipBuild
```

The package verifier restores the new local package into a package-only consumer,
publishes it, confirms all 36 native runtime DLLs are below `occt`, then loads OCCT and
executes box, generated typed-handle, and generated topology-value checks. See
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
- Experimental single-package NuGet layout with automatic application-local native
  loading from `occt` and a clean package consumer.
- Geometry-only STEP read/write, transformed compound assembly, metadata-preserving
  one-shot STEPCAF/XDE assembly, STL export with meshing, and BRep-mode IGES export.

General native and managed binding emitters, full type mapping, broad ownership
inference, general XDE/OCAF document APIs, and public release are not implemented yet.
Local package creation is implemented, but complete notices, provenance, CI production,
signing, and public publication are not.

## Build principles

- A clean checkout must not depend on an unrecorded machine-wide OCCT installation.
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

ADR-0008 selects one package containing the single current Windows x64 runtime.
ADR-0015 keeps that layout through topology/basic modeling, then permits the documented
managed module and RID-package split when size, optional-dependency, multi-RID, or
independent-release triggers are met.

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
SHA256 variables. Hosted CI execution is `NOT RUN`. Project licensing and non-OCCT
third-party legal review are `BLOCKED`; package signing and NuGet publication are
`NOT RUN`. Therefore local B20 implementation is complete while public release readiness
is false.

```powershell
cd OcctSharp
.\eng\release-check.ps1
```

Release evidence is written below `OcctSharp/artifacts/release/`: `api-diff.json`,
`sbom.cdx.json`, `provenance.json`, `release-gates.json`, and `checksums.sha256`.

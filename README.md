# OcctSharp

OcctSharp is a generator and .NET SDK for Open CASCADE Technology (OCCT).
Its purpose is to make OCCT bindings reproducible, reviewable, testable, and easier
to regenerate when the upstream OCCT version changes.

The repository now contains a .NET 10/Windows x64 foundation: a CMake native bridge,
managed API, Clang-based generator discovery, deterministic reports, and OCCT-backed
runtime tests against the first OCCT 8.0.1 baseline. An experimental local NuGet package
also delivers the complete native runtime below an application's `occt` directory.
Generation also produces deterministic package coverage and per-declaration diagnostics
for review during binding and OCCT upgrade work. A separate batched inventory audits the
entire public OCCT header surface without slowing the normal build. The verified Windows
x64 runtime is committed, so examples do not require a separate OCCT SDK.

## Clone and run

On Windows x64 with .NET SDK 10.0.400:

```powershell
git clone https://github.com/yoi102/OcctSharp.git
cd OcctSharp\OcctSharp
dotnet run --project .\samples\OcctSharp.Samples -- --smoke
```

The command verifies ABI 1.45, bridge 0.53.0, OCCT 8.0.1, all 62 application-local
DLLs, and exercises topology and detailed-mesh inspection on a six-face OCCT box. Run without `--smoke` for the interactive sample
menu. No OCCT installation, CMake, Visual Studio C++ workload, environment variable, or
private settings file is required for these examples.

## Repository boundary

- `docs/` contains project architecture, decisions, plans, and status documents.
- `OcctSharp/` is reserved for all solution, source, test, benchmark, configuration,
  generated-output, report, and packaging files.
- The Git repository belongs at this repository root so both areas are versioned
  together.

See [the documentation index](docs/DOCUMENTATION_INDEX.md),
[current status](docs/STATUS.md), [roadmap](docs/ROADMAP.md), and
[build instructions](docs/BUILD_AND_RELEASE.md). The runnable workflows are listed in
[console samples](docs/SAMPLES.md), and package layout is described in
[NuGet packaging](docs/NUGET_PACKAGING.md).

## Current state

- Batch B and Batch C are locally complete. Batch C closes four cross-family waves:
  common solid inspection, import-diagnose-repair, XDE property/occurrence/STEP options,
  and the final selective STEP/geometry/topology/viewer-input workflow. Generated/API
  coverage and public publication authority remain separate facts.
- Managed target is .NET 10; the validated baseline is Windows x64 and OCCT 8.0.1.
- ClangSharp semantic discovery, deterministic generation, native C ABI, friendly managed
  owners/values, geometry and metadata exchange, OCAF/XDE, and Windows HWND visualization
  are implemented for their declared profiles.
- Full inventory classifies 116,272 discovered declarations and all 7,090 catalogued
  headers with zero `SupportedUnselected`, zero broad LT001-LT004 reasons, and 16,353
  generated plus 102 accepted manual stable IDs. Narrow blocked dispositions are not
  claimed as managed APIs.
- The .NET 10 console sample contains seven separate workflows, including native BREP,
  topology/tolerance inspection, STEP diagnostics/repair, detailed mesh transfer, XDE
  validation properties/recursive occurrences, explicit STEPCAF options, and Viewer controls.
- Package version `0.1.0-alpha.54` carries the committed 62-DLL ABI 1.45 runtime and complete
  license/notice layout below application-local `occt/` and `licenses/`.
- An ordinary clone runs the Sample directly from the SHA256-pinned committed runtime.
  The SDK/CMake bootstrap remains available only as an explicit contributor override.
- The release pipeline records a 606-signature API baseline, clean regeneration,
  CycloneDX SBOM, provenance, checksums, release gates, and CI configuration.
- Project code is MIT licensed. OCCT and third-party runtime terms are preserved beside
  the bundled DLLs; hosted CI, signing, and NuGet publication remain separate gates.
- Architecture and behavior are recorded through ADR-0063 and SC-039. Advanced filters,
  custom rendering, optional integrations, cold schema, and exhaustive mesh attributes
  are outside the completed Batch C denominator.

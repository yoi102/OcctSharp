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
entire public OCCT header surface without slowing the normal build.

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

- B00-B18 are complete; B19 binding completion and B20 release completion remain open.
  Current batch progress is 19 of 21 (90.5%).
- Managed target is .NET 10; the validated baseline is Windows x64 and OCCT 8.0.1.
- ClangSharp semantic discovery, deterministic generation, native C ABI, friendly managed
  owners/values, geometry and metadata exchange, OCAF/XDE, and Windows HWND visualization
  are implemented for their declared profiles.
- Full inventory classifies 116,214 discovered declarations and all 7,090 catalogued
  headers. B19.1 raises actual generated coverage to 171 of 3,406 selected declarations
  (5.0206%); 10,338 full-inventory declarations remain `SupportedUnselected`.
  Classification completeness must not be read as binding coverage.
- The .NET 10 console sample contains six separate workflows: entity creation,
  STEP/STL/IGES export, transformed metadata-preserving XDE STEP assembly, and Viewer.
- Local package `OcctSharp.0.1.0-alpha.39.nupkg` restores and publishes into a clean
  consumer with 45 native DLLs under application-local `occt/`, including generated
  StepBasic shared-entity and enum behavior.
- The release pipeline records a 606-signature API baseline, clean regeneration,
  CycloneDX SBOM, provenance, checksums, release gates, and CI configuration.
- Public release is blocked until the project license and remaining third-party notices
  are resolved; hosted CI, signing, and NuGet publication have not been run.
- Architecture and behavior are recorded through ADR-0050.

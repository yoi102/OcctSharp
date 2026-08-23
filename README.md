# OcctSharp

OcctSharp is a planned generator and .NET SDK for Open CASCADE Technology (OCCT).
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

- Architecture baseline: documented with twenty-one accepted ADRs.
- Managed target: .NET 10 only.
- Initial platform: Windows x64.
- OCCT baseline: 8.0.1 combined VC14 x64 Debug/Release distribution.
- Parser: ClangSharp/libClangSharp semantic AST discovery.
- Native/managed builds, deterministic discovery, and selected runtime/lifetime tests:
  passed in Debug and Release on the recorded local environment.
- Experimental `SharedTransient` API preserves OCCT `Handle(Standard_Transient)` clone,
  null, reference-count, release, RTTI checks, and one checked derived cast without
  exposing native object layout.
- The first generated real typed shared wrapper, `GeomCartesianPoint`, preserves OCCT
  intrusive sharing and exposes generated coordinate/value behavior.
- Generated `TopoDS_Shape` value semantics now expose `Shape` null/kind/orientation,
  copy, reversal, and OCCT partner/same/equal comparisons without crossing C++ layout.
- Generated checked typed topology casts now expose `Compound`, `CompSolid`, `Solid`,
  `Shell`, `Face`, `Wire`, `Edge`, and `Vertex` wrappers.
- B05 transformation values now include opaque `GpTrsf`, `TopLocLocation`, `GpVec`,
  `GpDir`, `GpAx1`, and `GpMat` owners with transform conversion and topology placement.
- The first B06 foundation wave adds UTF-8/UTF-16 OCCT string owners and a mutable
  `NCollection_Sequence<double>` wrapper with explicit buffer and index rules.
- The second B06 foundation wave adds `NCollection_Array1<double>` and the OCCT 8
  dynamic-array-backed `NCollection_Vector<double>` wrapper with explicit bound/index rules.
- The current B06 map wave adds integer-key `NCollection_DataMap` and ordered
  `NCollection_IndexedMap` wrappers with clone and mutation contracts.
- One .NET 10 console project demonstrates box creation, STEP/STL/IGES export, and
  transformed metadata-preserving XDE STEP assembly.
- The local `OcctSharp.0.1.0-alpha.14.nupkg` passes package-only restore, publish,
  application-local native loading, runtime identity, typed shared-wrapper checks, and
  generated topology value checks.

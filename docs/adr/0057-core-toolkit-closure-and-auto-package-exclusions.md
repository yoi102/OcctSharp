# ADR-0057: Core Toolkit Closure and Automatic Package Exclusions

- Status: Accepted
- Date: 2026-08-25

## Context

The 16,017-binding full-selection wave compiled as C++ but failed to link with 454
unresolved symbols. Two causes were mixed together: the native target linked only a
subset of the OCCT components already selected by `find_package`, while whole Draw/test
and IVtk package families had entered automatic static/shared-handle generation even
though ADR-0047 excludes those dependencies from the core package.

## Decision

- The native target links and deploys one explicit `OCCTSHARP_CORE_TOOLKITS` list covering
  the selected FoundationClasses, ModelingData, ModelingAlgorithms,
  ApplicationFramework, DataExchange, and supported Windows visualization closure.
- The core list must not use `${OpenCASCADE_LIBRARIES}` because that aggregate includes
  IVtk and OpenGL ES optional profiles. Draw components are not requested or linked.
- `excludedAutoPackages` assigns whole discovered source packages a stable reason code,
  category, and detail. The exclusion is applied before automatic scope expansion and is
  also supplied to full-inventory classification.
- `SK009 / TestHarness` covers OCCT Draw/command/test packages. `SK010 /
  OptionalExternalDependency` covers IVtk packages until an isolated, pinned VTK profile
  exists.
- Exact header and stable-ID exclusions remain separate mechanisms for parse blockers or
  artifact-specific missing symbols; whole-package exclusion is not used for ordinary
  linkable core APIs.

## Alternatives considered

- Linking every OpenCASCADE component was rejected because it would silently add VTK,
  EGL/GLES, Draw, and Tcl/Tk dependencies to the core package.
- Maintaining generated-source deletions was rejected because regeneration would restore
  them and coverage accounting would become false.
- Expanding only an emitter deny list was rejected because declarations would disappear
  from generation without a stable inventory disposition.
- Enumerating thousands of stable IDs was rejected because source-package identity is the
  stable architectural boundary for these profiles.

## Consequences

- Link and NuGet runtime-copy closure now share the same toolkit list and cannot drift by
  simple omission.
- Optional/test declarations remain visible in discovery and inventory but cannot enter
  core generated output.
- Future isolated optional packages may remove their package exclusions only after their
  external SDK, ownership, build, runtime, and packaging evidence exists.

## Validation required

- Unit tests for package exclusion and full-inventory disposition.
- Generator regeneration proving excluded package stable IDs leave the manifest.
- Release and Debug native/managed builds with no Draw, IVtk, or OpenGL ES linkage.
- Runtime dependency audit and clean NuGet consumer proving the explicit core closure is
  complete below `occt/`.

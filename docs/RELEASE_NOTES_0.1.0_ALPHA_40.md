# OcctSharp 0.1.0-alpha.40 Local Release Notes

## Scope

This experimental .NET 10/Windows x64 package advances the native ABI to 1.32 and the
bridge version to 0.40.0 against the pinned OCCT 8.0.1 baseline. It is a locally
validated artifact and is not authorized for public NuGet publication.

## Generated binding expansion

- Generation configuration schema 1.5 adds deterministic header patterns and
  package-level shared-handle scope expansion.
- The StepBasic generated surface grows from ten to 129 public shared-entity types.
  Only discovered `Standard_Transient` descendants with a supported public default
  constructor are expanded; unknown lifetime and projection cases remain classified.
- The committed 13-file generated manifest owns 333 stable IDs in a selected scope of
  5,503 declarations, up from 171 of 3,406 in alpha.39.
- Full-inventory classification records 333 emitted, 10,177 supported-unselected,
  27,310 skipped, and 78,394 blocked declarations. B19 and B20 therefore remain open.

## Repository Sample bootstrap

- All Sample source and user-visible text is English.
- A fresh configured clone can build or run the Sample project without first invoking
  the complete `eng/build.ps1` pipeline. Incremental MSBuild calls the native-only
  `eng/ensure-native.ps1` entry point when the native artifact is missing or stale.
- The bootstrap accepts a manifest-validated local OCCT SDK or an immutable HTTPS
  archive plus SHA256, builds the requested Debug/Release CMake bridge, and copies the
  45-DLL closure below Sample output `occt/`.
- Git clone alone does not include the OCCT SDK. A developer must set
  `OCCTSHARP_OCCT_ROOT`, create ignored `config/local.settings.json`, or configure the
  approved archive URL and SHA256 pair. NuGet consumers still need no SDK.

## Validation evidence

- Release and Debug Generator tests: 41/41 in both configurations.
- Release and Debug Runtime/lifetime tests: 75/75 in both configurations.
- Generated freshness: 13 manifest-owned files with no generated diff.
- Clean package consumer: 45 DLLs below `occt/`, ABI 1.32, bridge 0.40.0, OCCT 8.0.1,
  and construction/clone/reference-count/disposal checks for all 129 StepBasic types.
- Full inventory: 116,214 declarations and 7,090 headers classified with zero pending
  and zero HD099; 7,058 headers parse semantically and 32 have named dependency/artifact
  dispositions.
- Managed API compatibility against the 606-signature alpha.38 baseline: 1,132 additions,
  zero removals, and no breaking removal gate.
- Missing-native Sample simulation: the Debug bridge was moved out of the expected
  path, a normal Sample build recreated the 45-DLL runtime, and the English entity
  workflow created a six-face box.
- Git working/staged whitespace and local Markdown-link checks pass.

## Remaining gates

- B19 is not complete: 10,177 declarations remain `SupportedUnselected` and broad
  LT001-LT004 projection/ownership work remains.
- B19.3 will prioritize common modeling, topology traversal/transforms,
  fillet/chamfer/offset, STEP/IGES/STL/XDE, mesh, and visualization APIs before more
  low-value STEP data entities.
- Project license selection and non-OCCT third-party legal review remain blocked.
- Hosted CI execution, package signing, and public NuGet publication are not run.

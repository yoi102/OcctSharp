# OcctSharp 0.1.0-alpha.39 Local Release Notes

This is a locally validated prerelease candidate for .NET 10 and Windows x64. It has not
been signed or published.

## Added in this build

- Typed managed enum emission from Clang enum definitions, including explicit values,
  nested/qualified names, and verified Int32 ABI range checks.
- Ten generated StepBasic intrusive shared-handle entities covering address, person,
  dimensions, SI units, and date/time scalar state.
- Manifest-aware full-inventory reconciliation with `Emitted/EM001` dispositions.
- Runtime and clean-consumer coverage for scalar/boolean/enum round-trips, retained
  clones, RTTI, reference counts, disposal, and disposed-use rejection.

Generated coverage is 171 of 3,406 selected declarations. The full inventory records
171 emitted, 10,338 supported-unselected, 27,310 skipped, and 78,395 blocked declarations
across the 116,214 declarations discovered from 7,058 successfully parsed headers.

Native ABI is 1.31, bridge version is 0.39.0, and the package contains the unchanged
45 native DLLs under application-local `occt/`.

## Validation summary

Release and Debug builds pass Generator 40/40 and Runtime 73/73. Generated freshness
passes for 13 manifest-owned files. The alpha.39 clean package consumer restores,
publishes, loads the 45-DLL closure, and executes generated StepBasic shared/enum behavior.

## Publication blockers

- B19 is not complete: 10,338 declarations remain `SupportedUnselected` and broad
  LT001-LT004 projection/ownership blockers remain.
- The user has not selected a project license (PD-012).
- Exact versions, notices, and redistribution/source obligations for non-OCCT native DLLs
  need legal/provenance review.
- Hosted full CI, package signing, and NuGet publication were not run.

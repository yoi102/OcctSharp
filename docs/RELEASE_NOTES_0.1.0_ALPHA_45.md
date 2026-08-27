# OcctSharp 0.1.0-alpha.45

## Scope

This experimental .NET 10 / Windows x64 package continues the single product-scale
migration batch `B`. It adds complete package-level selection for the common STEP
geometry, representation, shape, and visual data-model families.

## Identity

- Package: `OcctSharp.0.1.0-alpha.45.nupkg`.
- OCCT: 8.0.1 VC14 x64 combined artifact.
- Native ABI: 1.37.
- Bridge implementation: 0.45.0.
- Managed target: `net10.0`, Windows x64.

## Generator and API expansion

- Added `StepGeom`, `StepRepr`, `StepShape`, and `StepVisual` header-pattern and
  package-level shared-handle generation.
- Added 366 concrete public generated types: 85 StepGeom, 79 StepRepr, 92 StepShape,
  and 110 StepVisual types.
- The manifest grows from 775 to 1,594 emitted stable IDs. Selected discovery grows from
  16,633 to 22,879 declarations.
- Generated wrappers retain one OCCT intrusive reference and expose safe constructor,
  scalar, enum, point-copy, RTTI, clone, and disposal operations where mapped.

## Validation

- Release and Debug: Generator 44/44 and Runtime 98/98.
- Clean NuGet consumer: 47 DLLs below `occt/`; ABI 1.37, bridge 0.45.0, OCCT 8.0.1;
  representatives from all four new STEP package families execute successfully.
- Generated freshness: 13 manifest-owned files current.
- Clean-source regeneration: 13 byte-identical generated files.
- API baseline comparison: 5,251 additions, zero removals, non-breaking.
- Full inventory: 1,594 emitted, 61 manual, 8,934 supported-unselected, 27,310 skipped,
  and 78,315 blocked across 116,214 classified declarations.

## Remaining gates

- Batch `B` remains in progress while 8,934 declarations are supported but unselected
  and LT001-LT004 projection/ownership work remains.
- Hosted CI, signing, and NuGet publication are `NOT RUN`; project licensing and
  third-party legal review remain blocked. Public upload requires explicit authority.

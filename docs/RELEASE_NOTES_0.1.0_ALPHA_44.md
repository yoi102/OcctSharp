# OcctSharp 0.1.0-alpha.44

## Scope

This experimental .NET 10 / Windows x64 package continues the single product-scale
migration batch `B`. It expands the generated shared-owner surface across common mesh,
analysis, and healing packages; this workstream is not a separate numbered batch or a
batch commit boundary.

## Identity

- Package: `OcctSharp.0.1.0-alpha.44.nupkg`.
- OCCT: 8.0.1 VC14 x64 combined artifact.
- Native ABI: 1.36.
- Bridge implementation: 0.44.0.
- Managed target: `net10.0`, Windows x64.

## Generator and API expansion

- Binding-model schema 1.2 records Clang's abstract-record fact. Package expansion now
  rejects abstract records before emission even if a public constructor is visible.
- Header-pattern and package-level selection now include `BRepMesh`, `Poly`,
  `ShapeAnalysis`, `ShapeFix`, and `ShapeUpgrade`.
- Sixty-one generated public types are added: 14 BRepMesh, six Poly, four ShapeAnalysis,
  13 ShapeFix, and 24 ShapeUpgrade types.
- The manifest grows from 400 to 775 emitted stable IDs. Selected discovery grows from
  12,633 to 16,633 declarations.
- The new types reuse the verified intrusive shared-owner contract: type-specific live
  registries, retained clones, RTTI/reference-count inspection, idempotent disposal,
  exception containment, and scalar/enum/value-copy member projections.

## Coverage and validation

- Selected emitted coverage: 775/16,633 (4.6594%).
- Selected emitted plus accepted manual coverage: 836/16,633 (5.0262%).
- Selected safe support: 1,767/16,633 (10.6235%).
- Full inventory: 775 emitted, 61 manual, 9,738 supported-unselected, 27,310 skipped,
  and 78,330 blocked across 116,214 classified declarations.
- Release and Debug: Generator 44/44, Runtime 96/96, dependency profiles 6/6.
- Clean NuGet consumer: 47 DLLs below `occt/`; ABI 1.36, bridge 0.44.0, OCCT 8.0.1;
  representatives from every new package family execute successfully.
- HEAD-based freshness passes after temporarily staging only the six intended generated
  changes; 13 manifest-owned files regenerate without a worktree difference.
- The complete release check passes twice. Clean-source regeneration produces 13
  byte-identical generated files, and the alpha.38 public API baseline comparison reports
  2,160 additions, zero removals, and no breaking change.
- Release metadata generation and both Git whitespace checks pass. Temporary staging was
  removed after the HEAD-based freshness gate; no files remain staged.
- Full-inventory SHA256:
  `556A1C3DC664AE44DE2CAF716BB980F93373BBB4D70326A4FC1F09A7CEC0FB9D`.

## Remaining gates

- 9,738 declarations remain `SupportedUnselected`; LT001-LT004 projection and ownership
  work remains inside batch B.
- Hosted CI, signing, and NuGet publication are `NOT RUN`; project licensing and
  third-party legal review remain blocked. Public upload requires explicit authority.

# OcctSharp 0.1.0-alpha.43

## Scope

This experimental .NET 10 / Windows x64 package continues the single product-scale
migration batch B by generalizing package-level shared-handle generation to common
`Geom` and `Geom2d` types. It is locally validated but not approved for publication.

## Identity

- Package: `OcctSharp.0.1.0-alpha.43.nupkg`.
- OCCT: 8.0.1 VC14 x64 combined artifact.
- Native ABI: 1.35.
- Bridge implementation: 0.43.0.
- Managed target: `net10.0`, Windows x64.

## Generated expansion

- Header-pattern discovery now includes `Geom_*.hxx` and `Geom2d_*.hxx`.
- Package-level selection reuses the verified intrusive shared-handle generator for
  `Geom` and `Geom2d`; no manually authored per-type raw binding was added.
- Eight new public generated types cover 2D Cartesian points, 2D/3D directions,
  2D/3D vectors with magnitude, 2D/3D transformations, and planes.
- Supported generated members cover copied scalars, enums, and points for coordinates,
  magnitude/normalization, plane evaluation/reversal, and transformation inspection and
  mutation. Clone/reference-count/RTTI/disposal behavior is generated for every type.
- The manifest grows from 333 to 400 emitted stable IDs. Selected discovery grows from
  10,956 to 12,633 declarations.

## Coverage and validation

- Selected emitted coverage: 400/12,633 (3.1663%).
- Selected emitted plus accepted manual coverage: 461/12,633 (3.6492%).
- Selected safe support: 1,346/12,633 (10.6546%).
- Full inventory: 400 emitted, 61 manual, 10,110 supported-unselected, 27,310 skipped,
  and 78,333 blocked across 116,214 classified declarations.
- Release and Debug: Generator 44/44, Runtime 93/93, dependency profiles 6/6.
- Clean NuGet consumer: 47 DLLs below `occt/`; ABI 1.35, bridge 0.43.0, OCCT 8.0.1;
  generated Geom/Geom2d and all earlier public workflows execute successfully.
- Deterministic repeated generation and the complete local release check pass. The
  HEAD-based freshness gate passed by temporarily staging only the six intended changed
  generated files; that staging was removed after the check because batch B is not at
  its commit boundary.
- Clean source regeneration produced 13 byte-identical files. API comparison against the
  alpha.38 baseline reports 1,387 additions, zero removals, and no breaking change.

## Remaining gates

- 10,110 bindable declarations remain `SupportedUnselected`; LT001-LT004 general
  projection/ownership work remains.
- Hosted CI, signing, and NuGet publication are `NOT RUN`; third-party legal review is
  blocked. Public upload requires explicit authority.

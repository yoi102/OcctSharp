# OcctSharp 0.1.0-alpha.46

This local experimental package advances generated shared-handle relationships inside
the single migration batch B. It is not a public-release-readiness declaration.

## Identity

- Package: `OcctSharp.0.1.0-alpha.46.nupkg`.
- Target: .NET 10, Windows x64.
- OCCT baseline: 8.0.1.
- Native ABI: 1.38.
- Bridge implementation: 0.46.0.

## Additions

- Generated nullable parameters and returns between already selected
  `opencascade::handle<T>` wrapper types.
- Target-specific native registry validation for non-null inputs.
- Independent retained wrappers for non-null returned handles.
- Stable managed null mapping for null OCCT handles.
- Focused generator, runtime lifetime, and clean-package-consumer coverage.
- 2,235 emitted stable IDs, up from 1,594 in alpha.45.

## Coverage boundary

- Selected scope: 22,879 declarations.
- Emitted: 2,235 (9.7688%).
- Emitted plus 61 accepted manual declarations: 2,296 (10.0354%).
- Full inventory classification: 116,214 declarations and 7,090 headers classified;
  12,890 declarations remain `SupportedUnselected` and 73,718 remain blocked.
- Batch B remains in progress; this release does not satisfy its full binding gate.

## Validation

Release-check evidence is recorded in `STATUS.md`. Public publication, signing, hosted
CI execution, project licensing, and final third-party legal review remain outside this
local package validation.

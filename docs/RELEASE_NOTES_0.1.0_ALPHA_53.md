# OcctSharp 0.1.0-alpha.53

Alpha.53 is the third Batch C cross-family checkpoint. It keeps OCCT 8.0.1 and .NET 10,
advances the native ABI to 1.44 and bridge to 0.52.0, and ships the matching Windows x64
runtime in the repository and package.

## Added

- `XdeLabel.ValidationProperties` and `UpdateValidationPropertiesFromShape` for copied,
  optional XCAF area, volume, and centroid attributes backed by BRepGProp computation.
- `XdeLabel.GetOccurrences` and owning `XdeOccurrence` snapshots for direct or recursive
  assembly flattening, composed world locations, stable entry paths, and independent
  located shapes.
- `XdeStepReadOptions` and `XdeStepWriteOptions` for common STEPCAF name, color, layer,
  validation-property, and material modes plus STEP model representation on export.
- Runtime, sample, and clean package-consumer coverage for nested assembly placement,
  property mutation/clearing, BinXCAF and STEPCAF round trips, and metadata filtering.

## Compatibility

The managed and native changes are additive. The no-option STEPCAF APIs retain their
previous all-metadata defaults. Native consumers must use the bundled ABI 1.44 bridge;
the runtime manifest rejects the older alpha.52 bridge.

## Local evidence

- Release and Debug native/managed builds: 0 warnings, 0 errors.
- Generator 62/62 and Runtime 108/108 pass in both configurations.
- Full classification: 16,353 emitted, 85 manual, 49,344 skipped, 50,490 blocked,
  zero supported-unselected and zero pending declarations/headers.
- API compatibility against alpha.38: 36,883 additions, 0 removals, no breaking change.
- Hosted full release, signing, and NuGet publication remain `NOT RUN`.

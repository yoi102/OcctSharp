# OcctSharp 0.1.0-alpha.51

Alpha.51 is the first Batch C common-CAD-API large-wave checkpoint. It keeps OCCT 8.0.1
and .NET 10, advances the native ABI to 1.42 and bridge to 0.50.0, and ships the matching
Windows x64 runtime in the repository and package.

## Added

- Native BREP read/write through `ShapeExchange`.
- Whole-shape unique/occurrence topology counts, closedness, validity, and common
  tolerance ranges.
- Detailed mesh snapshots with transformed OCCT node normals, optional UVs,
  orientation-correct winding, and triangle-to-face mapping.
- `XdeDocument.AddPart` for common name/color/layer/material metadata.
- Viewer presentation color/transparency/display mode, standard Z-up projections,
  zoom/pan, and replace/add/remove/toggle/clear selection.
- One end-to-end runtime/package workflow and an interactive sample menu entry.

## Compatibility

The change is additive at the managed API level. Native consumers must use the bundled
ABI 1.42 bridge; the runtime manifest rejects the older alpha.50 bridge.

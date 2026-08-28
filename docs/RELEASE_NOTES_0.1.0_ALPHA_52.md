# OcctSharp 0.1.0-alpha.52

Alpha.52 is the second Batch C cross-family checkpoint. It keeps OCCT 8.0.1 and .NET 10,
advances the native ABI to 1.43 and bridge to 0.51.0, and ships the matching Windows x64
runtime in the repository and package.

## Added

- `ShapeExchange.ReadStepWithReport` with typed read status, root/shape transfer counts,
  and the copied STEP system length unit.
- `Shape.GetValidationReport` with copied per-subshape `BRepCheck` issue statuses and
  optional geometry/exact checking.
- `Shape.RepairWithReport` with an independently owned `ShapeFix` result and immutable
  validation snapshots from before and after repair.
- `OcctViewer.StartRotation` and `Rotate` for owner-thread-affine mouse rotation.
- Runtime, clean package-consumer, and common-workflow sample coverage across STEPControl,
  BRepCheck, ShapeFix, and V3d.

## Compatibility

The managed and native changes are additive. Native consumers must use the bundled ABI
1.43 bridge; the runtime manifest rejects the older alpha.51 bridge.

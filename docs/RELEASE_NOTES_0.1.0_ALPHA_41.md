# OcctSharp 0.1.0-alpha.41

Date: 2026-08-24

## Added

- `ShapeFactory.CreateCone` and `ShapeFactory.CreateTorus` owning solid builders.
- `Shape.Extrude` and `Shape.Revolve` owning modeling operations.
- All-edge and single-edge `Shape.Fillet` and `Shape.Chamfer` operations.
- `Shape.Offset` and shape-to-shape `Shape.Section` operations.
- `Shape.GetBoundingBox`, `Shape.IsValid`, and public `Shape.CountSubShapes` value APIs.
- Configuration schema 1.6 manual stable-ID validation and full-inventory
  `Manual/MN001` reconciliation for SC-032.

## Native and package identity

- Native ABI: 1.33.
- Bridge implementation: 0.41.0.
- OCCT baseline: 8.0.1 VC14 x64 combined Debug/Release distribution.
- Managed target: .NET 10, Windows x64.
- Package: `OcctSharp.0.1.0-alpha.41.nupkg`.
- Application-local runtime: 47 DLLs under `occt/`, adding `TKFillet` and `TKOffset`.

## Validation

- Release and Debug: Generator 44/44 and Runtime 81/81.
- Selected discovery: 9,567 declarations, zero diagnostics, 18/18 manual IDs found.
- Full inventory: 7,058/7,090 headers semantically scanned; 116,214 declarations and
  7,090 headers fully classified; 333 emitted, 18 manual, 10,177 supported-unselected,
  27,310 skipped, and 78,376 blocked.
- Clean package consumer: restore, publish, 47-DLL application-local load, ABI/bridge
  identity, all 129 generated StepBasic types, common modeling operations, and prior
  package profiles passed.
- Generated/report determinism passed in both configurations. Publication, signing, and
  hosted CI execution remain not run.

## Compatibility and limitations

This release is additive within ABI major 1. Algorithm builders, history, progress,
contour state, per-face offset controls, and analyzer objects remain native-local.
The 18 SC-032 declarations are accepted manual bindings, not generated declarations.
The package remains experimental and is not approved for public publication.

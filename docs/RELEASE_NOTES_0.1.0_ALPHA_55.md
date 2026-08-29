# OcctSharp 0.1.0-alpha.55

Alpha.55 completes the finite 24-capability Batch D production CAD viewport and
model-review closure. It is additive over alpha.54 and retains one managed assembly,
one native bridge, one NuGet package, and the application-local 62-DLL runtime layout.

## Added

- Copied XDE occurrence identity on viewer presentations and selection/detection results.
- Exact detected whole/subshape topology as an independent owning `Shape`.
- Rectangle and polygon selection with replace/add/remove/toggle schemes, bounded pixel
  tolerance, and built-in topology-kind filters.
- Copied selection bounds, fit-selected behavior, and reversible presentation isolation.
- `AIS_ColoredShape` per-subshape color, transparency, and width review overrides with
  individual and complete reset.
- Copied camera snapshot/restore, screen/world conversion, normalized pick rays, and
  client-rectangle window zoom.
- Linear-RGB background, parent-bound clip-plane lifecycle, computed hidden-line mode,
  orientation trihedron configuration, and durable RGB/RGBA/depth screenshot files.
- A real STEP/XDE-to-review-to-screenshot workflow in runtime tests and the clean package
  consumer, including source-document independence and Unicode output paths.

## Compatibility and ownership

- Native ABI: 1.46.
- Bridge implementation: 0.54.0.
- OCCT baseline: 8.0.1.
- Target: .NET 10, Windows x64.
- Presentations, filters, clip planes, AIS/V3d/Graphic3d state, and the HWND-bound owner
  remain viewer-parent-bound and creation-thread-affine.
- XDE identity, camera/coordinate/bounds/plane/color values are copied. Detected and
  selected topology are independent registered owners. Screenshots return only durable
  file paths; no native image storage crosses the ABI.

## Validation

- Release and Debug native/managed builds, Generator 91/91, Runtime 115/115, dependency
  profiles 6/6, deterministic generated freshness/regeneration, clean alpha.55 package
  consumption with 62 DLLs, complete inventory classification, API compatibility,
  runtime manifest, SBOM/provenance/checksums, Git whitespace, and the full local release
  gate pass together.
- API comparison against alpha.38 is additive at 37,125 additions, zero removals, and no
  breaking changes. Final inventory classification is 16,353 emitted, 120 manual,
  49,344 skipped, 50,455 blocked, and zero supported-unselected declarations.

Hosted full release execution, signing, credentials, and NuGet publication remain
separate and are not authorized by this local Batch D completion.

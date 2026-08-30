# OcctSharp 8.0.1-preview.3

This preview completes Batch F as one 24-capability freeform curve/surface and profile-
to-solid authoring wave. It retains one managed assembly, one native DLL, one package,
and managed assembly/file identity 0.1.0.0.

## Version identity

- NuGet package: `OcctSharp` `8.0.1-preview.3`.
- OCCT baseline: 8.0.1 VC14 x64 combined runtime.
- Native ABI: 1.48.
- Bridge: 0.56.0.
- Managed assembly identity: 0.1.0.0.
- Target: .NET 10, Windows x64.

## Included implementation

- Immutable copied rational and non-rational Bezier/B-spline curve and surface
  definitions, complete snapshots, validated edits, elevation, reverse, trim, and split.
- Interpolation, approximation, projection, extrema, and curve/surface intersection with
  copied diagnostics and multi-solution records.
- Located planar profiles, profile offsets, ruled/fill/freeform-offset construction,
  split/history diagnostics, controlled pipe shells and lofts, sewing, healing, and
  validation with independently owning topology results.
- Complete STEP/XDE-to-mesh/measurement-to-real-HWND selection and screenshot workflow.
- SC-042 reconciliation for exactly 94 directly used blocked OCCT 8.0.1 stable IDs; the
  1,122-declaration focused root audit was not bulk-marked manual.

## Local validation

- Release and Debug native/managed builds pass with zero errors, Generator 91/91,
  Runtime 123/123, and dependency profiles 6/6.
- Four focused Batch F completion tests pass.
- The clean package consumer restores, publishes, and runs the full authored freeform
  STEP/XDE, mesh, measurement, real-HWND selection, and screenshot workflow with 62 DLLs.
- All 83 generated files are fresh and byte-identical after clean regeneration; the
  generated shard graph has 27 resolved, target-compatible edges and no cycles.
- Full inventory: 116,272 declarations and 7,090 headers classified; 16,353 emitted,
  316 manual, 49,344 skipped, 50,259 blocked, zero supported-unselected/pending/HD099.
- API comparison against alpha.38 is additive at 37,636 additions and zero removals.
- The native bridge is 15,120,384 bytes with SHA256
  `B36051D6E1B9E8E5A5BD8BED9D06EF0C44D59DB83A9A2195BF9082827CDE7075`.
- Full inventory SHA256 is
  `8130A50973213311E2E4705CAEA750002B27CFD6D9574DD4E98C81F76F758F39`.
- The nupkg SHA256 is
  `43954141FB8CA19CBF176065F3D8E6B38F0DBD7E4923D4A8F76DF5E21D41E40C`.
- SBOM, provenance, fixed-order checksums, Git whitespace, and the complete Preview.3
  local release check pass.

Hosted full release execution, signing, and NuGet publication remain separate `NOT RUN`
release-readiness gates. This package is not uploaded by the local completion workflow.

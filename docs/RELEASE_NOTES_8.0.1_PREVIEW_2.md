# OcctSharp 8.0.1-preview.2

This preview completes Batch E as one 24-capability engineering-inspection, exact-
measurement, semantic PMI/AP242, saved-view, viewer-annotation, and screenshot wave.
It retains one managed assembly, one native DLL, one package, and managed assembly/file
identity 0.1.0.0.

## Version identity

- NuGet package: `OcctSharp` `8.0.1-preview.2`.
- OCCT baseline: 8.0.1 VC14 x64 combined runtime.
- Native ABI: 1.47.
- Bridge: 0.55.0.
- Managed assembly identity: 0.1.0.0.
- Target: .NET 10, Windows x64.

## Included implementation

- Complete exact-distance solutions, owning support topology, contact/interference
  classification, and length/area/volume/centroid/inertia/angle/radius/diameter snapshots.
- Explicit inspection units without implicit global display state.
- Stable document-parent-bound dimension, tolerance, datum, target, and saved-view APIs
  with complete copied snapshots, bidirectional reference graphs, transactional create/
  update/replace/detach/remove, rollback, cross-document guards, and persistence.
- Explicit STEP AP242 GDT and saved-view read/write controls.
- Viewer-parent-bound length, angle, radius, and diameter dimensions with lifecycle,
  styling, selection, update, and real-HWND screenshot evidence.
- SC-041 reconciliation for exactly 102 directly used blocked OCCT 8.0.1 stable IDs,
  including tolerance-datum detach and the OCCT 8.0.1 datum-point X correction.

## Local validation

- Release and Debug native/managed builds pass with zero warnings/errors, Generator
  91/91, Runtime 119/119, and dependency profiles 6/6.
- Four focused Batch E completion tests pass.
- The clean package consumer restores, publishes, and runs the full Batch E AP242/
  BinXCAF/saved-view/viewer-dimension/screenshot workflow with 62 DLLs.
- All 83 generated files are fresh and byte-identical after clean regeneration.
- Full inventory: 116,272 declarations and 7,090 headers classified; 16,353 emitted,
  222 manual, 49,344 skipped, 50,353 blocked, zero supported-unselected/pending/HD099.
- API comparison against alpha.38 is additive at 37,490 additions and zero removals.
- The native bridge is 15,044,096 bytes with SHA256
  `4A5B67B886146E704E5A31F25CDB87C75CC351EB496854D77F0435D0142B22B1`.
- Full inventory SHA256 is
  `2C8DE4940EAB609C5B24BCE45B50A473BF4120004DD337247E0172C0D1CAC3B1`.
- SBOM, provenance, fixed-order checksums, Git whitespace, and the complete Preview.2
  local release check pass.

Hosted full release execution, signing, and NuGet publication remain separate `NOT RUN`
release-readiness gates. This package is not uploaded by the local completion workflow.

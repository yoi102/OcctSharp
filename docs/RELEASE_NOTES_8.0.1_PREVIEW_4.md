# OcctSharp 8.0.1-preview.4

This preview completes Batch G as one 24-capability technical-drawing, hidden-line,
section, and vector-output wave. It retains one managed assembly, one native DLL, one
package, and managed assembly/file identity 0.1.0.0.

## Version identity

- NuGet package: `OcctSharp` `8.0.1-preview.4`.
- OCCT baseline: 8.0.1 VC14 x64 combined runtime.
- Native ABI: 1.49.
- Bridge: 0.57.0.
- Managed assembly identity: 0.1.0.0.
- Target: .NET 10, Windows x64.

## Included implementation

- Exact and polygonal hidden-line removal with validated copied orthographic and
  perspective projectors over one or many owning shapes.
- Ten independently owning visible/hidden sharp, smooth, sewn, outline, and
  isoparameter topology layers plus independently owning planar sections.
- Bounded count/copy transfer of projected edge polylines with preserved boundaries and
  closed flags; no HLR graph, explorer, adaptor, curve, or native vector escapes the ABI.
- Managed layered SVG with fitted extents and configurable colors, widths, hidden dashes,
  isoparameter inclusion, and background, plus front/top/right/isometric standard views.
- Complete STEP/XDE-to-drawing/SVG-to-real-HWND screenshot workflow in repository runtime
  and the clean package consumer.
- SC-043 reconciliation for exactly 33 directly used blocked OCCT 8.0.1 stable IDs; the
  1,069-declaration focused root audit was not bulk-marked manual.

## Local validation

- Release and Debug native/managed builds pass with zero warnings and zero errors;
  Generator 91/91, Runtime 127/127, focused Batch G 4/4, and dependency profiles 6/6.
- The clean package consumer restores, publishes, and runs the full HLR/section/SVG/
  STEP-XDE/real-HWND screenshot workflow with 62 DLLs.
- All 83 generated files are fresh and byte-identical after clean regeneration; the
  generated shard graph has 27 resolved, target-compatible edges and no cycles.
- Full inventory: 116,272 declarations and 7,090 headers classified; 16,353 emitted,
  349 manual, 49,344 skipped, 50,226 blocked, zero supported-unselected/pending/HD099.
- API comparison against alpha.38 is additive at 37,731 additions and zero removals.
- The native bridge is 15,139,840 bytes with SHA256
  `725C014D637E3100619A4F626B4AE6F626E3D0162761F85124ABB8C8E563FE14`.
- Full inventory SHA256 is
  `78BDED2909920DD037D99608604E85EBC87BDD5FD144FAAC247C88D69DE1A318`.
- The nupkg SHA256 is
  `44C59BE285F5388C468D2FBFFD7D17649A51AC53A0AD50FD0E6D10333EB71C0D`.
- SBOM, provenance, fixed-order checksums, documentation links, Git whitespace, and the
  complete Preview.4 local release check pass.

Hosted full release execution, signing, and NuGet publication remain separate `NOT RUN`
release-readiness gates. This package is not uploaded by the local completion workflow.

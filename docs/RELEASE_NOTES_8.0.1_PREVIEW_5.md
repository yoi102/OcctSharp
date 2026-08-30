# OcctSharp 8.0.1-preview.5

This preview completes Batch H as one 24-capability advanced-mesh, scene, material,
LOD, and interchange wave. It retains one managed assembly, one native DLL, one package,
and managed assembly/file identity 0.1.0.0.

## Version identity

- NuGet package: `OcctSharp` `8.0.1-preview.5`.
- OCCT baseline: 8.0.1 VC14 x64 combined runtime.
- Native ABI: 1.50.
- Bridge: 0.58.0.
- Managed assembly identity: 0.1.0.0.
- Target: .NET 10, Windows x64.

## Included implementation

- Independent configurable BRep meshing with copied transformed positions, unit normals,
  optional UVs, oriented triangle winding, and immutable source-face groups.
- Copied bounds, counts, surface area, memory estimates, degenerate/boundary/manifold/
  non-manifold edges, connected components, and ordered fine-to-coarse LOD snapshots.
- XDE metallic-roughness PBR set/get plus copied color, physical material, names, layers,
  local/world 3x4 transforms, hierarchy, paths, deduplicated definitions, and instances.
- Document-aware glTF/GLB and OBJ import/export plus PLY and VRML export. XDE BRep roots
  are triangulated before provider export so authored documents retain real geometry.
- Complete STEP/XDE-to-scene/LOD/interchange-to-real-HWND screenshot workflow in
  repository runtime and the clean package consumer.
- SC-044 reconciliation for exactly 24 directly used blocked OCCT 8.0.1 stable IDs; the
  840-declaration focused root audit was not bulk-marked manual.

## Local validation

- Release and Debug native/managed builds pass with zero warnings and zero errors;
  Generator 91/91, Runtime 131/131, focused Batch H 4/4, and dependency profiles 6/6.
- Runtime integration tests are serialized because OCCT viewer state is creating-thread
  affine and not safe for concurrent real-HWND test fixtures.
- The clean package consumer restores, publishes, and runs the full grouped-mesh/PBR/
  scene/LOD/glTF-GLB-OBJ-PLY-VRML/STEP-XDE/real-HWND workflow with 62 DLLs.
- All 83 generated files are fresh and byte-identical after clean regeneration; the
  generated shard graph has 27 resolved, target-compatible edges and no cycles.
- Full inventory: 116,272 declarations and 7,090 headers classified; 16,353 emitted,
  373 manual, 49,344 skipped, 50,202 blocked, zero supported-unselected/pending/HD099.
- API comparison against alpha.38 is additive at 37,904 additions and zero removals.
- The native bridge is 15,159,808 bytes with SHA256
  `26432903E96CA6AA981078596ADEE3A5866AE94DD93E71354E54D2638208A1BB`.
- Full inventory SHA256 is
  `75BD35320CA769AA54FEE0B09F17A15A1560352B0CDACEA6BEEFBDD8494AD695`.
- The final nupkg SHA256 is recorded in the generated release checksum artifact; it is
  not embedded here because this release note is itself packaged.
- SBOM, provenance, fixed-order checksums, Git whitespace, and the complete Preview.5
  local release check pass.

Hosted full release execution, signing, NuGet publication, and GitHub work remain
separate `NOT RUN` release-readiness gates. This package is not uploaded by the local
completion workflow.

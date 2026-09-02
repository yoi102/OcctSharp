# OcctSharp 8.0.1-preview.13 release notes

## Summary

Preview.13 completes Batch N's metadata-aware IGES/XDE interoperability wave. STEP and
IGES now share one XDE-centered read/import/write API, non-ASCII Windows exchange paths
are handled through cleanup-safe staging, and the WPF viewer displays IGES through the
same XDE-label style path as STEP. The additive native ABI advances to 1.57 and bridge to
0.65.0 while OCCT 8.0.1, schema 1.13, managed assembly identity `0.1.0.0`, the physical
managed module graph, and one shared 62-DLL Windows x64 runtime package remain unchanged.

## Highlights

- `XdeDocument.ReadIges`, `ImportIges`, and `WriteIges` expose IGESCAF/XDE workflows
  with names, generic/surface/curve colors, layers, visibility, diagnostics, and units.
- `ReadExchange`, `ImportExchange`, and `WriteExchange` route STEP or IGES from an
  explicit `XdeExchangeFormat` or a supported extension without a second object model.
- `XdeIgesReadOptions`, `XdeIgesWriteOptions`, and `XdeIgesReadReport` expose independent
  metadata modes and copied status/count/unit/diagnostic values.
- Non-ASCII Windows input paths are copied to unique ASCII staging files. Non-ASCII
  outputs are written to staging and promoted only after native success; success,
  failure, and exception paths clean temporary files.
- IGESCAF transfer output is cloned into a `TDocStd_Application`-owned XDE document before
  managed labels are exposed. Imported labels remain destination-parent-bound and layer
  references are explicitly copied, so source reader/session disposal is safe.
- `OcctSharpViewer.Wpf` routes both STEP and IGES through owned XDE documents and
  `Display(XdeLabel)`, preserving supported presentation metadata instead of using the
  former neutral-only IGES geometry path.
- The clean facade package consumer now executes the inherited Batch D-N workflow,
  including Unicode IGES, metadata, mixed STEP/IGES composition, round-trip, lifetime,
  and real-HWND evidence.

## Compatibility and ownership

The release is additive: the compatibility package, 12 module package IDs, namespaces,
and managed assembly/file versions remain unchanged. IGESCAF readers, writers, sessions,
models, maps, iterators, and diagnostics remain native-local. Managed code receives only
owned XDE documents, document-parent-bound labels, independently registered topology,
and copied metadata/diagnostic values. Temporary staging is internal and does not change
the public path reported to callers.

## Validation

- Release and Debug build all 19 projects with zero code warnings/errors; Generator
  91/91, Runtime 156/156, focused Batch N 4/4, and dependency profiles 6/6 pass.
- Real IGES/XDE metadata, option modes, diagnostics/units, Unicode read/write/failure
  cleanup, mixed STEP/IGES composition, round-trip, source/session disposal, and a real
  HWND workflow execute in the repository runtime and clean facade consumer.
- All 94 generated files are current and byte-identical after clean-source regeneration.
- API comparison against alpha.38 is additive at 38,838 additions and zero removals.
- Full inventory classifies all 116,272 declarations and 7,090 headers: 16,353 emitted,
  557 manual, 49,344 skipped, 50,018 narrowly blocked, and zero supported-unselected,
  pending, or HD099 entries. SC-051 reconciles exactly 15 directly used blocked IDs.
- The 14 packages pass README/icon/content isolation. The 13 managed packages contain
  one managed assembly and zero native DLLs each; the native package alone contains all
  62 DLLs and 11 notice/license files.
- The committed 15,356,928-byte `OcctSharp.Native.dll` is byte-identical to the Release
  rebuild with SHA256
  `7DD8EB7A3CF5EA975F45D2F84812FBB2521B0E35F87C500DF5A42E9FC64C9EAD`.
- SBOM, provenance, release-gate metadata, checksums, and Git whitespace checks pass.

Preview.13 was packaged and fully validated locally but was not uploaded. Hosted full
release execution, package signing, NuGet publication/indexing, and a public-source
consumer remain `NOT RUN`; they are not local implementation failures.

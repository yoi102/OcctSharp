# OcctSharp 8.0.1-preview.8 release notes

Preview.8 completes Batch K as one 24-capability assembly-authoring, occurrence,
graph/BOM, reference, effective-metadata, physical-rollup, history, STEP/XDE, viewer,
lifetime, and clean-package wave. It remains an experimental Windows x64 package for
.NET 10 and OCCT 8.0.1.

## Added

- Reusable definition replacement and explicit occurrence relocation, relinking,
  removal, reparenting, usage-policy definition removal, and metadata-preserving subtree
  clone.
- Stable occurrence-path resolution with independently owned located topology, direct and
  recursive where-used records, copied assembly graphs, structured and flattened BOMs,
  and deterministic structure diagnostics.
- Copied external path/URI metadata, XCAF assembly-item references, SHUO occurrence
  chains, and definition-fallback/occurrence-override effective metadata.
- Leaf-occurrence world-space bounds, mass, centroid, quantity, and definition-group
  rollups without assembly double counting.
- Focused named transaction, undo/redo/abort, real STEP/XDE, real HWND screenshot,
  source/document-disposal, and clean 62-DLL package-consumer evidence.

## Ownership and compatibility

XCAF/TDF tools, labels, attributes, maps, sequences, editors, SHUO graphs, and STEP
sessions stay document- or call-local. Managed labels remain parent-bound stable entries;
graph/BOM/reference/diagnostic values are copied; resolved topology and location values
are independently owned; presentations remain viewer/thread-bound. Package identity is
`8.0.1-preview.8`, native ABI is 1.53, bridge implementation is 0.61.0, configuration
schema is 1.11, and managed assembly/file identity remains `0.1.0.0`.

SC-047 reconciles exactly 24 directly used blocked OCCT 8.0.1 stable IDs. The other 586
blocked declarations in the Batch K preparation audit were not bulk-marked manual.

## Local validation

- Release and Debug native/managed builds pass with zero warnings and errors.
- Generator 91/91, Runtime 143/143, focused Batch K 4/4, and dependency profiles 6/6 pass.
- The clean package consumer restores, publishes, and runs with 62 DLLs, ABI 1.53,
  bridge 0.61.0, complete assembly/BOM/reference/history operations, real STEP/XDE, and
  real HWND screenshots.
- All 83 generated files are fresh and byte-identical after clean regeneration. Full
  classification closes 116,272 declarations and 7,090 headers with 16,353 emitted,
  524 manual, 49,344 skipped, 50,051 blocked, and no pending IDs; inventory SHA256 is
  `11BF0C50B56EBCF54F776366EB68BDDB93CACD5209DA9CBE93472FDD437A402B`.
- API comparison against alpha.38 is additive at 38,436 additions and zero removals. The
  15,290,880-byte bridge SHA256 is
  `2585B9CA96E7022914F6759F5E9CA863AC4D4140D8CB65DD049F8B3558619D2E`; the
  40,947,046-byte nupkg SHA256 is
  `F0BD9E01691AA8242E58DFBB0C5FA5E43A077CC9D7050DCB1245D56FE4065898`.
- The complete local release gate, including SBOM, provenance, and checksums, passes.

Hosted release execution, signing, NuGet publication, and GitHub work were not performed.

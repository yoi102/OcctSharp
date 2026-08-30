# OcctSharp 8.0.1-preview.6

This preview completes Batch I as one 24-capability document-state, attribute-graph,
history, undo/redo, savepoint, and persistence wave. It retains one managed assembly,
one native DLL, one package, and managed assembly/file identity 0.1.0.0.

## Version identity

- NuGet package: `OcctSharp` `8.0.1-preview.6`.
- OCCT baseline: 8.0.1 VC14 x64 combined runtime.
- Native ABI: 1.51.
- Bridge: 0.59.0.
- Managed assembly identity: 0.1.0.0.
- Configuration schema: 1.9.
- Target: .NET 10, Windows x64.

## Included implementation

- Copied stable label identity, deterministic child traversal, typed attribute metadata,
  Name/Comment/ASCII text, integer/real values, bounded arrays, references, reference
  arrays, application trees, and independent owning named topology.
- Immutable complete document snapshots plus managed outgoing/reverse dependency graphs,
  roots/leaves, Tarjan strongly connected components, cycle diagnostics, and acyclic
  topological ordering.
- Explicitly named document commands with commit/abort, zero/bounded/unlimited undo
  policy, copied undo/redo history metadata and changed labels, branching/clearing,
  dirty state, and explicit savepoints.
- Real BinOcaf, XmlOcaf, BinXCAF, and XmlXCAF save/open round trips integrated with
  STEP/XDE mutation, occurrence dependency edges, export, and source-disposal evidence.
- SC-045 reconciliation for exactly 54 directly used blocked OCCT 8.0.1 stable IDs; the
  676-declaration focused root audit was not bulk-marked manual.

## Local validation

- Release and Debug native/managed builds pass with zero warnings and zero errors;
  Generator 91/91, Runtime 135/135, focused Batch I 4/4, and dependency profiles 6/6.
- Focused tests cover abort rollback, attribute removal/empty arrays, owning topology,
  graph/SCC diagnostics, history branching, savepoints, all four persistence formats,
  XDE occurrence edges, STEP/XDE, and source/document disposal.
- The clean package consumer restores, publishes, and runs the complete four-format plus
  STEP/XDE Batch I workflow with 62 application-local DLLs.
- The committed runtime is byte-identical to the complete Release DLL closure. The native
  bridge is 15,216,128 bytes with SHA256
  `F8C7825FC770963068ADAE008FDAD95EC2C3155A2DAADA2CD3245DAFEAC76E00`.
- All 83 generated files are fresh and byte-identical after clean regeneration; the
  generated shard graph has 27 resolved, target-compatible edges and no cycles.
- Full inventory: 116,272 declarations and 7,090 headers classified; 16,353 emitted,
  427 manual, 49,344 skipped, 50,148 blocked, and zero supported-unselected/pending/HD099.
- API comparison against alpha.38 is additive at 38,128 additions and zero removals.
  Inventory SHA256 is
  `D8C4F6C1CC1F2AD378F5722DEB507E5F1C6E4AE62E153F07F6A5C65464307A64`.
- The final nupkg SHA256 is recorded in the generated release checksum artifact; it is
  not embedded here because this release note is itself packaged.
- SBOM, provenance, fixed-order checksums, Git whitespace, and the complete Preview.6
  local release check pass.

Hosted full release execution, signing, NuGet publication, and GitHub work remain
separate `NOT RUN` release-readiness gates. This package is not uploaded by the local
completion workflow.

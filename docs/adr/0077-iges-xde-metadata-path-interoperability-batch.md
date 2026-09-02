# ADR-0077: Implement metadata-aware IGES/XDE and path interoperability as Batch N

- Status: Accepted for implementation
- Date: 2026-09-02
- Scope: Batch N product denominator, IGESCAF/XDE exchange, path reliability, ownership, and validation

## Context

OcctSharp already provides geometry-only `IGESControl` read/write and metadata-aware
STEPCAF/XDE workflows. The WPF sample therefore displays STEP colors but routes IGES
through one owning shape with a neutral fallback. Names, colors, layers, and visibility
available through `IGESCAFControl` are not projected into the managed XDE model. The
selected OCCT file APIs also accept narrow `char*` paths, so Windows non-ASCII exchange
paths remain unvalidated under KI-009.

Adding only an IGES reader or a path workaround would leave the product workflow split.
The useful closure must include owned documents, composable import, metadata-aware export,
format-neutral routing, viewer display, round-trip evidence, and clean-package behavior.

## Decision

Open Batch N as the indivisible 24-capability wave in
`BATCH_N_IGES_XDE_INTEROPERABILITY_GAP_INVENTORY.md`.

Keep `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, work sessions, interface models,
maps, iterators, and transfer diagnostics native-local. Copy transfer status, counts,
diagnostic text, unit data, names, colors, layers, and visibility across the ABI. Return
only owned `XdeDocument` instances, document-parent-bound `XdeLabel` instances, existing
registered topology owners, and copied managed values.

Compose the IGESCAF path with the established XDE document and viewer boundaries. Import
transfers all eligible roots into an existing document and returns destination-parent-
bound labels. Export accepts independent name, color, and layer modes. Format-neutral
APIs route STEP or IGES without creating a second object model, and the WPF sample uses
the XDE-label display path for both formats.

Hide narrow-path limitations behind a managed internal staging boundary. ASCII paths use
the original full path. A non-ASCII input is copied to a unique ASCII temporary file; a
non-ASCII output is written to a unique ASCII temporary file and promoted only after a
successful native write. Cleanup runs for success, failure, and disposal, and public
diagnostics continue to identify the caller's path.

The additive wave targets package `8.0.1-preview.13`, native ABI 1.57, bridge 0.65.0,
and schema 1.13 unless a generator configuration or rule must change. It retains the
Preview.12 managed module graph and exactly one `OcctSharp.Native.dll`/62-DLL runtime
package.

## Alternatives considered

- Keeping IGES geometry-only was rejected because it cannot preserve or display authored
  XDE names, colors, layers, and visibility.
- Exposing generated IGESCAF/shared-handle wrappers was rejected because transfer
  sessions, models, and maps do not have a stable caller-owned lifetime across the ABI.
- Always staging every path was rejected because ordinary ASCII paths should preserve
  their current I/O behavior and diagnostics.
- Passing UTF-8 directly and documenting failure was rejected because it does not close
  KI-009 or provide deterministic Windows behavior.
- A separate IGES native or managed package was rejected because exchange shares the
  existing document, label, topology, and viewer ownership registries.

## Consequences

- STEP and IGES can share one XDE-centered managed workflow while retaining format-
  specific options where the formats differ.
- Temporary ASCII staging becomes an internal resource that requires collision-safe
  creation, atomic output promotion, and verified cleanup.
- IGES metadata fidelity is limited to what OCCT 8.0.1 `IGESCAFControl` transfers and
  writes; unsupported entity-level semantics are not inferred.
- Only exact directly called blocked declarations may enter SC-051. The 814 blocked root
  candidates are not bulk-classified as manual.
- Existing public/generated APIs and the physical module/native topology remain intact.
- Hosted release, signing, publication, GitHub, and push remain outside this decision.

## Validation required

Focused read/import/write/options/routing/metadata/Unicode/disposal tests; complete
Release and Debug Generator/Runtime suites; real colored/layered IGES and mixed STEP/IGES
assembly; real-HWND rendering; clean facade and direct-module package consumers; exact
SC-051 accounting; generation/freshness/compatibility/inventory/runtime/SBOM/provenance/
checksum gates; documentation synchronization; and `git diff --check`.

## Related decisions

- ADR-0002: fixed native C ABI.
- ADR-0037: geometry-only IGES read bridge.
- ADR-0045: parent-bound XDE metadata and assemblies.
- ADR-0046: HWND/thread-affine viewer ownership.
- ADR-0053: composable XDE STEP import.
- ADR-0059: committed Windows runtime.
- ADR-0065: OCCT-aligned preview versioning.
- ADR-0074: managed modules and one shared native package.
- ADR-0076: copied XDE presentation styles and XDE-label viewer display.

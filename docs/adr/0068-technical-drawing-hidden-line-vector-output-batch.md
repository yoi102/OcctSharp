# ADR-0068: Implement technical drawing, hidden-line removal, sections, and vector output as Batch G

- Status: Accepted and completed
- Date: 2026-08-30
- Scope: Batch G product denominator, dependency closure, ownership, and validation

## Context

Batch F closes freeform authoring, but an application still needs unmanaged OCCT code to
produce conventional projected drawing geometry. The missing workflow crosses HLRAlgo,
HLRBRep exact and polygonal algorithms, BRepMesh, BRepAlgoAPI sections, TopExp,
BRepAdaptor, STEP/XDE, AIS/V3d, and durable vector/image evidence.

## Decision

Open Batch G as one 24-capability product wave named **technical drawing, hidden-line
removal, sections, and vector output**. The immutable denominator and 24-root/
1,069-declaration audit are in `BATCH_G_TECHNICAL_DRAWING_GAP_INVENTORY.md`.

HLR and section algorithms remain native-local. Ten visible/hidden category results and
section results cross only as independent registered owning `Shape` values. Projected
edge geometry crosses through copied bounded polyline buffers. Managed SVG composition
owns its strings/files and receives no native iterator, curve, presentation, or image.

The batch retains one `OcctSharp.dll`, one `OcctSharp.Native.dll`, one NuGet package,
stable public type full names, and the accepted generated shard graph. Implementation
advances the package to Preview.4, native ABI to 1.49, and bridge to 0.57.0.

## Locked non-goals

Associative drawing-sheet persistence, dimensions/notes/BOM authoring, DXF/DWG,
custom rendering/callbacks, optional integration profiles, physical deliverable
splitting, hosted release, signing, publication, and GitHub work.

## Consequences

- Preparation freezes all 24 capabilities before implementation starts.
- Exact HLR, polygonal HLR, sections, polyline transfer, SVG, and standard views are not
  separate completion checkpoints.
- SC-043 records exactly 33 new directly used blocked declarations without changing
  generated output ownership.
- Prior Batch B-F evidence remains immutable.
- Preview.4 completes all 24 capabilities together without exposing an HLR algorithm,
  iterator, adaptor, borrowed topology, or native vector container.

## Validation required

Focused projection/category/section/SVG/lifetime tests, real STEP/XDE plus real HWND,
the same clean-package workflow, Release/Debug, generator/runtime suites, regeneration,
compatibility, inventory, runtime manifest, SBOM/provenance/checksums, and the complete
local release gate must pass together before G is complete.

This complete chain passed for Preview.4: Release/Debug, Generator 91/91, Runtime
127/127, focused 4/4, dependency profiles 6/6, real STEP/XDE plus real HWND, clean
62-DLL package consumption, 83-file freshness/clean regeneration, additive API,
complete inventory, runtime hashes, SBOM, provenance, checksums, and Git whitespace.

## Related decisions

- ADR-0046: viewer-parent and creating-thread ownership.
- ADR-0052: native-local algorithms and exact manual stable-ID accounting.
- ADR-0059: committed Windows runtime and MIT licensing.
- ADR-0061/0062: generated layering and cross-shard closure.
- ADR-0065: OCCT-aligned preview numbering.
- ADR-0067: completed freeform authoring boundary.

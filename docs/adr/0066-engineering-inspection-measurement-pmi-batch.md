# ADR-0066: Implement engineering inspection, measurement, and PMI as Batch E

- Status: Accepted
- Date: 2026-08-29
- Scope: Batch E product denominator, dependency closure, and ownership boundaries

## Context

Batch D closes production viewport review at alpha.55. The next common CAD gap is an
engineering-inspection workflow: exact measurement is only partially composed, semantic
PMI objects are generated without document traversal/reference ownership, STEPCAF GDT
round-tripping is not a friendly workflow, and generated `PrsDim` handles cannot safely
join the viewer-owned AIS graph.

These gaps cross BRepExtrema/BRepAdaptor/BRepGProp, TopoDS, XCAFDoc,
XCAFDimTolObjects, STEPCAF, XCAF saved views, PrsDim/AIS, V3d/Graphic3d, and image
evidence. Splitting them would leave either data without visualization or transient
measurements without persistent engineering meaning.

## Decision

Open Batch E as one 24-capability product wave named **engineering inspection, exact
measurement, and PMI**. The immutable denominator and 990-declaration root audit are in
`BATCH_E_INSPECTION_PMI_GAP_INVENTORY.md`.

Batch E is not a sequence of measurement, dimension, tolerance, datum, saved-view, or
viewer-annotation sub-batches. Focused tests are permitted while implementing, but only
the complete generated-real-AP242-file-to-inspection-screenshot workflow and its full
local gate can close E.

Ownership follows existing categories:

- solver/adaptor/property/STEPCAF objects remain native-local;
- measurement values and semantic PMI fields cross as immutable copied records;
- support, target, presentation, and overlap topology cross as independent owning
  registered shapes;
- PMI and saved-view label identities are parent-bound stable entries owned by
  `XdeDocument` and mutations require its transaction boundary;
- viewer dimensions are parent-bound IDs owned by the existing creating-thread-affine
  `OcctViewer`; generated `PrsDim` handles are not injected into that registry;
- saved views are copied camera/visibility/PMI/clipping state resolved against a live
  document and viewer at apply time.

The batch retains one managed assembly, one native DLL, one NuGet package, stable public
type full names, and the existing generated shard dependency graph.

## Locked non-goals

CMM devices, uncertainty analysis, automatic tolerance pass/fail solvers, arbitrary
markup authoring, custom rendering, native callbacks, optional integration profiles,
exhaustive STEP schema exposure, and physical deliverable splitting.

## Consequences

- Batch E preparation is complete at 0/24 before implementation starts.
- The generated AP242 fixture avoids making an unlicensed external PMI file a completion
  prerequisite.
- Existing scalar generated PMI APIs are inputs, not proof of document/reference/viewer
  workflow completion.
- Batch D evidence remains immutable and cannot be revised by Batch E progress.

## Validation required

The complete matrix in the inventory is required: numeric measurement fixtures,
ownership/disposal, OCAF commit/abort, cross-document and parent/viewer/thread rejection,
real AP242 export/reimport, real HWND dimension/view behavior, clean package execution,
Release/Debug, generation, inventory, compatibility, provenance, and local release
gates. No Batch E implementation validation is claimed by this preparation ADR.

## Related decisions

- ADR-0044/0045: document labels and XDE ownership.
- ADR-0046: HWND/thread-affine viewer ownership.
- ADR-0052: native-local operations and manual stable-ID accounting.
- ADR-0063: owning selected topology and application input.
- ADR-0064: copied review identity, clipping, and screenshot evidence.
- ADR-0065: OCCT-aligned NuGet preview versioning.

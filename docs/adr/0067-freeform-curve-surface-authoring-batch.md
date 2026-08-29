# ADR-0067: Implement freeform curve, surface, and profile-to-solid authoring as Batch F

- Status: Accepted
- Date: 2026-08-29
- Scope: Batch F product denominator, dependency closure, and ownership boundaries

## Context

Batch E closes engineering inspection and PMI in Preview.2. The next common authoring gap
is not another isolated primitive or viewer feature: applications can create several
basic edges and run basic loft/pipe/sew operations, while complete rational Bezier/
B-spline definitions, immutable editing, surface construction, profile offsets, filling,
splitting, controlled loft/pipe-shell behavior, and definition-preserving exchange remain
fragmented or unavailable.

These gaps cross Geom/Geom2d, GeomAPI/Geom2dAPI, BRepBuilderAPI, BRepOffsetAPI,
BRepFill/GeomFill, BRepAlgoAPI/BRepFeat, ShapeAnalysis/ShapeFix, TopoDS, STEPCAF/XDE,
AIS/V3d, mesh, and image evidence. A curve-only or surface-only checkpoint would not
prove a usable profile-to-solid workflow.

## Decision

Open Batch F as one 24-capability product wave named **freeform curve, surface, and
profile-to-solid authoring**. The immutable denominator and 24-root/1,122-declaration
audit are in `BATCH_F_FREEFORM_AUTHORING_GAP_INVENTORY.md`.

Batch F is not a sequence of curve, surface, profile, offset, fill, split, loft, sweep,
repair, exchange, or viewer sub-batches. Focused tests are permitted during implementation,
but only the complete copied-definition-to-owning-topology-to-real-STEP/XDE-to-real-HWND
workflow and full local gate can close F.

Ownership follows existing categories:

- curve/surface definitions and algorithm diagnostics are immutable managed copies;
- every point/pole/weight/knot/multiplicity/grid input is copied and validated;
- interpolation, approximation, projection, intersection, filling, offset, split, loft,
  pipe-shell, analysis, and repair objects remain native-local;
- topology results and multi-result pieces are independent registered owning shapes;
- XDE labels and viewer presentations retain their existing document-parent and
  creating-thread-affine viewer-parent identities;
- no generated `Geom` shared handle is used as undocumented mutable state for the
  friendly immutable authoring contract.

The batch retains one managed assembly, one native DLL, one NuGet package, stable public
type full names, and the accepted generated shard dependency graph. Preparation does not
change Preview.2, ABI 1.47, bridge 0.55.0, or the committed runtime.

## Locked non-goals

Parametric constraint solving, feature trees, Class-A optimization, subdivision surfaces,
reverse engineering, arbitrary drafting markup, custom rendering/callbacks, optional
integration profiles, exhaustive low-frequency fill algorithms, and physical deliverable
splitting.

## Consequences

- Batch F preparation is complete at 0/24 before implementation starts.
- Existing basic Bezier/interpolate/loft/pipe/sew APIs are inherited baseline inputs, not
  proof of complete definition/edit/topology/exchange workflow support.
- Direct blocked declarations used by implementation require exact SC-042 registration;
  the entire 1,122-declaration root audit cannot be marked manual.
- Batch E and Preview.2 evidence remain immutable and cannot be revised by F progress.

## Validation required

The complete matrix in the inventory is required: definition and array validation,
analytic numerics, immutable edit/source disposal, multi-result ownership, failure
diagnostics, real STEP/XDE type/topology retention, real-HWND selection/measurement/mesh/
screenshot behavior, clean package execution, Release/Debug, generation, inventory,
compatibility, provenance, and local release gates. No Batch F implementation validation
is claimed by this preparation ADR.

## Related decisions

- ADR-0039/0040: owning topology builders and copied adaptor snapshots.
- ADR-0042: owning algorithm results without native history leakage.
- ADR-0052: native-local operations and exact manual stable-ID accounting.
- ADR-0061/0062: generated module/layer partition and dependency closure.
- ADR-0063: final Batch C profile/topology/viewer ownership.
- ADR-0066: completed Batch E inspection/PMI boundary.

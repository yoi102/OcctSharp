# ADR-0040: BRep adaptor value snapshots

- Status: Accepted
- Date: 2026-08-23
- Scope: B08 adaptor completion on OCCT 8.0.1, Windows x64

## Decision

Keep `BRepAdaptor_Curve` and `BRepAdaptor_Surface` local to one native call. For an
edge, copy the OCCT curve type, finite first/last parameters, and evaluated endpoint
coordinates into a fixed C ABI structure. For a face, copy the OCCT surface type and
UV bounds into a second fixed structure, with an explicit restricted/unrestricted
selection. Managed code exposes immutable value snapshots.

The operation validates the live owning shape and exact topology kind before creating
the adaptor. No adaptor, underlying curve/surface, `TopoDS_*` reference, or borrowed
pointer crosses the ABI. A snapshot has value-copy ownership and remains valid after
the source shape is disposed.

## Validation

Native and managed layout assertions cover the 72-byte edge snapshot and 40-byte face
snapshot. Release and Debug runtime tests cover line endpoints and parameters, planar
surface type and bounds, wrong-kind rejection, access after source disposal, and
independence of previously copied values. The alpha.33 clean package consumer exercises
both snapshots with 36 application-local native DLLs.

## Upgrade impact

Re-check `GeomAbs_CurveType` and `GeomAbs_SurfaceType` numeric ordering, inherited
adaptor parameter methods, finite-bound behavior, structure layout, and toolkit linkage
for every OCCT/compiler upgrade.

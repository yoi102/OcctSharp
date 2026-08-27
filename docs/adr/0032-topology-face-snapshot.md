# ADR-0032: Owning topology face snapshots

- Status: Accepted
- Date: 2026-08-22
- Scope: topology traversal workstream inside B on OCCT 8.0.1, Windows x64

## Decision

Expose one-shot `TopoExp_Explorer` snapshots for face, edge, wire, and vertex kinds that
allocate independent registry-validated `TopoDS_Shape` copies. Managed
`Shape.GetFaces()`/`GetSubShapes()` owns every returned child and does not expose native
iterators, explorer state, or parent pointers. Snapshots are deterministic for OCCT
explorer order and remain valid after the source shape is disposed.

## Validation

Release/Debug runtime tests cover six faces, 24 edge occurrences, six wires, 48 vertex
occurrences, one-face child semantics, parent disposal, cleanup, and package alpha.24
loading with 36 native DLLs.

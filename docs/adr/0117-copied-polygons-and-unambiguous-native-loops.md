# ADR-0117: copied polygons and unambiguous native loops

Status: Accepted (2026-10-08).

Extend the exact copied workflow catalogue to initialized private polygons and
conservative native-only simple-cycle construction. An explicit index domain
binds the polygon to one owned triangulation copy; no topology provenance is
inferred. Native helper lifetimes are call-scoped and ambiguous branch choices
fail. Detached buffers and repeated native copies retain the established owned
registry/error and single-bridge contracts.

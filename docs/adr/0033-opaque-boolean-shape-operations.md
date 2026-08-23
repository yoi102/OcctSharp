# ADR-0033: Opaque boolean shape operations

- Status: Accepted
- Date: 2026-08-22
- Scope: B11/B12 first modeling and boolean sub-batch on OCCT 8.0.1, Windows x64

## Decision

Expose `BRepAlgoAPI_Fuse` and `BRepAlgoAPI_Cut` as status-returning native operations
over validated owning `Shape` handles. Builder state, history objects, and native
exception types stay inside the bridge; each result is a new registry-validated owner.
Inputs are never retained by the result and remain independently disposable.

## Validation

Release/Debug runtime tests cover overlapping transformed boxes, non-null result
topology, source disposal independence, and invalid lifetime paths. Alpha.25 package
consumer validates both operations with the application-local 36-DLL closure.

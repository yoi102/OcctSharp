# ADR-0031: Primitive solid builders remain opaque native constructors

- Status: Accepted
- Date: 2026-08-22
- Scope: BRep construction workstream inside B on OCCT 8.0.1, Windows x64

## Decision

Add application-facing `ShapeFactory.CreateSphere` and `CreateCylinder` operations
through status-returning C ABI exports backed by `BRepPrimAPI_MakeSphere` and
`BRepPrimAPI_MakeCylinder`. Native topology and builder state remain hidden; managed
callers receive normal owning `Shape` handles. Radius and height are finite and greater
than zero, and OCCT exceptions remain contained in the bridge.

## Validation

Release/Debug runtime tests and alpha.23 package consumer validate sphere/cylinder face
topology, invalid dimensions, native loading, and the existing shape lifetime suite.

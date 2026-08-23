# ADR-0015: Stage Managed and Package Modularity

- Status: Accepted
- Date: 2026-08-22

## Context

Full OCCT coverage will produce far more managed API than one maintainable assembly and
includes optional dependency boundaries such as VTK. Splitting immediately would make
the current single-RID package repeat the same 36 native DLLs and would force unstable
public project boundaries before topology ownership is established.

## Decision

- Keep the current `OcctSharp` managed project, native bridge, and NuGet package through
  topology and basic modeling foundation batches.
- Partition generated output by OCCT product module now, before physical project splits.
- Plan managed projects/packages for Runtime, Foundation, Modeling, Mesh, DataExchange,
  Xde, Visualization, and optional IVtk, with `OcctSharp` becoming a meta-package.
- Split when a documented size/build/dependency/RID trigger is met, not at an arbitrary
  declaration count alone.
- When multiple RIDs exist, move native assets to RID-specific packages. Do not split the
  native bridge by OCCT module until cross-DLL handle registration and creator-owned
  release have dedicated evidence.
- Keep the application output contract as one flat `occt` directory regardless of NuGet
  package boundaries.

## Alternatives considered

- Keeping one assembly forever was rejected because full OCCT and optional integrations
  would create excessive compile, IntelliSense, dependency, and consumer surface cost.
- Splitting every OCCT toolkit into a package was rejected because toolkit dependency
  edges are too fine-grained for normal .NET consumption and would create dozens of
  tightly coupled packages.
- Splitting native bridge DLLs now was rejected because handles allocated in one module
  must never be validated or released through another module's registry and allocator.

## Consequences

- Current consumers retain one package and one resolver while generator output gains
  stable future module ownership.
- Visualization/IVtk can later remain optional without imposing VTK on modeling users.
- A future physical split requires an API compatibility report and a package migration
  guide, but not a rewrite of generated module ownership.

## Validation

- The migration plan records module dependency direction and split triggers.
- Generated file manifests must retain deterministic ownership across module moves.
- Package splits require clean consumers for individual packages and the `OcctSharp`
  meta-package, with no duplicate native asset output.

# ADR-0062: Close generated shard dependencies before physical modularization

- Status: Accepted
- Date: 2026-08-28
- Scope: Generated signature dependency closure and the managed/native physical split decision

## Context

ADR-0061 gave every generated declaration and file a stable product module and API
layer, but it deliberately did not claim that the resulting shards were ready to become
separate managed projects or native DLLs. The first complete semantic audit of all
16,353 emitted declarations found eight dependency directions outside the target graph
and one strongly connected group spanning Foundation, Geometry, Modeling, and Mesh.

The cycle had structural causes. `Poly` value/data objects were grouped with meshing
algorithms even though BRep modeling signatures consume them. `FEmTool` and `Law` were
placed below the geometry APIs they use. `TopAbs_Orientation` is a cross-cutting value
contract used by geometry adaptors. Several valid document, exchange, visualization,
and XDE signatures also required dependencies that the first target graph omitted.

Directory layout or successful compilation inside one assembly cannot prove this
closure. The evidence must come from normalized emitted signatures and remain
deterministic as the binding set grows.

## Decision

Generation now produces `artifacts/generator-reports/dependency-closure.json`. For every
emitted declaration it resolves return, parameter, base, enum, `Handle<T>`, `gp_Pnt`,
and `TopoDS_Shape` projections to a product module. The report records direct edges,
transitive observed closure, target-graph compatibility, strongly connected groups,
and source stable IDs. `SD001` is a hard generation error for an emitted signature that
cannot be resolved. `SD002` records an observed edge outside the accepted graph.

The module model changes as follows:

- Add `MeshData` for `Poly` and copied triangulation/polygon contracts. It depends on
  Geometry and sits below Modeling.
- Modeling depends on Geometry and MeshData; Mesh contains meshing algorithms and
  depends on Modeling and MeshData.
- Place `FEmTool` and `Law` in Geometry.
- Lift the value-only `TopAbs_Orientation` enum into the Foundation contract. Other
  TopAbs, TopLoc, TopoDS, and BRep ownership remains Modeling.
- Record the actually required acyclic edges from Documents to MeshData, Visualization
  to Documents, DataExchange to Documents/Visualization, and Xde to Visualization.

The accepted generated-project graph is therefore acyclic. The current report covers
16,353 emitted declarations and 83 generated files, resolves 27 observed cross-shard
edges, and contains zero `SD001`, zero `SD002`, and zero cyclic groups.

Physical modularization is deliberately not performed in this decision:

- The generated managed graph is now eligible for a future project split, but moving
  public types changes assembly-qualified identity. Manual friendly APIs, type-forwarding
  policy, package references, and compatibility tests require a separate migration ADR.
- Native DLL splitting remains ineligible. Generated handles still use creator-owned
  registries, allocators, validation helpers, and release entry points inside one
  `OcctSharp.Native.dll`. A cross-DLL registry and creator-routed release design must be
  proven before any native binary split.
- Batch C continues on common CAD workflows with one `OcctSharp.dll`, one
  `OcctSharp.Native.dll`, one package, and unchanged public type full names.

## Alternatives considered

- Adding reverse dependencies until the old graph compiled was rejected because it
  preserves the Foundation/Geometry/Modeling/Mesh cycle and cannot support projects.
- Removing generated members that create reverse edges was rejected because it would
  trade architecture cleanliness for API loss.
- Moving all cyclic types into one high-level shard was rejected because it hides the
  real Poly/modeling data boundary and destroys stable product ownership.
- Splitting managed projects immediately was rejected because generated graph closure
  does not itself solve public assembly identity and manual-facade migration.
- Splitting native DLLs immediately was rejected because cross-DLL release through the
  wrong allocator or registry is a lifetime defect, not a packaging inconvenience.

## Consequences

- Every future generated API wave re-runs a fail-closed semantic dependency audit.
- `MeshData` becomes a stable source/project candidate and prevents Poly data from
  forcing a Modeling-to-Mesh algorithm dependency.
- Generated managed shards may be evaluated for a later physical split without first
  redesigning their dependency direction.
- Current consumers see no namespace, public type full-name, assembly, ABI, DLL, package,
  ownership, or runtime-layout change.

## Validation required

- Generator tests cover valid edges, reverse-edge cycles, unresolved handle targets,
  stable-ID evidence, and the `MeshData`/cross-cutting classification rules.
- The full selected model reports 16,353 emitted declarations, zero unresolved references,
  zero target-graph violations, and zero cyclic groups.
- Dependency-closure output is byte-stable across two generation runs and participates
  in release provenance.
- Release and Debug native/managed builds, runtime tests, freshness, clean regeneration,
  package consumer, API compatibility, inventory, and release gates pass without changing
  public identity.

## Related decisions

- ADR-0015: staged managed and package modularity.
- ADR-0055: generated operation namespaces and placement allocation.
- ADR-0057: core toolkit and runtime closure.
- ADR-0060: Batch C common-CAD-API product scope.
- ADR-0061: generated product-module and API-layer source partition.

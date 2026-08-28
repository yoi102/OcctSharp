# ADR-0061: Partition generated output by product module and API layer

- Status: Accepted
- Date: 2026-08-28
- Scope: Canonical module identity, generated file layout, and staged physical modularity

ADR-0062 refines the target graph with `MeshData`, closes all emitted signature edges,
and records the decision to retain one managed project and one native DLL for now.

## Context

The generated surface reached 16,353 stable binding IDs while native and managed output
remained concentrated in a small set of very large files. The largest translation units
mixed foundation, geometry, modeling, document, exchange, XDE, visualization, and
optional declarations. That obscured dependency direction, made generator ownership
harder to audit, and caused unrelated API families to recompile together.

ADR-0015 deliberately postponed project/package splitting until ownership and runtime
boundaries were mature enough. That postponement does not require generated source to
remain physically monolithic. A stable source partition is needed now, but changing
public type full names, assemblies, native allocation ownership, or DLL boundaries in
the same step would create unnecessary compatibility and lifetime risk.

## Decision

Assign every discovered declaration a fail-closed `OcctProductModule` in binding-model
schema 1.3. The stable modules are:

- `Runtime`
- `Foundation`
- `Geometry`
- `Modeling`
- `Mesh`
- `Documents`
- `DataExchange`
- `Xde`
- `Visualization`
- optional `IVtk`, `OpenGles`, and `Draw`

`Geometry` is separate from topology/BRep `Modeling`. OCAF/TDF/TDocStd persistence and
document infrastructure belong to `Documents`; XCAF/STEPCAF metadata and assemblies
belong to `Xde`. Package-prefix rules are evaluated before toolkit fallbacks, and an
emitted declaration with no assignment stops generation.

The target managed-project dependency graph is acyclic:

```text
Runtime <- Foundation <- Geometry <- Modeling <- Mesh
                                   |          |
                                   v          +------> DataExchange
                              Documents                  |
                                   |                     v
                                   +-------------------> Xde

Modeling + Mesh -----------------------------> Visualization
Visualization ------------------------------> IVtk / OpenGles
Xde + Visualization ------------------------> Draw
```

Generated paths are two-dimensional: product module directories plus an explicit API
layer/shard identity. Manifest schema 1.1 records `productModule`, `apiLayer`, and
`outputShard` for every file. Native and managed emitters produce module-local value,
enum, topology, shared-handle raw, and shared-handle friendly files. Common generated
native exception/helper contracts live in the `Runtime` shard. Cross-module shared
handle operations include dependency headers and use external helper linkage without
moving registries or allocators between binaries.

This target graph does not pretend that current intra-assembly source references are
already decoupled. Generated signatures still create audited cross-shard build edges,
including some Geometry/Modeling, Modeling/Mesh, Documents/DataExchange, and Xde/
Visualization relationships. They are valid while every shard compiles and links as one
assembly/DLL, but must be removed, lifted to a higher facade, or assigned to an explicit
shared contract before the corresponding projects can split. `OcctProductModuleGraph`
locks the desired public project direction; it is not yet an emitter authorization rule
for current translation-unit includes.

This decision does **not** split deliverables. All generated C# continues to compile
into the single `OcctSharp.dll`, and all generated C++ continues to link into the single
`OcctSharp.Native.dll`. Public generated types retain their existing `OcctSharp`
namespace and full names. The native handle registries, allocation/deallocation family,
ownership rules O001-O012, C ABI names, package ID, and runtime layout are unchanged.

## Alternatives considered

- Waiting until every API migration was complete was rejected because file ownership,
  dependency drift, and compile concentration worsen as the generated surface grows.
- Splitting managed projects and native DLLs immediately was rejected because it would
  combine source organization with public assembly identity, loader, registry, and
  creator-owned release changes.
- Mirroring OCCT toolkit DLLs one-for-one was rejected because toolkits are build/link
  units, not a stable .NET product model, and several common workflows cross them.
- Changing public namespaces to match directories was rejected because source layout
  does not justify breaking existing type full names.
- Hand-moving generated files was rejected because the manifest and emitters must remain
  the only owners of generated paths and stale cleanup.

## Consequences

- Generated diffs, coverage, and diagnostics can be reviewed by product module and API
  layer without changing consumer code.
- Native CMake recursively collects module translation units; adding a module shard no
  longer requires a central source-list edit.
- The previous centralized generated files are removed by manifest-owned stale cleanup.
- A future project/package split can use these stable module identities, but still
  requires cross-shard edge closure, an independent ADR, and an API/assembly
  compatibility plan.
- Optional modules remain visible and classified even when the active core profile does
  not emit them.

## Validation required

- Every emitted package has a non-`Unassigned` module, and representative package plus
  target managed dependency-graph tests pass.
- The generated manifest records module/layer/shard for every file and removes obsolete
  centralized paths.
- Release and Debug native/managed builds and runtime tests pass with one DLL per side.
- Generated freshness and a clean-source regeneration prove the complete file set is
  deterministic and byte-identical.
- Package-consumer and API-baseline checks prove unchanged public full names and zero
  removals.
- `STATUS.md`, `ARCHITECTURE.md`, and `MIGRATION_PLAN.md` record the staged boundary and
  do not claim that physical project/DLL modularization is complete.

## Related decisions

- ADR-0003: canonical binding model.
- ADR-0007: generated source ownership and raw/friendly separation.
- ADR-0015: staged managed package modularity.
- ADR-0055: generated operation namespaces and placement allocation.
- ADR-0056: generated translation-unit completion headers.

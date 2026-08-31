# ADR-0074: Split managed assemblies while retaining one native bridge and compatibility facade

- Status: Accepted and implemented
- Date: 2026-09-01
- Scope: Managed assembly identity, package graph, facade ownership, compatibility forwarding, and native runtime distribution

## Context

ADR-0015 staged managed modularity until package and runtime duplication could be
avoided. ADR-0061 assigned every generated declaration to a product module, and
ADR-0062 proved the complete 16,353-declaration generated signature graph is resolved,
acyclic, and eligible for managed project splitting. The DataExchange shard alone now
contains 6,320 emitted declarations, above the documented 5,000-declaration split
trigger. The generator report has `managedProjectSplitReady=true` and
`nativeDllSplitReady=false`.

The old single managed assembly also exposed hand-written partial types and cross-family
workflows. In particular, generated topology and the hand-written safe API jointly form
the partial `Shape` type. Moving only generated files would either fail compilation or
create a false module boundary. Moving every hand-written workflow into a low-level
module would instead introduce reverse dependencies and erase the facade boundary.

Native splitting remains unsafe. Generated and manual handles still depend on one
creator-owned registry, allocator family, validation path, and release path inside
`OcctSharp.Native.dll`. The current native surface has 2,613 live-handle registry sites,
and no cross-DLL creator-routed release protocol has been accepted.

## Decision

Physically split the managed implementation into these assemblies and packages:

- `OcctSharp.Runtime`
- `OcctSharp.Foundation`
- `OcctSharp.Geometry`
- `OcctSharp.MeshData`
- `OcctSharp.Modeling`
- `OcctSharp.Mesh`
- `OcctSharp.Documents`
- `OcctSharp.Visualization`
- `OcctSharp.DataExchange`
- `OcctSharp.Xde`
- optional direct-consumer modules `OcctSharp.IVtk` and `OcctSharp.Draw`

The accepted generated dependency graph remains acyclic. `Runtime` is the common loader
and error contract. `Foundation` depends on Runtime; Geometry depends on Foundation;
MeshData depends on Geometry; Modeling depends on Geometry and MeshData; Mesh depends on
Modeling and MeshData; Documents depends on Modeling and MeshData; Visualization depends
on Modeling, Mesh, and Documents; DataExchange depends on Modeling, Mesh, Documents, and
Visualization; Xde depends on Documents, DataExchange, and Visualization. Optional IVtk
and Draw remain above the stable graph.

All public namespaces remain `OcctSharp`. Source layout and assembly ownership do not
create new namespace prefixes.

`OcctSharp.Modeling` owns generated topology together with the hand-written `Shape`
partial implementation, `ShapeFactory`, topology/modeling DTOs, geometry snapshots used
by `Shape`, and the unified private interop contract required by those APIs. This keeps
the partial type and its `SafeHandle` lifetime in one assembly. `OcctSharp.Geometry`
owns `GpPoint`. Higher cross-family workflows, including XDE assembly authoring,
technical drawing, advanced mesh/scene workflows, document history, digital mock-up
analysis, viewers, and compatibility entry points remain in the `OcctSharp` facade.

`OcctSharp.dll` remains a real compatibility/facade assembly rather than an empty
package. It references the module graph and carries generated CLR type forwarders for
every public type moved from the former single assembly. `OcctSharp.ApiTool forwarders`
generates this deterministic source, and the build checks it for freshness. The
compatibility package references Draw and IVtk because those types were already exported
by the former single assembly. Direct modular consumers do not receive Draw or IVtk
unless they select those packages or select the compatibility facade.

Each managed assembly that owns P/Invoke declarations registers the native resolver for
its own `Assembly`. Generated native-method containers are module-unique, such as
`FoundationGeneratedNativeMethods` and `GeometryGeneratedNativeMethods`, so internal
types do not collide across project references.

Retain exactly one native bridge and runtime closure:

- one `OcctSharp.Native.dll`;
- one flat application-local `occt/` directory;
- one `OcctSharp.Native.win-x64` package containing the 62-DLL runtime and notices;
- one transitive dependency from `OcctSharp.Runtime` to the native package;
- zero native DLL copies in every managed package.

The complete package set is 14 packages: 12 module packages, the `OcctSharp`
compatibility/facade package, and `OcctSharp.Native.win-x64`. Every package uses the
OCCT-aligned version `8.0.1-preview.10`; managed assembly/file identity remains
`0.1.0.0`.

## Alternatives considered

- Keeping one managed assembly was rejected because the documented size and dependency
  triggers are met and the generated graph is now split-ready.
- Splitting generated source while leaving the `Shape` partial implementation in the
  facade was rejected because C# partial types cannot span assemblies and a direct
  Modeling consumer would lose the core topology API.
- Moving all hand-written APIs into Modeling was rejected because XDE, exchange,
  visualization, drawing, and document workflows would create reverse dependencies.
- Removing old type identities was rejected because it would create thousands of binary
  compatibility breaks without a technical need; CLR type forwarding preserves the old
  entry assembly.
- Copying the runtime into every module package was rejected because it would duplicate
  the same 62 files and produce ambiguous publish assets.
- Splitting the native bridge by product module was rejected because creator-owned handle
  registries and release routing are not cross-DLL safe.

## Consequences

- Direct consumers can reference only the managed product modules they need and still
  receive the shared native runtime transitively.
- Existing consumers can continue referencing `OcctSharp`; its aggregate public API is
  preserved through facade definitions and type forwarders.
- Assembly-qualified identity for moved types resolves through the old `OcctSharp`
  assembly only when the compatibility facade is deployed. Direct module consumers use
  the new owning assembly identity intentionally.
- Generator output remains owned by the generator. Project files link module directories
  without hand-editing generated source.
- Adding or moving an exported module type requires regenerating the forwarder source and
  passing the aggregate API diff.
- Native lifetime, ABI names, handle registries, allocator ownership, and application
  output layout are unchanged.

## Validation required

- Release and Debug solution builds with zero warnings and errors.
- Generator and runtime test suites in both configurations.
- Deterministic full generation, dependency closure, generated freshness, and clean
  regeneration.
- Forwarder freshness and aggregate API comparison with zero removals.
- All 14 packages created at one version; exactly one native package contains 62 DLLs and
  every managed package contains zero native DLLs.
- A clean compatibility-package consumer executes the inherited Batch D-L workflows.
- A clean direct `OcctSharp.Modeling` consumer creates and inspects topology without
  receiving `OcctSharp.dll`.
- `git diff --check` and repository documentation synchronization.

## Related decisions

- ADR-0008: application-local native runtime layout.
- ADR-0015: staged managed and package modularity.
- ADR-0047: optional integration isolation.
- ADR-0059: committed runtime and MIT licensing.
- ADR-0061: generated product-module ownership.
- ADR-0062: generated shard dependency closure and native split deferral.
- ADR-0065: OCCT-aligned preview versioning.

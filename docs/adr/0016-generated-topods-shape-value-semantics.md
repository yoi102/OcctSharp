# ADR-0016: Generate TopoDS Shape Value Semantics

- Status: Accepted
- Date: 2026-08-22

## Context

`TopoDS_Shape` is a small OCCT value object whose copies share an internal `TShape`
while carrying location and orientation as part of each value. Treating it as a raw
pointer would lose those semantics, while treating every copy as a deep geometry clone
would be incorrect and unnecessarily expensive. The existing manual shape handle
already owns one C++ `TopoDS_Shape` value and validates live wrappers, but its semantic
operations were not generator-owned.

## Decision

- Type-map rule `TM007` projects a `TopoDS_Shape` value as an opaque owning
  `OcctSharp_ShapeHandle*`, an internal `ShapeHandle`, and the public `Shape` wrapper.
- Each native wrapper owns an independent C++ `TopoDS_Shape` value. Normal OCCT copies
  share the internal `TShape`; no deep geometry copy is implied.
- Generated clone and reversed operations allocate new registered wrappers. Releasing
  one wrapper cannot invalidate another value copy.
- Generated comparisons preserve OCCT definitions: `IsPartner` compares the underlying
  `TShape`, `IsSame` also includes location, and `IsEqual` also includes orientation.
- Shape kind and orientation cross the C ABI as validated 32-bit enum values and become
  `ShapeKind` and `ShapeOrientation` in the friendly API.
- Generation configuration schema 1.4 adds explicit `topologyScopes`. The initial scope
  is limited to `TopoDS_Shape` copy construction, null state, kind, orientation,
  reversal, and the three comparison operations.
- Generated output is partitioned below a `Topology` module directory in preparation
  for the managed package stages accepted by ADR-0015.
- Typed `TopoDS_*` checked conversions are specified separately by ADR-0017; location
  mutation and child/explorer lifetimes remain later work.

## Alternatives considered

- Passing `TopoDS_Shape*` directly was rejected because pointer identity is not OCCT
  shape identity and the pointer would expose unstable C++ lifetime and layout.
- Deep-copying geometry for every wrapper clone was rejected because it does not match
  OCCT value-copy behavior and breaks partner/same/equal semantics.
- Reusing one managed wrapper instance for every native copy was rejected because
  location and orientation belong to each `TopoDS_Shape` value.
- Enabling the complete typed topology hierarchy in one batch was rejected because
  subtype validation, downcasts, construction rules, and explorer results need focused
  safety evidence.

## Consequences

- The generated set expands from eight to twelve files and from 42 to 50 emitted
  declarations in the selected scope.
- The additive exports require ABI 1.10 and bridge 0.11.0. The package advances to
  `0.1.0-alpha.7`.
- `Shape` remains the compatibility surface for existing manual creation and exchange
  APIs while its core value semantics become generator-owned.
- Hashing is intentionally deferred until its equality contract is selected and tested.

## Validation

- Generator tests cover `TM007`, the eight selected stable declarations, four
  module-partitioned outputs, and rejection of an unsupported topology scope.
- Runtime tests cover kind/orientation, clone independence, partner/same/equal
  distinctions, reversal, disposal order, and access after disposal.
- Release and Debug builds, generated-source freshness, and the clean NuGet consumer
  must pass for this decision's package baseline.

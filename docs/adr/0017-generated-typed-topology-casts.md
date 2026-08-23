# ADR-0017: Generate Typed Topology Casts

- Status: Accepted
- Date: 2026-08-22

## Context

The base `Shape` value contract from ADR-0016 is safe, but users need the OCCT
topology hierarchy as meaningful managed types. OCCT's `TopoDS_Compound`,
`TopoDS_Solid`, `TopoDS_Face`, and the other topology classes are value wrappers over
the same underlying topology representation; their public headers do not provide a
distinct native layout that can safely cross the ABI. OCCT provides the authoritative
`TopoDS::Compound`, `TopoDS::Solid`, and related checked conversions, which validate
`ShapeType()` and raise `Standard_TypeMismatch` for an incompatible non-null value.

## Decision

- Configuration schema 1.4 adds reviewed `typedTypes` entries below the topology scope.
  The initial set is `Compound`, `CompSolid`, `Solid`, `Shell`, `Face`, `Wire`, `Edge`,
  and `Vertex`.
- The generator emits one opaque C ABI cast operation per configured type. Native code
  calls the matching `TopoDS::Xxx` conversion, copies the resulting value into a new
  registered `OcctSharp_ShapeHandle`, and never exposes a C++ derived layout or
  reference.
- `Standard_TypeMismatch` maps to existing ABI status 9 (`TypeMismatch`). Other
  OCCT/standard/unknown exceptions keep the existing error contract.
- The friendly API exposes `Shape.CastXxx()` and `Shape.TryCastXxx(out Xxx?)`.
  `CastXxx()` throws `InvalidCastException` for a mismatched non-null kind;
  `TryCastXxx()` returns false and no wrapper for that case. A null `TopoDS_Shape` is
  accepted by OCCT's checked conversion and produces a valid null typed value.
- Every successful cast owns an independent C++ `TopoDS_Shape` value. It preserves
  normal OCCT `TShape`, location, orientation, and equality semantics and remains valid
  after the source wrapper is disposed.
- Typed wrappers inherit the generated base `Shape` API. Their constructors are
  internal and can only be created by a validated native cast.
- The initial batch does not emit typed default constructors, assignment operators,
  hashing, location mutation, or explorer/child APIs. Those require later contracts.

## Alternatives considered

- Reinterpreting the managed handle as a derived native pointer was rejected because
  the native layout and owner are not part of the C ABI contract.
- Returning borrowed references from `TopoDS::Xxx` was rejected because a managed
  wrapper must own a stable value after the source call and after source disposal.
- Returning false for every null cast was rejected because OCCT treats null topology
  values as valid values for these checked conversions.
- Implementing each subtype conversion manually in the friendly layer was rejected
  because the native `TopoDS::Xxx` type check must remain the semantic authority and
  generated exports must be traceable to configuration.

## Consequences

- B04 adds eight generated cast exports and eight public typed topology wrappers while
  retaining one native bridge and one managed assembly.
- The additive ABI requires ABI 1.11 and bridge 0.12.0. The package advances to
  `0.1.0-alpha.8`.
- Wrong-kind behavior is now testable without exposing OCCT exceptions or layouts.
- B05 still owns location/transformation values, and B10 still owns topology traversal
  and child lifetime semantics.

## Validation

- Generator tests verify all configured identities, generated raw/native/friendly casts,
  `TopoDS::Face` emission, TypeMismatch handling, and invalid-kind rejection.
- Release and Debug runtime tests verify solid and compound casts, wrong-kind `TryCast`
  and throwing casts, equality/identity preservation, and source-disposal independence.
- Generated freshness and the clean package consumer must pass for alpha.8.

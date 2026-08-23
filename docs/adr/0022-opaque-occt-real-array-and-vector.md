# ADR-0022: Opaque OCCT real array and vector bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: B06 second migration wave on OCCT 8.0.1, Windows x64

## Context

`NCollection_Array1<T>` owns allocator-backed storage with a caller-defined native
lower bound. In OCCT 8, `NCollection_Vector<T>` is a deprecated alias for the
zero-based `NCollection_DynamicArray<T>`. Neither layout nor element references can
cross the C ABI safely. `double` is already a verified value-copy type.

## Decision

Expose only `double` specializations through registry-validated opaque owning handles.
`NCollection_Array1<double>` is created with a native lower bound of 1 and exposes the
bound explicitly; the managed API presents a 0-based view. The vector alias is backed by
`NCollection_DynamicArray<double>` and remains zero-based. Both APIs copy input values,
support clone, count/value reads, bounded mutation, value-by-value enumeration, finite
value validation, idempotent release, and no native pointer exposure.

## Consequences

The bridge is safe for the selected value element and keeps allocator/lifetime details
native-local. Maps, richer element types, borrowed iterators, and generated template
rules remain pending in B06. These manual wrappers do not inflate generated declaration
coverage.

## Validation

Debug and Release builds, 32 generator tests, 36 runtime tests, generated freshness,
and a clean alpha.13 package consumer with 36 native DLLs are required and recorded in
`docs/STATUS.md`.

# ADR-0056: Generated Translation Unit Completion Headers

- Status: Accepted
- Date: 2026-08-25

## Context

The full generated shared-handle translation unit includes each selected OCCT scope
header directly. OCCT 8.0.1 `RWGltf_GltfLatePrimitiveArray.hxx` declares
`NCollection_Sequence<RWGltf_GltfPrimArrayData>` but only forward-declares the element.
MSVC instantiates the sequence destructor in the generated translation unit and requires
the complete element type. Normal OCCT build contexts happen to include its definition
first; alphabetical standalone generated includes do not.

## Decision

Configuration schema 1.8 exposes `generatedPreambleHeaders`. The shared emitter validates
each exact include name and emits the sorted list before all shared-scope headers. The
OCCT 8.0.1 configuration includes `RWGltf_GltfPrimArrayData.hxx`.

## Alternatives considered

- Depending on PCH or incidental include order was rejected because clean consumers and
  OCCT upgrades must compile deterministic standalone generated sources.
- Reordering all headers based on filenames was rejected because filename order does not
  model template completeness.
- Hard-coding RWGltf in the emitter was rejected because the dependency belongs to the
  pinned OCCT artifact configuration.
- Excluding the bindable RWGltf type was rejected because the missing completion include
  has a narrow compile-safe remedy.

## Consequences

- A pinned OCCT artifact may carry a small audited completion-header list.
- Upgrade review must remove entries no longer needed and add only compiler-proven exact
  dependencies.
- Generated source freshness changes when this list changes; no manual generated edit is
  allowed.

## Validation required

- Deterministic emitter tests verify preamble-before-scope include order and reject unsafe
  or duplicate values.
- Full Release and Debug native compilation must prove the configured header is sufficient.
- Generated freshness and byte-identical regeneration remain release gates.

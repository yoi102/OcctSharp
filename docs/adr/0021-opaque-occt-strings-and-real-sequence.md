# ADR-0021: Opaque OCCT Strings and Real Sequences

- Status: Accepted
- Date: 2026-08-22
- Scope: B06 first migration wave on OCCT 8.0.1, Windows x64

## Context

OCCT strings are value types with different encoding and indexing rules. `TCollection_AsciiString`
stores UTF-8 bytes even though its historical name says ASCII, while
`TCollection_ExtendedString` stores UTF-16 code units and converts to UTF-8 on request.
`NCollection_Sequence<T>` is a native container with 1-based indexing and native
allocation. None of these layouts or internal buffers are stable managed ABI contracts.

## Decision

Expose the first B06 families through registry-validated opaque owning handles:

- `OcctAsciiString`: UTF-8 input/output copies, byte length, append, clone, and
  conversion to `OcctExtendedString`.
- `OcctExtendedString`: UTF-8 input/output copies, UTF-16 code-unit length, UTF-8
  byte length, 0-based managed character access mapped to OCCT's 1-based `Value`,
  append, clone, and conversion to `OcctAsciiString`.
- `OcctRealSequence`: `NCollection_Sequence<double>` creation, clone, count, 0-based
  managed indexer, append, set, remove, and enumeration.

All text crossing the ABI uses caller-owned UTF-8 buffers and explicit byte lengths;
native-to-managed reads use caller-provided output buffers, so no OCCT allocation crosses
the boundary. Sequence values are copied in and out, finite values are required, and
native 1-based indices are never exposed directly in the friendly API.

## Alternatives considered

- Returning `const char*` or `const char16_t*`: rejected because the pointer lifetime is
  tied to a native value and mutation can invalidate it.
- Treating `TCollection_AsciiString` as a managed ASCII-only string: rejected because
  OCCT 8.0.1 explicitly stores UTF-8 bytes.
- Projecting `NCollection_Sequence<double>` as a managed array: rejected because it
  would lose native mutation/clone semantics and misstate ownership.

## Consequences

The bridge establishes reusable UTF-8 buffer and collection-index rules for the rest of
B06. These are manual special cases until the generator can express string encodings,
buffer contracts, container element mappings, and iterator ownership without fallback.
Maps, arrays, richer element types, and borrowed iterators remain in the same B06 batch.

## Validation required

Debug and Release native/managed builds, 32 generator tests, 34 runtime tests, generated
freshness, and a clean alpha.12 NuGet consumer with 36 native DLLs under `occt`.

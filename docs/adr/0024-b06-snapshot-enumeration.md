# ADR-0024: Caller-owned snapshots for B06 collection enumeration

- Status: Accepted
- Date: 2026-08-22
- Scope: B06 scalar collections and integer-key maps on OCCT 8.0.1, Windows x64

## Context

OCCT collection iterators and node references are native-owned and may become invalid
when the collection is mutated or released. Crossing an iterator object through the C
ABI would make disposal and early-exit behavior difficult to prove.

## Decision

Expose one-shot snapshot exports that copy the current values into caller-owned managed
buffers. Native code validates the registry handle and destination capacity, writes the
requested scalar values, and returns the count. Managed `Snapshot()` methods return new
arrays or key/value arrays and never expose native pointers, iterators, or references.

## Consequences

- Snapshots are stable after later mutation or disposal of the source collection.
- Enumeration remains fail-closed: an iterator started after disposal throws rather than
  dereferencing a stale handle.
- Large collections pay an explicit copy cost; streaming iterators remain pending until
  a separately validated lifetime contract exists.
- Additive exports require ABI 1.18, bridge 0.19.0, and package alpha.15.

## Validation

Debug and Release builds pass 32 generator and 40 runtime tests. The clean package
consumer passes with 36 native DLLs below `occt`, ABI 1.18, bridge 0.19.0, and OCCT 8.0.1.

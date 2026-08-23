# ADR-0023: Opaque OCCT integer-key map bridge

- Status: Accepted
- Date: 2026-08-22
- Scope: B06 map wave on OCCT 8.0.1, Windows x64

## Context

`NCollection_DataMap` owns hash buckets and nodes, while `NCollection_IndexedMap`
maintains ordered keys and native 1-based indexes. Their internal layout and iterators
are not an ABI contract. Integer keys and `double` values are already verified scalar
projections.

## Decision

Expose only `NCollection_DataMap<int,double>` and `NCollection_IndexedMap<int>` through
registry-validated opaque owning handles. Copy construction buffers, clone independently,
and provide bounded lookup/mutation operations. Data-map bind/unbind reports key state;
indexed-map operations preserve ordered keys and translate native 1-based indexes to the
managed 0-based list view. Duplicate creation keys and non-finite values are rejected.
No native node, bucket, reference, or iterator crosses the C ABI.

## Consequences

The selected maps are safe scalar-value foundations for later generated collection rules.
Richer keys/values, map iterators, borrowed views, and parent-bound collections remain
pending in B06. These manual wrappers do not inflate generated declaration coverage.

## Validation

Debug and Release builds, 32 generator tests, 38 runtime tests, generated freshness,
and a clean alpha.14 package consumer with 36 native DLLs are required and recorded in
`docs/STATUS.md`.

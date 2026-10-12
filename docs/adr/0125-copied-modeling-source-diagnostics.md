# ADR-0125: private copied modeling source diagnostics

Status: Accepted. Date: 2026-10-10.

Existing modeling operations construct geometry but do not retain independent
configuration and exact source diagnostics. The SDK Section ancestor query selects
the first matching interference; that value alone does not prove unique provenance.

Use an exact generated workflow profile with a single owned input/result domain,
geometry-deep-copied input graph, immutable diagnostics and success-only topology.
Privately traverse all bounded section interference paths and prove source membership
before publishing a unique ancestor. Retain Missing/Multiple/Unproven states, including
split/merge generated histories, without automatic topology rebinding. Private borrowed
SDK helpers are exact audited prerequisites and cannot escape the executing call.

Offsets explicitly initialize defaults and lofts disable mutable input. SDK failure
retains diagnostics; invalid topology does not escape. Querying a shape produces a
new independent deep copy. Lifetime uses existing typed registry leases and SafeHandles;
the C ABI remains in the single bridge. Input/output budgets do not promise SDK timeout.

This extends the copied result contract from ADR-0124 without broad reference mappings
or a new ownership category. See [BG contract](../BATCH_BG_MODELING_RESULTS.md).

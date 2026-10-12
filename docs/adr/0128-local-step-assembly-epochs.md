# ADR-0128: local STEPCAF assembly epochs

Status: Accepted within the continuously authorized BF–BK implementation.
Date: 2026-10-10.

## Decision

Extend STEPCAF through a private one-use selected-root workspace. Require an explicit
bounded flat local file manifest, exact source hashes and a complete same-SDK graph
preflight before transfer. Stage immutable private inputs and reject cycles/ambiguous
keys/path-bearing or unsupported references. The native cache cannot replace cycle
validation because it binds only after recursive transfer returns.

Copy actual file/root statuses and occurrence relations. Publish the close-safe
owned document only when selected and loaded external transfers succeed; preserve
independent failure records. Deep-copy result shapes for detached downstream use.
Keep implementation in Xde, which may depend on existing DataExchange/Modeling
contracts, without adding a reverse module dependency.

## Consequences

Absolute and directory-bearing external references, network access, arbitrary
streams/maps and custom recursive repair propagation remain outside this profile.
Manifest budgets do not promise a strict computation deadline. Fixed source/actual
file/runtime and full product-delivery validation remain necessary.

The pinned Format() lookup misses ordinary representation records emitted by its writer. Audit exact DocumentFile/property/representation associations with bounded native model/item counts; reject unknown or contradictory explicit formats before transfer. Record the three necessary additional declarations separately from the original six graph prerequisites.

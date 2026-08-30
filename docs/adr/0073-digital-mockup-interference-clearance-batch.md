# ADR-0073: Implement digital mock-up interference and clearance as Batch L

- Status: Accepted for implementation
- Date: 2026-08-31
- Scope: Batch L product denominator, dependency closure, ownership, and validation

## Context

Batch B through K cover routine modeling, inspection/PMI, freeform authoring, drawing,
mesh/scene, document history, feature recovery, and assembly authoring. A common product
gap remains: scalable occurrence-aware clearance, contact, penetration, containment, and
self-interference analysis with durable issue review.

## Decision

Open Batch L as the one indivisible 24-capability wave in
`BATCH_L_DIGITAL_MOCKUP_INTERFERENCE_GAP_INVENTORY.md`. Keep native broad/exact-phase
algorithms and containers call-local, copy scalar/diagnostic/traceability results, return
issue topology as independent owners, and preserve existing XDE/viewer parent boundaries.

Implementation will target Preview.9, ABI 1.54, bridge 0.62.0, and schema 1.12. No
family-only or numbered checkpoint is Batch L completion. All local gates must pass
together before the ADR becomes implemented.

## Consequences

- The 24-row product denominator is immutable; the 1,351 declaration audit is evidence,
  not a promise to wrap every audited declaration.
- Only direct blocked declarations actually used may enter SC-048.
- One managed assembly, one native DLL, one package, and the current shard graph remain.
- Hosted release, signing, publication, and GitHub work stay outside this batch.

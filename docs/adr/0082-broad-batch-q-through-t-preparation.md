# ADR-0082: Prepare four broad product batches Q through T

- Status: Accepted for scope preparation; implementation not started
- Date: 2026-09-05

## Context

B-P and ADR-0081 are complete locally at Preview.15. The user asks to prepare several
following batches with broader scope. This is preparation authorization, not a request
to begin their API implementation or publish packages. Earlier repeated whole-batch,
local-commit and no-automatic-publication boundaries remain in effect.

## Decision

Freeze four 40-capability cross-family product waves on commit `6b04bd9`:

- Q: shape repair and topology normalization, including protected repair, budgets and history.
- R: mesh authoring/editing and discrete-model delivery, not another shape-meshing statistics batch.
- S: guided sweeps, scalar laws and constrained surface authoring with fulfilment evidence.
- T: typed parametric recompute and fallible persistent topology selection.

The [shared preparation record](../BATCH_Q_T_PREPARATION.md) and four linked matrices
define 160 observable capability rows, source ownership, exact decision/support roots,
template-header exceptions, acceptance criteria, non-goals and validation gates.
Prepare together; implement and validate Q, R, S, then T as whole batches with one local
completion commit each. Source/task groups inside a batch are not delivery fragments.
Q/R/S largely reuse B-P; T has actual dependencies on their accepted result/recipe contracts.

All scopes use the frozen Preview.15 inventory. After each completed batch, explicitly
record the next batch's baseline delta before implementation; do not silently rebase
the hash or count previously delivered functionality again. Scope-prepared is not
implementation-ready when required predecessors or baseline revalidation are outstanding.
Planned Preview.16-19 slots are reservations only. Current package/ABI/bridge/schema
identities and generated/manual dispositions are unchanged in this checkpoint.

Keep ADR-0074's modules/facade and one native DLL and ADR-0081's source rules. R pure
mesh data must remain independent of Modeling/XDE. T Documents storage/naming must
not acquire higher-layer Mesh/XDE/viewer dependencies; built-in feature orchestration
lives in existing higher owners/facade. Temporary algorithms remain native-local,
DTOs copied, Shapes owning, labels/viewer IDs parent-bound. No new ownership category.

## Alternatives

- Per-class, getter/setter or per-family small waves: rejected because they do not
  deliver the requested broader end-to-end outcomes.
- One combined 160-capability implementation checkpoint: rejected because independent
  product waves would lose their separate preparation/validation/local-commit boundaries.
- Immediate further managed/native binary split: rejected; the current source boundaries
  suffice, and no cross-DLL allocator/registry/release proof has been introduced.
- Mark every candidate under a root as migrated: rejected; existing emitted/manual,
  skipped, generic and unsupported declarations are not new product capabilities.

## Consequences and safety boundaries

Each implementation must close its complete 40-row acceptance map and all local gates.
The audit pool is 149 distinct roots and 5,289 distinct stable IDs, not a new API count
or total remaining coverage denominator. Plans must expose OCCT's actual limitations:
explicit topology-changing repair, discrete versus exact shapes, sweep-mode conflicts,
filling residuals and fallible naming. Arbitrary callbacks, scripts, virtual proxies,
general mesh reverse engineering and complete constraint solving remain out of scope.
Scope changes require a recorded decision, not silent row deletion.

## Validation

Preparation requires deterministic exact-root audits, SDK header and representative
binary-symbol availability checks, stable-ID overlap accounting, wrong-baseline and
input-overwrite negative checks, document matrices/links and unchanged production files.
The verifier and final evidence are in the shared preparation document and STATUS.
Compile/runtime tests of Q-T, packaging, DLL updates and publication are NOT RUN here.
Each later implementation requires Release/Debug and actual Debug-native regression,
real workflows, ownership/source/dependency gates, clean consumers/regeneration,
compatibility/inventory/local release evidence, documentation and a local commit.

Related: ADR-0060, ADR-0065, ADR-0074, ADR-0079, ADR-0080, ADR-0081;
MIGRATION_PLAN, OWNERSHIP, NATIVE_SOURCE_LAYOUT.

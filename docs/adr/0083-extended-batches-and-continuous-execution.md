# ADR-0083: Extend preparation to U-W and define continuous Q-W execution

- Status: Accepted for preparation and the next explicitly started continuous run
- Date: 2026-09-05

## Context

ADR-0082 prepares Q-T as four 40-capability waves. The user asks for more broad batches
now, intending to implement them continuously next time. Completed baseline is B-P plus
ADR-0081 at Preview.15; preparation checkpoint is `eacd0ed`. Planning is not execution.

## Decision

Prepare U advanced contour finishing/limit-driven local features, V exact partition/
material-region/volume construction and W viewer lighting/materials/copied frame review,
each with 40 accepted capability rows. Preserve Q-T's denominators and initial evidence.
The [U-W preparation](../BATCH_U_W_PREPARATION.md) excludes already implemented J/D/H
features and records roots, SDK semantics, source/lifetime owners and future acceptance.

The next explicitly started continuous run follows Q -> R -> S -> T -> U -> V -> W,
280 capabilities. It completes each whole batch, all local gates and one local commit,
then performs the next explicit baseline delta and advances without routine user
reconfirmation. A checkpoint is not an instruction to end the active run. Persist the
[resume journal](../BATCH_CONTINUOUS_EXECUTION.md); never promise execution after the
session stops or create a scheduler/background task from this preparation request.

Shared prerequisite contracts are explicit: U needs Q/S/T, V needs Q/R/T, W needs R and
existing viewer/material work. Queue order does not imply all preceding algorithms are
dependencies. All scopes are prepared on the same unchanged Preview.15 inventory and
must be revalidated as implementation baselines evolve. In-scope revalidation is
agent work, not routine approval; material scope/authority changes still need direction.

Keep current managed modules, facade, one native DLL, source responsibility ceilings,
registry/TLS owners and copied/owning/parent-bound contracts. U/V use cohesive Modeling
units; W uses Visualization units; higher integration remains above Documents. W uses
copied CPU frame data in its WPF example and does not promise headless/D3DImage rendering.
Planned Preview.20-22 slots extend the existing sequence but do not change versions now.

## Alternatives

- End and ask after every validated batch: rejected for the next authorized continuous run.
- One 280-capability implementation commit: rejected; per-batch evidence and recoverability
  remain required.
- Repackage old basic fillets/cells/screenshots as new capabilities: rejected by live
  source comparison and the explicit new acceptance matrices.
- Publish automatically, bypass gates or split DLLs for speed: rejected as outside scope.

## Consequences and validation

Scope preparation is 7/7 and implementation remains 0/280. Frozen-root audits include
219 distinct roots/7,668 candidate IDs; candidates are not new APIs or all remaining
migration. Validate repeat audits, all matrix IDs, prerequisite ordering, SDK headers,
representative symbols/toolkit presence, baseline/input protection, links and source
boundaries. A shared verifier retains the Q-T wrapper and its original summary hash.
Actual compile/runtime/driver-success/package gates for new APIs remain NOT RUN.

The runbook defines recovery, evidence and stop conditions. Compiler/test failures are
repaired inside the same batch; unknown ownership, missing necessary hardware/access,
conflicting user work or materially unsupported outcomes cannot be hidden by “continuous.”
No NuGet upload, GitHub push, signing, hosted release or destructive expansion is implied.

Related: ADR-0060, ADR-0065, ADR-0074, ADR-0081, ADR-0082; STATUS, MIGRATION_PLAN,
OWNERSHIP, NATIVE_SOURCE_LAYOUT and BATCH_CONTINUOUS_EXECUTION.

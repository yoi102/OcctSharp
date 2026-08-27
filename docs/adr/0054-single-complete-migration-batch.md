# ADR-0054: Use one complete migration batch

- Status: Accepted
- Date: 2026-08-24
- Scope: Complete OCCT-to-C# migration and release completion
- Supersedes: The B00-B20 batch partition in ADR-0050 and the migration plan

## Context

The former plan labelled repository, generator, ownership, modeling, exchange,
visualization, long-tail, and release work as B00 through B20. Those labels encouraged
local completion percentages and commits even though the user's intended unit is the
complete OCCT migration. Subdividing the remaining work as B19.1, B19.2, and similar
labels made progress appear smaller and obscured the one final completion condition.

## Decision

OcctSharp's current migration has one batch named `B`. Everything formerly labelled B00
through B20, including any dotted Bxx.y label, is part of that single batch. The old
labels are historical capability milestones, not batches, and must not be used for
current loop state, progress, commit boundaries, or future planning.

If a genuinely new product-scale batch is needed after B, it uses the next whole-letter
identifier, for example `C`, and must be comparable in scope to the complete former
B00-B20 program. Numbered or dotted fragments such as B21, C01, or C.1 are forbidden.
No future letter batch is created merely to move unfinished B work out of the way.

Work inside B is organized as unnumbered workstreams and large coherent migration waves.
One wave should include as many related packages and API families as can share truthful
ownership, generation, validation, and packaging evidence. A wave is not a batch and
does not receive a B-derived identifier.

Batch B reaches 100% only when every bindable declaration in the declared profile is
emitted or accepted manual, broad unknown projection/ownership reasons are eliminated,
all required build/runtime/package/release gates pass, and remaining exclusions have
narrow evidence-backed dispositions. Classification completeness and engineering
estimates remain separate metrics.

No partial-wave commit is described as completing B. The requested batch-boundary
commit occurs only after all B exit criteria pass. Publication and pushing still require
their own authority and release prerequisites.

## Consequences

- `STATUS.md`, `MIGRATION_PLAN.md`, `ROADMAP.md`, and the reusable AI prompt report only
  `CURRENT_BATCH: B` and a single B completion percentage.
- Historical release versions and API milestones may still be described, but without
  treating old Bxx labels as active batches.
- Any future batch uses one whole letter and the same product-scale sizing; implementation
  waves inside it remain unnumbered and are not completion or commit boundaries.
- The loop selects large coherent workstreams instead of inventing sub-batches.
- Existing repository history is not rewritten; old commit messages can retain their
  historical wording.

## Validation required

- Current planning and loop-state documents contain no active B00-B20 or Bxx.y batch
  progression.
- Completion remains blocked while bindable declarations are unselected or required
  gates are not run or fail.
- Every material migration wave updates coverage, ownership, ABI, tests, package facts,
  and the next large workstream before continuing.

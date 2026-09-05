# AI Migration Loop Prompt

Use this prompt for a user-authorized migration task. Current progress comes only from
[STATUS](STATUS.md); execution policy comes from [WORKFLOW](WORKFLOW.md) under ADR-0091.
Q-W is complete at 280/280 and `15fd671`. Its frozen preparation is historical, not a
request to restart it. [The old Batch F prompt](AI_MIGRATION_LOOP_PROMPT_HISTORY.md)
is retained as reference only.

## Reusable prompt

```text
Recover the current user request, AGENTS.md, STATUS and WORKFLOW. Read the relevant
architecture/topic contracts and ADRs for the affected behavior; reuse context already
read. Consult ROADMAP for priority changes. Do not execute archived active-loop states.

For preparation-only work, audit the proposed outcome, dependencies and acceptance;
finish preparation without automatically starting product implementation.

For authorized implementation, select or resume a useful dependency-coherent scope.
There is no fixed capability count or minimum number of API families. Generator,
type-map and ownership improvements can be independent workstreams. Record baseline
and exact coverage changes where relevant. Do not silently shrink accepted outcomes.

Preserve generated/manual separation, explicit ownership and native exception safety,
module boundaries and the single native DLL. Make routine implementation decisions
within scope without repeated confirmation. Record architectural/ownership changes
in the relevant contract and an ADR when required by WORKFLOW.

Run impact-based checks during development. When commits are authorized, use truthful
intermediate commits as recovery points and continue the active task. A saved commit
does not complete a product scope. Downstream work needs verified prerequisites.

At product/binding delivery, run all full delivery gates in WORKFLOW. Documentation
and tooling maintenance use their applicable checks. Report only executed checks;
identify reused evidence by baseline and mark unexecuted checks NOT RUN. Repair failures
without disguising them as completion or dropping difficult requirements.

Update STATUS and only topic facts that changed. Continue until the user's accepted
outcome and applicable validation are complete, the user changes/stops the task, or
progress requires external input. Do not infer publication, GitHub push or a scheduler
from local implementation authorization. Never claim continued execution after the
session stops. Keep historical matrices and preparation inputs intact.
```

## Reporting

State the delivered outcome, remaining work and validation evidence in plain language.
No special footer is required for interactive tasks. If an existing external orchestrator
needs a machine-readable state, preserve its agreed interface and distinguish progress,
blocked work, preparation completion and the complete accepted implementation scope.

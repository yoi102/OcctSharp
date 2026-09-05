# Repository Instructions

These instructions apply to the entire repository.

## Required reading

Before making a material change, read:

1. `docs/STATUS.md` and `docs/WORKFLOW.md`
2. The relevant sections of `docs/ARCHITECTURE.md` and the topic contract
3. Relevant ADRs; consult `docs/DECISIONS.md` when changing a design boundary
4. `docs/ROADMAP.md` when selecting or changing priorities

Reuse context already read in the current session. Trivial documentation fixes need
only the affected context. ADR-0091 and WORKFLOW define current execution policy:
future work has no fixed capability count, authorized intermediate commits are allowed,
and validation is selected by impact with full product-delivery gates retained.

The detailed historical AI rules remain in `docs/AI_INSTRUCTIONS_OCCT_NET.md` as
reference only, not an additional mandatory checklist. Completed batch instructions
apply to their historical scope, not automatically to future tasks. Current user
instructions, current execution policy and applicable accepted contracts take priority.

## Repository boundary

- Keep documentation in the root `docs/` directory.
- Keep all code-related projects and artifacts under the root `OcctSharp/` directory.
- Do not create `src/`, `tests/`, `benchmarks/`, `config/`, `reports/`, or solution
  files directly at the repository root.
- Do not initialize another Git repository inside `OcctSharp/`.

## Generated and manual code

- Never hand-edit generated output as a long-term fix.
- Fix the parser, binding model, type map, transformation pass, emitter, or rule,
  then regenerate.
- Record every manual binding exception in `docs/SPECIAL_CASES.md`.
- Record ownership changes in `docs/OWNERSHIP.md` and important design changes in
  an ADR.

## Validation claims

- Report only checks actually run.
- Use `NOT RUN` when a check was not executed.
- A successful generation is not a successful compile.
- A successful compile is not runtime or lifetime validation.
- Update `docs/STATUS.md` after material progress.

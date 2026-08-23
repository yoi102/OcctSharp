# Repository Instructions

These instructions apply to the entire repository.

## Required reading

Before making a material change, read:

1. `docs/STATUS.md`
2. `docs/ARCHITECTURE.md`
3. `docs/ROADMAP.md`
4. The topic document relevant to the change
5. `docs/DECISIONS.md` and any relevant ADR

The detailed historical AI rules remain in `docs/AI_INSTRUCTIONS_OCCT_NET.md`.
If that document conflicts with an accepted ADR or the current status document,
the accepted ADR and current status take precedence.

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

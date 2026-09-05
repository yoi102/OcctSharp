# ADR-0091: Proportionate development workflow and separate current status

- Status: Accepted
- Date: 2026-09-06
- Authority: The user requested applying the reviewed workflow relaxation.
- Scope: Future work sizing, intermediate commits, validation cadence, documentation
  routing and bounded native source-size exceptions.
- Supersedes in part: ADR-0054/0060 work-size and commit restrictions, ADR-0080/0081
  absolute source-size policy, and extrapolation of ADR-0082/0083 Q-W scheduling to
  future work. Completed scopes and their validation evidence remain unchanged.

## Context

Q-W is complete at product commit `15fd671`, Preview.22, 280/280 accepted capabilities.
The fixed forty-row scopes and whole-batch-only commits served that authorized run.
Treating them as universal rules restricts focused generator improvements and routine
maintenance. Long documents also mix current W evidence with S-era active-loop state.
An absolute 1,000-line failure can encourage splitting a cohesive native responsibility
merely to satisfy a number. Ownership and runtime validation remain necessary.

## Decision

Adopt [WORKFLOW](../WORKFLOW.md) as the current execution policy and keep
[STATUS](../STATUS.md) as the only current progress summary. AGENTS routes to relevant
contracts rather than requiring every historical document for every material change.
Archived instructions are reference material, not an additional active checklist.

Future tasks have no fixed capability count or minimum family count. Permit reusable
generator/type-map/ownership work without unrelated application features. When commits
are authorized, allow truthful intermediate recovery commits and continue the requested
work; only accepted outcomes with applicable gates passed may be reported complete.
Q-W's frozen configuration, original denominators and historical commits are preserved.

Use impact-based checks during development and for documentation/tooling maintenance.
Keep full product/binding delivery gates and all existing full release-script checks.
Do not rerun them merely for an intermediate commit or prose edit. Repetitions address
observed risks, rather than a universal ten-run quota. Baseline reuse requires unchanged
relevant inputs and explicit evidence identity. Packaged-document changes must be
validated when delivering a new package; the old package remains historical meanwhile.

Allow finite, exact-file native size exceptions with rationale in the source-layout
configuration. The verifier must still reject malformed, duplicate or nonexistent
entries, unapproved oversize files, exceeded limits and all existing structural hazards.
No current source file receives an exception as part of this policy change.

Routine implementation choices and size exceptions require no ADR or repeated approval.
New ownership/ABI categories and architecture boundaries still require design records.
Generated/manual separation, native lifetime and exception safety, single-DLL ownership,
honest classification, and separate publication authority remain unchanged.

## Alternatives considered

- Delete safety and runtime gates: rejected because native failures can corrupt memory.
- Keep all policies but add another advisory note: rejected because conflicting active
  instructions and the executable size gate would remain.
- Rewrite past batch denominators or claim full OCCT completion: rejected; historical
  acceptance and full-library coverage answer different questions.

## Validation required

Run script syntax checks, real source-layout verification, existing structural rejection
fixtures and positive/negative size-exception fixtures. Verify documentation links,
current-state consistency, archived evidence preservation and whitespace. Product
build/runtime/pack tests are NOT RUN for this documentation and verification-tooling
change; the Preview.22 results remain attributed to their original product baseline.

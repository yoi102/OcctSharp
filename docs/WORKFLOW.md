# Development workflow

This is the current execution policy, accepted in [ADR-0091](adr/0091-proportionate-development-workflow.md).
Use [STATUS](STATUS.md) for current product facts. Completed batch matrices, preparation
inputs and archived prompts describe their historical scope; they do not impose work
size, commit frequency or a new execution queue on future requests.

## Recover only relevant context

Start with AGENTS.md, STATUS and this policy. For a material implementation change,
read the relevant architecture section, topic contract and ADRs; consult ROADMAP when
choosing or changing priorities and DECISIONS when changing a design boundary. Reuse
context already read in the current session. A spelling fix does not require reading
every ownership, packaging and historical batch document.

## Choose a useful scope

- Size work by dependencies, risk and a verifiable outcome. There is no required
  capability count, minimum number of API families or letter-only task naming rule.
- Generator, parser, type-map, ownership and infrastructure improvements are valid
  workstreams in their own right. Prefer reusable rules where they unlock safe bindings;
  do not attach unrelated exchange, viewer or WPF features just to enlarge a task.
- Preserve accepted scope and report added, deferred or blocked outcomes explicitly.
  Routine dependency discoveries and implementation choices within the user's request
  need no repeated confirmation. Material changes to requested outcomes need direction;
  an elapsed deadline or a failed test is not permission to discard a requirement.
- For a new binding scope, record its baseline and exact stable-ID delta. Reuse an
  unchanged, hash-identified inventory during development. Refresh full inventory when
  parser/configuration/classification/SDK inputs change and at binding delivery; do not
  repeat a full scan solely because another intermediate commit was made.

## Save progress without declaring completion

When local commits are authorized, allow cohesive intermediate commits with a truthful
description of implemented behavior, validation and remaining work. A commit is a
recovery point, not a completed batch or a reason to stop an authorized continuous run.
Downstream work may use an implemented prerequisite once its relevant checks pass;
unimplemented or failing prerequisites must remain explicit. Finish the complete
accepted outcome and its delivery checks before reporting completion.

The completed Q-W contract retains its original 280 rows and historical per-batch
commits. Future work need not inherit that scheduling model. This policy does not
authorize publication, pushing or an otherwise unrequested external action.

## Validate according to the change

Choose and record applicable checks before claiming completion. These are defaults,
not exemptions from a known affected contract or a user-required acceptance test.

| Change | Development and focused completion checks |
|---|---|
| Documentation only | Local links, affected examples/fences, consistency and whitespace; no native rebuild solely for prose |
| Build/verification tooling | Script syntax plus meaningful positive and negative fixtures for changed behavior; exercise the affected entry point |
| Managed behavior | Affected project build, focused semantics/error/lifetime tests and affected integration; API/consumer checks if public contracts or packaging change |
| Native implementation or ABI/ownership | Affected native/managed builds, ABI/layout/export checks where changed, focused runtime/lifetime/error tests against the actual native configurations; affected real-file/driver scenarios |
| Generator/type-map/discovery | Rule regressions, regenerate, compile generated native/managed outputs, determinism/freshness, affected runtime/lifetime behavior and coverage accounting |

At a product/binding delivery checkpoint, retain the full Release/Debug builds and
Generator/Runtime suites, actual Debug-native runtime, clean regeneration, dependency
and compatibility checks, applicable integration and clean package consumers, and the
local release check. Required failures or NOT RUN gates keep that delivery incomplete.
Intermediate commits and documentation/tooling-only maintenance are not product releases.
The existing full build/release scripts keep their checks; this policy changes when
they are invoked, not their success criteria.

Use repeated stress runs for concurrency, disposal, intermittent native failures and
driver instability when evidence warrants them. There is no universal ten-repeat quota;
record the chosen repetitions and results. A later failure invalidates an earlier clean
run for the affected behavior until investigated and fixed.

Reuse evidence only for unchanged relevant source, tools, SDK and runtime identities,
with its baseline stated. Repack and verify content when packaged documents change at
the next package delivery; a documentation-only edit need not immediately rebuild the
product or replace the last validated package. Never present an old package as containing
new documentation. Report only checks actually run; use NOT RUN for unexecuted checks.

## Source size and architectural decisions

Keep cohesive native responsibilities, explicit independent compilation and unique
registry/error owners. The default implementation size threshold remains 1,000 lines.
A cohesive exception may be added to
`OcctSharp/config/native-source-layout-exceptions.json` with an exact native-root-relative
`.cpp` path, a finite `maxLines` greater than 1,000, and a nonblank `reason` explaining
why splitting would harm cohesion. Include it in the same change as the source. The
verifier reports its use and still rejects unlisted/duplicate paths, missing rationale,
an exceeded bound and all structural violations. No global disable switch is provided.
A routine size exception within scope needs neither a new ADR nor repeated permission.

Use an ADR for changes to public architecture, ABI/ownership categories, module
boundaries or repository-wide policy. Routine helper extraction, a new source file,
tests and implementation choices within accepted contracts need no separate ADR.
Manual binding exceptions remain traceable in SPECIAL_CASES; ownership contract changes
remain recorded in OWNERSHIP. Related exact IDs may share one coherent exception record.

## Keep current facts separate from history

STATUS is the single current progress summary: product baseline, current work, coverage,
validation references and next action. Move older status narratives to linked historical
documents rather than appending another competing current state. ROADMAP owns priorities;
topic documents own contracts and limitations; batch records own their acceptance evidence.
Update only documents whose facts changed. Do not copy each new test count and hash into
every topic. Historical numeric evidence stays historical and is not silently rewritten.

## Invariants

Fix generators and regenerate instead of maintaining patches in generated output.
Preserve explicit ownership, matching allocation/release, parent/thread restrictions,
C ABI exception containment, safe type mapping and the single native bridge until an
accepted cross-DLL lifetime design is validated. Keep exact coverage accounting and
truthful compile/runtime/package evidence. None of these are waived by smaller tasks,
intermediate commits or proportionate validation.

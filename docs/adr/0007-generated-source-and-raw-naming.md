# ADR-0007: Commit Generated Source and Separate Raw Naming

- Status: Accepted
- Date: 2026-08-21

## Context

The first generated native and managed binding makes the previously pending naming
and generated-output policies structural. Generated code must remain traceable to the
canonical model, easy to review during an OCCT upgrade, and clearly separated from the
manually curated public API. A clean checkout must also compile without requiring a
developer to infer which generated files belong in source control.

## Decision

- Commit deterministic generated native source, generated managed raw source, and the
  generated ownership manifest.
- Put generated native files under `OcctSharp/src/OcctSharp.Native/generated/` and
  prefix their exported symbols with `occtsharp_generated_`.
- Put generated managed raw bindings in the internal `OcctSharp.Generated` namespace
  under `OcctSharp/src/OcctSharp/Generated/`.
- Keep the friendly public API manually curated in the `OcctSharp` namespace. Raw
  generated names do not establish public friendly API names.
- Generate through an isolated staging directory, verify the staged hashes, and remove
  stale output only when the previous generated manifest owns that path.
- Never hand-edit generated output as a lasting change. Modify model, transformation,
  mapping, naming, or emitter rules and regenerate.
- Do not commit transient staging directories or bulk discovery output. Commit a
  generated report only when a later policy explicitly selects it as a review baseline.

## Alternatives

- Ignoring generated source and regenerating it only during local builds was rejected
  because it makes upgrade diffs and clean-checkout review less direct and depends on a
  configured OCCT/parser environment before compilation.
- Exposing generated declarations directly as the public `OcctSharp` API was rejected
  because native traceability names and intentional .NET workflow names serve different
  purposes and evolve under different compatibility rules.
- Deleting every file in a generated directory was rejected because it could remove a
  file that was not produced by the current or previous manifest.

## Consequences

- Pull requests include deterministic generated diffs and `generated/manifest.json`.
- The build regenerates before compiling, and `eng/verify-generated.ps1` detects an
  untracked or stale generated set.
- Internal raw naming can evolve before a public package release, while friendly API
  compatibility remains deliberate.
- Generated/manual separation is visible in both native and managed source layout.

## Validation

- Generator tests must cover deterministic emission and manifest-owned stale cleanup.
- Debug and Release builds must compile the generated native and managed sources.
- At least one runtime test must cross the generated ABI.
- A regeneration check must finish with no generated Git diff.

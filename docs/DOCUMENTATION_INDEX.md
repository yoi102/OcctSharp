# Documentation Index

This directory is the authoritative documentation area for OcctSharp. Documents
describe either accepted rules, current project state, or explicitly unresolved
decisions. Recommendations are not accepted decisions unless an ADR says so.

## Start here

- [Architecture](ARCHITECTURE.md) — system boundaries and component ownership.
- [Repository layout](REPOSITORY_LAYOUT.md) — physical separation of documentation
  and code-related files.
- [Roadmap](ROADMAP.md) — staged implementation plan and exit criteria.
- [Complete migration plan](MIGRATION_PLAN.md) — full OCCT batches, module/package
  boundaries, metrics, and per-batch gates.
- [AI migration loop prompt](AI_MIGRATION_LOOP_PROMPT.md) — reusable re-entrant prompt
  for safely executing and polling one migration work unit at a time.
- [Status](STATUS.md) — current phase, blockers, and last verified results.
- [Decisions](DECISIONS.md) — accepted and pending architecture decisions.
- [Build and release](BUILD_AND_RELEASE.md) — current .NET 10/OCCT environment,
  build commands, validated scope, package requirements, and release gates.
- [NuGet packaging](NUGET_PACKAGING.md) — package contents, `occt` output layout,
  automatic native loading, and clean-consumer verification.
- [Optional integrations](OPTIONAL_INTEGRATIONS.md) — IVtk/VTK, OpenGL ES, Draw,
  C++/CLI, and platform-adapter availability and package boundaries.
- [Full inventory classification](FULL_INVENTORY_CLASSIFICATION.md) — B19 declaration
  and header dispositions, reason codes, and complete-versus-generated metrics.
- [Alpha.40 release notes](RELEASE_NOTES_0.1.0_ALPHA_40.md) — current locally validated scope,
  B19.2 package-expanded StepBasic coverage, native bootstrap, and publication blockers.
- [Alpha.39 release notes](RELEASE_NOTES_0.1.0_ALPHA_39.md) — prior B19.1 generated
  StepBasic/enum scope and evidence.
- [Alpha.38 release notes](RELEASE_NOTES_0.1.0_ALPHA_38.md) — prior locally validated scope,
  evidence, and publication blockers for the visualization-core prerelease.
- [Third-party notices](THIRD_PARTY_NOTICES.md) — recorded OCCT terms and unresolved
  native redistribution review items.
- [Console samples](SAMPLES.md) — entity creation, STEP/STL/IGES output, and
  transformed multi-STEP assembly commands.

## Generator and interop

- [Generation pipeline](GENERATION_PIPELINE.md) — reproducible input, model,
  generation, diff, and validation flow.
- [Native ABI](NATIVE_ABI.md) — C ABI boundary rules.
- [Ownership](OWNERSHIP.md) — lifetime and resource ownership rules.
- [Type mapping](TYPE_MAPPING.md) — native-to-managed mapping policy.
- [Special cases](SPECIAL_CASES.md) — manual exceptions to generated behavior.

## Delivery and quality

- [Dependency management](DEPENDENCY_MANAGEMENT.md) — OCCT and toolchain pinning.
- [Compatibility](COMPATIBILITY.md) — supported and planned platform matrix.
- [Versioning](VERSIONING.md) — SDK, generator, ABI, and OCCT version relationships.
- [Testing](TESTING.md) — validation layers and evidence requirements.
- [Known issues](KNOWN_ISSUES.md) — tracked open and resolved limitations.

## Background documents

- [Development guide](OcctSharp_%E5%BC%80%E5%8F%91%E5%AE%9E%E6%96%BD%E6%8C%87%E5%8D%97.md)
  contains the original broad implementation guidance.
- [Detailed AI instructions](AI_INSTRUCTIONS_OCCT_NET.md) contains the original
  long-form agent rules.

The background documents are useful context, but topic documents and accepted ADRs
are the maintainable sources of truth.

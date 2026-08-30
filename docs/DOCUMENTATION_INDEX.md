# Documentation Index

This directory is the authoritative documentation area for OcctSharp. Documents
describe either accepted rules, current project state, or explicitly unresolved
decisions. Recommendations are not accepted decisions unless an ADR says so.

## Start here

- [Architecture](ARCHITECTURE.md) — system boundaries and component ownership.
- [Repository layout](REPOSITORY_LAYOUT.md) — physical separation of documentation
  and code-related files.
- [Roadmap](ROADMAP.md) — completed Batch D/Batch E outcomes and historical phases.
- [Complete migration plan](MIGRATION_PLAN.md) — completed Batch C, Batch D, and Batch E
  matrices, large-wave rules, module/package boundaries, metrics,
  and product-scale gates.
- [Batch D viewport gap inventory](BATCH_D_VIEWPORT_GAP_INVENTORY.md) — locked
  24-capability production viewport/model-review denominator, OCCT root audit, ownership
  closure, non-goals, and validation gates.
- [Batch E inspection and PMI gap inventory](BATCH_E_INSPECTION_PMI_GAP_INVENTORY.md)
  — locked 24-capability engineering-inspection, measurement, PMI/AP242, annotation,
  saved-view, and screenshot denominator completed at 24/24.
- [Batch F freeform authoring gap inventory](BATCH_F_FREEFORM_AUTHORING_GAP_INVENTORY.md)
  — completed 24-capability Bezier/B-spline definition, surface/profile topology authoring,
  STEP/XDE, and viewer-evidence denominator.
- [Batch G technical drawing gap inventory](BATCH_G_TECHNICAL_DRAWING_GAP_INVENTORY.md)
  — completed 24-capability exact/polygonal HLR, section, copied-polyline, layered-SVG,
  standard-view, STEP/XDE, and real-HWND denominator.
- [Batch H advanced mesh and scene gap inventory](BATCH_H_ADVANCED_MESH_SCENE_GAP_INVENTORY.md)
  — locked 24-capability grouped-mesh, material, LOD, XDE-scene, interchange, and
  real-HWND denominator.
- [AI migration loop prompt](AI_MIGRATION_LOOP_PROMPT.md) — reusable re-entrant prompt
  for executing the largest coherent common-workflow wave instead of per-class tasks.
- [Status](STATUS.md) — current phase, blockers, and last verified results.
- [Decisions](DECISIONS.md) — accepted and pending architecture decisions.
- [Build and release](BUILD_AND_RELEASE.md) — current .NET 10/OCCT environment,
  build commands, validated scope, package requirements, and release gates.
- [NuGet packaging](NUGET_PACKAGING.md) — package contents, `occt` output layout,
  automatic native loading, and clean-consumer verification.
- [Optional integrations](OPTIONAL_INTEGRATIONS.md) — IVtk/VTK, OpenGL ES, Draw,
  C++/CLI, and platform-adapter availability and package boundaries.
- [Full inventory classification](FULL_INVENTORY_CLASSIFICATION.md) — complete declaration
  and header dispositions, reason codes, and complete-versus-generated metrics.
- [Preview.4 release notes](RELEASE_NOTES_8.0.1_PREVIEW_4.md) — complete Batch G technical
  drawing, HLR, section, SVG, standard-view, real-HWND, and local release evidence.
- [Preview.3 release notes](RELEASE_NOTES_8.0.1_PREVIEW_3.md) — complete Batch F freeform
  curve/surface, profile-to-solid, STEP/XDE, real-HWND, and local release evidence.
- [Preview.2 release notes](RELEASE_NOTES_8.0.1_PREVIEW_2.md) — complete Batch E exact
  inspection, PMI/AP242, saved-view, viewer-annotation, and local release evidence.
- [Preview.1 release notes](RELEASE_NOTES_8.0.1_PREVIEW_1.md) — OCCT-aligned NuGet
  version transition, inherited Batch D evidence, and Batch E preparation boundary.
- [Alpha.55 release notes](RELEASE_NOTES_0.1.0_ALPHA_55.md) — complete Batch D
  production viewport/model-review workflow and local completion evidence.
- [Alpha.54 release notes](RELEASE_NOTES_0.1.0_ALPHA_54.md) — final Batch C selective
  STEP session, geometry/topology edit, viewer selection/input, and local completion evidence.
- [Alpha.53 release notes](RELEASE_NOTES_0.1.0_ALPHA_53.md) — XCAF validation
  properties, recursive XDE occurrences, and explicit STEPCAF options.
- [Alpha.52 release notes](RELEASE_NOTES_0.1.0_ALPHA_52.md) — STEP import diagnostics,
  detailed validation/repair comparison, and viewer mouse rotation.
- [Alpha.51 release notes](RELEASE_NOTES_0.1.0_ALPHA_51.md) — first Batch C common API
  workflow across topology, BREP, detailed mesh, XDE, and viewer controls.
- [Alpha.50 release notes](RELEASE_NOTES_0.1.0_ALPHA_50.md) — MIT licensing,
  committed Windows x64 runtime, notices, and clone-and-run smoke.
- [Alpha.49 release notes](RELEASE_NOTES_0.1.0_ALPHA_49.md) — completed Batch B
  long-tail generation, classification, and local release evidence.
- [Alpha.48 release notes](RELEASE_NOTES_0.1.0_ALPHA_48.md) — current IGES entity
  expansion, final inventory counts, and complete local evidence.
- [Alpha.47 release notes](RELEASE_NOTES_0.1.0_ALPHA_47.md) — prior extended STEP
  entity expansion and complete local evidence.
- [Alpha.46 release notes](RELEASE_NOTES_0.1.0_ALPHA_46.md) — prior cross-generated
  shared-handle expansion and complete local evidence.
- [Alpha.45 release notes](RELEASE_NOTES_0.1.0_ALPHA_45.md) — prior generated STEP
  geometry/representation/shape/visual expansion and complete local evidence.
- [Alpha.44 release notes](RELEASE_NOTES_0.1.0_ALPHA_44.md) — prior generated
  mesh/Poly/analysis/healing expansion and abstract-record safety.
- [Alpha.43 release notes](RELEASE_NOTES_0.1.0_ALPHA_43.md) — prior generated
  Geom/Geom2d expansion, package evidence, and remaining migration gates.
- [Alpha.42 release notes](RELEASE_NOTES_0.1.0_ALPHA_42.md) — prior geometry/topology,
  composable XDE, package evidence, and remaining migration gates.
- [Alpha.41 release notes](RELEASE_NOTES_0.1.0_ALPHA_41.md) — prior common-modeling,
  manual-stable-ID accounting, package evidence, and publication blockers.
- [Alpha.40 release notes](RELEASE_NOTES_0.1.0_ALPHA_40.md) — prior package-expanded
  StepBasic coverage and repository-native bootstrap evidence.
- [Alpha.39 release notes](RELEASE_NOTES_0.1.0_ALPHA_39.md) — prior generated
  StepBasic/enum scope and evidence.
- [Alpha.38 release notes](RELEASE_NOTES_0.1.0_ALPHA_38.md) — prior locally validated scope,
  evidence, and publication blockers for the visualization-core prerelease.
- [Third-party notices](THIRD_PARTY_NOTICES.md) — recorded OCCT and bundled runtime
  component licenses, versions, DLL mappings, and provenance boundary.
- [Console samples](SAMPLES.md) — entity creation, STEP/STL/IGES output, and
  transformed multi-STEP assembly commands.

## Generator and interop

- [Generation pipeline](GENERATION_PIPELINE.md) — reproducible input, model,
  generation, diff, and validation flow.
- [Generated shard dependency closure](adr/0062-generated-shard-dependency-closure.md)
  — resolved cross-shard graph evidence and the decision to defer physical managed-project
  and native-DLL splitting.
- [Batch D production viewport decision](adr/0064-production-cad-viewport-review-batch.md)
  — copied occurrence/detection state, parent-bound viewer resources, one large wave,
  and durable screenshot evidence.
- [OCCT-aligned preview version decision](adr/0065-occt-aligned-nuget-preview-version.md)
  — package numeric core, preview counter, and independent assembly/ABI identities.
- [Batch E inspection/PMI decision](adr/0066-engineering-inspection-measurement-pmi-batch.md)
  — one large cross-family engineering-inspection, measurement, PMI/AP242 wave.
- [Batch F freeform authoring decision](adr/0067-freeform-curve-surface-authoring-batch.md)
  — one large cross-family curve/surface definition-to-profile-to-solid wave.
- [Batch G technical drawing decision](adr/0068-technical-drawing-hidden-line-vector-output-batch.md)
  — one large cross-family hidden-line/section/vector-output wave.
- [Batch H advanced mesh and scene decision](adr/0069-advanced-mesh-scene-material-interchange-batch.md)
  — one large cross-family mesh/material/LOD/scene/interchange wave.
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

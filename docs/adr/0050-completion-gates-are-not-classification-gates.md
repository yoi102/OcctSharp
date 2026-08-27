# ADR-0050: Completion gates are not classification gates

- Status: Superseded in part by ADR-0054
- Date: 2026-08-23
- Scope: B19/B20 migration completion integrity
- Supersedes: ADR-0049 only where it allowed B20 batch closure before all migration gates

## Context

The full inventory assigns a deterministic disposition to 116,214 discovered declarations,
but 10,486 are `SupportedUnselected` and 78,418 remain blocked by broad projection or
ownership rules. The generated manifest contains 58 stable IDs. Classification
completeness therefore proves that work is accounted for, not that bindable declarations
are generated or that unknown ownership/type projections are resolved.

The migration loop requires every bindable declaration to be generated or represented by
an accepted manual binding, no safety-critical unknown projection, and all declared
completion gates to pass. It explicitly forbids closing a batch while an exit criterion is
`FAIL`, `NOT RUN`, or silently omitted.

## Decision

The long-tail binding work remains in progress after classification. Its next large
workstreams must turn `SupportedUnselected` declarations into emitted bindings and
replace broad LT001-LT004 blockers with implemented projection/ownership rules or narrow
accepted reasons.

Release engineering is implemented but the complete migration remains open. The machine-readable
gate report exposes `releaseEngineeringImplemented: true` separately from
`batchImplementationComplete: false`, and includes a blocking
`bindable-emission-completeness` gate while eligible declarations remain unselected.

ADR-0054 supersedes the 19/21 numbered-batch statement: every former B00-B20 item belongs
to the single product-scale batch B. Classification percentage, emitted binding coverage,
engineering implementation, B completion, and publication readiness remain independent
metrics.

## Consequences

- A stable blocked disposition is useful evidence but cannot convert missing generator
  functionality into completed migration.
- Local release scripts, SBOM, provenance, and CI configuration remain valid release work and
  do not need to be discarded.
- New work uses large coherent package/ownership workstreams inside B, with generated-
  source, build, runtime, package, and documentation evidence. Those workstreams are not
  batches and have no numbered or dotted B identifiers.
- `LOOP_STATE=COMPLETE` is forbidden until the complete-migration gates in the migration
  prompt pass; public upload still requires explicit authority.

## Validation required

- Release gate JSON must report the separate implementation/completion facts.
- Status, roadmap, migration plan, generated manifest, and coverage reports must agree.
- Every material B workstream must increase emitted/manual coverage or resolve a real blocker; a
  reclassification-only change is not sufficient.

# ADR-0060: Make Batch C one common-CAD-API product batch

- Status: Accepted
- Date: 2026-08-27
- Scope: Post-B API migration priority, work sizing, and validation cadence

## Context

Batch B established a broad generated surface and safe runtime, but its history was
organized around many narrow capability milestones, individual ownership probes, and
declaration-count waves. Even after those labels were retired, the documents still make
it easy to select one class, one overload, or one small facade at a time. That produces
correct local results but does not deliver common CAD workflows quickly enough.

The next migration program must prioritize APIs that ordinary CAD applications use
together: geometry construction and evaluation, topology creation/query/editing,
modeling algorithms, meshing, data exchange and XDE documents, and interactive viewing.
These areas share dependencies and should be migrated as product workflows rather than
as isolated OCCT classes.

## Decision

Create one product-scale Batch `C`, named **Common CAD API Expansion**. Batch C is not a
catch-all for every remaining OCCT declaration. Its first objective is a coherent,
friendly, runtime-validated API for the high-frequency Windows core workflow from model
creation through inspection, modification, mesh/exchange, metadata, and display.

Batch C has three coverage lanes that advance together; they are not sub-batches,
completion milestones, or commit boundaries:

1. **Model and inspect** — `gp`, `Geom`, `Geom2d`, `GeomAPI`, `GCPnts`, `TopoDS`,
   `TopExp`, `BRep`, `BRepTools`, `BRepAdaptor`, `BRepGProp`, `BRepBndLib`,
   `BRepExtrema`, and validation/query utilities.
2. **Build, modify, and deliver** — `BRepBuilderAPI`, `BRepPrimAPI`, `BRepAlgoAPI`,
   `BOPAlgo`, `BRepFilletAPI`, `BRepOffsetAPI`, `BRepFeat`, `ShapeFix`,
   `ShapeUpgrade`, `BRepMesh`, `Poly`, common exchange providers, and OCAF/XDE
   document/assembly metadata.
3. **Present and interact** — the common `AIS`, `V3d`, `Prs3d`, `Graphic3d`, and
   `SelectMgr` operations needed for display, appearance, camera control, detection,
   selection, and application-owned window input.

An implementation wave must normally cross at least three connected API families and
finish at least one end-to-end user workflow. A single type or convenience method is
folded into the active wave. It may be isolated only when it is a demonstrated blocker
for multiple common workflows. Labels such as `C01`, `C.1`, per-class batches, and
method-count milestones are forbidden.

Prioritization is based on common workflow value and generalization leverage, not raw
declaration count. A parser/type-map/ownership/emitter rule that safely unlocks several
common families outranks a hand-written wrapper for one easy class. Low-frequency
schema entities, Draw/test packages, IVtk, C++/CLI, OpenGL ES, platform-specific backends,
and allocator/compiler infrastructure do not displace the common Windows-core lanes.

Use focused compile/runtime tests while a large wave is being built. Run the expensive
full Release/Debug, freshness, clean regeneration, package-consumer, inventory, and
release evidence once at the coherent wave checkpoint, and again at Batch C completion;
do not rerun the complete release pipeline after every method or small group of types.

## Batch C completion contract

Batch C is complete only when all high-frequency workflows declared in
`MIGRATION_PLAN.md` have intentional friendly/raw boundaries and pass their required
runtime, ownership, failure, integration, and package tests. It is not completed by:

- generating a large number of unrelated declarations;
- finishing one coverage lane while the other common workflows remain unusable;
- compiling without runtime/lifetime validation;
- reclassifying common APIs as blocked merely to make the denominator disappear; or
- publishing several small commits and calling them batches.

## Alternatives considered

- Continuing unnumbered but small per-class workstreams was rejected because removing
  labels alone did not increase delivery size.
- Migrating every remaining blocked declaration before common workflows was rejected
  because it prioritizes OCCT breadth over practical SDK usefulness.
- Building only friendly manual facades was rejected because it would duplicate native
  binding work and weaken regeneration/upgrade behavior.
- Splitting modeling, exchange, XDE, mesh, and visualization into separate letter
  batches was rejected because ordinary CAD applications require these areas together.

## Consequences

- The active planning unit becomes Batch C, while Batch B remains closed historical
  evidence.
- Status is reported by end-to-end workflow readiness and validated API-family coverage,
  not by counts of tiny tasks.
- Large waves may take longer between commits, but each checkpoint delivers a materially
  broader usable surface and amortizes the full release-validation cost.
- Optional/cold modules stay classified and auditable without controlling the execution
  order.

## Validation required

- `STATUS.md`, `ROADMAP.md`, `MIGRATION_PLAN.md`, and the reusable AI loop agree that C
  is the active whole-letter batch and contain no active numbered/dotted C fragments.
- Each selected wave names its connected API families and end-to-end workflows before
  implementation.
- Each completed wave records focused evidence plus one complete checkpoint validation.
- Batch C cannot reach complete while a declared common workflow is missing, unsafe,
  compile-only, or untested at its required integration level.

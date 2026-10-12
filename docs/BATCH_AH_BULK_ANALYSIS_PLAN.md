# Batch AH: bulk analysis, conversion and approximation plan

AH starts after the AG local delivery checkpoint. Its
[184 frozen audit candidates](BATCH_AH_BULK_ANALYSIS_CANDIDATES.json) are bound to
the validated Preview.31 exit inventory, with exact stable IDs and signatures. There are
138 BSplCLib, 36 BSplSLib, five GeomConvert and five Geom2dConvert candidates.
These are candidates, not an admitted declaration count. STATUS records the
validated baseline; WORKFLOW defines the delivery requirements.

## Intended result

Deliver one coherent workflow: inspect a spline's knot and pole relationships,
prepare and perform a bulk edit or conversion, evaluate the result and measure
sampled position/derivative error, then compose an independent edge or face and
roundtrip exchange. Exercise curves in both dimensions and surface grids together.
Keep related operations in this batch when their ownership and size contracts can
be proved; do not stop after a small set of easy scalar operations.

## Execution order within one batch

| Stage | Candidate families and work | Admission evidence |
|---|---|---|
| 1. Size and storage contracts | BSplCLib knot/pole counts, knot analysis, parameter location, preparation for insertion/trimming/unperiodizing; BSplSLib grid/pole relationships | Audit the pinned implementations and symbols; derive checked output formulas, exact bounds, degree/period relationships and aliasing rules before allocating or calling. |
| 2. Analysis and evaluation | Basis, D0-DN, resolution and rationality; curve/surface cache and coefficient operations only where their dimensions can be proved | Prove complete parameter domains and output shapes, rational weight constraints, derivative order limits and cache lifetime. Compare sampled results with the corresponding owning geometry evaluator. |
| 3. Bulk edits and reconstruction | Knot insertion/removal, degree increase, trimming, unperiodizing, pole operations and supported interpolation | Use Stage 1 sizes and owning copies. Specify failure publication and receiver mutation separately. Validate geometric invariance for representation edits and tolerance/error for approximation operations. |
| 4. Conversion and concatenation | GeomConvert and Geom2dConvert C0-to-C1 splitting and C1/G1 concatenation, including returned curve collections | First accept typed handle-element ownership, null/empty policy, output capacities and independent result lifetimes. Verify piece order, junction continuity, reversed inputs and periodic joins. |
| 5. Whole workflow delivery | Shared public-only sampled-error workflow, owning edge/face construction and STEP exchange | Run the same workflow in Runtime tests, the facade package consumer and direct module consumer; finish all local delivery gates before expanding to AI. |

Stage 4 may require a narrow accepted ownership extension beyond AG's numeric/gp
copies. Record it in OWNERSHIP and an ADR before emission. Include only the
supporting owner/result bindings needed to complete this workflow. Keep arbitrary
template mapping closed and retain borrowed/static addresses as blocked unless an
independent copy contract is proved.

## Scope freeze and accounting

Produce a row-by-row admission table before implementing each related family:
stable ID, exact signature, pinned source/symbol evidence, input and output sizes,
copy timing, aliasing, retention, error semantics and consumer use. Reject or defer
unproved raw pointer/cache/matrix layouts with a specific reason. Do not count
supporting Manual entries as newly migrated declarations. Refresh the full
inventory and reconcile every changed disposition against the frozen AG baseline.

The broader candidate set includes low-level matrix, banded-system, evaluator and
cache routines. A candidate is admitted only when its complete layout and useful
caller workflow are established; being present in the list does not make it safe
to expose. A rejected candidate does not shrink the intended end-to-end workflow:
use a proved owning operation when it supplies the same necessary capability.

## Acceptance

Cover nonuniform/rational/periodic curve and surface cases, shifted copied bounds
where allowed, dimensional mismatch, empty/null rules, finite-value and overflow
checks, raw ABI output capacities, aliasing, wrong/stale tokens, independent
results, GC and concurrent disposal. Test tolerance failure and success-only output
publication with actual invalid inputs, without asserting undocumented rollback.

Require full Release/Debug builds and Generator/Runtime suites, isolated actual
Debug-native tests, clean source regeneration, unchanged old output/API/export
compatibility, dependency closure, strict ABI headers and lifetime checks, exact
inventory, 14 packages, both clean consumers, payload/document/provenance/checksum
identity and the unified local release check. Report external CI, visual/human
acceptance, signing and publication separately, using NOT RUN when unexecuted.

AI then addresses topology and exchange collections with typed shape/entity
ownership, independent shape sets and history-to-repair/exchange workflows.

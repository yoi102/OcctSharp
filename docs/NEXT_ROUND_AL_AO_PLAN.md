# Next round: AL-AO after Preview.35

Planning baseline: 2026-10-06, locally delivered Preview.35 / ABI 1.79 / bridge 0.87.0.
[STATUS](STATUS.md) records the completed AF-AK round and main integration evidence.
This is a new four-batch plan, not additional unfinished work in the prior round.
All four batches are authorized for execution. **All four batches AL, AM, AN and AO are implemented and their local delivery
gates pass.** Final delivery evidence is tracked in
[STATUS](STATUS.md). This work does not alter the previously
validated Preview.35 package snapshot or authorize publication.

## Why this order

The current inventory contains 116,272 declarations: 26,322 Emitted, 1,218 Manual,
39,385 Blocked and 49,347 Skipped, with zero Pending. These are classification
counts, not a product-completion percentage. The largest Blocked groups are receiver
ownership (14,996), value construction (5,536), unmapped values (5,254), borrowed/output
references (5,049) and raw-pointer lifetime (3,572). Extend reusable contracts where
they close a useful workflow; do not turn every Blocked or Skipped row into a promise.

AH explicitly deferred six matrix/banded operations, six contact-order interpolation
operations and larger optional-buffer/workspace families. Its current owning geometry
APIs already cover many basic workflows. AK explicitly deferred seven writer-side
finder-process candidates while completing the reader workflow. These provide concrete
audit starting points; they do not prove demand for every low-level overload.

## Four batches

| Order | Complete intended workflow | Scope and admission boundary | Completion evidence |
|---|---|---|---|
| AL: copied matrices and private numeric solves | Parameters/knots -> basis matrix -> solve or least-squares result -> residual check -> independently owned spline/geometry | Audit copied dense/banded layouts, shifted row/column bounds, matrix snapshots and private Gauss/Householder/SVD receivers together. Reuse existing matrix bindings. Never expose work buffers or uninitialized result storage. | Exact residual/reference cases, singular/rank-deficient and invalid-dimension outcomes, source disposal, unchanged inputs, both consumers and the complete local delivery gate. |
| AM: spline workspaces and coupled outputs | Owned curve/surface data -> coefficient/cache/interpolation workspace -> evaluated or edited geometry -> sampled position/derivative checks -> CAD/STEP round trip | Build on AL where necessary. Re-audit AH's optional weight/multiplicity copies, scalar-looking references that actually address buffers, contact-order layouts and retained multiple outputs. Select a closed curve-and-surface workflow, not unrelated overloads. | Rational/nonrational and periodic/nonperiodic cases, exact workspace sizes, missing versus empty options, all-or-nothing output publication, independent lifetime and error bounds against existing owning evaluators. |
| AN: private writer transfer and export diagnostics | Shape set -> private STEP/IGES writer epoch -> immutable status/check/result snapshot -> output file -> independent readback | Start with AK's seven deferred writer-side candidates and audited writer prerequisites. Establish source-shape/model-entity index domains and prove exact mappings only where the SDK supplies evidence. Existing WriteStep/WriteIges behavior remains compatible. | Nonempty output, diagnostics and partial/failure outcomes, wrong-session/index rejection, input disposal, repeated inspection and real STEP/IGES consumer workflows. |
| AO: SDK blockers and reproducible delivery tooling | Reproducible header/symbol failure -> supported fix or precise exclusion -> stable inventory -> concurrent-safe local build and source-bound packages | Audit the 32 excluded headers and absent-symbol ledger. Resolve the observed concurrent native-bootstrap file lock and define a package input snapshot that detects concurrent document/source changes. Keep the pinned SDK/platform boundary unless a separately justified decision changes it. | Minimal positive/negative parser fixtures, exact exclusion deltas, export proof, concurrent bootstrap regression, package source/document identity, clean source build and applicable full delivery checks. |

The default execution order is AL -> AM -> AN -> AO. A prerequisite blocker discovered
during an earlier batch is resolved within that batch or recorded explicitly; it is
not hidden until AO. No concurrent agent or external project work is implied.
No Preview version is reserved until the admitted scope and baseline are frozen.

## Next batch AL: concrete audit input

[The frozen AL candidate inventory](BATCH_AL_NUMERIC_MATRICES_CANDIDATES.json) contains
**54 currently Blocked declarations**, selected from the fresh Preview.35 inventory:

| Family | Audit candidates |
|---|---:|
| math_Matrix | 23 |
| math_Gauss | 9 |
| math_Householder | 9 |
| math_SVD | 7 |
| BSplCLib matrix/banded operations deferred by AH | 6 |

The six BSplCLib rows are BuildBSpMatrix, EvalBsplineBasis and four SolveBandedSystem
overloads. The 25 already Emitted math_Matrix declarations are reused, not counted
as new migrations. The inventory binds exact stable IDs and source-report hashes;
its state is PLANNED_CANDIDATES_NOT_ADMITTED. It does not assert symbol availability,
safe construction or a final public API design.

AL execution must first inspect each selected signature and pinned implementation,
audit existing public matrix/solve coverage, and prove that the planned consumer
workflow gains a useful capability. Record every candidate as admitted or deferred
with concrete reasons; track new prerequisites and reused Manual/Emitted IDs separately.
Then document the closed matrix/solver ownership contract and any required ADR before
implementing the generator rules. Do not replace proven current APIs with duplicates.

The complete AL batch includes rule/emitter changes, generated native and managed
outputs, status/error results, lifetime tests, real geometry composition, both package
consumers and final delivery. Do not split those responsibilities into separately
claimed completed batches merely to increase the batch count.

## Deferred items and scope safeguards

- The 85 AK deferrals remain authoritative in its admission matrix. AN reconsiders
  the seven writer rows; it does not automatically admit the other 78 or guarantee
  admission of all seven. Broken SDK index/filter implementations, missing BRepCheck
  symbols and external mutable process/actor/context APIs remain excluded until proven.
- Public callbacks, arbitrary borrowed storage, zero-copy APIs and recursive native
  graph traversal need separate lifetime and execution contracts. AL/AM do not infer
  permission to expose these merely because a numeric algorithm can use them.
- AO starts with 19 missing-VTK headers, one EGL/GLES header, one RapidJSON header,
  one C++/CLI header and ten headers missing SDK-generated prerequisites. Inspection
  may confirm exclusions rather than eliminate them. New platform support or adding
  VTK to the runtime is not an implicit acceptance condition.
- Cadoryx A5 performance, physical input/display acceptance and whole downstream
  application acceptance remain separate. No application package is switched by
  this plan. Commit/push, hosted CI, signing and public publication are separate actions.

## Delivery and tracking

Each implementation batch follows [WORKFLOW](WORKFLOW.md): focused development checks,
then full Release/Debug suites, actual Debug-native identity, clean regeneration,
ABI/lifetime/API/export/module checks, fresh exact inventory, 14 packages, both clean
consumers and the unified local release check. Reuse unchanged evidence where allowed;
do not repeat full product gates solely for this planning-only document revision.

Record candidate/admitted/deferred/prerequisite/reused counts separately. Update STATUS
after material progress and keep completed input plans as historical records. Newly
selected candidate JSON must be wired into package documentation and audited at its
first delivery; the existing Preview.35 packages do not contain this new plan.

Current implementation tally: **AL-AO implemented and locally delivered / 0 remaining**.
The final package-delivery tally and evidence are maintained in [STATUS](STATUS.md).
Previous AF-AK tally: **6 delivered / 0 remaining**.

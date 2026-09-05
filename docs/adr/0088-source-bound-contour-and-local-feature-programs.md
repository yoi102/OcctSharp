# ADR-0088: Source-bound contour programs and explicit local-feature limits

- Status: Accepted and locally validated; final documentation-package/commit evidence in STATUS.
- Date: 2026-09-05.

## Context

Implement all forty original [U capabilities](../BATCH_U_ADVANCED_LOCAL_FEATURES_GAP_INVENTORY.md)
after the fully validated T commit `bfb8811`. Preserve the Preview.15 frozen roots and
record the post-T inventory and exact delta separately. Q budgets/source snapshots,
S immutable scalar laws and T atomic recipe execution are completed prerequisites.

## Decision

Keep the existing managed modules, facade and single Native DLL. Modeling owns copied
source-bound contour recipes, draft/limit plans, diagnostics and owning results. Native
contour finishing, chamfers, face/shell draft, limited prisms/sweeps, ribs/slots and holes
use cohesive independent translation units. Builders, section arrays and iterators
remain call-local. Reuse Shape and FeatureResult registration/release; private copied
result metadata must not create a new live registry or a borrowed native builder.

Selections belong to an immutable source identity/revision. Exact copy correspondence
preserves support and sliding relationships; foreign or stale selections reject before
an algorithm runs. Recipe replacement/removal always rebuilds from the original source.
No edit operates on an already filleted result unless explicitly supplied as a new source.
S laws remain copied definitions, never managed callbacks. Radius sampling domains are
normalized [0,1]; positive samples alone do not prove arbitrary interpolation positivity.

Algorithm completion, topology validity, partial output and protected acceptance are
separate states. Fillet BadShape is read only after HasResult, and only as a clearly
marked diagnostic owner. It never becomes the accepted root after a failed build.
Circle-section parameters describe the simulated circle's trim, not a fabricated spine
station. Patch/history maps name their exact supported source relation; unavailable
history remains explicit. All copied-array capacities are checked before writes.

## Checked OCCT semantics

- Chamfer distance-angle, throat and penetration programs use their actual OCCT modes
  and support-face orientation; dimensions are not interchangeable with ordinary setbacks.
- DraftAngle's final Add Boolean is not a general propagation-disable switch. Keep native
  propagation and implement explicit affected-face acceptance policies using ConnectedFaces
  and ModifiedFaces. Conflicting propagated programs reject; failed Add stops that builder.
  Angles below the kernel's effective threshold must not be advertised as applied changes.
- MakeDraft works on a wire, face or shell with free boundaries; an edge can explicitly
  become a one-edge wire. Surface and shape stop semantics remain distinct.
  Actual Debug-native testing exposed uncatchable SDK assertions in the cornered
  limit-driven sweep history. U limits that path to a single analytic line/circle
  boundary edge in both configurations. Multi-edge/cornered length-only drafts remain
  supported. U-22/23's positive fixtures therefore use measured analytic profiles;
  the former polygon-limit fixtures are retained as explicit pre-kernel rejections,
  not silently removed or represented as successfully supported. KI-034 records the
  evidence and restriction. MakeDraft Generated is queried only for source edges;
  vertex/face histories and base-class Modified/Deleted defaults are not advertised.
- BRepFeat profile/support/sliding membership and limiter modes are validated before
  construction. Kernel failure is reported, not retried as an unrelated global Boolean.
- MakeRevolutionForm's Height1/Height2 are linear thicknesses, not angles. Expose them
  as distances. Explicit optional angular clipping is a separate named construction
  stage on the successfully produced added/removed material only; preserve the base
  outside the angular interval, and disclose composed rather than exact kernel history.
  This supplies U-33's angular bounds without mislabelling SDK units or replacing its
  rib/slot builder. Tests must verify both additive and subtractive geometry.
- MakeDPrism UntilEnd takes the largest bounding-box dimension as a slanted
  generatrix length. A cube with nonzero draft can therefore stop short of the far
  cap and fail reconstruction. Keep the native failure diagnostic; do not substitute
  a Boolean. A 20x10x10 base verifies successful finite cuts, and the 10x10x10 failure
  has a separate negative regression. Drafted prism direction comes from the profile
  support geometry, not the ordinary prism Direction option.
- A single planar BRepFeat limiter may use its unbounded underlying surface. A
  remote trimmed patch in an intersecting plane is not necessarily unreachable.
  Tests use a parallel remote plane for true unreachable-limiter rejection.
- Revolved Until/FromUntil follows the SDK's selected base side, not necessarily
  the first positive-angle intersection. Tests independently measure the added
  material on the negative-Y side and require one connected solid.
- In the pinned SDK, LocOpe_Pipe converts an untrimmed Geom_Line to BSpline for
  limiter curves and can fail with `No such curve`. Limited pipe success is verified
  with a bounded Bezier spine; straight-line limit failure is disclosed and tested.
  Complete-spine mode and general S sweeps are separate contracts.

## Integration and ownership

### Pinned SDK radius-law defect

The first U runtime probe aborted with `0xC0000005` in fillet simulation after the
`SetRadius(Law_Function, ...)` path (`artifacts/batch-u-first-runtime.log`). Inspection
of OCCT 8.0.1 `ChFiDS_FilSpine::SetRadius(Law_Function, ...)` shows it clears the radius
sequence but appends the supplied law only to a temporary composite; it never retains
that composite. `SetLaw` after simulation is not a build fix either: Compute resets
stripes and clears their laws. Do not expose this unsafe setter as a working API.

U adapts S copied scalar laws to native radius sample programs, with normalized global
contour arc length mapped separately into each member edge. This remains law-driven
finishing but is explicitly interpolated, not exact arbitrary-function transfer.
Adaptive refinement checks a positive interpolation control hull and off-knot probes
against the requested law using the declared 3D approximation tolerance; failure to
meet that policy within bounded sample counts rejects. Copied contour diagnostics report
the measured probe error and sample count. This is not a rigorous global error bound.
Sampled recipes define a zero-end-derivative interpolation before edge-domain adaptation.
Tests must compare actual simulated radii and resulting geometry, not just IsDone.
Closed contours require matching authored value and first derivative at the seam.
Constant-program vertex radii may override endpoints; law/sample anchors must agree
with the authored law. A conflicting anchor rejects rather than replacing sampled
data and invalidating the reported approximation evidence.

The partial-result regression sews a four-sided off-center pyramid shell. Normal
2D tolerance .001 succeeds; deliberately coarse .1 approximation makes the actual
corner plate solver return HasResult with IsDone false. BadShape remains independently
owned and inspectable after source disposal, while RequireShape/acceptance reject.
No mock, forced native status or fabricated partial shape is involved.

Q tolerance/geometric-change checks are reused for U acceptance; protected topology
needs exact correspondence and unchanged geometry, never a nearest-neighbour guess.
T receives finite typed persisted finishing and limit recipes with explicit selector
context. Its evaluator remains in the facade; Documents gains no reverse orchestration
dependency. Failed/cancelled recompute preserves explicitly stale last-good results.
Occurrence-aware XDE publication and STEP/IGES/viewer review reuse the existing parent,
transaction, thread and metadata-conflict rules. Result/history shapes survive input,
plan and document disposal; viewer identities do not survive their parent/replacement.

## Validation and consequences

Keep all forty acceptance rows. Require numeric contour/chamfer/draft/limit/rib/hole
success evidence, meaningful failures and partial diagnostics, raw capacities, replay,
source isolation, protected rejection, persisted T reexecution, shared XDE definitions,
real STEP/IGES/HWND and both clean consumers. Also require complete Release/Debug and
actual Debug-native regression, repeated focused tests, strict standalone headers,
source/dependency closure, additive API/ABI and exact SC-058 accounting, regenerated
outputs, cold-source rebuild, runtime manifest and full local package/release checks.
The local version slot is Preview.20; ABI/bridge changes follow actual added contracts.
Only directly invoked new blocked overloads enter SC-058; no entire root is reclassified.

Complete one U local commit only after every gate, then V and W without reconfirmation.
No new project/DLL, hosted release, GitHub push or NuGet publication is authorized.

Related: ADR-0071, ADR-0074, ADR-0081, ADR-0083, ADR-0084, ADR-0086 and ADR-0087.

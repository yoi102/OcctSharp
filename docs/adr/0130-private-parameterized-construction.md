# ADR-0130: private finite parameterized construction

Status: Accepted within the user-authorized BL–BQ scope. Date: 2026-10-11.

## Decision

TM048 extends the existing closed-workflow constructor model to exact finite BL signatures.
Modeling owns its adapter in the existing single native bridge. Global type mapping and
manual dispositions are unchanged. Builders, geometric references and endpoint graphs
stay private; output carries native completion/error/range/orientation diagnostics.
Native failure exposes no partial topology. Shape queries return geometry-deep-copied owners.
Actual SDK orientation is reported: descending 3D intervals normalize to native forward,
while descending nonperiodic pcurve intervals use native reversed orientation; periodic
pcurves wrap forward across their seam. Endpoint sharing is
an exact IsSame relation within the private result, never source history inferred from counts.

Pcurve edges must retain their exact stored private surface representation and have a
complete independent 3D curve before publication. Parameter sampling uses that frozen
domain. No source topology history or automatic subshape rebinding is inferred.
Frames, intervals and copied data have explicit finite budgets; these are not computation timeouts.

## Verification

Source/symbol admission, actual native geometry/lifetime tests and the full per-batch
product-delivery gates remain required. Implementation status and frozen authority are
recorded in the BL topic and STATUS. Generation or compilation alone cannot close delivery.

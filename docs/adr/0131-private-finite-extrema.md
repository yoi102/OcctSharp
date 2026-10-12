# ADR-0131: finite extrema with coupled copied output

Status: Accepted within the user-authorized BL–BQ scope. Date: 2026-10-11.

## Decision

TM049 uses exact finite Extrema_ExtCC/ExtCS/ExtSS signatures in private initialized
solvers. Input geometry is checked before deep copying. Bounded adaptors outlive
the solver while coupled distance/point/parameter records are copied. Neither
solvers nor borrowed adaptors escape. The result privately retains independent
geometry for evaluation; public rows remain valid after result disposal.

Geometry owns computation; an additive generated Modeling adapter copies a 3D
edge curve with native location applied. This maintains Modeling → Geometry
without introducing a managed cycle or changing the single native bridge.

Native completion, parallel distance-only rows, empty results, four CC endpoint
combinations and single-solution configuration have separate meanings. Numerical
results do not imply complete/global extrema. Input, nesting and storage budgets
do not imply an execution timeout. No topology history is inferred.

## Verification

Exact source admission, generated-byte preservation, actual residual/lifetime and
ABI tests, and full local product-delivery gates remain required. The batch topic
and STATUS track implementation separately from frozen delivery authority.

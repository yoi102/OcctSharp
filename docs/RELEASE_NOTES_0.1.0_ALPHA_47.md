# OcctSharp 0.1.0-alpha.47

This local experimental package expands common STEP entity coverage inside the single
migration batch B. It is not a public-release-readiness declaration.

## Identity

- Package: `OcctSharp.0.1.0-alpha.47.nupkg`.
- Target: .NET 10, Windows x64.
- OCCT baseline: 8.0.1.
- Native ABI: 1.39.
- Bridge implementation: 0.47.0.

## Additions

- Generated `StepAP203`, `StepAP214`, `StepAP242`, `StepDimTol`, `StepElement`,
  `StepFEA`, and `StepKinematics` entity families.
- 249 new public constructible wrappers and 841 additional emitted declaration IDs.
- Runtime construction/clone/RTTI/retention/disposal coverage for every new wrapper.
- Focused cross-package relationship and scalar-state tests.
- `StepData` remains discovered and classified but awaits a linkable infrastructure-
  specific ownership profile; no per-class blacklist was introduced.

## Coverage boundary

- Selected scope: 28,836 declarations.
- Emitted: 3,076 (10.6672%).
- Emitted plus 61 accepted manual declarations: 3,137 (10.8788%).
- Batch B remains in progress; broad supported-unselected and blocked surfaces remain.

## Validation

Complete local release evidence and the refreshed full-inventory classification are
recorded in `STATUS.md`. Public publication, signing, hosted CI execution, project
licensing, and final third-party legal review remain outside this local validation.

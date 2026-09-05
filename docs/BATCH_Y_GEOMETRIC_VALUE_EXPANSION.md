# Batch Y: Broad generated geometric value expansion

- State: implementation in progress under ADR-0093 and WORKFLOW.
- Entry: X `2cbe55e`, Preview.23; 19,682 generated + 1,217 manual IDs.
- Entry inventory: 116,272 IDs; Blocked 46,028; Skipped 49,345.
- Entry inventory SHA256: `B714207D4224DFAAD3A34CEBB9CFFE7353211779276AC084CA330A7F06A480DB`.
- Target: Preview.24 / ABI 1.68 / bridge 0.76.0.

The accepted scope is the reusable geometric-copy table and its cross-module generated
inputs/returns, compatible overload epochs, semantic and lifetime tests, exact accounting
and complete local product delivery. See [ADR-0093](adr/0093-generated-geometric-value-projections.md)
for the field-copy, domain and handedness contracts. There is no fixed declaration quota.
Arrays/output references and opaque transformation internals remain explicit later work.
Only actually emitted, compiled and validated bindings count as this batch's migration.

The previous X checkpoint is locally committed. Subsequent cohesive checkpoints are
authorized, but do not end this batch before its required delivery checks pass. Current
execution state and actual validation are maintained in [STATUS](STATUS.md).

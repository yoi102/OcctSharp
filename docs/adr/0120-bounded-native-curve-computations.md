# ADR-0120: bounded native curve computations

Status: Accepted
Date: 2026-10-10

## Decision

Extend the exact closed-workflow emitter with TM037 CurveSampling. SDK samplers
and solvers remain private initialized receivers, taking copied geometry.
Publish detached points/parameters/diagnostics through an immediately consumed
typed token. Preserve the existing single registry and C ABI error boundary.

Use native adaptor overrides to count generic SDK evaluation calls. Audit
GetType=OtherCurve paths from the pinned source; restrict source geometry complexity
before copying. Finite domains, one-period limits, evaluation and output budgets
are explicit. Unfinished calculations expose no partial point distribution.

Reuse existing projection declarations rather than recounting them as newly
migrated. Copy all candidates in the explicit domain and report ambiguity;
never infer endpoints, unique minima or topology history.

## Consequences

The public API intentionally covers controlled computations rather than an
owning general-purpose adaptor or borrowed Extrema object. Numeric tolerance
and SDK status remain observable. Source mutation requires caller synchronization.
Release/Debug, actual Debug-native, cold generation/build, ABI/API, lifetime,
two consumers and final payload/source freeze remain delivery requirements.

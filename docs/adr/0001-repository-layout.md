# ADR-0001: Separate Documentation and Code Workspace

- Status: Accepted
- Date: 2026-08-21

## Context

Project documentation must be versioned from the current repository root, while all
solution and code-related files must remain grouped under an inner `OcctSharp/`
directory.

## Decision

Initialize one Git repository at the outer root. Keep documentation in `docs/` and
all source, solution, tests, benchmarks, configuration, baselines, reports, and
packaging work under the inner `OcctSharp/` directory.

## Consequences

- Documentation and implementation are pushed together.
- The root remains easy to inspect.
- Build scripts must use paths rooted under the inner workspace.
- A nested Git repository is prohibited unless a later ADR explicitly introduces a
  submodule.

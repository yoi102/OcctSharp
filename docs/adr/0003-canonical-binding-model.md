# ADR-0003: Use a Canonical Binding Model

- Status: Accepted
- Date: 2026-08-21

## Context

Emitting managed code directly from a compiler AST couples parsing, policy, output,
and reporting. It also makes stable API diffs and alternative emitters difficult.

## Decision

Normalize compiler AST facts into a versioned canonical binding model. Native,
managed, manifest, coverage, and diagnostic emitters consume that model after ordered
transformation and validation passes.

## Consequences

- Parsing can evolve independently of emitters.
- Type, naming, ownership, and skip rules have one testable location.
- Model schema and stable symbol identity require explicit versioning.
- Generation fails when the model contains unresolved safety-critical semantics.

# ADR-0014: Batched Full OCCT Inventory

- Status: Accepted
- Date: 2026-08-21

## Context

The normal generation configuration intentionally selects a small dependency closure.
Its declaration count is useful for that scope but is not the denominator for all of
OCCT. Adding every public header to every build would make the normal edit/build loop
slow and fragile, while header counts alone do not measure semantic API coverage.

## Decision

- Keep normal discovery and generation scoped by `config/generation.json`.
- Add a separate `eng/inventory.ps1` workflow that catalogs every top-level `.h` and
  `.hxx` entry header in the pinned OCCT `inc` directory.
- Full semantic inventory parses deterministic header batches and deduplicates canonical
  declaration stable IDs across translation units.
- Versioned inventory preamble headers supply common OCCT declarations that toolkit PCH
  builds normally provide; they are configuration inputs rather than parser heuristics.
- If a batch fails, recursively split it in stable halves until the failing individual
  headers are isolated. Continue scanning unaffected headers and record each failure.
- A report is complete only when every catalogued header parsed successfully. Partial
  declaration totals are diagnostic evidence and must not be called the full-OCCT
  coverage denominator.
- Inventory reports are transient artifacts under `OcctSharp/artifacts/`; only the
  workflow, schema, tests, and summarized evidence are committed.
- The emitted numerator continues to come from the generated manifest. Engineering
  roadmap progress and binding coverage remain separate metrics.

## Alternatives considered

- Using the selected generation closure as the full denominator was rejected because
  it omits most OCCT entry headers.
- Including all headers in one translation unit was rejected because one incompatible
  or incomplete header would invalidate the whole inventory and create excessive peak
  memory use.
- Counting headers as declarations was rejected because headers contain different
  numbers and kinds of APIs and include substantial dependency closures.

## Consequences

- OCCT upgrades gain a reproducible inventory report with header/package totals,
  semantic declaration/support totals, successful batches, and isolated failures.
- The full scan is intentionally slower and is run as an explicit audit, not by normal
  builds.
- A prebuilt OCCT distribution with incomplete public headers can be identified without
  blocking all other discovery evidence, but it cannot yield a complete denominator.

## Validation

- Unit tests verify extension filtering, deterministic package grouping, batch failure
  isolation, and preservation of declarations from valid headers.
- The pinned OCCT 8.0.1 bundle must produce the catalog report; semantic scan completion
  and all isolated failures are recorded in `STATUS.md` after each audit.

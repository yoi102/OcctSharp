# ADR-0058: Use narrow long-tail dispositions and a local implementation gate

- Status: Accepted
- Date: 2026-08-27
- Scope: Batch B final declaration accounting and completion reporting

## Context

The complete inventory had zero pending declarations but retained 68,313 declarations
under LT001-LT004. Those buckets described where the initial value-copy assessment
stopped, not the actual ABI, export, ownership, or type obstacle. A completion flag based
only on zero `SupportedUnselected` could therefore become true while broad unknowns
remained.

The selected generator also left safe standalone enums unowned and did not model void
returns or any export-proven free-function profile. Conversely, Clang anonymous-enum
display names contain local paths and cannot be stable managed type identities.

## Decision

Emit every named Int32-compatible enum in the generated profile, accept TM000 void
returns, and generate only the Standard foundation free-function profile whose toolkit
and exact native exports have been validated. Automatic callable scopes match an exact
native name and declaring header when they could overlap an explicit scope.

Replace LT001-LT004 with structural dispositions: non-callable type metadata,
destructor/pure-virtual/abstract surfaces, anonymous enums, toolkit/export provenance,
non-transient receiver/value ownership, raw pointer/reference/rvalue lifetimes,
unselected handle targets, template instantiations, and unmapped value types each have a
stable narrow code. A blocked declaration is never counted as emitted.

`batchImplementationComplete` requires zero `SupportedUnselected`, zero LT001-LT004,
and PASS for the local Release/Debug, generated freshness, clean regeneration, package
consumer, API compatibility, full classification, SBOM/provenance, and CI-configuration
gates. Public project licensing, third-party legal review, hosted CI execution, package
signing, NuGet credentials, and publication authorization remain separate
`publicReleaseReady` gates.

## Consequences

- Classification completeness still does not imply generated coverage.
- Narrow blockers can be advanced by future whole-letter batches without inventing an
  unsafe pointer or ownership projection.
- Local Batch B implementation may complete while public release readiness remains
  false for external legal, credential, signing, or hosted-execution reasons.
- An OCCT upgrade must fail closed if supported-unselected, LT001-LT004, pending, HD099,
  anonymous-path-derived managed names, or unverified free-function exports reappear.

## Validation required

- Release and Debug native/managed builds plus generator/runtime tests.
- Deterministic generation/discovery and full manifest-aware inventory.
- Generated freshness, clean-source regeneration, clean alpha.49 package consumer, API
  compatibility diff, provenance/SBOM/checksums, and machine-readable gate report.

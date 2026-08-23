# ADR-0049: Reproducible release evidence and explicit publication gates

- Status: Superseded in part by ADR-0050
- Date: 2026-08-23
- Scope: B20 upgrade and release engineering

## Decision

OcctSharp release engineering uses one local entry point, `eng/release-check.ps1`, for
the declared .NET 10/Windows x64/OCCT 8.0.1 profile. The entry point rebuilds Release
and Debug, verifies generated freshness, validates the clean package consumer, rebuilds
the full inventory, performs a clean-source regeneration comparison, checks the public
managed API baseline, creates supply-chain evidence, and runs both Git whitespace checks.

The canonical managed API baseline schema is versioned independently as `1.0`. It stores
sorted public reflection signatures and assembly/framework identity. A removed signature
is a failing compatibility change; additions are reported but do not fail this initial
gate. The baseline is not a claim that all OCCT declarations are generated.

Release evidence consists of the API diff, CycloneDX SBOM, provenance record, explicit
gate report, and a fixed-order SHA256 checksum file covering those records and the NuGet
package. The checksum file is written only after the gate report, so repeated runs cannot
hash a stale report. CI uses the same release entry point after acquiring an OCCT archive
from a configured immutable URL and SHA256.

Gate states are `PASS`, `BLOCKED`, or `NOT RUN`. This ADR originally allowed B20 to close
when the release machinery existed even while other completion gates remained blocked.
ADR-0050 supersedes that closure rule: release-engineering implementation remains a
separate fact, but neither B19 nor B20 may close while bindable declarations are
unemitted or required completion gates are blocked/not run.

## Consequences

- OCCT upgrades regenerate and review the public API baseline, full inventory, package,
  SBOM, provenance, checksums, and release notes as one evidence set.
- The project license, exact third-party redistribution review, package signing, hosted
  CI execution, and NuGet publication cannot be inferred from a successful local run.
- The initial API snapshot does not cover behavioral compatibility, attributes, nullable
  annotations, generic constraints, native ABI exports, or every OCCT declaration; their
  existing validation and versioning gates remain independent.
- CI's full Windows job is intentionally skipped until repository variables
  `OCCT_ARTIFACT_URL` and `OCCT_ARTIFACT_SHA256` identify an approved archive.

## Validation

The local B20 run passed Release and Debug Generator 37/37 and Runtime 68/68, the 6/6
optional-dependency audit, 12-file byte-identical clean regeneration, the alpha.38 clean
consumer with 45 application-local native DLLs, a 606-signature API diff with zero
additions/removals, complete 116,214-declaration/7,090-header classification, JSON parsing
of all release records, and both Git whitespace checks. Hosted CI, signing, and NuGet
publication were not run. Public release readiness remains false.

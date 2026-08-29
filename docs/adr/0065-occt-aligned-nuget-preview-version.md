# ADR-0065: Align the NuGet version line with the pinned OCCT version

- Status: Accepted
- Date: 2026-08-29
- Scope: NuGet package identity and prerelease numbering

## Context

The historical package line used `0.1.0-alpha.N` while the supported upstream runtime is
OCCT 8.0.1. Consumers must inspect separate metadata to learn which OCCT line a package
targets. The project still needs prerelease sequencing because public hosted release,
signing, and publication gates are not complete.

## Decision

The core `OcctSharp` NuGet package uses the pinned OCCT semantic version as its three-part
version and a NuGet prerelease counter:

```text
<OCCT major>.<OCCT minor>.<OCCT patch>-preview.<OcctSharp preview number>
```

The first package under this rule is `8.0.1-preview.1`. The preview number increments for
package-visible OcctSharp changes while OCCT remains 8.0.1 and resets to `preview.1` when
the pinned OCCT three-part version changes. Stable `8.0.1` is reserved for a package that
passes the separately authorized public release gates.

This is a package-version policy only. Native ABI 1.46, bridge 0.54.0, generator/config
schemas, and OCCT build identity remain independent diagnostics. Managed assembly
identity stays `0.1.0.0` for compatibility; package and informational versions carry
`8.0.1-preview.1`.

## Consequences

- NuGet search/restore identity immediately communicates the OCCT compatibility line.
- `preview.N` orders prereleases using NuGet SemVer rules without implying public-release
  readiness.
- A package version match does not replace runtime manifest, native ABI, compiler/RID,
  or exact OCCT build checks.
- Historical `0.1.0-alpha.N` packages and release notes remain immutable.

## Validation required

- Pack and clean-consumer restore `OcctSharp.8.0.1-preview.1.nupkg`.
- Inspect the nuspec, managed assembly identity/informational version, runtime manifest,
  SBOM, provenance, and release gate package identity.
- Verify ABI 1.46/bridge 0.54.0 and the 62-DLL runtime are unchanged.
- Run API compatibility to prove the assembly-identity-preserving version rebase removes
  no public signatures.

## Related decisions

- ADR-0004: OCCT 8.0.1 and Windows x64 baseline.
- ADR-0008: initial package/runtime layout.
- ADR-0049: release evidence and publication gates.
- ADR-0059: committed runtime and MIT license.

## Current application

Preview.1 remains the historical first package under this policy. Batch E is the next
package-visible additive change, so the current package is `8.0.1-preview.2` while the
OCCT baseline remains 8.0.1 and managed assembly/file identity remains `0.1.0.0`.
Preview.2 independently advances the additive native ABI to 1.47 and bridge
implementation to 0.55.0; those identities do not derive from the package number.

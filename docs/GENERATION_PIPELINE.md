# Generation Pipeline

## Goal

For the same normalized OCCT input, generator version, configuration, and toolchain,
the pipeline must produce byte-stable source and semantically stable reports.

## Required inputs

Every generation run must identify:

- OCCT version and immutable source or binary identifier.
- Header root and toolkit/package inventory.
- OCCT build options that affect public declarations.
- Compiler family, version, target triple, and C++ language standard.
- Include paths and preprocessor definitions.
- Generator version or commit.
- Schema version for generator configuration and binding manifests.
- Module/package inclusion and exclusion rules.
- Type-map, ownership, naming, and manual-exception rules.

Machine-specific absolute paths are runtime inputs but must be normalized out of
committed output.

## Pipeline stages

1. **Resolve input** — validate the dependency lock, header tree, target, and config.
2. **Discover** — inventory headers, toolkits, packages, and public declarations.
3. **Parse** — build an AST using the exact compiler argument set.
4. **Normalize** — convert AST facts into the canonical binding model.
5. **Transform** — apply ordered type, ownership, naming, ABI, scope, and skip passes.
6. **Validate model** — reject unknown or contradictory ownership and ABI mappings.
7. **Emit native** — generate deterministic C ABI bridge source.
8. **Emit managed** — generate deterministic raw managed binding source.
9. **Emit reports** — write API manifest, coverage, diagnostics, and skip reasons.
10. **Stage output** — write to an isolated staging directory.
11. **Verify generation** — ensure every expected output is present and internally
    consistent.
12. **Replace output** — replace the previous generated set only after a successful
    run; remove stale generated files using the generated manifest.
13. **Build and test** — compile native and managed output, then run the configured
    validation layers.

Partial generation must not silently leave a mixture of old and new files.

## Initial support classification

The first ordered classification pass is implemented. It preserves bindable-looking
declarations as `Pending` until type and ownership passes prove support; it never
promotes a declaration to `Supported` from syntax alone. Declarations that cannot enter
the initial automatic path receive the first matching stable reason:

| Code | Category | Current meaning |
|---|---|---|
| `SK001` | Unavailable | Clang marks the declaration unavailable. |
| `SK002` | Deleted | The C++ declaration is deleted. |
| `SK003` | NonPublic | The declaration is private or protected. |
| `SK004` | Variadic | A general safe C ABI projection is not defined. |
| `SK005` | Template | An explicit template specialization rule is required. |
| `SK006` | Operator | Operator overload projection is not implemented. |

Rule order is part of deterministic reporting: a declaration with multiple blocking
facts receives the earliest applicable reason. Later passes may add reason codes but
must not renumber or silently redefine an existing code.

The following simple-binding eligibility pass now promotes only value-copy constructors
and static methods whose complete parameter and return projections are known to the
central TypeMap. Instance receivers, pointer/reference returns, and any unknown ownership
remain pending. In the selected 3,062-declaration scope, 147 declarations are currently
eligible, 2,178 remain pending, and 737 retain their stable skip reasons. Eligibility
does not imply that every candidate is emitted yet.

The first batch emitter selects the `gp_Pnt` value-copy constructors and the configured
eligible static methods whose parameters and return values use only `TM001` through
`TM004`. The current configuration emits 28 static declarations: 20 from `Precision`,
three from `TopAbs`, and five ownership-neutral methods from `Standard`, `TopLoc`, and
`gp`, for 31 declarations total including three constructors. `Standard::Purge` remains
excluded because it has process-wide side effects rather than value-copy semantics.
The typed shared-handle emitter additionally selects 11 safe constructors and instance
members for configured `Geom_CartesianPoint`. The topology emitter selects eight
`TopoDS_Shape` value-semantic declarations and eight checked typed topology casts,
bringing the current emitted total to 58.
This still excludes borrowed receivers, typed topology subclasses, and other packages
until each scope has focused semantic tests. Static overloads are grouped by
native name, ordered by normalized native signature then stable ID, and receive a
zero-based ordinal in both raw managed and native export names. For example,
`Precision::PApproximation()` and `Precision::PApproximation(double)` become suffixes
`0` and `1` respectively.

Generation configuration schema 1.2 adds `generationScopes`. Each scope names its source
package, native prefix, required header, native export prefix, and managed raw prefix.
The emitter validates non-empty fields and rejects duplicate `(sourcePackage,
nativeNamePrefix)` scope identities or export prefixes. Multiple scopes may share a
source package when their native prefixes and output namespaces are distinct. Schema
1.1 remains readable with the default `Precision` scope for upgrade compatibility.
Schema 1.3 adds `sharedHandleScopes`, whose explicit native type/header/export/managed
identity gates the generated shared category after `TM006` and the shared-handle
eligibility pass prove each selected member.
Schema 1.4 adds `topologyScopes`; the initial fail-closed emitter accepts only the
reviewed `TopoDS_Shape`/`Shape` scope and produces module-partitioned `Topology` output.

## Full-library inventory

The selected generation scope is not the full OCCT API. `eng/inventory.ps1` is a
separate audit workflow that catalogs all 7,090 top-level `.h`/`.hxx` entry headers in
the pinned 8.0.1 bundle, parses deterministic batches, recursively isolates bad headers,
applies the versioned common preamble headers declared by configuration, and deduplicates
canonical stable IDs. A partial report cannot be used as the full-OCCT
coverage denominator. `-CatalogOnly` performs the fast header/package census without a
semantic coverage claim.

## Determinism rules

- Sort declarations, files, includes, members, diagnostics, and report records using
  explicit ordinal rules.
- Do not emit current time, random IDs, process IDs, user names, or absolute paths.
- Normalize line endings and text encoding.
- Use invariant culture for identifiers and numeric text.
- Assign stable symbol IDs from normalized semantic identity rather than visit order.
- Derive generated overload ordinals from normalized native signatures and stable IDs,
  never from AST traversal order.
- Record input hashes separately from generated source when provenance is required.
- A clean regeneration must be verifiable with `git diff --exit-code` once Git and
  the generator exist.

## Canonical API manifest

The machine-readable baseline should include, at minimum:

- Schema version and normalized input identity.
- Toolkit, package, header, namespace, and declaration identity.
- Stable symbol ID and native signature.
- Inheritance and `Handle<T>` relationships.
- Managed projection and ABI projection.
- Ownership classification and rule ID.
- Support state and skip reason.
- Source location for diagnostics, normalized relative to the OCCT root.

The manifest format and symbol identity algorithm require a dedicated ADR before
implementation.

The currently implemented `generated/manifest.json` is narrower than that future API
manifest. It is a generated-file ownership manifest (schema 1.0): it records the OCCT
and binding-model versions, selected source stable IDs, output paths, and SHA256 hashes.
It supports safe stale-file cleanup and clean-regeneration verification but is not yet
the module coverage or OCCT upgrade-diff baseline described above.

## Current coverage and diagnostics reports

Every `generate` run now writes these transient, deterministic reports below the inner
workspace `artifacts/generator-reports/` directory:

- `coverage.json` (schema 1.0) records totals and package/toolkit coverage for Pending,
  Skipped, Supported, Manual, and Emitted declarations, plus stable skip-reason counts.
- `diagnostics.json` (schema 1.0) records every declaration in stable-ID order with its
  native signature, source location, package/toolkit, support state, emitted flag, and
  a stable disposition code.

The initial disposition codes are `EM001` for emitted, `EL000` for eligible but not
emitted, `EL001` for unsupported declaration kind, `EL002` for an instance receiver,
`EL003` for an unverified return projection, `EL004` for an unverified parameter
projection, `MN001` for an explicit manual binding, and the existing `SK001`–`SK006`
classification codes. These reports are intentionally not committed baselines; they are
review and upgrade inputs until a canonical API-manifest ADR is accepted.

## Upgrade flow

1. Add a new pinned OCCT dependency definition without deleting the previous one.
2. Generate a discovery manifest for the new version.
3. Diff old and new canonical manifests.
4. Classify additions, removals, signature changes, inheritance changes, and new
   unsupported constructs.
5. Update generalized rules before adding manual exceptions.
6. Regenerate into staging.
7. Review native, managed, report, and public API diffs separately.
8. Run the complete compatibility matrix required for the release.
9. Publish an upgrade report and update `COMPATIBILITY.md`.

## Current CLI and exit behavior

The .NET 10 generator supports deterministic model smoke output, semantic OCCT discovery
from either one header or `config/generation.json`, full-library inventory, and generation through:

```powershell
dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj -- generate --occt-root <path> --config .\config\generation.json --output-root .
```

Generation currently selects three validated `gp_Pnt` value-copy constructors plus 28
value-copy static methods across `Precision`, `TopAbs`, `Standard`, `TopLoc`, and `gp`,
plus 11 typed shared-handle declarations for `Geom_CartesianPoint` and eight
`TopoDS_Shape` value-semantic declarations. It emits twelve native/managed files into
module-partitioned isolated staging, verifies their hashes, replaces the generated set, removes only stale
paths owned by the previous manifest, and writes the coverage/diagnostics reports through
separate isolated report staging.
Discovery reports use schema 1.1 and include structured declaration facts plus the
support summary. Exit code `0` means success, `1` means discovery/configuration/parsing/
generation failed, `2` means command-line usage is invalid, and a semantic inventory
returns `3` after writing its report when one or more headers could not be scanned.

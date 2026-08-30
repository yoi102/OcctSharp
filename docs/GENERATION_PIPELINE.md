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
remain pending. In the selected 16,633-declaration scope, 1,767 declarations are currently
eligible, 61 are accepted manual, 11,397 remain pending, and 3,408 retain their stable
skip reasons. Eligibility
does not imply that every candidate is emitted yet.

The first batch emitter selects the `gp_Pnt` value-copy constructors and the configured
eligible static methods whose parameters and return values use only `TM001` through
`TM004`. The current configuration emits 28 static declarations: 20 from `Precision`,
three from `TopAbs`, and five ownership-neutral methods from `Standard`, `TopLoc`, and
`gp`, for 31 declarations total including three constructors. `Standard::Purge` remains
excluded because it has process-wide side effects rather than value-copy semantics.
The typed shared-handle emitter selects safe constructors and instance members for nine
Geom/Geom2d public types, a schema-1.5 package expansion covering 129
default-constructible StepBasic shared types, and 61 concrete BRepMesh/Poly/
ShapeAnalysis/ShapeFix/ShapeUpgrade types. The enum emitter adds referenced enum
declarations. The topology emitter
selects eight `TopoDS_Shape` value-semantic declarations and eight checked typed topology
casts, bringing the current manifest total to 775 stable IDs.
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
Schema 1.5 adds deterministic `headerPatterns` plus `sharedHandlePackageScopes`.
The expander derives explicit per-type scopes only from discovered records that match
the package/native prefix, derive from `Standard_Transient`, and expose a supported
public default constructor. Exclusions remain configured and the resulting type order is
stable, so package expansion does not weaken `TM006` ownership checks.
Referenced `TM004` enums are emitted into a separate manifest-owned managed file from
their discovered definitions. Full inventory receives the same manifest stable IDs so
generated declarations become `Emitted/EM001` instead of remaining eligible-unselected.
Schema 1.6 adds `manualBindings`. Each entry names a discovered stable ID and an SC-xxx
record. The manual-binding pass rejects empty/duplicate IDs, malformed special-case IDs,
and missing declarations. Full inventory also rejects emitted/manual overlap and unknown
IDs, then reports accepted entries as `Manual/MN001`.

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
- `dependency-closure.json` (schema 1.0) resolves every emitted return, parameter, base,
  enum, handle, point, and topology projection to a product module. It records direct
  and transitive observed edges, target-graph compatibility, strongly connected groups,
  and stable-ID evidence. An unresolved emitted target (`SD001`) fails generation;
  an out-of-contract edge (`SD002`) blocks managed-project split eligibility.

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
plus typed shared-handle declarations for nine Geom/Geom2d, 129 StepBasic, and 61
BRepMesh/Poly/ShapeAnalysis/ShapeFix/ShapeUpgrade types, and eight `TopoDS_Shape`
value-semantic declarations. It owns 775 generated stable IDs,
while schema 1.6 separately reconciles 61 audited SC-032/SC-033 manual declarations, and emits
13 native/managed files into
module-partitioned isolated staging, verifies their hashes, replaces the generated set, removes only stale
paths owned by the previous manifest, and writes the coverage/diagnostics reports through
separate isolated report staging.
Discovery reports use binding-model schema 1.2 and include structured declaration facts,
record abstractness, and the support summary. Package-level shared-handle expansion
rejects abstract records before emission, even when they expose public constructors.
Exit code `0` means success, `1` means discovery/configuration/parsing/
generation failed, `2` means command-line usage is invalid, and a semantic inventory
returns `3` after writing its report when one or more headers could not be scanned.

## Full-selection generated symbol namespaces

Configuration schema 1.9 retains distinct native entry-point segments for each
generated operation category. Static value-copy functions use
`occtsharp_generated_<scope>_static_<member>_<ordinal>`, while generated shared instance
methods use `occtsharp_generated_<type>_method_<member>_<ordinal>`. Shared constructors
retain `_create_`; clone, RTTI, reference-count, and release entry points retain their
fixed infrastructure names. Ordinals are assigned across the complete normalized member
name group, not separately for case-sensitive native spellings.

When two emitted methods would have the same friendly C# name and parameter-type
signature, the later deterministic declaration receives `GeneratedN`. Legal C# overloads
keep one public name, and native case variants remain case variants when their managed
signatures are distinct.

`placementAllocatorNativeTypes` is an exact package-scope list. Each selected constructor
of such a type must expose exactly one generated `Handle<NCollection_IncAllocator>`
parameter or generation fails. The emitter uses allocator placement new and makes the
native wrapper retain the allocator; this is an ownership rule, not a text substitution.

`generatedPreambleHeaders` is an ordered, validated list of completion headers emitted
before generated shared-scope headers. It is reserved for artifact headers that instantiate
a template over a forward-declared element type and therefore fail in a standalone
generated translation unit. Entries must be exact safe include names; duplicates, empty
values, and include-delimiter/newline injection fail generation. The current OCCT 8.0.1
entry is `RWGltf_GltfPrimArrayData.hxx`, required before
`RWGltf_GltfLatePrimitiveArray.hxx`.

`excludedAutoPackages` is a configuration-level core-package boundary. Each entry groups
exact discovered `sourcePackage` names under one stable reason code, category, and detail.
The configured exclusion pass marks those declarations before automatic static/shared
scope expansion, and full inventory applies the same disposition. `SK009` records
Draw/command/test harness packages; `SK010` records IVtk packages blocked behind the
isolated VTK-dependent profile. Exact stable-ID exclusions remain mandatory for narrow
artifact symbol gaps and are not replaced by broad package filtering.

# Batch AC: Audited owned utility expansion

- State: implementation, Release/Debug tests and local delivery gates pass; public
  release readiness is intentionally not claimed.
- Candidate: 8.0.1-preview.28 / ABI 1.72 / bridge 0.80.0.
- Entry: c55a969, 8.0.1-preview.27 / ABI 1.71 / bridge 0.79.0.
- Contract: [ADR-0096](adr/0096-generated-owned-nontransient-values.md),
  [OWNERSHIP](OWNERSHIP.md), [TYPE_MAPPING](TYPE_MAPPING.md).
- Execution: [WORKFLOW](WORKFLOW.md); local commits are authorized.

## Scope

Extend the exact audited catalogue to new independently owned native utility classes:
curve conversion and projection, surface construction, intersection and modeling data,
numeric and color values, and independent exchange/visualization configuration objects.
Read the SDK headers and inline initialization paths before selecting each class and
method. The final catalogue and actual emitted IDs determine the delivered scope.

The implemented addition is 225 owners and 1,924 generated declaration IDs across
53 OCCT packages and nine physical modules. It includes 359 constructors and 1,565
methods. The combined catalogue contains 347 owners, with independent result copies
allowed only for 44 specifically audited data types. New families include analytic
spline conversion/projection, FairCurve and surface construction, conic intersections,
interval subtraction and sampling, HLR and wire ordering, initialized matrices and
color/date data, 88 STEP SELECT containers, and independent Vrml/material/mesh settings.

| Physical module | New owners | Migrated declarations |
| --- | ---: | ---: |
| DataExchange | 113 | 729 |
| Documents | 1 | 5 |
| Foundation | 6 | 85 |
| Geometry | 44 | 321 |
| Mesh | 4 | 26 |
| MeshData | 1 | 5 |
| Modeling | 51 | 630 |
| Visualization | 4 | 105 |
| Xde | 1 | 18 |
| Total | 225 | 1,924 |

887 inherited or existing-Manual method projections and three default factories are
supporting operations, separate from migrated declaration IDs. Another 369 typed
SELECT `SetValue` conveniences are derived only from an owner's exact selected,
parameterless typed-handle getters and an audited inherited `StepData_SelectType::SetValue`.
They use native implicit handle upcasts and checked `SetValue` dispatch, including
`CaseMem` alternatives. This makes entity inputs usable without a general managed
shared-wrapper upcast. Synthetic supporting IDs never enter the migration manifest.

Reuse the existing stable-address registry, typed non-reused tokens, call-duration
leases, SafeHandles and copied-ref publication. Intrusive handles retain the existing
shared category. Borrowed adaptors, retained external addresses, uninitialized query
paths, unapproved buffers, topology ownership changes and UI/context-affine objects
remain outside this extension. No new general pointer or template mapping is added.

New const methods use a const native receiver lease so overloaded native methods
resolve to the audited const signature. The frozen Preview.27 owner epoch preserves
all existing AB operation bodies and identities. This is overload-resolution behavior,
not a new ownership category or a promise to serialize native mutation.

Four declared methods have no symbol in either the pinned Release or Debug DLL/import
library and remain Blocked: `GeomInt_IntSS::SetTolFixTangents`, `TolFixTangents`, and
the `MAT2d_CutCurve::Perform(handle, MAT_Side)` / `PerformInf` overloads. Both toolkits
are already linked. A deprecated `Approx_CurveOnSurface` constructor is omitted in
favor of its selected replacement and `Perform`. Two `GeomInt_IntSS::MakeBSpline*`
helpers remain excluded because they would create a Geometry-to-Modeling dependency.

## Entry evidence

The immutable local baseline is under
`OcctSharp/artifacts/preparation-baselines/preview27-batch-ac/`.
It contains the generated manifest, full inventory, API snapshot, Release bridge and
owned-operation catalogue, with hashes in `baseline-evidence.json`.

- Generated: 23,703 IDs / 137 files; accepted manual: 1,217.
- Entry blocked: 42,005; skipped: 49,347; total: 116,272 declarations.
- Headers: 7,058/7,090, with 32 explicit exclusions.
- Managed public signatures: 51,443; native exports: 37,153.
- Inventory SHA256: `14B0D49AB1929D51D2E9BA17240641C6537C11DDEAF89F7F6F0AFAA0A9A24BE2`.
- Native SHA256: `FE5AE1330F6951EF2560E90AA97967BD0CC8A5B818DC00519111F58327E0498A`.

## Delivery checks

Required: exact signature/availability and const-overload regressions; representative
semantic/lifetime/error tests across the new families; generated native/managed
Release and Debug builds and full suites; actual Debug-native tests; C11 and registry
fixtures; old export/body/API preservation; deterministic and cold regeneration;
fresh full inventory and exact Blocked-to-Emitted reconciliation; all packages, clean
facade/direct-module consumers, local release-check and final package-content checks.

Executed: Release/Debug native/managed compilation and linking, Generator 177/177,
Runtime 568/568 (including 26 AC tests), 155 generated files with a complete acyclic
module graph, and exact preservation of 36,408 previous native functions/prototypes.
Managed API comparison adds 3,633 signatures with zero removals (55,076 total);
the facade has 4,097 type forwarders. New native source exports total 3,408, including
the supporting projections and 225 releases. Local delivery gates pass.

MSVC additionally emits eight default-constructor closure exports for TDF_ClosureMode
and seven Vrml data types. The entry DLL already contains 88 such compiler closures:
the pinned SDK defines Standard_EXPORT as dllexport, including default-argument
constructors. The native audit requires the exact eight additional names separately
from the 3,408 generated C ABI additions and rejects any other extra name. These
compiler artifacts are not supported binding entry points or migrated declarations.
The final Release/Debug export audit passes at 40,569 names: 3,416 additions and
zero removals, with exact parity between configurations.

Actual Debug-native Runtime 568/568 also passes in an isolated directory with all
62 DLL hashes checked. Final Release/Debug C11 and geometric ABI fixtures pass for
40 headers. Registry stress passes 250 release races / 16,000 acquisitions in each
configuration. Accounting tooling passes one positive and 11 rejection cases using
an isolated synthetic inventory. The final coverage manifest classifies 116,263 parsed
declarations as 61,571 Pending, 32,008 Skipped, 21,467 Supported and 1,217 Manual,
with 25,627 emitted IDs. The 32 parser-limited headers remain explicit exclusions.
The clean source rebuild and full suites pass, with all 155 generated files
byte-identical. Fourteen Preview.28 packages, final package-content verification and
both clean facade/direct-module consumers pass, including the new shared Batch AC
utility workflow.

[STATUS](STATUS.md) records current progress and executed evidence. GitHub push, hosted CI, signing and NuGet publication
remain NOT RUN and outside this local migration task.

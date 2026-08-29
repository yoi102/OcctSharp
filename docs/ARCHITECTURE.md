# Architecture

## Purpose

OcctSharp is an OCCT C++ to .NET binding generator and managed SDK. The system must
support controlled regeneration after an OCCT upgrade without relying on widespread
manual wrapper edits.

## Accepted architecture

```text
Pinned OCCT headers, libraries, and build metadata
                         |
                         v
                  C++ AST front end
                         |
                         v
              Canonical binding model
                         |
             +-----------+-----------+
             |                       |
             v                       v
    Generated native C ABI   Generated managed raw bindings
             |                       |
             +-----------+-----------+
                         |
                         v
             Manual friendly .NET API
                         |
                         v
             Tests, reports, and packages
```

The accepted boundaries are recorded in ADRs:

- [ADR-0001](adr/0001-repository-layout.md): repository layout.
- [ADR-0002](adr/0002-native-c-abi.md): native C ABI boundary.
- [ADR-0003](adr/0003-canonical-binding-model.md): canonical binding model.
- [ADR-0007](adr/0007-generated-source-and-raw-naming.md): committed generated
  source and raw/friendly naming separation.
- [ADR-0008](adr/0008-initial-nuget-and-native-runtime-layout.md): initial NuGet
  package and application-local native runtime layout.
- [ADR-0009](adr/0009-native-shape-handle-registry.md): native stale-handle registry.
- [ADR-0010](adr/0010-standard-transient-shared-handle.md): intrusive shared-handle probe.
- [ADR-0011](adr/0011-shared-handle-runtime-type-identity.md): runtime type identity checks.
- [ADR-0013](adr/0013-generated-typed-shared-handle.md): generated typed shared handles.
- [ADR-0014](adr/0014-batched-full-occt-inventory.md): separate batched full-library inventory.
- [ADR-0015](adr/0015-staged-managed-package-modularity.md): staged managed/project/package modularity.
- [ADR-0016](adr/0016-generated-topods-shape-value-semantics.md): generated
  `TopoDS_Shape` copy, identity, and orientation semantics.
- [ADR-0017](adr/0017-generated-typed-topology-casts.md): checked typed topology casts.
- [ADR-0018](adr/0018-opaque-gp-trsf-value-bridge.md): opaque `gp_Trsf` values.
- [ADR-0019](adr/0019-opaque-toploc-location-value-bridge.md): opaque
  `TopLoc_Location` values.
- [ADR-0020](adr/0020-opaque-gp-vector-axis-matrix-value-bridge.md): opaque `gp` vector,
  direction, axis, and matrix values.
- [ADR-0021](adr/0021-opaque-occt-strings-and-real-sequence.md): opaque OCCT strings
  and real sequences with explicit buffer/index contracts.
- [ADR-0046](adr/0046-hwnd-thread-affine-viewer-and-presentation-ids.md): HWND-bound,
  thread-affine visualization ownership and parent-bound presentation IDs.
- [ADR-0047](adr/0047-optional-dependency-profiles-and-package-isolation.md): explicit
  optional dependency profiles and isolated future packages.
- [ADR-0048](adr/0048-final-long-tail-and-header-classification.md): deterministic
  final dispositions for full-inventory declarations and failed entry headers.
- [ADR-0051](adr/0051-repository-native-bootstrap.md): incremental repository-native
  bootstrap from pinned local or immutable OCCT inputs.
- [ADR-0052](adr/0052-native-local-common-modeling-operations.md): native-local common
  modeling algorithms plus fail-closed stable-ID accounting for manual bindings.
- [ADR-0057](adr/0057-core-toolkit-closure-and-auto-package-exclusions.md): one explicit
  core native toolkit/runtime closure plus auditable automatic package exclusions.
- [ADR-0058](adr/0058-narrow-long-tail-dispositions-and-local-completion-gate.md): narrow
  final long-tail dispositions and separate local implementation/publication gates.
- [ADR-0059](adr/0059-committed-windows-runtime-and-mit-license.md): committed,
  manifest-verified Windows x64 runtime and MIT project license.
- [ADR-0060](adr/0060-common-cad-api-product-batch.md): one product-scale Batch C that
  prioritizes common end-to-end CAD workflows and large cross-family implementation waves.
- [ADR-0061](adr/0061-domain-layered-generated-output.md): module/layer-partitioned
  generated source while retaining one managed assembly, one native DLL, and stable
  public type full names.
- [ADR-0062](adr/0062-generated-shard-dependency-closure.md): semantic closure of every
  emitted cross-shard signature, the MeshData layer, and the evidence-based decision to
  defer physical managed/native splitting.
- [ADR-0063](adr/0063-final-batch-c-selective-session-topology-viewer-closure.md): final
  Batch C selective STEP session, owning topology edit/selection, and parent-bound input
  ownership boundary.
- [ADR-0064](adr/0064-production-cad-viewport-review-batch.md): one finite Batch D
  production viewport/model-review closure with copied XDE identity and topology,
  parent-bound filters/clip planes, camera values, and durable screenshot evidence.
- [ADR-0065](adr/0065-occt-aligned-nuget-preview-version.md): OCCT-aligned NuGet numeric
  versions with an independent OcctSharp preview counter.
- [ADR-0066](adr/0066-engineering-inspection-measurement-pmi-batch.md): one finite Batch E
  engineering-inspection, exact-measurement, and PMI/AP242 closure with explicit
  ownership, mutation, round-trip, visualization, and screenshot gates.

## Components

### AST front end

Reads the pinned OCCT header set with explicit compiler arguments, include paths,
preprocessor definitions, target platform, and language standard. It produces facts;
it does not emit C# directly.

The initial AST implementation is pinned ClangSharp/libClangSharp as recorded in
ADR-0006. Regex and ad hoc header splitting are not valid primary parsers.

### Canonical binding model

Normalizes declarations into stable generator concepts such as types, methods,
parameters, inheritance, templates, ownership, availability, product module, and skip reasons.
It is the shared input for every emitter and report.

Binding-model schema 1.3 assigns every declaration one fail-closed product module.
Foundation, Geometry, MeshData, Modeling, Mesh, Documents, DataExchange, Xde, Visualization, and
optional integration identities are stable generator facts rather than filesystem guesses.
`MeshData` owns Poly triangulation/polygon data below Modeling; Mesh owns meshing
algorithms above Modeling. `TopAbs_Orientation` is intentionally lifted into the
Foundation value contract so geometry adaptors do not acquire a reverse Modeling edge.

### Transformation passes

Apply ordered, testable rules for naming, type mapping, ownership, overload conflicts,
unsupported constructs, module scope, ABI projection, and manual exclusions. Passes
must not depend on filesystem enumeration order.

### Native bridge

Links to OCCT through C++ but exports only an explicitly defined C ABI. It owns C++
exception containment, allocation symmetry, handle validation, and conversion of ABI
types. The current owning shape category uses a mutex-protected live-handle registry
to reject stale handles before dereference. Generated `TopoDS_Shape` operations allocate
independent wrapper-owned C++ values that preserve shared internal `TShape`, location,
and orientation semantics; this is not shared `Handle<T>` reference counting or a
general concurrency contract. B05 extends the same registry/error pattern to opaque
`gp_Trsf`, `TopLoc_Location`, `gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` values; every
operation result is an independent owning wrapper and no C++ layout crosses the ABI.
B06 extends the same rule to UTF-8/UTF-16 OCCT strings, `NCollection_Sequence<double>`,
`NCollection_Array1<double>`, and the OCCT 8 dynamic-array-backed
`NCollection_Vector<double>` alias, `NCollection_DataMap<int,double>`, and
`NCollection_IndexedMap<int>`: all text is copied through caller-owned buffers and all
collection values are copied through bounded value calls; no native string pointer,
container layout, element reference, or iterator crosses the boundary. C++ class layouts
and STL types never cross this boundary.

The B17 Windows visualization profile owns the complete display-driver/viewer/context/
view/window graph in one native wrapper bound to an application-owned HWND. AIS objects
remain native and are addressed by parent-scoped IDs; selection is copied as IDs. The
application forwards window/input events on the creating thread, with no reverse callback.

The final Batch C boundary extends that model without exposing borrowed OCCT state.
Edge/surface derivatives and pcurves cross as copied values; trim, wire, reshape, STEP
transfer, and selected topology cross as independent registered owning shapes. A
`StepReadSession` owns one native reader until disposal and transferred shapes survive it.
Viewer subshape modes, selected topology snapshots, and mouse/wheel/semantic-key input
remain presentation- or viewer-parent-bound and creating-thread-affine. Alpha.54 closes
the finite common-workflow denominator; advanced filters, custom rendering, optional
integrations, cold schema, and exhaustive mesh attributes are outside this architecture
milestone rather than implicit unfinished work.

Batch D extends only the existing friendly visualization owner graph. `OcctViewer`
continues to own the interactive context, view, AIS presentations, built-in selection
filters, and clip planes on its creating thread. XDE occurrence paths/entries, camera
state, coordinate conversions, bounds, colors, and plane equations cross as copied
managed values. Detected and selected topology crosses as an independent registered
owning `Shape`; OCCT's borrowed detected shape never escapes. Presentations, filters,
and clip planes remain parent-bound IDs. Screenshot output is a durable file operation;
no `Image_PixMap`, pixel pointer, callback, or custom rendering pipeline crosses the ABI.
The accepted 24-capability denominator is complete at alpha.55. Its runtime and clean
package paths both execute the real STEP/XDE-to-real-HWND review-to-screenshot workflow;
the complete Release/Debug, ownership, inventory, compatibility, provenance, and local
release gate chain passes together.

Batch E preserves the same physical and ownership architecture while adding a new product
closure. Call-local BRepExtrema/adaptor/property algorithms do not escape the native ABI;
exact solutions, parameters, classifications, scalar measurements, matrices, units, and
PMI graph records cross as copied managed values. Topology supports cross only as
independent owning `Shape` copies. XCAF dimension/tolerance/datum labels remain bound to
their owning document by stable entries, and mutations are document-owned transactions.
STEPCAF/AP242 sessions own native reader/writer state only for the call or explicit
session lifetime. Viewer annotations and saved-view presentation resources remain
creating-thread-affine and viewer-parent-bound; screenshots remain durable file results.
Preparation is complete at 0/24 and no Batch E implementation validation is claimed.

The common-modeling capability milestone follows the existing owning-shape category. Primitive,
feature, offset, section, bounding, and analyzer objects exist only during one native
call. Topology results are independent registered owners; bounds are fixed copied values.
Configuration schema 1.6 links each directly used manual declaration stable ID to SC-032,
and both selected discovery and full inventory fail when a configured ID disappears or
overlaps generated ownership.

Batch B's high-value API work extended this result-oriented boundary across
curves, surfaces, topology maps, multi-shape construction, solid features, and Boolean
history summaries. Builder, adaptor, projection, adjacency, sewing, pipe, loft, thick-
solid, and history objects remain native-local. Only registered owning shapes, immutable
value snapshots, and caller-owned compact index arrays cross the ABI. Schema 1.6 now
reconciles 61 audited manual stable IDs across SC-032 and SC-033. This is progress inside
the single product-scale B batch, not completion of its full long-tail exit criteria.

XDE STEP composition uses the established document/label boundary. `XdeDocument.ImportStep`
clones source roots and metadata into an existing destination document during a
transaction and returns destination-parent-bound labels. The application chooses the
assembly graph, locations, metadata edits, persistence, and export timing. The older
one-shot `StepAssembly` facade is obsolete compatibility surface rather than the primary
architecture.

Package-level generated shared-handle expansion now applies the same O004 category to
selected `Geom`, `Geom2d`, `BRepMesh`, `Poly`, `ShapeAnalysis`, `ShapeFix`, and
`ShapeUpgrade` descendants of `Standard_Transient`. Header-pattern discovery selects
only concrete records with at least one independently supported value-copy constructor.
Instance members may use verified scalar/enum/copied-point projections or nullable
relationships to other selected generated shared-handle wrappers. Binding-model
schema 1.3 carries Clang's abstract-record fact and product module so abstract bases
cannot reach native
construction. Each generated wrapper owns one retained intrusive handle behind a
type-specific registry; no class layout, raw `Handle<T>`, or borrowed member crosses
the ABI.

Generated native files are partitioned below `generated/<ProductModule>/` by value,
topology, and shared-handle shard. CMake recursively collects those translation units.
Cross-module shared handles include their dependency module headers and share a Runtime
helper contract, but all registries, allocators, and release functions still link into
the single `OcctSharp.Native.dll`.

The semantic dependency-closure pass resolves every emitted signature projection before
output replacement. The accepted 16,353-declaration graph has 27 observed cross-shard
edges, all compatible with the target graph, and no strongly connected group. This makes
the generated managed shards eligible for later project migration; it does not authorize
native DLL splitting or change current deliverables.

### Managed raw bindings

Mirror the generated C ABI closely enough for traceability. They own marshalling,
safe handle integration, managed exception construction, and low-level ABI version
checks. Raw bindings are not necessarily the preferred public API.

Generated raw bindings live in the internal `OcctSharp.Generated` namespace. Generated
native exports use the `occtsharp_generated_` prefix. These sources and their ownership
manifest are committed and must be changed by regeneration, not direct editing.

Raw and friendly generated C# are physically partitioned below
`Generated/<ProductModule>/`. Directory placement does not change namespaces: raw types
remain internal `OcctSharp.Generated`, public generated wrappers remain `OcctSharp`, and
all compile into the single `OcctSharp.dll`.

Referenced native enums are emitted as public typed managed enums from discovered
enumerators while their raw ABI remains validated `int32_t`. Qualified and unqualified
C++ spellings resolve to one canonical managed enum name; the enum declaration stable ID
is owned by the same generated manifest as the methods that reference it.

### Friendly managed API

Provides intentional .NET-oriented workflows without changing OCCT semantics.
This layer may include STEP helpers, topology enumeration, bulk mesh transfer, and
other manually designed APIs. Manual behavior must be documented and tested.

### Package and native loading

The initial single NuGet package carries the managed assembly and the Windows x64
Release native dependency closure. Transitive build assets copy native files below the
consumer's `occt` output directory. An assembly-level resolver loads the bridge from
that exact application-local path; process `PATH` and machine-wide OCCT installations
are not package dependencies.

Repository Sample projects default to the committed, SHA256-manifested Windows x64
Release closure below `runtime/win-x64/occt`. Debug and Release managed builds copy that
same ABI-compatible closure to output `occt/`, so clone-and-run needs only the pinned
.NET SDK. Contributors can set `OcctSharpUseBundledNativeRuntime=false` to invoke
ADR-0051's incremental `eng/ensure-native.ps1` path from an explicit SDK or immutable
HTTPS archive plus SHA256. Repository and NuGet paths converge on the same output layout.

### Reports and baselines

Generation produces machine-readable and human-readable reports for discovered,
generated, skipped, manually wrapped, failed, and validated APIs. A canonical API
manifest supports upgrade diffs between pinned OCCT baselines.

Manifest schema 1.1 records product module, API layer, and output shard for every owned
generated file. Coverage and diagnostics include the same module identity so source
layout and declaration accounting cannot drift independently.
`dependency-closure.json` separately records normalized direct/transitive signature
edges, target-graph violations, strongly connected groups, and source stable-ID evidence.

## Non-negotiable invariants

1. Generated and manual source remain physically separate.
2. Native exceptions never cross the C ABI.
3. Unknown ownership never defaults to owning or borrowed behavior.
4. OCCT `Handle<T>` semantics are not reduced to an untracked `IntPtr`.
5. Generated output is deterministic for the same normalized inputs.
6. Every skipped API has a stable reason code and diagnostic context.
7. Compile success is not reported as runtime or lifetime success.
8. Friendly APIs may simplify usage but must preserve native semantics.
9. Emitted declarations cannot be `Unassigned` or retain an unresolved signature target;
   the generated managed dependency graph must remain target-compatible and acyclic.
10. Physical managed splitting still requires an assembly-identity/manual-facade migration;
    native DLL splitting additionally requires cross-DLL registry and creator-release evidence.

## Explicitly unresolved decisions

- The repository supports immutable URL/SHA256 acquisition, but an approved public
  OCCT 8.0.1 artifact location is not yet selected or committed.
- General shared, borrowed, parent-bound, and runtime-typed handle representation.

The first checked cast boundary is implemented for the experimental
`OcctSharp_TransientDerived` probe under ADR-0012. ADR-0013 generalizes the validated
shared category into configured generated typed wrappers; `GeomCartesianPoint` is the
first real OCCT type. Borrowed handles and parent-bound projections must still be
resolved through ADRs before their implementation becomes structural.

### Full-library inventory

Normal generation remains a deliberately small, fast dependency closure. The separate
inventory workflow catalogs every public `.h`/`.hxx` entry header, parses deterministic
batches, isolates failures, and deduplicates semantic stable IDs. Only a complete scan
may establish the full-OCCT declaration denominator; partial totals remain diagnostics.

The long-tail workstream additionally produces a complete *classification* over all declarations discovered
from successful headers and all catalogued headers. This does not fill declarations that
cannot be parsed and never promotes blocked/eligible-unselected items into generated
coverage. Semantic inventory, classification, emission, and validation remain separate.

These items must be resolved through ADRs before their implementation becomes
structural or difficult to reverse.

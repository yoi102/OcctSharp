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

## Components

### AST front end

Reads the pinned OCCT header set with explicit compiler arguments, include paths,
preprocessor definitions, target platform, and language standard. It produces facts;
it does not emit C# directly.

The initial AST implementation is pinned ClangSharp/libClangSharp as recorded in
ADR-0006. Regex and ad hoc header splitting are not valid primary parsers.

### Canonical binding model

Normalizes declarations into stable generator concepts such as types, methods,
parameters, inheritance, templates, ownership, availability, and skip reasons.
It is the shared input for every emitter and report.

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

The B19.3 common-modeling profile follows the existing owning-shape category. Primitive,
feature, offset, section, bounding, and analyzer objects exist only during one native
call. Topology results are independent registered owners; bounds are fixed copied values.
Configuration schema 1.6 links each directly used manual declaration stable ID to SC-032,
and both selected discovery and full inventory fail when a configured ID disappears or
overlaps generated ownership.

### Managed raw bindings

Mirror the generated C ABI closely enough for traceability. They own marshalling,
safe handle integration, managed exception construction, and low-level ABI version
checks. Raw bindings are not necessarily the preferred public API.

Generated raw bindings live in the internal `OcctSharp.Generated` namespace. Generated
native exports use the `occtsharp_generated_` prefix. These sources and their ownership
manifest are committed and must be changed by regeneration, not direct editing.

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

Repository projects use a distinct developer bootstrap. The Sample project's incremental
MSBuild target invokes `eng/ensure-native.ps1` only when the requested native artifact is
missing or stale. That script validates the pinned OCCT manifest and builds CMake native
targets without invoking the managed solution. Inputs come from an explicit/local SDK
path or an immutable HTTPS archive plus SHA256; no floating or machine-wide fallback is
allowed. Both the repository and NuGet paths converge on the same output `occt/` layout.

### Reports and baselines

Generation produces machine-readable and human-readable reports for discovered,
generated, skipped, manually wrapped, failed, and validated APIs. A canonical API
manifest supports upgrade diffs between pinned OCCT baselines.

## Non-negotiable invariants

1. Generated and manual source remain physically separate.
2. Native exceptions never cross the C ABI.
3. Unknown ownership never defaults to owning or borrowed behavior.
4. OCCT `Handle<T>` semantics are not reduced to an untracked `IntPtr`.
5. Generated output is deterministic for the same normalized inputs.
6. Every skipped API has a stable reason code and diagnostic context.
7. Compile success is not reported as runtime or lifetime success.
8. Friendly APIs may simplify usage but must preserve native semantics.

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

B19 additionally produces a complete *classification* over all declarations discovered
from successful headers and all catalogued headers. This does not fill declarations that
cannot be parsed and never promotes blocked/eligible-unselected items into generated
coverage. Semantic inventory, classification, emission, and validation remain separate.

These items must be resolved through ADRs before their implementation becomes
structural or difficult to reverse.

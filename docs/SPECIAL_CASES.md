# Special Cases

Manual bindings and exceptions to general generator rules are recorded here. A special
case must be narrow, justified, tested, and linked to the generalized rule it cannot
use.

## Registry format

Each entry must contain:

- ID and status.
- Affected OCCT versions, toolkits, packages, and symbols.
- Problem and why normal generation is unsafe or impossible.
- Native, ABI, and managed behavior.
- Ownership and cleanup rules.
- Tests and diagnostics.
- Upgrade impact.
- Removal criteria or reason it is permanent.

## Planned investigation areas

These are not approved manual wrappers; they are known areas likely to need dedicated
rules or design:

| ID | Area | State | Reason |
|---|---|---|---|
| SCI-001 | Broad `Handle<T>` | Partial | Generated shared ownership is implemented for configured types; borrowed/parent-bound/general casts remain |
| SCI-002 | `TopoDS_*` | Partial | Base `TopoDS_Shape` value semantics generated; typed hierarchy/location/explorers pending |
| SCI-003 | OCCT string classes | Investigation | Encoding, copying, and ownership differ by use |
| SCI-004 | `NCollection_*` | Investigation | Container semantics and index bases vary |
| SCI-005 | Multiple inheritance | Investigation | Managed projection and runtime cast ambiguity |
| SCI-006 | XDE/OCAF documents and labels | Deferred | Parent/document lifetime and metadata graph |
| SCI-007 | Visualization callbacks and window handles | Deferred | Platform integration, reentrancy, threading |
| SCI-008 | Bulk mesh transfer | Deferred | Performance-sensitive array and lifetime contract |

## SC-001: Interim manual geometry exchange bridge

- Status: Accepted interim implementation.
- Scope: OCCT 8.0.1; `TKDESTEP`, `TKDESTL`, `TKDEIGES`, `TKMesh`; STEP reader/writer,
  STL writer, IGES writer, rigid transform, and compound builder symbols.
- Reason: The user requires runnable samples before native/managed emitters exist.
  Treating these APIs as generated would misrepresent generator coverage.
- Native/ABI/managed behavior: Narrow C ABI functions expose file operations and
  owning shape results. `ShapeExchange`, `ShapeTransform`, and `ShapeAssembly` are the
  manual friendly layer. Paths are call-bound UTF-8 inputs.
- Ownership: Read and transformed shapes are owning handles under O001/O005/O006/O007.
  Compound addition copies `TopoDS_Shape` value semantics; it does not consume the child.
- Validation: Release runtime tests cover STEP round-trip, STL/IGES file creation, and
  transformed compound STEP round-trip. All five console commands ran successfully.
- Upgrade impact: Rebuild and rerun real-file tests for every OCCT upgrade; data-exchange
  return statuses and runtime DLL closure must be rechecked.
- Removal criteria: Replace raw manual functions only after generated equivalents have
  compile/runtime/lifetime evidence. The friendly workflow may remain manual.

## SC-002: Geometry-only STEP assembly

- Status: Superseded by SC-003 for the console assembly workflow; the geometry-only
  `ShapeAssembly` API remains available for ordinary topology compounds.
- Scope: `STEPControl_Reader`, `TopoDS_Compound`, rigid `gp_Trsf`, and
  `STEPControl_Writer`.
- Reason: A useful placement/merge sample is needed without prematurely introducing
  XDE/OCAF document and label lifetimes.
- Behavior: Every input is translated to OCCT geometry, transformed, added to a compound,
  and written as ordinary STEP. No Boolean fuse is performed.
- Metadata: Names, colors, layers, materials, and product structure are not preserved.
- Ownership: Source, transformed, and compound shapes remain independently owned
  `SafeHandle` wrappers; compound topology remains valid after temporary wrappers close.
- Validation: Seven local STEP fixtures produced a 701-face geometry compound; a focused
  two-box runtime test round-tripped 12 faces through STEP.
- Supersession: SC-003 is a separate API and sample path, so the semantics of this
  geometry-only API have not silently changed.

## SC-003: One-shot STEPCAF/XDE assembly exchange

- Status: Accepted interim implementation.
- Scope: OCCT 8.0.1; `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, XCAF document
  tools, `XCAFDoc_Editor`, and `TopLoc_Location`; toolkits `TKDESTEP`, `TKXCAF`,
  `TKCAF`, `TKLCAF`, and `TKCDF`.
- Reason: Document labels, OCAF attributes, and metadata graphs need parent/document
  lifetime rules that the current generated API does not have. A narrow one-shot native
  operation preserves their scope while providing the requested assembly workflow.
- Native/ABI/managed behavior: ABI 1.2 accepts a contiguous UTF-8 path and rigid
  transform array. Native code creates all source and destination XDE documents, clones
  shape-label trees and metadata, adds every root as a component under one output
  assembly, and writes via STEPCAF. `StepAssembly.WriteXde` owns only temporary UTF-8
  buffers; it exposes no XDE handles.
- Metadata: Colors, styles, names, layers, properties, visual materials, and physical
  materials are copied. OCCT 8.0.1 writes physical material relationships only for
  top-level part labels; when an input material is attached to a subshape, the bridge
  retains that assignment and promotes it to the cloned part root for STEP round-trip.
- Ownership: All source/output XDE objects are native local handles. Input paths are
  borrowed for the duration of the call; native code allocates no caller-released XDE
  resource.
- Validation: Native static layout assertions plus managed 64-byte/offset tests; a
  two-box runtime XDE assembly round-trip validates one assembly root and 12 faces. A
  seven-file local run retained color/style entities, four material-property records,
  product definitions, and seven assembly occurrences.
- Upgrade impact: Rebuild and rerun the XDE real-file evidence for every OCCT upgrade;
  rerun against metadata-bearing fixtures, not geometry-only STEP files.
- Removal criteria: Replace the raw operation only after generated XDE/OCAF bindings
  have explicit document/label ownership, equivalent metadata evidence, and lifetime
  tests.

## SC-004: Checked `Standard_Transient` derived wrapper

- Status: Accepted experimental probe.
- Scope: OCCT 8.0.1; `Standard_Transient` and the bridge-local
  `OcctSharp_TransientDerived` RTTI type.
- Reason: General `Handle<T>` generation is not yet safe, but runtime type identity
  and retained sharing need one concrete cast boundary before topology or XDE types
  are projected.
- Native/ABI/managed behavior: ABI 1.8 validates a live, non-null wrapper with
  OCCT `IsKind("OcctSharp_TransientDerived")`, returns `TypeMismatch` on failure,
  and copies the opaque shared handle on success. `SharedTransient.TryCastDerived`
  and `CastDerived` expose the result as `SharedTransientDerived`; no native object
  pointer or layout is exposed.
- Ownership: The successful cast retains one intrusive OCCT reference. Each managed
  wrapper releases exactly its own native wrapper; null and incompatible casts create
  no output wrapper.
- Validation: Release and Debug runtime tests verify retained reference counts,
  null/wrong-kind rejection, and the throwing `InvalidCastException` path.
- Upgrade impact: Re-run RTTI identity and cast tests against every OCCT upgrade;
  this probe does not establish a general generated `Handle<T>` compatibility claim.
- Removal criteria: Replace with generated typed-handle descriptors only after
  shared, borrowed, parent-bound, and cast lifetime contracts have dedicated ADRs
  and stress evidence.

## SC-005: Opaque `gp_Trsf` transformation value bridge

- Status: Accepted B05 interim implementation; B05 is now closed as one coarse batch.
- Scope: OCCT 8.0.1; `gp_Trsf` creation, copy, inversion, multiplication, matrix
  reads, and `TopoDS_Shape` transformation; toolkit `TKMath` plus `TKTopAlgo`.
- Reason: `gp_Trsf` contains OCCT-specific matrix and form state; exposing its C++
  layout as a managed struct would make ABI and upgrade behavior unsafe. The first
  safe migration unit therefore keeps the value opaque while preserving OCCT copy
  semantics through explicit operations.
- Native/ABI/managed behavior: ABI 1.12 adds a registry-validated `gp_Trsf` handle,
  finite-value checked construction, independent clone/results, 1-based `Value`
  reads, and shape application. Native exceptions remain contained by the existing
  status contract; no C++ layout crosses the boundary.
- Ownership: Every returned transformation is an owning `SafeHandle`; clone,
  inverse, multiplication, and transformed shape results are independent values.
  Releasing a source does not invalidate a result.
- Validation: Debug/Release builds and the 32-test runtime suite cover identity,
  composition, clone, inverse, shape application, finite-value rejection, and index
  validation. The alpha.11 package consumer and generated-freshness checks also pass.
- Upgrade impact: Re-check `gp_Trsf` matrix indexing, inversion failure behavior, and
  `BRepBuilderAPI_Transform` semantics for every OCCT upgrade.
- Removal criteria: Replace the bridge only after generalized generated value rules
  can model `gp_Trsf` state without layout crossing and provide equivalent lifetime
  evidence.

## SC-006: Opaque `TopLoc_Location` value bridge

- Status: Accepted B05 interim implementation; B05 is now closed as one coarse batch.
- Scope: OCCT 8.0.1 `TopLoc_Location` identity, construction from `gp_Trsf`, copy,
  inversion, multiplication, identity query, transformation conversion, and shape
  `Located`/`Moved` operations; toolkits `TKMath` and `TKBRep`.
- Reason: A location owns a composite datum/power list and exposes references to
  internal `gp_Trsf` state. A managed layout projection or borrowed reference would
  be unsafe across calls and OCCT upgrades.
- Native/ABI/managed behavior: ABI 1.13 adds a registry-validated opaque location
  handle. All returned locations, transformations, and shapes are independent values;
  OCCT exceptions remain inside the C ABI.
- Ownership: `TopLocLocation`, `GpTrsf`, and returned `Shape` instances each own one
  native wrapper. Source disposal does not invalidate operation results.
- Validation: Debug/Release runtime tests cover identity, clone, inverse, composition,
  transform conversion, and absolute/relative shape placement. The alpha.11 package
  consumer also verifies the location path.
- Upgrade impact: Re-check `TopLoc_Location` composition order and `Located` versus
  `Moved` semantics for every OCCT upgrade.
- Removal criteria: Replace this bridge after generated value rules can model the
  location datum sequence and parent/reference lifetime safely.

## SC-007: Opaque `gp` vector, direction, axis, and matrix value bridge

- Status: Accepted B05 interim implementation; B05 is closed as one migration batch.
- Scope: OCCT 8.0.1 `gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat`, plus vector/axis
  conversion entry points on `gp_Trsf`; toolkit `TKMath`.
- Reason: These values have OCCT-specific invariants (including non-zero direction
  validation) and implementation layouts that must not become a managed ABI struct.
  The generator does not yet emit operation-bearing opaque value families safely.
- Native/ABI/managed behavior: ABI 1.14 adds registry-validated opaque handles with
  finite input checks, clone/components operations, vector magnitude/dot/cross,
  direction reversal, axis reversal, matrix identity/value/determinant, and independent
  `gp_Trsf` translation/rotation results. Matrix indices are 1-based and limited to
  1..3; zero directions are rejected by OCCT and surfaced through the existing status.
- Ownership: Every `GpVec`, `GpDir`, `GpAx1`, `GpMat`, and returned `GpTrsf` is an owning
  `SafeHandle`. Source disposal never invalidates a result; release is idempotent after
  registry removal.
- Validation: Debug and Release builds pass 32 generator and 32 runtime tests. The
  alpha.11 clean package consumer loads all 36 native DLLs from `occt` and validates
  ABI 1.14/bridge 0.15.0 plus vector, direction, axis, and matrix behavior.
- Upgrade impact: Re-check direction/finite validation, cross-product orientation,
  matrix indexing/determinant, rotation convention, and native dependency closure for
  every OCCT upgrade.
- Removal criteria: Replace the manual bridge only after generated value rules can emit
  equivalent opaque ownership, validation, and conversion contracts with fresh runtime
  evidence. Do not count this bridge as generated declaration coverage.

## SC-008: Opaque OCCT strings and real sequence

- Status: Accepted B06 first migration wave; the broader B06 strings/collections batch
  remains in progress.
- Scope: OCCT 8.0.1 `TCollection_AsciiString`, `TCollection_ExtendedString`, and
  `NCollection_Sequence<double>`; toolkit `TKernel`/foundation headers and `TKMath`
  consumers.
- Reason: String buffers have distinct UTF-8/UTF-16 semantics and native lifetimes;
  `NCollection_Sequence` has native allocation and 1-based indexing. Returning native
  pointers or crossing container layout would create invalid ownership contracts.
- Native/ABI/managed behavior: ABI 1.15 adds opaque registry handles, explicit UTF-8
  input/output buffers with lengths, ASCII/extended conversion, UTF-16 code-unit reads,
  finite real-sequence values, and copy/append/set/remove operations. Managed string
  reads are copies; managed sequence indices are 0-based and translated to OCCT's
  1-based API.
- Ownership: Each string or sequence is an owning `SafeHandle`; clone results are
  independent values, buffers are caller-owned for the duration of a call, and native
  release is idempotent after registry removal.
- Validation: Debug/Release builds pass 32 generator and 34 runtime tests. The alpha.12
  clean package consumer passes with ABI 1.15/bridge 0.16.0 and 36 DLLs under `occt`.
- Upgrade impact: Re-check UTF-8 conversion, UTF-16 surrogate/code-unit behavior,
  1-based sequence indexing, finite-value checks, and native dependency closure for
  every OCCT upgrade.
- Removal criteria: Replace this bridge after generated rules can model encoding,
  caller-buffer, element mapping, container mutation, and iterator ownership. Do not
  count this manual wave as generated declaration coverage.

## SC-009: Opaque real arrays and vectors

- Status: Accepted B06 second migration wave; the broader B06 strings/collections batch
  remains in progress.
- Scope: OCCT 8.0.1 `NCollection_Array1<double>` and the deprecated
  `NCollection_Vector<double>` alias backed by `NCollection_DynamicArray<double>`.
- Reason: These templates own native storage and expose different index contracts;
  crossing their layout or returning element references would leak allocator and
  lifetime assumptions into managed code.
- Native/ABI/managed behavior: ABI 1.16 adds registry-backed opaque owners. Array values
  are copied into a native 1-based array and the lower bound is reported explicitly;
  vector values are copied into a zero-based dynamic array. Managed wrappers expose
  0-based `IReadOnlyList<double>` views, clone, bounded mutation, and value-by-value
  enumeration with finite-value validation.
- Ownership: Each collection is an owning `SafeHandle`; clone results are independent,
  input buffers are borrowed only for one call, and release is idempotent after registry
  removal. No native pointer or iterator escapes.
- Validation: Debug/Release builds pass 32 generator and 36 runtime tests. The alpha.13
  clean package consumer passes with ABI 1.16/bridge 0.17.0 and 36 DLLs under `occt`.
- Upgrade impact: Re-check `Array1` lower/upper behavior, dynamic-array zero-based
  indexing, default-value behavior, finite checks, and native dependency closure.
- Removal criteria: Replace this manual bridge after generated rules can model template
  element mapping, allocator ownership, lower-bound translation, mutation, and iterator
  lifetime. Do not count this bridge as generated declaration coverage.

## SC-010: Opaque integer-key maps

- Status: Accepted B06 map wave; the broader B06 strings/collections batch remains in progress.
- Scope: OCCT 8.0.1 `NCollection_DataMap<int,double>` and `NCollection_IndexedMap<int>`.
- Reason: Native hash buckets, nodes, and iterators are allocator-owned implementation
  details. The selected scalar keys/values can be copied safely without exposing them.
- Native/ABI/managed behavior: ABI 1.17 adds lookup/bind/unbind/extent/clone for the data
  map and ordered key/index/add/remove-last/clone for the indexed map. Managed indexed
  map access is 0-based while OCCT key indexes remain 1-based internally. Duplicate input
  keys are rejected; real values must be finite.
- Ownership: Each map is an owning `SafeHandle`; construction buffers are borrowed only
  for one call, clone results are independent, and no native node or iterator escapes.
- Validation: Debug/Release builds pass 32 generator and 38 runtime tests. The alpha.14
  clean package consumer passes with ABI 1.17/bridge 0.18.0 and 36 DLLs under `occt`.
- Upgrade impact: Re-check hash/index duplicate behavior, `Bind` replacement semantics,
  `UnBind` return behavior, and ordered map index rules for every OCCT upgrade.
- Removal criteria: Replace this manual bridge after generated template rules can model
  key/value element mapping, allocator ownership, and iterator lifetime. Do not count
  this bridge as generated declaration coverage.

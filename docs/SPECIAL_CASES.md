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
| SCI-007 | Visualization callbacks and window handles | Partial | B17 accepts an existing HWND and explicit same-thread event forwarding; callbacks remain excluded under SC-031 |
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

## SC-011: Caller-owned scalar collection snapshots

- Status: Accepted B06 completion wave.
- Scope: `OcctRealSequence`, `OcctRealArray`, `OcctRealVector`, `OcctIntRealMap`, and
  `OcctIntIndexedMap` snapshot APIs.
- Reason: Native iterators and node pointers cannot safely cross the ABI or survive
  mutation/disposal. A one-shot copy gives a deterministic early-exit and lifetime
  contract without exposing implementation layout.
- Native/ABI/managed behavior: ABI 1.18 validates live handles and buffer capacity,
  copies scalar values or key/value pairs, and returns the number written. Managed
  `Snapshot()` allocates caller-owned arrays; empty snapshots use a non-null sentinel
  allocation internally and return empty managed arrays.
- Ownership: Snapshot buffers are borrowed for one call and are never retained by OCCT;
  returned managed arrays are independent values. No native iterator is created or
  released by managed code.
- Validation: Debug/Release builds pass 32 generator and 40 runtime tests, including
  mutation independence, empty collections, disposal during enumeration, and package
  consumer validation with ABI 1.18/bridge 0.19.0.
- Upgrade impact: Re-check collection length/index semantics and map iteration order;
  keep capacity validation and stale-handle rejection fail-closed.
- Removal criteria: Replace with generated safe iterators only after a documented parent
  lifetime, mutation, and early-exit contract is proven for each element family.

## SC-012: Opaque `gp_XYZ` value algebra

- Status: Accepted B07 first geometry value wave.
- Scope: OCCT 8.0.1 `gp_XYZ` default/create/copy/add/cross/dot/modulus/normalized.
- Reason: The type is a C++ value with checked normalization and must not cross through
  an assumed managed object layout. The selected algebra is a closed value-copy family.
- Native/ABI/managed behavior: ABI 1.19 exposes a 24-byte `OcctSharp_Xyz` contract and
  status-returning normalization. `GpXyz` validates finite construction inputs and maps
  zero-normalization failure through `OcctException`.
- Ownership: No allocation or handle ownership; every result is an independent value.
- Validation: Debug/Release pass 32 generator and 42 runtime tests; alpha.17 clean
  package consumer passes with 36 native DLLs, ABI 1.19, and bridge 0.20.0.
- Upgrade impact: Re-check `gp_XYZ` normalization exception text/conditions, arithmetic
  semantics, and value size/alignment on every OCCT/compiler upgrade.
- Removal criteria: Replace with generated value emission after trivial-layout and
  exception-preserving rules cover this family; do not count this manual bridge as a
  generated declaration.

## SC-013: Opaque `gp_Lin` geometry value

- Status: Accepted line-geometry capability milestone inside B.
- Scope: `gp_Lin` default/create/reversed/distance/angle with copied `gp_Pnt` origin and
  unit `gp_Dir` direction.
- Reason: A line embeds native axis state and direction invariants; exposing references
  or C++ layout would make zero-direction failures and lifetime ambiguous.
- Native/ABI/managed behavior: ABI 1.20 exposes a 48-byte origin/direction value. Native
  creation delegates to `gp_Lin(gp_Pnt,gp_Dir)` and catches OCCT construction failures;
  managed `GpLine` remains immutable.
- Ownership: Value-copy only; no allocation or handle ownership crosses the ABI.
- Validation: Debug/Release and alpha.18 package consumer pass; tests cover default Z
  direction, zero-direction rejection, distance, angle, and reversal.
- Removal criteria: Replace with generated value emission after axis composition and
  constructor exception rules are generalized and proven.

## SC-014: Opaque `gp_Circ` geometry value

- Status: Accepted circle-geometry capability milestone inside B.
- Scope: `gp_Circ` default/create/area/length/distance with copied center, normal, and radius values.
- Reason: Circle construction owns axis invariants and raises on negative radius or a zero normal; native object layout and axis references must remain hidden.
- Native/ABI/managed behavior: ABI 1.21 exposes a 56-byte value. Native construction and measurements delegate to OCCT; managed `GpCircle` is immutable and validates finite radius input.
- Ownership: Value-copy only; no allocation or handle ownership crosses the ABI.
- Validation: Debug/Release tests and alpha.19 package consumer pass with 44 runtime tests, ABI 1.21, bridge 0.22.0, and 36 packaged DLLs.
- Removal criteria: Replace with generated value emission after `gp_Ax2` orientation, axis mutation, and constructor exception rules are generalized.

## SC-015: Opaque `gp_Ax3` coordinate-system value

- Status: Accepted axis-geometry capability milestone inside B.
- Scope: `GpAx3Value` over OCCT 8.0.1 `gp_Ax3` default/create/direct semantics.
- Reason: `gp_Ax3` embeds an axis and two derived directions; native references and
  compiler layout must not be projected into managed code. OCCT also rejects zero or
  parallel direction inputs during construction.
- Native/ABI/managed behavior: ABI 1.24 exposes an explicitly sized 96-byte copied value;
  construction is status-returning and `Direct` is normalized to `int32_t`. The managed
  facade is immutable and returns copied `GpXyz` values.
- Ownership: Value-copy only; no allocation or handle ownership crosses the ABI.
- Validation: Release native/managed tests and alpha.21 clean package consumer pass with
  ABI 1.24, bridge 0.25.0, and 36 native DLLs under application-local `occt`.
- Upgrade impact: Re-check direction normalization, directness, exception behavior, and
  value size/alignment on every OCCT/compiler upgrade.
- Removal criteria: Replace with generated value emission after generalized axis
  composition and exception rules cover this family.

## SC-016: Opaque `GProp_GProps` property accumulator

- Status: Accepted property capability milestone inside B.
- Scope: `GPropProperties` over `GProp_GProps`, plus shape-driven `BRepGProp` linear,
  surface, and volume calculations.
- Reason: The native accumulator owns mutable inertia state and returns C++ `gp_Pnt`/
  `gp_Mat` values. Exposing its layout or references would make lifetime and mutation
  unsafe; BRepGProp also has explicit mode and closed-solid semantics.
- Native/ABI/managed behavior: ABI 1.24 adds registry-validated opaque handles,
  explicit mode validation, copied centre/inertia values, clone, and density-weighted
  add. The managed facade owns disposal and rejects invalid density/index inputs.
- Ownership: Every created property result is an owning native allocation released by
  the matching bridge module; source `Shape` remains caller-owned and is not retained.
- Validation: Release runtime and alpha.22 clean package consumer pass for box volume,
  centre, inertia symmetry, clone/composition, invalid density/index, and 36-DLL loading.
- Upgrade impact: Re-check BRepGProp tolerances, only-closed behavior, and matrix
  semantics on every OCCT/compiler upgrade.
- Removal criteria: Replace with generated safe wrappers only after parent/lifetime and
  output-value rules are generalized for GProp families.

## SC-017: Native-local basic BRep builders

- Status: Accepted B09 complete basic construction profile.
- Scope: Box/sphere/cylinder solids plus straight edge, polygon wire, and planar face
  builders backed by BRepPrimAPI/BRepBuilderAPI.
- Reason: Builder objects and C++ topology construction state are native-owned; only
  the resulting `TopoDS_Shape` value should cross through the existing owning handle.
- Native/ABI/managed behavior: Additive ABI 1.24 exports validate finite-positive
  dimensions, contain OCCT exceptions, and return registry-validated `Shape` handles.
- Validation: Release/Debug runtime tests and alpha.32 clean package consumer validate
  topology kinds, invalid dimensions/endpoints/cardinality/type, source independence,
  disposal, and 36-DLL loading.
- Removal criteria: Replace each manual constructor with generated safe builder output
  after constructor/result ownership rules are generalized.

## SC-018: Owning `TopoExp_Explorer` face snapshots

- Status: Accepted B10 complete owning-snapshot profile.
- Scope: `Shape.GetFaces()`, `Shape.GetSubShapes(ShapeKind)`, and the native face/
  subshape snapshot exports for `TopAbs_FACE`, `EDGE`, `WIRE`, and `VERTEX`.
- Reason: Native explorers are parent-bound mutable cursors; exposing them would make
  early exit and parent disposal unsafe. A one-shot copied snapshot gives deterministic
  lifetime and avoids native iterator ABI leakage.
- Ownership: Every returned face is an independent owning `TopoDS_Shape` wrapper. The
  source shape is not retained and may be disposed immediately after the snapshot.
- Validation: Release/Debug runtime and alpha.32 clean package consumer validate
  six faces, 24 edge occurrences, six wires, 48 vertex occurrences, one-face child
  semantics, parent disposal, cleanup, and 36-DLL loading.
- Removal criteria: Replace with generated child iterators only after parent lifetime,
  mutation, and early-exit contracts are generalized for all topology kinds.

## SC-019: Opaque BRepAlgoAPI Fuse/Cut operations

- Status: Accepted B11/B12 complete basic owning-result profiles.
- Scope: `Shape.Fuse` and `Shape.Cut` over OCCT `BRepAlgoAPI_Fuse` and `BRepAlgoAPI_Cut`.
- Reason: Boolean builders own native algorithm state and may raise OCCT exceptions;
  history and builder internals must not cross the C ABI.
- Ownership: Inputs are validated but not retained. Each successful operation returns a
  new owning `Shape` handle with independent disposal.
- Validation: Release/Debug runtime 64/64 and alpha.34 package consumer validate
  transformed overlapping boxes, non-empty results, both-input disposal independence,
  null rejection, and application-local 36-DLL loading.
- Removal criteria: Replace with generated algorithm bindings after generalized history,
  failure, and result ownership rules are proven.

## SC-020: Caller-owned BRep mesh snapshots

- Status: Accepted mesh bulk-transfer capability milestone inside B.
- Scope: `Shape.CreateMesh`, `occtsharp_shape_mesh_count`, and
  `occtsharp_shape_mesh_snapshot` over `BRepMesh_IncrementalMesh` and
  `Poly_Triangulation` for all traversed faces.
- Reason: Triangulation handles, node arrays, and face-local locations are native-owned
  and may be invalidated by remeshing; exposing them would leak parent-bound pointers.
  A count-then-copy ABI provides a bounded bulk transfer without a zero-copy lifetime
  promise.
- Ownership: Managed arrays own copied `MeshVertex` records (position plus normal) and
  32-bit triangle indices. Native mesher and triangulation objects remain bridge-local.
  Three vertices are copied per triangle so no native vertex map or topology identity is
  implied.
- Validation: Release/Debug runtime tests cover non-empty box meshes, index bounds,
  finite positions/normals, invalid deflections, disposed sources, and native build.
- Removal criteria: Replace with generated Poly/RWMesh projections only after an explicit
  stable vertex identity, normals, buffer capacity, and benchmark contract is accepted.

## SC-021: Opaque ShapeFix_Shape healing result

- Status: Accepted B12 complete owning-result/no-history profile.
- Scope: `Shape.Fixed` and `occtsharp_shape_fix` over `ShapeFix_Shape`.
- Reason: Shape-fix modes and status internals are native algorithm state; exposing the
  fixer would require a broader parent/lifetime and history model. The minimal safe
  contract is one contained pass returning a copied owning result.
- Ownership: The input is validated but not retained. A successful non-null result is a
  new registry-validated owning `Shape`, independent of source disposal. OCCT failures
  map to the existing status/diagnostic channel.
- Validation: Release/Debug runtime tests cover non-null result, topology preservation,
  source disposal independence, and native exception containment.
- Removal criteria: Replace with generated ShapeFix/ShapeUpgrade bindings only after
  mode, history, diagnostics, and lifetime semantics are explicitly modeled.

## SC-022: Opaque ShapeUpgrade_UnifySameDomain result

- Status: Accepted B12 complete owning-result/no-history profile.
- Scope: `Shape.UnifiedSameDomain` and `occtsharp_shape_unify_same_domain` with OCCT
  default edge/face unification and BSpline concatenation disabled.
- Reason: The operation owns mutable topology maps and optional `BRepTools_History`;
  exposing those internals would cross an unproven ownership boundary.
- Ownership: Input is validated but not retained. Successful output is a new owning
  registry shape, independent of source lifetime. The native bridge links TKShHealing.
- Validation: Release/Debug runtime and clean package consumer validate non-null output,
  topology presence, source disposal independence, and the native dependency closure.
- Removal criteria: Replace with generated ShapeUpgrade APIs after history, mode, and
  parent-bound topology semantics have dedicated rules and tests.

## SC-023: Interim IGES reader transfer bridge

- Status: Accepted IGES exchange capability milestone inside B.
- Scope: `ShapeExchange.ReadIges` and `occtsharp_shape_read_iges` over
  `IGESControl_Reader` on OCCT 8.0.1.
- Reason: Reader model and transfer roots are native-local; exposing them would require
  a document/metadata lifetime model. A one-shot owning shape matches the existing STEP
  geometry contract.
- Ownership: Reader and transient model are destroyed in the native call. The returned
  `Shape` owns an independent copied `TopoDS_Shape` and survives source disposal.
- Validation: Release/Debug round-trip tests and alpha.29 clean package consumer cover
  non-empty transfer and application-local native loading.
- Removal criteria: Replace with generated reader bindings after document, metadata, and
  transfer-history ownership rules are accepted.

## SC-024: Interim STL reader transfer bridge

- Status: Accepted STL exchange capability milestone inside B.
- Scope: `ShapeExchange.ReadStl` and `occtsharp_shape_read_stl` over `StlAPI_Reader`.
- Reason: STL reader state and per-facet construction remain native-local; a one-shot
  faceted shape is the safe counterpart to the existing STL writer.
- Ownership: The reader is destroyed after the call and the returned faceted shape is
  an independent owning handle.
- Validation: Release/Debug round-trip tests and alpha.30 clean package consumer cover
  non-empty transfer and source-disposal independence.
- Removal criteria: Replace with generated RWStl/Poly bindings after mesh identity and
  bulk ownership rules are accepted.

## SC-025: Null-topology modeling failure contract

- Status: Accepted topology-failure capability milestone inside B.
- Scope: `ShapeFactory.CreateNull` and null validation in Fuse, Cut, Fixed, and
  UnifiedSameDomain.
- Reason: Null `TopoDS_Shape` values are representable but invalid algorithm inputs;
  relying on native exception text is not a stable managed contract.
- Ownership: The null fixture is an owning shape wrapper. Operations reject it before
  OCCT dereference and return `InvalidArgument` with a stable diagnostic.
- Validation: Release/Debug runtime tests and alpha.31 clean package consumer cover
  null inspection, boolean rejection, healing rejection, and disposal.
- Removal criteria: Retain the guard unless a future generated input rule explicitly
  models empty-topology behavior for each algorithm family.

## SC-026: Call-local BRep adaptor value snapshots

- Status: Accepted B08 completion profile.
- Scope: OCCT 8.0.1 `BRepAdaptor_Curve` for an edge and `BRepAdaptor_Surface` for a
  face; toolkits `TKBRep`, `TKGeomBase`, and `TKGeomAlgo` through the existing bridge.
- Reason: Adaptors contain topology references and expose underlying borrowed geometry.
  Returning the adaptor or its references would make source disposal and future OCCT
  upgrades unsafe.
- Native/ABI/managed behavior: ABI 1.25 adds fixed 72-byte edge and 40-byte face
  snapshot structures. Native code copies curve/surface enum values, parameter bounds,
  and edge endpoint coordinates during one call. Managed snapshots are immutable values.
- Ownership: Both snapshots are value copies with no release operation or parent
  lifetime. The input shape is borrowed only for the call and must be a live edge/face.
- Validation: Release/Debug runtime tests cover line/plane semantics, native/managed
  layout, wrong-kind `TypeMismatch`, source disposal, and alpha.33 clean-consumer use.
- Upgrade impact: Re-check enum numeric values, finite curve bounds, UV restriction,
  inherited adaptor behavior, and fixed structure layouts.
- Removal criteria: Replace with generated adaptor projections only when they can emit
  equivalent value-copy operations without exposing borrowed native geometry.

## SC-027: Native-local basic modeling algorithm results

- Status: Accepted B11 complete basic profile.
- Scope: `BRepAlgoAPI_Fuse`, `BRepAlgoAPI_Common`, and
  `BRepExtrema_DistShapeShape` on OCCT 8.0.1.
- Reason: Boolean/extrema builders own mutable execution state, support topology, and
  optional history. Those objects cannot cross the ABI without separate ownership rules.
- Native/ABI/managed behavior: Fuse/Common return existing owning shape handles.
  ABI 1.26 adds a fixed 64-byte distance result containing the minimum distance, one
  copied point pair, and solution count. The native algorithm is destroyed in the call.
- Ownership: Shape results are independent owners. Distance results are pure values.
  Inputs are borrowed only for the call and are never retained by either result.
- Validation: Release/Debug 64/64 runtime tests, layout assertions, null/disposed and
  source-independence paths, freshness, and alpha.34 clean consumer pass.
- Upgrade impact: Re-check completion, null-result, solution indexing, point ordering,
  layout, and toolkit closure.
- Removal criteria: Replace with generated algorithms only after builder/result/history
  descriptors can reproduce the same fail-closed ownership contract.

## SC-028: Native-local mesh-format providers

- Status: Accepted B14 complete geometry-exchange profile.
- Scope: `DEOBJ_Provider`, `DEGLTF_Provider`, and `DEVRML_Provider` read/write plus
  `DEPLY_Provider` write on OCCT 8.0.1.
- Reason: Provider configuration, document/scene state, unit handling, progress, and
  metadata graphs have no accepted cross-ABI ownership contract. Explicit configuration
  nodes are also required for working providers in the pinned build.
- Ownership: Every provider and configuration node is call-local. Writers borrow a live
  shape only for the call and mesh it before export. Readers return one independent
  registered owning shape. No provider, document, label, or mesh pointer escapes.
- Unsupported direction: PLY read is `UNSUPPORTED` because OCCT 8.0.1's provider does
  not implement it; the managed API does not pretend otherwise.
- Validation: Release/Debug 65/65 runtime tests, generated freshness, and the alpha.35
  clean consumer write OBJ/PLY/GLB/VRML, read OBJ/GLB/VRML, and load all 41 packaged
  runtime DLLs from `occt`.
- Removal criteria: Replace with generated DataExchange bindings only after provider,
  configuration, document, unit, progress, and metadata lifetimes are modeled.

## SC-029: OCAF document owner and stable-entry labels

- Status: Accepted B15 complete document profile.
- Scope: `TDocStd_Application`, `TDocStd_Document`, TDF labels/tags/entries,
  `TDataStd_Name`, command transactions, and BinOcaf persistence.
- Reason: `TDF_Label` is a parent-bound value over internal label nodes; copying its C++
  layout or pretending it is independently owned would outlive the data framework.
- Ownership: `OcafDocument` owns a native wrapper retaining application and document
  handles. `OcafLabel` owns no native resource; it stores a stable entry and a strong
  parent reference. Every operation re-resolves the entry and fails after parent dispose.
- Transactions: Mutations require an open command. Uncommitted transaction disposal
  aborts attributes. OCCT retains newly allocated empty label nodes after abort; this is
  documented behavior, while default BinOcaf persistence omits empty labels.
- Validation: Release/Debug 66/66, freshness, and alpha.36 clean consumer cover commit,
  abort, invalid mutation, UTF-8 names, persistence, parent disposal, and the 43-DLL
  runtime closure.
- Removal criteria: Replace with generated OCAF projections only when stable-entry
  parent binding, application/document ownership, transactions, and persistence statuses
  can be emitted equivalently.

## SC-030: Parent-bound XDE labels and copied metadata

- Status: Accepted B16 complete metadata/assembly profile.
- Scope: XCAF shape/color/layer/material tools, assembly occurrences, BinXCAF, and
  STEPCAF read/write.
- Reason: XCAF tools and reference attributes are document-owned and expose label trees;
  returning them as independent managed handles would violate B15 ownership.
- Ownership: `XdeDocument` owns the application/document pair. Labels are stable-entry
  parent-bound values. Shapes and locations are independent owners. Names, colors,
  layers, materials, entries, and counts are caller-owned copies.
- Color rule: Friendly effective color writes Gen/Surf/Curv and reads Gen then Surf then
  Curv so STEPCAF channel normalization does not appear as metadata loss.
- Validation: Release/Debug 67/67, freshness, and alpha.37 clean consumer validate
  memory/BinXCAF/STEPCAF shape, two layers, RGB, material, assembly, occurrence,
  referred part, transform, and 44-DLL loading.
- Removal criteria: Replace with generated XCAF bindings only when document-parent
  relations, reference attributes, copied sequences, and channel semantics are emitted.

## SC-031: Native-owned HWND visualization graph and presentation IDs

- Status: Accepted B17 visualization-core profile.
- Scope: OCCT 8.0.1 Windows `Aspect_DisplayConnection`, `OpenGl_GraphicDriver`,
  `V3d_Viewer`, `AIS_InteractiveContext`, `V3d_View`, `WNT_Window`, and `AIS_Shape`.
- Reason: Viewer objects form a mutable, thread-affine ownership graph and expose
  callbacks, selectors, and transient handles that cannot safely cross a generic C ABI.
- Native/ABI/managed behavior: One registered viewer wrapper owns the complete graph for
  an application-owned HWND. Managed presentations carry parent-scoped 64-bit IDs.
  Applications explicitly forward resize/mouse events; no callback crosses into .NET.
- Ownership: The viewer releases presentations and the OCCT graph but never destroys the
  HWND. Each displayed AIS shape owns a copied topology value. Selection copies IDs into
  caller-owned buffers. Presentation removal and parent disposal invalidate child APIs.
- Validation: Release/Debug 68/68, actual HWND display/redraw/selection, cross-thread
  rejection, source-disposal independence, freshness, interactive sample build, and
  alpha.38 clean consumer with 45-DLL loading all pass.
- Removal criteria: Replace with generated visualization projections only after creator
  threading, parent graphs, callback cancellation/reentrancy, and selector snapshots can
  be emitted with equivalent fail-closed semantics.

## SC-032: Native-local common modeling operations with audited stable IDs

- Status: Accepted common modeling profile inside batch B.
- Scope: OCCT 8.0.1 `BRepPrimAPI_MakeCone`, `BRepPrimAPI_MakeTorus`,
  `BRepPrimAPI_MakePrism`, `BRepPrimAPI_MakeRevol`, `BRepFilletAPI_MakeFillet`,
  `BRepFilletAPI_MakeChamfer`, `BRepOffsetAPI_MakeOffsetShape`,
  `BRepAlgoAPI_Section`, `BRepBndLib::AddOptimal`, `BRepCheck_Analyzer`, and
  `TopExp::MapShapes`; toolkits `TKPrim`, `TKTopAlgo`, `TKBO`, `TKFillet`,
  `TKOffset`, `TKBRep`, and `TKMath`.
- Reason: These builders and analyzers own mutable algorithm, contour, history, progress,
  or referenced topology state. Exposing them as general managed objects would require
  unimplemented history/borrowed/parent-bound contracts. The common result-oriented
  workflows are safe as one-shot calls.
- Native/ABI/managed behavior: ABI 1.33 adds cone/torus creation, extrusion/revolution,
  all-edge and single-edge fillet/chamfer, skin/join offset, shape section, copied finite
  bounds, and validity checks. Friendly `Shape` APIs return independent owners or the
  immutable `BoundingBox3d` value. Public `CountSubShapes` exposes the existing copied
  occurrence-count operation.
- Ownership: Inputs are borrowed only for a call. Builders, indexed edge maps, bounds,
  analyzers, history, and progress state remain native-local. Each topology result is a
  new registered owning `Shape`; bounds and booleans are copied values with no native
  lifetime. Source disposal cannot invalidate a successful result.
- Coverage accounting: Schema 1.6 lists the 18 directly used declaration stable IDs.
  Discovery requires all IDs, the inventory reports them as `Manual/MN001`, and any
  duplicate, unknown, malformed, or emitted/manual overlap fails closed.
- Validation: Release/Debug Generator 44/44 and Runtime 81/81 cover result semantics,
  all/single-edge variants, finite bounds/layout, validity/count, null/disposed/wrong-kind
  and numeric failures, and source independence. The alpha.41 clean consumer loads 47
  application-local DLLs and exercises the new profile.
- Upgrade impact: Re-run semantic discovery before generation; stable-ID signature drift
  is an intentional hard failure. Re-check completion, result nullability, edge membership,
  offset defaults, analyzer behavior, bounding tolerance, and toolkit closure.
- Removal criteria: Replace each manual raw operation only after generalized generated
  algorithm-result descriptors can reproduce the same owning/value, error, history
  exclusion, coverage, and runtime/lifetime evidence.

## SC-033: Native-local high-value geometry and topology workflows

- Status: Accepted current workstream inside batch B; complete-batch exit remains open.
- Scope: 43 OCCT 8.0.1 declarations across `BRepAdaptor`, `BRep_Tool`,
  `BRepBuilderAPI`, `BRepOffsetAPI`, `BRepAlgoAPI`, `BRepPrimAPI`, `GC`, `GCPnts`,
  `Geom`, `GeomAPI`, and `TopExp`. The public families are circle/ellipse/arc/Bezier/
  interpolated edges, curve length/evaluation/projection, surface evaluation/projection,
  topology adjacency, loft, pipe, sewing, Boolean history summaries, wedge, and thick
  solid.
- Reason: The involved OCCT builders, adaptors, projectors, history lists, indexed maps,
  and algorithms carry mutable, borrowed, or parent-related state for which the general
  generator does not yet emit a safe ownership graph. The selected high-frequency
  workflows can safely keep that state inside one native call and cross only owning
  topology or copied snapshots.
- Native/ABI/managed behavior: ABI 1.34 adds one-shot native operations. Successful
  topology results are registered owning `Shape` values. Curve/surface results are fixed
  copied points, vectors, parameters, distances, and counts. Adjacency is copied into
  independent owning shapes plus compact managed offset/index arrays. Boolean history
  returns an owning result and copied modified/generated/deleted summaries.
- Ownership: Every input shape is borrowed only for the call. Multi-shape calls acquire
  all `SafeHandle` references before entering native code and release them in reverse
  order. No adaptor, curve/surface handle, projector, OCCT list/map, builder, progress
  object, or history object crosses the ABI. Returned shapes and snapshots remain valid
  after all inputs are disposed.
- Coverage accounting: Schema 1.6 lists all 43 directly used stable IDs. Discovery must
  find every ID; inventory reconciliation reports them as `Manual/MN001`. Unknown,
  duplicate, malformed, or emitted/manual-overlap IDs fail generation.
- Validation: Current Debug validation passes Generator 44/44 and Runtime 90/90,
  including success, wrong-kind, empty, invalid-number, disposal, layout, adjacency,
  builder, and history behavior. Release, freshness, full inventory, alpha.42 package
  consumer, and release-check evidence remain required before accepting the current
  package evidence chain.
- Upgrade impact: Re-run stable-ID discovery and re-check parameter ranges, curve/surface
  domains, tolerance behavior, builder completion/null results, topology-map ordering,
  history semantics, toolkit closure, and ABI layouts on every OCCT upgrade.
- Removal criteria: Replace each operation only when generated algorithm/adaptor/history
  descriptors reproduce the same call-local ownership, copied-result, validation,
  coverage, and runtime/lifetime guarantees.

## SC-034: Generated incremental-allocator placement construction

- Status: Accepted generator exception inside batch B; full-wave compile and runtime
  validation remain open.
- Scope: OCCT 8.0.1 `BRepMeshData_Curve` construction with
  `Handle<NCollection_IncAllocator>`.
- Reason: `DEFINE_INC_ALLOC` removes ordinary allocation and supplies allocator placement
  new plus a no-op ordinary delete. Emitting `new T(allocator)` is ill-formed, while
  forcing global allocation would mismatch the class deletion contract.
- Generated behavior: Configuration schema 1.8 marks the exact native type. The emitter
  requires exactly one generated incremental-allocator parameter, calls
  `new (allocator) T(allocator)`, and stores an additional allocator handle in every
  native wrapper and clone.
- Ownership: The wrapper declares the retained allocator before the object handle, so
  reverse field destruction releases the object while allocator storage remains alive.
  Managed construction rejects a null allocator. No allocator pointer crosses the ABI.
- Validation: Generator 53/53 passes. Full Release native/managed compile, allocator
  lifetime runtime stress, Debug, package consumer, and release gates are `NOT RUN`.
- Upgrade impact: Recheck the type's allocation macros, constructor parameter, member
  allocator retention, and delete behavior on every OCCT upgrade.
- Removal criteria: Replace the explicit type marker only after semantic discovery models
  class-specific allocation/deallocation operators and proves equivalent lifetime order.

## SC-035: Final long-tail disposition and export-proof boundary

- Status: Accepted for the alpha.49 Batch B completion gate.
- Scope: Full OCCT 8.0.1 observed declaration inventory and standalone generated enum,
  static method, and free-function selection.
- Reason: The former LT001-LT004 buckets mixed non-callable type metadata, destructors,
  abstract/pure-virtual surfaces, pointer/reference lifetime, handle targets, templates,
  unmapped values, and link-unproven free functions. A single broad blocker could not
  prove whether a declaration was safely bindable or deliberately excluded.
- Generated behavior: Named Int32-compatible enums are emitted independently of callable
  references. Anonymous enum declarations are `SK017`. Void returns reuse the no-value
  TM000 projection. Free functions are generated only for the export-proven Standard
  foundation profile; missing toolkit provenance and other unverified exports are BL002
  and BL003. Exact method/function scopes include the declaring header and cannot overlap
  a broader automatic prefix.
- Ownership: No new pointer, reference, borrowed view, or C++ layout crosses the ABI.
  BL102/BL103 and BL202-BL208 retain the exact unresolved ownership/type boundary.
- Validation: Release and Debug native/managed builds pass; Generator 62/62, Runtime
  105/105, discovery/report determinism, and dependency profiles 6/6 pass. The final
  inventory has 16,353 emitted, 61 manual, zero supported-unselected, zero LT001-LT004,
  zero pending/HD099, and 50,514 narrow blocked dispositions.
- Upgrade impact: Re-run exact symbol/export inspection before broadening any free-function
  profile. Recompute every reason count and fail the completion gate if LT001-LT004,
  supported-unselected, pending, or HD099 reappears.

## SC-036: Batch C common CAD cross-family workflow facade

- Status: Accepted for the first Batch C large-wave checkpoint.
- Scope: Nine directly used OCCT 8.0.1 declarations from `BRepTools`, `BRep_Tool`,
  `Poly_Triangulation`, and `AIS_InteractiveContext`, combined with already emitted
  `TopExp`, `BRepCheck`, `BRepMesh`, `Poly`, `V3d`, and AIS operations. The friendly
  surface covers native BREP exchange, whole-shape topology/tolerance inspection,
  detailed mesh snapshots, XDE part metadata convenience, and viewer appearance,
  camera, and selection modes.
- Reason: BREP stream/build state, typed topology references, triangulation-owned node
  data, and viewer-owned presentations cannot safely cross the stable C ABI as borrowed
  OCCT objects. Keeping them call-local unlocks one common workflow across five connected
  families without exposing C++ layout or lifetime.
- Native/ABI/managed behavior: ABI 1.42 adds fixed copied topology, tolerance, mesh-node,
  UV, triangle, face-index, and Boolean/scalar records plus status-returning BREP and
  viewer calls. Existing generated declarations reused by the facade are not counted as
  manual. `XdeDocument.AddPart` composes the existing transaction-bound metadata calls.
- Ownership: BREP and topology inputs are borrowed for one call; returned shapes are
  registered owning copies. Detailed mesh arrays are caller-owned snapshots and retain
  no `Poly_Triangulation`, face, location, or shape reference. XDE labels remain parent-
  bound. Viewer presentations remain parent-bound and every viewer operation remains
  owner-thread-affine.
- Coverage accounting: Configuration schema 1.8 lists the nine newly direct manual
  stable IDs. Discovery must find each ID and must reject emitted/manual overlap. Methods
  already emitted by the generated surface, including UV/normal presence, transparency,
  display mode, clear selection, projection, zoom, and pan, stay outside the manual
  denominator.
- Validation: Release and Debug native/managed builds, runtime workflows, clean package
  consumer, generated freshness/regeneration, inventory, and local release gates pass at
  the alpha.51 checkpoint.
- Upgrade impact: Recheck BREP format defaults, shape closedness, topology-map ordering,
  tolerance semantics, `Poly_Triangulation` node/UV/normal conventions, reversed-face
  winding, selection schemes, Z-up orientations, and AIS display-mode indices on every
  OCCT upgrade.
- Removal criteria: Replace the exception only after generalized generated descriptors
  can reproduce the same copied snapshots, owning results, parent/thread boundaries,
  validation, and end-to-end runtime evidence.

## SC-037: STEP import diagnostics, BRepCheck issue snapshot, and repair comparison

- Status: Accepted for the second Batch C cross-family checkpoint.
- Scope: Six directly used OCCT 8.0.1 declarations from `STEPControl_Reader`,
  `XSControl_Reader`, `BRepCheck_Analyzer`, and `BRepCheck_Result`, composed with the
  existing ShapeFix owning-result bridge and generated V3d rotation operations.
- Reason: Reader/work-session transfer state, analyzer result handles, status lists, and
  subshape iterators are borrowed or algorithm-owned. They cannot become independent
  managed objects without adding unsafe lifetime coupling. A call-local copied report
  closes the common import-diagnose-repair workflow across four families.
- Native/ABI/managed behavior: ABI 1.43 adds fixed 24-byte STEP read reports, fixed 8-byte
  validation issues, a two-call validation count/snapshot protocol, and thread-affine
  V3d start/continue rotation exports. The friendly API exposes typed read/validation
  statuses and a repaired owning shape with immutable before/after reports.
- Ownership: STEP readers, work sessions, transfer roots, BRepCheck analyzers/results/
  lists, and ShapeFix algorithm state remain native-local. STEP read and repair return
  independent registered owning shapes. Reports contain only copied scalar/enum values;
  viewer rotation mutates only its parent viewer on the creating thread.
- Coverage accounting: Configuration schema 1.8 lists the six newly direct manual stable
  IDs. Existing generated or previously reconciled declarations used by the workflow are
  not counted again. Discovery rejects missing IDs and generated/manual overlap.
- Validation: Release and Debug native/managed builds pass with Generator 62/62 and
  Runtime 107/107. The clean package consumer, full inventory, regeneration, and local
  release gates are required at the alpha.52 checkpoint.
- Upgrade impact: Recheck `IFSelect_ReturnStatus` values, reader transfer-count and unit
  semantics, `BRepCheck_Status` ordering, analyzer exact/geometric modes, status-list
  stability, ShapeFix output ownership, and V3d rotation threshold behavior on every
  OCCT upgrade.
- Removal criteria: Replace the exception only after generated reader/analyzer/result
  descriptors can reproduce the same call-local state, copied reports, owning shapes,
  option validation, and runtime/lifetime evidence.

## SC-038: XDE validation properties, recursive occurrences, and STEPCAF options

- Status: Accepted for the third Batch C cross-family checkpoint.
- Scope: Nine directly used OCCT 8.0.1 declarations covering `Get`, `Set`, and `GetID`
  on `XCAFDoc_Area`, `XCAFDoc_Volume`, and `XCAFDoc_Centroid`, composed with existing
  BRepGProp, XDE assembly/location, and STEPCAF reader/writer facilities.
- Reason: XCAF attributes are document-owned and transaction-bound, while assembly
  component/reference traversal and STEPCAF mode objects are parent- or call-local.
  Exposing those native objects would add unsafe borrowed lifetime coupling. Copied
  nullable values, parent-bound labels, owning locations/shapes, and call-local exchange
  state close the workflow without crossing native layouts.
- Native/ABI/managed behavior: ABI 1.44 adds a fixed 56-byte validation-property record,
  read/set exports, and option-bearing XDE STEP read/write exports. The existing no-option
  exports retain all-metadata defaults. Managed APIs compute properties through existing
  BRepGProp owners, flatten direct or recursive occurrences with cycle rejection, compose
  world locations, and select STEP representation plus name/color/layer/property/material
  modes.
- Ownership: Area, volume, centroid, occurrence entries, and paths are copied values.
  `XdeOccurrence` owns one independent composed `TopLocLocation`; its located shape is a
  separate registered owner. Labels remain parent-bound to their XDE document. STEPCAF
  readers/writers, XCAF tools, sequences, references, and attribute handles remain native-
  local, and validation-property mutation requires an open document transaction.
- Coverage accounting: Configuration schema 1.8 lists exactly nine SC-038 stable IDs.
  Discovery requires all nine and rejects overlap with generated ownership. Existing
  BRepGProp/location/STEPCAF declarations are not counted again.
- Validation: Release and Debug native/managed builds pass with Generator 62/62 and
  Runtime 108/108. Nested assembly traversal composes world translation `(11,22,33)`,
  creates an independent located shape, round-trips complete properties through BinXCAF
  and STEPCAF, verifies reader/writer filters, clears attributes transactionally, and is
  repeated by the clean alpha.53 package consumer.
- Upgrade impact: Recheck XCAF attribute GUIDs and missing-value behavior, BRepGProp mass/
  centroid conventions, `TopLoc_Location` multiplication order, XDE reference-cycle
  behavior, STEP model-type numeric values, and every STEPCAF mode on each OCCT upgrade.
- Removal criteria: Replace the exception only after generated document-attribute and
  traversal descriptors preserve the same copied/parent-bound/owning contracts, option
  validation, deterministic stable-ID accounting, and end-to-end evidence.

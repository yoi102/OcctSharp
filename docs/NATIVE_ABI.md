# Native ABI

## Scope

The native bridge is implemented in C++ and links to OCCT, but its exported surface
is a versioned C ABI. Managed code must not bind directly to OCCT C++ symbols.

## ABI rules

1. Export names are explicit and stable; C++ name mangling is not part of the ABI.
2. Calling convention is explicit for every supported platform.
3. ABI integers use fixed-width types where width matters.
4. ABI booleans use an explicitly sized integer representation.
5. Enums have an explicit underlying representation.
6. Struct layout, alignment, and field order are documented and verified.
7. Strings specify encoding, length unit, null behavior, and ownership.
8. Arrays specify element count, element layout, and allocation ownership.
9. C++ classes, references, exceptions, templates, and STL containers never cross
   the ABI directly.
10. Memory is released by the same native module and allocation family that created it.

## Handle model

Native objects exposed across the boundary use opaque handles. Before implementation,
the handle representation must specify:

- Owning, shared, borrowed, or value-copy category.
- Runtime type identity and legal casts.
- Null representation.
- Destruction function and idempotency guarantees.
- Parent/child lifetime relationship.
- Thread-affinity or concurrency restrictions.
- Behavior after disposal and invalid-handle detection.

Passing an untyped pointer is not sufficient evidence of ownership or runtime type.

## Error model

Every exported operation that can fail must participate in one consistent error
contract. The native boundary catches at least:

- `Standard_Failure`.
- `std::exception`.
- Unknown C++ exceptions.

The managed side receives a stable error category/code and UTF-8 diagnostic text,
then creates an appropriate managed exception. The lifetime of diagnostic text must
not depend on an unsafe temporary buffer. No failure may be converted silently into
a successful default value.

ADR-0005 selects a stable status return plus out parameters and a thread-local UTF-8
diagnostic string for the initial ABI. Managed code reads the diagnostic immediately
after a failed call. Future result shapes require an ABI-versioned ADR.

ABI 1.1 adds `FileIoError` and `TransferFailed` without changing existing numeric
values. File paths are passed as non-owning UTF-8 strings valid for the call. Successful
shape reads and transforms return new owning `OcctSharp_ShapeHandle` values; compound
addition copies the OCCT topology value into the target compound and does not transfer
ownership of the child handle.

The current exchange exports are ordinary geometry operations:

- STEP reads all transferable roots and returns `OneShape()`.
- STEP writes one shape or compound with `STEPControl_AsIs`.
- STL performs explicit incremental meshing before writing.
- IGES writes millimeter BRep geometry.
- Rigid transforms rotate about the origin and then translate.

ABI 1.2 adds a one-shot metadata-preserving STEPCAF/XDE merge operation. XDE document,
label, and attribute lifetimes remain entirely inside the native call; no OCAF pointer,
label, or `Handle<T>` crosses the ABI.

```c
typedef struct OcctSharp_StepAssemblyInput
{
  const char* file_path;
  double translation_x;
  double translation_y;
  double translation_z;
  double rotation_axis_x;
  double rotation_axis_y;
  double rotation_axis_z;
  double rotation_angle_radians;
} OcctSharp_StepAssemblyInput;

OcctSharp_Status occtsharp_step_merge_xde(
  const OcctSharp_StepAssemblyInput* inputs,
  int32_t input_count,
  const char* output_path);
```

`inputs` is a non-null, call-bound contiguous array with at least one element;
`file_path` and `output_path` are non-null UTF-8, null-terminated paths valid for the
call. On Windows x64 the struct is 64 bytes with 8-byte alignment: `file_path` is at
offset 0, `translation_x` at 8, and `rotation_angle_radians` at 56. Native compile-time
assertions and managed runtime tests verify that layout.

Every input is read by `STEPCAFControl_Reader` into a source XDE document. Its complete
label tree and supported metadata are cloned into one output document, then placed as a
component below an `OcctSharp Assembly` root using a rigid `TopLoc_Location`. The writer
uses `STEPCAFControl_Writer` with color, name, layer, property, metadata, SHUO, dimension/
tolerance, material, and visual-material modes enabled. The rigid transform is rotation
about the origin followed by translation; all values must be finite and the axis non-zero.

ABI 1.3 adds the first generated value-copy operation:

```c
typedef struct OcctSharp_Point3d
{
  double x;
  double y;
  double z;
} OcctSharp_Point3d;

OcctSharp_Point3d occtsharp_generated_gp_pnt_create(double x, double y, double z);
```

`OcctSharp_Point3d` is 24 bytes with 8-byte alignment on the current Windows x64
baseline. The implementation constructs an OCCT `gp_Pnt` and copies its accessor
values into the ABI struct; native `gp_Pnt` layout never crosses the boundary. This
operation has no native lifetime and follows value-copy ownership.

ABI 1.4 adds 28 generated static value-copy exports: 20 `Precision::*` methods, three
`TopAbs::*` enum methods, and five scalar methods from `Standard`, `TopLoc`, and `gp`.
It also carries the default and copy `gp_Pnt` constructors alongside the coordinate
constructor, for 31 generated declarations total. `double` values cross directly, enum
values use validated `int32_t`, and `Standard_Boolean` results cross as normalized
`int32_t` zero or one. Native export names contain a deterministic overload ordinal
derived from normalized signature order, such as
`occtsharp_generated_precision_p_approximation_0` and `_1`. No OCCT object, reference,
pointer, or ownership-bearing value crosses these exports. `Standard::Purge` remains
excluded because its process-wide side effects are outside the value-copy ABI contract.

## ABI versioning

The native library must expose a queryable ABI version and build identity. Managed
initialization must reject incompatible major ABI versions with a clear diagnostic.
Additive compatibility within an ABI major version is a goal, not an assumption; it
must be tested against the supported package matrix.

ABI 1.5 adds `OCCTSHARP_STATUS_INVALID_HANDLE` (8). Shape handles are registered while
live; all shape operations validate registration before dereferencing, and repeated
release is a no-op. This guard does not claim concurrent release/use safety or implement
OCCT `Handle<T>` reference counting.

ABI 1.6 adds an experimental `Standard_Transient` shared-handle probe. Its wrapper owns
one `opencascade::handle<Standard_Transient>` and exposes create, null-create, clone,
null-state, reference-count, and release operations. Only opaque wrapper pointers cross
the C ABI; the OCCT object pointer and intrusive counter remain native-local. Wrapper
registration still rejects stale pointers and repeated release is safe.

ABI 1.7 adds shared-handle runtime type identity queries: an OCCT RTTI type name and an
`IsKind` check against a UTF-8 type name. The derived probe uses OCCT's registered RTTI;
compiler-mangled `typeid` names and `Standard_Type` pointers never cross the ABI.

ABI 1.8 adds the experimental `occtsharp_transient_try_cast_derived` operation. It
validates that the live, non-null wrapper is an `OcctSharp_TransientDerived` through
OCCT RTTI before copying the native shared handle. Incompatible and null values return
`OCCTSHARP_STATUS_TYPE_MISMATCH` (9), and the output remains null on failure.

ABI 1.9 adds generated per-type shared-handle operations. The first scope wraps
`Geom_CartesianPoint` construction, coordinate/value access and mutation, clone,
reference count, RTTI name/`IsKind`, and idempotent release. Each opaque
`OcctSharp_GeomCartesianPointHandle` contains one intrusive OCCT handle and is validated
by its per-type registry before use. All generated translation units preserve the same
thread-local error contract through the bridge-internal error setter.

ABI 1.10 adds eight generated `TopoDS_Shape` value operations: clone, null state,
shape kind, orientation, reversal, `IsPartner`, `IsSame`, and `IsEqual`. Every result
shape is a newly registered opaque wrapper owning an independent C++ value. OCCT's
internal `TShape` sharing, location, and orientation semantics are preserved; no native
shape layout or object pointer crosses the C ABI. Shape-kind and orientation values use
validated 32-bit enum projections.

ABI 1.11 adds eight generated checked topology conversions from `TopoDS_Shape` through
`TopoDS::Compound`, `CompSolid`, `Solid`, `Shell`, `Face`, `Wire`, `Edge`, and `Vertex`.
Each successful result is a newly registered opaque shape value. OCCT
`Standard_TypeMismatch` is caught inside the bridge and returned as status 9.

ABI 1.12 adds an opaque registry-validated `gp_Trsf` value bridge: identity and
finite translation/rotation construction, clone, inversion, multiplication, 1-based
3x4 matrix reads, release, and shape application. Results are independent native
values and no `gp_Trsf` layout crosses the boundary.

ABI 1.13 adds an opaque registry-validated `TopLoc_Location` value bridge: identity,
construction from `gp_Trsf`, clone, inversion, multiplication, identity query,
conversion back to `gp_Trsf`, and absolute/relative shape placement.

ABI 1.14 completes the B05 transformation family with opaque registry-validated
`gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` handles. Vector operations include finite
construction, clone, component reads, magnitude, dot, and cross; directions add
non-zero validation and reversal; axes add origin/direction reads and reversal; matrices
add nine-value construction, identity, clone, 1-based value reads, and determinant.
Vector translation and axis rotation create independent `gp_Trsf` results. These
operations never expose C++ layouts, and all handles use the same stale-handle and
thread-local diagnostic contract.

The current native ABI is 1.14 and the bridge implementation version is 0.15.0.

ABI 1.15 begins B06 with opaque registry-validated `TCollection_AsciiString`,
`TCollection_ExtendedString`, and `NCollection_Sequence<double>` handles. String
creation and mutation accept explicit UTF-8 byte buffers; reads copy UTF-8 bytes into
caller-provided buffers and never return OCCT-owned pointers. Extended strings expose
UTF-16 code-unit length/value plus UTF-8 length/conversion. Real sequences copy finite
`double` values, preserve native clone/mutation semantics, and translate friendly 0-based
indices to OCCT's 1-based sequence operations. No C++ string/container layout crosses
the ABI.

The current native ABI is 1.15 and the bridge implementation version is 0.16.0.

ABI 1.16 adds opaque registry-validated `NCollection_Array1<double>` and
`NCollection_Vector<double>`/`NCollection_DynamicArray<double>` value collections.
Array creation copies finite values into a native 1-based array and exposes its lower
bound explicitly; managed callers translate a 0-based index. Vector creation copies
finite values into the OCCT 8 dynamic-array implementation, whose native indices are
zero-based. Both support clone, count/value reads, bounded mutation, enumeration by
individual value calls, and idempotent release without crossing container layout.

The current native ABI is 1.16 and the bridge implementation version is 0.17.0.

ABI 1.17 adds opaque registry-validated `NCollection_DataMap<int,double>` and
`NCollection_IndexedMap<int>` value collections. Data maps copy finite values and expose
key lookup, bind, unbind, extent, clone, and bound checks. Indexed maps copy unique keys
and expose ordered 1-based native index/key lookup through a 0-based managed view,
append, last-item removal, clone, and idempotent release. No hash buckets, node pointers,
or native iterators cross the ABI.

The current native ABI is 1.17 and the bridge implementation version is 0.18.0.

ABI 1.18 adds one-shot caller-owned snapshot exports for the scalar sequence, array,
vector, integer-real map, and indexed map families. The native side validates the live
registry handle and destination capacity, copies values into caller-provided buffers,
and returns the number written. No iterator object, node pointer, or container layout
crosses the ABI; snapshots remain valid after subsequent native mutation or disposal.

The current native ABI is 1.18 and the bridge implementation version is 0.19.0.

ABI 1.19 adds the explicitly sized `OcctSharp_Xyz` value-copy exports for the first B07
geometry family. Default/create/copy/add/cross/dot/modulus are non-throwing value calls;
normalization uses the status/diagnostic contract so zero vectors fail closed.

The current native ABI is 1.19 and the bridge implementation version is 0.20.0.

ABI 1.20 adds the explicitly sized 48-byte `OcctSharp_Line` value-copy exports for the
first line primitive. Construction delegates to `gp_Lin`/`gp_Dir` and preserves the
zero-direction failure; reversal, point distance, and line angle remain native OCCT
operations.

The current native ABI is 1.20 and the bridge implementation version is 0.21.0.

ABI 1.21 adds the explicitly sized 56-byte `OcctSharp_Circle` value-copy exports for
`gp_Circ` default/create/area/length/distance. Native construction preserves OCCT axis,
normal, and negative-radius failures.

The current native ABI is 1.21 and the bridge implementation version is 0.22.0.

ABI 1.22 and 1.23 add the explicitly sized `OcctSharp_Ax2` and `OcctSharp_Plane`
value-copy exports for right-handed orientation and signed plane distance. ABI 1.24
adds the explicitly sized 96-byte `OcctSharp_Ax3` value-copy exports for coordinate
system construction and OCCT directness evaluation. Construction remains status-returning
so zero or parallel directions cannot silently cross the boundary.

The current native ABI is 1.24 and the bridge implementation version is 0.32.0.

The B08 property bridge is additive within ABI 1.24: `GProp_GProps` is represented by
an owning opaque handle with registry validation. Shape-driven BRepGProp calculations
accept an explicit mode (linear/surface/volume), and mass, centre, inertia reads, clone,
and density-weighted composition stay inside the C ABI. No native property layout crosses.

The first B09 construction wave is also additive within ABI 1.24: sphere and cylinder
exports validate dimensions and return the existing registry-validated owning shape
handle. `BRepPrimAPI` builder objects never cross the boundary.

The first B10 traversal wave is additive within ABI 1.24: subshape snapshots accept a
caller-owned opaque-handle buffer and a validated `TopAbs` kind, copy face/edge/wire/
vertex values into registry-validated owners, and return the written count. Native
explorers and parent references never cross the C ABI; managed disposal releases each
returned owner.

The first B11/B12 boolean wave is additive within ABI 1.24: Fuse and Cut accept two
validated shape handles, contain OCCT algorithm failures, and return a new owning shape
handle. Algorithm history and native builder state remain bridge-local.

The first B12 healing and B13 bulk waves are additive within ABI 1.24. B12 adds
`occtsharp_shape_fix`, which keeps `ShapeFix_Shape` native-local and returns a new
owning shape after a contained OCCT failure check. B13 adds
`occtsharp_shape_mesh_count` and
`occtsharp_shape_mesh_snapshot` run `BRepMesh_IncrementalMesh` and copy triangulated
face data into caller-owned vertex/normal and 32-bit index buffers. Every triangle owns
three copied vertices, face orientation is reflected in winding and normals, and no
`Poly_Triangulation` or native array pointer crosses the ABI. The two-call count/snapshot
contract rejects invalid deflections and undersized buffers before writing past a caller
buffer. B12 also adds `occtsharp_shape_unify_same_domain`, which keeps
`ShapeUpgrade_UnifySameDomain` native-local and returns an owning unified result;
the bridge implementation advances to 0.28.0.

The first B14 exchange extension adds `occtsharp_shape_read_iges`, which keeps
`IGESControl_Reader` and transfer-root state native-local and returns one owning
shape after file/transfer validation. The bridge implementation advances to 0.29.0.

The same B14 extension adds `occtsharp_shape_read_stl`, which keeps
`StlAPI_Reader` native-local and returns one owning faceted shape after file and
null-result validation. The bridge implementation advances to 0.30.0.

The B12 failure contract adds `occtsharp_shape_create_null` as a diagnostic fixture
and rejects null topology values in Fuse, Cut, ShapeFix, and UnifySameDomain before
calling OCCT. These paths return `InvalidArgument` with a stable diagnostic; the
bridge implementation advances to 0.31.0.

The B09 completion adds straight-edge, polygon-wire, and planar-face owning builder
exports. Point arrays are copied for the call, planar face inputs require a live wire,
and builder state stays native-local. The bridge implementation advances to 0.32.0.

ABI 1.25 completes the B08 safe adaptor snapshot profile with
`occtsharp_shape_edge_curve_snapshot` and `occtsharp_shape_face_surface_snapshot`.
The 72-byte edge value contains a 32-bit `GeomAbs_CurveType`, finite first/last
parameters, and two copied `OcctSharp_Xyz` endpoints. The 40-byte face value contains
a 32-bit `GeomAbs_SurfaceType` and four copied UV bounds. Native compile-time and
managed runtime assertions verify size and offsets. Adaptors and their borrowed
geometry remain call-local; wrong shape kinds return `TypeMismatch`. The bridge
implementation advances to 0.33.0.

The current native ABI is 1.25 and the bridge implementation version is 0.33.0.

ABI 1.26 completes the B11 basic modeling-result profile. The additive
`occtsharp_shape_boolean_common` export returns a registered owning shape from a
native-local `BRepAlgoAPI_Common`. `occtsharp_shape_distance` uses native-local
`BRepExtrema_DistShapeShape` and copies a 64-byte result containing the minimum
distance, the first corresponding point on each input, and solution count. No support
topology, history, or algorithm state crosses the ABI. The bridge advances to 0.34.0.

The current native ABI is 1.26 and the bridge implementation version is 0.34.0.

ABI 1.27 completes the B14 geometry-exchange profile with one-shot OBJ, glTF/GLB,
and VRML read/write exports plus PLY write. Each call creates an explicit format
configuration node and provider, keeps provider/document/scene state native-local,
and returns the existing registered owning shape category on reads. Writers mesh the
input before export. OCCT 8.0.1 does not implement PLY import, so no PLY read export is
declared. The runtime closure adds TKDEOBJ, TKDEPLY, TKDEGLTF, TKDEVRML, and TKRWMesh.

The current native ABI is 1.27 and the bridge implementation version is 0.35.0.

ABI 1.28 adds the B15 OCAF document profile. One registered owning document wrapper
retains `TDocStd_Application` and `TDocStd_Document`; labels cross only as stable UTF-8
TDF entries and are resolved per call. Command begin/commit/abort, child-tag creation,
child count, copied `TDataStd_Name`, and BinOcaf save/open exports contain all OCAF
objects and exceptions inside the bridge. Release aborts any open command and closes
the application session. TKBin and TKBinL expand the runtime closure to 43 DLLs.

The current native ABI is 1.28 and the bridge implementation version is 0.36.0.

ABI 1.29 adds the B16 XDE metadata/assembly profile. XDE documents reuse the registered
application/document owner with BinXCAF drivers. Shape/assembly creation, component
occurrences, referred labels, locations, free shapes, copied RGBA/layers/materials, and
STEPCAF read/write are status-returning exports. Stable entries and caller-owned copies
cross the ABI; XCAF tools, sequences, reference tree nodes, and transfer state do not.
The effective-color export maps Gen/Surf/Curv channels to survive STEP normalization.
TKBinXCAF expands the runtime closure to 44 DLLs.

The current native ABI is 1.29 and the bridge implementation version is 0.37.0.

ABI 1.30 adds the B17 Windows visualization profile. One registered viewer owner holds
the display connection, OpenGL driver, V3d viewer, AIS context, view, and WNT window.
Presentation IDs, visibility/removal, resize/redraw/fit, mouse detection/selection, and
caller-owned selected-ID snapshots are status-returning exports. The HWND remains
application-owned, calls are checked against the creating thread, and no callback or
OCCT visualization pointer crosses the ABI. TKOpenGl expands the runtime closure to
45 DLLs.

The current native ABI is 1.30 and the bridge implementation version is 0.38.0.

ABI 1.31 adds the alpha.39 generated StepBasic scalar/shared-entity profile. Ten generated
registries retain `Handle<T>` values for Address, date/time, dimensions, Person, and
SiUnit entities. Constructors, scalar/boolean/enum members, clone, RTTI, reference count,
and release use the existing status and exception boundary. Enum values cross only as
validated `int32_t`; native enum and entity layouts do not cross the ABI.

The current native ABI is 1.31 and the bridge implementation version is 0.39.0.

ABI 1.32 adds the alpha.40 package-expanded StepBasic shared-entity closure. The generator
uses the same per-type registry, retained clone, RTTI, reference-count, status, and
exception contract for 129 public managed StepBasic types. The manifest owns 333 stable
IDs; no entity layout or raw `Handle<T>` pointer crosses the ABI. Large generated MSVC
translation units compile with `/bigobj` without changing the public contract.

The current native ABI is 1.32 and the bridge implementation version is 0.40.0.

ABI 1.33 adds the alpha.41 common-modeling profile. Cone/torus, prism/revolution,
all-edge and single-edge fillet/chamfer, skin/join offset, and shape section exports
return new registered owning shapes. Bounding-box extraction copies six doubles through
a fixed 48-byte structure; validity and subshape occurrence counts copy scalars. Builder,
indexed-edge, history, progress, `Bnd_Box`, and analyzer state remain native-local.
`TKFillet` and `TKOffset` expand the application-local runtime closure to 47 DLLs.

The current native ABI is 1.33 and the bridge implementation version is 0.41.0.

ABI 1.34 adds the current high-value geometry/topology workstream inside batch B.
One-shot exports cover circle/ellipse/arc/Bezier/interpolated edge construction,
curve/surface evaluation and projection, curve length, owning topology-adjacency
snapshots, loft, pipe, sewing, wedge, thick solid, and copied Boolean history summaries.
Call-local builders, adaptors, projectors, indexed maps, lists, progress state, and
history objects do not cross the ABI. Curve/surface/history data crosses as fixed copied
values; every returned topology value is registered and owning.

ABI 1.34 also adds transaction-bound STEPCAF import into an existing owned XDE document.
The source document, XCAF tools, clone maps, and material maps stay native-local; newly
imported roots cross as destination-parent-bound stable entries. The compatibility
one-shot STEP assembly export remains present.

The current native ABI is 1.34 and the bridge implementation version is 0.42.0.

ABI 1.35 expands the existing generated intrusive shared-handle contract across selected
`Geom` and `Geom2d` types. Eight new public wrappers add 67 manifest stable IDs for 2D
Cartesian points, 2D/3D directions and vectors with magnitude, 2D/3D transformations,
and planes. Registry ownership, retained clone, RTTI, reference count, status, exception,
and release semantics are unchanged. Parameters and results cross only through existing
scalar, enum, and copied `gp_Pnt` projections; no OCCT geometry layout or raw
`Handle<T>` pointer crosses the ABI.

The current native ABI is 1.35 and the bridge implementation version is 0.43.0.

ABI 1.36 expands the same generated intrusive shared-handle ABI to selected BRepMesh,
Poly, ShapeAnalysis, ShapeFix, and ShapeUpgrade records. Sixty-one new public wrappers
add 375 manifest stable IDs. No new pointer category is introduced: each wrapper owns
one typed OCCT intrusive handle, clones retain it, scalar/enum/value-copy calls reuse
the existing projections, and native exceptions stay inside the status boundary.
Binding-model schema 1.2 records abstract records and prevents their construction.

ABI 1.37 expands the generated intrusive shared-handle ABI to concrete `StepGeom`,
`StepRepr`, `StepShape`, and `StepVisual` records with safe scalar/enum/value-copy members.
It is retained as the alpha.45 baseline.

ABI 1.38 generalizes parameters and returns between selected generated
`opencascade::handle<T>` types. A null native wrapper pointer maps to a null OCCT handle;
non-null inputs are validated through the target type registry. Non-null results allocate
a new target wrapper that retains its own intrusive reference. The C ABI exposes only
typed opaque wrapper pointers and never an OCCT handle layout.

ABI 1.39 expands the same generated shared-handle ABI across selected `StepAP203`,
`StepAP214`, `StepAP242`, `StepDimTol`, `StepElement`, `StepFEA`, and
`StepKinematics` entities. It introduces no new ownership category or ABI layout.

ABI 1.40 expands the same generated shared-handle ABI across selected `IGESAppli`,
`IGESBasic`, `IGESDefs`, `IGESDimen`, `IGESDraw`, `IGESGeom`, `IGESGraph`, and
`IGESSolid` entity packages. No IGES session/reader/selector ownership is implied.

The current native ABI is 1.40 and the bridge implementation version is 0.48.0.

ABI 1.41 is the alpha.49 final long-tail generation boundary. It adds export-proven
Standard foundation free-function and verified void-return entry points using the same
fixed-width C ABI conventions as generated static methods. Standalone enum generation
is managed metadata and does not expose C++ enum layout. The current native ABI is 1.41
and the bridge implementation version is 0.49.0.

ABI 1.42 is the alpha.51 first Batch C common-workflow boundary. It adds native BREP
read/write, fixed 120-byte topology summaries, 72-byte detailed mesh vertices, 20-byte
face-mapped triangles, and thread-affine viewer appearance/camera/selection exports.
Topology and mesh data are copied snapshots; BREP reads return registered owning shapes;
XDE labels and viewer presentations keep their existing parent-bound contracts. The
current native ABI is 1.42 and the bridge implementation version is 0.50.0.

## Verification

Alpha.48 verification confirms the existing ABI contract for all 162 IGES public
wrappers: 156 default-constructible wrappers passed construction, clone, RTTI,
reference-count, and disposal checks. IGES session, selector, reader, and transfer
state remains native-local. The clean package consumer loaded the 47-DLL closure from
the application-local `occt` directory under SDK 10.0.400.

- Compile consumer tests against the exported C headers.
- Inspect exports and native dependencies on every supported platform.
- Verify struct sizes and offsets in both native and managed tests.
- Exercise exception, null, invalid handle, double-dispose, and wrong-type paths.
- Run memory and sanitizer diagnostics where supported.

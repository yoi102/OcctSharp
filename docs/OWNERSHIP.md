# Ownership and Lifetime

Ownership rules are safety requirements. Coverage goals never override them.

## Ownership categories

Every value crossing the native boundary must use exactly one category:

| Category | Meaning | Managed responsibility |
|---|---|---|
| Value copy | Independent data copied across the ABI | No native lifetime |
| Owning | Managed wrapper exclusively owns a native resource | Deterministic release |
| Shared | Lifetime follows OCCT reference-counted semantics | Release one retained reference |
| Borrowed | Resource is owned elsewhere | Never release; enforce valid lifetime |
| Parent-bound | Borrowed resource is valid only while a parent remains alive | Retain/validate parent relationship |
| Static/process | Process or library owns the resource | Never release; document concurrency |

Unknown ownership is a model validation error, not an implicit category.

## Rules

- **O001** — Every owning object returned across the ABI has one matching native
  release operation.
- **O002** — Borrowed handles are never released by managed code.
- **O003** — OCCT `Handle<T>` wrappers preserve intrusive reference counting and do
  not become untracked raw pointers.
- **O004** — A native reference or pointer is not exposed beyond its proven lifetime.
- **O005** — Managed disposal is idempotent and cannot double free.
- **O006** — Finalization is a fallback, not the primary release path.
- **O007** — Access after disposal fails predictably before native dereference.
- **O008** — Native exceptions cannot bypass cleanup or cross the C ABI.
- **O009** — Native allocation and deallocation use the same module and allocator family.
- **O010** — Parent-bound wrappers keep or validate the parent relationship for every call.
- **O011** — Runtime casts retain the correct shared ownership and dynamic type semantics.
- **O012** — Callback state, if later supported, has explicit pinning, cancellation,
  reentrancy, and teardown rules.

## OCCT-specific baselines

### `Handle<T>` and `Standard_Transient`

The generator must recognize handle declarations, inheritance, null handles, copies,
base/derived conversions, and the retain/release behavior implemented by the native
bridge. Multiple managed wrappers referring to one native object must be tested.

The current shape-only bridge now registers each owning shape handle in a mutex-protected
live set. Operations reject a non-null handle that is not registered, and release removes
the registration before deletion, making repeated native release calls no-ops. This is a
stale-handle guard for the owning shape category; it is not yet OCCT `Handle<T>` reference
counting and does not make concurrent release/use safe.

The experimental `SharedTransient` wrapper is the first shared category. Each native
wrapper owns one `opencascade::handle<Standard_Transient>` value, so cloning increments
OCCT's intrusive reference count and releasing a wrapper decrements it. Null handles are
valid values with reference count zero. The wrapper registry protects the ABI wrapper
pointer; it does not replace OCCT's object counter or provide weak references.

The same probe exposes OCCT RTTI type names and `IsKind` base checks. ABI 1.8 adds one
checked cast target, `OcctSharp_TransientDerived`: the native bridge validates the
dynamic kind before copying the shared handle, and returns `TypeMismatch` for null or
incompatible values. `SharedTransient.TryCastDerived` therefore creates a typed
wrapper only after validation; it never reinterprets an unverified pointer.

ABI 1.9 applies the validated shared category to configured generated types. Each
`GeomCartesianPoint` wrapper owns one native wrapper and one intrusive reference;
`Clone` copies that handle, mutations are visible through retained wrappers, and either
managed wrapper may be disposed first. Per-type registries reject stale wrapper
addresses. Borrowed handles, parent-bound handles, general casts, and concurrent
release/use remain pending.

The initial generated StepBasic milestone applies that same shared category without changing O001-O012 to ten
StepBasic entity classes. Constructors create one intrusive handle, `Clone` retains the
same entity, scalar/boolean/enum members operate through a registry-validated receiver,
and disposal releases one retained reference. Handle-valued fields and parameters remain
unemitted until cross-generated-type conversion and null semantics are generalized.

The alpha.44 package-level expansion applies the same category to selected BRepMesh,
Poly, ShapeAnalysis, ShapeFix, and ShapeUpgrade records. Binding-model schema 1.2
records `IsAbstract`, and abstract records are excluded even when a public constructor
is visible. The 61 emitted concrete types keep per-type live registries, one retained
OCCT intrusive handle per managed wrapper, and value-copy scalar/enum arguments/results.
No algorithm child, mesh buffer, topology reference, or cross-type `Handle<T>` value is
  borrowed through this profile.

The alpha.45 STEP model expansion applies the same owning-wrapper category to concrete
`StepGeom`, `StepRepr`, `StepShape`, and `StepVisual` entities. Each managed wrapper retains
one OCCT intrusive reference and can be cloned independently.

Alpha.46 generalizes relationships between selected generated shared-handle types. Managed
`null` maps to a null OCCT handle. A non-null parameter is checked for disposal before its
opaque registry token is passed, then the target-specific native registry validates it.
A returned non-null handle is copied into a newly allocated target wrapper and therefore
remains valid after the source argument or receiver is disposed. Returned null handles map
to managed `null`; no borrowed relationship wrapper is created.

### `TopoDS_*`

ABI 1.10 and `TM007` implement the base `TopoDS_Shape` value category. Each registered
native wrapper owns one independent C++ shape value; copy and reversal allocate new
wrappers, but OCCT's internal `TShape` remains shared as normal. Disposing one wrapper
cannot invalidate another copy. `IsPartner` compares `TShape`, `IsSame` adds location,
and `IsEqual` adds orientation. Null state, shape kind, and orientation are generated.

B04 adds `Compound`, `CompSolid`, `Solid`, `Shell`, `Face`, `Wire`, `Edge`, and `Vertex`
wrappers. Each checked `TopoDS::Xxx` conversion copies into a new owning shape value;
wrong non-null kinds return ABI `TypeMismatch`, and a successful typed wrapper remains
valid after source disposal. Location mutation/composition, hashing, and topology
children remain pending and must extend this value contract rather than infer ownership
from a pointer spelling.

### Iterators and child objects

Explorer results, collection views, triangulations, document labels, and other child
objects require explicit copied, retained, or parent-bound semantics. Temporary native
objects may not escape through a borrowed wrapper.

### BRep adaptor snapshots

`BRepAdaptor_Curve` and `BRepAdaptor_Surface` exist only within a native bridge call.
Their topology references and underlying curve/surface accessors never cross the ABI.
The edge and face snapshot APIs copy enum, parameter, UV-bound, and point values into
caller-owned structures. These results are value copies with no native lifetime and
remain valid after the source topology wrapper is disposed. Wrong topology kinds fail
before adaptor construction.

### Basic modeling results

`BRepAlgoAPI_Fuse` and `BRepAlgoAPI_Common` never leave the native call; each successful
shape is copied into a new registered owning wrapper that does not retain its inputs.
`BRepExtrema_DistShapeShape` likewise remains call-local, while distance, one point pair,
and solution count cross as a value copy. Support topology, parameters, progress state,
and history are deliberately not borrowed or parent-bound through this profile.

### Common feature-modeling results

Cone/torus builders, prism/revolution builders, fillet/chamfer contour state, offset
state, section state, `Bnd_Box`, and `BRepCheck_Analyzer` remain call-local under
SC-032/ADR-0052. Input shapes, vectors, axes, and selected edges are borrowed only for
the call. Every topology result is a new registered owning shape and does not retain an
input wrapper. Bounding boxes are six copied doubles with no native lifetime; validity
and subshape counts are copied scalar values. Algorithm history, progress, contours,
and per-face offset state do not cross the common-modeling ABI.

### Boolean and healing history exclusion

Cut, ShapeFix, and same-domain unification return new registered owning shapes and do
not retain their inputs. Their BOP/ShapeFix/ShapeUpgrade history and mode state remain
native-local. No history object, modified/generated topology map, status reference, or
borrowed child crosses the ABI in the B12 owning-result/no-history profile.

### Mesh-format exchange providers

OBJ, PLY, glTF/GLB, and VRML provider/configuration objects are created and destroyed
inside one native call. Writers borrow a live shape only during the call and build its
triangulation before export. OBJ, glTF/GLB, and VRML readers copy the transferred result
into a new registered owning `Shape`; it has no dependency on provider, document, or
scene lifetime. No provider configuration, progress object, label, native mesh, or
metadata graph crosses the ABI. PLY read has no ownership contract because it is not
implemented by OCCT 8.0.1 and is exposed as unsupported.

### OCAF documents and labels

`OcafDocument` owns one native wrapper containing retained `TDocStd_Application` and
`TDocStd_Document` handles. Native release aborts any open command, closes the document
from the application session, and deletes the registered wrapper. `OcafLabel` has no
native handle: it stores a stable TDF entry plus a strong reference to its parent
document. Every operation resolves that entry against the current data framework, so
explicit parent disposal deterministically invalidates all labels. Names are UTF-8
copies. Transactions are parent-owned command state; an uncommitted managed transaction
aborts on dispose. Abort rolls back attributes but OCCT may retain allocated empty label
nodes, which are not treated as independent child owners.

### XDE metadata and assemblies

`XdeDocument` uses the same owning application/document category as OCAF with BinXCAF
drivers. `XdeLabel` is parent-bound by stable entry. XCAF tools, free-shape/component
sequences, color/layer/material tables, reference tree nodes, and STEPCAF state remain
inside each native call. Shape and occurrence-location results are new independent owners.
Names, effective RGBA, layer arrays, material records, counts, and entries are copied.
An occurrence's referred part is represented by another parent-bound entry, not a native
reference handle. Effective color deliberately maps Gen/Surf/Curv because STEPCAF may
normalize an overall color between those document-owned channels.

### Visualization graph and presentations

`OcctViewer` is an owning, thread-affine wrapper for the display connection, OpenGL
driver, viewer, interactive context, view, and `WNT_Window` bound to an application-owned
HWND. The application must keep the HWND alive until the viewer is disposed and must call
viewer methods only on the creating thread. The viewer does not own or destroy the HWND.

`ViewerPresentation` is parent-bound and owns no native allocation. Its 64-bit ID is
resolved through the parent viewer for every show/hide/remove operation. Display copies
the source topology into an `AIS_Shape`, so the source `Shape` can be disposed immediately.
Selection returns a caller-owned managed snapshot derived from copied presentation IDs;
no `AIS_InteractiveObject`, selector iterator, or native pointer crosses the ABI.

### B06 strings and sequences

`TCollection_AsciiString` and `TCollection_ExtendedString` are owned opaque values.
UTF-8 input buffers are borrowed only for the duration of one call; UTF-8 output is
copied into caller-owned buffers and no native string pointer escapes. Extended-string
length and character access use UTF-16 code units, while the friendly API exposes
0-based indexing.

`NCollection_Sequence<double>` is an owned opaque container. Creation copies the input
array, clone creates an independent sequence value, and append/set/remove operate on
the native sequence while translating friendly 0-based indices to OCCT's 1-based
contract. Enumerators read values one at a time and do not retain native pointers.

`NCollection_Array1<double>` is an owned opaque container created with OCCT's native
lower bound 1. The managed wrapper exposes a 0-based view and translates each access;
clone results are independent and enumerators read one value at a time. The OCCT 8
`NCollection_Vector<double>` alias is backed by `NCollection_DynamicArray<double>` and
is an owned zero-based dynamic container with the same copy/clone/no-pointer rules.

`NCollection_DataMap<int,double>` and `NCollection_IndexedMap<int>` are owned opaque
containers. Input key/value buffers are borrowed only for construction; map operations
copy scalar keys and values, clone independently, and never return node or bucket
pointers. Indexed-map key/index calls copy values and translate native 1-based indices
to the friendly 0-based view. No native iterator escapes.

## Required lifetime tests

- Create, use, dispose, repeated dispose, and access after dispose.
- Multiple wrappers over a shared native object.
- Base-to-derived and derived-to-base conversion.
- Parent disposed before and after child.
- Managed GC and finalizer fallback.
- Exception during construction, call, and conversion.
- Collection enumeration and early exit.
- Concurrent access where the API is documented as supported.
- Stress loops with native leak and invalid-access diagnostics enabled.

Any change to O001–O012 requires an ADR and corresponding tests.

The package-level StepBasic milestone does not introduce a new ownership category. Discovery expands the
existing O004 generated intrusive shared-handle contract from ten to 129 StepBasic
types. Each wrapper registry owns exactly one retained `Handle<T>` value; `Clone()` adds
one intrusive reference, disposal releases one wrapper, and every generated type is
runtime-tested through 1-to-2-to-1 reference counts and disposed-use rejection.

The generated Geom/Geom2d expansion also reuses O004 without a new ownership category.
Eight additional public wrappers each own exactly one retained `Handle<T>` in a
type-specific live registry. `Clone()` shares and increments the intrusive count;
disposal releases only that wrapper. Scalar, enum, and copied-point parameters/results
do not retain managed storage, and no borrowed curve/surface member is emitted.

The common-modeling milestone likewise introduces no new ownership category. Its 18 audited declarations are
reported as accepted manual bindings because generalized builder/history descriptors do
not yet exist; their runtime behavior reuses O001/O004/O005/O007/O008 and the registered
owning `Shape` category.

The current high-value B workstream adds 43 SC-033 declarations without adding a raw
native ownership category. Curve/surface adaptors, GeomAPI projectors, curve builders,
loft/pipe/sewing/thick-solid builders, topology maps, and Boolean algorithm history are
call-local. Curve/surface evaluations and history summaries are immutable copied values.
Adjacency snapshots own independent `Shape` copies plus managed offset/index arrays;
disposing the source cannot invalidate them, while disposing the map disposes all owned
shape copies. This does not close the entire B batch. Multi-shape input arrays borrow every `SafeHandle` only after paired
`DangerousAddRef` acquisition and release all acquired references in reverse order.

`XdeDocument.ImportStep` mutates only the destination owned document during an open
transaction. Source STEPCAF documents, source labels, XCAF tools, clone maps, and
material maps die inside the native call. Returned `XdeLabel` values are destination-
parent-bound stable entries; commit preserves them, abort may remove their attributes,
and destination disposal invalidates them. No source-document relationship survives.

### Generated placement-allocator shared objects

Types explicitly listed in configuration schema 1.8 under
`placementAllocatorNativeTypes` are still O004 intrusive shared objects, but their native
wrapper owns an additional allocator retention. The allocator field is declared before
the OCCT object handle so C++ destroys the object first and releases the retained
allocator last. Constructor emission requires a non-null generated
`NCollection_IncAllocator` wrapper, uses `new (allocator) T(...)`, and clone emission
copies both retained handles. Ordinary `new`, global `::new`, and a wrapper that stores
only the object handle are forbidden for these types because their allocation and delete
contracts do not match or can free incremental storage during object destruction.

### Batch C common workflow snapshots and presentation state

`Shape.GetTopologySummary` returns only copied counts, Booleans, and tolerance ranges.
All `TopExp` maps, typed subshapes, `BRepCheck_Analyzer`, and tolerance references die
inside the native call. `Shape.CreateDetailedMesh` returns caller-owned arrays of copied
node positions, transformed normals, optional UV values, triangle indices, source-face
indices, and orientation flags. No `Poly_Triangulation`, face, location, or source-shape
relationship survives the call.

`ShapeExchange.ReadBrep` returns a registered owning shape; `WriteBrep` borrows its shape
only for the call. `XdeDocument.AddPart` creates one parent-bound label and composes the
existing copied metadata operations inside the caller's open transaction. Viewer color,
transparency, display mode, projection, zoom, pan, and selection operations mutate only
the parent viewer on its creation thread; presentation IDs never become standalone
owners.

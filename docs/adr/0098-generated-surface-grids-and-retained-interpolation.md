# ADR-0098: Generated grids and retained interpolation data

- Status: Accepted for Batch AG; delivery evidence is recorded separately.
- Date: 2026-10-02.
- Baseline: Preview.30 / ABI 1.74 / bridge 0.82.0.
- Related: [ADR-0097](0097-generated-copied-arrays-and-sequences.md).

## Decision

Admit 63 exact Blocked declarations and 18 compatible Manual prerequisites from
the frozen AG catalogue. Keep both nullable Weights pointer overloads excluded.
Emit separate grid support and operation files while sharing the existing operation
emitter and registry. Preserve AF profile order, exports, public APIs and generated
files. The general template type map remains closed.

CopiedGrid<T> owns an immutable managed row-major copy, with explicit row/column
lower bounds and positive dimensions. Native factories use owning Array2 allocation.
Check dimensions, product, endpoints and every signed intermediate in the pinned
BeginPosition/LastPosition calculation before allocation. Transfer and snapshot
elements through Value(row,column), never native geometric layout or external storage.
Only point and real grids, and the closed vector/bool Array1 additions, cross this ABI.
Boolean elements cross as normalized uint8_t; public snapshots hold bool values.

Interpolate input arrays become independent NCollection_HArray1 owning copies
managed by OCCT intrusive handles. Non-transient algorithms retain those handles
through their own members. Registry leases own an InterpolationOwner wrapper that
also records validated point count and tolerance for later Load checks. No transient
object is independently deleted by shared_ptr while also held by an intrusive handle.
Surface fitting inputs are call-local owning copies. Returned geometry has independent
shared handle ownership; returned arrays are immutable registered snapshots.

SC-066 records the pinned constant-height fitting defect: the kernel reports IsDone
but yields NaN. SurfaceFitOwner supplies an exact planar B-spline with physical X/Y
domains and requested degree, while preserving the native path for nonconstant grids.
Its optional fallback handle is cleared on reinitialization; returned surfaces keep
independent ownership. This adaptation is emitted from the rule template.

Bounds, dimensions, spline definitions, periodic counts, tangent flags, positive
weights and output capacity are checked independently of Release kernel range checks.
All native exceptions remain inside the C ABI. Copy-out uses publication rollback
and success-only managed ref assignment. Native receiver mutations are not rolled
back; concurrent mutations require caller synchronization.

## Alternatives and consequences

External buffers and general template mapping cannot prove retained lifetimes or
kernel sizes. Rewriting AF generated files would make preservation harder to audit.
Nullable array pointers need an optional-copy contract; callers use WeightsArray.
The module graph, facade and single native bridge remain unchanged.

The public-only direct consumer exposed a pre-existing facade-only placement of
FreeformAuthoring/FreeformTypes and ShapeExchange/StepReadResult/StlWriteOptions.
Place the former group in Modeling and the latter in DataExchange, within the
acyclic graph accepted by [ADR-0074](0074-physical-managed-modules-and-shared-native-package.md).
Preserve all namespaces, signatures, native ownership and entry points. Generate
facade type forwarders for the 17 moved public types and check aggregate API
compatibility. This is module delivery closure, not additional OCCT migration.

## Validation

Require exact signature selection, old-file/API/export preservation, asymmetric
non-square grids, shifted bounds, rational weights, partial edits, periodic seams,
2D/3D tangent and parameter interpolation, malformed raw dimensions/capacity/bools,
post-input and post-algorithm result lifetime, grid-to-face/STEP and curve-to-edge
workflows in both consumers, full local delivery gates and fresh inventory.

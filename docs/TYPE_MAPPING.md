# Type Mapping

AW TM030 maps exact spline kernels/filling receivers to bounded copied rows and
private owned surfaces. TM031 maps new initialized vertex/edge assembly with a
validation gate. Raw callbacks/scratch references and input representation flags
stay deferred. See [ADR-0115](adr/0115-private-filling-kernels-and-controlled-topology-assembly.md).

AV TM029 maps exact call-private Foundation value operations to bounded copied
text/scalars. It preserves UTF16 code-unit indices with checked edit boundaries,
pure OSD syntax, copied color vectors and proven unit dimensions. See
[ADR-0114](adr/0114-generated-call-private-foundation-values.md).

AP's TM021 is the exact private modeling profile in
[ADR-0108](adr/0108-generated-private-modeling-builders.md). Its reusable emitter
does not admit unlisted signatures or general borrowed references.

AN's TM020 maps only exact private writer diagnostics and prerequisites to
DataExchange; see [ADR-0106](adr/0106-private-writer-diagnostics.md).
It does not expose arbitrary mutable finder processes or model pointers.

AM's exact signature rule TM019 targets Geometry and private spline workspaces;
see [ADR-0105](adr/0105-private-spline-workspaces.md). It does not globally map
borrowed pointers or scalar references to public buffers.

## Batch AJ closed repair-workspace profile

TM016 maps only the exact AJ catalogue's private containers, coupled shape/list
maps, wire data and configured repair controls. It does not extend InitialTypeMap
or admit arbitrary mutable maps, sequences, references or external contexts.
Private HSequence signatures are handled before the generic shared-handle rule.
One generated Modeling C ABI header, five native implementation shards and three
managed files use typed leased registry owners and SafeHandles. New TopoDS Oriented
support copies the value while retaining TShape/location. See
[ADR-0101](adr/0101-generated-coupled-repair-workspaces.md).

## Batch AI exact topology and exchange profile

The closed AI catalogue adds TM015Shape for copied list/sequence/map shape values,
TM015Entity for detached retained entity sequences/private iterators/graphs,
TM015Text for immediate UTF8 strings, and TM015Control for private default progress
ranges. These projections apply only to exact admitted/prerequisite IDs and
signatures, without extending InitialTypeMap. Modeling owns shape sets/algorithm
results; DataExchange owns retained entity sets/cursors/private sessions/graphs
and references Modeling. The dependency graph stays acyclic. History/model identity
checks do not use geometric similarity. See [ADR-0100](adr/0100-generated-topology-and-exchange-collections.md).

## Batch AH exact bulk and typed curve profile

TM014 is limited to the 42 exact AH declarations. It maps sized numeric/gp copies,
audited scalar refs and closed typed spline/HArray containers. It does not add a
general reference, pointer, matrix, evaluator or arbitrary-template projection.
Scalar refs use value-copy initialization and success-only publication. Curve elements
use independent intrusive deep copies held inside typed leased registry owners.
[ADR-0099](adr/0099-generated-bulk-analysis-and-typed-curve-collections.md) and the
[AH contract](BATCH_AH_BULK_ANALYSIS.md) specify bounds and lifetime restrictions.

## Batch AG grid and retained profile

TM013 applies only to exact AG signatures: Array2<gp_Pnt/double>, tangent
Array1<gp_Vec/gp_Vec2d>, Boolean Array1 and retained HArray1 inputs. Existing AF
Array1 profiles are reused. HArray inputs project immutable managed arrays, then
deep-copy into intrusive native owners retained by the algorithm. Boolean elements
use normalized uint8_t in the raw ABI and CopiedArray<bool> publicly. Grid dimensions,
storage offsets and capacity are checked. Nullable pointers and arbitrary templates
remain blocked. General InitialTypeMap behavior is unchanged. TM012 owns the three
non-transient algorithm families. See [ADR-0098](adr/0098-generated-surface-grids-and-retained-interpolation.md).

## Batch AF copied collection profile

[ADR-0097](adr/0097-generated-copied-arrays-and-sequences.md) adds exact-operation
TM011 for Array1<double/int/gp_Pnt/gp_Pnt2d> and Sequence<double>. The managed
containers are CopiedArray<T>/CopiedSequence<T>; gp_Pnt uses Coordinates3d. Signed
lower bounds and counts are explicit; arrays are nonempty and reserve INT_MIN/INT_MAX
endpoint indices for safe pinned-kernel arithmetic. Sequences may be empty.
Domain guards enforce one-based arrays where the selected algorithm needs
them. TM012 covers the two new stable-address point-fitting owners. Both rules are
scoped to the signature catalogue; InitialTypeMap still rejects arbitrary templates,
arrays, mutable references and pointers. All support is generated from rules and
templates; no handwritten native binding exception is added.


## Owned data workflow extension

[Batch AD](BATCH_AD_OWNED_DATA_WORKFLOWS.md) extends the exact TM010 catalogue.
`HatchGen_PointOnElement`, `HatchGen_PointOnHatching` and
`SelectBasics_PickResult` join the explicitly copyable result set. Scalar, enum
and gp ref outputs retain the existing success-only publication contract: a
returned false value is a successful native call, not an exception. Default-null
curve results reuse TM006 nullable retained handles. No general sequence,
template vector, external buffer or pointer mapping is added.

## Batch AC audited utility extension

[Batch AC](BATCH_AC_OWNED_UTILITY_EXPANSION.md) extends the exact TM010 catalogue while
preserving the Preview.27 owner epoch. New const methods use const receiver leases.
For audited STEP SELECT owners, typed parameterless getters supply supporting typed
SetValue overloads; native implicit handle upcasts and existing checked SetValue retain
the normal shared category. Neither these conveniences nor inherited projections are
additional migrated declaration IDs. The general type map and buffer exclusions remain
unchanged.

## Batch AB audited owning non-transient projections (ADR-0096)

An explicit stable-ID and exact-signature catalogue selects supported constructors
and operations on independently owned non-transient classes. Context-specific TM010
maps each selected owner to its typed SafeHandle and opaque native token, with exact
type-tag validation and a call-duration shared_ptr lease. It does not add a fallback
IntPtr mapping for unknown classes or change the general mutable-reference rejection.
Native instances are constructed directly in stable heap storage and cannot be copied
or moved after publication. Eligible owned inputs must have an audited call-bound
relationship; borrowed storage and retained addresses require separate lifetime rules.

Scalar, enum and geometric copies retain existing ABI records and conversions.
Audited mutable copied-reference parameters use separate input values and output
pointers, with managed ref assignment only after success. All output conversions
complete before publication, without promising rollback of owned object mutation.
Do not infer buffers from array-start references, mutable-reference return ownership,
template layout or copy/move constructors. Preserve Manual/Skipped declarations,
existing public identities and native exports/bodies in separate generated output.
The implemented category and validation are recorded in [Batch AB](BATCH_AB_OWNED_NONTRANSIENT_VALUES.md);
see [OWNERSHIP](OWNERSHIP.md) and
[ADR-0096](adr/0096-generated-owned-nontransient-values.md).

TM010 accepts const references to audited owners as leased inputs, and copied value
or const-reference results only for the 24 explicitly copyable owner records. An owned
result gets independent heap storage. The supporting method catalogue preserves
Manual dispositions and projects inherited queries without inventing migrated IDs.

## Batch AA audited reference parameters (ADR-0095)

The explicit reference-operation catalogue matches both stable ID and native signature.
Only listed mutable lvalue-reference parameters may normalize to an existing ValueCopy
projection. InitialTypeMap still rejects general mutable references. Each accepted
parameter crosses the C ABI as an input value and a separate updated-value pointer;
the public managed signature uses ref. Mutable reference returns, shared-handle refs,
pointer/array parameters and dimensioned array-start references remain excluded.
Only const shared/copy receivers and audited static methods participate. New exports
and operation classes use separate files and preserve existing signatures and bodies.

## Batch Z copied operations and transformation coefficients (ADR-0094)

TM009 additionally maps Transform2d, Transform3d, AffineTransform2d and
AffineTransform3d. Their M11...M23 or M11...M34 fields are row-major coefficients,
including the final translation column. These are not C++ memory layouts or form flags.
Similarity inputs rebuild through SetValues; every accessor must be finite and match
its input within 1e-12 * max(1, abs(input)). This rejects shear and invalid/singular
similarity matrices. Affine inputs retain general coefficients, including singularity;
when the same similarity check succeeds, they use a canonical similarity form.
Native Form queries describe this reconstructed form, not the original object's cache.
Native domain and numerical-resolution rules still apply to inversion and operations.
Default zero records are not identity transforms; use Transform2dOperations.Create()
or Transform3dOperations.Create() for identity values.

EL011 selects public geometric constructors and instance calls with copied inputs and
outputs. Geometry exposes them under OcctSharp.ValueOperations. Const receivers are
passed by value; non-const receivers use ref and update only after successful status.
Mutable-reference parameters/returns, operators, pointers, arrays and direct form-cache
editing remain excluded. Existing manual stable IDs remain manual. The older Y scope
below describes its thirty-record delivery, not the current transform restriction.


## Batch Y copied geometric values (ADR-0093)

TM009 maps thirty gp types by value or const lvalue reference to explicit records:
coordinates, points, vectors, directions, axes, matrices, quaternions, lines, conics,
planes and elementary analytic surfaces. Managed records live in OcctSharp.Values
in Foundation. Existing owning facade wrappers remain available with their original
identities. Mutable/output references, pointers, arrays, gp_Trsf and gp_GTrsf remain
outside this rule. Mapping a value does not emit its native constructors automatically.

Input fields must be finite. Native constructors normalize directions and validate
domains. Full axes contain origin, normal, X and Y; inconsistent axes reject. Axis3
preserves both handedness choices and Axis22d preserves its orientation. Matrix fields
are row-major; gp_Mat2d is reconstructed with two explicit column vectors. Native
results use accessors, including sentinel values where the native API defines them.
Thirty C/C# size and offset contracts and strict C11 headers are verified by the Y tests.

## Batch X numeric and copied-reference rules (ADR-0092)

TM008 extends canonical numeric values and const-reference inputs with fixed ABI copies:
float -> float; signed/unsigned char -> int8_t/uint8_t; short -> int16_t/uint16_t;
unsigned int -> uint32_t; Windows long -> int32_t/uint32_t; long long and size_t aliases
-> int64_t/uint64_t as dictated by their canonical types. Managed types preserve width
and signedness. Generated compilation asserts byte/integer widths, Windows 64-bit size_t
and IEEE float/double; argument casts preserve the exact native overload. Plain char,
long double, pointers, mutable/out references and rvalue references remain unmapped.

TM001-TM005/TM008 may copy const-reference return values; TM006 may retain a selected
const-reference intrusive handle into a new wrapper. Scalars/enums are copied inside
the call, gp_Pnt through coordinates, and handles by reference-counted value construction.
No reference address escapes. TopoDS reference returns are not added by this rule.
New const member calls use a const native receiver to avoid dispatching a mutable overload.
New static bindings use status/output C ABI calls, zero outputs on failure and contain
all C++ exceptions; managed raw helpers translate status through the common error path.
Preview.22 direct-return static ABIs and overload identities are preserved.

An individual const scalar reference can describe a sequence start in OCCT. SC-061
keeps BSplCLib::FlatBezierKnots outside generic scalar emission; its header explicitly
describes the whole flat-knot array. A dedicated manual buffer binding now copies all
`2 * (degree + 1)` values after degree and capacity checks. Type-map acceptance alone
is not declaration acceptance. Other dimensioned array-start references still need
their own explicit contracts. Current validation belongs in STATUS.
Historical initial-rule descriptions below predate this additive expansion.

## Policy

Native-to-ABI and ABI-to-managed mappings are centralized, versioned generator rules.
A native spelling alone is not enough to choose a mapping; qualifiers, pass mode,
ownership, lifetime, target platform, and use as parameter or return value all matter.

Each mapping record must define:

- Native canonical type and accepted aliases.
- Applicable const, pointer, reference, value, array, and template forms.
- ABI representation.
- Managed raw representation.
- Optional friendly representation.
- Ownership rule ID.
- Nullability and range behavior.
- Marshal/copy behavior and cleanup.
- Supported directions: input, output, return, field, callback.
- Required tests and unsupported combinations.

## Initial mapping candidates

The first six rules are implemented in the centralized generator map. Value-copy mappings are
accepted only for direct values and const-reference inputs; pointer and reference
returns remain unmapped until ownership and lifetime rules exist.

| Native concept | ABI candidate | Managed candidate | Status |
|---|---|---|---|
| `void` return | `void` | `void` | Implemented (`TM000`, return-only no-value projection) |
| `Standard_Integer` | `int32_t` after compile-time width verification | `int` | Implemented (`TM001`) |
| `Standard_Real` | `double` after compile-time width verification | `double` | Implemented (`TM002`) |
| `Standard_Boolean` | `int32_t`, normalized to 0 or 1 | raw `int`, friendly `bool` | Implemented (`TM003`) |
| Native enum | `int32_t` after underlying-width verification | raw `int`, typed friendly enum | Implemented (`TM004`) |
| `Standard_CString` input | UTF-8 pointer plus defined null rule | `string`/span-based marshal path | Proposed |
| OCCT string class | UTF-8 copy or opaque handle by use case | `string` or wrapper | Pending |
| `Handle<T>` value | `OcctSharp_TransientHandle*` typed opaque shared wrapper | `SharedTransientHandle` plus generated public type | Implemented (`TM006`) for configured shared scopes |
| `TopoDS_Shape` | Registered opaque wrapper owning one C++ value | `ShapeHandle` / `Shape` | Implemented (`TM007`) |
| Common BRep builder/analyzer results | Registered owning `Shape`, fixed copied bounds, or copied scalar | `Shape`, `BoundingBox3d`, or `bool`/`int` | Manual (`SC-032`) |
| Typed `TopoDS_*` | Checked opaque topology value projection | Specialized topology wrappers | Implemented (`ADR-0017`) |
| `gp_Pnt` | Explicit `OcctSharp_Point3d` X/Y/Z copy, never native layout | internal `Point3dRaw`; friendly `Point3d` pending | Implemented (`TM005`) for coordinate, default, and const-copy constructors |
| `gp_Trsf` | Registry-validated opaque native value | `GpTrsf` owning wrapper | Manual B05 (`SC-005`) |
| `TopLoc_Location` | Registry-validated opaque native value | `TopLocLocation` owning wrapper | Manual B05 (`SC-006`) |
| `gp_Vec` / `gp_Dir` | Registry-validated opaque native value with finite/non-zero checks | `GpVec` / `GpDir` owning wrappers | Manual B05 (`SC-007`) |
| `gp_Ax1` | Registry-validated opaque origin-plus-direction value | `GpAx1` owning wrapper | Manual B05 (`SC-007`) |
| `gp_Mat` | Registry-validated opaque 3x3 value with 1-based access | `GpMat` owning wrapper | Manual B05 (`SC-007`) |
| `TCollection_AsciiString` | UTF-8 byte copy with explicit length | `OcctAsciiString` | Manual B06 (`SC-008`) |
| `TCollection_ExtendedString` | UTF-16 code-unit value with UTF-8 copy conversion | `OcctExtendedString` | Manual B06 (`SC-008`) |
| `NCollection_Sequence<double>` | Registry-validated opaque native sequence | `OcctRealSequence` (`IReadOnlyList<double>`) | Manual B06 (`SC-008`) |
| `NCollection_Array1<double>` | Registry-validated opaque native array with explicit lower bound | `OcctRealArray` (`IReadOnlyList<double>`) | Manual B06 (`SC-009`) |
| `NCollection_Vector<double>` / `NCollection_DynamicArray<double>` | Registry-validated opaque native dynamic array | `OcctRealVector` (`IReadOnlyList<double>`) | Manual B06 (`SC-009`) |
| `NCollection_DataMap<int,double>` | Registry-validated opaque key/value map | `OcctIntRealMap` | Manual B06 (`SC-010`) |
| `NCollection_IndexedMap<int>` | Registry-validated opaque ordered key map | `OcctIntIndexedMap` (`IReadOnlyList<int>`) | Manual B06 (`SC-010`) |
| `NCollection_*` | Specialized iteration or bulk-copy ABI | .NET collection/view by semantics | Per-template |

Compile fixtures verify the selected OCCT baseline uses 32-bit `Standard_Integer`,
binary64 `Standard_Real`, native `bool` for `Standard_Boolean`, and a 32-bit underlying
representation for `TopAbs_ShapeEnum`. `TM003` deliberately does not expose native
`bool` size across the ABI. `TM005` uses accessor/constructor copying and makes no
claim that `gp_Pnt` is ABI-blittable. The initial generated constructor proves the
24-byte X/Y/Z ABI value at native compile time and managed runtime. The default and
const-copy constructors use the same accessor/constructor copy boundary; broader
`gp_Pnt` parameter and return emission remains pending generalized eligibility rules.

The configured `TopAbs` static scope now exercises `TM004` end to end: enum parameters
are converted from `int32_t` to the native enum before the C++ call, and enum returns are
converted back to `int32_t` for the managed raw binding.

The generated StepBasic enum milestone completes the managed side of `TM004`: the AST model records enum definitions,
explicit signed/unsigned values, and underlying types; the emitter writes typed public
enums and rejects values outside the verified Int32 range. Canonical qualified names and
unique unqualified spellings map to the same managed type, which covers nested
`Standard::AllocatorType` and the StepBasic enum families without alias drift.

`TM006` applies only when semantic discovery identifies an OCCT
`opencascade::handle<T>` value. It preserves intrusive sharing through a native wrapper
and `SafeHandle`; no OCCT object pointer crosses the ABI. Pointer/reference layers,
borrowed returns, parent-bound projections, and unconfigured target classes remain
rejected. `Geom_CartesianPoint` is the first configured target.

`TM007` applies to `TopoDS_Shape` value and const-reference copy semantics. Each
registered wrapper owns an independent C++ `TopoDS_Shape` value while normal OCCT
copies retain the same internal `TShape`. The C ABI exposes only the wrapper pointer;
shape kind and orientation cross as validated `int32_t` enum values. Generated clone,
reversal, `IsPartner`, `IsSame`, and `IsEqual` preserve OCCT semantics. Typed topology,
locations as parameters/results, hashing, and explorer children remain gated by later
rules. `B04` adds checked `TopoDS::Xxx` conversions for the eight configured subtype
values. A successful conversion copies the value into a new owning shape wrapper;
`Standard_TypeMismatch` becomes ABI `TypeMismatch` and never crosses the boundary.

## Rejection rules

The generator must not silently map an unknown type to `IntPtr`, silently discard
const/reference distinctions, flatten all templates into arrays, or convert borrowed
storage into an owning managed object.

## Conflict handling

If two declarations produce incompatible mappings for the same canonical native
type, generation fails with both source locations and rule IDs. A context-specific
mapping requires an explicit rule and documentation rather than accidental emitter
behavior.

The package-level StepBasic milestone keeps `TM004` enum and `TM006` intrusive shared-handle semantics unchanged while
generalizing selection. Schema 1.5 header patterns discover the complete StepBasic
header family, and the package scope expands only records proven to derive from
`Standard_Transient` with a supported public default construction path. Unknown
parameters, returns, borrowed references, and non-default-constructible entities remain
classified rather than receiving an invented mapping.

The Geom/Geom2d package expansion uses the same `TM004`, `TM005`, and `TM006` rules.
Package selection emits only `Standard_Transient` descendants with a supported public
constructor; instance methods require value/void returns and value-copy parameters.
This produces 67 additional generated stable IDs without adding a pointer, reference,
borrowed-view, or layout projection.

The common-modeling milestone adds no general TypeMap. Schema 1.6 instead records 18 audited algorithm declarations
as `Manual/MN001`. `TopoDS_Shape` results reuse TM007-compatible registered owners,
`gp_Vec`/`gp_Ax1` inputs reuse the existing opaque values for call-bound borrowing, and
the six-double bounding structure is a dedicated value-copy contract. Builder/history,
  edge-map, progress, and analyzer layouts remain unmapped.

The alpha.45 STEP model expansion reuses TM006 for concrete `StepGeom_*`, `StepRepr_*`,
`StepShape_*`, and `StepVisual_*` records. Alpha.46 extends TM006 to method parameters and
returns when the target is another selected generated `Standard_Transient` wrapper.
Parameters are nullable, target-registry-validated opaque tokens; results are nullable
independent retained wrappers. Package expansion still requires an independently supported
value-copy constructor, so a type cannot enter the selected scope solely because its
constructor depends on another handle.

## Batch AE exchange scalars

Batch AE adds no general type-map rule. Its two DataExchange non-transient
owners select existing integer, Boolean and double value mappings only. STEP
indexed setters and IGES string handles remain excluded; their representation
and lifetime need separate contracts.

## Change process

Batch AL uses signature-specific `TM018` in dependency-closure reporting for its
closed numeric adapter. It belongs to Foundation and does not change InitialTypeMap.
Only exact admitted IDs, canonical signatures and public accessible declarations
can select the numeric emitter. Private matrix/solver pointers become existing
MathMatrix owners, immutable CopiedGrid<double> results or SplineMatrixSystem
SafeHandles; arbitrary references and external buffers remain unmapped. See
[the AL contract](BATCH_AL_NUMERIC_MATRICES.md) and
[ADR-0104](adr/0104-copied-numeric-matrices-and-private-solvers.md).

1. Identify the canonical native type and all affected use sites.
2. Determine semantics and ownership from OCCT source/API evidence.
3. Add or change one centralized mapping rule.
4. Add focused model, emission, compile, runtime, and lifetime tests as applicable.
5. Regenerate and review affected symbol counts.
6. Update this document when the semantic contract changes.

## TM022: exact copied curved hatching workflow

[ADR-0109](adr/0109-generated-copied-curved-hatching.md) projects only the frozen Geom2dHatch/event prerequisite catalogue to Geometry private workflow snapshots. No general borrowed intersector/adaptor or mutable container mapping is admitted. Existing emitted/manual prerequisites remain separately reused.

TM023 / TM024 are exact copied Poly data / private controlled meshing catalogues
under ADR-0110. They do not globally map borrowed arrays or mutable references.
Missing channels stay absent; native neighbor zero becomes public -1. Native flags
and effective parameter values remain copied diagnostics. MeshData does not gain
a Modeling dependency; Mesh uses its already accepted Modeling reference.

TM025 / TM026 are exact document/XDE projection catalogues under ADR-0111.
Private keyed maps and label sequences become bounded immutable copied values;
document owner domains and context identity remain explicit. Native containers,
attribute handles, callbacks, arbitrary filters and raw document pointers receive
no new global mapping. Existing feature-specific parametric state remains reused.

TM027 / ADR-0112 admits only exact exchange-session operations. Private reader,
header/selector/model references become bounded copied metadata and session IDs;
no general mutable entity/model/container map is introduced. STEP schema and
IGES units/scale remain read-only; external actors, streams and callbacks deferred.

TM028 / ADR-0113 admits exact parent-bound point/box volume operations. Initialize
rectangular frusta before setters/build; copy eight proved vertices and bounded
planes, optional hit fields and scalar colors. Scene picking uses AIS. Pure copied
values and generated core belong to Visualization; friendly Viewer/presentation
adapters remain in Facade. No general camera/window/intersector mapping.

TM032 / TM033 / ADR-0116 admits only exact reviewed initialized/copy-only operations.
No general mutable container, callback or borrowed tool/session mapping is added.

TM034 / ADR-0117 admits only exact reviewed initialized/copy-only operations.
No general mutable container, callback or borrowed tool/session mapping is added.

TM035 / [ADR-0118](adr/0118-audited-foundation-function-diagnostics.md) extends
closed workflows to explicitly audited public header functions only. Exact
kind/header/package/toolkit provenance supplements signature admission. Copied
numeric optional results, strict bounded message formats and initialized private
buffers do not broaden general pointer/reference/free-function mappings.

TM036 / [ADR-0119](adr/0119-initialized-assembly-and-copied-validation.md) extends
exact call-private assembly with copied geometry graphs and face-member diagnostics.
No general builder/borrowed-topology or automatic history mapping is introduced.

TM037 / [ADR-0120](adr/0120-bounded-native-curve-computations.md) admits only exact copied bounded curve computations and reused projection operations. It adds no general adaptor/callback/borrowed Extrema mapping.

TM038 / [ADR-0121](adr/0121-identity-bound-document-association-diagnostics.md) admits exact command-scoped association edits and copied diagnostics. Uninitialized naming getters, undo-unsafe imported flag mutation and general borrowed attributes/maps remain deferred.

## TM039: Private identity-bound selected-root transfer

Exact admitted signatures use the closed-workflow profile. Copied input/output and
private typed registry owners follow [ADR-0122](adr/0122-private-identity-bound-selected-root-transfer.md). Raw borrowed
entities, topology history and mutable buffers are not promoted.

## TM040: Exact copied mesh boundary diagnostics

Exact admitted signatures use the closed-workflow profile. Copied input/output and
private typed registry owners follow [ADR-0123](adr/0123-exact-copied-mesh-boundary-diagnostics.md). Raw borrowed
entities, topology history and mutable buffers are not promoted.

## TM041: finite copied numeric aggregates

Eleven exact MathLin/MathUtils signatures follow [ADR-0124](adr/0124-finite-copied-numeric-results-and-sdk-supplements.md). Optional native matrices/vectors/scalars are private and copied only after full finite validation; arbitrary result structs, pointer lifetimes and callbacks are not promoted.

## TM042: copied modeling result/source profile

[BG](BATCH_BG_MODELING_RESULTS.md) admits 19 exact operations and eight private prerequisites. It maps only the reviewed closed workflow; general reference/pointer mappings stay unchanged. Finite initialized configuration, bounded whole-graph inputs, atomic copied diagnostic buffers and valid success-only topology are mandatory.

## TM043 / TM044: finite and coupled intersection profiles

[BH](BATCH_BH_INTERSECTION_RESULTS.md) maps 13 exact finite-hit declarations in Modeling and three pcurve construction/trim declarations in Geometry. Existing GeomInt operations are reused in independent coupled result workflows. Two declared tangent-setting methods lack fixed-source definitions/linked implementations and remain deferred. General reference/pointer mapping stays closed.
TM045: exact direct collection signatures map only to the private typed projection profile. GUID/type/bounds/presence and reference context remain explicit; generic raw array/list receivers are not unlocked. See [BI](BATCH_BI_DIRECT_COLLECTIONS.md).
TM046 maps exact STEPCAF root/external-result and STEPConstruct graph signatures only into a private bounded local workspace. No reader/session/map/reference escapes. See [BJ](BATCH_BJ_LOCAL_STEP_ASSEMBLY.md).
TM047 admits exact private converter/Common material/mesh diagnostics operations into Xde, preserving the existing DataExchange dependency direction. Existing MeshUpAxis and setters are reused. See [BK](BATCH_BK_CONFIGURED_MESH.md).
TM048 admits only the exact BL constructor and native representation/range catalogue. Global type mapping is unchanged; no curve, surface, builder or borrowed vertex reference escapes. See [BL](BATCH_BL_PARAMETERIZED_CONSTRUCTION.md).
TM049 admits only the exact BM finite solver catalogue. No borrowed solver/adaptor/point reference or arbitrary reconfiguration escapes; global mapping is unchanged. See [BM](BATCH_BM_FINITE_EXTREMA.md).
TM050 admits exact explicit-length string operations and bounded Unicode transformations. Global pointer/view mapping and AV/SC-008 rules are unchanged. See [BN](BATCH_BN_BOUNDED_TEXT_TRANSFORMS.md).
TM051 admits exact initialized private face-mesh overlap/value operations and necessary bounded dependencies. No global map/BVH/pointer mapping changes. See [BO](BATCH_BO_MESH_PROXIMITY.md).
TM052 admits exact private initialized selected-sequence STEPCAF operations and required bounded dependencies. No global writer/sequence/map mapping changes. See [BP](BATCH_BP_LOCAL_STEP_EXPORT.md).
TM053 admits the exact BSplCLib::FlatBezierKnots static sequence into the existing bounded copied-buffer API. It preserves degree/capacity guards and prohibits generic scalar-reference emission; no new ownership category or global pointer mapping is introduced. See [BR](BATCH_BR_GENERATOR_COMPATIBILITY.md).

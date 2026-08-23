# Type Mapping

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
| `Standard_Integer` | `int32_t` after compile-time width verification | `int` | Implemented (`TM001`) |
| `Standard_Real` | `double` after compile-time width verification | `double` | Implemented (`TM002`) |
| `Standard_Boolean` | `int32_t`, normalized to 0 or 1 | raw `int`, friendly `bool` | Implemented (`TM003`) |
| Native enum | `int32_t` after underlying-width verification | raw `int`, typed friendly enum | Implemented (`TM004`) |
| `Standard_CString` input | UTF-8 pointer plus defined null rule | `string`/span-based marshal path | Proposed |
| OCCT string class | UTF-8 copy or opaque handle by use case | `string` or wrapper | Pending |
| `Handle<T>` value | `OcctSharp_TransientHandle*` typed opaque shared wrapper | `SharedTransientHandle` plus generated public type | Implemented (`TM006`) for configured shared scopes |
| `TopoDS_Shape` | Registered opaque wrapper owning one C++ value | `ShapeHandle` / `Shape` | Implemented (`TM007`) |
| Typed `TopoDS_*` | Checked opaque topology value projection | Specialized topology wrappers | Implemented (`B04` / `ADR-0017`) |
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

B19.1 completes the managed side of `TM004`: the AST model records enum definitions,
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

## Change process

1. Identify the canonical native type and all affected use sites.
2. Determine semantics and ownership from OCCT source/API evidence.
3. Add or change one centralized mapping rule.
4. Add focused model, emission, compile, runtime, and lifetime tests as applicable.
5. Regenerate and review affected symbol counts.
6. Update this document when the semantic contract changes.

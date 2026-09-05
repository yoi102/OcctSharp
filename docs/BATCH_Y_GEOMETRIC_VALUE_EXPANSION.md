# Batch Y: Broad generated geometric value expansion

- State: implementation and complete local product gates pass; final documentation-package
  evidence is recorded in STATUS. Implementation checkpoint: `dda4c3b`.
- Entry: X `2cbe55e`, Preview.23; 19,682 generated + 1,217 manual IDs.
- Entry inventory: 116,272 IDs; Blocked 46,028; Skipped 49,345.
- Entry inventory SHA256: `B714207D4224DFAAD3A34CEBB9CFFE7353211779276AC084CA330A7F06A480DB`.
- Target: Preview.24 / ABI 1.68 / bridge 0.76.0.

The accepted scope is the reusable geometric-copy table and its cross-module generated
inputs/returns, compatible overload epochs, semantic and lifetime tests, exact accounting
and complete local product delivery. See [ADR-0093](adr/0093-generated-geometric-value-projections.md)
for the field-copy, domain and handedness contracts. There is no fixed declaration quota.
Arrays/output references and opaque transformation internals remain explicit later work.
Only actually emitted, compiled and validated bindings count as this batch's migration.

The previous X checkpoint is locally committed. Subsequent cohesive checkpoints are
authorized, but do not end this batch before its required delivery checks pass. Current
execution state and actual validation are maintained in [STATUS](STATUS.md).

## Implemented breadth

The regenerated manifest contains 20,650 stable IDs, exactly 968 added and none removed.
These are declaration bindings, not 968 independent CAD workflows.

| Physical module | Added bindings |
|---|---:|
| Geometry | 646 |
| DataExchange | 127 |
| Modeling | 101 |
| Visualization | 73 |
| Mesh | 7 |
| Xde | 7 |
| Documents | 6 |
| MeshData | 1 |
| Total | 968 |

Thirty Foundation DTOs support the new signatures without being counted as extra native
declaration IDs. The managed API adds 1,085 public signatures and removes none. All
32,076 X native generated signatures and bodies remain identical. The facade has 3,491
current type forwarders. Existing assembly identities and dependency graph are preserved.
Release/Debug export sets match at 33,789 names: 968 new generated C entry points and
one exact SDK inline C++ symbol documented in SC-062, with zero removals. The incidental
C++ symbol is excluded from supported binding accounting.

## Contracts and evidence

The table/emitter covers coordinates, points, vectors, directions, five axis types,
two matrices, quaternions, 2D/3D lines and conics, and five analytic surface values.
Inputs validate finite fields. Directions scale before normalization to avoid overflow
for finite extreme values; the native resolution lower bound remains enforced. Complete
frames preserve Y direction, orthogonality and handedness. Outputs copy native fields,
including sentinel values defined by a native getter. No borrowed storage escapes.

The 74-declaration return-reference audit finds individual fields, indexed values and
one retained surface handle, with no array-start reference. SC-062 records two exact
unavailable artifact signatures and two remaining projection contracts; no manual
binding ID is added. The 223 verified Blocked reason refinements are explicitly listed
in `config/batches/batch-y-disposition-refinements.json`; they do not count as migration.
The final inventory preserves all 116,272 IDs and all 32 header exclusions, with zero
SupportedUnselected/pending declarations. Exact accounting passes; coverage is 18.81%
including 1,217 manual IDs. The full inventory hash and evidence are maintained in STATUS.

Twelve focused runtime tests cover public curve/surface round trips, direct/indirect
frames, derivative and quaternion calculations, nested invalid inputs, large directions,
copy lifetime, Documents/XDE/IGES values and raw failure outputs. The shared geometric
workflow is compiled into the runtime suite and both clean package consumers. Native
fixtures verify row/column semantics with non-symmetric matrices and all 21 generated
headers as C11. Both native and managed sides check thirty record layouts. Full suites,
actual Debug-native, exact exports, cold regeneration and package checks all pass.
Release/Debug Generator is 144/144 and Runtime 493/493; actual Debug-native is 493/493.
Both clean consumers pass, and cold regeneration preserves all 96 generated files.
STATUS records final documentation-package and local completion evidence.

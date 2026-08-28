# Versioning

OcctSharp has several related but distinct versions. They must never be collapsed
into one ambiguous version string.

## Version identities

| Identity | Purpose |
|---|---|
| OcctSharp package version | Public managed SDK release |
| Generator version | Generation behavior and model/emitter rules |
| Native ABI version | Compatibility between managed raw bindings and native bridge |
| Binding model schema version | Compatibility of canonical manifests and baselines |
| Configuration schema version | Compatibility of generator configuration files |
| OCCT version/build ID | Exact upstream native API and ABI input |

## Proposed policy

- Use semantic versioning for released OcctSharp packages and generator tooling.
- Increment the native ABI major version for incompatible ABI changes.
- Increment the native ABI minor version for compatible additive changes only after
  compatibility tests establish that property.
- Treat changes to ownership, disposal, error behavior, or public managed signatures
  as compatibility-significant even if native export names remain unchanged.
- Record the exact OCCT build identity in package metadata and runtime diagnostics.
- Do not force the OcctSharp package version to equal the OCCT version.

This policy is proposed until package layout and release ADRs are accepted.

## Runtime identity

The managed SDK should be able to query and report:

- OcctSharp managed package version.
- Generator version that produced the raw binding.
- Expected and loaded native ABI versions.
- Native bridge build identity.
- OCCT version/build identity.
- Platform, architecture, and compiler identity where useful for diagnosis.

Version mismatch diagnostics must show both expected and actual values.

The current workspace reports native ABI 1.15 and native bridge 0.16.0. ABI 1.4 adds the
generated `Precision`, `TopAbs`, `Standard`, `TopLoc`, and `gp` value-copy exports plus
the `gp_Pnt` default/copy constructors over ABI 1.3's coordinate constructor. ABI 1.5
adds the invalid-handle status and live shape-handle registry; the compatibility claim
is limited to the currently tested Windows x64 configuration. ABI 1.6 adds the
experimental `Standard_Transient` shared-handle probe and the package advances to
`0.1.0-alpha.3`. ABI 1.7 adds shared-handle runtime type identity queries and the
package advances to `0.1.0-alpha.4`. ABI 1.8 adds the checked derived shared-handle
cast and the package advances to `0.1.0-alpha.5`.
ABI 1.9 adds generated typed shared handles, beginning with `Geom_CartesianPoint`,
and the package advances to `0.1.0-alpha.6`. Configuration schema 1.3 adds explicit
shared-handle scopes without changing older value-scope configuration semantics.
ABI 1.10 adds generated `TopoDS_Shape` copy/null/kind/orientation/reversal and identity
semantics. Configuration schema 1.4 adds explicit topology scopes, and the package
advances to `0.1.0-alpha.7`.
ABI 1.11 adds generated checked `TopoDS_*` conversions for the eight topology subtypes;
the package advances to `0.1.0-alpha.8`.
ABI 1.12 adds the opaque `gp_Trsf` transformation value bridge and the package
advances to `0.1.0-alpha.9`.
ABI 1.13 adds the opaque `TopLoc_Location` value bridge and the package advances to
`0.1.0-alpha.10`.
ABI 1.14 completes the B05 opaque `gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` value
family, including vector/axis transform creation, and the package advances to
`0.1.0-alpha.11`.
ABI 1.15 begins B06 with opaque OCCT string and `NCollection_Sequence<double>` values;
the package advances to `0.1.0-alpha.12`.
ABI 1.16 adds opaque `NCollection_Array1<double>` and the OCCT 8 dynamic-array-backed
`NCollection_Vector<double>` value collections; the package advances to `0.1.0-alpha.13`.
ABI 1.17 adds opaque integer-key `NCollection_DataMap<int,double>` and
`NCollection_IndexedMap<int>` collections; the package advances to `0.1.0-alpha.14`.
ABI 1.18 adds caller-owned snapshot exports for the B06 scalar collections and maps;
the package advances to `0.1.0-alpha.15` and the bridge version to `0.19.0`.
The first B07 `GpPoint` managed facade is additive and advances the package to
`0.1.0-alpha.16` without changing the native ABI or bridge version.
The B07 `gp_XYZ` value bridge is additive ABI 1.19/bridge 0.20.0 and advances the
package to `0.1.0-alpha.17`.
The B07 `gp_Lin` value bridge is additive ABI 1.20/bridge 0.21.0 and advances the
package to `0.1.0-alpha.18`.
The B07 `gp_Circ` value bridge is additive ABI 1.21/bridge 0.22.0 and advances the
package to `0.1.0-alpha.19`.
The B07 `gp_Ax2` and `gp_Pln` bridges are additive ABI 1.22/1.23 and advance the
package through alpha.20. The `gp_Ax3` bridge is additive ABI 1.24/bridge 0.25.0 and
advances the package to `0.1.0-alpha.21`.
The first B08 `GProp_GProps`/`BRepGProp` property bridge is additive within ABI 1.24 and
advances the package to `0.1.0-alpha.22`; bridge implementation remains 0.25.0.
The first B09 sphere/cylinder primitive builders are additive within ABI 1.24 and
advance the package to `0.1.0-alpha.23`; bridge implementation remains 0.25.0.
The first B10 owning face snapshot is additive within ABI 1.24 and advances the package
to `0.1.0-alpha.24`; bridge implementation remains 0.25.0.
The first B11/B12 Fuse/Cut boolean operations are additive within ABI 1.24 and advance
the package to `0.1.0-alpha.25`; bridge implementation remains 0.25.0.
The first B13 bulk mesh snapshot is additive within ABI 1.24 and advances the package
to `0.1.0-alpha.26`; bridge implementation advances to 0.26.0. The B12
`ShapeFix_Shape` result contract then advances the package to `0.1.0-alpha.27` and
the bridge implementation to 0.27.0.
The B12 `ShapeUpgrade_UnifySameDomain` result contract advances the package to
`0.1.0-alpha.28` and the bridge implementation to 0.28.0.
The initial IGES reader transfer is additive within ABI 1.24 and advances the package
to `0.1.0-alpha.29`; bridge implementation advances to 0.29.0.
The initial STL reader transfer is additive within ABI 1.24 and advances the package
to `0.1.0-alpha.30`; bridge implementation advances to 0.30.0.
The null-topology failure contract is additive within ABI 1.24 and advances the
package to `0.1.0-alpha.31`; bridge implementation advances to 0.31.0.
The B09 basic construction completion advances the package to `0.1.0-alpha.32` and
the bridge implementation to 0.32.0 without changing ABI major/minor 1.24.
The B08 adaptor snapshot completion is additive ABI 1.25/bridge 0.33.0 and advances
the package to `0.1.0-alpha.33`. It adds only fixed value-copy structures and operations;
no adaptor or borrowed geometry lifetime becomes part of the public ABI.
The B11 Common and minimum-distance completion is additive ABI 1.26/bridge 0.34.0 and
advances the package to `0.1.0-alpha.34`. Common reuses owning shape handles; distance
adds a fixed copied result without exposing algorithm or support lifetimes.
The B14 mesh-format completion is additive ABI 1.27/bridge 0.35.0 and advances the
package to `0.1.0-alpha.35`. It adds geometry-only OBJ, glTF/GLB, and VRML read/write
plus PLY write while keeping format providers and configuration native-local. PLY read
remains upstream-unsupported in OCCT 8.0.1.
The B15 OCAF document completion is additive ABI 1.28/bridge 0.36.0 and advances the
package to `0.1.0-alpha.36`. It adds owning documents, stable-entry parent-bound labels,
transactions, copied name attributes, and BinOcaf persistence without exposing TDF
node or label layouts.
The B16 XDE metadata completion is additive ABI 1.29/bridge 0.37.0 and advances the
package to `0.1.0-alpha.37`. It adds parent-bound XDE labels, copied colors/layers/
materials, assemblies/occurrences/locations, BinXCAF persistence, and STEPCAF exchange.
The B17 visualization-core completion is additive ABI 1.30/bridge 0.38.0 and advances
the package to `0.1.0-alpha.38`. It adds an HWND-bound thread-affine viewer owner,
parent-bound presentation IDs, explicit input/resize forwarding, and copied selection
snapshots without exposing AIS/V3d pointers or reverse callbacks.
The alpha.39 StepBasic generated-binding milestone is additive ABI 1.31/bridge 0.39.0 and
advances the package to `0.1.0-alpha.39`. It adds typed Int32-backed enums and ten
registry-validated intrusive shared-entity wrappers without changing existing exports.
The alpha.40 package expansion is additive ABI 1.32/bridge 0.40.0 and advances the package
to `0.1.0-alpha.40`. It expands the same verified ownership contract to 129 generated
StepBasic public types and 333 manifest IDs without removing an existing managed or
native API.
The alpha.41 common-modeling profile is additive ABI 1.33/bridge 0.41.0 and advances the
package to `0.1.0-alpha.41`. It adds owning cone/torus, extrusion/revolution,
fillet/chamfer, offset, and section results plus copied bounds/validity/count values;
schema 1.6 reconciles the 18 directly used declarations as accepted manual bindings.
The current large geometry/topology/XDE workstream is additive ABI 1.34/bridge 0.42.0
and advances the package to `0.1.0-alpha.42`. It adds curve/surface construction,
evaluation and projection, topology adjacency, loft/pipe/sewing, wedge/thick-solid,
Boolean history summaries, and composable STEPCAF import without removing the obsolete
compatibility assembly facade. Schema 1.6 now reconciles 61 accepted manual stable IDs.
The generated Geom/Geom2d expansion is additive ABI 1.35/bridge 0.43.0 and advances the
package to `0.1.0-alpha.43`. It reuses the generated intrusive shared-handle contract for
eight new public types and 67 additional emitted stable IDs; existing public/native APIs
are not removed.
The generated mesh/analysis/healing expansion is additive ABI 1.36/bridge 0.44.0 and
advances the package to `0.1.0-alpha.44`. Binding-model schema 1.2 records abstract
records and excludes them from package-level construction; 61 public types and 375
emitted stable IDs are added without removing an existing public/native API.
The generated STEP model expansion is additive ABI 1.37/bridge 0.45.0 and advances the
package to `0.1.0-alpha.45`. It adds concrete StepGeom, StepRepr, StepShape, and StepVisual
shared owners while keeping cross-generated Handle<T> relationships gated.
The cross-generated shared-handle expansion is additive ABI 1.38/bridge 0.46.0 and
advances the package to `0.1.0-alpha.46`. It adds nullable handle parameters and returns
between already selected generated owners, with target-registry validation and independent
retention of returned wrappers; existing public/native APIs are not removed.
The extended STEP entity expansion is additive ABI 1.39/bridge 0.47.0 and advances the
package to `0.1.0-alpha.47`. It adds selected StepAP203/AP214/AP242, DimTol, Element, FEA,
and Kinematics shared entities under the existing TM006 ownership contract.
The IGES entity expansion is additive ABI 1.40/bridge 0.48.0 and advances the package to
`0.1.0-alpha.48`. It adds selected IGES application/basic/definition/dimension/drawing/
geometry/graphics/solid entities without treating IGES sessions as standalone owners.
The final Batch B long-tail wave is additive ABI 1.41/bridge 0.49.0 and advances the
package to `0.1.0-alpha.49`. It emits all named Int32-compatible enums in the generated
profile, adds verified void/static and Standard foundation free-function projections,
and replaces LT001-LT004 with narrow evidence-backed dispositions. Existing public and
native APIs are not removed.
The clone-and-run distribution wave keeps ABI 1.41/bridge 0.49.0 and advances the
package to `0.1.0-alpha.50`. It adds the MIT project license, committed SHA256-pinned
Windows x64 runtime, complete bundled notices, and Sample `--smoke` entry point. No
managed or native API is removed.

The first Batch C common-workflow wave is additive ABI 1.42/bridge 0.50.0 and advances
the package to `0.1.0-alpha.51`. It adds native BREP exchange, topology/tolerance
summaries, detailed UV/normal/face-mapped meshes, XDE part metadata convenience, and
viewer appearance/camera/selection controls. The committed Windows x64 runtime and
manifest advance with the source so a fresh clone cannot load the alpha.50 bridge.

The second Batch C import-diagnostics and repair wave is additive ABI 1.43/bridge 0.51.0
and advances the package to `0.1.0-alpha.52`. It adds typed STEP read reports, copied
BRepCheck issue snapshots, owning ShapeFix results with before/after validation, and
thread-affine V3d mouse rotation. No existing managed or native API is removed.

The third Batch C XDE property/occurrence/exchange-options wave is additive ABI 1.44/
bridge 0.52.0 and advances the package to `0.1.0-alpha.53`. It adds optional XCAF area,
volume, and centroid snapshots, recursive occurrence/world-location results, independent
located shapes, and explicit STEPCAF metadata/model-type switches. Existing no-option
STEPCAF calls retain their all-metadata defaults.

## Upgrade classification

Alpha.50 uses the unchanged alpha.49 generated surface: 16,353 emitted and 61 accepted
manual declarations. The
observed full inventory remains separate: 116,272 classified declarations, zero
supported-unselected, and 50,514 narrow blocked dispositions. Package verification is
pinned to .NET SDK 10.0.400 by the inner workspace `global.json`.

An OCCT upgrade report must classify:

- Source/API changes in the selected generation scope.
- Native ABI changes in OcctSharp.
- Managed raw API changes.
- Friendly public API changes.
- Behavioral, ownership, and packaging changes.
- Compatibility impact and required migration.

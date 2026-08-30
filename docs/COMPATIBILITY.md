# Compatibility

This document records verified compatibility only. Planned combinations are not
reported as supported.

## Current verified matrix

| OcctSharp | Generator | Native ABI | OCCT | Platform | Compiler | .NET | Status |
|---|---|---|---|---|---|---|---|
| 8.0.1-preview.8 workspace | ClangSharp 21.1.8.4 | 1.53 | 8.0.1 VC14 x64 combined | Windows x64 | MSVC 19.51 / VS 2026 | net10.0 / SDK 10.0.400 | Experimental |

Validated in both Debug and Release: native and managed build, ABI/runtime identity,
OCCT box creation, topology traversal, error/disposal behavior, STEP geometry
round-trip, STL/IGES output, transformed compound assembly, controlled AST fixture,
real OCCT header discovery, TypeMaps, and two-run discovery determinism.
The three generated `gp_Pnt` value-copy constructors, 20 generated `Precision` static
methods, three generated `TopAbs` enum methods, and five additional scalar methods also
compile and run in both configurations.
The generated `GeomCartesianPoint` typed shared handle is validated for construction,
value access/mutation, retained clone/reference count, RTTI, and disposal.
The generated `Shape` topology value surface is validated for null state, kind,
orientation, copy independence, reversal, and OCCT partner/same/equal distinctions.
Checked `Compound` and `Solid` casts are validated for successful identity preservation,
wrong-kind rejection, and source-disposal independence; all eight cast exports compile.
The B05.1 `GpTrsf` owner is validated for identity, composition, clone, inversion,
finite-value rejection, matrix-index validation, and shape application in Debug and
Release runtime tests.
The B05.2 `TopLocLocation` owner is validated for identity, clone, inversion,
composition, conversion, and absolute/relative placement in Debug and Release.
The completed B05 value family is also validated for `GpVec` magnitude/dot/cross,
`GpDir` non-zero validation/reversal, `GpAx1` reversal and rotation conversion, and
`GpMat` identity/value/determinant/index validation in Debug and Release.
The first B06 wave is validated for UTF-8 `OcctAsciiString`, UTF-16/UTF-8
`OcctExtendedString`, conversion/clone ownership, and mutable `OcctRealSequence`
index translation in Debug and Release.

The second B06 wave adds `OcctRealArray` lower-bound translation and
`OcctRealVector` dynamic-array append/mutation/enumeration in Debug and Release.
The B07 geometry values now include `GpAx2Value`, `GpPlane`, and `GpAx3Value`; the
alpha.21 package consumer validates their orientation, signed-distance, directness, and
construction-failure semantics against OCCT 8.0.1.
The alpha.22 package additionally validates `GPropProperties` volume mass, centre of
mass, inertia symmetry, clone, and density-weighted composition over a box shape.
The alpha.23 package also validates native-local sphere and cylinder builders and their
finite-positive dimension failures. The alpha.24 package additionally validates owning
face/edge/wire/vertex snapshots and parent-disposal independence for a box topology.
The alpha.33 package validates BRepAdaptor edge-curve and face-surface value snapshots,
including fixed ABI layouts, copied line/plane values, wrong-kind rejection, and source
disposal independence under ABI 1.25/bridge 0.33.0.
The alpha.34 package adds owning Common topology and copied minimum-distance results,
including point pairs, solution counts, fixed layout, failures, and source independence.
The alpha.35 package adds geometry-only OBJ, glTF/GLB, and VRML read/write plus PLY
write. Release/Debug tests write all four formats and read OBJ/GLB/VRML into non-empty
topology. PLY read is upstream-unsupported in OCCT 8.0.1.
The alpha.36 package adds owning BinOcaf documents, stable-entry parent-bound labels,
transactions, UTF-8 names, and binary persistence. Release/Debug tests cover commit,
abort, parent disposal, and save/open semantics.
The alpha.37 package adds XDE shape/assembly/occurrence labels, copied effective colors,
multiple layers, physical materials, locations, BinXCAF persistence, and STEPCAF
metadata exchange. Release/Debug and clean-consumer tests exercise both round-trips.
The alpha.38 package adds the Windows HWND viewer, AIS shape display, resize/redraw/fit,
mouse detection/selection, copied presentation snapshots, and creating-thread checks.
Release/Debug tests and the clean consumer use a real off-screen HWND; the interactive
sample compiles but was not launched as part of automated validation.
The alpha.39 package adds generated typed enums and ten StepBasic intrusive shared
entities. Release/Debug tests cover scalar, boolean, enum, clone/reference-count, RTTI,
idempotent disposal, and disposed-use behavior; the clean consumer repeats a shared
clone and enum round-trip.
The alpha.40 package expands the same ownership contract to all 129 default-constructible
generated StepBasic public types selected by schema 1.5. Release/Debug and clean-consumer
tests construct, clone, reference-count, and dispose every type. A missing-native
repository Sample simulation also rebuilds the 45-DLL Debug runtime and runs an English
entity-creation workflow.
The alpha.41 package adds native-local cone/torus, extrusion/revolution,
all/single-edge fillet/chamfer, offset, section, finite bounds, validity, and public
subshape-count APIs. Release/Debug runtime tests and the clean consumer validate owning
result independence, value layouts, errors, and the 47-DLL closure with TKFillet/TKOffset.
The alpha.42 package adds curve/surface construction, evaluation and projection,
topology adjacency, loft/pipe/sewing, wedge/thick-solid, copied Boolean history, and
composable XDE STEP import. Release/Debug pass Generator 44/44 and Runtime 90/90; the
clean consumer exercises these families with the unchanged 47-DLL runtime closure.
The alpha.43 package expands deterministic shared-handle generation to eight additional
Geom/Geom2d types and 67 emitted declarations. Release/Debug pass Generator 44/44 and
Runtime 93/93; the clean consumer exercises point, direction, vector, plane, transform,
clone/reference-count, RTTI, and disposal behavior.
The alpha.44 package expands the same shared-owner generator to 61 BRepMesh, Poly,
ShapeAnalysis, ShapeFix, and ShapeUpgrade types and 375 additional emitted declarations.
Binding-model schema 1.2 excludes abstract records before emission. Release/Debug pass
Generator 44/44 and Runtime 96/96; the clean consumer exercises representatives from all
five package families, retained ownership, RTTI, scalar state, and the 47-DLL closure.

The map wave adds integer-key real lookup/bind/unbind and ordered indexed-key behavior
with clone and duplicate-key validation in Debug and Release.

The local `OcctSharp.0.1.0-alpha.44.nupkg` is clean-consumer validated for Windows x64:
47 native DLLs are copied below the published application's `occt` directory, automatic
native resolution reports ABI 1.36/bridge 0.44.0/OCCT 8.0.1, and generated, modeling,
exchange, OCAF, BinXCAF, STEPCAF metadata/assembly, adaptor, and viewer checks succeed.

This is not yet `Supported`: CI, broad ownership, generated XDE/OCAF bindings,
committed licensed fixtures, complete third-party notices/provenance, and public release gates have
not run. A manual STEPCAF/XDE assembly has been validated with local metadata-bearing
fixtures in Debug/Release scope; it is not a general document API.

The alpha.48 package adds the selected IGESAppli, IGESBasic, IGESDefs, IGESDimen,
IGESDraw, IGESGeom, IGESGraph, and IGESSolid entity families. Release and Debug native/
managed builds pass Generator 44/44 and Runtime 147/147; 13 generated files are fresh,
clean regeneration is byte-identical, and the alpha.38 API diff is 10,272 additions and
zero removals. A clean SDK 10.0.400 consumer restores, publishes, and runs with all 47
native DLLs below `occt`, reporting ABI 1.40, bridge 0.48.0, and OCCT 8.0.1. This remains
Experimental because B still has 11,144 supported-unselected declarations and unresolved
publication/legal gates.

The alpha.49 workspace expands the generated manifest to 16,353 stable IDs and removes
the final supported-unselected and LT001-LT004 inventory states. Release and Debug pass
Generator 62/62 and Runtime 105/105 with ABI 1.41/bridge 0.49.0. Clean alpha.49 package
consumer and complete release-check evidence are recorded separately in `STATUS.md`;
public legal, hosted-CI, signing, and publication gates remain independent.

The alpha.50 distribution keeps the same API, ABI, bridge, and OCCT identities while
adding the committed 62-DLL Windows x64 runtime. Debug and Release Sample smoke must run
from an ordinary clone without an OCCT SDK; each committed runtime and notice file is
verified against `runtime-manifest.json`.

The alpha.51 workspace advances to ABI 1.42/bridge 0.50.0 for the first Batch C
cross-family checkpoint. Release and Debug pass Generator 62/62 and Runtime 107/107.
Clean regeneration is byte-identical, the clean package consumer runs with all 62 DLLs,
and runtime tests cover native BREP, topology/tolerance snapshots, detailed mesh
normals/UV/face mapping, XDE part metadata composition, and real-HWND viewer appearance,
camera, and selection modes.

The alpha.52 workspace advances to ABI 1.43/bridge 0.51.0 for the second Batch C
cross-family checkpoint. Release and Debug pass Generator 62/62 and Runtime 107/107.
Runtime tests cover typed STEP read/transfer/unit reports, copied per-subshape BRepCheck
statuses, owning ShapeFix repair with before/after validation, and real-HWND mouse
rotation. Clean package and release evidence are recorded in `STATUS.md`.

The alpha.53 workspace advances to ABI 1.44/bridge 0.52.0 for the third Batch C
cross-family checkpoint. Release and Debug pass Generator 62/62 and Runtime 108/108.
Runtime tests cover BRepGProp-derived XCAF properties, nullable attribute mutation and
clearing, nested occurrence/world-location traversal, independent located shapes, and
STEPCAF metadata/model-type options. Clean package and release evidence are recorded in
`STATUS.md`.

The alpha.54 workspace advances to ABI 1.45/bridge 0.53.0 and closes Batch C. Release
and Debug builds pass with Generator 91/91, Runtime 114/114, and dependency profiles
6/6. Tests cover copied edge/surface derivatives and pcurves, owning trim/wire/reshape
results, bidirectional adjacency, owning STEP sessions with file units/selective roots/
target units, real-HWND whole/subshape selection with owning selected topology, application
input forwarding, and a real STEP import-edit-export-re-read-viewer workflow. The clean
consumer executes the final workflow with all 62 DLLs; 83 generated files are fresh and
byte-identical after clean regeneration.

The alpha.55 workspace advances to ABI 1.46/bridge 0.54.0 and closes Batch D. Release
and Debug builds pass with Generator 91/91, Runtime 115/115, and dependency profiles
6/6. A real STEP/XDE assembly flows through copied viewer identity, exact owning picks,
rectangle/polygon selection, filters, reversible isolate, per-subshape review styles,
camera and coordinate state, clipping/review aids, and Unicode screenshot output on a
real HWND. The clean consumer executes the same complete workflow with all 62 DLLs;
83 generated files remain fresh and byte-identical after clean regeneration.

The `8.0.1-preview.1` workspace changes only NuGet and informational package identity
under ADR-0065. Managed assembly identity remains `0.1.0.0`; ABI 1.46, bridge 0.54.0,
OCCT 8.0.1, generated surface, and the 62-DLL runtime closure are unchanged. Direct
nupkg inspection, clean restore/publish/runtime, Release/Debug, Generator 91/91, Runtime
115/115, dependency profiles 6/6, clean regeneration, API/inventory, provenance, and the
complete Preview.1 local release check pass. Batch E is prepared at 0/24 and contributes
no compatibility claim yet.

The `8.0.1-preview.2` workspace advances to ABI 1.47/bridge 0.55.0 and completes Batch
E. Release and Debug pass Generator 91/91, Runtime 119/119, and dependency profiles 6/6.
Exact inspection, complete semantic PMI/reference mutation, BinXCAF and explicit AP242
GDT/view round trips, saved views, four viewer-owned dimension kinds, and real-HWND
screenshot output pass. The clean consumer executes the complete workflow with the
application-local 62-DLL closure; 83 generated files are fresh and byte-identical after
clean regeneration. The additive API diff is 37,490 additions and zero removals.

The `8.0.1-preview.3` workspace advances to ABI 1.48/bridge 0.56.0 and completes Batch
F. Release and Debug pass Generator 91/91, Runtime 123/123, and dependency profiles 6/6;
the clean 62-DLL package repeats the complete freeform authoring workflow. The additive
API diff against alpha.38 is 37,636 additions and zero removals.

The `8.0.1-preview.4` and `8.0.1-preview.5` workspaces complete Batch G technical drawing
and Batch H advanced mesh/scene interchange at ABI 1.49/bridge 0.57.0 and ABI 1.50/
bridge 0.58.0 respectively. Their complete evidence remains in their release notes.

The `8.0.1-preview.6` workspace advances to ABI 1.51/bridge 0.59.0 and completes Batch I.
Release and Debug pass Generator 91/91 and Runtime 135/135; focused Batch I 4/4 and the
clean 62-DLL package consumer validate copied typed state, dependency graphs, named
history, undo/redo/savepoints, all four OCAF/XCAF persistence formats, STEP/XDE, and
source-disposal ownership.

The `8.0.1-preview.7` workspace advances to ABI 1.52/bridge 0.60.0 and completes Batch J.
Release and Debug pass Generator 91/91 and Runtime 139/139; focused Batch J 4/4 and the
clean 62-DLL package consumer validate selected/variable/planar features, robust Boolean
options and preflight, bounded recovery, copied modified/generated/deleted history,
STEP/XDE, real HWND screenshots, and source-disposal lifetime. API comparison against
alpha.38 is additive at 38,232 additions and zero removals.

The `8.0.1-preview.8` workspace advances to ABI 1.53/bridge 0.61.0 and completes Batch K.
Release and Debug pass Generator 91/91 and Runtime 143/143; focused Batch K 4/4 and the
clean 62-DLL package consumer validate assembly editing, occurrence paths, graph/BOM,
references, effective metadata, rollups, history rollback, STEP/XDE, real HWND
screenshots, and source/document-disposal lifetime.

## Planned validation dimensions

- Operating system and version.
- CPU architecture.
- Native compiler and runtime library.
- OCCT version and build configuration.
- .NET target framework and runtime.
- Debug and Release native builds where behavior differs.
- Managed JIT and NativeAOT where claimed.

## Support states

- **Supported** — build, runtime, lifetime, integration, and packaging checks required
  by the release policy passed.
- **Experimental** — available for evaluation but missing part of the release matrix.
- **Build only** — compilation passed; runtime support is not claimed.
- **Unsupported** — deliberately not provided.
- **Not evaluated** — no reliable current evidence.

## OCCT upgrade entry requirements

Every newly supported OCCT version requires:

1. Immutable dependency record.
2. Canonical API diff against the previous supported baseline.
3. Successful regeneration without unresolved safety-critical mappings.
4. Native and managed compile validation.
5. Required runtime, lifetime, integration, and real-file validation.
6. Native dependency and packaging inspection.
7. Updated compatibility row and upgrade report.

Supporting a new OCCT version does not imply binary compatibility between native
bridges compiled against different OCCT builds.

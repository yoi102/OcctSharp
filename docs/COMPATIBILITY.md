# Compatibility

This document records verified compatibility only. Planned combinations are not
reported as supported.

## Current verified matrix

| OcctSharp | Generator | Native ABI | OCCT | Platform | Compiler | .NET | Status |
|---|---|---|---|---|---|---|---|
| 0.1.0-alpha.41 workspace | ClangSharp 21.1.8.4 | 1.33 | 8.0.1 VC14 x64 combined | Windows x64 | MSVC 19.51 / VS 2026 | net10.0 / SDK 10.0.400 | Experimental |

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

The map wave adds integer-key real lookup/bind/unbind and ordered indexed-key behavior
with clone and duplicate-key validation in Debug and Release.

The local `OcctSharp.0.1.0-alpha.41.nupkg` is clean-consumer validated for Windows x64:
47 native DLLs are copied below the published application's `occt` directory, automatic
native resolution reports ABI 1.33/bridge 0.41.0/OCCT 8.0.1, and generated, modeling,
exchange, OCAF, BinXCAF, STEPCAF metadata/assembly, adaptor, and viewer checks succeed.

This is not yet `Supported`: CI, broad ownership, generated XDE/OCAF bindings,
committed licensed fixtures, complete third-party notices/provenance, and public release gates have
not run. A manual STEPCAF/XDE assembly has been validated with local metadata-bearing
fixtures in Debug/Release scope; it is not a general document API.

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

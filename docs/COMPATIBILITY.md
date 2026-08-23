# Compatibility

This document records verified compatibility only. Planned combinations are not
reported as supported.

## Current verified matrix

| OcctSharp | Generator | Native ABI | OCCT | Platform | Compiler | .NET | Status |
|---|---|---|---|---|---|---|---|
| 0.1.0-alpha.14 workspace | ClangSharp 21.1.8.4 | 1.17 | 8.0.1 VC14 x64 combined | Windows x64 | MSVC 19.51 / VS 2026 | net10.0 / SDK 10.0.400 | Experimental |

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

The map wave adds integer-key real lookup/bind/unbind and ordered indexed-key behavior
with clone and duplicate-key validation in Debug and Release.

The local `OcctSharp.0.1.0-alpha.14.nupkg` is clean-consumer validated for Windows x64:
36 native DLLs are copied below the published application's `occt` directory, automatic
native resolution reports ABI 1.17/bridge 0.18.0/OCCT 8.0.1, and generated
`GeomCartesianPoint` plus typed topology behavior succeeds.

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

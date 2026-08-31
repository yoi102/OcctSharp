# NuGet Packaging

## Current package set

The current experimental package version is `8.0.1-preview.10` for .NET 10 and
Windows x64. ADR-0074 produces 14 packages:

- 12 managed module packages: Runtime, Foundation, Geometry, MeshData, Modeling, Mesh,
  Documents, Visualization, DataExchange, Xde, IVtk, and Draw;
- `OcctSharp`, the compatibility entry, cross-family facade, and convenience meta-package;
- `OcctSharp.Native.win-x64`, the only package containing `OcctSharp.Native.dll`, the
  complete 62-DLL OCCT/third-party runtime closure, the transitive copy target, and all
  bundled notice/license material.

Every managed package contains one assembly/XML documentation pair and zero native DLLs.
`OcctSharp.Runtime` depends on the native package; higher modules receive it transitively.
All packages use the MIT project-license expression and include the repository README.
The facade package also embeds the stable linked documentation set.

The live repository `docs/STATUS.md` is intentionally not embedded in the package: it
contains a package hash and would make the artifact self-referential. Release notes and
the stable architecture/topic documents are packaged; status remains authoritative in
the repository.

This package is validated locally but is not approved for publication. The release workstream produces
immutable native provenance, SBOM/checksum evidence, API diff, and CI configuration.
The project license and bundled third-party notice layout are resolved by ADR-0059;
hosted release execution, signing, and publication are separate `NOT RUN` gates.

## Application output layout

Package consumers receive this layout:

```text
ApplicationOutput/
├── Application.exe
├── Application.dll
├── OcctSharp.Runtime.dll
├── selected OcctSharp module DLLs
├── OcctSharp.dll                    # only for facade/meta-package consumers
└── occt/
    ├── OcctSharp.Native.dll
    ├── TKernel.dll
    ├── TK*.dll
    └── required third-party DLLs
```

The package does not put `OcctSharp.Native.dll` beside the application executable.
The managed resolver loads the bridge from the absolute `occt` path. The complete
dependency closure must stay together in that directory; applications do not need to
modify `PATH` or call a configuration API.

## Create and verify the package

From the inner `OcctSharp/` workspace:

```powershell
.\eng\pack.ps1
.\eng\verify-package.ps1 -SkipBuild
```

`pack.ps1` performs a Release build unless `-SkipBuild` is supplied, then writes all 14
packages to `artifacts/packages/`. `verify-package.ps1` audits that only the native
package contains the 62 DLLs. It restores the local `OcctSharp` facade package into
`tests/OcctSharp.PackageConsumer` and the direct `OcctSharp.Modeling` package into
`tests/OcctSharp.ModuleConsumer`. Both are published and executed. The facade consumer
checks the output layout, loads the native runtime, checks ABI/OCCT identity, and exercises generated
`GeomCartesianPoint`, base `TopoDS_Shape`, typed topology, modeling, mesh, exchange,
all 129 generated StepBasic shared types and typed enums,
the generated Geom/Geom2d point/direction/vector/plane/transformation families,
generated BRepMesh/Poly/ShapeAnalysis/ShapeFix/ShapeUpgrade shared types,
copied BRep adaptor snapshot behavior, OBJ/PLY/GLB/VRML provider workflows, BinOcaf
document persistence, BinXCAF/STEPCAF metadata assemblies, and an HWND-bound viewer
display/selection smoke. It also exercises cone/torus, extrusion, fillet/chamfer,
offset, section, bounding, validity, topology-count, curve/surface evaluation and
projection, topology adjacency, loft/pipe/sewing, wedge/thick-solid, Boolean history,
composable XDE STEP import, native BREP read/write, topology/tolerance summaries,
detailed mesh normals/UV/face mapping, atomic XDE part metadata, typed STEP read reports,
per-subshape validation, repair comparison, XCAF validation properties, recursive XDE
occurrences/world locations, explicit STEPCAF options, and common viewer appearance/
camera/selection/rotation operations. Alpha.54 additionally exercises edge/surface
derivatives and pcurves, edge/face trim, edge-wire construction, topology replace/remove,
bidirectional adjacency, owning STEP sessions with unit metadata and selective root
transfer, whole/subshape selection, owning selected topology, mouse/wheel/semantic-key
input, and the final real STEP edit/export/re-read/viewer workflow. The current
application-local closure contains 62 DLLs. Alpha.55 additionally runs the complete
24-capability STEP/XDE-to-real-HWND-to-screenshot review workflow, including copied
identity, owning detection, area selection/filtering, isolate/fit, subshape overrides,
camera conversions, clipping, review aids, and durable image output.
Preview.9 additionally runs the complete Batch L world-located XDE occurrence, copied
AABB/OBB, native-local broad/exact phase, filtering, clearance/contact/penetration/
containment, pair-matrix, diagnostics, incremental, real-HWND review, screenshot, and
source-disposal workflow.

The direct module consumer creates and inspects a six-face box, verifies OCCT 8.0.1 and
the same 62-DLL `occt/` closure, and fails if the `OcctSharp.dll` facade is present.

## Consumer use

Once a package source contains the package, an application uses the normal command:

```powershell
dotnet add package OcctSharp --version 8.0.1-preview.10
```

A narrow consumer can instead select a module, for example:

```powershell
dotnet add package OcctSharp.Modeling --version 8.0.1-preview.10
```

The application must run as a Windows x64 process on the current compatibility matrix.
Missing or incomplete native assets produce an exception naming the expected `occt`
directory rather than falling back to a machine-wide OCCT installation.

The repository ProjectReference and NuGet consumer paths both use a self-contained
runtime. A fresh clone copies the committed, manifest-verified DLL closure; native
contributors may explicitly opt into ADR-0051's SDK bootstrap. In all cases the
executable receives the same application-local `occt/` directory.

## Release evidence

`eng/release-check.ps1` rebuilds and revalidates the package, then writes the API diff,
CycloneDX SBOM, provenance, gate report, and SHA256 checksum list under
`artifacts/release/`. The checksum list covers the four JSON evidence records and the
`.nupkg` after the gate report is finalized. A passing local package consumer and
completed release tooling do not override a `BLOCKED` or `NOT RUN` publication gate.

## Managed split evidence

Preview.10 package verification runs from the inner `OcctSharp/` workspace, where
`global.json` selects SDK 10.0.400. Direct nupkg inspection confirms 13 managed packages
with one managed DLL and zero native DLLs each, plus one native package with exactly 62
DLLs. Package and informational versions are `8.0.1-preview.10`; managed assembly/file
identity remains `0.1.0.0`; ABI remains 1.54 and bridge remains 0.62.0.

The compatibility consumer restores, publishes, and runs the inherited Batch D-L paths.
The direct Modeling consumer proves module-only consumption without the facade. Both
converge on one application-local `occt` directory. Toolkit-per-package fragmentation and
native bridge splitting are not planned. Signing, hosted release execution, and
publication authorization remain separate `NOT RUN` gates.

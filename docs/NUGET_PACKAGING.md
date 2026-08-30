# NuGet Packaging

## Current package

The current experimental package is `OcctSharp` `8.0.1-preview.9` for .NET 10 and
Windows x64. It contains:

- `lib/net10.0/OcctSharp.dll` and XML documentation.
- The repository README and linked documentation set.
- The Release `OcctSharp.Native.dll` bridge.
- The complete currently inspected OCCT and third-party runtime DLL closure.
- A transitive MSBuild target that copies native files for build and publish.
- The MIT project-license expression and repository license.
- OCCT, oneTBB, FreeImage, FreeType, OpenVR, FFmpeg, and jemalloc license/notice material.

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
├── OcctSharp.dll
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

`pack.ps1` performs a Release build unless `-SkipBuild` is supplied, then writes the
package to `artifacts/packages/`. `verify-package.ps1` restores only that local package
into `tests/OcctSharp.PackageConsumer`, publishes it, checks the output layout, loads
the native runtime, checks ABI/OCCT identity, and exercises generated
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

## Consumer use

Once a package source contains the package, an application uses the normal command:

```powershell
dotnet add package OcctSharp --version 8.0.1-preview.9
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

## Planned package split

Preview.9 preparation package verification runs from the inner `OcctSharp/` workspace, where
`global.json` selects SDK 10.0.400. Direct nupkg inspection confirms package identity
`OcctSharp`/`8.0.1-preview.9`, managed assembly/file identity `0.1.0.0`, exact
informational version `8.0.1-preview.9`, ABI 1.53, bridge 0.61.0, and 62 native DLLs under
`occt`. The clean consumer restores, publishes, and runs the inherited Batch D-J paths
plus Batch K assembly edits, occurrence paths, graph/BOM, references, effective metadata,
rollups, history, STEP/XDE, real HWND screenshots, and source-disposal workflow. Signing,
hosted release execution, and publication
authorization remain separate `NOT RUN` gates.

The preparation nupkg is 40,952,685 bytes with SHA256
`B33DDF2D190ABB463A3C926387B05D73F2A0DD9461D4449D4BE4D64886A57FC4`.
This package verification does not claim Batch L implementation or replace Preview.8's
complete release-check evidence.

ADR-0015 keeps one package during the topology/modeling foundation, then introduces
Runtime, Foundation, Modeling, Mesh, DataExchange, Xde, Visualization, and optional IVtk
managed packages when the documented size/dependency/RID triggers are met. `OcctSharp`
becomes the convenience meta-package. Native assets later move to RID packages, but all
packages continue to converge on one application-local `occt` directory without duplicate
files. Toolkit-per-package fragmentation is not planned.

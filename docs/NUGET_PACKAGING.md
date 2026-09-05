# NuGet Packaging

## Current package set

The current local preview package version is `8.0.1-preview.17` for .NET 10 and
Windows x64. ADR-0074 produces 14 packages:

- 12 managed module packages: Runtime, Foundation, Geometry, MeshData, Modeling, Mesh,
  Documents, Visualization, DataExchange, Xde, IVtk, and Draw;
- `OcctSharp`, the compatibility entry, cross-family facade, and convenience meta-package;
- `OcctSharp.Native.win-x64`, the only package containing `OcctSharp.Native.dll`, the
  complete 62-DLL OCCT/third-party runtime closure, the transitive copy target, and all
  bundled notice/license material.

Every managed package contains one assembly/XML documentation pair and zero native DLLs.
`OcctSharp.Runtime` depends on the native package; higher modules receive it transitively.
All packages use the MIT project-license expression, the same practical package README,
and the 256-by-256 transparent `occtsharp-icon.png` package icon.
The facade package also embeds the stable linked documentation set.

The live repository `docs/STATUS.md` is intentionally not embedded in the package: it
contains a package hash and would make the artifact self-referential. Release notes and
the stable architecture/topic documents are packaged; status remains authoritative in
the repository.

Preview.17 is local-only and is not published. The release workstream produces
immutable native provenance, SBOM/checksum evidence, API diff, and CI configuration.
The project license and bundled third-party notice layout are resolved by ADR-0059;
hosted release execution and signing remain separate gates. Publication evidence is
recorded only after all 14 package IDs are indexed and a clean nuget.org consumer runs.

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
Preview.13 additionally runs the complete Batch N IGESCAF/XDE metadata, option,
diagnostic/unit, Unicode-path cleanup, mixed STEP/IGES, round-trip, lifetime, and
real-HWND workflow.

Preview.14 adds the complete Batch O copied-sketch to planar-feature workflow, including
mixed analytic/freeform loops, a hole-aware extrusion, offset, projection/intersection,
XDE metadata, STEP/IGES round-trip, and real-HWND review. The repository's focused tests
also cover reversed trim, similarity transforms, exact nesting, self-intersections,
overlap boundaries, numeric measurements, and explicit wire-gap tolerance.

Preview.15 adds the 32-capability Batch P surface/UV closure. A shared public-only
workflow exercises smooth UV definitions, lifting, repair, projections, trimmed curved
faces, sections, names/colors/layers in STEP and IGES, source disposal, and real-HWND
selection/screenshots in both repository tests and the isolated facade consumer.

Preview.16 adds the 40-capability Batch Q repair closure. The shared public-only
workflow checks a protected boundary, budgeted hole removal, atomic shared-definition
publication, names/colors, repeated placements, undo/redo, STEP/IGES reimport and real
defect selection/screenshots. The direct Modeling consumer additionally accepts an
independently owning repair result and verifies that it survives source/preview disposal.

Preview.17 adds R's 40-capability authored/edited mesh closure. The shared public-only
workflow verifies groups/materials, repeated shared XDE definitions, rigid placements,
undo/redo, Unicode STL/OBJ/glTF/GLB/PLY delivery, optional channels, independent PLY
readback and real-HWND revision replacement. Direct Modeling also snapshots an owning
discrete copy after source disposal. No mesher is invoked by the direct delivery path.
The native package keeps notices/licenses at their original relative paths beneath
`licenses/`, without duplicating dependency directories.

The direct module consumer creates and inspects a six-face box, verifies OCCT 8.0.1 and
the same 62-DLL `occt/` closure, and fails if the `OcctSharp.dll` facade is present.

## Consumer use

Once a package source contains the package, an application uses the normal command:

```powershell
dotnet add package OcctSharp --version 8.0.1-preview.17 --source ./OcctSharp/artifacts/packages
```

A narrow consumer can instead select a module, for example:

```powershell
dotnet add package OcctSharp.Modeling --version 8.0.1-preview.17 --source ./OcctSharp/artifacts/packages
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
After any final documentation repack, regenerate metadata/checksums and run
`eng/verify-package-content.ps1` to compare all stable documents, runtime/license bytes
and final checksum/provenance identities. Stale package documents must not pass delivery.

## Managed split evidence

Preview.17 package verification runs from the inner `OcctSharp/` workspace, where
`global.json` selects SDK 10.0.400. Direct nupkg inspection confirms 13 managed packages
with one managed DLL and zero native DLLs each, plus one native package with exactly 62
DLLs. Every nupkg contains the shared README and icon. Package and informational versions
are `8.0.1-preview.17`; managed assembly/file identity remains `0.1.0.0`; ABI is 1.61
and bridge is 0.69.0.

The compatibility consumer restores, publishes locally, and runs the Batch D-R paths.
The direct Modeling consumer proves module-only consumption without the facade. Both
converge on one application-local `occt` directory. Toolkit-per-package fragmentation and
native bridge splitting are not planned. Signing, hosted full release execution, NuGet
publication/indexing, and the public-source consumer remain separate gates.

Per the current delivery policy, a completed batch is locally packed, validated, and
committed. No NuGet publication is performed as part of that workflow. The user's
2026-09-05 instruction applies to every subsequent batch: check the local packages only.
Any later publication requires a new explicit request for the exact version; earlier
Preview.12 upload permission does not authorize another upload.

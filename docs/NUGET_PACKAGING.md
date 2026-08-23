# NuGet Packaging

## Current package

The current experimental package is `OcctSharp` `0.1.0-alpha.14` for .NET 10 and
Windows x64. It contains:

- `lib/net10.0/OcctSharp.dll` and XML documentation.
- The repository README and linked documentation set.
- The Release `OcctSharp.Native.dll` bridge.
- The complete currently inspected OCCT and third-party runtime DLL closure.
- A transitive MSBuild target that copies native files for build and publish.
- The OCCT LGPL 2.1 text and OCCT linking exception.

The live repository `docs/STATUS.md` is intentionally not embedded in the package: it
contains a package hash and would make the artifact self-referential. Release notes and
the stable architecture/topic documents are packaged; status remains authoritative in
the repository.

This package is validated locally but is not approved for publication. A project
license, complete third-party notices, immutable native provenance, CI, and the other
release gates remain required.

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
`GeomCartesianPoint`, base `TopoDS_Shape`, and typed topology behavior.

## Consumer use

Once a package source contains the package, an application uses the normal command:

```powershell
dotnet add package OcctSharp --version 0.1.0-alpha.14
```

The application must run as a Windows x64 process on the current compatibility matrix.
Missing or incomplete native assets produce an exception naming the expected `occt`
directory rather than falling back to a machine-wide OCCT installation.

## Planned package split

ADR-0015 keeps one package during the topology/modeling foundation, then introduces
Runtime, Foundation, Modeling, Mesh, DataExchange, Xde, Visualization, and optional IVtk
managed packages when the documented size/dependency/RID triggers are met. `OcctSharp`
becomes the convenience meta-package. Native assets later move to RID packages, but all
packages continue to converge on one application-local `occt` directory without duplicate
files. Toolkit-per-package fragmentation is not planned.

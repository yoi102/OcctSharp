# OcctSharp

OcctSharp is a .NET 10 SDK for Open CASCADE Technology (OCCT) 8.0.1. It combines a
versioned native C ABI, generated low-level bindings, and friendly managed CAD APIs for
modeling, STEP/IGES/STL exchange, XDE assemblies and metadata, meshing, inspection, and
Windows visualization.

Current local preview: `8.0.1-preview.15` for Windows x64. The NuGet graph contains 12 managed
modules, the `OcctSharp` compatibility/facade package, and one shared
`OcctSharp.Native.win-x64` runtime package. The native package places the complete
62-DLL runtime in the application's `occt/` directory; no machine-wide OCCT installation
or `PATH` change is required.

## Install

```powershell
dotnet add package OcctSharp --version 8.0.1-preview.15 --source ./OcctSharp/artifacts/packages
```

A narrow consumer can reference a module directly, for example:

```powershell
dotnet add package OcctSharp.Modeling --version 8.0.1-preview.15 --source ./OcctSharp/artifacts/packages
```

The supported runtime baseline is .NET 10, Windows x64, and OCCT 8.0.1.
Preview.15 is a local-only build; it is not published on NuGet.org. Create the
local package feed with `OcctSharp/eng/pack.ps1` using the contributor toolchain, or run
the repository samples directly with the committed runtime.

## Create a solid

```csharp
using OcctSharp;

using Shape box = ShapeFactory.CreateBox(40, 30, 20);
using Shape cylinder = ShapeFactory.CreateCylinder(6, 20);

Console.WriteLine($"Box faces: {box.FaceCount}");
Console.WriteLine($"Cylinder faces: {cylinder.FaceCount}");
```

## Build a 2D sketch with a hole and extrude it

```csharp
using OcctSharp;

SketchCurveChain2d outer = SketchCurveChain2d.Create([
    SketchCurve2d.Segment(new(0, 0), new(40, 0)),
    SketchCurve2d.Segment(new(40, 0), new(40, 30)),
    SketchCurve2d.Segment(new(40, 30), new(0, 30)),
    SketchCurve2d.Segment(new(0, 30), new(0, 0))], requireClosed: true);
SketchCurveChain2d hole = SketchCurveChain2d.Create([
    SketchCurve2d.Circle(new(20, 15), 5)], requireClosed: true);
SketchProfile2d profile = SketchProfile2d.Classify([hole, outer]);

using Shape solid = profile.Extrude(SketchPlane.XY, 8);
SketchProfile2d.WriteStep(solid, "sketch.step",
    new XdePartMetadata("Sketch plate", new XdeColor(0.2, 0.7, 0.35), ["Sketch"]));
```

Sketch definitions own copied data. Curves support evaluation, projection,
intersection, trim/split/reverse, translation, rotation, uniform scale, and mirror.
Bezier and B-spline definitions support rational weights; `Interpolate` creates a
degree-one, piecewise-linear B-spline. Wire/face/feature results are independent
`IDisposable` shapes. The API does not include a parametric constraint solver.

## Work on a curved surface in UV space

```csharp
using OcctSharp;

using Shape cylinder = SurfaceModeling.CreateAnalyticFace(
    AnalyticSurfaceKind.Cylinder, SketchPlane.XY,
    new SurfaceParameterBounds(0, Math.Tau, 0, 20), radius: 8);
SurfaceEvaluationPoint sample = SurfaceModeling.Evaluate(cylinder, new(1, 5));
SurfaceCurveDefinition uv = SurfaceModeling.InterpolateUv(
    [new(0.3, 2), new(1.5, 8), new(3.2, 17)]);
using Shape edge = SurfaceModeling.LiftCurve(cylinder, uv.Curve);
IReadOnlyList<SurfaceCurveSample> samples = SurfaceModeling.SampleCurve(cylinder, edge, 50);
using Shape section = SurfaceModeling.IntersectPlane(cylinder,
    new SketchPlane(new(0, 0, 10), new(1, 0, 0), new(0, 1, 0)),
    new SurfaceParameterBounds(-10, 10, -10, 10));
```

`SurfaceModeling` also supports hole-aware UV classification, full/batch projections,
seam branches and tracing, UV offsets, boundary measurements, trimmed faces, copied
topology repair, and face splitting with diagnostics. Points and derivatives are in
world coordinates; UV values are surface-local. UV offset distance is not world-space
distance. Definitions and reports are copied; returned shapes and repair/split result
containers must be disposed. See the [32-capability Batch P scope](docs/BATCH_P_SURFACE_UV_CURVE_GAP_INVENTORY.md)
and [Preview.15 notes](docs/RELEASE_NOTES_8.0.1_PREVIEW_15.md).

## Read ordinary STEP geometry

Use the geometry-only API when product structure and presentation metadata are not
needed:

```csharp
using OcctSharp;

using Shape shape = ShapeExchange.ReadStep("part.step");
Console.WriteLine($"Imported faces: {shape.FaceCount}");
```

## Read STEP/XDE colors and subshape styles

Use `XdeDocument` for assemblies, names, layers, colors, materials, visibility, and
location-aware face/subshape presentation styles:

```csharp
using OcctSharp;

using XdeDocument document = XdeDocument.ReadStep("assembly.step");

foreach (XdeLabel root in document.GetFreeShapes())
{
    IReadOnlyList<XdePresentationStyle> styles = root.GetPresentationStyles();
    try
    {
        foreach (XdePresentationStyle style in styles)
        {
            XdeColor? color = style.EffectiveColor;
            Console.WriteLine(
                $"faces={style.Shape.FaceCount}, visible={style.IsVisible}, " +
                $"rgba={color?.Red:F3},{color?.Green:F3},{color?.Blue:F3},{color?.Alpha:F3}");
        }
    }
    finally
    {
        foreach (XdePresentationStyle style in styles) style.Dispose();
    }
}
```

`OcctViewer.Display(XdeLabel)` applies the same inherited occurrence, part, face, edge,
material, alpha, and visibility styles to one `AIS_ColoredShape` presentation. Viewer
objects are UI-thread-affine and require a native child-window handle.

## Read or write metadata-aware IGES

Use the XDE-centered exchange API for IGES names, colors, layers, visibility, diagnostics,
and Unicode Windows paths. The format-neutral methods route STEP or IGES by extension:

```csharp
using OcctSharp;

using XdeDocument document = XdeDocument.ReadIges(
    @"C:\CAD\彩色装配.iges",
    new XdeIgesReadOptions(ReadNames: true, ReadColors: true, ReadLayers: true),
    out XdeIgesReadReport report);

Console.WriteLine($"Transferred {report.TransferredRootCount} IGES roots");
document.WriteExchange(@"C:\CAD\导出副本.igs");

using XdeDocument routed = XdeDocument.ReadExchange("assembly.step");
```

Imported labels remain bound to their owning `XdeDocument`; copied diagnostics and
independently owned topology do not retain the native IGES transfer session.

## Samples

The repository contains two runnable .NET 10 sample projects. Both use the committed,
manifest-verified Windows x64 runtime, so a normal clone does not need a separate OCCT
SDK or native build toolchain.

| Sample | Description | Detailed guide |
|---|---|---|
| `OcctSharp.Samples` | Interactive console menu covering solid creation, STEP/STL/IGES export, transformed XDE STEP assemblies, a native viewer window, and a complete BREP/topology/mesh/XDE workflow. It also provides the non-interactive `--smoke` clone/runtime check. | [Console sample README](https://github.com/yoi102/OcctSharp/blob/main/OcctSharp/samples/OcctSharp.Samples/README.md) |
| `OcctSharpViewer.Wpf` | `CommunityToolkit.Mvvm` WPF viewer for STEP/STP and IGES/IGS with XDE presentation colors, standard views, shaded/wireframe display, selection, rotation, pan, and zoom. | [WPF viewer README](https://github.com/yoi102/OcctSharp/blob/main/OcctSharp/samples/OcctSharpViewer.Wpf/README.md) |

Run the console sample from the inner workspace:

```powershell
cd OcctSharp
dotnet run --project .\samples\OcctSharp.Samples --configuration Release
```

Run the WPF/MVVM viewer:

```powershell
dotnet run --project .\samples\OcctSharpViewer.Wpf --configuration Release
```

The WPF viewer hosts OCCT's OpenGL output with `HwndHost`. WPF controls can be placed
around the viewport, but WPF airspace rules prevent reliable overlays above it; a
`D3DImage` bridge is not included. See the project README for architecture, mouse
controls, STEP color behavior, and troubleshooting.

## Clone and run without an OCCT SDK

On Windows x64 with .NET SDK 10.0.400:

```powershell
git clone https://github.com/yoi102/OcctSharp.git
cd OcctSharp\OcctSharp
dotnet run --project .\samples\OcctSharp.Samples -- --smoke
```

The committed, SHA256-pinned Release runtime is copied automatically. Building the
native bridge or regenerating bindings still requires the documented MSVC/OCCT contributor
toolchain.

## License and third-party runtime terms

The OcctSharp project code, generated bindings, and repository-built
`OcctSharp.Native.dll` are licensed under the [MIT License](https://github.com/yoi102/OcctSharp/blob/main/LICENSE).
The NuGet packages therefore declare `MIT` for the OcctSharp-owned package content.

That MIT declaration does **not** relicense OCCT or the other native libraries bundled in
`OcctSharp.Native.win-x64`. In particular, the Open CASCADE Technology 8.0.1 DLLs are
distributed under `LGPL-2.1-only` with the Open CASCADE exception. oneTBB, FreeImage,
FreeType, OpenVR, FFmpeg, and jemalloc retain their respective upstream terms.

Before redistributing the native package or an application that ships its `occt/`
directory, review the complete
[third-party runtime notice](https://github.com/yoi102/OcctSharp/blob/main/OcctSharp/runtime/win-x64/THIRD_PARTY_NOTICES.md)
and the accompanying
[license texts](https://github.com/yoi102/OcctSharp/tree/main/OcctSharp/runtime/win-x64/licenses).
Those files are included in `OcctSharp.Native.win-x64` under its `licenses/` directory.

## Project structure and status

- `docs/` contains architecture, decisions, compatibility, packaging, samples, and status.
- `OcctSharp/` contains the solution, source, tests, generator, runtime, samples, and
  release tooling.
- Project code is MIT licensed; bundled native components remain governed by the terms
  summarized above.

See [documentation](docs/DOCUMENTATION_INDEX.md), [current status](docs/STATUS.md),
[samples](docs/SAMPLES.md), [NuGet packaging](docs/NUGET_PACKAGING.md), and
[build/release instructions](docs/BUILD_AND_RELEASE.md).

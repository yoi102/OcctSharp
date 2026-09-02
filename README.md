# OcctSharp

OcctSharp is a .NET 10 SDK for Open CASCADE Technology (OCCT) 8.0.1. It combines a
versioned native C ABI, generated low-level bindings, and friendly managed CAD APIs for
modeling, STEP/IGES/STL exchange, XDE assemblies and metadata, meshing, inspection, and
Windows visualization.

Current preview: `8.0.1-preview.12` for Windows x64. The NuGet graph contains 12 managed
modules, the `OcctSharp` compatibility/facade package, and one shared
`OcctSharp.Native.win-x64` runtime package. The native package places the complete
62-DLL runtime in the application's `occt/` directory; no machine-wide OCCT installation
or `PATH` change is required.

## Install

```powershell
dotnet add package OcctSharp --version 8.0.1-preview.12
```

A narrow consumer can reference a module directly, for example:

```powershell
dotnet add package OcctSharp.Modeling --version 8.0.1-preview.12
```

The supported runtime baseline is .NET 10, Windows x64, and OCCT 8.0.1.

## Create a solid

```csharp
using OcctSharp;

using Shape box = ShapeFactory.CreateBox(40, 30, 20);
using Shape cylinder = ShapeFactory.CreateCylinder(6, 20);

Console.WriteLine($"Box faces: {box.FaceCount}");
Console.WriteLine($"Cylinder faces: {cylinder.FaceCount}");
```

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

## WPF MVVM viewer

The `OcctSharpViewer.Wpf` sample uses `CommunityToolkit.Mvvm` and an OCCT OpenGL viewport
hosted by `HwndHost`. It loads STEP/STP and IGES/IGS, preserves STEP/XCAF presentation
colors, fits the model, provides standard views, shaded/wireframe modes, selection,
right-drag rotation, middle-drag pan, and wheel zoom.

From the inner workspace:

```powershell
cd OcctSharp
dotnet run --project .\samples\OcctSharpViewer.Wpf --configuration Release
```

WPF controls can be placed around the viewport. WPF airspace rules still prevent reliable
WPF overlays above the `HwndHost`; a `D3DImage` bridge is not included.

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

## Project structure and status

- `docs/` contains architecture, decisions, compatibility, packaging, samples, and status.
- `OcctSharp/` contains the solution, source, tests, generator, runtime, samples, and
  release tooling.
- Project code is MIT licensed. OCCT and bundled third-party components retain their own
  license terms beside the native runtime.

See [documentation](docs/DOCUMENTATION_INDEX.md), [current status](docs/STATUS.md),
[samples](docs/SAMPLES.md), [NuGet packaging](docs/NUGET_PACKAGING.md), and
[build/release instructions](docs/BUILD_AND_RELEASE.md).

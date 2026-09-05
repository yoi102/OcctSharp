


# OcctSharpViewer.Wpf

`OcctSharpViewer.Wpf` is a .NET 10 WPF/MVVM desktop CAD viewer. It uses
`CommunityToolkit.Mvvm` for commands and observable state and hosts OCCT's OpenGL viewer
inside a native child window.

https://github.com/user-attachments/assets/2f3e5b9b-bc01-4aeb-b36e-64ade2463fe4

## Features

- Open STEP/STP and IGES/IGS files.
- Preserve STEP/XDE assembly, part, face, edge, material, transparency, visibility, and
  disconnected presentation styles supported by the Preview.12 recovery path.
- Fit the complete model and switch between axonometric, front, top, left, and right views.
- Switch all presentations between shaded and wireframe display.
- Detect and select geometry with add, toggle, remove, and clear-selection behavior.
- Rotate, pan, and zoom with the mouse.
- Display current file, operation status, and selection count through MVVM-bound state.
- Capture an independent frozen 360 x 240 review thumbnail beside the live viewport.

## Copied review snapshot

Use **Capture snapshot** after loading a model. `CaptureSnapshotCommand` calls the
viewer service on the creating UI thread, copies `ViewerColorFrame` pixels and adapts
them to a frozen WPF `WriteableBitmap` in `ViewerSnapshotBitmap`. The thumbnail remains
usable when the live camera changes or the viewer closes. It uses opaque Bgr32 rather
than misinterpreting the renderer's composite alpha as a straight-alpha WPF image.

This is an opt-in still image, not a live D3DImage bridge. The interactive viewport
still uses HwndHost and retains its airspace restriction; put ordinary WPF controls
beside it or over the copied thumbnail, not over the native child window.

For an automated real-WPF snapshot smoke check after building:

```powershell
.\samples\OcctSharpViewer.Wpf\bin\Release\net10.0-windows\win-x64\OcctSharpViewer.Wpf.exe --snapshot-smoke C:\Temp\occt-review.png
```

The diagnostic uses the same MVVM command and bitmap adapter, checks frozen dimensions,
and writes the requested PNG. It still requires a Windows desktop/OpenGL context.

## Requirements

- Windows x64 with desktop/OpenGL support.
- .NET SDK 10.0.400, selected by the repository `global.json`.
- A clone of this repository.

The committed, SHA256-verified 62-DLL runtime is copied automatically. No separate OCCT
SDK, native compiler, machine-wide installation, or `PATH` change is needed to run the
sample.

## Run

From the inner `OcctSharp/` workspace:

```powershell
dotnet run --project .\samples\OcctSharpViewer.Wpf --configuration Release
```

When the window opens, select **Open STEP / IGES** and choose one supported model file.
Loading can take noticeable time for large STEP assemblies because OCCT transfers the
product tree, geometry, locations, and presentation styles before display.

## Mouse controls

| Input | Action |
|---|---|
| Right drag | Rotate |
| Middle drag | Pan |
| Mouse wheel | Zoom |
| Left click | Replace selection |
| Shift + left click | Add to selection |
| Ctrl + left click | Toggle selection |
| Alt + left click | Remove from selection |

The toolbar also provides Fit All, standard projections, Shaded, Wireframe, and Clear
selection commands.

## STEP colors and styles

STEP files are loaded through `XdeDocument.ReadStep`, not the geometry-only reader. Each
free XDE root is displayed through `OcctViewer.Display(XdeLabel)`, which creates one
viewer-owned colored presentation and applies OCCT's inherited occurrence, part, face,
edge, material-base, alpha, and visibility overrides.

Preview.12 also recovers styled AP242 targets that OCCT 8.0.1's normal product transfer
can omit, including disconnected visible presentation geometry and representation
transforms. The neutral blue-gray color is only a fallback for topology without an XCAF
presentation style.

IGES files use `XdeDocument.ReadIges`/`IGESCAFControl` through the same XDE-label display
path. Supported IGES names, generic/surface/curve colors, layers, and visibility are
therefore projected by OCCT instead of forcing the neutral fallback over the whole model.
STEP and IGES can also be routed uniformly with `XdeDocument.ReadExchange`.

## MVVM and viewer structure

- `MainWindow.xaml` defines the toolbar, status surface, file summary, and viewport layout.
- `MainWindowViewModel` owns `CommunityToolkit.Mvvm` commands and application state.
- `IFileDialogService`/`FileDialogService` isolate the native file dialog from the view model.
- `IViewerService` is the view-model-facing viewer contract.
- `OcctViewportHost` implements that contract, owns the child HWND and `OcctViewer`,
  displays imported presentations, and forwards Win32 pointer/resize/paint messages.
- `MainWindow.xaml.cs` only connects viewer lifecycle/events to the view model.

OCCT viewer objects and presentations are creating-thread-affine. `OcctViewportHost`
disposes the existing presentation set before replacing it and disposes the viewer before
destroying its child window.

## Why the viewport uses HwndHost

OCCT 8.0.1 renders through OpenGL to a native window, so the sample uses `HwndHost` as
the smallest reliable WPF integration. Normal WPF controls can be placed around the
viewport, as this sample does. WPF airspace rules prevent reliable WPF overlays directly
above the native viewport.

A `D3DImage` implementation is not included because it would require an additional
OpenGL-to-D3D9Ex shared-surface bridge plus explicit synchronization, resize, device-loss,
and recovery behavior. Applications that require WPF overlays should account for that
larger rendering-integration project rather than treating it as a control replacement.

## Troubleshooting

- **The model is uniformly blue-gray:** confirm it is STEP/STP if metadata colors are
  expected. IGES uses the geometry-only path. Some STEP topology legitimately has no style.
- **The model is not visible after loading:** use **Fit All** and switch to Axonometric.
- **Native runtime load failure:** confirm the executable output contains exactly 62 DLLs
  below its `occt/` directory and run from a normal repository build.
- **WPF overlay is hidden above the model:** this is the expected `HwndHost` airspace rule.

See the [repository README](../../../README.md), the
[console samples](../OcctSharp.Samples/README.md), and the
[complete samples guide](../../../docs/SAMPLES.md).

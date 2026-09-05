# OcctSharp.Samples

`OcctSharp.Samples` is the interactive console sample project for common OcctSharp CAD
workflows. It demonstrates solid creation, STEP/STL/IGES export, XDE assembly import and
composition, the native OCCT viewer, and an end-to-end BREP/topology/mesh/XDE workflow.

## Requirements

- Windows x64.
- .NET SDK 10.0.400, selected by the repository `global.json`.
- A clone of this repository.

The default build uses the committed, SHA256-verified 62-DLL runtime. An OCCT SDK, C++
compiler, machine-wide OCCT installation, and `PATH` changes are not required to run the
sample.

## Run

Run commands from the inner `OcctSharp/` workspace:

```powershell
dotnet run --project .\samples\OcctSharp.Samples --configuration Release
```

The application displays this menu:

| Choice | Workflow | Main APIs |
|---|---|---|
| 1 | Create a `40 x 30 x 20` box and report its faces | `ShapeFactory`, `Shape` |
| 2 | Create a box and export ordinary STEP geometry | `ShapeExchange.WriteStep` |
| 3 | Create a box, mesh it, and export binary STL | `ShapeExchange.WriteStl` |
| 4 | Create a box and export BRep-mode IGES | `ShapeExchange.WriteIges` |
| 5 | Import, transform, and compose STEP files into an XDE assembly | `XdeDocument`, `ImportStep`, `AddComponent`, `WriteStep` |
| 6 | Open a native interactive OCCT viewer window | `OcctViewer`, `ViewerPresentation` |
| 7 | Run the common BREP/topology/mesh/XDE workflow | Modeling, topology, mesh, validation, BREP, XDE, STEP |

Enter `0` or send end-of-input to exit.

## Automated smoke check

Use `--smoke` for a non-interactive clone-and-runtime check:

```powershell
dotnet run --project .\samples\OcctSharp.Samples --configuration Release -- --smoke
```

The smoke path verifies the exact 62-DLL output closure, native ABI 1.60, bridge 0.68.0,
OCCT 8.0.1, box topology, validity, and a non-empty detailed mesh. It returns a non-zero
exit code if any expectation fails.

## Output files

Press Enter at a simple export path prompt to use these defaults:

| Workflow | Default output |
|---|---|
| STEP export | `artifacts/samples/box.step` |
| STL export | `artifacts/samples/box.stl` |
| IGES export | `artifacts/samples/box.iges` |
| XDE assembly | `artifacts/samples/assembled.step` |
| Common API BREP | `artifacts/samples/common-api-part.brep` |
| Common API XDE STEP | `artifacts/samples/common-api-assembly.step` |

Paths are resolved from the current working directory. Files below `artifacts/` are
reproducible local outputs and are not committed.

## XDE STEP assembly input

Choice 5 first asks for the output path and then for STEP inputs. You can either:

- press Enter at the file-count prompt to use all `.step` and `.stp` files in the
  repository-root `data/` directory; or
- enter a positive count and then provide one input path per prompt.

The sample imports each source root into one owned XDE document, applies deterministic
translation and Z-axis rotation, creates component occurrences, commits the transaction,
and writes STEPCAF. This is assembly composition, not Boolean fuse. Where supported by
OCCT 8.0.1, the workflow preserves part/assembly structure, names, colors, styles,
layers, properties, and physical materials. Input and output metadata counts are printed
so a metadata-free source is not mistaken for successful metadata preservation.

The repository `data/` directory is intentionally ignored until fixture provenance and
licensing policy PD-010 is resolved.

## Native viewer controls

Choice 6 opens a Win32 window owned by the sample and backed by `OcctViewer`.

- Move the pointer over the box to detect/highlight it.
- Left-click to select it.
- Resize or expose the window to exercise resize/redraw forwarding.
- Close the viewer window to return to the console menu.

Viewer creation and all viewer calls remain on the creating UI thread.

## Project map

- `Program.cs` owns menu dispatch and the non-interactive smoke entry point.
- `*Sample.cs` files keep each workflow independent and readable.
- `SampleConsole.cs` owns prompts and menu text.
- `SamplePaths.cs` owns default output paths and repository `data/` discovery.
- `NativeViewerWindow.cs` owns the Win32 window and message loop.
- `StepMetadataSummary.cs` provides copied input/output STEP metadata diagnostics.

See the [repository README](../../../README.md), the
[WPF viewer sample](../OcctSharpViewer.Wpf/README.md), and the
[complete samples guide](../../../docs/SAMPLES.md).

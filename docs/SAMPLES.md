# WPF Viewer Sample

`OcctSharp/samples/OcctSharpViewer.Wpf` is a .NET 10 WPF/MVVM desktop viewer using
`CommunityToolkit.Mvvm`. Run it from the inner workspace with:

```powershell
dotnet run --project .\samples\OcctSharpViewer.Wpf --configuration Release
```

The sample opens STEP/STP and IGES/IGS files, fits the model, provides axonometric and
standard orthographic views, switches between shaded and wireframe display, and supports
selection. Right-drag rotates, middle-drag pans, the mouse wheel zooms, and left-click
selects; Shift, Ctrl, and Alt apply add, toggle, and remove selection semantics.

STEP files use the XDE/STEPCAF reader instead of the geometry-only reader. Each free XDE
root is displayed through `OcctViewer.Display(XdeLabel)`, which applies OCCT's inherited
occurrence, part, face, edge, material-base, alpha, and visibility styles to one colored
presentation. Preview.12 also recovers styled representation targets that OCCT 8.0.1's
ordinary product transfer can omit, including disconnected presentation geometry. Only
topology without an XCAF style uses the sample's neutral fallback color. The current IGES
path is geometry-only and therefore still uses the fallback color.

OCCT renders through OpenGL into a native child window hosted by `HwndHost`. WPF controls
can be arranged around the viewport, but WPF airspace rules prevent reliable WPF overlays
above it. A `D3DImage` path was not selected because it would require an additional
OpenGL-to-D3D9Ex shared-surface, synchronization, resize, and device-recovery bridge.
The committed 62-DLL Windows x64 runtime is copied automatically; no local OCCT SDK is
needed for the default sample build.

# Console Samples

The .NET 10 project `OcctSharp/samples/OcctSharp.Samples` is the single interactive
console entry point for current examples. It shows a menu and reads choices and paths
with `Console.ReadLine`; each workflow has its own English-named class. `--smoke` is the
non-interactive clone/runtime verification entry point.

Run this command from the inner `OcctSharp/` workspace:

```powershell
dotnet run --project .\samples\OcctSharp.Samples --configuration Release
```

For an automated first run:

```powershell
dotnet run --project .\samples\OcctSharp.Samples --configuration Release -- --smoke
```

All Sample source, prompts, diagnostics, and output text are English. On a fresh clone,
the Sample build verifies and copies the committed 62-DLL Windows x64 runtime below its
output `occt/` directory. No OCCT SDK or native toolchain is required. Native contributors
can explicitly set `OcctSharpUseBundledNativeRuntime=false` and use ADR-0051's pinned SDK
bootstrap; use `eng/build.ps1` for regeneration and the full test pipeline.

Choose an item from the menu. The first item creates a `40 x 30 x 20` box. The next
three items create a box and write STEP, binary STL, or BRep-mode IGES. For each export,
press Enter at the path prompt to use `artifacts/samples/box.step`, `box.stl`, or
`box.iges`, or enter another output path.

The assembly item first asks for an output path, then asks for the number of STEP input
files. Press Enter at the count prompt to use all STEP files in the repository-root
`data/` directory, or enter a count and provide one path per prompt. It demonstrates the
composable `XdeDocument` API: create a caller-named assembly, import each STEP file with
`ImportStep`, place every imported root with `AddComponent` and `TopLocLocation`, commit,
then call `WriteStep`. Import copies the source XDE label tree and supported metadata; the
Sample applies deterministic translation and Z-axis rotation. The `data/` directory is
ignored until fixture provenance and licensing policy PD-010 is resolved.

The command does not perform Boolean fuse. It preserves colors, styles, names, layers,
properties, physical materials, and the input part/assembly structure where OCCT 8.0.1
STEPCAF supports those entities. It prints input/output counts for color, style, material,
product-definition, and assembly-usage entities so that a metadata-free fixture is not
mistaken for a preservation result.

`StepAssembly.WriteXde` remains only as an obsolete compatibility convenience. New code
should use the document operations shown by the Sample so applications can choose their
own hierarchy, mix imported and in-memory parts, edit metadata, persist BinXCAF, or export
STEP later. The other console examples already call general `ShapeFactory`,
`ShapeExchange`, `ShapeAssembly`, and `OcctViewer` APIs and contain no Sample-only wrapper.

The sixth item opens a native Win32 window backed by `OcctViewer`, displays a box, and
runs a standard message loop. Resizing forwards `WM_SIZE`, painting requests redraw,
mouse movement updates detection, and a left click selects at the client coordinate and
updates the title with the copied selection count. Close the viewer window to return to
the console menu. The sample keeps window ownership outside the viewer and performs all
viewer calls on the creating UI thread.

The seventh item runs the first Batch C common-API workflow without a window. It creates
and chamfers a solid, copies unique/occurrence topology counts and tolerance ranges,
extracts an orientation-correct detailed mesh with normals/UVs/face mapping, writes and
reads native BREP, adds name/color/layers/material with `XdeDocument.AddPart`, and writes
an XDE STEP assembly. Its default outputs are `common-api-part.brep` and
`common-api-assembly.step` under `artifacts/samples/`.

The validated local run on 2026-08-21 consumed seven STEP files from `data/` and wrote
one assembly STEP. The inputs contained 73 `COLOUR_RGB`, 106 `STYLED_ITEM`, 73
`PRESENTATION_STYLE_ASSIGNMENT`, 4 material-property, and 8 product-definition records.
The output contained 8 deduplicated `COLOUR_RGB`, 100 `STYLED_ITEM`, 101
`PRESENTATION_STYLE_ASSIGNMENT`, 4 material-property, 9 product-definition, and 7
`NEXT_ASSEMBLY_USAGE_OCCURRENCE` records. Outputs under `artifacts/` are reproducible
local evidence and are not committed.

# Console Samples

The .NET 10 project `OcctSharp/samples/OcctSharp.Samples` is the single interactive
console entry point for current examples. It shows a menu and reads choices and paths
with `Console.ReadLine`; each workflow has its own English-named class. There are no
command-line subcommands or separate sample README files.

Run this command from the inner `OcctSharp/` workspace:

```powershell
dotnet run --project .\samples\OcctSharp.Samples --configuration Release
```

All Sample source, prompts, diagnostics, and output text are English. On a fresh clone,
the Sample build automatically invokes the native-only bootstrap when the requested
native runtime is absent or stale, then copies all DLLs below its output `occt/`
directory. Configure one of these dependency inputs before the first run:

- Set `OCCTSHARP_OCCT_ROOT` to the pinned OCCT 8.0.1 SDK root; or
- copy `config/local.settings.example.json` to ignored `config/local.settings.json` and
  set `occtRoot`; or
- set `OCCTSHARP_OCCT_ARTIFACT_URL` and
  `OCCTSHARP_OCCT_ARTIFACT_SHA256` to an approved immutable HTTPS archive.

The bootstrap runs CMake only and does not recursively build the managed solution or
run the complete test pipeline. Use `eng/build.ps1` when regeneration and full tests are
required.

Choose an item from the menu. The first item creates a `40 x 30 x 20` box. The next
three items create a box and write STEP, binary STL, or BRep-mode IGES. For each export,
press Enter at the path prompt to use `artifacts/samples/box.step`, `box.stl`, or
`box.iges`, or enter another output path.

The assembly item first asks for an output path, then asks for the number of STEP input
files. Press Enter at the count prompt to use all STEP files in the repository-root
`data/` directory, or enter a count and provide one path per prompt. It reads each input
through STEPCAF/XDE, copies its XDE label tree and supported metadata, applies
deterministic translation and Z-axis rotation as an assembly-component location, and
writes one STEP file with an `OcctSharp Assembly` root. The `data/` directory is ignored
until fixture provenance and licensing policy PD-010 is resolved.

The command does not perform Boolean fuse. It preserves colors, styles, names, layers,
properties, physical materials, and the input part/assembly structure where OCCT 8.0.1
STEPCAF supports those entities. It prints input/output counts for color, style, material,
product-definition, and assembly-usage entities so that a metadata-free fixture is not
mistaken for a preservation result.

The sixth item opens a native Win32 window backed by `OcctViewer`, displays a box, and
runs a standard message loop. Resizing forwards `WM_SIZE`, painting requests redraw,
mouse movement updates detection, and a left click selects at the client coordinate and
updates the title with the copied selection count. Close the viewer window to return to
the console menu. The sample keeps window ownership outside the viewer and performs all
viewer calls on the creating UI thread.

The validated local run on 2026-08-21 consumed seven STEP files from `data/` and wrote
one assembly STEP. The inputs contained 73 `COLOUR_RGB`, 106 `STYLED_ITEM`, 73
`PRESENTATION_STYLE_ASSIGNMENT`, 4 material-property, and 8 product-definition records.
The output contained 8 deduplicated `COLOUR_RGB`, 100 `STYLED_ITEM`, 101
`PRESENTATION_STYLE_ASSIGNMENT`, 4 material-property, 9 product-definition, and 7
`NEXT_ASSEMBLY_USAGE_OCCURRENCE` records. Outputs under `artifacts/` are reproducible
local evidence and are not committed.

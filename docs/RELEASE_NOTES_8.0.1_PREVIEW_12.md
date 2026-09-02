# OcctSharp 8.0.1-preview.12 release notes

## Summary

Preview.12 fixes incomplete STEP presentation colors in XDE and the WPF viewer. It keeps
the Preview.10 managed module graph and Preview.11 product APIs, advances the additive
native ABI to 1.56 and bridge to 0.64.0, and retains OCCT 8.0.1, schema 1.13, managed
assembly identity `0.1.0.0`, and one shared 62-DLL Windows x64 runtime package.

## Highlights

- STEPCAF import now pre-transfers styled AP242 targets omitted by ordinary product
  transfer, resolves representation transforms, installs recovered surface/curve/
  rendering colors and visibility in XDE, and retains disconnected visible presentation
  geometry instead of silently dropping it.
- `XdeLabel.GetPresentationStyles()` returns location-aware copied style snapshots with
  independently owned topology and surface, curve, material, alpha, and visibility data.
- `OcctViewer.Display(XdeLabel)` applies inherited XCAF occurrence, part, face, edge,
  material, transparency, and visibility overrides in one viewer-owned presentation.
- `OcctSharpViewer.Wpf` displays whole XDE roots through the new presentation path. The
  existing MVVM commands, STEP/IGES loading, fit, standard views, shaded/wireframe modes,
  selection, rotate, pan, and zoom remain intact.
- Every package includes a new 256-pixel transparent OcctSharp icon and a rewritten README
  with install, modeling, STEP, XDE style, and WPF viewer examples.

## Real-file evidence

- `ArduinoUnoRev3PCB.step`: 241 free roots, 244 effective styles, and 17 distinct colors.
  Visual inspection showed green board, dark chips, metallic pins, white/gray connectors,
  and gold/orange components instead of uniform RGB `(216, 224, 236)`.
- `fullArduinoUnoRev3PCB.step`: 247 free roots, 763 effective styles, and 19 distinct
  colors. Processing this larger file takes several minutes on the release machine.

These downloaded files are validation inputs only and are not distributed in the
repository or NuGet packages.

## Compatibility and ownership

The change is additive. The compatibility/facade package and all 12 module package IDs
remain unchanged. `XdePresentationStyle` owns its `Shape`; dispose every returned style.
The copied topology remains valid after source-document disposal. Viewer presentations
remain bound to their `OcctViewer` and its creating thread.

## Package set

The release contains these 14 packages at one exact version:

- `OcctSharp.Native.win-x64`
- `OcctSharp.Runtime`
- `OcctSharp.Foundation`
- `OcctSharp.Geometry`
- `OcctSharp.MeshData`
- `OcctSharp.Modeling`
- `OcctSharp.Mesh`
- `OcctSharp.Documents`
- `OcctSharp.Visualization`
- `OcctSharp.DataExchange`
- `OcctSharp.Xde`
- `OcctSharp.IVtk`
- `OcctSharp.Draw`
- `OcctSharp`

Only `OcctSharp.Native.win-x64` contains native DLLs. Managed packages receive the native
runtime transitively through `OcctSharp.Runtime`.

## Validation

The complete local release check passes:

- Release and Debug build all 19 projects with zero warnings/errors; Generator 91/91,
  Runtime 152/152, and dependency profiles 6/6 pass in both configurations.
- All 94 generated files are current and byte-identical after clean-source regeneration.
- API comparison against the alpha.38 baseline is additive at 38,791 additions and zero
  removals. Full inventory classifies all 116,272 declarations and 7,090 headers with
  zero supported-unselected, pending, or HD099 entries.
- All 14 packages pass README/icon/nuspec/content inspection. The 13 managed packages
  each contain one managed assembly and zero native DLLs; the native package alone owns
  all 62 DLLs and 11 notice/license files.
- The clean facade consumer executes the inherited Batch D-M workflow with ABI 1.56,
  bridge 0.64.0, and OCCT 8.0.1. The direct Modeling consumer creates a six-face solid
  without receiving the compatibility `OcctSharp.dll` facade.
- The committed 15,348,736-byte `OcctSharp.Native.dll` is byte-identical to the Release
  rebuild and has SHA256
  `56E0854F062D672552792C11DF2CE760EEF0EB1687F270C3E6671B15B96A13AC`.
- SBOM, provenance, release-gate metadata, checksums, and Git whitespace checks pass.

### Package hashes

| Package | SHA256 |
|---|---|
| `OcctSharp.Native.win-x64` | `7AD3146DEEE10EF0890978246F12AA73C4DBF6FC58C2F764F8A112B1E259C19A` |
| `OcctSharp.Runtime` | `D8C9646AD8587730262C9C1A568E89555160098735F910C414D5A0E109EAE2A2` |
| `OcctSharp.Foundation` | `BCEF243CA5656B04B8422C181A0B2E5DAEF300A9B80C9C4B27D92574A667E2FE` |
| `OcctSharp.Geometry` | `F9035A77075A45F13FAA436BE7529CFC79BDD469FF9C7F5FC37E36F0B399A89B` |
| `OcctSharp.MeshData` | `4F601D739B4166A4B40A95A3A104A96AA245604F6A13EEC21F96D356EB1C3387` |
| `OcctSharp.Modeling` | `D7230D191FD420B474C2295421FC845EE0E23775CBECD3C71DB2659FED0462C4` |
| `OcctSharp.Mesh` | `D9DDE25E8DAF9953C3A59E98B181AE3A2E0AD57F643976458857BD31CE6032F5` |
| `OcctSharp.Documents` | `27EDC81ACE7EF584422838A0B73335CEB96F83012CBF1B63E8E976F614FAB9E7` |
| `OcctSharp.Visualization` | `FB8C1449D9CA0F8AE85AB860781FEF2983932380166D066A69E825009E3761B3` |
| `OcctSharp.DataExchange` | `2D3347F0C7B7EF626BBDF9627017DAD571FA7876B7097F5B5745EEE30318B796` |
| `OcctSharp.Xde` | `A26F0C090673F38C3A103FDD38E01CF5BA0099F7378BAFC8A48F708ACC3F7D0E` |
| `OcctSharp.IVtk` | `91F4B57364D112511D74E414E1EECAAB723DDCFC8373390C4CE8EAFFABABCA04` |
| `OcctSharp.Draw` | `2689EEA84C668EEDD49B56972BDCC361C24745B5F871F8E966258F4697AF115A` |
| `OcctSharp` | `246584492CF1A5116C68E4D131C76225509C374320DA2915FB2C01355BCF3E6D` |

The final `OcctSharp.Native.win-x64` package was uploaded to NuGet.org and passed package
validation. Its flat-container endpoint still returns 404 while indexing is pending. The
other 13 packages and the clean public-source consumer are `NOT RUN` by explicit user
direction. Hosted full release execution and package signing also remain `NOT RUN`; these
are not presented as local build or package failures.

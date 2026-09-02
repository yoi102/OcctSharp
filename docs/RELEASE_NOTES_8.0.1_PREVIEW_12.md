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
| `OcctSharp.Native.win-x64` | `58EFBC0C306A04C401C485FF6C42EE04539167E9D0A2B3B13FD5A5BB8A49765C` |
| `OcctSharp.Runtime` | `BA6E729F289E8E68DD6630EC97979BB55D86DEEB22EFBDDD5698D4AE22247476` |
| `OcctSharp.Foundation` | `692CE66432083662C6C81E99E8F80EE12474C461545119C6BF741661FEB600B8` |
| `OcctSharp.Geometry` | `582451241EDC9A0B3D179C2259AD035BE6143907AA2BE644D66D90FC90B78B77` |
| `OcctSharp.MeshData` | `6A95017EE10CE36A0DB5A1D69158ED308045DBA013174A957FA1D0107714421E` |
| `OcctSharp.Modeling` | `392A4D99C4356392DBB6323385A4C6000AF2536532A1E71854E1DCAB1E3815DA` |
| `OcctSharp.Mesh` | `B48DFF64028437319878D4BE0DE1CD3A56EB5328FC300ADA3C09E10B83FB2EB9` |
| `OcctSharp.Documents` | `C63B1B7D8918D02736DB64CA1F025DBE075110E8EB5E76A1BEFD526D3736BD68` |
| `OcctSharp.Visualization` | `723CF52C11EAF231B869068976FF51C7B74F09905F887CAED69F7F6A3EB242C6` |
| `OcctSharp.DataExchange` | `BFCBAB6C726A187BB9ED7D2C3DFB3198DC2F403D93A3434D30E18CE133E8D17E` |
| `OcctSharp.Xde` | `45BB8538DDDA8DDBA966F5117D0D1765F92A1B7BE0614438ECF36398D1B04355` |
| `OcctSharp.IVtk` | `E417002D5A55E9F8040F7C405422D576377A16A08276C09B6B8CA9F728D4DA0C` |
| `OcctSharp.Draw` | `A4A7E719335C92D8DEF40785529972965F50EB7435BDFC4B518D737BC2D5DBA5` |
| `OcctSharp` | `60CE880F30283F3FF0EA7CA2E7140A1E940E1A6FA64458001EA7B8F010BCE1E0` |

NuGet upload/indexing and the clean public-source consumer are `NOT RUN`. The public
flat-container endpoints currently return 404 for all 14 package IDs. Hosted full release
execution and package signing also remain `NOT RUN`; these are not presented as local
build or package failures.

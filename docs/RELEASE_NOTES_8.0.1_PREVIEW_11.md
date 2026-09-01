# OcctSharp 8.0.1-preview.11

## Batch M interactive placement editing

- Adds presentation-local transform get, set, and reset.
- Adds a viewer-parent-bound manipulator with translation, rotation, scaling, plane,
  axis-part, activation, position, size, gap, skin, and zoom-persistence controls.
- Adds copied manipulator state plus custom and real-view mouse preview, apply, cancel,
  detach, and deterministic thread/parent/lifetime guards.
- Adds `XdePlacementEditSession` for reversible viewer preview and named transactional
  rigid occurrence relocation with replacement-label identity.
- Composes placement commits with existing undo/redo, DMU interference recheck,
  STEP/XDE round-trip, and real-HWND screenshot workflows.

## Architecture and compatibility

- Retains the Preview.10 Runtime/Foundation/Geometry/MeshData/Modeling/Mesh/Documents/
  Visualization/DataExchange/Xde/IVtk/Draw assembly graph and `OcctSharp` facade.
- Retains one `OcctSharp.Native.dll` and the shared 62-DLL Windows x64 native package.
- Advances native ABI to 1.55, bridge implementation to 0.63.0, and binding schema to
  1.13 while retaining OCCT 8.0.1 and managed assembly/file identity `0.1.0.0`.
- SC-049 reconciles exactly eight directly used blocked declarations; it does not
  bulk-reclassify the 1,650-candidate Batch M audit.

## Validation

- Focused Batch M 4/4 covers all 24 locked capabilities, including replacement
  occurrence identity, named history, undo/redo, DMU, STEP/XDE, real HWND, screenshot,
  and lifetime/thread failure paths.
- Complete Release/Debug, package, clean-consumer, regeneration, inventory,
  compatibility, runtime identity, SBOM, provenance, and checksum results are recorded
  in `STATUS.md` and the generated release evidence.

## Publication boundary

The package set is locally validated but not published. Hosted release execution,
package signing, NuGet publication, GitHub, and push remain `NOT RUN`.

# ADR-0043: Native-local mesh-format providers

- Status: Accepted
- Date: 2026-08-23
- Scope: B14 mesh-format exchange profile on OCCT 8.0.1, Windows x64

## Decision

Expose one-shot geometry exchange for OBJ, glTF/GLB, and VRML read/write plus PLY
write. Each operation constructs an explicit format configuration node and matching
`DEOBJ_Provider`, `DEGLTF_Provider`, `DEVRML_Provider`, or `DEPLY_Provider` inside the
native call. Provider, document, scene, progress, and configuration state never cross
the C ABI. Writes mesh the input shape first and reads return one registered owning
shape that has no dependency on the provider lifetime.

PLY import is reported as `UNSUPPORTED` for this profile because OCCT 8.0.1's
`DEPLY_Provider` does not implement reading. No placeholder API is emitted for it.
Format options, XDE metadata projection, and provider configuration objects remain
outside this geometry-only profile.

## Validation

Release and Debug builds pass 32 generator and 65 runtime tests. The runtime test writes
non-empty OBJ, PLY, GLB, and VRML files, reads OBJ, GLB, and VRML back into non-empty
topology, and verifies source/result independence. Generated freshness verifies 12
manifest-owned files. The `0.1.0-alpha.35` clean consumer restores, publishes, loads
ABI 1.27/bridge 0.35.0, executes the same format family, and verifies 41 DLLs below the
application-local `occt` directory.

## Upgrade impact

Re-check provider configuration-node construction, supported read/write directions,
unit conversion warnings, filename-extension selection, meshing defaults, null-result
behavior, and the TKDE/TKRWMesh dependency closure for every OCCT upgrade.

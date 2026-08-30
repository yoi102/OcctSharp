# ADR-0069: Implement advanced mesh, scene, material, LOD, and interchange as Batch H

- Status: Accepted for implementation
- Date: 2026-08-30
- Scope: Batch H product denominator, dependency closure, ownership, and validation

## Context

Batch G closes technical drawing, but common realtime, web, visualization, and digital-
twin workflows still need unmanaged OCCT code to retain grouped triangulations, normals,
UVs, materials, assembly instances, multiple detail levels, and document-aware mesh
interchange. The missing workflow crosses BRepMesh, Poly, BRep_Tool, TopLoc, XCAFPrs,
XCAFDoc colors/physical/visual materials, RWMesh, RWGltf, RWObj, RWPly, VRML, and AIS.

## Decision

Open Batch H as one 24-capability product wave named **advanced mesh, scene, material,
LOD, and interchange**. The immutable denominator and 24-root/840-declaration audit are
in `BATCH_H_ADVANCED_MESH_SCENE_GAP_INVENTORY.md`.

Meshing, Poly access, scene exploration, material lookup, and file providers remain
native-local. Managed code receives immutable copied mesh, group, diagnostic, material,
transform, LOD, and scene records. XDE labels keep document-parent ownership; copied
scene snapshots remain usable after document disposal. Document-aware provider methods
own no state beyond one call.

The batch retains one `OcctSharp.dll`, one `OcctSharp.Native.dll`, one NuGet package,
stable public type full names, and the accepted generated shard graph. Implementation
advances the package to Preview.5, native ABI to 1.50, and bridge to 0.58.0.

## Locked non-goals

GPU/native mutable mesh ownership, arbitrary callbacks/shaders, texture image processing,
animation/skinning, Draco, point-cloud editing, IVtk/VTK and other optional profiles,
physical deliverable splitting, hosted release, signing, publication, and GitHub work.

## Consequences

- Preparation freezes all 24 capabilities before implementation starts.
- Mesh extraction, styles/materials, LOD, scene graph, and individual formats are not
  separate completion checkpoints.
- SC-044 records only newly direct blocked declarations actually used by implementation.
- Prior Batch B-G evidence remains immutable.

## Validation required

Focused mesh/group/statistics/diagnostic/LOD/scene/material/interchange/lifetime tests,
real STEP/XDE plus real HWND, the clean-package workflow, Release/Debug, generator/runtime
suites, regeneration, compatibility, inventory, runtime manifest, SBOM/provenance/
checksums, and the complete local release gate must pass together before H is complete.

## Related decisions

- ADR-0046: viewer-parent and creating-thread ownership.
- ADR-0052: native-local algorithms and exact manual stable-ID accounting.
- ADR-0059: committed Windows runtime and MIT licensing.
- ADR-0061/0062: generated layering and cross-shard closure.
- ADR-0065: OCCT-aligned preview numbering.
- ADR-0068: completed technical-drawing boundary.

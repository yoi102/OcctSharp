# Batch H advanced mesh, scene, material, LOD, and interchange gap inventory

This document locks the product denominator and complete cross-family dependency closure
for Batch H before implementation. It measures one mesh-scene interchange workflow, not
individual Poly, RWMesh, RWGltf, RWObj, XCAF, or viewer class counts.

Preparation status: **COMPLETE**. Implementation status: **COMPLETE (24/24)**. The
denominator below remains immutable for Batch H.

## Product outcome

A Windows x64 .NET application can turn BRep or XDE assemblies into copied advanced mesh
snapshots with positions, transformed normals, UVs, face groups, colors, physical/PBR
materials, statistics, topology diagnostics, and multiple LODs; preserve copied scene
hierarchy and instances; exchange glTF/GLB/OBJ/PLY/VRML through document-aware OCCT
providers; and display the result without exposing Poly arrays, RW iterators, XCAF
attributes, or borrowed native mesh objects.

```text
owning BRep or STEP/XDE document with copied style/material values
  -> call-local BRepMesh/Poly and document-aware RW providers
  -> caller-owned grouped meshes, diagnostics, LODs, and scene graph
  -> glTF/GLB/OBJ/PLY/VRML files with XDE hierarchy/material semantics where supported
  -> real STEP/XDE and real-HWND clean-package workflow
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch H exit evidence |
|---:|---|---|---|
| 1 | BRepMesh | Validate finite deflection, angular, minimum-size, relative, parallel, and LOD options | Invalid and valid option fixtures pass |
| 2 | Poly/BRep_Tool | Copy transformed vertex positions | Located shape fixture passes |
| 3 | Poly | Copy or compute transformed unit normals | Normal magnitude/orientation fixtures pass |
| 4 | Poly | Copy optional UV coordinates without inventing availability | UV-present/absent fixtures pass |
| 5 | Poly/TopoDS | Preserve oriented triangle winding and source-face identity | Reversed-face fixture passes |
| 6 | Mesh grouping | Produce immutable per-face primitive groups and index ranges | Group coverage is complete and non-overlapping |
| 7 | XCAF color | Attach copied RGBA style to scene nodes and mesh groups | Generic color round trip passes |
| 8 | XCAF visual material | Set/get copied metallic-roughness PBR values | Base color/metallic/roughness/emissive/IOR/alpha pass |
| 9 | XCAF physical material | Preserve copied physical-material metadata beside visual style | STEP/XDE material fixture passes |
| 10 | Mesh statistics | Compute finite bounds, counts, area, and memory-size estimates | Analytic box fixture passes |
| 11 | Mesh diagnostics | Count degenerate, boundary, manifold, and non-manifold edges | Closed/open mesh fixtures pass |
| 12 | Mesh connectivity | Compute triangle connected-component count | Disjoint compound fixture passes |
| 13 | LOD | Build an ordered multi-deflection LOD set | Fine/coarse monotonic fixture passes |
| 14 | Ownership | Every snapshot/LOD is independent of source shapes and documents | Source disposal tests pass |
| 15 | XDE scene | Copy all free roots into a stable scene-node table | Root coverage fixture passes |
| 16 | XDE scene | Preserve parent/child hierarchy and occurrence paths | Nested assembly fixture passes |
| 17 | XDE scene | Deduplicate shared mesh definitions while retaining instances | Two-instance/one-definition fixture passes |
| 18 | TopLoc/gp | Copy local and composed world 3x4 transforms | Nested transform fixture passes |
| 19 | XDE scene | Copy names, entries, layers, colors, PBR, and physical materials | Metadata snapshot fixture passes |
| 20 | RWGltf/XCAF | Import glTF and GLB as document-aware scenes | Authored scene read fixtures pass |
| 21 | RWGltf/XCAF | Export glTF and GLB with hierarchy, transforms, mesh, and material | Read-back consistency passes |
| 22 | RWObj/XCAF | Import and export OBJ through document-aware providers | Geometry/group read-back passes |
| 23 | RWPly/Vrml/XCAF | Export PLY and VRML from the same scene document | Non-empty readable outputs pass |
| 24 | STEP/XDE/AIS/package | Execute STEP/XDE-to-scene/LOD/interchange-to-real-HWND in repository and clean package runtime | 62-DLL Preview.5 workflow and screenshot pass |

No mesh-only, material-only, LOD-only, scene-only, format-only, numbered, or dotted
fragment is a Batch H completion point.

## Root-declaration audit

The Preview.4 final inventory was queried for exactly 24 decision-driving roots:
`Poly_Triangulation`, `Poly_TriangulationParameters`, `BRepMesh_IncrementalMesh`,
`BRep_Tool`, `BRepAdaptor_Surface`, `XCAFPrs_DocumentExplorer`,
`XCAFPrs_DocumentNode`, `XCAFPrs_Style`, `XCAFDoc_VisMaterial`,
`XCAFDoc_VisMaterialTool`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`,
`XCAFDoc_Location`, `RWMesh_CoordinateSystemConverter`,
`RWMesh_TriangulationSource`, `RWGltf_CafReader`, `RWGltf_CafWriter`,
`RWObj_CafReader`, `RWObj_CafWriter`, `RWPly_CafWriter`, `VrmlAPI_Writer`,
`DEGLTF_Provider`, `DEOBJ_Provider`, and `DEPLY_Provider`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 450 | Requires native-local algorithms/providers or copied snapshots |
| `Emitted` | 154 | Reused only where generated ownership already matches |
| `Manual` | 10 | Existing mesh/XDE behavior is not counted again |
| `Skipped` | 226 | Destructors, metadata, protected helpers, and unsafe declarations remain excluded |
| **Total** | **840** | Deduplicated audit candidates; product completion remains the 24 rows above |

SC-044 reconciles exactly 24 newly direct blocked overloads used by the implementation.
This audit was not bulk-marked manual.

## Cross-family dependency closure

- BRepMesh algorithms, Poly triangulations/arrays, XCAFPrs explorers/styles, visual-
  material attributes/tools, and RW provider/session state remain native-call-local.
- Positions, normals, UVs, indices, face/group IDs, transforms, colors, materials,
  bounds, statistics, diagnostics, LODs, and scene nodes cross only as copied values and
  caller-owned arrays.
- XDE labels remain document-parent-bound. Scene snapshots retain no label, document,
  provider, triangulation, iterator, image texture, or native collection.
- Shared scene definitions are deduplicated by stable copied definition identity;
  instances carry copied transforms and never borrow occurrence locations.
- One managed assembly, one native DLL, one package, stable public full names, and the
  accepted generated shard dependency graph remain unchanged.

## Validation gates

Batch H reaches 24/24 only when SC-044 reconciliation, focused tests, full Release and
Debug builds, Generator and Runtime suites, real STEP/XDE and real-HWND execution, the
clean 62-DLL package consumer, generated freshness, byte-identical regeneration, API
compatibility, full inventory, runtime hashes, SBOM/provenance/checksums, documentation,
and the complete Preview.5 local release check all pass together.

All gates passed for Preview.5. Release and Debug build with zero warnings/errors;
Generator 91/91, Runtime 131/131, focused Batch H 4/4, and dependency profiles 6/6 pass.
Repository runtime and the clean 62-DLL package consumer execute grouped mesh attributes,
statistics, diagnostics, LODs, PBR/physical metadata, nested/shared scene instances,
glTF/GLB/OBJ read/write, PLY/VRML write, STEP/XDE, and a real-HWND screenshot. All 83
generated files are current and byte-identical after clean regeneration; the 27-edge
generated dependency graph remains resolved and acyclic. Full inventory closes 116,272
declarations and 7,090 headers with 16,353 emitted, 373 manual, 49,344 skipped, 50,202
blocked, and zero supported-unselected/pending/HD099. API comparison is additive at
37,904 additions and zero removals. Runtime hashes, SBOM, provenance, checksums, Git
whitespace, and the complete local release check pass.

## Explicit non-goals

GPU-resident buffers, mutable native Poly handles, arbitrary shader/callback systems,
texture image decoding/editing, Draco compression, animation/skinning, point-cloud
editing, DXF/DWG, IVtk/VTK, Draw/OpenGL ES profiles, physical assembly/DLL/package
splitting, hosted release, signing, NuGet publication, and GitHub work are outside H.

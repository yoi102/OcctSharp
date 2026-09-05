# OcctSharp 8.0.1-preview.17

Local-only Batch R preview. No NuGet publication or GitHub push is part of delivery.
Native ABI 1.61, bridge 0.69.0, OCCT 8.0.1, schema 1.13; assembly identity stays 0.1.0.0.

## Authored mesh workflows

The unchanged [40-capability wave](BATCH_R_MESH_AUTHORING_EDITING_GAP_INVENTORY.md)
adds immutable attributed meshes, polylines and per-corner seams; copied position,
connectivity, extraction, deletion, compaction and concatenation edits; exact composable
revision maps; full adjacency, boundaries/components and constrained selections;
coherent patch editing, degenerate/duplicate removal, actual node welding, seam/crease
splitting, orientation and normal/UV editing; rigid/affine/mirror and coordinate/unit
conversion; authored statistics and selected measurements; owning discrete topology
and exact-cache copy-on-write; editable STL/OBJ import; grouped XDE instances, direct
STL/OBJ/glTF/GLB/PLY delivery and real-viewer revision replacement.

## Structure and ownership

MeshData owns copied values with no Modeling/XDE dependency. Mesh owns editing and
graph orchestration; Modeling owns Shape/cache adapters; facade integration owns XDE,
formats and viewer review. Four independent Native units add call-local algorithms,
not another registry, native owner, managed project or DLL. The existing twelve module
assemblies, compatibility facade and shared runtime package remain. SC-055 records
48 exact formerly Blocked declarations, not every member of the audited roots.
Sixteen additive C exports use fixed copied buffers and existing Shape ownership.
Native package license paths no longer duplicate the dependency directory (for
example `licenses/occt/occt/`); each notice/license retains its original source bytes.

Copied meshes/maps do not require Dispose. DiscreteMeshModel and Shape do; independent
copies survive their source. Selections reject foreign/stale revisions. Viewer review
remains creating-thread-affine and parent-bound; replacement failure retains the old
presentation. Exact-cache replacement works on copied selected faces and preserves
exact geometry/source caches. No snapshot or direct writer implicitly invokes meshing.

## Format and algorithm limits

- A triangulation-only face is not an exact surface or proof of a valid closed solid.
  Exact STEP/IGES exports reject; surface-backed checks are explicit.
- STL/OBJ units are caller assumptions. STL drops authored attributes/materials;
  editable OBJ retains supported UV/normal seams, not MTL/group names or line assets.
- Missing UV/normal channels stay absent; incomplete OBJ normals are undefined.
  Export omits a channel containing undefined normals and reports that loss instead
  of using the SDK iterator's fabricated +Z direction. glTF forces supplied UV export.
- glTF uses metre/Y-up coordinates and float vertex precision. PLY retains supported
  attributes/colors/part IDs, not a complete assembly or PBR material model.
- Welding rechecks double distances after OCCT float hashing and preserves requested
  attribute/material partitions. Non-orientable/nonmanifold components are reported,
  not guessed. Degenerate removal does not silently collapse/reconnect other facets.
- No mesh Boolean, hole-filling/decimation engine, reverse engineering into exact CAD,
  texture pipeline, headless/D3DImage rendering or additional platform proof is included.

## Validation

Focused tests pass 24/24, including independent numeric checks, a manifold Mobius strip,
large-normal affine transforms, raw ABI failures, cache/disposal loops, malformed input,
Unicode real formats, shared XDE definitions/undo-redo and real HWND material captures.
Release/Debug Generator 91/91 and Runtime 229/229, plus isolated actual Debug-native
229/229, pass; all 36 private headers pass standalone strict MSVC checks. OBJ invalid
attribute references reject instead of inventing channel values; very large/small
finite normals retain their direction. Full local release-check, both clean consumers,
14 package audits, fresh-source build, 94 byte-identical generated files, inventory
and additive API/ABI comparison pass. There are 404 managed additions and 16 Native
exports with no removals against Q. All original 40 capabilities are locally validated;
publication readiness remains separate and false.

OcctSharp-owned code is MIT. Bundled OCCT retains LGPL-2.1 with the Open CASCADE
exception (or a separately obtained commercial license); other native dependencies
retain their own terms. The native package includes applicable notices/license texts.

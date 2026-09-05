# Batch R: Mesh authoring editing and discrete model delivery

- Status: scope preparation complete; implementation **0/40**; new compile/runtime gates **NOT RUN**.
- Decision: [ADR-0082](adr/0082-broad-batch-q-through-t-preparation.md).
- Preparation baseline: commit `6b04bd9`, package `8.0.1-preview.15`, OCCT 8.0.1.
- Planned local package slot: `8.0.1-preview.17`; not a version change or publication.
- Execution contract and shared gates: [Q-T preparation](BATCH_Q_T_PREPARATION.md).
- Frozen configuration: [batch-r-mesh-authoring-editing.json](../OcctSharp/config/batches/batch-r-mesh-authoring-editing.json).

## Product outcome and existing coverage

Batch H `AdvancedMesh.Create/CreateLods` already meshes shapes and produces grouped snapshots, counts, welded diagnostics, copied scenes, materials and mesh-format exports. R adds caller-authored/edited mesh data and its delivery path; existing statistics, LOD generation and exporters are not counted again.

Caller arrays/STL/OBJ -> attributed copied mesh -> connected selection/edit/weld ->
old/new remap -> discrete owning shape -> XDE occurrences/material groups ->
mesh-format delivery -> real viewer replacement. Include seams, duplicated/reversed
triangles, disconnected components, a non-manifold junction, negative scale and a
triangulation-only face rejected by exact-modeling operations.

One row below is one independently observable workflow capability, not one getter,
native class, test case or commit. All 40 rows plus their dependencies and validation
form one batch. Internal source groups are implementation responsibilities, not smaller
delivery batches. The `Integration`/`Execution` groups compose the audited roots and
existing public operations; they do not imply new OCCT class roots.

## Frozen capability and acceptance matrix

All rows are prepared and unimplemented. Each acceptance statement is a required future
test, not a report that it has passed. Shared lifetime/negative/package gates apply to
every applicable row and are not counted as additional capabilities.

| ID | Root group | New capability | Required observable acceptance |
|---|---|---|---|
| R-01 | Data | Caller-authored copied triangulation | Create an immutable mesh from points and zero-based triangle indices; validate counts, finiteness and overflow before native allocation. |
| R-02 | Data | Attributed mesh authoring | Author optional normals and UVs with explicit presence and cardinality; reject mismatched channels rather than filling plausible zeros. |
| R-03 | Data | Mesh-associated polyline authoring | Create copied 3D polylines and triangulation-index polygons with parameter/closedness validation. |
| R-04 | Data | Per-corner attribute seam representation | Represent UV/normal discontinuities by explicit vertex duplication and retain logical source-vertex correspondence. |
| R-05 | Edit | Immutable vertex-position edit | Edit selected positions into a new revision; invalidate or recompute affected derived channels without changing the source. |
| R-06 | Edit | Immutable triangle-connectivity edit | Replace selected connectivity with bounded indices and return affected region plus old/new triangle correspondence. |
| R-07 | Edit | Selected submesh extraction | Return a standalone copied submesh with compact indices, retained channels and original group provenance. |
| R-08 | Edit | Multi-mesh concatenation | Combine independently authored meshes with source offsets, group/material remapping and overflow checks. |
| R-09 | Edit | Selected triangle deletion | Delete a triangle set and return explicit deleted/retained index maps with unchanged surviving attributes. |
| R-10 | Edit | Unused-node compaction | Remove orphan vertices and remap all triangle/polyline/channel references consistently. |
| R-11 | Edit | Connected patch insertion | Use coherent-triangulation add/replace operations for a caller-supplied patch; reject conflicting adjacency without partial mutation. |
| R-12 | Edit | Composable mesh-edit correspondence | Compose one-to-many/deleted vertex and triangle maps across edits; scope IDs to source/result revisions. |
| R-13 | Edit | Copied full adjacency graph | Expose triangle neighbors and vertex incident triangles, not only H diagnostic counts; keep boundary and non-manifold uses distinct. |
| R-14 | Edit | Ordered mesh boundary extraction | Build copied oriented boundary chains/loops from connectivity; report branch points instead of inventing a closed contour. |
| R-15 | Edit | Connected-component extraction | Return independently usable mesh components and provenance, extending H component counts without duplicating them. |
| R-16 | Edit | Selected region expansion by adjacency | Expand a triangle selection by a bounded number of rings without crossing requested attribute/material boundaries. |
| R-17 | Edit | Degenerate-triangle removal | Use coherent triangulation removal with an explicit area/length policy; return a new mesh and exact removed-index set. |
| R-18 | Edit | Duplicate-triangle elimination | Remove duplicate oriented/unoriented triangles under an explicit policy; retain intentional opposite-facing surfaces by default. |
| R-19 | Edit | Geometric node welding | Use Poly_MergeNodesTool with declared tolerance and deterministic correspondence; recheck connectivity after actual topology changes. |
| R-20 | Edit | Attribute-preserving welding | Partition merges by normal/UV/material compatibility so a geometric seam is not silently collapsed. |
| R-21 | Edit | Crease/material boundary vertex splitting | Duplicate vertices across explicitly selected discontinuities and rebuild local adjacency without changing geometry. |
| R-22 | Edit | Component orientation repair | Propagate consistent winding on orientable manifold components; report non-orientable or ambiguous components without guessing. |
| R-23 | Edit | Selected winding reversal | Reverse a selected complete component with coherent normals and index maps; prevent unintended mixed winding at a shared boundary. |
| R-24 | Data | Area-weighted normal reconstruction | Rebuild missing normals for authored meshes with declared crease handling and undefined-normal flags at degenerate vertices. |
| R-25 | Data | Explicit normal-channel editing | Replace/normalize selected normals in a copied mesh and retain UVs, groups and topology correspondence. |
| R-26 | Data | UV-channel transformation | Apply an explicit affine UV transform to selected charts without altering geometric coordinates or merging seams. |
| R-27 | Data | Rigid and uniform-scale mesh transformation | Bake placement/scale into points and normals with correct units and bounds while retaining provenance. |
| R-28 | Data | General affine and mirror mesh transformation | Use inverse-transpose normals and determinant-aware winding; reject singular transforms rather than emit invalid normals. |
| R-29 | Delivery | Coordinate-system and unit conversion | Convert authored mesh coordinates through RWMesh conventions exactly once; include handedness/up-axis/unit metadata. |
| R-30 | Data | Authored mesh spatial/area queries | Make existing statistics available on edited data and add selected-region bounds/area with provenance; do not relabel H whole-mesh counts as new. |
| R-31 | Bridge | Triangulation-only owning face construction | Create an owning discrete face/shape from authored data; explicitly distinguish it from exact surface-backed BRep. |
| R-32 | Bridge | Discrete shape round-trip snapshot | Read authored triangulation, groups and placement back after source mesh disposal without an accidental remeshing step. |
| R-33 | Bridge | Copy-on-write BRep triangulation replacement | Attach validated replacement triangulation only to copied selected faces; keep exact geometry and source triangulation unchanged. |
| R-34 | Bridge | Controlled mesh invalidation and remeshing | For exact shapes only, invalidate selected copied caches and remesh with explicit parameters; reject triangulation-only targets. |
| R-35 | Delivery | Direct STL-to-editable-mesh import | Read discrete STL into authored mesh contracts with unit assumptions, dropped-channel disclosure and bounded malformed-input handling. |
| R-36 | Delivery | Direct OBJ-to-editable-mesh import | Expose supported positions/connectivity/UV/normal data for editing and explicitly report channels the reader cannot retain. |
| R-37 | Delivery | Authored mesh-to-XDE assembly placement | Add discrete products with material/group mapping, repeated occurrences and rigid placements without converting them to exact solids. |
| R-38 | Delivery | Edited-mesh material-group reassignment | Assign/split groups and materials on edited triangle ranges and carry their correspondence into copied scenes. |
| R-39 | Delivery | Edited discrete-model format delivery | Deliver caller-authored/edited data through STL/OBJ/glTF/PLY as supported; round-trip supported channels and reject exact STEP/IGES promises. |
| R-40 | Integration | Authored mesh viewer review and replacement | Display the owning discrete result with group materials, fit/select it, replace an edited revision and reject stale IDs on the real HWND path. |

## Native decision roots and dependency closure

| Root group | Exact inventory roots |
|---|---|
| Data | `Poly`, `Poly_Triangle`, `Poly_Triangulation`, `Poly_TriangulationParameters`, `Poly_Polygon3D`, `Poly_PolygonOnTriangulation` |
| Edit | `Poly_Connect`, `Poly_CoherentTriangulation`, `Poly_CoherentNode`, `Poly_CoherentTriangle`, `Poly_CoherentLink`, `Poly_MergeNodesTool` |
| Bridge | `BRep_Builder`, `BRepMesh_IncrementalMesh`, `IMeshTools_Parameters` |
| Delivery | `RWStl`, `RWObj_TriangulationReader`, `RWObj_CafWriter`, `RWGltf_CafWriter`, `RWPly_CafWriter`, `RWMesh_CoordinateSystemConverter`, `XCAFDoc_VisMaterial`, `XCAFDoc_VisMaterialTool` |

These 23 decision roots are a candidate audit, not a commitment to expose
every declaration. Reused support roots (23) are:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `gp_Trsf`, `gp_GTrsf`, `Bnd_Box`, `Quantity_ColorRGBA`, `XCAFPrs_DocumentExplorer`, `XCAFPrs_Style`.

There are no additional header-only exceptions in this batch configuration.

Dependencies close through copied value/definition inputs, native-local algorithm and
container use, registered owning topology results, parent-bound documents and viewer
objects, and existing exchange providers. OCCT toolkit dependencies reuse the existing
explicit CMake core closure; availability evidence is not link/runtime proof for new code.

### Baseline audit evidence

Full inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 46 | 2213 | 1011 | 526 | 85 | 591 |

Two audit runs are byte-identical. Report SHA256:
`5E684FCEE0294B9CC26D0069F8BA0D0CADF73FB4BE67268C6E9017EF69B055EF`.
Regenerate with `eng/audit-batch-roots.ps1` using the linked config and the pinned
inventory; report path is `artifacts/generator-reports/batch-r-root-audit.json`
inside the code workspace. Reused support accounts for much of these counts.
Candidates are neither 40 capabilities nor an implementation/API denominator.
Do not mark unrelated blocked/template/unsupported IDs manual merely because their
root appears here.

## Implementation ownership and source placement

Native: add cohesive `Mesh/MeshAuthoring.cpp`, `Mesh/MeshEditing.cpp` and
`Mesh/MeshTopology.cpp`; format adapters remain in Exchange, viewer code in Visualization.
Managed: pure copied data in MeshData, algorithms in Mesh, topology adapters in Modeling,
and integration in existing higher owners/facade. MeshData must not acquire a dependency
on Modeling or XDE; primitive-group material references at that layer are opaque copied
keys, with XDE material resolution above it.

Builders, adaptors, iterators and temporary arrays remain native-call-local; copied
results contain no borrowed pointers. Any owning result container needs a matching
release path and source-disposal tests. Shape owners reuse the current registration
and release family. Document labels and viewer IDs remain parent-bound and thread rules
remain unchanged. Concurrent release/use is not newly supported. Before introducing an
actual handle/layout/manual binding exception, update OWNERSHIP, NATIVE_ABI and
SPECIAL_CASES with exact directly invoked stable IDs; this preparation does not add one.

## Constraints and non-goals

No general reverse engineering from triangles into exact CAD surfaces, mesh Boolean,
automatic hole-filling solver, decimation engine or arbitrary texture-asset pipeline.
Adjacency/welding/editing reuse OCCT Poly primitives; small copied-data orchestration
does not justify a new geometry engine. STEP/IGES support roots describe the inherited
exchange boundary, not guaranteed exact export of discrete-only shapes. Volume/solid
validity must not be inferred from a closed-looking triangle set. `Poly_MergeNodesTool`
is the SDK class; there is no `Poly_MergeNodes.hxx`.

## Entry and completion gates

Use the shared [entry/delta protocol and validation gates](BATCH_Q_T_PREPARATION.md).
R follows the previous whole-batch checkpoint in the delivery sequence. It has no artificial hard dependency on every earlier new algorithm; the shared baseline must nevertheless be re-audited after preceding commits.
The capability count stays 40 when the baseline changes; already delivered capabilities
are prerequisites, not a reason to pad the denominator. A substantive unsupported
capability or changed product outcome requires an explicit documented scope decision,
not silent deletion or a smaller completion claim.

Completion requires a 40-row test mapping, Release/Debug builds and regression with the
actual Debug native DLL, source-layout/dependency checks, exact stable-ID reconciliation,
applicable real-file/HWND workflows, both clean package consumers, clean regeneration,
API/ABI compatibility, runtime manifest and local release evidence, documentation and
one local batch commit. No automatic NuGet publication or GitHub push.

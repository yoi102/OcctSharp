# Batch K assembly authoring, BOM, and occurrence gap inventory

This document locks the product denominator and complete cross-family dependency closure
for Batch K before implementation. It measures one editable product-structure workflow,
not individual XCAFDoc, TDF, STEPCAFControl, TopLoc, BRepGProp, or AIS class counts.

Preparation status: **COMPLETE**. Implementation status: **NOT STARTED (0/24)**. The
denominator below is immutable for Batch K.

## Product outcome

A Windows x64 .NET application can author and revise nested XDE assemblies, resolve and
audit occurrence identity, produce structured and flattened BOMs, preserve external and
instance metadata, aggregate physical properties, and review the result through STEP/XDE
and a real viewer without exposing TDF labels, XCAF tools, native iterators, or borrowed
topology.

```text
owned XDE document, part definitions, and explicit occurrence edits
  -> document-bound XCAF/TDF mutation inside one named transaction
  -> copied product-structure, BOM, diagnostics, and effective metadata snapshots
  -> independently owning located topology and aggregate properties
  -> STEP/XDE round trip, real-HWND review, disposal, and clean-package workflow
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch K exit evidence |
|---:|---|---|---|
| 1 | Part definition | Add, find, and update reusable part definitions with owning shapes | Definition replacement and source-disposal fixtures pass |
| 2 | Nested assembly | Create nested subassemblies with stable parent/document identity | Three-level hierarchy fixture passes |
| 3 | Occurrence | Add repeated component occurrences with independent locations | Shared-part/multiple-placement fixture passes |
| 4 | Relocation | Change one occurrence location without mutating its definition or siblings | World-location and sibling-independence checks pass |
| 5 | Relink | Replace an occurrence's referred part or subassembly atomically | Old/new where-used tables and located shape pass |
| 6 | Remove occurrence | Remove one component while retaining the reusable definition | Hierarchy, free-shape, and metadata checks pass |
| 7 | Remove definition | Remove a part or assembly only under an explicit usage policy | In-use rejection and orphan cleanup pass |
| 8 | Clone subtree | Clone a part/assembly subtree with selected metadata and independent labels | Deep-copy and source-disposal checks pass |
| 9 | Reparent | Move an occurrence between assemblies as one transaction | Placement preservation and rollback pass |
| 10 | Path resolution | Resolve an occurrence path to occurrence/referred labels and owning located topology | Valid, missing, and stale path fixtures pass |
| 11 | Where-used | Copy direct and recursive reverse-usage records for any definition | Shared nested-part fixture passes |
| 12 | Assembly graph | Copy roots, nodes, links, node kinds, and occurrence counts | Deterministic graph snapshot passes |
| 13 | Structured BOM | Produce a hierarchy-preserving BOM with entries, names, quantities, and paths | Nested assembly table passes |
| 14 | Flattened BOM | Group repeated definitions into deterministic total quantities | Multi-level shared-part quantity fixture passes |
| 15 | Structure diagnostics | Detect cycles, dangling references, invalid paths, duplicate links, and orphans | Valid and deliberately corrupted fixtures pass |
| 16 | Item reference | Create and resolve assembly-item and subshape references without exposing label layouts | Label/GUID/subshape-index fixtures pass |
| 17 | External reference | Copy, replace, clear, and round-trip external reference path/URI metadata | Multi-reference and missing-target diagnostics pass |
| 18 | SHUO | Author and resolve specific higher-usage occurrence chains | Multi-level style/usage chain fixture passes |
| 19 | Effective metadata | Set and resolve occurrence name, color, layer, physical material, and visual material overrides | Definition fallback and instance override fixtures pass |
| 20 | Property rollup | Aggregate world-space bounds, mass, centre, and count by occurrence and BOM group | Transformed shared-part rollup passes |
| 21 | Transaction/history | Make edits atomic, undoable, redoable, and rollback-safe with copied before/after snapshots | Named command, undo/redo, and abort fixtures pass |
| 22 | STEP/XDE | Import, edit, export, and re-read hierarchy, placements, references, and effective metadata | Real STEP/XDE round trip passes |
| 23 | Viewer review | Present/isolate/select occurrences, fit, and capture a screenshot keyed by occurrence path | Real HWND workflow passes |
| 24 | Package/lifetime | Repeat the complete assembly/BOM/reference workflow from a clean package | 62-DLL Preview.8 workflow passes |

No editing-only, BOM-only, metadata-only, STEP-only, viewer-only, numbered, or dotted
fragment is a Batch K completion point.

## Root-declaration audit

The Preview.7 final inventory was queried for exactly 24 decision-driving roots:
`XCAFDoc_ShapeTool`, `XCAFDoc_AssemblyGraph`, `XCAFDoc_AssemblyItemRef`,
`XCAFDoc_Location`, `XCAFDoc_GraphNode`, `XCAFDoc_Editor`,
`XCAFPrs_DocumentExplorer`, `XCAFDoc_ColorTool`, `XCAFDoc_LayerTool`,
`XCAFDoc_MaterialTool`, `XCAFDoc_VisMaterialTool`, `XCAFDoc_DocumentTool`,
`STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `STEPControl_Reader`,
`STEPControl_Writer`, `TDF_Label`, `TDF_ChildIterator`, `TDataStd_TreeNode`,
`TDataStd_Name`, `TopLoc_Location`, `TopoDS_Shape`, `BRepGProp`, and
`AIS_InteractiveContext`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 610 | Requires document-local tools/iterators, copied snapshots, or owning topology work |
| `Emitted` | 292 | Reused only where generated ownership already matches |
| `Manual` | 34 | Existing XDE/document/location/property/viewer behavior is reused, not counted again |
| `Skipped` | 292 | Destructors, operators, protected helpers, and unsafe declarations remain excluded |
| **Total** | **1,228** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only newly direct blocked overloads used by the implementation may be reconciled under
SC-047. This audit will not be bulk-marked manual and does not change the Preview.7 full
inventory.

## Cross-family dependency closure

- XCAFDoc tools, TDF iterators/labels, STEP/STEPCAF sessions, maps, sequences, graph
  builders, and presentation explorers remain document- or call-local.
- Managed labels and occurrences remain parent-bound stable entries. Mutations require
  one live owned document and an open named transaction; multi-step edits roll back as a
  unit and cannot silently change caller-owned shape wrappers.
- Product-structure, BOM, where-used, path, external-reference, effective-metadata,
  diagnostics, and property-rollup results cross as immutable copied records and arrays.
  No native iterator, label node, graph node, string container, or attribute handle
  crosses the ABI.
- Located/subshape results cross only as independent registered owning `Shape` values;
  locations remain independent opaque owners. Returned snapshots retain neither the
  document nor an XCAF tool.
- External references are copied metadata. Batch K does not perform implicit network
  access or promise automatic loading of an external document.
- Viewer presentations remain creating-thread-affine parent IDs. One managed assembly,
  one native DLL, one package, stable public full names, and the accepted generated shard
  dependency graph remain unchanged.

## Validation gates

Batch K reaches 24/24 only when SC-047 exact reconciliation, focused tests, full Release
and Debug builds, Generator and Runtime suites, named transaction/undo/lifetime evidence,
real STEP/XDE plus real-HWND evidence, the clean 62-DLL package consumer, generated
freshness, byte-identical regeneration, API compatibility, full inventory, runtime
hashes, SBOM/provenance/checksums, documentation, and the complete Preview.8 local release
check all pass together.

All Batch K implementation and validation gates are currently `NOT RUN`. The only
completed evidence is the finite 24-capability denominator and the 24-root/1,228-
declaration Preview.7 baseline audit above.

## Explicit non-goals

PLM/PDM servers, implicit network fetching, proprietary CAD translators, arbitrary
callbacks, concurrent document mutation, cross-document transactions, editable native
label/graph/iterator wrappers, custom STEP allocators, physical assembly/DLL/package
splitting, hosted release, signing, NuGet publication, and GitHub work are outside Batch K.

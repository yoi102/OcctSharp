# Batch N IGES/XDE interoperability gap inventory

This document locks the product denominator, dependency closure, and exit evidence for
Batch N. It measures one metadata-aware IGES/XDE workflow, not isolated IGESCAF,
IGESControl, XSControl, XCAF, OCAF, path, viewer, or package class counts.

Preparation and implementation status: **COMPLETE — 24/24 capabilities (100%)**. The
denominator below is immutable for Batch N.

## Product outcome

A Windows x64 .NET application can read, compose, display, and write IGES through an
owned XDE document while retaining supported names, colors, layers, visibility, units,
and diagnostics. The same public workflow accepts STEP or IGES, supports non-ASCII
Windows paths without exposing temporary files, and preserves the existing document,
label, topology, and viewer ownership boundaries.

```text
STEP or IGES path plus explicit/derived format and options
  -> native-local exchange session and XDE transfer
  -> parent-bound labels plus copied diagnostics and metadata
  -> assembly composition, XDE-aware viewer display, export, round-trip, and package evidence
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch N exit evidence |
|---:|---|---|---|
| 1 | Exchange | Resolve STEP or IGES from an explicit format or supported extension | Format-routing fixtures pass |
| 2 | IGES/XDE | Read IGES into an owned `XdeDocument` | Document/root fixture passes |
| 3 | IGES/XDE | Transfer all transferable IGES roots | Multi-root fixture passes |
| 4 | Metadata | Preserve IGES part/entity names | Name snapshot fixture passes |
| 5 | Metadata | Preserve generic, surface, and curve colors | Color snapshot fixture passes |
| 6 | Metadata | Preserve layer assignments | Layer snapshot fixture passes |
| 7 | Metadata | Preserve effective visibility and style inheritance | Presentation fixture passes |
| 8 | Diagnostics | Copy source, root, and transfer diagnostics | Failure/partial fixture passes |
| 9 | Units | Copy source and target length-unit diagnostics | Unit fixture passes |
| 10 | Paths | Read from a non-ASCII Windows path | Unicode input fixture passes |
| 11 | Composition | Import IGES roots into an existing XDE document | Import fixture passes |
| 12 | Ownership | Return destination-parent-bound imported labels | Parent/lifetime fixture passes |
| 13 | Assembly | Compose IGES roots with existing STEP and in-memory parts | Mixed assembly fixture passes |
| 14 | Ownership | Retain imported labels after source/session disposal | Source-disposal fixture passes |
| 15 | IGES/XDE | Write an owned XDE document as IGES | Export fixture passes |
| 16 | Options | Independently enable name, color, and layer export | Option matrix passes |
| 17 | IGES/XDE | Export all eligible free roots | Multi-root export fixture passes |
| 18 | Paths | Write to a non-ASCII Windows path | Unicode output fixture passes |
| 19 | Round-trip | Preserve geometry through IGES read/write/read | Geometry comparison passes |
| 20 | Round-trip | Preserve supported names, colors, and layers | Metadata comparison passes |
| 21 | API | Offer format-neutral `ReadExchange`, `ImportExchange`, and `WriteExchange` | STEP/IGES routing passes |
| 22 | WPF sample | Route IGES through XDE instead of geometry-only exchange | Sample build/workflow passes |
| 23 | Viewer | Display IGES/XDE labels with `Display(XdeLabel)` styles | Real-HWND evidence passes |
| 24 | Package | Validate real files, Unicode paths, disposal, and clean package consumption | Preview.13 consumer passes |

No reader-only, writer-only, color-only, layer-only, Unicode-only, viewer-only,
numbered, or dotted fragment is a Batch N completion point.

## Root-declaration audit

The Preview.12 final inventory was queried for exactly these 24 decision-driving roots:
`IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `IGESControl_Reader`,
`IGESControl_Writer`, `XSControl_WorkSession`, `Interface_InterfaceModel`,
`IFSelect_ReturnStatus`, `XCAFDoc_DocumentTool`, `XCAFDoc_ShapeTool`,
`XCAFDoc_ColorTool`, `XCAFDoc_LayerTool`, `TDocStd_Document`, `TDF_Label`,
`TCollection_AsciiString`, `OSD_Path`, `BRep_Builder`, `TopoDS_Shape`,
`TopLoc_Location`, `Quantity_Color`, `AIS_ColoredShape`, `AIS_InteractiveContext`,
`V3d_View`, `IGESData_IGESModel`, and `UnitsMethods`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 814 | Requires native-local sessions, copied values, or workflow composition |
| `Emitted` | 434 | Reused only where generated ownership already matches |
| `Manual` | 45 | Existing exchange/XDE/viewer behavior is reused |
| `Skipped` | 300 | Destructors, operators, protected helpers, and unsafe declarations stay excluded |
| **Total** | **1,593** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only blocked overloads directly invoked by the new bridge may be reconciled under
SC-051. The other blocked candidates keep their prior dispositions; this audit does not
bulk-promote any root, class, or overload family to manual ownership.

## Cross-family dependency closure

- `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `IGESControl`, `XSControl`, model,
  map, iterator, progress, and diagnostic objects remain call-local in the native bridge.
- Managed code owns `XdeDocument`; returned `XdeLabel` values stay document-parent-bound.
  Shapes copied out of reports or style snapshots retain their existing independent
  registered ownership.
- Names, colors, layers, visibility, unit information, transfer counts, and diagnostic
  text cross only as copied managed values. No OCCT container or C++ layout crosses the
  fixed C ABI.
- IGES import clones transferred roots and supported metadata into the destination
  document and returns destination-parent-bound labels. Source readers, models, and
  transfer sessions may be destroyed before the labels are used.
- Non-ASCII public paths are staged through an internal unique ASCII temporary path only
  when the narrow OCCT API requires it. Input copies, output promotion, exception paths,
  and cleanup are deterministic; callers continue to see their original full path.
- Format-neutral routing composes the existing STEPCAF implementation with the new
  IGESCAF path. It does not duplicate document, assembly, viewer, or topology models.
- The WPF sample displays each IGES free root through the existing XDE-label viewer path,
  so supported XCAF colors and visibility replace the current neutral-only fallback.
- Managed ownership remains in the existing DataExchange/Documents/Viewer modules and
  facade. Native ownership remains one `OcctSharp.Native.dll`; no module or DLL split is
  introduced.
- The additive wave reserves package `8.0.1-preview.13`, native ABI 1.57, bridge 0.65.0,
  and binding schema 1.13 unless implementation proves a generator rule change necessary.

## Validation gates

Batch N reaches 24/24 only when SC-051 exact reconciliation, focused option/routing/
metadata/Unicode/lifetime tests, complete Release and Debug builds, Generator/Runtime
suites, real IGES/XDE plus real-HWND evidence, the clean 62-DLL package consumer,
deterministic generation, clean regeneration, compatibility/inventory/runtime hashes,
SBOM/provenance/checksums, documentation, and the complete Preview.13 local release check
pass together.

All gates pass in Preview.13. SC-051 reconciles exactly 15 directly used blocked stable
IDs. Release and Debug build all 19 projects with zero code warnings/errors; Generator
91/91, Runtime 156/156, focused Batch N 4/4, and dependency profiles 6/6 pass. The
repository runtime and clean facade consumer execute metadata-aware IGES read/import/
write, independent name/color/layer modes, copied diagnostics and units, non-ASCII input
and output staging with failure cleanup, mixed STEP/IGES composition, round-trip,
source/session disposal, and real-HWND XDE-label display. All 94 generated files are
fresh and byte-identical after clean regeneration. API comparison against alpha.38 is
additive at 38,838 additions and zero removals. Full inventory closes 116,272 declarations
and 7,090 headers with zero pending/HD099. The committed 15,356,928-byte bridge is
byte-identical to the Release rebuild with SHA256
`7DD8EB7A3CF5EA975F45D2F84812FBB2521B0E35F87C500DF5A42E9FC64C9EAD`.

## Explicit non-goals

Native-DLL splitting, a cross-platform viewer backend, D3DImage rendering, IGES schema
editing, exposing raw IGES entities or transfer sessions, arbitrary OCCT callbacks,
concurrent document/viewer mutation, hosted release, signing, NuGet publication, GitHub,
and push are outside Batch N.

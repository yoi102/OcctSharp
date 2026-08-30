# Batch G technical drawing, hidden-line, section, and vector-output gap inventory

This document locks the product denominator and complete cross-family dependency closure
for Batch G before its implementation wave. It measures one technical-drawing workflow,
not isolated HLR classes, method counts, or family checkpoints.

Preparation status: **COMPLETE**. Implementation status: **COMPLETE (24/24)**. The full
Preview.4 local release gate passes and the denominator below remains immutable for Batch G.

## Product outcome

A Windows x64 .NET application can project one or more owning shapes through exact or
polygonal hidden-line removal, retain visible and hidden edge categories separately,
create planar sections, copy projected curves into managed polylines, compose standard
views, and write layered SVG without exposing an HLR algorithm, iterator, curve, or
borrowed OCCT object.

```text
owning BRep / STEP-XDE shapes plus copied camera and section-plane values
  -> call-local exact or polygonal HLR and section algorithms
  -> ten independently owning visible/hidden category layers
  -> caller-owned projected polyline records
  -> fitted layered SVG and four standard views
  -> real STEP/XDE and real-HWND package workflow
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch G exit evidence |
|---:|---|---|---|
| 1 | gp/HLRAlgo | Validate copied origin, view, up, projection mode, and focus | Finite/non-zero/non-parallel validation passes |
| 2 | HLRAlgo | Orthographic projector | Front/top/right/isometric fixtures pass |
| 3 | HLRAlgo | Perspective projector with explicit focus | Perspective multi-shape fixture passes |
| 4 | TopoDS/HLRBRep | Project one or many owning input shapes in one view | Input collection and disposal independence pass |
| 5 | HLRBRep | Exact analytic hidden-line removal | Exact box/freeform fixtures pass |
| 6 | BRepMesh/HLRBRep | Polygonal hidden-line removal with explicit deflection | Meshed perspective fixture passes |
| 7 | HLRBRep | Owning visible sharp-edge layer | Non-empty copied polylines pass |
| 8 | HLRBRep | Owning hidden sharp-edge layer | Non-empty hidden polylines pass |
| 9 | HLRBRep | Owning visible smooth-edge layer | Layer identity/disposal passes |
| 10 | HLRBRep | Owning hidden smooth-edge layer | Layer identity/disposal passes |
| 11 | HLRBRep | Owning visible sewn-edge layer | Layer identity/disposal passes |
| 12 | HLRBRep | Owning hidden sewn-edge layer | Layer identity/disposal passes |
| 13 | HLRBRep | Owning visible-outline layer | Curved-body outline fixture passes |
| 14 | HLRBRep | Owning hidden-outline layer | Layer identity/disposal passes |
| 15 | HLRBRep | Owning visible isoparameter layer for exact HLR | Configured iso fixture passes |
| 16 | HLRBRep | Owning hidden isoparameter layer for exact HLR | Configured iso fixture passes |
| 17 | ABI/TopoDS | Ten results are independent registered `Shape` owners | Source and sibling disposal tests pass |
| 18 | BRepAlgoAPI | Exact/approximated planar section as owning topology | Four-edge box section and failures pass |
| 19 | BRepAdaptor/TopExp | Copy every drawing edge to bounded managed polylines | Two-call count/copy and bounds checks pass |
| 20 | Managed API | Preserve polyline boundaries and closed flags | Line/curve sampling records pass |
| 21 | SVG | Emit fitted layered SVG with visible/hidden semantics | XML, view box, metadata, and non-empty file pass |
| 22 | SVG | Configure colors, widths, hidden dashes, iso inclusion, and background | Style option fixtures pass |
| 23 | HLR/managed | Compose front, top, right, and isometric standard views | Four-view owning collection passes |
| 24 | STEP/XDE/AIS/package | Execute STEP/XDE-to-drawing/SVG-to-real-HWND in repository and clean package runtime | 62-DLL Preview.4 workflow and screenshot pass |

No exact-only, polygonal-only, section-only, SVG-only, numbered, or dotted fragment is a
Batch G completion point.

## Root-declaration audit

The Preview.3 final inventory was queried for exactly 24 decision-driving roots:
`HLRAlgo_Projector`, `HLRBRep_Algo`, `HLRBRep_HLRToShape`, `HLRBRep_PolyAlgo`,
`HLRBRep_PolyHLRToShape`, `HLRBRep_ShapeBounds`, `HLRBRep_EdgeData`,
`HLRBRep_FaceData`, `BRepAlgoAPI_Section`, `BRepAdaptor_Curve`,
`BRepAdaptor_Surface`, `BRepLib`, `TopExp_Explorer`, `TopExp`, `Geom2d_Curve`,
`Geom2d_BSplineCurve`, `Geom2d_TrimmedCurve`, `GeomAPI_ProjectPointOnCurve`,
`GCPnts_AbscissaPoint`, `Prs3d_LineAspect`, `Prs3d_Drawer`,
`Graphic3d_ArrayOfSegments`, `XCAFDoc_ShapeTool`, and `STEPCAFControl_Reader`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 535 | Requires native-local algorithms, owning topology, or copied drawing records |
| `Emitted` | 233 | Reused only where generated ownership already matches |
| `Manual` | 18 | Inherited section/adaptor/STEP/XDE behavior is not counted again |
| `Skipped` | 283 | Destructors, metadata, protected helpers, and unsafe declarations remain excluded |
| **Total** | **1,069** | Audit candidates; product completion remains the 24 rows above |

SC-043 reconciles only the 33 newly direct blocked overloads used by the implementation.
The root audit is not bulk-marked manual.

## Cross-family dependency closure

- HLR projectors, exact/polygonal algorithms, extractors, mesh state, section builders,
  topology explorers, and curve adaptors are call-local.
- Projection/plane/options and SVG styles are copied managed values. Polyline points,
  offsets, counts, and closed flags cross through caller-owned bounded buffers.
- Every visible/hidden category and section result is an independently registered owning
  `Shape`; no result retains an input, algorithm, projector, iterator, or adaptor.
- STEP/XDE labels retain their document-parent identity and the existing viewer graph
  remains creating-thread-affine and viewer-parent-bound.
- One managed assembly, one native DLL, one package, stable public full names, and the
  accepted generated shard dependency graph remain unchanged.

## Validation gates

Batch G reaches 24/24 only when SC-043 reconciliation, focused tests, full Release and
Debug builds, Generator and Runtime suites, real STEP/XDE and real-HWND execution, the
clean 62-DLL package consumer, generated freshness, byte-identical regeneration, API
compatibility, full inventory, runtime hashes, SBOM/provenance/checksums, documentation,
and the complete local release check all pass together.

All gates passed for Preview.4. Release and Debug build with zero warnings/errors;
Generator 91/91, Runtime 127/127, focused Batch G 4/4, and dependency profiles 6/6 pass.
Repository runtime and the clean 62-DLL package consumer execute exact and polygonal HLR,
all ten owning layers, sections, copied polylines, layered SVG, four standard views,
STEP/XDE, and a real-HWND screenshot. All 83 generated files are fresh and byte-identical
after clean regeneration; the 16,353-declaration generated graph has 27 resolved edges,
zero violations, and zero cycles. Full classification closes 116,272 declarations and
7,090 headers with 349 accepted manual stable IDs and zero pending/HD099. API comparison
against alpha.38 is additive at 37,731 additions and zero removals. Hosted release,
signing, and NuGet publication remain separate `NOT RUN` release-readiness gates.

## Explicit non-goals

Associative drafting documents, dimensions/notes/BOM layout, DXF/DWG, arbitrary custom
rendering or callbacks, IVtk/VTK/Draw/OpenGL ES profiles, physical assembly/DLL/package
splitting, hosted release, signing, NuGet publication, and GitHub work are outside G.

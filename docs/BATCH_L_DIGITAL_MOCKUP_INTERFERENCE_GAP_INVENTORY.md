# Batch L digital mock-up interference and clearance gap inventory

This document locks the product denominator and complete cross-family dependency closure
for Batch L before implementation. It measures one occurrence-aware digital mock-up
validation workflow, not individual Bnd, BVH, BRepExtrema, BRepClass3d, BOP, XDE, or AIS
class counts.

Preparation status: **COMPLETE**. Implementation status: **NOT STARTED (0/24)**. The
denominator below is immutable for Batch L.

## Product outcome

A Windows x64 .NET application can load an XDE assembly, run scalable occurrence-aware
clearance, contact, penetration, and containment analysis, retain copied traceability and
owning issue topology, and review deterministic issues through STEP/XDE and a real viewer
without exposing native broad-phase trees, classifiers, extrema objects, or BOP data.

```text
owned shapes or XDE occurrences plus finite analysis policy
  -> native-local broad phase and exact pair analysis
  -> copied pair matrix, classification, witnesses, and diagnostics
  -> independently owning issue/contact topology
  -> STEP/XDE traceability, real-HWND review, disposal, and clean package
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch L exit evidence |
|---:|---|---|---|
| 1 | Input | Analyze one or many owning shapes with stable caller IDs | Reordering and source-disposal fixtures pass |
| 2 | XDE | Expand selected assembly roots into world-located leaf occurrences | Shared/nested occurrence fixture passes |
| 3 | Bounds | Copy finite axis-aligned world bounds | Analytic transformed fixtures pass |
| 4 | Bounds | Copy oriented bounds with center, axes, and half sizes | Rotated slender-part fixture passes |
| 5 | Broad phase | Build a native-local candidate index without quadratic exact work | Sparse assembly candidate-count fixture passes |
| 6 | Filtering | Exclude same-definition, adjacent, named, or caller-supplied ID pairs | Deterministic exclusion fixtures pass |
| 7 | Distance | Compute exact minimum distance and squared-distance-safe tolerance decisions | Separated solids fixture passes |
| 8 | Witness | Copy closest points and supporting input/subshape identities | Face-edge-vertex witness fixtures pass |
| 9 | Clearance | Classify positive clearance against an explicit threshold | Below/at/above-threshold fixtures pass |
| 10 | Contact | Classify touching pairs without reporting penetration | Tangent face/edge/vertex fixtures pass |
| 11 | Penetration | Classify volumetric overlap and copy overlap volume | Intersecting solids fixture passes |
| 12 | Containment | Distinguish inside, contains, and coincident cases | Nested and coincident solid fixtures pass |
| 13 | Interference | Copy face/face, edge/edge, and mixed interference groups | Multi-contact fixture passes |
| 14 | Self-check | Detect self-intersection on one shape with bounded diagnostics | Valid and deliberately bad fixtures pass |
| 15 | Tolerance | Configure finite confusion, fuzzy, angular, and clearance tolerances | Invalid/edge tolerance fixtures pass |
| 16 | Robustness | Configure parallel, non-destructive, and early-exit behavior | Deterministic serial/parallel results pass |
| 17 | Pair matrix | Return every requested pair with one stable terminal state | Complete symmetric matrix fixture passes |
| 18 | Aggregation | Group and rank issues by severity, occurrence, and definition | Repeated-part assembly fixture passes |
| 19 | Diagnostics | Copy stage, warning, failure, skipped-pair, and timing/count summaries | Success, partial, and invalid inputs pass |
| 20 | Ownership | Return overlap/contact topology as independent owning shapes | Input/result/document disposal passes |
| 21 | Incremental | Reanalyze changed occurrence transforms while preserving stable pair IDs | Relocation fixture matches full rerun |
| 22 | STEP/XDE | Import, analyze, preserve occurrence traceability, and export/re-read | Real assembly round trip passes |
| 23 | Viewer | Color/isolate/select issues, fit, and capture a keyed screenshot | Real HWND workflow passes |
| 24 | Package/lifetime | Repeat the complete analysis/review workflow from a clean package | 62-DLL Preview.9 workflow passes |

No bounds-only, distance-only, clash-only, XDE-only, viewer-only, numbered, or dotted
fragment is a Batch L completion point.

## Root-declaration audit

The Preview.8 final inventory was queried for exactly 24 decision-driving roots:
`Bnd_Box`, `Bnd_OBB`, `BRepBndLib`, `BRepExtrema_DistShapeShape`,
`BRepExtrema_ShapeProximity`, `BRepExtrema_SelfIntersection`,
`BRepExtrema_DistanceSS`, `BRepClass3d_SolidClassifier`,
`BRepClass3d_SolidExplorer`, `BRepAlgoAPI_Common`, `BOPAlgo_ArgumentAnalyzer`,
`BOPAlgo_PaveFiller`, `BOPDS_DS`, `IntTools_Context`, `IntTools_EdgeEdge`,
`IntTools_FaceFace`, `TopExp_Explorer`, `TopoDS_Shape`, `TopLoc_Location`,
`XCAFDoc_ShapeTool`, `XCAFPrs_DocumentExplorer`, `STEPCAFControl_Reader`,
`STEPCAFControl_Writer`, and `AIS_InteractiveContext`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 656 | Requires native-local algorithms/containers, copied results, or owning topology work |
| `Emitted` | 194 | Reused only where generated ownership already matches |
| `Manual` | 51 | Existing bounds/inspection/XDE/viewer behavior is reused, not counted again |
| `Skipped` | 450 | Destructors, operators, protected helpers, and unsafe declarations remain excluded |
| **Total** | **1,351** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only newly direct blocked overloads used by the implementation may be reconciled under
SC-048. The audit is not bulk-marked manual.

## Cross-family dependency closure

- Bnd/BVH trees, BRepExtrema/BRepClass3d/BOP/IntTools algorithms, explorers, maps, and
  progress objects remain call-local. Inputs are borrowed only during a call.
- Pair IDs, classifications, witnesses, bounds, counts, timings, diagnostics, occurrence
  paths, and aggregation tables cross as immutable copied values.
- Overlap/contact/subshape results cross only as independent registered owning `Shape`
  values. XDE labels remain parent-bound stable entries; locations remain owning values.
- Viewer presentations remain viewer/thread-parent-bound. The one managed assembly, one
  native DLL, one package, public full names, and generated shard graph remain unchanged.
- Batch L reserves package identity Preview.9 during preparation. Native ABI 1.54,
  bridge 0.62.0, and schema 1.12 remain implementation changes; Preview.8 remains the
  last complete implementation/release baseline.

## Validation gates

Batch L reaches 24/24 only when SC-048 exact reconciliation, focused tests, full Release
and Debug builds, Generator/Runtime suites, deterministic pair/lifetime evidence, real
STEP/XDE plus real-HWND evidence, the clean 62-DLL consumer, freshness, byte-identical
regeneration, compatibility, inventory, runtime hashes, SBOM/provenance/checksums,
documentation, and the complete Preview.9 local release check pass together.

All Batch L implementation and validation gates are currently `NOT RUN`. Only the finite
24-capability denominator and 24-root/1,351-declaration Preview.8 audit are complete.

## Explicit non-goals

Continuous physics, motion planning, deformable contact, finite-element analysis, GPU
collision engines, arbitrary callbacks, concurrent document mutation, proprietary CAD
translators, physical project/DLL/package splitting, hosted release, signing, NuGet
publication, and GitHub work are outside Batch L.

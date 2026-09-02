# Batch O 2D sketch and planar-modeling gap inventory

This document locks the product denominator, dependency closure, and exit evidence for
Batch O. It measures one useful 2D-definition-to-3D-CAD workflow, not isolated `gp_*2d`,
`Geom2d`, curve, wire, face, feature, exchange, or viewer class counts.

Preparation status: **COMPLETE**. Implementation status: **0/24 capabilities (0%)**.
The denominator below is immutable for Batch O.

## Product outcome

A Windows x64 .NET application can define, inspect, edit, validate, and compose mixed
analytic/freeform 2D sketch curves in an explicit plane, turn closed loops into faces
and solid features, preserve the result through XDE plus STEP/IGES, and review it in the
existing viewer without exposing mutable or borrowed OCCT geometry objects.

```text
copied 2D points/vectors/curve definitions plus explicit sketch plane
  -> native-local Geom2d/Geom2dAPI construction, evaluation, intersection, and offset
  -> copied diagnostics plus independently owning edge/wire/face/solid topology
  -> XDE metadata, STEP/IGES round-trip, real-HWND review, and package evidence
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch O exit evidence |
|---:|---|---|---|
| 1 | Values | Immutable finite 2D point, vector, and normalized direction values | Value/validation fixtures pass |
| 2 | Plane | Explicit origin/X/Y/normal sketch plane with local/world conversion | Round-trip and degeneracy fixtures pass |
| 3 | Analytic curve | Define a bounded line segment | Endpoint/evaluation fixture passes |
| 4 | Analytic curve | Define a circle and bounded circular arc | Radius/sweep fixtures pass |
| 5 | Analytic curve | Define an ellipse and bounded elliptic arc | Axis/sweep fixtures pass |
| 6 | Freeform curve | Define rational/non-rational Bezier curves | Pole/weight fixtures pass |
| 7 | Freeform curve | Interpolate or define rational B-spline curves | Knot/multiplicity/interpolation fixtures pass |
| 8 | Inspection | Evaluate point and first derivative at a normalized or native parameter | Numeric fixtures pass |
| 9 | Inspection | Project a 2D point and return copied ordered solutions | Projection fixture passes |
| 10 | Inspection | Intersect two sketch curves with copied point/parameter solutions | Crossing/tangent/disjoint fixtures pass |
| 11 | Edit | Trim or split a curve without mutating the source definition | Source-disposal fixture passes |
| 12 | Edit | Translate, rotate, scale, and mirror curve definitions | Transform fixture passes |
| 13 | Offset | Offset an open or closed planar curve chain with explicit join policy | Positive/negative/failure fixtures pass |
| 14 | Topology | Convert each supported curve definition to an owning 3D edge in the sketch plane | Edge-kind/location fixture passes |
| 15 | Topology | Order mixed edges into an owning wire with explicit gap tolerance | Shuffled/mixed fixture passes |
| 16 | Validation | Report gaps, duplicate edges, zero length, self intersections, and closure | Diagnostic matrix passes |
| 17 | Measurement | Copy perimeter, signed area, orientation, and bounds | Numeric fixture passes |
| 18 | Regions | Classify outer and inner loops and reject ambiguous nesting | Multi-loop fixture passes |
| 19 | Topology | Build a planar face with zero or more holes | Face-area fixture passes |
| 20 | Feature | Extrude a profile into an owning prism/solid | Volume/topology fixture passes |
| 21 | Feature | Revolve a profile around a plane-relative axis | Volume/topology fixture passes |
| 22 | Feature | Apply additive and subtractive profile features to an owning solid | Fuse/cut fixture passes |
| 23 | XDE/exchange | Preserve named/color/layered planar-feature results through XDE and STEP/IGES | Mixed round-trip fixture passes |
| 24 | Viewer/package | Display/select the result in a real HWND and execute the full workflow from clean packages | Preview.14 consumer passes |

No point-only, line-only, circle-only, freeform-only, intersection-only, wire-only,
face-only, feature-only, exchange-only, viewer-only, numbered, or dotted fragment is a
Batch O completion point.

## Root-declaration audit

The Preview.13 final inventory was queried for exactly these 24 decision-driving roots:
`gp_Pnt2d`, `gp_Vec2d`, `gp_Dir2d`, `gp_Ax2d`, `gp_Ax22d`, `gp_Lin2d`, `gp_Circ2d`,
`gp_Elips2d`, `Geom2d_Curve`, `Geom2d_Line`, `Geom2d_Circle`, `Geom2d_TrimmedCurve`,
`Geom2d_BezierCurve`, `Geom2d_BSplineCurve`, `GC_MakeSegment2d`,
`GC_MakeArcOfCircle2d`, `Geom2dAPI_InterCurveCurve`,
`Geom2dAPI_ProjectPointOnCurve`, `Geom2dAPI_PointsToBSpline`,
`BRepBuilderAPI_MakeEdge2d`, `BRepBuilderAPI_MakeWire`, `BRepBuilderAPI_MakeFace`,
`BRepOffsetAPI_MakeOffset`, and `BRepPrimAPI_MakePrism`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 587 | Requires copied values, native-local algorithms, or owning topology results |
| `Emitted` | 93 | Reused only where generated intrusive ownership already fits |
| `Manual` | 14 | Existing wire/face/offset/prism bridge behavior is inherited |
| `Skipped` | 155 | Destructors, operators, protected helpers, and unsafe declarations stay excluded |
| **Total** | **849** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only blocked overloads directly invoked by the new bridge may be reconciled under
SC-052. The other blocked candidates keep their prior dispositions; the audit does not
bulk-promote a class, root, or overload family.

## Cross-family dependency closure

- Public 2D values, curve definitions, solutions, measurements, and diagnostics are
  immutable copied managed data. Every input collection is validated and copied.
- `gp_*2d`, `Geom2d*`, GC/Geom2dAPI builders, intersectors, projectors, adaptors, maps,
  wire explorers, classifiers, and offset builders remain call-local in the native bridge.
- No generated `Geom2d` shared wrapper becomes mutable state for the friendly API.
- Every edge, wire, face, prism, revolved feature, or Boolean result crosses as an
  independent registered owning `Shape`; native iterators and borrowed curve references
  never cross the C ABI.
- A sketch plane is copied as origin and orthonormal axes. Local/world conversion never
  exposes `gp_Ax3`, `gp_Trsf`, or `Geom_Surface` layout.
- Loop ordering, closure, nesting, orientation, and failure diagnostics are copied.
  Tolerance is explicit and finite; invalid or ambiguous topology fails before mutation.
- XDE labels remain document-parent-bound, viewer presentations remain viewer-parent-
  bound and creating-thread-affine, and exchange uses the existing cleanup-safe path layer.
- ADR-0074's 12 managed modules, facade, one `OcctSharp.Native.dll`, and one shared native
  package remain unchanged. Native DLL splitting is not part of Batch O.
- The additive wave reserves package `8.0.1-preview.14`, native ABI 1.58, bridge 0.66.0,
  and schema 1.13 unless a generator-model change proves a schema bump necessary.

## Validation gates

Batch O reaches 24/24 only when SC-052 exact reconciliation, focused definition/edit/
intersection/validation/topology/feature/lifetime tests, complete Release and Debug builds,
Generator/Runtime suites, real XDE plus STEP/IGES and real-HWND evidence, the clean shared-
runtime facade and direct-module package consumers, deterministic generation, clean
regeneration, compatibility/inventory/runtime hashes, SBOM/provenance/checksums,
documentation, and `git diff --check` pass together.

Preparation has run only the Preview.13 root audit above. Batch O implementation,
native/managed compile, focused/runtime tests, package consumer, regeneration, inventory,
compatibility, real-file/HWND evidence, and Preview.14 release check are **NOT RUN**.

## Explicit non-goals

Parametric constraint solving, a feature-history solver, DXF/DWG, custom callbacks,
D3DImage/OpenGL-D3D sharing, cross-platform viewer work, native-DLL splitting, optional
VTK/OpenGL ES profiles, hosted release, signing, NuGet publication, GitHub, and push are
outside Batch O.

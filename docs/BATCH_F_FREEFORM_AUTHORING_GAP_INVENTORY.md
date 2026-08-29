# Batch F freeform curve, surface, and profile-to-solid authoring gap inventory

This document locks the product denominator and cross-family dependency closure for
Batch F before implementation. It measures one freeform CAD-authoring workflow rather
than isolated OCCT classes, individual algorithms, or method counts.

Preparation status: **COMPLETE**. Implementation status: **0/24 capabilities; NOT
STARTED**. Preview.2 remains the completed Batch E baseline; no Batch F API, ABI,
runtime, package, or test result is claimed here.

## Product outcome

A Windows x64 .NET application should be able to define rational Bezier and B-spline
curves/surfaces from copied data, interpolate and approximate design points, inspect and
edit complete definitions, build profiles and freeform faces, split/offset/fill/loft/
sweep topology, repair the result, preserve it through STEP/XDE, and review it in the
existing HWND viewer without an undocumented native escape hatch.

```text
copied design points, poles, weights, knots, multiplicities, degrees, and tangents
  -> validated rational Bezier/B-spline curve and surface definitions
  -> owning edges, wires, faces, shells, and solids
  -> projection/intersection/offset/fill/split/loft/pipe-shell operations
  -> copied diagnostics plus independently owning topology results
  -> STEP/XDE round trip with analytic/freeform checks
  -> real-HWND selection, measurement, mesh, and screenshot evidence
```

## Locked 24-capability denominator

| # | Family | Stable capability | Preview.2 baseline | Batch F exit evidence |
|---:|---|---|---|---|
| 1 | Geom/BRepBuilderAPI | Create rational and non-rational Bezier edges from copied poles and optional positive weights | Unweighted Bezier edge only | Degree, endpoints, weights, invalid data, and source-array independence pass |
| 2 | Geom/BRepBuilderAPI | Create explicit B-spline edges from poles, optional weights, knots, multiplicities, degree, and periodicity | Interpolation only; no explicit definition | Complete definition validates and reconstructs the same curve |
| 3 | GeomAPI | Interpolate copied 3D points with optional start/end tangents and periodic closure | Point interpolation lacks tangent constraints | Tangent, periodic, tolerance, and failure behavior pass |
| 4 | GeomAPI | Approximate copied 3D points with requested continuity, degree bounds, and tolerance | Missing | Error bound, achieved degree/continuity, and invalid options pass |
| 5 | Geom/Adaptor | Copy a complete Bezier/B-spline curve definition snapshot | Scalar generated methods are fragmented | Poles, weights, knots, multiplicities, degree, rational/periodic/closed flags, and parameter range survive source disposal |
| 6 | Geom | Produce an edited curve definition with pole/weight/knot updates and degree elevation | Generated shared mutation is not a friendly owning workflow | Original remains unchanged; edited reconstruction and validation pass |
| 7 | Geom/BRepBuilderAPI | Reverse, segment, and split a curve into independently owning edge results | Edge trim exists; reverse/multi-split definition semantics are missing | Parameter/order/orientation and source-disposal behavior pass |
| 8 | GeomAPI | Compute curve-curve extrema/intersections and curve-point projection as copied multi-solution records | One edge point projection exists | All solutions, parameters, distances, tangency, ordering, and invalid inputs pass |
| 9 | Geom2d/gp/BRepBuilderAPI | Build a located planar profile from copied 2D line/arc/Bezier/B-spline segments | Polygon wire and separate 3D edges exist | Plane transform, closure, orientation, continuity, and owning wire pass |
| 10 | BRepOffsetAPI/Geom2d | Offset planar wires with join mode, open/closed behavior, and multiple owning results | General shape offset is not a profile offset contract | Convex/concave profiles, join modes, result count, and failure diagnostics pass |
| 11 | Geom/BRepBuilderAPI | Create rational and non-rational Bezier surfaces from rectangular pole/weight grids | Missing friendly surface construction | Bounds, corners, weights, grid validation, and owning face creation pass |
| 12 | Geom/BRepBuilderAPI | Create explicit B-spline surfaces from grids, U/V weights, knots, multiplicities, degrees, and periodicity | Missing friendly surface construction | Complete definition round-trips and invalid topology of arrays fails atomically |
| 13 | GeomAPI | Interpolate or approximate a surface through a copied point grid with degree/continuity/tolerance controls | Missing | Analytic fixtures and maximum deviation checks pass |
| 14 | Geom/Adaptor | Copy a complete Bezier/B-spline surface definition snapshot | Surface adaptor exposes analytic values, not full freeform data | U/V poles, weights, knots, multiplicities, degrees, rational/periodic/closed flags, and bounds survive disposal |
| 15 | Geom | Produce an edited surface definition with pole/weight/knot changes and U/V degree elevation | Missing friendly immutable edit workflow | Original remains unchanged; edited face reconstruction and validation pass |
| 16 | Geom/BRepBuilderAPI | Create a rectangularly trimmed owning face from a freeform surface definition | Planar face and existing face trim do not construct arbitrary surfaces | U/V bounds, orientation, tolerance, and invalid ranges pass |
| 17 | GeomFill/BRepFill | Create a ruled surface/face between compatible boundary edges or wires | Ruled loft is only whole-section topology | Boundary correspondence, orientation, and owning result pass |
| 18 | GeomFill/BRepFill | Fill a surface from two to four boundary edges with optional point/continuity constraints | Missing | G0/G1 fixtures, residual/error report, and incompatible-boundary failures pass |
| 19 | GeomOffset/BRepOffsetAPI | Offset a freeform face/shell with explicit side, tolerance, and join behavior | General offset lacks freeform result diagnostics | Surface distance, orientation, invalid/self-intersecting cases, and owning output pass |
| 20 | BRepAlgoAPI/BRepFeat | Split faces, shells, or solids by owning wire/face tools and copy result/history groups | `Section` and `ReplaceSubshape` do not expose split products | Owning pieces, modified/generated grouping, completeness, and disposal independence pass |
| 21 | BRepOffsetAPI | Build a pipe shell from multiple profiles with auxiliary spine/mode/transition controls and optional solid closure | Single-profile `CreatePipe` only | Section placement, transition modes, solid/shell behavior, and diagnostics pass |
| 22 | BRepOffsetAPI | Build controlled smooth/ruled lofts with compatibility checking, vertex ends, precision, and optional solid closure | Basic loft options only | Compatibility, continuity, precision, cap behavior, and failure report pass |
| 23 | ShapeAnalysis/ShapeFix/BRepBuilderAPI | Analyze, sew, heal, and validate a freeform shell/solid as one copied report plus owning repaired result | Generic sew/fix/validation calls are separate | Free edges, continuity, tolerance growth, repair deltas, and source independence pass |
| 24 | STEP/XDE/AIS/Image | Complete a generated freeform design-to-STEP/XDE-to-real-HWND workflow in repository runtime and a clean package consumer | Batch E validates inspection/PMI, not freeform definition retention | Author, edit, split/fill/loft/sweep, repair, export/reimport, select/measure/mesh, and screenshot pass with 62 DLLs |

The denominator is immutable for the Batch F implementation wave. Required overloads,
records, enums, validation, diagnostics, disposal, and composition belong to their row and
cannot be deferred as curve-only, surface-only, profile-only, topology-only, numbered,
dotted, or per-class completion checkpoints.

## Root-declaration audit

The final Preview.2 inventory was queried for exactly 24 decision-driving OCCT roots:
the four `Geom` Bezier/B-spline curve/surface roots; six `GeomAPI`/`Geom2dAPI`
interpolation, approximation, projection, extrema, and intersection roots; three
`BRepBuilderAPI` edge/wire/face roots; three `BRepOffsetAPI` offset/pipe-shell/loft roots;
`BRepFill_Filling`; two split roots; and four ShapeAnalysis/ShapeFix roots.

| Inventory state | Count | Meaning for Batch F |
|---|---:|---|
| `Emitted` | 215 | Reuse scalar/shared methods only where generated ownership matches the friendly contract |
| `Manual` | 23 | Existing edge/wire/face/interpolate/loft/pipe behavior remains owned by prior special cases |
| `Blocked` | 598 | Requires copied arrays/records, call-local algorithms, owning topology, or multi-result projection |
| `Skipped` | 286 | Destructors, operators, protected helpers, or non-callable declarations remain excluded |
| **Total** | **1,122** | Candidate dependency declarations only; product completion remains 24 capabilities |

The roots are `Geom_BezierCurve`, `Geom_BSplineCurve`, `Geom_BezierSurface`,
`Geom_BSplineSurface`, `GeomAPI_Interpolate`, `GeomAPI_PointsToBSpline`,
`GeomAPI_PointsToBSplineSurface`, `GeomAPI_ProjectPointOnCurve`,
`GeomAPI_ExtremaCurveCurve`, `GeomAPI_IntCS`, `Geom2dAPI_Interpolate`,
`BRepBuilderAPI_MakeEdge`, `BRepBuilderAPI_MakeWire`, `BRepBuilderAPI_MakeFace`,
`BRepOffsetAPI_MakeOffset`, `BRepOffsetAPI_MakePipeShell`,
`BRepOffsetAPI_ThruSections`, `BRepFill_Filling`, `BRepAlgoAPI_Splitter`,
`BRepFeat_SplitShape`, `ShapeAnalysis_Surface`, `ShapeAnalysis_Curve`,
`ShapeFix_Face`, and `ShapeFix_Shell`.

Direct declarations actually used by the implementation must be reconciled by SC-042
with exact stable IDs after the code path is fixed. The 1,122-root audit is implementation
guidance, not permission to mark whole roots manual or inflate the product denominator.

## Cross-family dependency closure

### Definition and array ownership

Public curve/surface definitions are immutable managed records containing copied arrays.
Constructors validate finite points, rectangular grid shape, positive weights, strictly
increasing knots, multiplicity/degree relationships, periodic closure, and parameter
bounds before native mutation. No `TColgp`, `TColStd`, `NCollection`, pole reference, or
native array storage crosses the ABI.

### Native algorithm lifetime

GeomAPI interpolation/approximation/projection/extrema/intersection, GeomFill,
BRepBuilderAPI, BRepOffsetAPI, BRepAlgoAPI/BRepFeat, ShapeAnalysis, and ShapeFix state is
call-local. Results cross only as copied value/diagnostic records or independent registered
owning `Shape` values. Multi-result operations return caller-owned managed collections;
no native iterator or history map escapes.

### Topology and workflow composition

Edges, wires, faces, shells, split pieces, repaired shapes, and solids are independent
owning shapes. Inputs are borrowed only for one validated call and can be disposed after
the result returns. The integration uses existing XDE stable labels, STEPCAF exchange,
viewer parent/thread rules, copied selection/measurement values, mesh snapshots, and
durable screenshot output without changing their ownership categories.

## Validation and completion gates

Batch F reaches 24/24 only when all of these pass together:

- exact SC-042 stable-ID reconciliation for every directly used manual declaration;
- Release and Debug native/managed builds and generator/runtime tests;
- definition/grid/weight/knot/multiplicity/degree validation and immutable edit tests;
- analytic curve/surface interpolation, approximation, extrema, intersection, offset,
  fill, split, loft, sweep, sew, heal, and continuity/error fixtures;
- null/disposed/wrong-kind/invalid-array/algorithm-failure and lifetime tests;
- real STEP/XDE freeform export/reimport with definition/type and topology assertions;
- real-HWND selection/measurement/mesh/screenshot evidence;
- the same complete workflow from the clean application-local 62-DLL package consumer;
- generated dependency closure, freshness, byte-identical clean regeneration, additive
  API compatibility, full inventory, runtime hashes, SBOM/provenance/checksums,
  documentation, and the complete local release check.

Preparation ran no Batch F implementation gate:

| Check | Result |
|---|---|
| API/ABI implementation | NOT RUN |
| Native/managed compile after Batch F changes | NOT RUN |
| Batch F runtime/lifetime/definition tests | NOT RUN |
| Real STEP/XDE plus real-HWND integration | NOT RUN |
| Clean package consumer for Batch F | NOT RUN |
| Full local release check after Batch F implementation | NOT RUN |

## Explicit non-goals

Parametric constraint solving, feature-tree/history persistence, CMM or GD&T judgment,
Class-A surface optimization, subdivision surfaces, point-cloud reverse engineering,
arbitrary drawing/markup authoring, custom rendering, native callbacks, optional IVtk/
VTK/Draw/OpenGL ES profiles, exhaustive GeomFill/BRepFill algorithms, and physical
managed/native/package splitting are outside Batch F.

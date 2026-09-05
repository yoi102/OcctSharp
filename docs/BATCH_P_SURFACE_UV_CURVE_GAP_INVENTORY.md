# Batch P surface UV and curve-on-surface gap inventory

Preparation: **COMPLETE**. Implementation: **0/24, NOT RUN**. This is one indivisible
cross-family wave over the completed Batch O/Preview.14 baseline, not separate surface,
projection, seam, repair, or viewer mini-batches. ADR-0079 owns the accepted boundary.

## Product outcome and existing API reuse

An application can inspect a located analytic or freeform face in UV space, project and
lift copied 2D curves, retain seam branches, build hole-aware trimmed topology, validate
and repair curve representations on independent copies, and exchange/review the results.

The baseline already supplies `Shape.GetFaceSurfaceSnapshot`, `EvaluateFace`,
`EvaluateFaceDerivatives`, `ProjectPointOnFace`, `GetPcurveSnapshot`,
`EvaluatePcurve`, and rectangular `TrimFace`. Batch F supplies freeform definitions,
surface creation, and aggregate topology splitting. Batch O supplies immutable 2D curve
definitions and planar loops. These implementations are dependencies, not missing APIs
to reimplement or count again. In particular, the current face projector returns one
nearest solution plus a count; it does not copy every solution or classify holes.

## Locked 24-capability denominator

| # | Capability and incremental contract | Required exit evidence |
|---:|---|---|
| 1 | Copy a unified face descriptor with analytic/freeform kind, bounded UV domain, orientation, closure, and U/V periods; extend the existing minimal snapshot | Plane, cylinder, sphere, freeform, located and reversed face fixtures |
| 2 | Compose existing point/first-derivative evaluation into bounded batch UV evaluation with explicit parameter-space and location semantics | Native versus normalized UV; invalid ranges; world-coordinate consistency |
| 3 | Report oriented normals and derivative singularities without inventing a normal at a pole or degenerate point | Reversed face, sphere pole, degenerate derivatives, copied result lifetime |
| 4 | Copy principal, mean, and Gaussian curvatures with defined/undefined flags and an explicit face-orientation sign convention | Plane/cylinder/sphere analytic values and freeform curvature |
| 5 | Copy all bounded point-to-surface projection solutions with parameters, points, distance, and face-domain classification | Ordered multiple solutions, hole exclusion, no solution, tolerance and location |
| 6 | Classify UV points as inside/on/outside the actual trimmed face, not only its rectangular parameter bounds | Exterior, boundary, holes, reversed and located faces |
| 7 | Copy a rectangular UV grid containing points, normals, singularity and inside/outside flags | Stable indexing, checked allocation bounds, hole mask, no native view retained |
| 8 | Build independently owning U/V isoparametric edge segments within explicit finite ranges | Analytic/freeform endpoints, orientation, trims, source disposal |
| 9 | Copy a complete bounded existing pcurve definition, including parameter mapping and conversion diagnostics | Analytic/Bezier/B-spline round-trip, unsupported/conversion status, lifetime |
| 10 | Derive a copied pcurve from an existing 3D edge on a face with a declared approximation tolerance and achieved residual | On-surface success, off-surface rejection, located face, unmodified source |
| 11 | Normally project edges or wires onto a face with boundary limiting, maximum-distance, approximation, and multiple-result controls | Trimmed/holed faces, multiple pieces, far projection, owning result cleanup |
| 12 | Lift a Batch O copied UV curve to an independently owning 3D edge on an analytic or freeform face | Plane, cylinder and spline surface; 2D/3D consistency and source disposal |
| 13 | Retrieve both seam pcurve branches with stable branch identity and orientation rather than silently choosing one | Cylinder seam, reversed edge, distinct UV branches with coincident 3D geometry |
| 14 | Normalize periodic UV values and unwrap connected curve/sample sequences with explicit period shifts and seam diagnostics | Positive/negative periods, seam crossing, non-periodic rejection, closed loop ambiguity |
| 15 | Sample a curve-on-surface by 3D arc length, returning copied native parameter, UV, point and tangent records | Cylinder metric differs from UV length, endpoint inclusion, singular/nonuniform speed |
| 16 | Rebuild missing 3D edge curves from pcurves on copied topology, with before/after validity and tolerance diagnostics | Missing-curve fixture repaired while source flags/tolerances remain unchanged |
| 17 | Validate and repair SameParameter/SameRange consistency on copied topology, with bounded tolerance growth | Consistent and inconsistent fixtures, failure report, source independence |
| 18 | Order and orient owning edge copies as a face-supported wire while checking pcurve continuity and gaps | Shuffled/reversed edges, explicit gaps, face membership, independent ownership |
| 19 | Copy oriented UV loops and hole membership from a face, preserving seam occurrences and rejecting ambiguous nesting | Holed plane, periodic face seam, duplicate occurrence identity, lifetime |
| 20 | Build a trimmed owning face on an existing surface from one outer UV loop and holes | Plane/cylinder/freeform area, orientation, non-planar holes, invalid loop rejection |
| 21 | Split a face with supported on-surface edge/wire tools and return owning face pieces plus copied diagnostics | Piece count/area conservation, unchanged source, non-planar split and cleanup |
| 22 | Smoothly interpolate and approximate copied UV B-splines with degree/continuity/tolerance controls | Actual higher-degree interpolation, residual bound, periodicity, invalid input; no degree-one substitute |
| 23 | Preserve named/colored/layered surface-workflow results through XDE and STEP/IGES, retaining copied data and owning results after source disposal | Two-format round-trip and metadata/lifetime assertions |
| 24 | Execute the full surface/UV/projection/repair/exchange workflow in a real HWND and clean packages, without changing physical modules | Selection/screenshot, facade workflow, facade-free direct Modeling consumer, complete local gates |

The denominator is immutable. Reuse rows are accepted only when their new composition,
validation, and lifecycle requirements pass; their existing scalar calls do not imply
Batch P completion. No row is a separate implementation/commit checkpoint.

## Reproducible root audit

Source baseline: Preview.14 at local completion commit `d6e9e18`. Full inventory SHA256:
`176C37BFF338B3E0BA59EFB7CF7BA3803ABC0030B881D0A526873139F89AC2C5`.

The exact 24 decision-driving roots are committed in
`OcctSharp/config/batches/batch-p-surface-uv.json`: `BRepAdaptor_Surface`,
`BRepAdaptor_Curve2d`, `BRep_Tool`, `BRepTools`, `BRepLib`,
`GeomAPI_ProjectPointOnSurf`, `GeomProjLib`, `BRepOffsetAPI_NormalProjection`,
`ShapeConstruct_ProjectCurveOnSurface`, `ShapeAnalysis_Surface`, `ShapeAnalysis_Edge`,
`ShapeFix_Edge`, `ShapeFix_Wire`, `Geom2dAPI_Interpolate`, `Geom2dAPI_PointsToBSpline`,
`Geom2dAdaptor_Curve`, `GCPnts_AbscissaPoint`, `GCPnts_UniformAbscissa`, `Geom_Surface`,
`Geom_Plane`, `Geom_CylindricalSurface`, `Geom_SphericalSurface`,
`BRepBuilderAPI_MakeEdge`, and `BRepBuilderAPI_MakeFace`.

| State | Count | Treatment |
|---|---:|---|
| Blocked | 516 | Candidates only; exact direct calls may later enter SC-053 |
| Emitted | 153 | Reuse accepted generated ownership where appropriate |
| Manual | 31 | Inherited behavior stays attributed to existing special cases |
| Skipped | 263 | Existing language/visibility/ownership exclusions remain |
| Total | 963 | Deduplicated stable IDs, not a product-completion denominator |

Run from the inner workspace:

```powershell
.\eng\audit-batch-roots.ps1 -ConfigPath config/batches/batch-p-surface-uv.json
```

The audit selects exact `NativeName` roots before `::`, rejects a changed inventory hash,
duplicate roots or IDs, missing roots, incomplete classification, and overwriting inputs.
It changes no binding dispositions. Two actual runs produce identical report SHA256
`D0B99F166A8686CE5312CB81B42E0A04DC05D3C26241596CBD5D8919143A2886`.
All 24 headers exist in the pinned OCCT 8.0.1 SDK. Wrong-inventory and input-overwrite
negative checks also pass without changing either input. The audited inventory remains a local
artifact; its hash and root config make the preparation reproducible and fail closed.

## Full dependency and ownership closure

- Geometry: native-local `BRepAdaptor_Surface`, `BRepAdaptor_Curve2d`, `Geom_Surface`,
  `Geom2d_Curve`, `GeomLProp_SLProps`, `gp_*`, `Geom2dConvert`, interpolators and
  projectors. Shape location is applied exactly once; UV is surface-local and copied 3D
  points/vectors are world-space. Existing `Geom2d` shared wrappers are not friendly
  mutable state. Unsupported exact curve forms must be converted with explicit
  approximation/residual diagnostics or rejected, never silently replaced by a polyline.
- Topology: `BRep_Tool`, `BRepTools`, `BRepTools_WireExplorer`, `BRepClass_FaceClassifier`,
  `BRepBuilderAPI_MakeEdge/MakeWire/MakeFace/Copy`, `BRepLib`, `BRepFeat_SplitShape` or
  the inherited splitter, and `TopExp`/`TopoDS` support. Seam branches are occurrences,
  not deduplicated by edge identity. Periodic UV shifts and hole classification cannot
  be inferred from a sampled bounding rectangle.
- Projection/healing: `GeomAPI_ProjectPointOnSurf`, `GeomProjLib`, normal projection,
  `ShapeConstruct_ProjectCurveOnSurface`, `ShapeAnalysis_Surface/Edge`, and
  `ShapeFix_Edge/Wire` are call-local. Builders report failures explicitly. Healing
  operates on geometry/topology copies before changing flags, curves or tolerances;
  wrapper ownership alone does not make the shared OCCT TShape safe to mutate.
- Measurement: GCPnts/adaptors and native surface derivatives measure 3D arc length;
  copied grids/samples include singularity/domain status. Counts use overflow-checked
  allocations and count/copy contracts. Native and normalized parameter conventions
  are explicit and preserve reversed curves.
- Managed code: proposed copied surface/UV DTOs and a cross-family `SurfaceModeling`
  facade compose existing Modeling `Shape`, Batch F, and Batch O. No new project,
  native DLL, registry, allocator, resolver, or alternate ownership model is added.
  DTO snapshots retain no native parent. Topology results have independent registered
  owners, with all already-created results released if a multi-result call fails.
- Integration: existing XDE parent-bound labels and owned documents, STEPCAF/IGESCAF
  path staging, viewer-parent-bound presentation IDs, and creating-thread rules remain.
  Metadata round-trips and source/document-disposal tests are required, not implied.
- Toolkit closure: use the existing TKMath/TKG2d/TKG3d/TKGeomBase/TKGeomAlgo/TKBRep/
  TKTopAlgo/TKShHealing/TKBO/TKBool/TKOffset plus XDE/exchange/viewer runtime graph.
  The 24 roots are an audit anchor, not an assertion that support classes are absent.
  Every newly invoked blocked support declaration must also receive an exact SC-053 ID.

## Identity, gates, and explicit limits

Reserve package `8.0.1-preview.15`, ABI 1.59, bridge 0.67.0, and schema 1.13. These are
preparation reservations only: current build/package/runtime identities remain Preview.14,
ABI 1.58, and bridge 0.66.0 until implementation begins. SC-053 is reserved, not registered
as implemented; no candidate is promoted by this document.

Required gates: the complete 24-row matrix; exact stable-ID audit; Release/Debug native
and managed builds; Generator/Runtime suites plus the real native Debug sweep; analytic,
periodic, seam, holed, singular, freeform and located/reversed fixtures; invalid inputs,
copy isolation and lifetime; real XDE/STEP/IGES/HWND; both local package consumers;
generated closure/determinism/freshness/clean regeneration; additive API comparison;
full inventory; runtime/package hashes; SBOM/provenance/checksums; docs and Git checks.
Implementation, compile, runtime, and Preview.15 package gates are **NOT RUN**.

Preparation only validates the baseline audit and its reproducibility. Completion will
require one local Batch P implementation commit after all gates pass. NuGet publication
and GitHub push are not batch work. Parametric constraints, arbitrary unbounded domains,
global UV atlas/unwrapping, geodesics, remeshing/flattening, cross-platform rendering,
D3DImage, callbacks, and physical native splitting are outside Batch P.

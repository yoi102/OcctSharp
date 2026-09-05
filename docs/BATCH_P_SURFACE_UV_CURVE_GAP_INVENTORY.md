# Batch P surface UV and curve-on-surface gap inventory

Preparation: **COMPLETE**. Implementation: **COMPLETE (32/32)**. This is one indivisible
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

## Locked 32-capability denominator

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
| 25 | Intersect a supported face with an explicit plane and retain owning section edges with face pcurves | Trimmed domain, section geometry, source disposal |
| 26 | Intersect two trimmed analytic/freeform faces with owning section curves and support pcurves | Plane/cylinder and freeform cases, no intersection, source independence |
| 27 | Copy 3D curve/surface intersection points and coincident intervals with native curve parameters and UV witnesses | Point and overlap results, holes, located faces, bounded intervals |
| 28 | Project a copied batch of 3D points with stable source indices and all per-point solutions | Empty/no-solution groups, ordering, bounded allocation and atomic failures |
| 29 | Summarize each oriented boundary loop with 3D perimeter, copied UV area, outer/hole and seam occurrence counts | Planar holes versus curved metric, seam and degenerate-edge diagnostics |
| 30 | Construct explicitly bounded plane/cylinder/sphere/cone/torus faces from copied frame and analytic parameters | Finite ranges, radii/angles, placement, topology and evaluation |
| 31 | Offset a copied UV curve with tolerance/residual diagnostics, then lift it onto a surface | UV-unit contract, exact/approximated geometry, source independence |
| 32 | Trace projected point sequences continuously across periodic seams using copied UV shifts and residual diagnostics | Cylinder seam crossing, failed point groups, preserved sequence order |

The original preparation committed at `72854bd` covered 24 capabilities. Before any
implementation, the user's explicit request for a broader wave expands the denominator
to 32 capabilities under ADR-0080. The expanded denominator is now immutable. Reuse rows are accepted only when their new composition,
validation, and lifecycle requirements pass; their existing scalar calls do not imply
Batch P completion. No row is a separate implementation/commit checkpoint.

## Reproducible root audit

Source baseline: Preview.14 at local completion commit `d6e9e18`. Full inventory SHA256:
`176C37BFF338B3E0BA59EFB7CF7BA3803ABC0030B881D0A526873139F89AC2C5`.

The exact 32 decision-driving roots are committed in
`OcctSharp/config/batches/batch-p-surface-uv.json`: `BRepAdaptor_Surface`,
`BRepAdaptor_Curve2d`, `BRep_Tool`, `BRepTools`, `BRepLib`,
`GeomAPI_ProjectPointOnSurf`, `GeomProjLib`, `BRepOffsetAPI_NormalProjection`,
`ShapeConstruct_ProjectCurveOnSurface`, `ShapeAnalysis_Surface`, `ShapeAnalysis_Edge`,
`ShapeFix_Edge`, `ShapeFix_Wire`, `Geom2dAPI_Interpolate`, `Geom2dAPI_PointsToBSpline`,
`Geom2dAdaptor_Curve`, `GCPnts_AbscissaPoint`, `GCPnts_UniformAbscissa`, `Geom_Surface`,
`Geom_Plane`, `Geom_CylindricalSurface`, `Geom_SphericalSurface`,
`BRepBuilderAPI_MakeEdge`, `BRepBuilderAPI_MakeFace`, `Geom_ConicalSurface`,
`Geom_ToroidalSurface`, `GeomAPI_IntCS`, `GeomAPI_IntSS`, `Geom2d_OffsetCurve`,
`Geom2dConvert`, `BRepAlgoAPI_Section`, and `BRepGProp`.

| State | Count | Treatment |
|---|---:|---|
| Blocked | 608 | Candidates only; exact direct calls may later enter SC-053 |
| Emitted | 204 | Reuse accepted generated ownership where appropriate |
| Manual | 41 | Inherited behavior stays attributed to existing special cases |
| Skipped | 325 | Existing language/visibility/ownership exclusions remain |
| Total | 1,178 | Deduplicated stable IDs, not a product-completion denominator |

Run from the inner workspace:

```powershell
.\eng\audit-batch-roots.ps1 -ConfigPath config/batches/batch-p-surface-uv.json
```

The audit selects exact `NativeName` roots before `::`, rejects a changed inventory hash,
duplicate roots or IDs, missing roots, incomplete classification, and overwriting inputs.
It changes no binding dispositions. Two actual runs produce identical report SHA256
`A8E6C84A4E6333E54EDD9E9E0BE657F7BAF6EB64C434ECD1228D89F3B726A955`.
All 32 headers exist in the pinned OCCT 8.0.1 SDK. Wrong-inventory and input-overwrite
negative checks also pass without changing either input. The audited inventory remains a local
artifact; its hash and root config make the preparation reproducible and fail closed.

After implementation, use `-InventoryPath` to point at the frozen Preview.14 inventory,
not the newly classified Preview.15 inventory. This run preserves that local baseline at
`artifacts/generator-reports/batch-p-baseline-inventory.json`.

## Executable acceptance map

The 13 facts in `BatchPCompletionTests` cover the whole matrix, not separate batches:

| Matrix rows | Primary regression evidence |
|---|---|
| 1-4, 30 | Analytic frames/derivatives/curvature/singular charts; located topology |
| 5-7, 28-29 | Holed-domain projection/grid/boundary metrics; periodic/degenerate controls |
| 8-10, 12, 15 | Lifted/copied/derived curves, iso edges and 3D arc-length composition |
| 13-14, 32 | Seam branches, periodic shifts and continuous point traces |
| 16-17 | Missing-3D reconstruction and deliberately inconsistent BREP flags; source isolation |
| 18-21 | Shuffled wires, non-planar holes, split count/area/validity and source independence |
| 22, 31 | Smooth/periodic fitting, measured residuals, continuity, offsets and freeform faces |
| 11, 25-27 | Normal projection controls, sections and bounded point/holed-overlap intersections |
| 23-24 | Shared public-only STEP/IGES metadata, lifetime, real-HWND selection/screenshot workflow |
| All input boundaries | Wrong-kind, disposed, invalid range/count/enum and failed-result cases |

The shared workflow is compiled by both Runtime.Tests and PackageConsumer. The direct
Modeling consumer separately proves that the physical module/runtime graph stays facade-free.

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
- Managed code: copied surface/UV DTOs and a cross-family `SurfaceModeling`
  facade compose existing Modeling `Shape`, Batch F, and Batch O. No new project,
  native DLL, registry, allocator, resolver, or alternate ownership model is added.
  DTO snapshots retain no native parent. Topology results have independent registered
  owners, with all already-created results released if a multi-result call fails.
- Integration: existing XDE parent-bound labels and owned documents, STEPCAF/IGESCAF
  path staging, viewer-parent-bound presentation IDs, and creating-thread rules remain.
  Metadata round-trips and source/document-disposal tests are required, not implied.
- Toolkit closure: use the existing TKMath/TKG2d/TKG3d/TKGeomBase/TKGeomAlgo/TKBRep/
  TKTopAlgo/TKShHealing/TKBO/TKBool/TKOffset plus XDE/exchange/viewer runtime graph.
  The 32 roots are an audit anchor, not an assertion that support classes are absent.
  Every newly invoked blocked support declaration must also receive an exact SC-053 ID.

## Identity, gates, and explicit limits

The implementation uses package `8.0.1-preview.15`, ABI 1.59, bridge 0.67.0, and schema
1.13. SC-053 registers 100 exact directly invoked blocked IDs from the baseline inventory,
including support declarations outside the 32-root audit. No root or overload family
is bulk-promoted; inherited Manual/Emitted/Skipped attribution stays unchanged.

Required gates: the complete 32-row matrix; exact stable-ID audit; Release/Debug native
and managed builds; Generator/Runtime suites plus the real native Debug sweep; analytic,
periodic, seam, holed, singular, freeform and located/reversed fixtures; invalid inputs,
copy isolation and lifetime; real XDE/STEP/IGES/HWND; both local package consumers;
generated closure/determinism/freshness/clean regeneration; additive API comparison;
full inventory; runtime/package hashes; SBOM/provenance/checksums; docs and Git checks.
All implementation and local validation gates pass. Focused regression is 13/13;
Release/Debug Generator is 91/91 and Runtime is 177/177, including an isolated run
against actual Debug native binaries. Both clean consumers and the complete release
check pass; all 94 generated files remain byte-identical after clean regeneration.
API comparison against alpha.38 is additive at 39,281 additions and zero removals.
The final inventory has 16,353 Emitted, 709 Manual, 49,866 Blocked and 49,344 Skipped
declarations, with zero pending/HD099. Its SHA256 is
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.
The native bridge SHA256 is
`2230D43CC32F749615A2202EAA2FB8891BB9D4EC09345B3FEB1E165C75C91710`.
Final package hashes and completion evidence are recorded in STATUS.

Preparation validated the baseline audit and its reproducibility before implementation.
The complete wave is delivered in one local Batch P implementation commit. NuGet publication
and GitHub push are not batch work. Parametric constraints, arbitrary unbounded domains,
global UV atlas/unwrapping, geodesics, remeshing/flattening, cross-platform rendering,
D3DImage, callbacks, and physical native splitting are outside Batch P.

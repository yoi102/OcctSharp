# Batch AG: surface grids and interpolation

Baseline: Preview.30 / ABI 1.74 / bridge 0.82.0. Candidate identity:
Preview.31 / ABI 1.75 / bridge 0.83.0. Work is authorized by the request to execute
the next batch. [ADR-0098](adr/0098-generated-surface-grids-and-retained-interpolation.md)
defines the ownership extension. [STATUS](STATUS.md) owns current gate results.

## Exact scope

The [frozen 83-candidate list](BATCH_AG_SURFACE_INTERPOLATION_CANDIDATES.json)
admits 63 Blocked declarations and 18 supporting Manual entries across B-spline
surfaces, Bezier surfaces, point-to-surface fitting and 2D/3D interpolation.
The two nullable Weights() array pointers remain excluded. Manual entries are
supported by the generated workflow, retain their inventory state and do not
inflate the migration count. Selection is checked against exact stable IDs and
native signatures in grid-collection-operation-contracts.json.

New profiles are Array2<gp_Pnt/double>, Array1<gp_Vec/gp_Vec2d/bool> and retained
HArray1<gp_Pnt/gp_Pnt2d/double/bool>. Existing AF helper identities remain unchanged.
CopiedGrid<T> copies row-major data; CopiedArray<T> supplies tangents and flags.
Boolean data crosses the raw ABI as checked bytes 0/1. All elements are copied
into owning native storage; no external buffer constructor is selected.

## Audited kernel boundaries

- Grid dimensions are positive. Check endpoint arithmetic, rows*columns and the
  signed intermediates in Array2 BeginPosition and LastPosition before allocation.
  Constructors also check the normalized one-based output grid; knot insertion
  checks a conservative pole-growth/product bound. Interpolation reserves count
  space for every active tangent to add a pole and for expanded knot endpoints.
  SDK hashes and upstream V8_0_1 implementations are retained in batch evidence.
- Surface constructors accept safe shifted grid bounds and normalize stored poles
  to one-based indices. Surface fitting requires one-based input grids because its
  MultiLine and constraint tables are indexed directly with those indices.
- Copy-out grids report the actual replacement bounds supplied by the kernel;
  assigning its internal pole/weight grid resets those bounds to one. Failed calls
  publish no managed ref replacement. Native receiver mutation is not rolled back.
- Pole row/column edits may cover a bounded subrange. Bezier SetWeightCol requires
  a complete one-based column in the pinned implementation. Before insertion
  delegates to After(index-1), making its effective accepted range 2..count+1.
  After accepts 1..count. These bounds are checked before the kernel.
- Periodic interpolation omits the repeated first point and needs one extra
  parameter. Points, tangents and flags are one-based; parallel counts and active
  tangent magnitudes are checked before Load. Algorithms own independent HArray
  copies, so temporary managed inputs and native factory tokens may be released.
- The pinned scalar height fitter yields NaN for an exactly constant height grid
  despite IsDone. SC-066 supplies an exact planar B-spline with physical X/Y domains,
  degree elevation to the requested minimum (cubic for interpolation), and independent
  result ownership. Nonconstant grids use the pinned algorithm. Reinitialization
  clears the fallback and preserves previously returned surfaces.

## Workflow and validation contract

Shared public-only consumer workflow constructs an asymmetric surface, evaluates it,
composes its copied definition into a face and roundtrips STEP. It also fits a surface
and interpolates 2D/3D constrained curves, disposing algorithms before composing copied
curve definitions into owning edges. The same workflow is compiled into Runtime tests,
the facade package consumer and the direct module package consumer.

Existing FreeformAuthoring/FreeformTypes are placed in Modeling and geometry-only
ShapeExchange/StepReadResult/StlWriteOptions in DataExchange. Their 17 public types
are forwarded through the facade with unchanged signatures and native contracts.
The shared workflow checks assembly ownership and absence of a reverse facade
dependency; the direct consumer also rejects a deployed OcctSharp.dll.
An unchanged consumer compiled against Preview.30 resolves all 17 former facade
types with Preview.31 modules and completes owning freeform and STEP workflows.
This binary check complements aggregate signature compatibility.

The public copied grid can use safe shifted bounds for surface construction:

```csharp
using OcctSharp.CopiedCollections;
using OcctSharp.Values;

var grid = new CopiedGrid<Coordinates3d>(
    [new(0, 0, 0), new(0, 1, .2), new(0, 2, 0),
     new(2, 0, 0), new(2, 1, .4), new(2, 2, 0)],
    rows: 2, columns: 3, rowLower: -2, columnLower: 4);
using var surface = GeomBezierSurfaceCollections.Create(grid);
var snapshot = GeomBezierSurfaceCollections.Poles(surface);
surface.Dispose();
// The returned snapshot remains independent; stored surface bounds are one-based.
Coordinates3d pole = snapshot[2, 3];
```

For fitting, supply one-based grids. Interpolation also takes one-based copied
arrays; call Perform, check IsDone, and obtain Curve before disposing its algorithm.
The returned owning geometry can then be used independently. The shared consumer
workflow covers face/edge composition and exchange as well as these input boundaries.

Focused tests cover rational grids, shifted bounds, all bulk editing/reading families,
periodic seams, smoothing/height fitting, retained tangent flags, malformed dimensions,
raw output capacity, invalid bool values, stale/wrong-type tokens, independent results
and repeated/concurrent disposal. Delivery requires full Release/Debug suites, actual
Debug-native tests, cold regeneration, old-file/API/export preservation, exact inventory,
strict C ABI/private headers, package/payload checks and the unified local release gate.

## Local delivery acceptance (2026-10-02)

| Check | Executed result |
|---|---|
| Exact migration/accounting | PASS 63 Blocked -> Emitted, 18 supporting Manual entries unchanged, two nullable pointers excluded; all other dispositions and 32 parser exclusions preserved. |
| Full Release and Debug | PASS 205/205 Generator and 655/655 Runtime per configuration; the XDE companion contributes six Runtime cases separately. |
| Isolated actual Debug-native | PASS 655/655 and all 62 DLL hashes. |
| Cold source build/regeneration | PASS full suites and byte-identical manifest plus all 169 generated files. |
| Preservation and compatibility | PASS old 25,935 IDs/162 files; +104 native/+100 managed AG additions, companion +1/+1, zero removals; aggregate 41,073 exports and 55,672 signatures. |
| Module/binary compatibility | PASS 4,150 facade forwarders, acyclic modules and unchanged Preview.30 binary resolving 17 moved public types plus freeform/STEP workflows. |
| ABI and lifetime | PASS 44 C11 headers, 34 geometric layouts, 44 private headers, strict standalone grid support; 250 release races/16,000 acquisitions per configuration. |
| Packages and workflows | PASS 14 packages, 62 native DLLs, 13 managed assemblies; facade and direct module consumers, zero native duplication and all payload hashes. |
| Final local engineering gate | PASS generated freshness, exact full inventory, notices/document content, provenance/SBOM/checksums and unified release-check. |
| External/human gates | NOT RUN hosted CI, signing, push, publication, full downstream app acceptance, existing mixed-DPI/RDP and physical pre-repaint viewer observations. |

The final source/DLL identities and detailed checks are retained in
`OcctSharp/artifacts/batch-ag/result.json` and the companion evidence files.
Original concurrent-link and stale-consumer/module-placement failures are retained
separately; the successful final unified run is `release-check-final3.log`.
Delivery here is the validated local Preview.31 candidate; public release readiness
remains false. The 63 declaration count does not include Manual support, existing
public types moved between assemblies or the independently developed XDE companion.

## Following batches

AH groups bulk analysis, conversion and approximation into a full sampled/error-data
workflow. Its [184 exact candidates](BATCH_AH_BULK_ANALYSIS_CANDIDATES.json) cover
138 BSplCLib, 36 BSplSLib, 5 GeomConvert and 5 Geom2dConvert declarations. These
are audit candidates, not admitted or implemented declarations. The
[AH execution plan](BATCH_AH_BULK_ANALYSIS_PLAN.md) defines the stages and exit gates. Bind them to the
AG exit inventory, audit output counts/aliasing and geometric sequence lifetime,
then close conversion -> measured approximation -> edited geometry -> exchange in
both consumers. Large coherent scope is preferred when ownership and size contracts
support it; no declaration quota substitutes for those audits.

AI follows with topology/exchange collections, typed element ownership, null policy
and independent shape sets/history. Keep every batch's delivery gate complete before
expanding implementation. Parser fixes remain demand-driven and evidence-bound.

# Batch AF: copied arrays and sequences

Baseline: completed local Preview.29. Delivered local candidate: additive Preview.30,
ABI 1.74, bridge 0.82.0. Full local engineering validation completed on 2026-10-02.

## Frozen scope

The authoritative exact signatures and stable IDs are in
[the generator catalogue](../OcctSharp/src/OcctSharp.Generator/Transformation/copied-collection-operation-contracts.json).
The entry audit and frozen Preview.29 generated files, DLLs, API and inventory are
under `OcctSharp/artifacts/batch-af/preview29-baseline/` and `selected-declarations.json`.

| Family | Blocked declarations selected | Useful outcome |
|---|---:|---|
| Geom_BSplineCurve / Geom2d_BSplineCurve | 22 | Copied poles/knots/multiplicities, ref output buffers, set/insert knots |
| Geom_BezierCurve / Geom2d_BezierCurve | 12 | Copied definitions; construction uses separately counted supporting entries |
| GeomAPI_PointsToBSpline / Geom2dAPI_PointsToBSpline | 18 | Empty algorithm, explicit parameterization, supplied parameters, smoothing and repeated Init |
| BSplCLib | 7 | Copied reversal, reparameterization and bounded multiplicity inspection |
| GeomLib | 1 | Array input to copied sequence output through interval merging |
| Total | 60 | One linked curve-definition/edit/fitting workflow across eight interface families |

Fourteen existing Manual curve/fitting operations supply compatible constructor/status/result
entries. They retain their dispositions. Factories, releases and snapshot utilities
are supporting infrastructure, not migrated OCCT declarations.

Closed element profiles, dimensions, copying, ownership and publication are specified
by [ADR-0097](adr/0097-generated-copied-arrays-and-sequences.md). Nullable array
pointers, arbitrary templates, retained addresses, HArray and Array2 are excluded.

## API example

```csharp
using OcctSharp.CopiedCollections;
using OcctSharp.Values;

var poles = new CopiedArray<Coordinates3d>(
    [new(0, 0, 0), new(1, 2, 0), new(3, 0, 0)]);
using var curve = GeomBSplineCurveCollections.Create(
    poles, new([0, 1]), new([3, 3]), 2, false);
var snapshot = GeomBSplineCurveCollections.Poles(curve);
curve.Dispose();
var independentPoints = snapshot.ToArray();
```

Compatible curve constructors above remain Manual in inventory. Snapshot, editing,
fitting and interval operations use the exact generated catalogue. Invalid domain
arguments produce ArgumentException; kernel failures produce OcctException. Caller
synchronization is required for native receiver mutation.

## Delivery plan

1. Freeze baseline and exact catalogue; audit pinned SDK headers and preconditions.
2. Implement reusable container support and independent operation emission.
3. Regenerate; test semantics, errors, lifetime and success-only ref publication.
4. Exercise both package consumer paths and preserve previous generated/API/ABI
   identities; run Release/Debug, actual Debug native and cold source regeneration.
5. Refresh full inventory and all packages; record the unified local release gate
   and separate external acceptance statuses. Only then select the next migration.

## Executed delivery evidence

Evidence is retained under `OcctSharp/artifacts/batch-af/`; `result.json` binds it to
the uncommitted source snapshot. `release-check.log` is the completed unified run;
earlier failed/interrupted attempts remain separate diagnostic records.

| Check | Actual result |
|---|---|
| Release and Debug builds/suites | PASS; Generator 197/197 and Runtime 625/625 in each configuration |
| Isolated actual Debug-native | PASS; Runtime 625/625, all 62 DLL hashes verified |
| Clean source regeneration | PASS; 197/197 and 625/625; 162 generated files and manifest byte-identical |
| ABI and lifetime | PASS; 42 strict C11 headers, 34 geometric size/offset assertions; 250 release races / 16,000 acquisitions per configuration |
| Prior generated source | PASS; 25,875 IDs, 155 complete files and 40,103 function bodies/prototypes preserved |
| Native and managed compatibility | PASS; native 40,968 exports (+96/-0), Debug=Release; managed 55,571 signatures (+96/-0 versus Preview.29); alpha.38 removals 0 |
| Full inventory | PASS; 116,272 declarations; 25,935 Emitted, 1,218 Manual, 39,772 Blocked, 49,347 Skipped; Pending 0; exact 60-ID transition; 32 exclusions unchanged |
| Packages and consumers | PASS; 14 packages, two clean consumers, 62 native / 13 managed payloads with matching hashes, no native duplication |
| Bundled sample, closure and release gate | PASS; correct ABI/bridge smoke, acyclic 28-edge closure, 4,127 current forwarders, complete local release check |
| Final documentation and package identity | PASS; source-matched stable documents, candidate JSON, notices/licenses, provenance/SBOM and checksums |

Arrays reserve the signed integer endpoint indices because the pinned kernel's
intermediate Upper calculation and loop increments/decrements must remain safe.
Managed and raw guards reject those endpoints; extrema accepted by the contract
are covered by reversal, reparameterization and range-query regressions.

Hosted CI, signing, push and public NuGet publication are NOT RUN. Existing mixed-DPI,
RDP, physical pre-repaint observations and full Cadoryx application acceptance against
Preview.30 remain NOT RUN. Local gate completion does not establish public readiness.

## Next broad batch: AG surface grids and interpolation

AG groups related families into one complete input-to-result workflow. The
[exact candidate list](BATCH_AG_SURFACE_INTERPOLATION_CANDIDATES.json) has 83 entries:
65 currently Blocked and 18 compatible Manual prerequisites. These are planned
candidates, not admitted/emitted declarations; nullable array-pointer results remain
subject to exclusion unless a specific optional-copy contract is accepted.

| Next family | Blocked candidates | Manual supporting entries |
|---|---:|---:|
| Geom_BSplineSurface | 27 | 2 |
| Geom_BezierSurface | 25 | 2 |
| GeomAPI_PointsToBSplineSurface | 9 | 4 |
| GeomAPI_Interpolate | 1 | 6 |
| Geom2dAPI_Interpolate | 3 | 4 |

The grouped scope includes:

- Array2<gp_Pnt>, Array2<double> and HArray1<gp_Pnt/gp_Pnt2d/double> projections.
  Define row/column lower bounds, row-major transfer, checked dimension products,
  empty/null policy and copy timing first. HArray inputs must retain an independent
  native owner when an algorithm stores the handle beyond a call.
  Preserve AF helper exports and profile identities/order when appending new profiles.
- Tangent Array1<gp_Vec/gp_Vec2d> and HArray1<bool> accompany interpolation constraints.
  Normalize bool elements explicitly across the C ABI and validate parallel point,
  tangent and flag counts before Load; do not marshal native bool storage directly.
- Geom_BSplineSurface and Geom_BezierSurface constructors, copied pole/weight grids,
  U/V knot/multiplicity data and bulk edits; extend to matching 2D/3D interpolation
  through GeomAPI_Interpolate and Geom2dAPI_Interpolate.
- GeomAPI_PointsToBSplineSurface initialization/interpolation, reinitialization,
  status and surface results. Reuse AF Array1, immutable snapshots and publication
  helpers; audit Array2 access order and output shape for each exact overload.
- Validate grid -> surface -> evaluation/face -> STEP roundtrip, and interpolation
  -> independent curve -> edge workflows in the two clean consumers. Include
  non-square grids, shifted bounds, rational weights, periodic seams, mismatched
  dimensions, overflow and post-input-disposal results.

The pinned SDK headers confirm that GeomAPI_Interpolate and Geom2dAPI_Interpolate
retain myPoints, myParameters, myTangents and myTangentFlags as HArray handles.
AG therefore needs independent owning HArray copies retained by the algorithm,
rather than AF's call-local Array1 temporaries. NCollection_HArray1(Array1) deep
copies through Array1's owning copy constructor; its external-buffer constructor
must stay excluded. Retain HArray through an OCCT owning handle; the registry may
own a wrapper containing that handle. Never independently shared_ptr-delete the
same transient object held by opencascade::handle. NCollection_Array2 also provides non-owning buffer-reuse
constructors. Use owning allocation and validate BeginPosition/LastPosition offset
arithmetic as well as the row/column product and upper bounds. Test row-major
ordering against native Value(row, column) with a non-square asymmetric grid.

Entry deliverable: exact Blocked stable-ID/signature inventory across these families,
supporting Manual entries separately counted, SDK symbol and retained-handle audit,
accepted ownership extension and complete module dependency closure. Aim for a
larger useful cross-family closure than AF where that audit supports it; do not
promise an unaudited number. AG begins after AF's local gates pass.

## Subsequent batches

AH: bulk curve/surface analysis and approximation. Audit BSplCLib/BSplSLib sized
mutable outputs, GeomConvert/Geom2dConvert multi-result collections and geometric
sequence elements. Counts must come from documented algorithms, never array-start
references. Exit with approximation -> sampled/error data -> edited geometry and
exchange workflows plus error/capacity/publication evidence.

AI: topology and exchange collections. Audit sequence/list of TopoDS and shared
entity handles, deep value snapshots versus shared topology identity, null elements,
typed element disposal, and cross-module ownership. Exit with shape sets/history ->
repair/exchange and package workflows. These element lifetimes need an ADR beyond
AF's unmanaged values.

Parser/symbol triage proceeds only when a selected workflow exposes a real blocker:
record reproducible causes for the 32 excluded headers, fix reusable discovery
issues, and rerun inventory. Missing SDK symbols remain explicitly blocked.

All batches finish their required local delivery gates before the next expansion.
Public NuGet, push, signing and hosted CI require their own execution evidence.

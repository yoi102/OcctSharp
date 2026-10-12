# OcctSharp 8.0.1-preview.31

Batch AG adds generated copied surface grids, surface fitting and constrained 2D/3D
interpolation across five families. Exactly 63 Blocked declarations migrate to Emitted;
18 compatible Manual prerequisites remain separately counted. Two nullable Weights
pointer overloads remain excluded; use the independent WeightsArray snapshots.

CopiedGrid<T> stores immutable row-major data with explicit native bounds and checked
dimension/offset arithmetic. Retained HArray inputs own independent copies, including
parameters, tangents and Boolean flags. Returned curves/surfaces outlive algorithms.
The pinned constant-height fitter defect is handled with an exact planar B-spline;
other kernel boundaries and partial-edit limits are documented in
[Batch AG](BATCH_AG_SURFACE_INTERPOLATION.md).

Existing freeform composition and geometry-only exchange APIs now reside in their
Modeling/DataExchange assemblies, enabling the same complete workflow without the
facade package. Seventeen moved public types retain facade compatibility through
generated forwarders; public signatures and native ownership remain unchanged.

The shared working tree also contains the separately developed XDE transformed
definition-copy companion. Its one native export, one managed method and six
Runtime tests are accounted separately from AG in delivery evidence.

Native identity: ABI 1.75 / bridge 0.83.0, OCCT 8.0.1, .NET 10, Windows x64.
The module graph, facade and one shared native bridge remain. Preview.30 and earlier
packages retain their identities. Current executed local gates and external statuses
are recorded in [STATUS](STATUS.md). Hosted CI, signing, push and public publication
are not performed by this local batch.

AH is planned as a broad bulk-analysis/conversion/approximation workflow with
[184 exact audit candidates](BATCH_AH_BULK_ANALYSIS_CANDIDATES.json). Candidate inventory
is not an admission or implementation count.

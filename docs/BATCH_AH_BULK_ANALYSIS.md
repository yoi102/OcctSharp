# Batch AH: sized bulk analysis and independent curve conversion

Baseline: validated Preview.31 / ABI 1.75 / bridge 0.83.0. Candidate:
Preview.32 / ABI 1.76 / bridge 0.84.0. [ADR-0099](adr/0099-generated-bulk-analysis-and-typed-curve-collections.md)
governs the new boundary. STATUS owns current delivery results.

## Admission and useful outcome

The [184-candidate audit](BATCH_AH_BULK_ANALYSIS_CANDIDATES.json) admits **42 exact
Blocked declarations**: 23 BSplCLib, nine BSplSLib, five GeomConvert and five
Geom2dConvert operations. The [complete admission matrix](BATCH_AH_BULK_ANALYSIS_ADMISSION.json)
records exact signatures, pinned source/function locations and hashes, dimensions,
aliasing, retention, errors and concrete deferral reasons for all 142 remaining rows.
There are no newly counted Manual prerequisites. Source/link evidence remains in
`OcctSharp/artifacts/batch-ah/`.

The batch combines knot and pole counts, knot form and range analysis, expanded /
compressed knot data, Schoenberg parameters, trimming and degree/preperiodic
preparation, packed surface data, coefficient conversion and typed spline splitting /
concatenation. Existing owning geometry editors/evaluators close degree increase,
knot insertion, approximation and sampled position/derivative error, then independent
edge/face construction and STEP exchange. The identical public-only workflow runs
through Runtime, facade and direct module consumers.

## Numeric contract

AH bulk arrays/grids are explicitly one-based. Existing AF/AG shifted-bound APIs
retain their accepted behavior. Require exact output shapes, matching linked bounds,
positive multiplicities, finite values, degrees 1..25 and checked arithmetic before
kernel calls. Reject mismatched capacities before touching any caller-visible data.
Multiplicity sums reserve headroom for degree expansion. Hunt midpoint and reverse
temporary offsets/intermediate sums are checked separately. Mutable scalars and arrays
are private input copies; all output preparation precedes publication. Aliased raw
output addresses are rejected. Managed refs are replaced only after success.
Metadata scalar slots cannot overlap each other or the index buffer. Invalid storage
aliases fail before touching the slots; ordinary capacity failures leave the buffer
untouched and reset separate count/closure outputs.

Packed surface poles use exactly rows*columns*3 values, or *4 for homogeneous data;
the selected U/V order is preserved. Rational unpacking checks every weight before
division. Return values from UnitWeights may reference static SDK storage: copy
them immediately into independent owning Array1/Array2 storage and expose immutable
snapshots. Outputs do not share the kernel's static data.

Pinned rationality predicates retain native behavior: BSplCLib compares exact
weights and ignores epsilon; BSplSLib compares U-neighbors within the supplied ranges.
For BSplSLib require positive epsilon, avoiding its fallback's unusual Weights(I1,I2)
access. Call the corresponding owning geometry predicates for overall surface
rationality; this low-level predicate does not establish both directional flags.
The no-degree KnotSequence overload admits nonperiodic expansion only; periodic
expansion uses the explicit-degree overload and a validated full degree of poles.

## Curve element ownership

OwnedCurveSet2d/3d stores only typed deep-copied splines, via OCCT intrusive handles
inside a leased registry owner. Null elements and empty input sets fail. GetCurve
uses a zero-based index and returns an independent deep owning copy that the caller
disposes. Factories retain every input SafeHandle until native copying completes.

Split clones its source even when the SDK singleton branch returns that same input.
Concat requires at least two curves and exactly n-1 finite, nonnegative junction
tolerances. Convert into the pinned zero-based native arrays. The result owner
retains updated private inputs, output curves, C1 indices (empty for G1) and the
returned closure flag. InputCurves and Curves return independent owners; each
GetCurve result survives both set/result disposal. No caller source curve is mutated.
Raw result tokens start null; failed calls publish none. All exceptions stay in the C ABI.

## Verification and remaining boundaries

Meaningful cases cover nonuniform/rational/periodic data, both packing directions,
strict junction tolerance failure, reversed/disconnected curves, singleton and corner
splits, every C1/G1 overload in both dimensions, rational periodic joins, invalid
dimensions/bounds/weights/counts, alias copies, wrong/stale tokens, insufficient raw
metadata capacities, independent outputs, GC and repeated concurrent disposal.
The workflow measures sampled position and first-derivative error for representation
edits and approximation, and composes independent CAD geometry for exchange.

The manifest adds 42 IDs and six files while preserving all previous 25,998 IDs and
169 complete files. Product/binding delivery requires full Release/Debug builds and
suites, actual Debug-native isolation, cold regeneration, API/exports/module closure,
strict C ABI/lifetime checks, fresh exact inventory, 14 packages and both clean
consumers, payload/source/document/provenance identities and the unified release check.
The final isolated source completes these local gates at Preview.32: Generator
210/210 and Runtime 702/702 in Release, Debug and the cold source build; actual
Debug-native 702/702 with all 62 DLL hashes checked. AH contributes five Generator
and 31 Runtime cases; 16 concurrent companion Runtime cases remain separate.
Native exports add 53 AH plus two companion names, with no removals and matching
Debug/Release names. Managed API adds 67 AH plus 17 companion signatures without
removals. The 14 packages, both consumers, payload/document identities and refreshed
provenance/SBOM/checksums pass. STATUS and the hash-bound aggregate evidence identify
the tested source. External CI, signing, publication, human/visual and whole
downstream app acceptance remain NOT RUN.

Deferred low-level pointers, workspaces, matrices, solver/contact-order layouts and
callbacks keep their explicit blockers. The next [AI plan](BATCH_AI_TOPOLOGY_EXCHANGE_PLAN.md)
groups typed topology/exchange collections and history-to-repair/exchange workflows;
its candidate count is an audit scope, not a migration promise.

# BT: Nested coupled multi-point constraint copies

State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.

Candidate: Preview.69 / ABI1.111 / bridge0.119.0. Baseline: frozen BR Preview.68
plus BS generator-tooling corrections.16 primary declarations and4 new prerequisites
are admitted under TM054 / [ADR-0136](adr/0136-call-private-coupled-constraint-copies.md).
See [exact admission](BATCH_BT_COUPLED_COLLECTIONS_ADMISSION.json).

## Public workflow and generation

`OcctSharp.Geometry.Constraints.CoupledConstraints.Copy` accepts bounded rows of
3D/2D position arrays and optional tangent/curvature channels. Both nonempty sides
have matching channel modes; curvature requires tangency. Counts match separately
in each dimension. Null dimensions/empty positions denote absent sides; optional
arrays on an absent side are invalid. Caller bounds are preserved in detached
CopiedArray results; NativeOrdinal starts at one.

The exact contract, native/managed templates and TM054 emitter integration live in
`OcctSharp/src/OcctSharp.Generator`. Standard generation emits three files, with
contract/template resource hashes in the manifest.20 Blocked→Emitted transitions
are required at delivery, no Manual transition and no new Manual exception.
Existing315 generated outputs remain unchanged; five public types add forwarding
entries through `eng/generate-type-forwarders.ps1`, not by hand editing output.

Call-private SDK objects use ten initialized constructor profiles, including the
empty/default profile. Empty sides never enter mixed-array constructors.2D SDK
getters use global indices after3D points; absent getters are never called. SDK
shallow copies are not exposed. Every row is computed before scalar output copies;
exceptions, budgets and insufficient capacities publish no partial result.

Limits4096 rows/65536 total positions bound storage, not execution time. Lazy inputs
are bounded and enumerators are disposed on failure/success. Caller mutation must
be synchronized during enumeration; no input buffers or SDK objects are retained.

## Consumer and evidence

The shared workflow exercises all ten profiles with asymmetric3D/2D counts and
negative/positive caller bounds. Original arrays are mutated after copying. Copied
positions/tangents/curvatures then feed endpoint Hermite interpolation using the
existing AM spline contract. The SDK band solver has no pivot exchange: right-end
constraints are ordered curvature/tangent/position. This preserves all constraints
and nonzero pivots; the earlier failing ordering remains in runtime-focused-1.log.

Twenty-one samples check the polynomial x=t,y=t²,z=t³ and first/second derivatives.
An independent edge survives spline/workspace disposal, is written/read as STEP,
and topology plus three readback points are checked. Both clean package consumers
execute the same shared workflow. Tests also cover null/empty/mismatched channels,
finite values, lazy failures, overflow-safe bounds, exact budgets, immutable copies,
80 independent concurrent calls and raw ABI output sentinels on three failure paths.

Generator Release324 and focused Runtime7 passed. Initial generate-1.log lacks the
VC include environment; normal build.ps1 generation corrected that environment.
build-release-1.log retains a test-fixture compile error from using a nonexistent
EdgeCount property, corrected to the existing CountSubShapes API. The initial raw
ABI test loaded an old DLL; it now loads the exact test-output occt bridge explicitly.
Full Release/Debug/cold, actual Debug-native, ABI/API/export/lifetime, inventory,
14 packages and source/package freeze are required before closing BT.

Frozen authority will be `OcctSharp/artifacts/batch-bt/delivery-result.json` PASS.
Hosted CI, signing, push, public NuGet, external CAD and full downstream application
acceptance remain separate NOT RUN gates. BU–CI remain authorized after BT closes.

Local evidence:

16 admitted / 0 deferred, 4 new prerequisites / 4 reused; generated +20 / Manual +0.
Release/Debug/cold Generator 324, Runtime 1356: PASS.
Actual Debug-native 1356, 62 actually observed DLL paths/hashes: PASS.
Exports 41461 (+1/-0), Debug parity; API 58874 (+55/-0).
All 315 baseline generated files unchanged; current generated files 318.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bt/delivery-result.json PASS completes BT.

## Retained actual Debug failure and serial verification

The initial actual Debug run passed1354 and failed two unchanged existing Viewer
screenshot tests at File.Move with Windows file-sharing errors. All62 actual DLL
identities passed, but that run remains FAIL. Five serial repetitions in the same
isolated Debug directory passed both tests10/10 without changing product code or
assertions. The source audit confirms a synchronous SDK save path; the initial lock
holder was not observed, so its cause is NOT DETERMINED. A separate complete serial
actual Debug run is required and retained; identities alone cannot close delivery.
See screenshot-serial-verification.json and initial-failed logs in batch-bt.

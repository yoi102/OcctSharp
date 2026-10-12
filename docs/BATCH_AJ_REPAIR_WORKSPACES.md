# Batch AJ: ordered diagnostics, coupled repair and exact history

Preview.34 / ABI 1.78 / bridge 0.86.0 is the AJ candidate. [STATUS](STATUS.md)
owns current validation and delivery state. The [admission matrix](BATCH_AJ_REPAIR_WORKSPACES_ADMISSION.json)
freezes **136 admitted / 52 deferred** out of **188** candidates. Supporting
operations comprise 25 reused Manual, 43 reused Emitted and one new prerequisite.
The exact generated delta is **137**. No new Manual disposition is added.

## Public workflow

`OcctSharp.RepairWorkspaces` in Modeling provides `WireDataSnapshot`,
`RepairWireSegment`, `RepairCoupledWorkspace`, `TopologyRepairSession`,
`RepairResultSnapshot` and typed wire/shell diagnostics. These compose owning AI
shape sets, repair, exact history, validation and private STEP/IGES exchange.
The shared public-only workflow runs in Runtime and both clean package consumers.
It removes an actual tiny edge from a five-edge face, preserves source area and
topology, splits two disconnected outer bounds into two valid faces of total area
8, disposes receivers and uses surviving outputs, then roundtrips STEP and IGES.
Area tolerances are 1e-10 for unchanged sources, 1e-6 for repaired area and 1e-5
for exchange. Shell/solid fixtures preserve volume 24 to seven decimals.
The split-face fixture uses public Modeling operations in both consumers. Boolean
output can store wire edges out of traversal order; `FromWire(alreadyChained:false)`
and `ToWire()` establish the chained bounds required by native SplitFace. It still
requires two valid faces, actual change, area preservation and detached lifetime.

## Ownership and coupling

- WireData edits are functional copies of ordered container storage. TopoDS
  values retain TShape/location/orientation. Indices are native one-based;
  insertion zero means append. Duplicates/orientation and nonmanifold policy are
  explicit. A segment rejects separately retained nonmanifold storage because
  SDK Load would silently omit it; manifold=false data remains usable.
- Segments own private WireData and all four patch-bound sequences. All bounds,
  overflow and mode checks run before mutation. Native exceptions retire segments.
- Sessions deep-copy topology and geometry with BRepBuilderAPI_Copy(copyGeom=true)
  and use its ModifiedShape correspondence. Contexts are private, location-aware
  and serialized. No borrowed scalar mode or mutable external context escapes.
- Four small-edge sets/maps form one workspace. Face orientation maps capture
  the same receiver/context epoch consumed by SplitFace. Foreign/stale inputs are
  rejected; invalid input does not change epoch. Successful consume publishes a
  new workspace with every post-operation collection and shape; publication or
  native mutation failure retires the receiver.
- Workspace inspection is detached in one common geometry copy. Result/history
  snapshots similarly preserve one identity domain. Views survive parent disposal
  and subsequent mutations. Map traversal indices are temporary snapshot indices.
  Keys use IsSame, including location and excluding orientation.

## Exact replacement evidence and limits

Queries require exact original membership. Modified/Generated retain all native
paths; Leaf retains the native direct carrier. Edge/face/solid split requests use
native wire/shell/compsolid carriers so BRepTools_History retains every path.
Multi-vertex replacement is rejected before mutation because native history
cannot represent that carrier. Explicit merge retains a common output for every
input. Foreign lookalikes never match. Removed, unchanged, modified and unavailable
are distinct; unsupported compound/wire/shell history remains unavailable.
No correspondence is inferred from geometry and no selected topology is rebound.

Face Init does not initialize SDK Result; current-shape access uses the exact
face/context when Result is null. Empty shell ErrorFaces becomes an empty compound.
FixFaceLoops initializes the private wire tool for each face wire and returns all
loop wires; this SDK operation produces outputs rather than replacing the face.
Normal single-wire orientation may legitimately return an empty map. Native DONE
means some selected work occurred: FixWireGaps fixes curves and can leave an
unresolved gap between distinct topological vertices. Inspect the final topology,
statuses and residuals before accepting a repair.

Implicit WireData/WireSegment copies share mutable handles and are deferred.
WireSegment's TopoDS_Wire constructor has no implementation/export in the pinned
SDK. Reverse(face) can change shared seam pcurves and is deferred. ComposeShell
has no complete admitted composite-surface initialization/continuation path.
Advanced UV/intersection/external analysis and unsafe SDK branches remain deferred.
The SDK source commit is b8f597c677811d1f9f4d8a97f5ae2825c0353a42; source/header
hashes and failed attempts are retained under `OcctSharp/artifacts/batch-aj/`.

## Validation and following work

Focused fixtures cover reordered/reversed/nonmanifold wires, empty/duplicate and
wrong inputs, indices/overflow, patches, actual small-edge/face split/loop/shell
repairs, gaps and no-op, four-output maps, foreign/stale epochs, exact split/merge/
removal history, edge splitting, source/result disposal and release races.
Full gates remain governed by [WORKFLOW](WORKFLOW.md); consult STATUS for observed
results. Preserve all Preview.33 IDs/files/API/exports. Bind final fresh inventory,
packages, payloads, provenance and synchronization to source hashes.
After AJ delivery, freeze AK transfer-record/process candidates from its fresh
exit inventory. Unsafe root/recorded index and recursive graph branches remain
excluded until proven safe. AK planning is distinct from implementation.

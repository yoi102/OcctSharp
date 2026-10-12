# Ownership and Lifetime

## CH independent serialized graphs and sketch support (in validation)

CH also admits viewer-owned source creation IDs and instance-retained snapshots
under ADR-0139. Removing a creation entry leaves existing instances alive.
Separate copied TShapes and mesh storage prevent viewer mutation of caller
topology. Original identities are retained only for exact source correspondence;
styles and mutable drawers are owned independently. Context destruction clears
both maps. Source refresh stages changes before publication and never guesses a
geometric match. Managed sources are marked removed when their viewer disposes.

SerializedReferences/SerializedOccurrences shared payloads belong to the existing
FeatureResultHandle registry. Capture/import publish only a complete bounded
graph; failure unwinds private temporary owners. Restored roots/members use
ordinary independent Shape handles. OccurrenceRestoreResult retains a feature
owner for native identity/path queries; disposal releases root, members and
query owner. Source and decoded domain labels are not correspondence proof.
Sketch expression DTOs retain copied primitive arrays; callback-pinned raw nodes
must not escape. Exact template sources and budget contracts are in
BATCH_CH_APPLICATION_PREREQUISITES.json; actual lifetime validation remains pending.

## BY private Boolean/repair copies

TM058 keeps the existing input graph and result ownership. Native-only builders
own their initialized SDK object; topology/history getters return retained values
and independent vector storage, not geometry deep copies. SDK errors/warnings
become owning strings while the builder is alive, with65536 bytes per kind and
atomic failure on stream overflow. Existing Message and feature-result owners
carry them across the ABI. Repair history is accessed only under its existing
nonnull private owner and keeps all relations. No callback, borrowed public
receiver or new registry category is added. See [BY](BATCH_BY_BOOLEAN_REPAIR.md).

## BU generated unit/diagnostic composition

UnitDiagnosticWorkflow composes TM029 private units and TM035 copied diagnostics.
Enumeration is bounded and disposed before SDK calls; each private native row
owner is disposed before retaining detached copies. Success returns a read-only
managed batch; failure publishes no batch. No global working-unit setter or new
ownership category is introduced. See [BU contract](BATCH_BU_UNIT_DIAGNOSTIC.md).

## BT call-private coupled constraints

TM054 ([ADR-0136](adr/0136-call-private-coupled-constraint-copies.md)) retains no
SDK owner or caller buffers. Copied array constructors initialize each private row;
2D indices follow the3D SDK domain, absent channels are never dereferenced, and all
rows are staged before final publication. Managed snapshots own independent arrays
with caller bounds and survive input mutation or enumeration disposal. No shallow
copy detachment, shared mutable owner or callback is claimed. Inputs require
synchronization during enumeration; row/point budgets bound storage only.

BR generates compatible Foundation Text/Collections/Transient and bounded
BSplineKnotBuffers implementations from explicit templates. Existing registry
owners, release validation and managed SafeHandles remain identical; compile
units keep independent includes without generated PCH. TM053 copies the complete
static SDK knot sequence after degree/capacity checks, never its address.
No new ownership category is introduced. See [ADR-0135](adr/0135-generated-compatible-legacy-adapters.md).

AW TM030 retains only private copied curves/surfaces and completion-gates every
filling output; sampled values are immutable detached rows. TM031 assembles new
initialized topology from private curve copies, validates it before publication,
and provides no inferred topology-history mapping. See
[ADR-0115](adr/0115-private-filling-kernels-and-controlled-topology-assembly.md).

AV TM029 initializes strings/paths/colors privately and proves unit dimensions
before conversion. Bounded success-only registry snapshots are fully copied and
released before managed results return. UTF16 edit boundaries reject surrogate
splitting; no borrowed string, vector or dimension escapes. See
[ADR-0114](adr/0114-generated-call-private-foundation-values.md).

## AP private modeling builders

[ADR-0108](adr/0108-generated-private-modeling-builders.md) keeps builders call-private
and publishes frozen typed registry owners. Curves and edge input graphs are geometry
copies. Unfinished builders publish diagnostics only. Result/group queries make new
geometry copies. Contents counters are explicitly initialized. Wire checks copy an
exact member with its face and publish optional coupled values only when found.

AN's [private writer diagnostics](BATCH_AN_WRITER_DIAGNOSTICS.md) deep-copy all
source shapes and geometry before one serialized export epoch. Immutable managed
snapshots retain no native objects. Writer SafeHandles use typed registry leases;
source, map and model index domains remain separate and session-bound. See
[ADR-0106](adr/0106-private-writer-diagnostics.md).

Private .15 prepares only the existing viewer-owned detached topology before shaded graphics. The configured native mesh operation owns its parallel flag and completes synchronously before OpenGL work; original caller topology and process-wide mesh defaults are untouched. No ABI/ownership category or selected native owner is replaced. This remains implemented, not SDK-delivery accepted; see [source tessellation](CADORYX_A5_SOURCE_TESSELLATION.md) and [current status](STATUS.md).

Previous current summary preserved in [history](history/CADORYX_A5_PROGRESS_20261006_1.md).

## Earlier progress records (historical state)

2026-10-06 private A5 .14 texture validation: formal .13 application freeze8 complete2928/2928 passes with zero failures/skips and all accepted B17 cases;852 source/436 payload and actual62 native bindings match. Same freeze feedback39/full60 PNG/general WPF evidence remains separate. Independent .13 consumer75 produces57 PASS/18 FAIL: all prior51 and six file-removal lifecycles pass; six material slots each accept three corrupt nonempty encodings during source creation and refresh. Twelve base/emissive/occlusion/common-diffuse refreshes change retained frames; metallic-roughness/normal remain identical under this profile but also accept bad data. Other viewer/selection and recovery hold. Actual13 SDK/62 native and package/cache/lock bindings are verified. The failed audit assumption that every slot must alter this profile is retained; original75 assertions/results are unchanged.

Isolated private .14 now validates detached encoded textures before publishing source styles, with per-source deduplication and a compressed-image reader path; original encoded ownership and exact BRep/copy/transform proof remain. Release native and managed zero-warning/error builds pass. Source53 contains all prior29 plus24; full786 contains all prior762 with zero failures/skips. Both actual62 native sets match frozen6;965 source inputs have two changed originals/two additions,184 original generated files unchanged and61 native dependencies unchanged. All18 malformed create/refresh attempts now reject without changing retained frames,selected instance or other viewer, and recovery passes. Initial managed analyzer errors/freeze-classifier failure remain; fixes preserve behavioral assertions. Actual Debug,compressed-texture positive fixtures,50 private headers,exports/Generator,immutable14 packages/consumers and formal Cadoryx14 integration remain NOT RUN. Formal .13 packages/application are unchanged. New app freeze8 benchmarks,6482ms target,complete driver/material matrices and physical input remain pending. Latest accepted A4/B17/C5/D5/E4/F5g;A5 NOT_ACCEPTED;whole A-F IN_PROGRESS;G deferred. Evidence: OcctSharp/artifacts/cadoryx-a5/texture-validation-full-verification-1.json,package-consumer-negative-verification-2.json and Cadoryx/artifacts/a5-full-verification-5.json.

2026-10-06 A5 imported source and encoded texture snapshots: frozen .12
Release755/755 contains all previous750 plus five added cases, and focus22/22
passes. Four actual STEP/IGES workflows exercise shaded/wireframe source refresh,
two-level shared STEP assembly structure, caller/file release, exact full frames
and original whole/face/edge identities. This is a constructed exchange fixture
matrix, not complete vendor/PMI/texture interoperability acceptance.
The external glTF base-color texture first-display test reproduced a borrowed
file dependency. Source material snapshots now copy encoded file, buffer and
file-slice bytes into viewer-owned buffers. All six common/PBR texture slots
reuse exact owned bytes, retain scalar metadata, and use never-reused GPU IDs.
Public native build18/run15 passes25/25 including buffer mutation, caller-file
removal, file slices and inactive cached-mode release. All34 PNGs are reviewed:
33 match prior reviewed frames byte-for-byte and one new frame is directly read.
Imported STEP whole selection exposed old-source OpenGL connections in cached
wireframe after shaded-mode refresh. Four crash/Aborted runs are retained.
Recompute all cached modes before retiring the previous source; imported four
cases now pass without changing exact selection or full-frame assertions.
Frozen961 sources have23 changes/additions;14 managed/62 native and all62 actual
loaded native DLLs are bound. Standalone private headers48/48 pass. Release
exports remain five additions over .11/no removals. The existing script
regenerates/checks4152 unchanged forwarders; all generated source stays .11.
Debug, packages/consumers, complete material/PBR import fidelity, rendering
failure injection, application integration and6.482s timing remain pending.
Cadoryx stays .11; A5 NOT_ACCEPTED, complete A-F IN_PROGRESS, G deferred.
Evidence: Cadoryx artifacts/a5-sdk-verification-6.json and SDK sdk-freeze-3.json.


2026-10-06 A5 explicit source-style refresh: the private .12 SDK now exposes
ViewerSource.RefreshStyles(XdeLabel). Exact original topology membership,
native partner, twelve placement coefficients and root orientation are required.
Staged per-viewer styles rebase existing instances and explicit root/subshape
overrides without replacing AIS objects or logical IDs. Review appearances and
textures bind to new owned drawers; reset restores the refreshed source.
Newly hidden topology is deselected; unchanged selected native owners are reused.
Frozen Release750/750 contains all previous741 plus nine added cases; focus17/17,
direct public OCCT build16/run13 22/22 and standalone private headers47/47 pass.
Release exports add exactly five names over .11 with no removals; actual Debug
and Release-Debug comparison have not run. Frozen959 sources have21 changes;
14 managed/62 native and all62 actually loaded native DLLs are bound. Generated
source and4152 script-regenerated/check forwarders are byte-identical to .11.
Thirty-three native PNGs include31 identical reviewed frames and two new frames
directly reviewed. The transparent-face fixture failure and explicit-zero-alpha
implementation failure remain; the latter fix retains the original assertion.
The SDK appearance helper stages allocation before swaps. Invalid-input atomic
rejection is tested; driver/render failure injection is NOT RUN.
Cadoryx stays .11. Imported hierarchy/material/texture fidelity, Debug, immutable
packages/independent consumers, application integration and6.482s timing remain
pending. A5 NOT_ACCEPTED; complete A-F IN_PROGRESS; G deferred by the user.
Evidence: Cadoryx artifacts/a5-sdk-verification-4.json and SDK sdk-freeze-2.json.
Frozen parent packaging documents remain .11; no new package is claimed.


## Cadoryx direct XDE presentation metadata (SC-070)

The independent color/visibility readers return copied values only. Setting
one native channel or visibility borrows its parent document SafeHandle and
requires that document's open transaction. Explicit metadata copying retains
both source and destination SafeHandles during the synchronous call. XCAF
recreates layer/material records and clones visual material objects using a
call-local map. Destination labels remain parent-bound to the destination;
source disposal does not invalidate copied assignments. No topology identity,
geometry, name, validation, PMI or arbitrary attribute is transferred. The
caller proves native label correspondence separately and aborts on failure.
No new owner, registry category, borrowed pointer or release API is added.

## Cadoryx exact transform and compound history

SC-069 reuses LocalFeatureResult ownership. Managed calls retain snapshot and
transform SafeHandles throughout native copying/building. The native InputGraph
copies all inputs as one private graph, preserving shared topology without
mutating caller flags or geometry. Transform builders and compound membership
maps remain call-local; only copied results and history are published. Those
owners survive input and transform disposal and use the existing feature-result
release path. No borrowed native builder, additional registry or lifetime
category is added. Diagnostic-copy order must be proven before Evolution support
is published; callers must treat unavailable history separately from valid geometry.

## Batch AJ coupled repair receivers and snapshots

[ADR-0101](adr/0101-generated-coupled-repair-workspaces.md) defines private deep
topology/geometry copies, exact native copy correspondence and location-aware
ShapeBuild_ReShape contexts. Repair calls serialize and retire receivers after
native mutation/publication failures. Validation rejection does not advance epochs.
The four small-edge collections and face orientation map retain their parent and
epoch. Consume requires the same receiver/epoch and publishes all post-operation
collections together. Inspection maps and result/history are copied in one common
identity domain; native containers remain private. WireData copies only sequence
storage and preserves shape identity. Segment storage and patch sequences are
private; implicit aliasing copies remain deferred. Returned owners outlive managed
receiver disposal. See the [AJ contract](BATCH_AJ_REPAIR_WORKSPACES.md).

## Batch AI topology values and retained exchange entities

[ADR-0100](adr/0100-generated-topology-and-exchange-collections.md) adds ordered
ShapeSet value copies preserving TShape/location/orientation, not deep geometry
clones. Private Boolean/build/split receivers retain arguments/tools and exact
history, run serially/non-destructively, and require exact membership for history.
Removal and keep-shape inputs require exact source topology. Returned values/sets
have independent lifetimes. SafeHandle pointer-array inputs retain every element.

EntitySet retains intrusive references and RTTI with detached container storage;
entities remain shared. Iterator storage/cursor are private and serialized. Native
implicit cursor copies are excluded. Private file readers expose one transfer
epoch, validate all members before transfer, retire after transfer exceptions,
and forbid repeat transfer/cache reuse. Owned direct graph data retain the entire
private native session/model and survive managed reader disposal through that
retention; graph/transfer calls serialize through the parent. Model-number/transfer
calls require a live session; foreign identity is rejected. Shape-to-entity lookup
admits only the mapped first-match branch; root and recorded lookup branches use
invalid index domains in the pinned SDK and are rejected before the call.
External entity mutation is unsupported
while model/graph consumers use it. Shared sewing/binder calls keep their normal
caller synchronization requirement. Leased typed registry owners, rollback
publication, exception containment and separate result disposal remain mandatory.

## Batch AH typed curve collections and sized scalar outputs

[ADR-0099](adr/0099-generated-bulk-analysis-and-typed-curve-collections.md) adds
typed curve sets and concatenation results using leased non-transient registry
owners containing intrusive handles to deep copies. Input SafeHandles are retained
during copying. Set and curve accessors return independent owning copies. Concat
mutates private copies and retains updated-input and output sets; callers dispose
each returned owner. Numeric scalar refs publish after all output preparation succeeds.

## Batch AG grids and retained interpolation

[ADR-0098](adr/0098-generated-surface-grids-and-retained-interpolation.md) extends
the copied profile to owning Array2 grids and independent HArray1 copies. Algorithms
retain HArray inputs through OCCT intrusive handles. Typed registry leases own only
non-transient algorithm wrappers and owning Array1/Array2 containers. Boolean elements
cross as checked bytes. Results retain independent handles; immutable snapshots outlive
receivers. SurfaceFitOwner additionally owns the exact constant-height fallback surface,
clears it on reinitialization and retains previous returned surfaces through their handles.
Copy-out reports the kernel's actual replacement bounds and publishes managed refs only
after complete conversion. Caller synchronization is required for receiver mutation.

For direct AG consumption, the existing FreeformAuthoring and FreeformTypes public
types are owned by the Modeling assembly; ShapeExchange, StepReadResult and
StlWriteOptions are owned by DataExchange. Their native allocation/release and
independent Shape result contracts are unchanged. The facade forwards the moved
types, preserving existing public namespaces and signatures without a reverse
module dependency.

## Batch AF copied arrays and sequences

[ADR-0097](adr/0097-generated-copied-arrays-and-sequences.md) defines independent
managed snapshots, typed leased native containers and owning fitting algorithms.
Factories copy input buffers during the call; algorithms use native copies. Output
references are copied before receiver release and retained in private immutable
snapshots for bounded info/copy transfer. Ref replacements happen only after every
managed conversion succeeds; finally releases all result tokens. Native receiver
mutation is not rolled back. Existing shared receiver contracts and Manual entries
remain intact. Array2, retained HArray handles, pointer results and other element
lifetimes require the next explicit contract.


## Owned data workflow expansion

[Batch AD](BATCH_AD_OWNED_DATA_WORKFLOWS.md) reuses ADR-0096 for 14 initialized
hatch, intersection, IGES, flag and picking data owners. Hatch endpoint sequences
copy their nodes; returned endpoints/elements and nearest-pick results have
independent storage. IGES result handles retain their entities after builder
disposal. Shared handle inputs remain non-null and registry-validated. No borrowed
address or mutable owner reference is published, and the permission-flag owner
does not perform filesystem operations.

## Batch AC utility and SELECT extension

[Batch AC](BATCH_AC_OWNED_UTILITY_EXPANSION.md) reuses ADR-0096 for additional audited
owners. Data-result copies include explicitly checked color, date and intersection
records; no algorithm copy/move or borrowed storage is inferred. STEP SELECT containers
retain intrusive entity handles. Typed setter conveniences use native checked SetValue
and preserve both entity and select-member acceptance semantics; returned entity handles
retain the entity independently of the container. Native mutations still require caller
synchronization. New const operations resolve native overloads through const leases.

## Batch AB owning non-transient values (ADR-0096)

The new category constructs audited native instances directly in stable heap storage;
published instances are never copied or moved. A typed managed SafeHandle owns an
opaque, nonzero token with matching release in the existing Native DLL. Tokens are
issued monotonically, never recycled and never interpreted as object addresses. Token
exhaustion fails rather than wrapping. The type-tagged registry owns each live instance
through shared_ptr and obtains an operation lease under its lock before native access.
Every accepted owned input is leased too. Removing a token during release invalidates
future acquisitions while existing leases retain the object until their calls end;
repeated/stale release cannot destroy a newer allocation or an entry of another type.

This is a concurrent-release lifetime guarantee for the new category. It prevents the
validation-to-use race and allocation-address ABA, but does not serialize operations
on mutable native state. Callers must synchronize such access and retain native thread
restrictions. Existing shape/shared categories are unchanged. No borrowed member,
shared_ptr, class layout, copy/move operation or retained external address is inferred.

Audited copied refs use initialized independent snapshots and complete every output
conversion before successful publication. Managed refs remain unchanged on exceptions;
boolean false still publishes successful copies and aliased refs assign in argument
order. Native object mutation or cache changes before a failure are not rolled back.
Construction/destruction and lease-race checks pass in Release and Debug fixtures.
See [ADR-0096](adr/0096-generated-owned-nontransient-values.md) and
[Batch AB](BATCH_AB_OWNED_NONTRANSIENT_VALUES.md); current progress is in [STATUS](STATUS.md).

## Batch AA copied references (ADR-0095)

ReferenceOperations reconstruct initialized ref arguments as independent local native
values. All output conversions finish before publication; managed assignments happen
only after a successful status. A false boolean result still publishes successful
outputs. Aliased refs are independent snapshots and assignments follow parameter order.
Use distinct variables when native aliasing semantics matter. Shared receivers and
inputs use typed SafeHandles for call-duration retention and registry validation;
null or disposed shared inputs are rejected. No persistent owner is added. Native
const queries may update caches; shared object rollback and concurrent access are not
promised. Initialize geometric refs with valid values, including valid directions,
frames and transforms, even for parameters normally used as outputs.

## Batch Z receiver copies (ADR-0094)

ValueOperations never allocate a persistent native owner. The receiver and arguments
are reconstructed as local native values. A successful non-const operation publishes
a complete copied receiver; native failure leaves the managed ref argument unchanged.
Return conversions finish before any successful output is published. Const-reference
returns copy a value before its native temporary dies. No reference, array-start address
or cache state escapes, and no new lifetime/registry category is introduced. A ref
argument still needs ordinary caller synchronization when shared between threads.


## Batch Y geometric copies

ADR-0093 adds only ValueCopy projections. Geometric fields are copied during the
checked native call and survive receiver mutation/disposal without retaining an owner.
Input records are reconstructed into temporary native values; no C++ storage address
or layout crosses the ABI. Existing receiver/thread restrictions still apply during
the call. Invalid nested values fail before invoking the native operation. Indexed
element references copy one element and retain the native index/domain contract;
this does not implement bulk sequences or output references.

## Batch X generated const-reference copies

ADR-0092 adds no borrowed owner category. Known scalar/enum/point const-reference
returns are copied before leaving the native call. A selected const handle reference
is copied into a local intrusive handle and then moved into a new registered wrapper;
null stays null. The result survives receiver disposal or later replacement of its
handle field. Existing sharing remains intentional; it is not a deep copy of OCCT state.
The receiver and inputs must remain valid during the call under existing thread and
mutation rules. New const methods invoke a const receiver. Pointers, mutable references,
rvalue transfers and general TopoDS reference returns retain their prior restrictions.
Sequence-start references require an explicit length/copy contract. The
`BSplCLib::FlatBezierKnots` SC-061 manual binding copies its complete bounded static
array into caller-owned storage during one guarded native call. No borrowed reference
survives the call; other sequence-start references remain excluded.

## Batch W render-resource contract

The existing creating-thread Viewer context owns light, texture, environment and layer
IDs; no independent global registry or GPU handle escapes. Pixel inputs are copied.
Environment images retain their original owned storage even after source textures are
replaced or removed. Appearance overrides clone effective shading aspects and keep
original parent/custom drawer aspects for reset; XDE documents are never mutated.
Capture outputs own managed arrays with fresh-copy accessors and capture-time matrices,
surviving viewer disposal. Capturing temporarily masks layer state and restores it.
Portable recipes contain copied settings and explicit application keys, not native IDs
or local paths. Replay resolves all referenced assets before setting changes; individual
settings are atomic, but the whole replay is not a document transaction.
Focused lifetime tests and full Release/Debug/actual Debug-native sweeps pass;
the complete exit record remains in STATUS and ADR-0090.

## Batch V partition and volume contract (locally validated)

ADR-0089 keeps builders/PaveFillers and their mutable material/history tables local
to a call. The existing FeatureResult temporarily owns copied RegionData records and
TopoDS values, released by its existing release operation. Managed plans capture one
private input graph; managed results own private Shape values retaining exact sharing.
Every public topology getter deep-copies; no mutable alias into a plan/result escapes.
Copied cell/boundary IDs carry a new result revision and reject cross-result use.
Multi-output failure releases the entire native-local result before publication.
No new registry, allocator, project or DLL is introduced. V lifetime and atomicity
regressions pass in Release/Debug and actual Debug-native; full exit gates are in STATUS.

## Batch U local-feature contract

Modeling recipes own copied scalar programs and revision-bound selections. Limited,
rib and shell plans own one deep-copied input graph; every native call makes a further
private graph copy while preserving exact support/sliding sharing. Native builders,
maps, iterators, laws and section arrays never escape. Result diagnostics reuse the
existing FeatureResult owner; result/history topology reuses Shape registration.
The internal selected-subshape bridge returns an owning TopoDS value sharing only a
private snapshot, preserving exact support identity for the next whole-graph copy.
It is not a borrowed pointer and adds no registry or public mutable snapshot property.

LocalFeatureAcceptance verifies source/result fingerprints and Q budgets, then owns
a private candidate snapshot. Protected topology requires exact Unchanged history
and matching canonical standalone geometry, never a proximity match. Accept returns
another independent copy and consumes the acceptance once. Failed algorithm roots
are cleared; HasResult-gated fillet partials remain diagnostic history owners only.
T evaluation stays in the facade with original Documents ownership unchanged.
Current evidence and missing whole-batch gates are recorded in STATUS.

## Batch T parametric graph contract

ParametricDocument.Create/Open owns its OCAF/XDE parent; Attach borrows the existing
parent and never disposes it. All operations check both wrapper and parent disposal.
Definitions/values/expressions copy managed inputs. Algorithms, TFunction graphs,
TNaming iterators, relocation tables and temporary buffers stay call-local; no new
handle registry, native callback or cross-DLL allocator is introduced.

Successful candidate results publish only after the entire selected closure finishes.
Failure/cancellation releases every candidate, rolls back edits and leaves last-good
geometry available only through explicit stale access. GetResult deep-copies geometry
and triangulations. Selection/history results have independent registered Shape
ownership and survive document disposal; callers dispose each returned history item.
Result-generation GUIDs index immutable history labels, not OCCT transaction levels.
Boolean/S source-owner history does not claim exact persistent subshape mapping.
Duplicate uses real TDF topology relocation and rewrites identities and metadata paths.
Viewer review owns thread/parent-bound IDs and replaces them on recompute/undo.

## Batch S authoring implementation contract

Scalar laws and conversion pieces own copied immutable data. Sweep and fill plans own
one copied dependency graph, preserving profile/spine-vertex/support-face relationships.
Builds copy that graph again so compatibility/filling cannot mutate a reusable plan.
Plans reject disposed use and survive original argument disposal. Results and each
history shape use registered Shape ownership and survive plan/input disposal. Temporary
FeatureResult containers use the existing matching release; their private history adds
source/subshape provenance without changing the existing feature-history ABI. Missing
mapping is explicit and has no Shape. No registry, allocator, project or DLL is added.
Source references are scoped to the plan identity; no nearest/count/address identity.
Temporary compound assembly restores the source TShape.Free flag even on failure.
Required residuals never call OCCT's unsafe per-index temporary-array getters.
Forty-eight G2 lifetime loops run in every expanded focused suite; ten complete
repeats and an isolated actual Debug-native 273/273 sweep pass. Final whole-batch
acceptance evidence remains in STATUS, not inferred solely from those loops.


## Batch R authored mesh contract

`AuthoredMesh` and its channels, groups, polylines, origins, selections and maps own
immutable managed copies and do not require disposal. Edits create unique revisions;
map composition checks intermediate revision and cardinality. One-to-many splitting
and deletions are explicit, not inferred by address, proximity or matching counts.
`DiscreteMeshModel` owns an existing registered Shape and private copied metadata;
public shape copies duplicate geometry and triangulation. No public mutable alias can
change its private topology. Exact-face cache replacement/remeshing also works on
private copies. Poly builders, graphs and merge maps remain call-local, with no new
allocator/registry or borrowed pointers. Null outputs stay null on failure.

Mesh assembly publication owns one transaction; labels are document-bound while its
scene is a copied publication-time snapshot. A viewer review owns one parent-bound
presentation on the creating thread; successful replacement invalidates old IDs/tokens,
and failure keeps the original. Output staging promotes the complete sidecar set.
Focused disposal/copy/isolation/transaction tests pass; full gates remain in STATUS.

## Batch Q validated implementation contract

Repair snapshots own independently copied topology; source and result indices are
scoped to a managed snapshot ID and revision. Public diagnostic/recipe/history values
are copied. A native `OcctSharp_RepairResultHandle` owns stage output and copied history
until its matching `occtsharp_repair_result_release`; the existing Runtime registry
owns its live set. A separately retrieved Shape has its own registered owner and
survives result release. Algorithms, ReShape contexts and iterators remain call-local.
No borrowed OCCT pointers cross the ABI. Managed preview acceptance is all-or-nothing;
failed stages and unavailable required budgets cannot publish a result. Public copy
operations isolate mutable topology/geometry; retained external pcurve support is
read-only. Focused tests now cover source/preview/result disposal, 48 repeated native
result lifecycles, invalid/stale handles, negative/undersized buffers, failed-stage
cleanup, protected/outside-selection guards and conflict-free XDE publication. Final
Release/Debug and isolated actual Debug-native Runtime sweeps pass 205/205; both
clean package consumers pass, including independent direct-Modeling repair ownership.

## Prepared Q-T lifetime constraints (no ownership change)

ADR-0082's [four prepared waves](BATCH_Q_T_PREPARATION.md) retain O001-O013. Definitions,
recipes, diagnostic/history/index maps and graph snapshots are copied; temporary
algorithms, laws, iterators and Poly/TNaming containers remain native-local. Accepted
topology uses the existing Shape owner. A future owning preview/result container must
have a matching release operation, failure cleanup and source-disposal evidence.
Labels and selectors remain document-bound; viewer IDs remain parent-bound/thread-affine.
Revision-scoped IDs do not promise persistent topology identity. TNaming selection uses
dedicated labels to avoid clearing user data. Preparation adds no handles or ownership
categories; actual implementation must record any new concrete lifetime contract here.

## Prepared U-W resources (no new category)

ADR-0083 extends the same constraints to native-local contour sections, partition/volume
builders and copied diagnostics. Partial fillet results are explicitly failed previews,
not automatically accepted shapes. W light/texture IDs are viewer-bound and thread-affine;
their concrete release/replacement contract is implementation work. Pixel/cubemap inputs
and frame outputs are owned copies with declared stride/format/origin, never borrowed
GPU/PixMap pointers. Documents does not own a renderer and core frame data does not own
WPF objects. No new handle or registry exists merely because this scope is prepared.

## Native source ownership after ADR-0081

Source extraction does not change O001-O013 or add an ownership category. The manual
live sets and mutex have one storage owner in `Runtime/Registry.cpp`; the error buffer
has one thread-local owner in `Runtime/Error.cpp`. `Runtime/Shape.cpp` implements the
same creator/validation/release path used by generated bindings and all manual domains.
Other owning handle definitions and operations reside with their domain, while registry
templates still access that single manual state. Generated per-type registries are not
moved or replaced. Separate source files do not make concurrent release/use safe, and
they do not establish any cross-DLL allocator or release protocol.

## Batch P surface workflow boundary

Surface descriptors, grids, UV definitions, boundary loops, projection solutions,
intersections, traces and diagnostics contain only copied managed values. No surface,
pcurve, adaptor, iterator, intersector or temporary builder escapes a native call.
Topology results use the existing registered Shape owner and survive their source.
Repair and split result containers own their Shape and must be disposed.

Copying an edge without its support face through OCCT's generic copier drops external
pcurves. Batch P explicitly restores copied pcurves and copies 3D curve geometry onto
independent topology; supporting surfaces are shared read-only. Flags, vertices and
tolerances are modified only on the result. All intermediate owners are released on
failure. Count/copy boundaries expose no native arrays. XDE labels remain document-bound;
viewer presentations remain viewer-bound and creation-thread-affine. SC-053 and ADR-0080
do not create a second registry or cross-DLL allocation contract.

## Preview.14 sketch ownership

Batch O public definitions and arrays are immutable managed copies. Geom2d curves,
intersectors, projectors, classifiers, and builders exist only during native calls;
no borrowed OCCT curve or iterator escapes. Curve transformations and edits create
new definitions. Their derivatives use the definition's native parameter direction.
Each edge, wire, face, extrusion, revolution, or Boolean result is an independent
registered Shape owner. Wire building copies topology before changing vertex tolerance.
XDE labels remain document-parent-bound; viewer presentations remain viewer-parent-bound
and thread-affine. SC-052 records the exact manual declarations. The physical module
and one-native-DLL boundary from ADR-0074 is unchanged.

Ownership rules are safety requirements. Coverage goals never override them.

## Ownership categories

Every value crossing the native boundary must use exactly one category:

| Category | Meaning | Managed responsibility |
|---|---|---|
| Value copy | Independent data copied across the ABI | No native lifetime |
| Owning | Managed wrapper exclusively owns a native resource | Deterministic release |
| Shared | Lifetime follows OCCT reference-counted semantics | Release one retained reference |
| Borrowed | Resource is owned elsewhere | Never release; enforce valid lifetime |
| Parent-bound | Borrowed resource is valid only while a parent remains alive | Retain/validate parent relationship |
| Static/process | Process or library owns the resource | Never release; document concurrency |

Unknown ownership is a model validation error, not an implicit category.

## Rules

- **O001** — Every owning object returned across the ABI has one matching native
  release operation.
- **O002** — Borrowed handles are never released by managed code.
- **O003** — OCCT `Handle<T>` wrappers preserve intrusive reference counting and do
  not become untracked raw pointers.
- **O004** — A native reference or pointer is not exposed beyond its proven lifetime.
- **O005** — Managed disposal is idempotent and cannot double free.
- **O006** — Finalization is a fallback, not the primary release path.
- **O007** — Access after disposal fails predictably before native dereference.
- **O008** — Native exceptions cannot bypass cleanup or cross the C ABI.
- **O009** — Native allocation and deallocation use the same module and allocator family.
- **O010** — Parent-bound wrappers keep or validate the parent relationship for every call.
- **O011** — Runtime casts retain the correct shared ownership and dynamic type semantics.
- **O012** — Callback state, if later supported, has explicit pinning, cancellation,
  reentrancy, and teardown rules.
- **O013** — Managed assembly/package boundaries never change native creator ownership.
  Every module resolves the same bridge, registry, allocator, validation, and release
  family until a separately proven cross-DLL protocol exists.

ADR-0074 moves managed type ownership without moving native ownership. `Shape`, its
`ShapeHandle`, generated topology, and their release path share `OcctSharp.Modeling` on
the managed side, but the actual allocation/registry/release implementation remains in
the single `OcctSharp.Native.dll`. Other managed modules follow the same one-bridge rule.

## OCCT-specific baselines

### `Handle<T>` and `Standard_Transient`

The generator must recognize handle declarations, inheritance, null handles, copies,
base/derived conversions, and the retain/release behavior implemented by the native
bridge. Multiple managed wrappers referring to one native object must be tested.

The current shape-only bridge now registers each owning shape handle in a mutex-protected
live set. Operations reject a non-null handle that is not registered, and release removes
the registration before deletion, making repeated native release calls no-ops. This is a
stale-handle guard for the owning shape category; it is not yet OCCT `Handle<T>` reference
counting and does not make concurrent release/use safe.

The experimental `SharedTransient` wrapper is the first shared category. Each native
wrapper owns one `opencascade::handle<Standard_Transient>` value, so cloning increments
OCCT's intrusive reference count and releasing a wrapper decrements it. Null handles are
valid values with reference count zero. The wrapper registry protects the ABI wrapper
pointer; it does not replace OCCT's object counter or provide weak references.

The same probe exposes OCCT RTTI type names and `IsKind` base checks. ABI 1.8 adds one
checked cast target, `OcctSharp_TransientDerived`: the native bridge validates the
dynamic kind before copying the shared handle, and returns `TypeMismatch` for null or
incompatible values. `SharedTransient.TryCastDerived` therefore creates a typed
wrapper only after validation; it never reinterprets an unverified pointer.

ABI 1.9 applies the validated shared category to configured generated types. Each
`GeomCartesianPoint` wrapper owns one native wrapper and one intrusive reference;
`Clone` copies that handle, mutations are visible through retained wrappers, and either
managed wrapper may be disposed first. Per-type registries reject stale wrapper
addresses. Borrowed handles, parent-bound handles, general casts, and concurrent
release/use remain pending.

The initial generated StepBasic milestone applies that same shared category without changing O001-O012 to ten
StepBasic entity classes. Constructors create one intrusive handle, `Clone` retains the
same entity, scalar/boolean/enum members operate through a registry-validated receiver,
and disposal releases one retained reference. Handle-valued fields and parameters remain
unemitted until cross-generated-type conversion and null semantics are generalized.

The alpha.44 package-level expansion applies the same category to selected BRepMesh,
Poly, ShapeAnalysis, ShapeFix, and ShapeUpgrade records. Binding-model schema 1.2
records `IsAbstract`, and abstract records are excluded even when a public constructor
is visible. The 61 emitted concrete types keep per-type live registries, one retained
OCCT intrusive handle per managed wrapper, and value-copy scalar/enum arguments/results.
No algorithm child, mesh buffer, topology reference, or cross-type `Handle<T>` value is
  borrowed through this profile.

The alpha.45 STEP model expansion applies the same owning-wrapper category to concrete
`StepGeom`, `StepRepr`, `StepShape`, and `StepVisual` entities. Each managed wrapper retains
one OCCT intrusive reference and can be cloned independently.

Alpha.46 generalizes relationships between selected generated shared-handle types. Managed
`null` maps to a null OCCT handle. A non-null parameter is checked for disposal before its
opaque registry token is passed, then the target-specific native registry validates it.
A returned non-null handle is copied into a newly allocated target wrapper and therefore
remains valid after the source argument or receiver is disposed. Returned null handles map
to managed `null`; no borrowed relationship wrapper is created.

### `TopoDS_*`

ABI 1.10 and `TM007` implement the base `TopoDS_Shape` value category. Each registered
native wrapper owns one independent C++ shape value; copy and reversal allocate new
wrappers, but OCCT's internal `TShape` remains shared as normal. Disposing one wrapper
cannot invalidate another copy. `IsPartner` compares `TShape`, `IsSame` adds location,
and `IsEqual` adds orientation. Null state, shape kind, and orientation are generated.

B04 adds `Compound`, `CompSolid`, `Solid`, `Shell`, `Face`, `Wire`, `Edge`, and `Vertex`
wrappers. Each checked `TopoDS::Xxx` conversion copies into a new owning shape value;
wrong non-null kinds return ABI `TypeMismatch`, and a successful typed wrapper remains
valid after source disposal. Location mutation/composition, hashing, and topology
children remain pending and must extend this value contract rather than infer ownership
from a pointer spelling.

### Iterators and child objects

Explorer results, collection views, triangulations, document labels, and other child
objects require explicit copied, retained, or parent-bound semantics. Temporary native
objects may not escape through a borrowed wrapper.

### BRep adaptor snapshots

`BRepAdaptor_Curve` and `BRepAdaptor_Surface` exist only within a native bridge call.
Their topology references and underlying curve/surface accessors never cross the ABI.
The edge and face snapshot APIs copy enum, parameter, UV-bound, and point values into
caller-owned structures. These results are value copies with no native lifetime and
remain valid after the source topology wrapper is disposed. Wrong topology kinds fail
before adaptor construction.

### Basic modeling results

`BRepAlgoAPI_Fuse` and `BRepAlgoAPI_Common` never leave the native call; each successful
shape is copied into a new registered owning wrapper that does not retain its inputs.
`BRepExtrema_DistShapeShape` likewise remains call-local, while distance, one point pair,
and solution count cross as a value copy. Support topology, parameters, progress state,
and history are deliberately not borrowed or parent-bound through this profile.

### Common feature-modeling results

Cone/torus builders, prism/revolution builders, fillet/chamfer contour state, offset
state, section state, `Bnd_Box`, and `BRepCheck_Analyzer` remain call-local under
SC-032/ADR-0052. Input shapes, vectors, axes, and selected edges are borrowed only for
the call. Every topology result is a new registered owning shape and does not retain an
input wrapper. Bounding boxes are six copied doubles with no native lifetime; validity
and subshape counts are copied scalar values. Algorithm history, progress, contours,
and per-face offset state do not cross the common-modeling ABI.

### Boolean and healing history exclusion

Cut, ShapeFix, and same-domain unification return new registered owning shapes and do
not retain their inputs. Their BOP/ShapeFix/ShapeUpgrade history and mode state remain
native-local. No history object, modified/generated topology map, status reference, or
borrowed child crosses the ABI in the B12 owning-result/no-history profile.

### Mesh-format exchange providers

OBJ, PLY, glTF/GLB, and VRML provider/configuration objects are created and destroyed
inside one native call. Writers borrow a live shape only during the call and build its
triangulation before export. OBJ, glTF/GLB, and VRML readers copy the transferred result
into a new registered owning `Shape`; it has no dependency on provider, document, or
scene lifetime. No provider configuration, progress object, label, native mesh, or
metadata graph crosses the ABI. PLY read has no ownership contract because it is not
implemented by OCCT 8.0.1 and is exposed as unsupported.

### OCAF documents and labels

`OcafDocument` owns one native wrapper containing retained `TDocStd_Application` and
`TDocStd_Document` handles. Native release aborts any open command, closes the document
from the application session, and deletes the registered wrapper. `OcafLabel` has no
native handle: it stores a stable TDF entry plus a strong reference to its parent
document. Every operation resolves that entry against the current data framework, so
explicit parent disposal deterministically invalidates all labels. Names are UTF-8
copies. Transactions are parent-owned command state; an uncommitted managed transaction
aborts on dispose. Abort rolls back attributes but OCCT may retain allocated empty label
nodes, which are not treated as independent child owners.

### XDE metadata and assemblies

`XdeDocument` uses the same owning application/document category as OCAF with BinXCAF
drivers. `XdeLabel` is parent-bound by stable entry. XCAF tools, free-shape/component
sequences, color/layer/material tables, reference tree nodes, and STEPCAF state remain
inside each native call. Shape and occurrence-location results are new independent owners.
Names, effective RGBA, layer arrays, material records, counts, and entries are copied.
An occurrence's referred part is represented by another parent-bound entry, not a native
reference handle. Effective color deliberately maps Gen/Surf/Curv because STEPCAF may
normalize an overall color between those document-owned channels.

### Visualization graph and presentations

`OcctViewer` is an owning, thread-affine wrapper for the display connection, OpenGL
driver, viewer, interactive context, view, and `WNT_Window` bound to an application-owned
HWND. The application must keep the HWND alive until the viewer is disposed and must call
viewer methods only on the creating thread. The viewer does not own or destroy the HWND.

`ViewerPresentation` is parent-bound and owns no native allocation. Its 64-bit ID is
resolved through the parent viewer for every show/hide/remove operation. Display copies
the source topology into an `AIS_Shape`, so the source `Shape` can be disposed immediately.
Selection returns a caller-owned managed snapshot derived from copied presentation IDs;
no `AIS_InteractiveObject`, selector iterator, or native pointer crosses the ABI.

### B06 strings and sequences

`TCollection_AsciiString` and `TCollection_ExtendedString` are owned opaque values.
UTF-8 input buffers are borrowed only for the duration of one call; UTF-8 output is
copied into caller-owned buffers and no native string pointer escapes. Extended-string
length and character access use UTF-16 code units, while the friendly API exposes
0-based indexing.

`NCollection_Sequence<double>` is an owned opaque container. Creation copies the input
array, clone creates an independent sequence value, and append/set/remove operate on
the native sequence while translating friendly 0-based indices to OCCT's 1-based
contract. Enumerators read values one at a time and do not retain native pointers.

`NCollection_Array1<double>` is an owned opaque container created with OCCT's native
lower bound 1. The managed wrapper exposes a 0-based view and translates each access;
clone results are independent and enumerators read one value at a time. The OCCT 8
`NCollection_Vector<double>` alias is backed by `NCollection_DynamicArray<double>` and
is an owned zero-based dynamic container with the same copy/clone/no-pointer rules.

`NCollection_DataMap<int,double>` and `NCollection_IndexedMap<int>` are owned opaque
containers. Input key/value buffers are borrowed only for construction; map operations
copy scalar keys and values, clone independently, and never return node or bucket
pointers. Indexed-map key/index calls copy values and translate native 1-based indices
to the friendly 0-based view. No native iterator escapes.

## Required lifetime tests

- Create, use, dispose, repeated dispose, and access after dispose.
- Multiple wrappers over a shared native object.
- Base-to-derived and derived-to-base conversion.
- Parent disposed before and after child.
- Managed GC and finalizer fallback.
- Exception during construction, call, and conversion.
- Collection enumeration and early exit.
- Concurrent access where the API is documented as supported.
- Stress loops with native leak and invalid-access diagnostics enabled.

Any change to O001–O012 requires an ADR and corresponding tests.

The package-level StepBasic milestone does not introduce a new ownership category. Discovery expands the
existing O004 generated intrusive shared-handle contract from ten to 129 StepBasic
types. Each wrapper registry owns exactly one retained `Handle<T>` value; `Clone()` adds
one intrusive reference, disposal releases one wrapper, and every generated type is
runtime-tested through 1-to-2-to-1 reference counts and disposed-use rejection.

The generated Geom/Geom2d expansion also reuses O004 without a new ownership category.
Eight additional public wrappers each own exactly one retained `Handle<T>` in a
type-specific live registry. `Clone()` shares and increments the intrusive count;
disposal releases only that wrapper. Scalar, enum, and copied-point parameters/results
do not retain managed storage, and no borrowed curve/surface member is emitted.

The common-modeling milestone likewise introduces no new ownership category. Its 18 audited declarations are
reported as accepted manual bindings because generalized builder/history descriptors do
not yet exist; their runtime behavior reuses O001/O004/O005/O007/O008 and the registered
owning `Shape` category.

The current high-value B workstream adds 43 SC-033 declarations without adding a raw
native ownership category. Curve/surface adaptors, GeomAPI projectors, curve builders,
loft/pipe/sewing/thick-solid builders, topology maps, and Boolean algorithm history are
call-local. Curve/surface evaluations and history summaries are immutable copied values.
Adjacency snapshots own independent `Shape` copies plus managed offset/index arrays;
disposing the source cannot invalidate them, while disposing the map disposes all owned
shape copies. This does not close the entire B batch. Multi-shape input arrays borrow every `SafeHandle` only after paired
`DangerousAddRef` acquisition and release all acquired references in reverse order.

`XdeDocument.ImportStep` mutates only the destination owned document during an open
transaction. Source STEPCAF documents, source labels, XCAF tools, clone maps, and
material maps die inside the native call. Returned `XdeLabel` values are destination-
parent-bound stable entries; commit preserves them, abort may remove their attributes,
and destination disposal invalidates them. No source-document relationship survives.

### Generated placement-allocator shared objects

Types explicitly listed in the current configuration schema under
`placementAllocatorNativeTypes` are still O004 intrusive shared objects, but their native
wrapper owns an additional allocator retention. The allocator field is declared before
the OCCT object handle so C++ destroys the object first and releases the retained
allocator last. Constructor emission requires a non-null generated
`NCollection_IncAllocator` wrapper, uses `new (allocator) T(...)`, and clone emission
copies both retained handles. Ordinary `new`, global `::new`, and a wrapper that stores
only the object handle are forbidden for these types because their allocation and delete
contracts do not match or can free incremental storage during object destruction.

### Batch C common workflow snapshots and presentation state

`Shape.GetTopologySummary` returns only copied counts, Booleans, and tolerance ranges.
All `TopExp` maps, typed subshapes, `BRepCheck_Analyzer`, and tolerance references die
inside the native call. `Shape.CreateDetailedMesh` returns caller-owned arrays of copied
node positions, transformed normals, optional UV values, triangle indices, source-face
indices, and orientation flags. No `Poly_Triangulation`, face, location, or source-shape
relationship survives the call.

`ShapeExchange.ReadBrep` returns a registered owning shape; `WriteBrep` borrows its shape
only for the call. `XdeDocument.AddPart` creates one parent-bound label and composes the
existing copied metadata operations inside the caller's open transaction. Viewer color,
transparency, display mode, projection, zoom, pan, and selection operations mutate only
the parent viewer on its creation thread; presentation IDs never become standalone
owners.

### Batch C import diagnostics, repair comparison, and rotation

`Shape.GetValidationReport` constructs `BRepCheck_Analyzer` and all per-subshape
`BRepCheck_Result`/status lists inside each native call, then returns only copied shape
kinds and status values. The two-call count/snapshot protocol owns no native iterator or
result; the source shape is borrowed for each call. `Shape.RepairWithReport` owns one
independent repaired `Shape` and two immutable managed validation snapshots. Disposing the
result releases only the repaired shape.

`ShapeExchange.ReadStepWithReport` keeps `STEPControl_Reader`, work session, roots, and
transfer state call-local. It returns one registered owning shape plus copied counts,
status, and system-length-unit data. `OcctViewer.StartRotation` and `Rotate` mutate only
the parent viewer's `V3d_View` on the creation thread and introduce no input callback or
standalone camera owner.

### Batch C XDE validation properties, recursive occurrences, and STEP options

Area, volume, centroid, stable entries, and occurrence paths cross as copied managed
values. XCAF attribute handles and IDs never cross the ABI. Attribute writes and clears
use the existing XDE transaction boundary; reads remain parent-resolved by stable entry.

Each `XdeOccurrence` owns one independently allocated composed `TopLoc_Location` and
holds parent-bound occurrence/referred labels. `GetWorldLocation` clones that owner;
`GetLocatedShape` returns a separate registered owning shape. Recursive traversal keeps
component sequences and XCAF references call-local, copies path entries, and disposes all
partially created location owners if traversal fails or detects an active-assembly cycle.

STEPCAF readers, writers, work sessions, metadata tools, and transfer state remain native-
local. Read results own their XDE document; write operations borrow the document only for
the call. Metadata switches and STEP model type cross as validated scalar values.

### Final Batch C selective reader, topology editing, and viewer input

Edge/surface adaptors, `Geom2d_Curve`, trimmed geometry, BRep builders, and
`BRepTools_ReShape` remain native-local. Derivative and pcurve data cross only as copied
scalars and points. Trim, wire, replace, and remove operations allocate independent
registered `Shape` owners and retain no builder, reshaper, input wrapper, or borrowed
geometry object.

`StepReadSession` owns one registry-validated `STEPControl_Reader` wrapper through a
`SafeHandle`. Unit names are copied as UTF-8 bytes into managed immutable collections.
Each selective transfer clears only the reader's transient result list and returns a new
registered `Shape` owner. Transferred shapes do not retain the session and remain valid
after session disposal; disposed sessions reject further transfer or unit access.

`ViewerInputController` is parent-bound to one `OcctViewer` and owns no native resource.
All input methods reach the viewer's existing owner-thread check. Presentation selection
mode remains parent-bound. `GetSelectedItems` copies each selected `TopoDS_Shape` into a
new registered owner paired with its managed presentation; the copied shape survives the
source shape and viewer, while disposing the snapshot never removes the presentation.

### Batch D viewport review boundary

ADR-0064 extends the same parent-bound visualization category without adding standalone
AIS/V3d/Graphic3d ownership. `OcctViewer` owns colored presentations, a built-in filter
handles, and clip planes on its creating thread. Presentation, filter, and clip-plane
references remain parent-bound IDs. XDE occurrence paths/entries and camera/coordinate/
plane/color values are copied. Detected and selected OCCT topology is copied into a new
registered owning `Shape` before detection state can change. Screenshot output returns a
durable file result and no native image object. Alpha.55 runtime and clean-package tests
exercise this boundary through a real STEP/XDE assembly and real HWND.

### Batch E inspection and PMI boundary

ADR-0066 retains native-local exact-inspection algorithms and document/viewer ownership.
BRepExtrema/adaptor/property builders and transient solution iterators do not escape the
ABI. Measurements, solution parameters, classifications, unit values, inertia matrices,
PMI graph records, camera/view data, and display properties cross as copied values;
topology supports cross as independent owning `Shape` copies. Dimension, tolerance,
datum, and target labels are stable-entry children of their owning XCAF document, and
mutation occurs inside document-owned transactions with commit/abort behavior. AP242
reader/writer state is call-local or session-owned. PrsDim/AIS annotations and saved-view
presentation resources are viewer-parent-bound and creating-thread-affine. Dimension,
tolerance, and datum snapshots own copied text/value arrays and independent registered
presentation/target topology; disposal of source topology cannot invalidate them.
Tolerance-datum replacement removes the old graph even when the replacement set is
empty. Preview.2 validates this complete boundary in Release/Debug repository runtime
and the clean 62-DLL package consumer.

### Batch F freeform authoring boundary

ADR-0067 treats every curve/surface definition as immutable copied managed data. Pole,
weight, knot, multiplicity, degree, tangent, parameter, and rectangular point-grid inputs
are validated and copied before native execution. No `TColgp`, `TColStd`, `NCollection`,
native array, pole reference, or generated mutable `Geom` handle is the friendly API's
authoritative definition state.

Interpolation, approximation, projection, extrema, intersection, filling, offset, split,
loft, pipe-shell, analysis, and repair objects are call-local. Copied multi-solution and
diagnostic records have no release path. Every returned edge, wire, face, shell, split
piece, repaired result, and solid is an independent registered owning `Shape`; disposing
any input cannot invalidate it. STEP/XDE labels and AIS/viewer resources retain their
existing parent-bound categories. Preview.3 validates this complete 24/24 boundary in
Release/Debug repository runtime and the clean 62-DLL package consumer.

### Batch G technical drawing boundary

ADR-0068 keeps `HLRAlgo_Projector`, exact/polygonal `HLRBRep` algorithms and extractors,
`BRepAlgoAPI_Section`, `TopExp_Explorer`, and `BRepAdaptor_Curve` call-local. Projector,
plane, sampling, and SVG options cross as copied values. Each visible/hidden
sharp/smooth/sewn/outline/isoparameter layer and each section is an independent
registered owning `Shape`. Projected polyline points, offsets, counts, and closed flags
are copied into caller-owned arrays. No HLR graph, iterator, curve/adaptor, borrowed
topology, or native vector container crosses the ABI. STEP/XDE and viewer resources keep
their document-parent and creating-thread-affine viewer-parent categories. Preview.4
validates this complete 24/24 boundary in Release/Debug repository runtime and the clean
62-DLL package consumer.

### Batch H advanced mesh and scene boundary

ADR-0069 keeps BRepMesh algorithms, Poly triangulations/arrays, XCAFPrs explorers,
XCAF visual-material attributes/tools, and all mesh-exchange providers call-local.
Advanced mesh vertices, normals, UVs, indices, groups, statistics, diagnostics, LODs,
scene nodes, transforms, colors, and material properties cross only as copied values and
caller-owned arrays. XDE labels remain document-parent-bound; a copied `MeshScene`
retains no native document, label, triangulation, iterator, provider, or texture. Shared
definitions are managed-owned and instances contain copied transforms. This boundary is
complete at Preview.5. Advanced meshing first copies the source shape, so LOD construction
does not replace source triangulation. XDE mesh exporters triangulate document roots in
place only for durable provider output; no triangulation owner crosses the ABI. Focused,
Release/Debug, clean-source, and clean-package real-HWND tests validate disposal and
thread behavior across the full 24/24 closure.

### Batch I document state, history, and persistence boundary

ADR-0070 extends the existing `OcafDocument`/`XdeDocument` owning wrapper and stable-entry
label contract without introducing borrowed attribute or delta wrappers. TDF child and
attribute iterators, TDataStd/TNaming handles, reference collections, tree nodes, TDF
deltas, TDocStd undo/redo lists, and persistence drivers remain native-local. Attribute,
reference, history, and document snapshots cross as copied values; optional named
topology is copied into an independent registered owning `Shape`.

Undo, redo, history trimming, dirty state, and savepoint mutation are operations on one
live owned document and are rejected while a command is open. Copied history entries are
diagnostic data, not executable delta owners. Managed dependency graphs are built from
copied stable-entry edges and retain no document or label. Binary/XML/STEP calls borrow
the document only for the call. Preview.6 validates this boundary in Release and Debug,
the focused four-format/STEP-XDE/source-disposal tests, and the clean package consumer.

### Batch J advanced feature modeling, history, and recovery boundary

ADR-0071 keeps BRepFilletAPI, BRepOffsetAPI, BRepFeat, BOPAlgo, BRepTools history,
ShapeUpgrade, BRepCheck, progress, maps, contours, alerts, and iterators native-local.
Input shapes and explicitly selected edges/faces/profiles/spines are borrowed for one
call only. Successful result, modified, and generated topology is copied into independent
registered owning `Shape` values; deletion and source association cross as copied request
indices. Options, diagnostics, and stage messages are copied values.

Recovery validates and may create native-local healed/simplified copies before one retry;
it never mutates or replaces the managed input wrappers. XDE stable entries remain
document-parent-bound and viewer IDs remain viewer-parent-bound and thread-affine. This
boundary is complete at Preview.7. Focused, Release/Debug, clean-source, and clean-package
tests validate copied request association, independent result/history owners, recovery,
STEP/XDE, and real-HWND behavior across the full 24/24 closure.

### Batch K assembly authoring, BOM, and occurrence boundary

ADR-0072 keeps XCAFDoc shape/assembly/metadata tools, TDF labels, attributes and
iterators, XCAFPrs document exploration, assembly graphs, and STEP/STEPCAF sessions
inside the owned document or one native call. Managed `XdeLabel` and occurrence values
continue to address stable entries and cannot outlive or detach from their document.

Structure edits require one named document transaction. Relocation, relinking, removal,
cloning, and reparenting must commit or roll back as a unit. Copied assembly/BOM/where-
used/path/reference/metadata/diagnostic snapshots retain no native label, iterator,
attribute, graph, or tool. Located and referenced topology crosses only as independent
registered owning `Shape` values; locations are independent opaque owners. External
references are copied metadata and do not authorize implicit I/O. Viewer resources
remain viewer-parent-bound and creating-thread-affine. Preview.8 focused 4/4,
Release/Debug 143/143, STEP/XDE, real-HWND, source/document-disposal, and clean-package
tests validate this complete 24/24 boundary.

### Batch L digital mock-up interference and clearance boundary

ADR-0073 keeps Bnd/BVH indexing, BRepExtrema distance state, BRepClass3d classifiers,
BRepAlgoAPI/BOP/IntTools algorithms, explorers, maps, and progress objects inside one
synchronous native call. Input `Shape` values are borrowed only while the call executes.
Pair IDs, AABB/OBB values, witnesses, classifications, occurrence paths, diagnostics,
timings, counts, and aggregation records cross as copied managed values. Exact support,
contact, and overlap topology crosses only as independent registered owning `Shape`
values. Incremental analysis clones reused owning results and never aliases a prior
report. XDE labels remain document-parent-bound; viewer presentations remain session/
viewer-parent-bound and creating-thread-affine. Preview.9 focused 4/4, Release/Debug
147/147, real STEP/XDE, real HWND, source/document disposal, and clean-package tests
validate this complete 24/24 boundary.

### Batch M interactive placement editing boundary

ADR-0075 extends the existing viewer owner graph with a native-local
`AIS_Manipulator` registry. `ViewerManipulator` and its state are addressed only through
one `OcctViewer` and its creation thread; no AIS context, interactive-object, view,
selection owner, or C++ layout crosses the ABI. Presentation removal and viewer disposal
detach dependent manipulators before invalidating their IDs. Returned transforms are
independent registry-owned `GpTrsf` values, while position, appearance, mode, axis, and
activation state are copied values.

`XdePlacementEditSession` owns copied original and pending transforms. Preview changes
only the viewer presentation. Cancel restores the original presentation transform even
when disposal closes an unfinished session; copied transforms are released on every
completion path. Commit rejects scale and mirror, opens one named document transaction,
relocates the occurrence, returns the document-parent-bound replacement label, and
updates the presentation's copied source identity. DMU and STEP/XDE operations remain
call-local and reuse their existing owning/copied contracts. Preview.11 focused 4/4,
Release/Debug, real STEP/XDE, real HWND, source/document/presentation disposal, and the
clean package consumer validate the complete 24/24 boundary.

### Preview.12 STEP/XCAF presentation-style recovery boundary

ADR-0076 keeps STEP models, styled entities, transfer binders, representation graphs,
style decoders, XCAF tools, labels, and indexed shape/style maps inside one native import
or snapshot call. `XdePresentationStyle` receives only copied visibility and RGBA values
plus an independently registered, already located owning `Shape`. Disposing the source
XDE document does not invalidate returned style topology; callers must dispose every
style entry to release that topology.

`OcctViewer.Display(XdeLabel)` borrows the document and stable-entry label only while it
collects styles and creates the AIS presentation. The resulting presentation retains its
own OCCT shape reference and style overrides, remains bound to its `OcctViewer`, and is
valid only on the viewer creation thread. No STEP/XCAF/AIS native handle is returned to
managed or WPF code. The WPF host owns and disposes the resulting presentation list.

### Preview.13 IGES/XDE and Unicode-path boundary

ADR-0077 keeps IGESCAF readers, writers, work sessions, interface models, maps,
iterators, progress state, and diagnostic objects inside one native call. A reader's
transfer document is never exposed directly. The bridge clones every transferable root
and supported metadata into a `TDocStd_Application`-owned destination XDE document, then
managed code exposes only document-parent-bound stable-entry labels. Layer references are
copied explicitly. Reader/session/source disposal therefore cannot invalidate returned
labels; imported topology copied out of a report retains its existing independent owner.

Names, colors, layers, visibility, counts, units, and diagnostics cross as copied values.
Managed non-ASCII path staging owns unique temporary files, promotes output only after
successful native completion, and cleans success/failure/exception paths. No temporary
path or file owner escapes to callers. The WPF host owns the imported XDE document and
viewer-parent-bound presentations exactly as it does for STEP.

## Boolean topology-history companion

BooleanHistoryModeling holds SafeHandle references to every RepairSnapshot input
while native InputGraph copies the entire graph. Builders and history iterators
remain call-local. Existing FeatureResult and Shape owners publish the result and
history shapes; input disposal after Build cannot invalidate outputs. History
result indices refer to the exact final graph, with final traversal orientation.
There is no new registry or borrowed pointer; existing release paths are reused.

## Batch AE exchange scalar owners

`IGESData_GlobalSection` and `StepData_Field` use ADR-0096's typed owned tokens,
call-duration leases and typed release. The IGES constructor implementation in
the locked OCCT 8.0.1 SDK initializes all selected scalar fields, despite the
header's general warning. The STEP constructor clears its scalar state.
No string handle, borrowed pointer, list storage or mutable output crosses this
new surface. See [Batch AE](BATCH_AE_EXCHANGE_SCALAR_VALUES.md) for the exact scope.

## Viewer view cube

`OcctSharp_ViewerHandle` owns its optional `AIS_ViewCube` and two curved-arrow
`AIS_Shape` handles. Visibility adds or removes them from that viewer's
`AIS_InteractiveContext`; the arrows are additionally removed during oblique
views. Release clears the context before nullifying all three handles. Hit
testing copies only an orientation or roll-action value, and the managed facade
never receives an OCCT owner handle. All operations retain the viewer's
creating-thread restriction.

## Cadoryx C3 plane reflection ownership

Plane mirror creation allocates the existing registry-owned TransformHandle;
GpTrsf owns it through its existing SafeHandle release. Failed creation leaves
the output null. Shape.Transformed borrows the source and transformation for
the call, uses BRepBuilderAPI_Transform with copy enabled, and returns an
independent owning Shape. Disposing either input does not invalidate the
result. No new registry category or borrowed pointer is introduced.

## Exact transformed XDE definition copy (Cadoryx C3, 2026-10-02)

The synchronous call borrows both document SafeHandles and the existing GpTrsf
SafeHandle. The builder copies owning geometry and keeps native history only
inside the call. Destination labels remain parent-bound to the destination XDE
document; disposing or removing the source does not invalidate their topology
or metadata. Physical materials/layers are recreated through XCAF metadata copy;
a call-local visual-material map creates independent destination material
records. Only the stable destination entry string crosses the ABI. Failed
operations require aborting the open destination transaction, so no partially
copied definition may be published. No borrowed shape pointer, history handle,
registry category or release API is added.

## Copied self-interference report (Cadoryx D2, 2026-10-01)

The call borrows the source Shape SafeHandle only during synchronous execution. BRepBuilderAPI_Copy(copyGeom=true, copyMesh=false) isolates source geometry from analyzer cache updates. The analyzer, its fault list and all copied topology die in the native call. Only two int32 fault-record counts cross the boundary; output counts are zeroed before guarded work and assigned after successful analysis. ShapeSelfInterferenceReport holds no native handle, borrowed pointer or lifetime dependency. Algorithm errors throw rather than returning a clean report; other/unknown fault records keep IsClear false. Disposed managed Shape inputs reject before native entry.

## Adaptive knot-span inspection (Cadoryx B9, 2026-10-02)

Inspection borrows the Shape SafeHandle synchronously. If finite continuity
requires partitioning, BRepBuilderAPI_Copy(copyGeom=true,copyMesh=false) creates
independent call-local geometry before ShapeUpgrade modifies any topology.
Partitioned shapes, contexts and GProp objects die in the call; original topology,
locations and cached reference identities remain untouched. The existing property
record and new AdaptiveShapeInspection contain copied scalar values only. No
registry category, owning/borrowed pointer, native result lifetime or release API
is added. Failed adaptive calls initialize outputs to zero and publish scalars
only after a successful computation.

If the pinned continuity divider fails during volume inspection, its partial
result is discarded. Span-aware GK reads the validated independent geometry
copy; all numerical objects remain call-local and the original is untouched.

SketchModeling.InspectIntersections uses call-local copied 2D curves and a local
intersector. Point/interval counts and endpoint values are independent scalars;
SketchIntersectionInspection owns a read-only copy and no native handle. The
explicit pair call remains separate from legacy same-object self-intersection.

## Cadoryx A5 viewer-owned source snapshots (private candidate, 2026-10-06)

The isolated `.12` candidate introduces source creation IDs in the existing
viewer. The managed `ViewerSource` is parent/thread bound and owns no borrowed
pointer or independent SafeHandle. Its viewer owns copied native topology,
effective XCAF styles and exact original/copy correspondence. Instance entries
retain the snapshot after a source creation entry is disposed or cleared;
existing instances remain usable, while new instances from that source reject.
Source IDs are globally non-reused; foreign/stale IDs reject. Presentation
removal drops its retained source. Viewer disposal removes graphics and clears
both source and instance maps. Original shapes are kept solely for exact
selection correspondence, not rendered or modified by viewer operations.

Mutable per-instance drawers are copied; style edits detach shared graphics.
Selected/detected topology uses exact TShape partnership plus all twelve
location coefficients to return original topology; ambiguous/missing matches
fail. Imported texture ownership and source-revision refresh are not yet
accepted. Direct C++20/20 and frozen source-snapshot SDK Release741/741
(eight added cases, focused9/9) are separate evidence; all62 actual loaded
native DLLs match that frozen candidate. Debug, package consumers,
application and performance remain separate gates.

## Cadoryx A5 explicit viewer-source style revisions

Refresh borrows the XDE SafeHandle for one synchronous, viewer-thread call.
The viewer retains its detached topology and exact original/copy map. Root
identity, orientation, placement and full membership are checked before styles
are staged in a new private XCAF document. Each existing instance gets staged
owned drawers and explicit override replay; no presentation object is replaced.
Appearance originals point to the new source defaults, and textures remain
viewer-owned. Handle/map swaps retain IDs, transforms and manipulators. Exact
selected native owners are reused only where the same copied topology remains
sensitive; newly hidden topology is deselected. Released source creation entries
cannot be refreshed; retained instances keep their last source snapshot.
Frozen2 Release750/750 and17 focused tests exercise this finite source contract;
22 direct native cases independently prove selected owner retention and hidden
sensitive removal. Complex imported textures and rendering failure injection
remain separate unverified gates. Cadoryx integration is not yet performed.

## Cadoryx A5 source texture bytes and retired cached graphics

SourceTextures.hxx snapshots caller encoded buffers, external files and file
slices before publishing the source. Each copied Image_Texture owns its own
NCollection_Buffer and has a globally non-reused GPU cache ID. A call-local map
reuses the exact same owned texture when multiple material slots share one
original handle. Common/PBR scalar fields, alpha/culling remain copied values.
The original texture handle, mutable buffer and file path are not retained by
the source styles. Failure leaves the source unregistered; refresh stages every
texture before exchanging live instance state. Texture errors use the existing
FILE_IO_ERROR status; no new native resource handle or ABI export is introduced.

Source style refresh recomputes every cached graphics mode before releasing the
previous source. A mode0 cached at initial Display cannot keep an OpenGL instance
pointer to a retired source after mode1 refresh. Instance and exact selected
owner identities remain; source release/removal are exercised after actual STEP/
IGES import. Frozen3 Release755/755 and22 focused cases pass; Debug and complete
import/material/driver failure evidence remain pending.

## Batch AK transfer records

ADR-0103 adds private Session, immutable Snapshot and functional ShapeBinder registry owners. SafeHandle leases serialize session mutation and protect release races. Snapshots copy container and diagnostic storage and retain intrusive entities/TShapes; no deep geometry immutability is claimed. Model and map identifiers carry different types and a session identity. Validation preserves the epoch; transfer/publication failure retires it. Native record trees with children are rejected before recursive SDK calls.

## Batch AS document and XDE attribute projections

AS [ADR-0111](adr/0111-generated-document-and-xde-attribute-projections.md) adds
parent-bound document projection contexts and detached typed data. Every native
call pins the existing parent SafeHandle, validates its owner domain and rejects
cross-context or removed label/note/layer inputs. Context disposal never releases
its parent. Atomic writes use a private nested transaction inside the existing
open document command. Copied payload owners use typed registry leases; copied
maps, arrays, Unicode and associations survive all owner disposal.

SC-062 application-local persistence adapters never modify the storage source.
They encode bounded complete data beside an SDK-readable safe subset and decode
fully before replacing a retrieval target. Only OcctSharp-aware drivers promise
full extension fidelity; legacy/external readers can lose extended values.
XML note adapters preserve existing attribute identity and associations, with
copied text extensions. No global driver registry or user callback escapes.

## Batch AL numeric matrices and private solvers

AM extends this copied-result boundary through [ADR-0105](adr/0105-private-spline-workspaces.md).
SplineWorkspace retains immutable one-based private arrays under a typed registry
lease and SafeHandle. Creating it deep-copies source geometry; callers synchronize
source mutation during the copy. Results are detached managed grids. Interpolation
keeps all coordinate/weight work arrays private and publishes one combined result
only after every finite/positive-weight check passes. C ABI buffers must be valid
and disjoint; lengths never authorize arbitrary pointers.

[ADR-0104](adr/0104-copied-numeric-matrices-and-private-solvers.md) reuses MathMatrix ownership; functional results own deep native copies, and managed snapshots own copied arrays. Existing mutable MathMatrix inputs require caller synchronization. Gauss/Householder/SVD receivers and all work buffers are private to one call. SplineMatrixSystem retains immutable original/factored matrices under typed registry leases and SafeHandle disposal. Weighted coordinates and weights publish together, with no partial solution on native noncompletion. Native C ABI output buffers must be valid, writable and disjoint; checked lengths never authorize arbitrary pointers.

## Curved hatching workspaces (AQ)

[ADR-0109](adr/0109-generated-copied-curved-hatching.md) retains managed input handles across copying, keeps native hatcher/intersector private, and stores only independent curves plus copied event/domain data in typed leased tokens. Optional endpoints preserve absence; each trimmed result is deep copied. Synchronize source mutation while copying.

AR / ADR-0110: TM023 retains private deep-copied triangulations and freezes channel,
bounds and conditional adjacency arrays. Every triangulation query deep-copies;
mapped arrays never escape. TM024 retains an independent exact meshing shape,
freezes effective parameters/status and gates each independently copied mesh shape.
Native handles remain retained during source copying; source mutation is caller
synchronized. Only inactive default progress ranges are used, with no callbacks.

AT / ADR-0112: TM027 owns a private STEP/IGES reader and serializes operations.
Opaque tokens use registry leases; selection IDs carry a unique managed session
domain. Detached headers/entities/checks own their data. Export prepares copied
header attributes before swapping private model metadata, restores original
handles on success/failure, and retires the receiver if restoration fails.
No raw model/entity/selector or copied header alias is publicly exposed.

AU / ADR-0113: TM028 retains private copied camera/frustum data in typed leased
tokens. A managed parent SafeHandle and validation delegate strongly retain the
Facade Viewer; creating-thread checks precede queries/input state mutations.
Exact conservative source stamps reject changed camera/client/scene/mode/filter
state. Metadata owns immutable copied arrays and survives retirement. Volume
disposal needs only its creating thread and remains valid after parent disposal.
The Facade adapter resolves copied IDs to existing presentations; world-pick and
selected-item source identity is frozen when copied. General shared geometry
mutation remains caller synchronized.

AX / ADR-0116: Private imported documents call BeforeClose before Data destruction; application-backed owners already close through Application.Close. Bare generated receivers cannot create naming-backed views. STEPCAF initializes both managed interop resolvers before reading or releasing payloads. View subtree attributes are explicitly backed up before nested deletion so caller abort/undo/redo restores their identity and associations. The generated XML color driver adds bounded exact RGBA to the existing SC-062 local persistence boundary; legacy readers see nearest named colors. No SDK/global driver mutation or sidecar is used.

Extend the exact private workflow catalogue to existing identity-checked XCAF
documents and fresh call-private STEPCAF sessions. Reuse parent SafeHandles,
context identity and command-atomic mutations. Detached immutable metadata owns
its values; an imported generated document owns its native data independently.
Borrowed tool/session/container APIs and inferred topology history stay excluded.
The single bridge and existing registry/error contracts remain unchanged.

AY / ADR-0117: Extend the exact copied workflow catalogue to initialized private polygons and
conservative native-only simple-cycle construction. An explicit index domain
binds the polygon to one owned triangulation copy; no topology provenance is
inferred. Native helper lifetimes are call-scoped and ambiguous branch choices
fail. Detached buffers and repeated native copies retain the established owned
registry/error and single-bridge contracts.

AZ / [ADR-0118](adr/0118-audited-foundation-function-diagnostics.md): TM035 uses
initialized call-private matrix/message/polynomial inputs and detached immutable
output buffers. Optional numerical absence contains no matrix or eigenvalues.
Private message tokens use typed registry leases and SafeHandle disposal; copied
strings survive release. No global message lookup, borrowed receiver or SDK work
array escapes. Exact header-function provenance is independent of ownership.

BA / [ADR-0119](adr/0119-initialized-assembly-and-copied-validation.md): TM036
initializes private builders before queries and deep copies input graphs.
UV pcurves share one private surface through face construction. Results own
independent topology; every shape query deep copies again. Copied failure and
validation diagnostics survive SafeHandle disposal. No input history is inferred.

BB / [ADR-0120](adr/0120-bounded-native-curve-computations.md): TM037 copies bounded geometry before private native solver/sampler use. Typed result tokens are consumed and released immediately; detached points/parameters/projection diagnostics survive source disposal. Native-only evaluation callbacks never cross the ABI.

BC / [ADR-0121](adr/0121-identity-bound-document-association-diagnostics.md): TM038 uses the existing exact document/context identities and nested transactions. Copied GUID/naming/assembly/dimension/clip diagnostics outlive the parent. Direct descriptors are fully initialized; no borrowed tool or topology-history rebind escapes. BeforeClose and AX exact RGBA protocols remain retained.

## TM039 Private identity-bound selected-root transfer

A private XSControl reader is explicitly initialized to STEP or IGES. The private
STEP/IGES actor is bound to the exact model; shape-fix parameters belong to that
actor. No Interface_Static values, WorkSession, model/entity handles, mutable maps,
streams or callbacks are exposed. Existing AK/AT/AN/AX workflows are reused.

Root IDs bind to one workspace GUID and one hash-verified source file. Root-rank,
model-number and entity routes retain selection order and duplicates. Invalid
selection does not consume the one-transfer epoch; native exceptions retire it.
Native false, null and empty roots publish no partial topology. Each GetShape
returns an independent geometry-deep copy. Detached root/load/transfer diagnostics
and repair parameters survive disposal. External-file traversal is not implemented.

Source files are bounded to 128 MiB, models to 1048576 entities, selections/roots/
published shapes to 4096 and copied text to 16 MiB. Lazy selections stop at 4097
and dispose the enumerator. Synchronize calls per workspace and source-file edits.
Acceptance uses real STEP/IGES selection, all three transfer routes, different
private configurations, exact foreign/file identity rejection, a real empty-root
STEP fixture, copied lifetime and selected export with independent readback.

## TM040 Exact copied mesh boundary diagnostics

A copied Poly_Polygon2D preserves the input-bound metadata separately from the
SDK's one-based normalized native bounds. Inputs are finite and bounded; closure
is explicit. Nodes and triangles of one private deep triangulation copy define
the only index domain used for quality, incident-edge, boundary, nonmanifold,
orientation and unreferenced-node diagnostics. No external topology is inferred.

Native coherent free-node diagnostics are used only for a proved manifold,
consistent-orientation, distinct-index profile. Native zero-based indices convert
to the private one-based domain and must match copied triangle references;
unsupported adjacency returns null. Each CopyTriangulation is independent and
preserves the verified node/triangle ordinal order.

Exact private copied faces expose BRepMesh native 2D/3D ranges, UV ends and maximum
tolerance. Bounds are 4096 polygon points/faces/edges, 16384 mesh nodes/triangles,
16 MiB output and finite coordinates at most 1e12. Synchronize source mutation
while copying. This profile belongs to Mesh, which already references MeshData
and Modeling; no reverse module dependency is introduced.

The full workflow executes BD root selection, controlled meshing, exact quality
and UV diagnostics, BC reference persistence, saved occurrence identity, actual
HWND mouse selection and more than 100 changed DXGI client pixels, then visibility
and transform stale-volume checks. The direct-module consumer retains the accepted
raw native Viewer workflow. Model-only assertions do not substitute for pixels.

## TM041: finite copied numeric aggregates

[ADR-0124](adr/0124-finite-copied-numeric-results-and-sdk-supplements.md) uses initialized call-private matrices, optional fields checked on SDK completion, staged finite outputs and independent copied grids/lists. Failure publishes no partial aggregate. LU pivots are private one-based row ordinals; no borrowed result or mutable solver state escapes.

## BG private modeling results

[BG](BATCH_BG_MODELING_RESULTS.md) stores a geometry-deep-copied input graph and success-only result in typed registry storage. SafeHandles retain leases during reads; each queried Shape is independently deep-copied. Detached configuration/source records have no native lifetime. Private section interference borrows never leave the executing call; ambiguous history cannot rebind topology.

## BH finite and coupled intersection results

[BH](BATCH_BH_INTERSECTION_RESULTS.md) creates private initialized solvers with copied input shapes/surfaces/curves. Typed registry leases and SafeHandles own immutable result storage; queried faces, source surfaces and coupled 3D/pcurve tuples are deep copies. Missing fields remain absent, tuple construction rolls back on failure, and detached records outlive disposal. No original source is automatically rebound.
BI/TM045 direct collections retain parent/context/thread restrictions; typed scalar/text copies are detached, exact label references keep issuing context identity. Mutations require caller command, prevalidation and nested rollback; no container address escapes. See [BI](BATCH_BI_DIRECT_COLLECTIONS.md).
BJ/TM046 owns one initialized selected-root STEPCAF epoch and immutable private staged files; copied source/root/status/occurrence diagnostics outlive the workspace. Only fully successful transfers publish close-safe owned documents; deep-copied shapes are detached. See [BJ](BATCH_BJ_LOCAL_STEP_ASSEMBLY.md).
BK/TM047 owns configured mesh readers/writers and bounded immutable file/material diagnostics. Complete close-safe documents and independently triangulated copies are the only output owners. Exact definition replacement copies geometry/triangulation under caller commands; topology history is not inferred. See [BK](BATCH_BK_CONFIGURED_MESH.md).
BL/TM048 owns copied curve/surface/vertex inputs and frozen constructor results. Native errors expose no partial topology; every public shape query deep-copies geometry. See [BL](BATCH_BL_PARAMETERIZED_CONSTRUCTION.md).
BM/TM049 retains private independent curve/surface geometry, local bounded adaptors and success-only coupled output copies. Detached rows outlive disposal; Geometry computation and the Modeling edge adapter preserve the dependency graph. See [BM](BATCH_BM_FINITE_EXTREMA.md).
BN/TM050 confines native string mutation to one private value, copies UTF8/UTF16 lengths and text, and publishes bounded managed batches only after all copies succeed. See [BN](BATCH_BN_BOUNDED_TEXT_TRANSFORMS.md).
BO/TM051 owns independent face geometry and triangulations; native maps/BVH/tools stay private. Metadata is copied and native references/statuses require completed layer mode. See [BO](BATCH_BO_MESH_PROXIMITY.md).
BP/TM052 builds one selected-root clone map and deep geometry graph; writers/maps/files stay private. Complete canonical outputs and exact SHA256 are published by one adjacent directory rename. See [BP](BATCH_BP_LOCAL_STEP_EXPORT.md).

## Cadoryx A5 private SDK28 rendering health (2026-10-11, candidate only)

The [private SDK28 workspace](../OcctSharp/artifacts/cadoryx-a5-render-health-28-draft-1/workspace-preparation-1.json)
adds no new native owner. `GetRenderingHealth()` leases the existing viewer for
one owner-thread call and copies one bounded integer flag mask. The const
reference to a shared `OpenGl_Context` stays inside the native call; neither that
context nor the driver's or structure manager's address escapes. Failed reads
publish zero for a valid output pointer and retain the original ownership error.

`InvalidateGpuResources()` explicitly changes only the existing viewer's OCCT
presentation reupload flag. Presentations, source snapshots, exact selection
identity, camera and caller-owned geometry retain their existing ownership and
disposal rules. Repeated marking is not permission to replay modeling commands,
rebind topology or reconstruct a document. It is not context destruction or
physical GPU removal.

The isolated WPF candidate uses exact current host/viewport/document/attachment
epoch checks and at most one automatic attempt per external attachment. It
retains the primary exception and additional cleanup errors, attempts each
subscription/animation/popup/capture/progressive/native cleanup, and rebuilds
the current scene/camera without model-command replay. A disposed-owner allowance
checks the complete known cleanup exception graph and must not hide another
failure. The 30-second readiness observation is not native-call preemption.
WPF remains a source contract, not accepted behavior: exact-owner
exception naming, actual two-pane/closing/failure resource tests, subsequent
modeling/history/save/reopen, full images and zero assets remain NOT RUN.


SDK28 build/runtime continuation (2026-10-11): both actual native/managed0,
exports+2/API+8 and actual focused12 each passed;20 full320 PNGs directly reviewed.
All1031 sources including generated4176 forwarders are exact: the copied health
enum lives in the facade and needs no self-forwarder. Original false guard FAIL
was retained and the same completed build independently audited without replay.
Full Release1358 passed; Debug full status is tracked in STATUS. Actual missing
or invalid context fault injection, physical GPU removal, private packages,
public consumers and WPF automatic recovery remain separate NOT RUN gates.

## BV call-private projection helpers

TM055 owns SDK projector receivers only on a native call stack. Generated getters
copy points/scalars/UV and never publish a borrowed owner. The five existing
callers retain source validation, coordinate transforms, clipping, sorting,
errors and publication. Periodic finite windows in the sampling template retain
parameter/point/distance coupling after source release; nonperiodic domains and
maximum one period remain enforced. See [BV](BATCH_BV_PERIODIC_PROJECTION.md).

## BW copied nonuniform fitting and private adapters

TM056 emits stack-only initialized fit/patch receivers. Handle getters return owning
handle copies by value. Two existing callers keep all validation, input copying,
topology construction, errors and publication. The parameterized filling entry
copies finite strictly ascending section parameters into a private1-based SDK
array; the managed result retains a separate readonly snapshot, including failure.
No borrowed receiver or caller array escapes. Legacy/sweep snapshots are empty.
See [BW](BATCH_BW_NONUNIFORM_FITTING.md) for strategy boundaries.

## BX typed pcurve construction and private face adapters

TM057 keeps initialized MakeFace receivers on a native stack. SDK TopoDS reference
getters return owning topology values. Eight old callers retain validation, input
graphs, transforms, options, errors and publication. Typed2D spline/surface copy
entries validate their own registries, apply4096-element complexity budgets and
publish independent base owners. Nine typed/mixed pcurve overloads retain these
owners through existing private constructors; no token reinterpretation or source
alias escapes. See [BX](BATCH_BX_TYPED_TOPOLOGY.md).

## BZ private local-feature, sweep and offset builders

TM059 keeps ten typed SDK builder families on the native call stack. TopoDS
returns are owning values; copied list containers preserve order, repetitions and
orientation within a1000000-entry budget. They do not imply deep geometry copies.
Private Sect handles and array lower bounds stay within the call; existing
contour callers copy circles before builder closure under the65536-section
budget. Nine callers retain their original input isolation, state/option guards,
failure/history and publication policy. Simulation and construction use fresh
PipeShell receivers per operation, as before. No mutable SDK receiver or borrowed
address escapes. See [BZ](BATCH_BZ_LOCAL_FEATURES.md).

## CA private mesh adapter ownership (TM060)

Primary operations stay inside the call-private mesh profile; they are excluded
from the generic public shared-handle emitter. Reused Emitted signatures retain
their previous public outputs. Triangle indices copy into scalar DTOs before
owner closure; coherent mutation pointers stay in the original native call.
MergeNodesTool composes a private transient receiver and clears its output without
returning a mutable SDK reference. Mesh approximation does not replace exact CAD.

## CB private document delegates (TM061)

The125 admitted operations retain their exact SDK native receiver/return types.
Borrowed labels, attributes, strings, maps and history entries stay in the
original call/parent scope. Private iterator/naming/storage bases preserve SDK
construction and runtime type identity. Typed lookup retains pinned missing and
wrong-type semantics; NamedData driver Paste uses qualified base dispatch.
The29 audited callers retain command, foreign-context, thread/epoch where
applicable, bounded copy and publication policy. No C ABI or public mutable
managed receiver is added. See [CB](BATCH_CB_DOCUMENTS.md).

## CC private XDE delegates and shared located clones (TM062)

Borrowed colors, PMI, material, shape/assembly labels and view returns remain in
their original private call/parent scope. Exact value factories retain SDK base
copy semantics; the private XML base retains SDK dynamic type identity.
Selected exports keep one source/destination-domain-checked exact clone map and
independent geometry. Located definition placement is pushed once onto outgoing
components; shared definitions and label identity stay intact. Failure discards
the unpublished clone. No public mutable SDK receiver or C ABI widening occurs.
See [ADR-0137](adr/0137-private-located-assembly-normalization.md).

## CD exact STEP delegates and configured writer

TM063 replaces11 exact Manual overrides under the original private reader/call
owners. No new manual binding exception is added; shape/reader registry release,
finite units, root mappings, exception Guard and success-only publication remain.
Configured STEPCAF uses the existing dual document-domain context and projection
payload owner. No session/SDK model/attribute handle escapes; the file is staged
before replacement. Precise supported and lost fields are documented in
[CD](BATCH_CD_STEP.md). No public architecture/ownership category changes.

## CE private exchange adapters and owned mesh resources

TM064 replaces34 precise Manual declarations without exposing mutable sessions;
six provider session overloads remain deferred. Original policy receivers retain
validation, exception and publication behavior. TM065 keeps buffer write pointers
call-local and embeds Image_Texture/NCollection_Buffer handles in the native
material graph, independent of managed result/temp files. Targeted texture snapshots
are copied immutable managed data. See [CE](BATCH_CE_EXCHANGE.md) and
[ADR-0138](adr/0138-owned-mesh-resource-import.md). No new Manual StableId is added.
Existing manual IO infrastructure ExchangePathStage is compiled once in Runtime
and reused by facade IGES and geometry-only STL; its implementation is unchanged.

## CF exact private Viewer and document-identity adapters

[CF contract](BATCH_CF_VIEWER.md) retains existing parent/thread/epoch ownership.
Graphic3d_ClipPlane factory preserves the original fresh SDK allocation followed
by immediate viewer-owned handle-map transfer. GUID values remain call-private;
attribute DynamicType and camera/aspect references remain in their original
native lease/call scope until immediate copies. CancelPointerGesture checks the
viewer thread/lifetime before clearing only managed gesture state.

## CG context observation and render-resource generation

TM068 preserves original viewer-owned resource/copy/allocation types and borrowed
SDK return scope. Generated health API returns only a copied0..7 mask on the live
owner thread; queries neither clear flags nor redraw/recreate. Explicit reupload
marks only SDK presentation loss. Existing facade and Modeling interop owners remain.
Test-only context-slot instrumentation restores parked context only for teardown;
recovery creates a fresh production host with retained owned model/asset copies.
No graphics/context pointer is exposed by the public product.

## Preview85 copy/history and exact STEP ownership

See [ADR-0140](adr/0140-public-consumer-copy-history-and-exact-exchange.md).
AuthoringInputCopy owns copied Shapes; reports contain immutable values and
independent source/copy/final domain IDs. Existing FeatureResult owns native
report payloads and actual history source shapes until managed copies are read.
TopExp maps collapse orientation/shared occurrences; ambiguous membership stays
explicit and does not authorize an inferred occurrence or reference rebind.
Defeature retains RepairSnapshot source-domain checks and owning LocalFeatureResult.
Exact STEP read/write scopes keep private geometry/document graphs and release
their application-local copy owners. Units and ordering are tied to actual
representation contexts and original root sequences. Periodic distance remains
call-private and returns original support identities/parameters only.
Initial display retains viewer-thread/source lifetime and isolation tracking;
the new native exports require public ABI 1.118.

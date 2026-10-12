# BX: typed pcurve construction and generated face adapters

Baseline: frozen BW Preview.72 / ABI1.112 / bridge0.121.0.
Candidate: Preview.73 / ABI1.113 / bridge0.122.0.
State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.

TM057 emits12 exact MakeFace/MakeShape SDK calls as initialized native stack
adapters. TopoDS reference getters return owning values. Eight caller files keep
their original validation, input graphs, transformations, errors and publication.
Other manual MakeShape callers remain unchanged; no generic receiver is exposed.
The [admission](BATCH_BX_TYPED_TOPOLOGY_ADMISSION.json) converts12 Manual identities
to Emitted; newly implemented SDK identities and coverage gain are0.

The parameterized-construction template emits typed2D spline and typed surface
copy entries, producing independent base owners through matching registries.
At most4096 poles/knots are admitted (surface poles are a coupled product).
No pointer reinterpretation, borrowed alias or silently changed strategy is used.
Nine typed/mixed overloads cover parameter-only, points and vertex profiles.
Their private copies feed the existing finite constructor/error/orientation and
coupled point/UV result contracts. Eight existing Copy/budget queries are reused.
Three admitted generated outputs change,318 old outputs stay unchanged and one
Modeling helper is new. No existing public API/export is removed.

Runtime and both consumers cover source mutation/closure, descending orientation,
all nine profiles, independent surface/face and actual STEP hole-area12 readback.
The Viewer consumes that file through a real HWND: outer-ring hit, hole-centre
miss, selected presentation identity, DXGI repaint and visibility invalidation.
Native probes compare all12 adapters directly with the SDK, including holes,
and C11 probes reject null/unregistered sources with cleared publications.
The first helper build lacked a complete TopoDS_Face include; the template was
corrected and regenerated. A native fixture's missing gp_Pln include was also
corrected; original failure logs are retained and assertions remain.

The initial unified entry also found CS0246 in the direct-module fixture: the
facade alone owns SketchCurveChain2d/SketchProfile2d/SurfaceModeling. The facade
and Runtime keep that public path. The module fixture uses the same existing
public surface C ABI to lift the exact UV loops, assemble wires and trim a face,
with an explicit clockwise hole, through a STEP-copied support face. It still
calls the packaged generated typed pcurve API and retains every area, topology,
lifetime and actual HWND/DXGI assertion. Its native declarations are test
infrastructure and receive no new binding credit. Failed fixture compilation
and initial descriptor/topology-kind probes remain in the batch evidence.

The corrected module fixture passed its whole public workflow and actual frame:
1612 changed pixels, ring hit, hole-centre miss, exact selected presentation and
visibility invalidation. A hash-guarded continuation rebuilds both Runtime
projects, reruns both complete suites and executes the original package/cold/
inventory/API suffix; the initial command's failure remains separate. Unchanged
native/source/API auxiliary evidence is retained under529 input hashes.

Actual Debug starts only when observed peer testhosts have no visible window;
peer processes are never modified. Final before/selected frames in all four
contexts must exactly match the inspected fixture's complete model/selection
region `[16,16,344,344)`. This includes the entire model, hole and selection
lines plus surrounding background. Full PNG identities are still frozen. The
36 differing pixels found between inspected facade/module fixtures were solely
at rounded desktop window corners outside this region. A bounded PNG decoder
checks dimensions, RGB format, CRC, row filters and decompression length; positive
and malformed/changed-model fixtures verify that unrelated desktop corners do
not become model evidence. Original pixel/hit/identity/lifetime assertions remain.

The first isolated actual Debug run passed1392 tests and62 loaded DLL identities,
but strict frame comparison rejected its initial window-animation pixels. That
run is retained as historical evidence and does not close the visual gate.
The shared fixture now waits before input for three consecutive unchanged model
frames, with a ten-second bound. Selection still uses the original immediate
capture, without an added redraw or post-input wait. The corrected focused
TypedAssemblyHolesAndActualStepReadback test passed1/1 and both complete model
regions exactly match the inspected reference frames. An earlier mistyped filter
matched zero tests; its log is retained and receives no passing-test credit.
Both Runtime configurations and the cold/package suffix passed against this
fixture. The new isolated actual Debug run passed1392/1392 with62 observed DLL
identities, and both actual Debug model/selection regions exactly match the
inspected references. Final package/source freeze remains the delivery authority.

The subsequent unchanged full actual Debug run passed1392/1392 and62 observed
DLL identities. Its final image guard caught a separate evidence-routing error:
the legacy BE mesh test used the BX output directory and overwrote the BX PNGs
and JSON. The mixed artifacts and failing controller log are retained. BE now
uses its own legacy directory. A focused BX test passed1/1 in the exact same
isolated directory, with unchanged full TRX and DLL hashes,62 newly observed
Debug modules and exact model/selection frames. This is separate recapture
evidence, not a replacement full-suite result. Two recapture-controller failures
(an overly broad directory prefix and a report-writing error) remain archived.

Release/Debug/cold, actual Debug-native, ABI/lifetime, packages and source freeze
remain required. Hosted CI, signing, push, public NuGet, external CAD and full
downstream application acceptance are NOT RUN without independent evidence.

Local evidence:

12 admitted / 0 deferred, 0 new prerequisites / 8 reused; generated +12 / Manual -12 (new SDK declarations 0, Manual to Emitted 12).
Release/Debug/cold Generator 332, Runtime 1392: PASS.
Actual Debug-native 1392, 62 actually observed DLL paths/hashes: PASS.
Exports 41464 (+2/-0), Debug parity; API 58938 (+11/-0).
All 318 baseline generated files unchanged; current generated files 322.
Explicitly audited modified generated files: 3.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bx/delivery-result.json PASS completes BX.

# Batch AQ: curved hatching and copied domain workspaces

Baseline: locally delivered Preview.40. Delivery: Preview.41 / ABI 1.84 / bridge
0.92.0. Full local gates PASS; final identities are frozen in
OcctSharp/artifacts/batch-aq/delivery-result.json.

[Admission](BATCH_AQ_HATCHING_ADMISSION.json) audits [50 exact candidates](BATCH_AQ_HATCHING_CANDIDATES.json):
20 admitted, 30 deferred, seven new inherited event prerequisites and ten reused
Emitted/Manual prerequisites. Expected generated delta: 27 Blocked→Emitted IDs.
[ADR-0109](adr/0109-generated-copied-curved-hatching.md) defines TM022.

## Outcome and boundaries

GeometricHatching.Scan accepts owning lines, circles, BSplines, Beziers, trimmed
and generic 2D curves with finite requested ranges. Forward/Reversed boundary
orientation is explicit. All source SafeHandles remain retained during deep
copying; mutation requires caller synchronization. Native intersector and hatcher
are call-private. No raw adaptor/intersector or mutable reference escapes.

Copied scalar events preserve hatching parameters, boundary ordinals/parameters,
position, before/after state, segment flags and intersection type. All related
boundary events remain present at shared vertices. TrimDone, TrimFailed, per-curve
IsDone and native error status remain separate. Successful empty output is valid;
failed domains cannot be queried. Point/segment inclusion flags are explicit.

The pinned header declares a no-argument IsDone without a pinned definition/import
symbol; a native link probe fails. It is deferred with its exact signature, never
claimed emitted. Overall completion requires each per-hatching IsDone flag,
TrimDone, !TrimFailed and NoProblem; an empty list is complete. The SDK can retain
an error status even with IsDone=true. Preserve both native diagnostics, expose
no domains and keep overall completion false in that case.

Directed open boundaries may produce valid semi-infinite domains; this workspace
does not validate contour closure. Failure fixtures use verified incoherent
coincident segments and crossing states rather than assuming every open contour fails.

Original native domains can omit either endpoint; optional First/Last stay null.
Requested finite ranges clip independent trimmed outputs while preserving native
endpoints. Point-only/empty clips return no curve. Each 2D/planar 3D query deep-copies
the retained hatching geometry. Geometry returns curves, then existing AP Modeling
builds topology; this introduces no Geometry→Modeling dependency.

Indices are zero-based publicly, bounded before native one-based access. All input,
pair and expanded event/domain capacities are bounded. Insufficient raw buffers,
stale or wrong-typed result tokens fail before array writes. Results and copied
diagnostics survive source disposal; queried geometry survives snapshot disposal.

## Required evidence

Analytic rectangle/circular-hole references, empty/outside, point tangency and
coincident boundary segments, native failure on invalid contours, infinite native
domain clipped to a finite range, spline/source mutation, indexes/capacity, wrong
tokens and concurrent disposal. Shared public-only workflow lifts copied curves,
builds AP topology, checks face inclusion and reads actual STEP/IGES files back
from Runtime and both clean consumers.

Full Release/Debug builds/suites, actual Debug-native identities, clean regeneration,
stable-ID and module accounting, ABI/API/exports, final inventory, 14 packages,
both clean consumers and unified local release-check remain required.
Hosted CI, signing, push, public NuGet and whole downstream application acceptance:
NOT RUN.

## Local delivery evidence

Release, Debug and cold-source Release each pass 247 Generator / 954 Runtime,
with zero failures/skips; isolated actual Debug-native passes 954/954 and observes
62 loaded DLL paths/hashes. All 210 generated files reproduce, including 207
unchanged baseline files. Exact migration delta: 27. Exports: 41,280
(+6/-0), Debug parity; managed API 56,558 (+102/-0); 4,229 current
facade forwarders. Both configurations pass 55 C11 headers, 34 layouts,
250 release races / 16,000 acquisitions; all 45 private headers compile.
Fourteen packages and both clean consumers exercise copied trims, AP topology,
face inclusion, STEP/IGES readback and the native done-flag/error-status case.
Final source inputs, payloads, packaged documents, provenance, SBOM and checksums
are checked before freezing PASS. The same 32 parser failures remain.

# OcctSharp 8.0.1-preview.20 — Batch U

Local validation target for OCCT 8.0.1, .NET 10 and Windows x64. Not published to
NuGet.org. Whole-batch validation/commit status is authoritative in [STATUS](STATUS.md).

## Advanced local features

Source-bound immutable contour recipes support independent constant, sampled and
copied-law radius programs, compatible vertex constraints, representation/continuity
policies, copied simulated circle sections, patch history and detailed faults.
The pinned OCCT law setter can lose its law; the bridge instead uses bounded sample
interpolation with measured probe error. It is not an exact arbitrary-function transfer
or rigorous global error bound. Closed seams and conflicting anchors reject.
Real HasResult-gated partial topology is diagnostic-only and cannot become an accepted root.

Chamfer programs include distance-angle, throat and throat-penetration modes with
their actual distinct dimensions. Face draft supports individual programs, checked
tangent propagation and problem-face diagnosis. Shell draft supports length, underlying
surface and shape stops. BRepFeat prisms, drafted prisms, revolutions, pipes, ribs/slots
and cylindrical holes expose explicit support/sliding/limit contracts.

In the pinned SDK, limit-driven shell drafts require a single analytic line/circle
boundary; cornered/multi-edge limit profiles reject before an internal Debug assertion.
Length-only cornered drafts remain available. Circle surface limits and circle/line
shape limits have numeric success evidence. An open line to an unbounded surface can
produce completed but invalid topology and is rejected by RequireShape. Shell history
queries only supported source edges and distinguishes pre-limit sweeps from final laterals.

MakeDPrism UntilEnd can have insufficient axial reach because its SDK extent is a
box-derived slanted length. Limited pipes with untrimmed line curves can fail native
BSpline conversion; bounded Bezier spines have success evidence. Planar limits may use
unbounded supports; revolved limits use native base-side selection, not necessarily
the first positive-angle intersection. Failures are never retried as unrelated Booleans.
Rotational rib clipping is an explicitly composed material-only stage; original exact
history is not misrepresented as final composed history.

## Integration and ownership

Q acceptance verifies budgets, source/result fingerprints and exact protected topology.
T persisted fillet/chamfer/draft/limited recipes execute through the facade; changed
geometry requires explicit selector rebind. Four storage formats reopen and rerun the
kernels; failures preserve explicitly stale last-good results. Exact XDE shared-definition
publication preserves placements and rejects ambiguous metadata or changed context.
STEP/IGES carries supported geometry/name/color, not executable recipes or history.
Viewer review uses parent/thread-bound IDs and sampled traces of copied circle sections.

ABI 1.64 / bridge 0.72.0 add eleven C calls. Existing modules, one Native DLL and
the shared 62-DLL runtime remain; assembly/file 0.1.0.0 and schema 1.13 are unchanged.
SC-058 lists 108 exact newly invoked blocked overloads. Unsafe SDK law/history getters
are not broadly promoted. Generated files are regenerated through the normal pipeline.

## Validation and licensing

The [original forty-row matrix](BATCH_U_ADVANCED_LOCAL_FEATURES_GAP_INVENTORY.md) has
named assertions. Focused 96/96 and ten repeats, Release/Debug Generator 91/91 and
Runtime 409/409, actual Debug-native 409/409, 42 strict headers, additive API/exports,
exact 108-ID accounting, both clean consumers, 94-file cold regeneration and complete
local release-check pass. Final documentation-package delivery is recorded in STATUS.
This is local validation, not public release readiness. No NuGet upload or GitHub push.

OcctSharp code is MIT. OCCT and bundled native dependencies keep separate licenses;
read the [third-party notices](../OcctSharp/runtime/win-x64/THIRD_PARTY_NOTICES.md)
and bundled license texts before redistribution.

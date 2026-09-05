# ADR-0081: Extract all historical native implementation responsibilities

- Status: Accepted and implemented
- Date: 2026-09-05

## Context

After Batch P, the historical native implementation still occupies 13,510 lines and
contains 511 C ABI definitions. The user explicitly requests completing this source
organization before another API wave. This is an architecture workstream, not Batch Q
and not a new product-capability denominator.

## Decision

Extract the entire legacy implementation into independently compiled, explicitly listed
translation units for Runtime, Foundation collections, Geometry, Modeling, Mesh,
Documents, Xde, Exchange, Visualization and the existing Surfaces workflows. Divide large
families by cohesive responsibilities such as topology inspection versus construction,
XDE structure versus presentation/PMI, and viewer selection versus manipulators.

Keep one native DLL and the current managed modules/facade. Retain the public C header,
export names, signatures, layouts, ABI 1.59, bridge 0.67.0, Preview.15 and schema 1.13.
Do not change algorithm bodies, public ownership, generated sources or manual stable-ID
classification. No new API or manual binding exception is introduced.

Private handle definitions and helper declarations have domain-owned headers. Shared
helpers use a private named namespace and explicit includes; they are not exported.
The manual registry mutex/live sets and thread-local error storage each have exactly
one definition. Template guards and registry access reuse that storage. Domain headers
must not become an all-OCCT include umbrella. Do not include implementation `.cpp` files,
enable unity builds, duplicate registries, or hide the monolith in `.inc` files.

## Preparation and validation

The source-move baseline is commit `5620ae5`. Its LF-normalized legacy source SHA256 is
`B22F73FFD21546F35483708D39F16FB18E9E86EC38E86627318D929C2D132195`.
Before replacement, record source blocks, exported symbols, public header and generated
file hashes, managed API and runtime identities. Mechanical extraction must account for
every declaration and preserve exported function bodies.

Require independent Release/Debug native compilation; existing Generator and Runtime
regression, including an actual Debug-native run and real HWND/XDE/STEP/IGES workflows;
same-baseline native export and managed API comparisons with zero additions/removals;
generated freshness and clean regeneration; committed runtime manifest refresh; fourteen
local packages and both clean consumers; release metadata; source-layout checks; docs
and whitespace checks. Implementation checks are NOT RUN at decision acceptance.

Correct stale Batch P preparation/loop-state text in STATUS while documenting the actual
architecture progress. Complete this workstream in one local commit. No NuGet publication
or GitHub push. Cross-DLL lifetime protocols and further API expansion remain separate.

Related: ADR-0002, ADR-0009, ADR-0074, ADR-0080; OWNERSHIP and NATIVE_ABI.

## Completion evidence

The 39 extracted implementations and 33 new private headers replace the entire legacy
source. With Batch P, 42 manual units compile independently without PCH/unity and all
34 private headers pass standalone syntax checks. The 693 historical function bodies
and 511 complete historical C entry-point definitions remain unchanged. Shared manual
storage has 22 unique definitions. Six invalid-layout fixtures are rejected.

Release/Debug Generator 91/91 and Runtime 180/180 pass, including three new boundary
tests. The final actual Debug-native isolated sweep also passes 180/180. Both native
configurations preserve all 29,402 exports; the same-baseline managed API has zero
additions/removals. Fresh-source native/managed builds and final header inclusion pass;
all 94 generated files remain identical. The complete inventory hash is unchanged.
The complete local release check passes, with fourteen packages and both consumers.
Final artifact hashes are maintained in STATUS. No new product wave, physical DLL
split, NuGet publication, signing or GitHub push is included.

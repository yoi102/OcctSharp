# ADR-0109: generated copied curved hatching workspaces

Status: Accepted. Date: 2026-10-07.

## Context

AQ extends existing straight-line Hatch_Hatcher with Geom2dHatch curved boundaries,
multiple hatch curves, copied events/domains and independent trimmed geometry.
The SDK disables missing-index guards in Hatcher, and a domain can omit either
endpoint. TrimDone, TrimFailed, domain IsDone and error status are distinct gates.
The header's no-argument IsDone has no pinned implementation/import symbol and
fails native linkage. It remains deferred; aggregate verified per-hatching IsDone
results together with TrimDone, !TrimFailed and NoProblem, treating an empty
hatching list as complete. Native per-hatching IsDone can be true with an error
status; preserve that diagnostic flag without claiming snapshot success.

## Decision

TM022 is an exact private Geometry workflow catalogue, emitted by the existing
closed-workflow emitter. Global InitialTypeMap remains unchanged. Hatcher and
intersector stay call-private; their constructor state and linked implementation
are audited at the pinned source commit. Copy each owning input curve before
creating finite-range adaptors; retain all managed SafeHandles across the call.
Forward/reversed boundary orientation is explicit and never inferred from shape.
Source mutation requires caller synchronization while copying.

An immutable typed leased token retains copied input geometry and scalar event/
domain snapshots. Bound every zero-based public index and native one-based access.
Copy all intersections with their element index, parameter, position, transition,
segment flags and intersection type. Failed domain computation preserves status
and permits no domain access; empty successful output remains separate from failure.
Represent absent native endpoints as null, never zero. Preserve point-only and
coincident-segment events through explicit KeepPoints/KeepSegments options.

Finite requested ranges clip native domains for independent trimmed-curve queries;
retain original optional native endpoints in the snapshot. Empty/point-only clips
have no trimmed curve. Each query deep-copies before Geom2d_TrimmedCurve/GeomAPI::To3d.
Geometry returns owning 2D/3D curves; existing AP Modeling builds topology, avoiding
a Geometry→Modeling dependency. No borrowed intersector/adaptor/native container
escapes. Exceptions publish no owner or partially written array.
Copied region states use a Geometry-scoped HatchingRegionState projection;
the existing Modeling-owned TopAbsState identity stays unchanged.

## Consequences

No new native DLL, raw mutable receiver or callback. Resource limits bound input,
intersection and domain expansion. Necessary existing emitted/manual prerequisites
are reused separately from new Blocked→Emitted IDs. Actual analytic reference,
tangent/coincident/failure, spline, ownership, real topology/exchange and both clean
consumers are required alongside complete local delivery checks.

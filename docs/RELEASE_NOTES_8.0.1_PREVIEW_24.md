# 8.0.1-preview.24: Broad generated geometric values

Batch Y implements [ADR-0093](adr/0093-generated-geometric-value-projections.md).
It adds 968 generated declaration IDs, from 19,682 to 20,650, across eight modules.
The managed public API adds 1,085 signatures and removes none.
Thirty immutable records in OcctSharp.Values cover coordinates, vectors, directions,
axes, matrices, quaternions, lines, conics, planes and elementary surfaces. Constructors
and getters can now exchange these values without native object-layout marshaling.

Examples include Geom/Geom2d analytic curve construction, static curve derivatives,
elementary surface round trips, camera directions, IGES values and mesh/exchange
coordinate systems. Copied results survive source mutation or disposal. Full coordinate
systems preserve handedness; malformed frames and nonfinite geometric inputs reject.
Existing public type identities and generated overload signatures remain compatible.

Mutable/output references, bulk arrays and general transformation internals are later
work. One exact reverse-module contract remains Blocked; two exact missing-artifact
signatures are excluded in SC-062. The initial gp_Pnt(gp_XYZ) constructor emission path
also remains Blocked. No manual ID is added. Declaration coverage is not
functional completeness; full OCCT migration remains incomplete.

Package Preview.24 / ABI 1.68 / bridge 0.76.0 preserve twelve modules, the facade,
one Native DLL, fourteen packages, configuration 1.13, binding model 1.3 and assembly
identity 0.1.0.0. [STATUS](STATUS.md) records actual validation and delivery state.
This is a local preview; GitHub push and NuGet publication are outside this task.

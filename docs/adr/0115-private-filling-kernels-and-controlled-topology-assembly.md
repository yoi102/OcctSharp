# ADR-0115: Private filling kernels and controlled topology assembly

Status: Accepted (2026-10-08).

TM030 extends exact closed workflows to bounded copied spline derivative data,
initialized section generators and completion-gated filling surfaces. Call-private
curve copies and explicit section parameters prevent mutable caller state and
uninitialized SDK fields from leaking into approximation. Outputs retain owned
private geometry; each surface query returns another independent copy.

TM031 permits only initialized new vertex/edge assembly. Endpoint values and edge
parameters derive from the private curve copy; BRepCheck_Analyzer gates publication.
Representation flags are proved only for new 3D-only edges. Existing input edges,
general pcurve/continuity assertions and topology-history inference stay excluded.

Both reuse the one bridge, registry leases, exception guards and success-only
publication. Existing model/repair/exchange modules compose the real workflow;
no new manual binding or cross-DLL ownership category is introduced.

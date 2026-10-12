# ADR-0126: finite and coupled copied intersection domains

Status: Accepted. Date: 2026-10-10.

Hit faces and 3D/pcurve intersection outputs must retain their exact computation
domain without exposing mutable algorithm receivers or borrowed SDK arrays.

Use two exact generated profiles: finite shape hits in Modeling and coupled surface
intersection/pcurve diagnostics in Geometry. Each call owns copied input geometry,
initializes all required solver state and publishes only completed outputs. Native
face ordinals belong to a private shape snapshot. A surface result retains the same
native line's 3D curve and optional two pcurves, preserving absence and periodic branches.
Queries return independent deep copies, and partial managed tuple construction rolls back.

The shape module may extract copied world-coordinate surfaces; Geometry does not
depend on Modeling. Existing typed registry/SafeHandle ownership remains unchanged.
Input/copy/output budgets constrain supported profiles but do not promise strict SDK
execution timeouts. Header declarations without linked definitions remain deferred. SDK tolerances and sampled residuals are separately disclosed.

See [BH contract](../BATCH_BH_INTERSECTION_RESULTS.md).

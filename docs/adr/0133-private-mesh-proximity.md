# ADR-0133: private mesh proximity result domains

Status: Accepted within the authorized BL–BQ scope.

Use a Mesh/TM051 generated closed workflow with independent input geometry/triangulation, exact native face indices and copied mode-specific diagnostics. Keep native solver, triangle set, map and BVH access private. Completion guards every layer-thickness reference/status copy. Avoid ShapeProximity's value-mode unconditional status copy by loading audited finite overlap triangle sets into a private ProximityValueTool.

Results state approximate mesh provenance explicitly. Stored deflection is not a certified error bound, and layer thickness is not exact BRep shortest distance. Bounded input geometry and conservative refinement preflight constrain supported work without claiming cancellation or a timeout. CopyFace returns independent data; release invalidates future queries while copied metadata remains valid.

See [BO contract](../BATCH_BO_MESH_PROXIMITY.md).

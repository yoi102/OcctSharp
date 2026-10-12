# 8.0.1-preview.76: CA mesh adapters

Candidate, delivery incomplete until batch-ca/delivery-result.json PASS.
ABI 1.113 is retained; bridge 0.125.0 identifies this implementation.

TM060 migrates 24 existing Manual signatures into a native-private generated
mesh profile and reuses 6 already-Emitted supports. New SDK declarations: 0.
Triangles copy indices before owner closure; mutation pointers stay call-private.
Existing public APIs, budgets, channel/index/transform and publication policies
are retained. Generic shared-handle emission excludes the private primary rows.

The public module/facade workflow covers controlled independent meshing/remesh,
quality/adjacency, optional channels, coherent patch/weld, foreign/stale rejection,
source geometry/mesh/document preservation, XBF/STEP/BREP real readback and actual
HWND/DXGI pick/select/invalidation. Exact geometry and discrete mesh remain distinct.

Hosted CI, signing, push, NuGet publication, external CAD and full downstream
application/device acceptance: NOT RUN. Current evidence: [STATUS](STATUS.md).

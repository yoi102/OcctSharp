# ADR-0113: Generated parent-bound selection workspaces

Status: Accepted for implementation. Date: 2026-10-08.

## Context

AU follows delivered Preview.44 and closes the authorized AP–AU round. Existing
ViewerInputController, presentation/source identities, selection items and copied
frames remain reused. They do not provide a copied SDK selecting volume or exact
world-space scene hit. Input validation currently has state-before-thread paths,
and selection mutations do not explicitly redraw the live view.

## Decision

TM028 owns an initialized private point/box SelectMgr volume. Capture a copied
camera and client size on the Viewer creating thread; initialize volume before
setters/build. Copy exactly eight vertices only for the proved rectangular SDK
type; GetVertices contains an unsafe static cast for other volume types, which
remain excluded. Copy planes and point-only near/far/ray/mouse data. Bounded
world-space point/box/sphere/segment/triangle/polygon probes expose optional SDK
depth/point validity. Never read uninitialized pick-result normals. These are
geometric volumes without rendering clip/filter semantics; live scene picking
uses AIS and reports exact presentation/source identity.

Workspaces strongly reference the managed Viewer and reject wrong-thread,
disposed parent, cross-parent and stale queries. A conservative captured source
stamp compares camera handle/revisions, client dimensions, presentation IDs,
shape identity/location, transforms, display state, active modes, pixel tolerance
and filter identity. No automatic rebasing or geometric topology inference.
External in-place mutation of shared source geometry remains caller synchronized
and outside this stamp proof. Copied metadata survives parent/workspace disposal.

Use a call-private initialized AIS_ViewController for world point/axis hits and
gravity; copy diagnostics and background colors from the existing V3d view.
No callbacks, animation actors, touch/input buffers, arbitrary cameras/windows
or general SDK containers are exposed. Existing managed input forwarding owns
gesture state and checks the parent/thread before changing it. Explicit opt-in
rectangle gestures use the existing selection route. Successful point/rectangle/
polygon/clear selection refreshes the live view immediately.

## Validation

Require analytical volume bounds/probes, optional hit fields, source-stamp changes,
removed/cross-parent/disposed/thread rejection, actual HWND inputs and visible
screen pixels before/after selection. Copied ToPixMap frames alone do not establish
live repaint. Pure copied values and generated native core stay in Visualization; friendly
Viewer/presentation/source-identity adapters remain in Facade, per ADR-0090.
No Visualization-to-Facade reference is permitted. The direct module consumer
checks AS native-document metadata and AU raw C ABI against the composed model;
Facade owns save/reopen occurrence identity and actual HWND input/frame checks.

Chain AP modeling, AQ hatching, AR meshing, AS document metadata,
AT exchange and Viewer source identity in both consumers. Full local delivery
gates remain required. Physical display/input latency, external CAD, hosted CI,
signing, push/publication and full downstream app acceptance are separate NOT RUN.

# Batch AU: parent-bound selection workspaces and round integration

Status: IMPLEMENTED / LOCAL GATES PASS; delivered only after final freeze PASS. Baseline: delivered Preview.44, 2026-10-08.

AP–AT delivered (5/6); AU is the final authorized batch.
[Candidates](BATCH_AU_SELECTION_WORKSPACES_CANDIDATES.json): 213 primary Blocked.
[Admission](BATCH_AU_SELECTION_WORKSPACES_ADMISSION.json): 39 admitted, 174 deferred,
2 new prerequisites, 19 reused; generated delta 41, Manual delta 0.
[ADR-0113](adr/0113-generated-parent-bound-selection-workspaces.md) governs TM028.

Copy point/box selecting volumes and bounded primitive diagnostics; provide
exact scene world-point/axis picks, copied view diagnostics and source stamps.
Retain strict Viewer thread/lifetime semantics, opt-in rectangle gestures and
live selection repaint. Integrate all six AP–AU workflows in clean consumers.
Unrestricted custom volumes, actor/animation/touch buffers, arbitrary cameras,
stereo and unproved transformations/clipping stay deferred. Geometric volume
queries are distinct from actual AIS scene picking.

Generated/native Release and managed compilation pass. Four exact-contract
Generator tests and nine runtime boundary/identity tests pass. The source audit
proves 41 new generated StableIds and 226 byte-identical old generated files.
The six-batch model chain has volume 72; AS XCAF save/reopen preserves occurrence
NamedData, and actual HWND click/rectangle inputs select the exact occurrence.
Copied selection identity survives later presentation identity changes/removal;
client resizing invalidates volumes. These results are independent of screen proof.

Visible desktop-frame acceptance now PASS in the current desktop state: actual
HWND click and rectangle inputs change the captured model frame. Initial Release
plus three repeats pass; complete Release/Debug/cold/actual-Debug suites and both
consumers pass. Final packaged documents/payload/source identity freeze remains
the delivery authority. Historical DXGI/WGC failures and diagnostic controls are
retained; their cause is unresolved. No visual assertion was skipped or relaxed.

The generated core/copied values belong to Visualization. The manual friendly
Facade adapter contains no SDK/native binding calls and introduces no cycle.
The Facade consumer covers the complete XCAF occurrence/input/frame route.
The direct consumer composes AQ/AP/AR/AT geometry, AS native-document metadata
and an explicit AU raw C ABI probe, with no OcctSharp.dll dependency. Its path
does not claim Facade save/reopen occurrence coverage.

World-point/axis diagnostics preserve AIS_ViewController selector semantics.
They do not promise the context post-selection filter policy; selection APIs
remain authoritative for filtered selection. Geometric volumes use the copied
camera/client frustum and ignore rendering clipping/filter interpretation.

Physical display FPS/input latency, external CAD/AS extension interoperability,
hosted CI, signing, push, public publication and whole downstream app acceptance:
NOT RUN.

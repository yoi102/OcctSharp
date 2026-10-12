# CF: exact Viewer adapters and real WPF input

State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.
Baseline CE frozen Preview.80 / ABI1.115 / bridge0.129.0 PASS.
Candidate Preview.81 / ABI1.115 / bridge0.130.0; native ABI unchanged.
See [exact admission](BATCH_CF_VIEWER_ADMISSION.json).

TM066 migrates36 existing Manual Viewer declarations, TM067 migrates4 Foundation
identity declarations. No new SDK discovery or public raw SDK owner is added.
The25-unit read-only libclang audit found63 exact calls:59 in11 native policy
callers and4 in the existing selection-workspaces template. Generation emits two
private headers;331 prior generated files retain330 unchanged files and one
precisely admitted template conversion. Allocation/handle transfer and borrowed
return lifetimes stay in their original scope. Factories preserve original SDK
types: Standard_GUID by value, Graphic3d_ClipPlane as the original new allocation
immediately transferred into viewer-owned handle storage. No derived receiver.

The fixed SDK commit is b8f597c677811d1f9f4d8a97f5ae2825c0353a42;18 header/implementation
inputs and installed header identity are checked. Nested method-scope types such
as AIS_Manipulator::OptionsForAttach use the model canonical owner qualification
when emitted at namespace scope. This fixes code emission, not SDK signatures.

Camera matrices/eye/up and view-camera returns borrow initialized native camera
storage only until immediate caller copy. Existing source-camera revisions and
selection owner/domain stamps remain authoritative. Selected/detected shapes are
published through existing independent Shape handles; exact presentation identity,
source membership, bounded selection and context/thread validation remain intact.
Custom shape aspects remain AIS-parent-bound. Manipulator attach/transform calls
retain explicit viewer/presentation owner and active-transform guards.
GUID format validates before constructor; ToCString writes the existing37-byte
zeroed buffer. DynamicType borrows from a live leased document attribute until
the type name is copied. No pointer or attribute owner escapes into managed code.

Actual Win32 capture transfer reproduced the WPF bug: after WM_CAPTURECHANGED,
the controller retained its pressed state and a later release selected a shape.
CancelPointerGesture now cancels click/rectangle state without changing selection
or camera, with thread/disposed-owner checks before mutation. Stale button flags
cannot pan/rotate without a matching active gesture. Production HwndHost routes
WM_CAPTURECHANGED, WM_CANCELMODE and Escape into this policy. Escape also clears
selection and releases capture. These are manual UI policies, not SDK migration IDs.

The dedicated WPF validation executable compiles the production HwndHost source,
creates two visible native children, sends real Win32 messages, transfers actual
capture to a different HWND, and reads model-region desktop pixels with a persistent
DXGI session. It checks click/rectangle cancellation, recovery with a fresh gesture,
Escape/CancelMode, exact presentation identity, wheel invalidation of a selection
workspace, pan/rotation, clipping/removal rejection and unaffected second viewport.
Offscreen snapshots do not replace desktop pixels. Earlier blank first-frame
captures and the original failed reproduction remain in batch-cf evidence.

Release/Debug/cold full suites, actual Debug-native, ABI/lifetime/export/API,
generated freshness and14 packages/two consumers passed. Final frozen authority
is batch-cf/delivery-result.json.
Hosted CI, full downstream applications, external CAD, mixed-DPI/RDP, physical GPU
removal, signing, push and public NuGet: NOT RUN. CF does not close CG recovery.

Local evidence:

40 admitted / 0 deferred, 0 new prerequisites / 0 reused; generated +40 / Manual -40 (new SDK declarations 0, Manual to Emitted 40).
Release/Debug/cold Generator 355, Runtime 1435: PASS.
Actual Debug-native 1435, 62 actually observed DLL paths/hashes: PASS.
Exports 41467 (+0/-0), Debug parity; API 59004 (+1/-0).
All 330 baseline generated files unchanged; current generated files 333.
Explicitly audited modified generated files: 1.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-cf/delivery-result.json PASS completes CF.

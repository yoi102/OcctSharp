# Batch BG: copied modeling results and exact source diagnostics

State: FROZEN LOCAL DELIVERY PASS at 8.0.1-preview.57; authority: OcctSharp/artifacts/batch-bg/delivery-result.json.
Delivered baseline: Preview.56 / ABI 1.99 / bridge 0.107.0 (BF frozen PASS).
Development target: Preview.57 / ABI 1.100 / bridge 0.108.0.
[Admission](BATCH_BG_MODELING_RESULTS_ADMISSION.json): 19 admitted, 16 deferred,
8 private prerequisites and 24 exact reused operations; no Manual delta.

## Contract

The generated TM042 adapter adds configuration snapshots and source diagnostics
for existing section, planar offset and wire loft operations. It does not count
the existing basic operations as new bindings. Inputs are copied as one graph
before execution; the private loft disables mutable input. Default offset builders
are configured with Init before AddWire/Perform, or initialized from a face.

Each result owns its copied inputs and successful topology. A new Guid identifies
the managed result domain. Input roots are zero-based; topology ordinals are one-based
in that private domain. A queried Shape is a new geometry-deep copy, so ordinals are
diagnostic correspondence rather than a general mutable topology identity service.
No original operand or detached Shape is automatically rebound.

Section ancestor queries in the fixed SDK return only the first pair. The adapter
therefore traverses the private DSFiller/PDS face interference curves and pave blocks,
retains all distinct source face pairs and checks the SDK query against exact source
membership. Missing/common-edge, unique, multiple and unproven results stay distinct.
Source face ordinals are published only for a proven unique pair. Multiple paths do
not pick a preferred source. Eight prerequisite declarations permit this private
traversal only; no borrowed data structure leaves the call.

Offset and loft Generated lists are copied into result/source ordinal records.
Multiple destinations or multiple sources remain Multiple, not automatic split/merge
rebinding. GeneratedFace additionally retains the SDK's ruled/smoothed domain rules:
missing entries for other profiles remain missing. Approximation can replace offset
profiles; absence of a matching original history is explicit.

## Failure, limits and lifetime

The result distinguishes incomplete SDK computation, valid completion, invalid topology
and caught SDK failure. Invalid/unfinished operations retain copied input/configuration
diagnostics but publish no result topology. Argument/budget failures throw before a
result is published. Snapshot arrays are fully staged and atomically committed.
All queried output geometry and detached diagnostic records survive workspace/source
disposal. Caller mutation of input objects must be synchronized while copying.

Limits: at most 64 input roots, 16,384 input/output topology entries, 4,096 visited edges
per shape, 65,536 source records and 262,144 section provenance visits. Lofts accept
wire sections, finite precision, positive weights, bounded degree and audited C0/C1/C2
continuity. Offset join supports Arc and Intersection. These bounds constrain copies
and diagnostics; the SDK computation has no strict timeout or cancellation guarantee.

## Acceptance

Retain exact admission/source/old-generated-byte checks, generator positive/negative
regressions, intersecting/separated/tangent sections, private ancestor membership,
valid/open/failed offsets, ruled/smoothed lofts, input isolation, missing history,
lazy budget+1 disposal, lifetime and real STEP/IGES round trips. Delivery requires
full Release/Debug/cold source, actual Debug-native, ABI/API/exports, lifetime,
inventory, 14 packages, two clean consumers and final source/payload freeze.
BF's existing actual HWND/pixel and identity/stale-query assertions remain in the
whole-chain suites. Only batch-bg/delivery-result.json PASS completes BG.

See [ADR-0125](adr/0125-copied-modeling-source-diagnostics.md).

Local evidence:

19 admitted / 16 deferred, 8 new prerequisites / 24 reused; generated +27 / Manual +0.
Release/Debug/cold Generator 292, Runtime 1195: PASS.
Actual Debug-native 1195, 62 actually observed DLL paths/hashes: PASS.
Exports 41408 (+8/-0), Debug parity; API 58134 (+119/-0).
All 272 baseline generated files unchanged; current generated files 275.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze PASS is recorded in batch-bg/delivery-result.json.

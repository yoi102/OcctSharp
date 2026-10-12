# BA assembly diagnostics

State: IMPLEMENTED / LOCAL GATES PASS; final freeze remains delivery authority. Baseline Preview.50 / AZ is frozen PASS.
Exact admission: 22 primary admitted, 68 deferred, one new prerequisite, 24 reused.
Generated delta 23, Manual delta zero. See [admission](BATCH_BA_ASSEMBLY_DIAGNOSTICS_ADMISSION.json),
[candidates](BATCH_BA_ASSEMBLY_DIAGNOSTICS_CANDIDATES.json) and
[ADR-0119](adr/0119-initialized-assembly-and-copied-validation.md).

TM036 initializes private edge builders from independent curves or pcurves and
support surfaces. Finite in-domain parameters and optional explicit endpoints
retain SDK construction errors. Bounded SDK 3D-curve approximation follows
pcurve construction. Complete results pass validity checks; no partial shape is
available on failure. Repeated shape queries return independent copies.

Mixed edges/wires are copied as a graph, preserving source sharing. Sequential
BRepBuilderAPI/BRepLib addition stops at first disconnection and records its
zero-based input index. Empty inputs retain EmptyWire diagnostics. Borrowed
builder references, conversions, copy/move receivers and unordered partial-list
assembly remain deferred.
Stage identifies the SDK edge/wire/face error family even during UV construction.
Input graph traversal and total wire edges are bounded before deep copying or
builder execution, including repeated input wires.

WireOnSurface builds piecewise linear UV pcurves on one exact private surface
through wire/face creation. A face requires at least three segments, repeated
endpoints, closed topology and valid representations. No topology history,
welding provenance or trimming-domain inference is claimed. Source mutation
during capture remains caller synchronized.

Validation initializes exact/parallel flags and copies invalid-member counts.
The exact flag has the SDK SameParameter limitation. Wire-area queries select a
native zero-based wire in the copied face; preserve status bits/noncompletion.
Contextual SetSurface is deferred because it creates a new face and cannot prove
existing pcurve membership in the original face.

Tests cover reversed/disconnected/degenerate inputs, release independence, bounded
lazy UV enumeration, concurrency, repair and actual STEP/IGES readback. Full
Release/Debug/cold, actual Debug-native, ABI/API, consumers and source/package
freeze remain required. See actual local evidence below.

Local evidence:

22 admitted / 68 deferred, 1 new prerequisites / 24 reused; generated +23 / Manual +0.
Release/Debug/cold Generator 278, Runtime 1121, all passed.
Actual Debug-native 1121, 62 observed DLL paths/hashes. Exports
41371 (+9/-0), Debug parity; API 57556
(+52/-0); 4315 facade forwarders. All 252 baseline
generated files unchanged; current files 255. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; BA counts complete only when delivery-result.json is PASS.

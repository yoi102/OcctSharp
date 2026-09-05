# OcctSharp 8.0.1-preview.19 — Batch T

Local validation checkpoint for OCCT 8.0.1, .NET 10 and Windows x64. This package is
not published to NuGet.org. Final validation/commit evidence is recorded in STATUS.

## Parametric documents

Batch T retains all forty original capabilities. Documents now provides copied typed
feature definitions, named values, units, bounded expressions and deterministic DAG
plans. The facade executes primitive, rigid placement, extrusion/revolution, Boolean,
Q repair, S guided sweep/constrained fill and R mesh recipes against persisted inputs.

Incremental, full and targeted recompute compute temporary results before publishing
one atomic command. Failure and cancellation between synchronous calls preserve
last-good geometry but mark it stale. EditAndRecompute rolls back the edit too on
failure; successful edits/results undo and redo together. Reopen recovers interrupted
Executing states and dirties their dependants.

Persistent selections use dedicated TNaming children and exact source transform
correspondence. Missing, ambiguous, deleted, unsupported and wrong-type outcomes are
explicit; arbitrary topology changes do not guarantee subshape naming. Owning history
snapshots are selected by durable result-generation GUIDs, not the native transaction
nesting counter. Boolean/sweep history retains actual input-feature associations and
does not claim unavailable exact source-subshape relations.

Closed-subgraph duplication uses TDF relocation and rewrites GUIDs, native function
IDs, expressions, references, selectors and history metadata paths. External dependencies
require explicit retain/reject policy; deletion supports reject-dependants or cascade.
Four Bin/XML OCAF/XDE formats preserve executable recipes and support actual recompute
after reopening. Saving without an extension chooses the format's proper extension.

## Cross-family delivery and boundaries

Publishing a recomputed XDE definition updates repeated occurrences once while keeping
placements. Unmapped subshape metadata is a conflict, not silently discarded. STEP/IGES
delivery carries supported exact geometry, names and colors, not the parametric graph
or guarantees for mesh-only results. Viewer review replaces parent-bound IDs after
recompute/undo and highlights explicitly stale failure inputs.

ABI 1.63 / bridge 0.71.0 add sixteen C functions. Package core remains aligned to OCCT;
assembly/file identity 0.1.0.0 and generator schema 1.13 do not change. Existing twelve
modules, facade, single native DLL and shared 62-DLL package remain. SC-057 records
65 exact new Blocked-to-Manual overloads; no unrelated inventory classifications change.

## Validation and licensing

Focused tests, repeated lifetime runs, Release/Debug and actual Debug-native regression,
strict source/header/dependency checks, exact API/export compatibility, both clean
consumers, cold regeneration and full local release-check are required together.
They pass: focused 40/40 and ten repeats, Generator 91/91, Runtime 313/313 in both
configurations and actual Debug-native, both clean consumers, 94 byte-identical
generated files, 40 strict private headers and exact additive compatibility/accounting.
See [STATUS](STATUS.md) for executed results and [the forty-row matrix](BATCH_T_PARAMETRIC_DOCUMENT_RECOMPUTE_GAP_INVENTORY.md)
for capability evidence. Hosted CI, signing, publication and GitHub push are NOT RUN.

OcctSharp code is MIT; bundled OCCT and its native dependencies retain their separate
licenses. Read [third-party notices](../OcctSharp/runtime/win-x64/THIRD_PARTY_NOTICES.md)
and the packaged license texts before redistributing the runtime.

# Boolean topology history for Cadoryx H2-B2

Development extension on the Preview.28 source baseline; not a public release.
BooleanHistoryModeling.Build accepts ordered RepairSnapshot inputs and an argument
group count, and returns owning LocalFeatureResult evidence. Source.ArgumentIndex
and TopologyIndex identify an input slot and its full-map topology. ResultTopologyIndex
identifies membership in the final result graph. PlanId scopes all relations to
this invocation. The supplied snapshots remain caller-owned and unchanged.

The implementation uses one private graph copy, then one non-destructive serial
Fuse/Cut/Common build. Modified/Generated/Deleted come from that builder; Unchanged
uses exact TShape/location membership. Split and shared-result relations are kept.
No geometry-proximity mapping or persistent naming is implied. Generated/modified
items absent from the final graph have no final index and consumers must fail closed.
No same-domain simplification or multi-step propagation is enabled.

New ABI export: occtsharp_boolean_topology_history. Existing fixed structs and
enum values remain unchanged; LocalFeatureOperation.Boolean is appended as 11.
The generated binding set is untouched. The manual composition is recorded in
SPECIAL_CASES; the existing ownership category is recorded in OWNERSHIP.

The actual Repair::Copy ModifiedShape correspondence also verifies every final
full-map slot and orientation before history is published. A failed correspondence
keeps valid Boolean geometry but exposes no evolution group. The native tests
select history targets in the exact returned graph; independent deep copies can
change p-curve representation and are not compared as raw serialized bytes.

Executed development validation: Release/Debug native and affected managed builds;
BooleanTopologyHistoryTests 6/6 against each actual native configuration, including
multiple argument slots and a separate tool slot; all 62 DLLs
in each test output match the corresponding build hashes; C11 fixed layouts and
function signature; 40,570 identical Release/Debug exports, exactly one addition and
no removals from the 40,569-export Preview.28 baseline. No generated file is edited.

For source-tree tests, pass `-p:OcctSharpUseBundledNativeRuntime=false` to
`dotnet test`. The default bundled Preview.28 runtime predates this companion;
running the new managed tests against it produces `EntryPointNotFoundException`.
The Release and Debug reruns with the local bridge passed 6/6 on 2026-09-23,
and the loaded test-output bridge matched the corresponding build hash.

Local development package version: 8.0.1-preview.28.cadoryx.h2b2.2. This is an
additive experimental companion, not a new full-library migration or public release.
Cadoryx owns the downstream NuGet, BRep reload, transaction and desktop acceptance
records in its docs/BOOLEAN_HISTORY.md and docs/ROADMAP.md. Package-generation and
consumer completion are recorded in this repository's STATUS, which is excluded
from NuGet documentation to avoid representing later execution as packaged evidence.

Full upstream Generator/Runtime suites, clean regeneration, complete release-check,
hosted CI, signing and public publication: NOT RUN for this extension. The historical
Batch AC full delivery remains evidence for its original Preview.28 source only.

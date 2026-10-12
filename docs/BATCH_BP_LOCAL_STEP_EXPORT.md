# Batch BP: selected-root local STEP assembly export

State: IMPLEMENTED / LOCAL GATES PASS; frozen delivery authority: batch-bp/delivery-result.json.

The Xde/TM052 generated workflow exports exact committed definition/assembly labels from one document context. It builds one shared clone map across every selected root, copies geometry without retained source triangulations, preserves supported names/colors/layers and rebuilds assemblies. Foreign labels, duplicate/overlapping roots, open commands, null/cyclic/oversized graphs and geometry outside the explicit surface/3D curve budgets are rejected. Occurrence components are not standalone selected roots; select their definition or assembly. Located assembly definitions are rejected; identity assembly definitions with located components preserve their exact native placements.

Native STEPCAF writers remain private. Pinned SDK b8f597c677811d1f9f4d8a97f5ae2825c0353a42 originally derives leaf filenames from model names and fails to copy the complete main-model parameters to external writers. The generator embeds a hash-pinned private copy, preserving OCCT copyright/license, with a deterministic transformation: private class/linkage, canonical part-N.stp filenames and full external private model/shape-processing settings before geometry transfer. Model Unicode names remain separate. All original static-setting convenience overloads stay private and are not invoked by this workflow. No process global setters are used.

Only AP203/AP214 millimeter output and explicit private precision 1e-9..0.1 are admitted. Declared source document units are copied into the private document before transfer; absent units use the explicit profile millimeter convention. SourceLengthUnitMeters and SourceLengthUnitWasDeclared report that decision, avoiding the SDK prepareUnit fallback to process globals. Names/colors/layers are format-dependent; no PBR, PMI or generic metadata fidelity is promised. At most 128 selected roots, 127 simple definitions, 4096 graph labels/occurrences, depth 16 and 100000 copied topology objects are supported. Finite linear-extrusion surfaces are audited through their non-null basis curves; the same curve nesting/degree/pole budget applies to those bases. Surface and 3D curve nesting, degree/poles and finite geometric values are checked before copy. Output is at most 128 flat files and 64 MiB. These budgets are not execution timeouts or certified geometry error bounds.

The complete canonical filename set and native configuration are inspected before SDK writes in a private empty stage. SDK transfer/write statuses, complete output set and hashes are checked. Managed code copies to an adjacent private stage and publishes by one directory rename. Existing targets are rejected; failures remove this operation's owned stages. Output directory parents must exist and reparse paths are rejected. Results expose exact filenames, sizes, SHA256 and source definition diagnostics; Validate/OpenForReadback detect modification, missing or extra outputs.

The private SDK writer remains one independent generated compilation unit to preserve its intertwined transfer/metadata helper state; the generator bounds its native resource to 6000 lines and header to 1000, forbids implementation inclusion and pins both original source hashes; the manual layout checker stays unchanged. It does not expose or migrate all copied internal methods. Exact admitted/prerequisite/reused StableIds remain in the [admission](BATCH_BP_LOCAL_STEP_EXPORT_ADMISSION.json).

Both consumers use real BL solids and BN Unicode names, same-name distinct parts, repeated occurrences and multiple roots sharing a definition. They run the new multi-file export and BJ reader on those new files, verifying exact occurrences/definitions, placements, Unicode names, supported colors/layers and detached result lifetime. Tests additionally cover source/context mismatch, rejected/oversized selection, configurations, refused overwrite and changed output sets. Existing IGES workflows are reused, with no new multi-file IGES contract.

Only OcctSharp/artifacts/batch-bp/delivery-result.json PASS closes BP. Full local gates and source/package freeze are NOT RUN until recorded PASS. Hosted CI, external CAD interoperability, full downstream acceptance, signing, push and public publication: NOT RUN.

See [ADR-0134](adr/0134-private-canonical-step-export.md), [ownership](OWNERSHIP.md) and [continuous plan](NEXT_ROUND_BL_BQ_PLAN.md).

Native export delta: three bounded C API exports plus the MSVC-generated SDK default-constructor closure `??_FGeomToStep_MakeAxis2Placement3d@@QEAAXXZ`. The compiler helper is not a public managed capability and receives no additional StableId credit. All four exact names and Debug/Release parity are checked.

Local evidence:

9 admitted / 23 deferred, 6 new prerequisites / 34 reused; generated +15 / Manual +0.
Release/Debug/cold Generator 313, Runtime 1348: PASS.
Actual Debug-native 1348, 62 actually observed DLL paths/hashes: PASS.
Exports 41460 (+4/-0), Debug parity; API 58819 (+56/-0).
All 304 baseline generated files unchanged; current generated files 309.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bp/delivery-result.json PASS completes BP.

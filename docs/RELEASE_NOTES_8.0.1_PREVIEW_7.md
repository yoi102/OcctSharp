# OcctSharp 8.0.1-preview.7 release notes

Preview.7 completes Batch J as one 24-capability advanced feature-modeling, robust-
Boolean, copied-history, recovery, STEP/XDE, viewer, lifetime, and clean-package wave.
It remains an experimental Windows x64 package for .NET 10 and OCCT 8.0.1.

## Added

- Explicit constant and variable selected-edge fillets, symmetric and two-distance
  chamfers, planar fillet/chamfer editing, and selected-face draft.
- Prismatic boss and pocket, blind/through hole, additive/subtractive revolved feature,
  and additive/subtractive pipe feature workflows.
- Multi-argument/tool split, defeaturing, Boolean cell selection, and batch fuse/cut/
  common/section with fuzzy, parallel, non-destructive, and glue options.
- Boolean argument preflight, copied diagnostics, modified/generated/deleted history,
  input repair, same-domain result simplification, and bounded recovery.
- Focused real STEP/XDE, real HWND screenshot, input/result/history disposal, and clean
  62-DLL package-consumer evidence.

## Ownership and compatibility

All OCCT builders, maps, alerts, progress objects, contours, and history collections stay
inside native calls. Returned result and history topology are independent owning `Shape`
values; deletion and request association are copied indices. No native builder or borrowed
subshape crosses the ABI. Package identity is `8.0.1-preview.7`, native ABI is 1.52,
bridge implementation is 0.60.0, configuration schema is 1.10, and managed assembly/file
identity remains `0.1.0.0`.

SC-046 reconciles exactly 73 directly used blocked OCCT 8.0.1 stable IDs. The final
inventory remains complete at 116,272 declarations and 7,090 headers: 16,353 emitted,
500 manual, 49,344 skipped, 50,075 blocked, and zero supported-unselected/pending.
Comparison with the alpha.38 baseline is additive at 38,232 additions and zero removals.

## Local validation

- Release and Debug native/managed builds pass.
- Generator 91/91, Runtime 139/139, focused Batch J 4/4, and dependency profiles 6/6 pass.
- The clean package consumer restores, publishes, and runs with 62 DLLs, ABI 1.52,
  bridge 0.60.0, real STEP/XDE, copied history/recovery, and real HWND screenshots.
- All 83 generated files are fresh and byte-identical after clean-source regeneration.
- Full inventory, API compatibility, runtime hashes, SBOM, provenance, checksums, and
  Git whitespace checks pass locally.
- The final 40,907,579-byte `OcctSharp.8.0.1-preview.7.nupkg` has SHA256
  `AF77CA3E048277192DFB349F6C122F7CDD5909C06DAF910CD939C3BE2F95B3EC`, matching
  `artifacts/release/checksums.sha256`.

Hosted release execution, signing, NuGet publication, and GitHub work were not performed.

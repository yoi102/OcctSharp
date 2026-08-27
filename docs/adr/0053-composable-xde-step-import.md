# ADR-0053: Import STEP roots into an owned XDE document

- Status: Accepted
- Date: 2026-08-24
- Scope: composable STEPCAF/XDE workflows on OCCT 8.0.1

## Context

The interim `StepAssembly.WriteXde` API combined file import, creation of a fixed assembly
root, component placement, and STEP export in one operation. It preserved metadata, but
applications could not inspect imported labels, choose their own hierarchy, add metadata,
mix imported and in-memory parts, or delay export. B16 subsequently established an owned
`XdeDocument` and parent-bound `XdeLabel` lifetime model, so the one-shot boundary is no
longer the appropriate primary API.

## Decision

- Add `XdeDocument.ImportStep(string)` as a transaction-bound mutation. It clones every
  free STEPCAF/XDE shape root and supported metadata into the destination document and
  returns parent-bound labels for those newly imported roots.
- Keep STEPCAF readers, source documents, XCAF tools, label maps, and visual-material maps
  native-local. Returned labels contain stable entries and strong references to the
  destination `XdeDocument`; no cross-document native label escapes.
- Applications compose imported roots with `AddAssembly`, `AddComponent`,
  `TopLocLocation`, metadata APIs, BinXCAF persistence, and `WriteStep`.
- Keep `StepAssembly` and `StepAssemblyInput` only as obsolete source-compatibility
  conveniences implemented through the composable document API. The native ABI 1.2
  export remains for binary compatibility but is no longer the Sample or package path.

## Consequences

The assembly Sample now demonstrates reusable document operations instead of a fixed
workflow facade. Existing applications can migrate incrementally without an immediate
source or native ABI break. Import must run inside an open XDE transaction and all
returned labels are invalid after destination-document disposal.

## Validation

Release and Debug runtime tests must import two STEP files, create an application-chosen
assembly with distinct locations, write STEP, read it back through both geometry and XDE,
and verify 12 faces, one free assembly root, and two component occurrences. The clean
NuGet consumer must also import and wrap a metadata-bearing STEP assembly.

## Upgrade impact

Re-check `XCAFDoc_Editor::CloneShapeLabel`, metadata clone flags, visual/physical material
copying, free-root enumeration, transaction rollback, and STEPCAF reader/writer modes on
every OCCT upgrade.

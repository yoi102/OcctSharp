# ADR-0045: Parent-bound XDE metadata and assemblies

- Status: Accepted
- Date: 2026-08-23
- Scope: B16 XDE metadata profile on OCCT 8.0.1, Windows x64

## Decision

Build XDE on B15's owning document and stable-entry label contract. `XdeDocument` owns
the application/document wrapper; `XdeLabel` stores only a TDF entry and parent reference.
XCAF shape/color/layer/material tools, component sequences, references, and STEPCAF
reader/writer state remain native-local.

Shapes and occurrence locations cross as independent existing owners. Names, layer
lists, RGBA values, material records, entry lists, and assembly counts cross as copies.
The friendly effective shape color writes Gen/Surf/Curv channels together and reads in
that order because STEPCAF may import an overall entity color as Surf or Curv rather
than Gen. Assembly occurrences expose copied stable entries, referred-part entries, and
owning location copies; they never expose `TDataStd_TreeNode` references.

BinXCAF and STEPCAF are both supported. Transactions are required for mutation, and
labels become unusable after the parent document is disposed.

## Validation

Release and Debug pass 32 generator and 67 runtime tests. One scenario validates shape,
assembly, occurrence/reference, translated location, two layers, name, effective RGB,
physical material, and topology in memory, after BinXCAF save/open, and after STEPCAF
write/read. Freshness verifies 12 files. The alpha.37 clean consumer repeats BinXCAF and
STEPCAF metadata workflows through ABI 1.29/bridge 0.37.0 with 44 DLLs under `occt`.

## Upgrade impact

Re-check XCAF label/reference structure, color-channel mapping, layer ordering,
material tree references, STEPCAF mode defaults, component/free-shape enumeration,
location composition, and TKBinXCAF closure on every OCCT upgrade.

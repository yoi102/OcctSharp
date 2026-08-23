# ADR-0044: Owning OCAF documents and stable-entry parent-bound labels

- Status: Accepted
- Date: 2026-08-23
- Scope: B15 OCAF document profile on OCCT 8.0.1, Windows x64

## Decision

Represent `TDocStd_Document` with one owning native wrapper that also retains its
`TDocStd_Application`. Represent managed labels by their stable colon-separated TDF
entry plus a strong reference to the parent `OcafDocument`; never expose a `TDF_Label`
layout, node pointer, or independent label release operation.

All mutations require an explicit `OcafTransaction`. Commit and abort stay native;
disposing an uncommitted transaction aborts it. `TDataStd_Name` values cross as copied
UTF-8. Binary persistence uses a call-local registered BinOcaf driver through the
document's retained application. Disposing the document aborts an open command, closes
the application session, and makes every previously issued managed label unusable.

OCCT `AbortCommand()` rolls back attributes but may retain newly allocated empty label
nodes in memory. This behavior is observable through `ChildCount` and is preserved;
the default binary writer omits those empty labels.

## Validation

Release and Debug pass 32 generator and 66 runtime tests. Tests cover commit, implicit
abort, explicit abort, mutation outside a transaction, save rejection during a command,
UTF-8 names, stable-entry lookup, empty-label retention, binary save/open, and label
failure after parent disposal. Freshness verifies 12 generated files. The alpha.36
clean consumer executes create/commit/save/open through ABI 1.28/bridge 0.36.0 and loads
43 DLLs from the application-local `occt` directory.

## Upgrade impact

Re-check TDF entry stability, transaction semantics, empty-label abort behavior,
BinOcaf status values, UTF-8 conversion, application session close behavior, and
TKBin/TKBinL dependency closure for each OCCT upgrade.

# Batch I document state, attribute graph, history, and persistence gap inventory

This document locks the product denominator and complete cross-family dependency closure
for Batch I before implementation. It measures one document-centric editing workflow,
not individual TDF, TDataStd, TDocStd, TNaming, persistence-driver, or XDE class counts.

Preparation status: **COMPLETE**. Implementation status: **NOT STARTED (0/24)**. The
denominator below is immutable for Batch I.

## Product outcome

A Windows x64 .NET application can inspect and mutate a copied, typed view of OCAF/XDE
labels and attributes; traverse reference and dependency graphs; execute named commands;
inspect, undo, and redo durable history; track dirty/savepoint state; and round-trip the
same logical document through BinOcaf, XmlOcaf, BinXCAF, XmlXCAF, and STEP/XDE without
exposing TDF labels, attribute handles, delta lists, iterators, or persistence drivers.

```text
owned OCAF/XDE document and parent-bound stable entries
  -> native-local label/attribute/reference/delta traversal
  -> copied typed snapshots and managed dependency graph
  -> named command, commit/abort, undo/redo, dirty/savepoint state
  -> binary/XML persistence plus STEP/XDE integration
  -> source-disposal and clean-package consistency workflow
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch I exit evidence |
|---:|---|---|---|
| 1 | TDF label identity | Copy stable entry, tag, depth, root state, and optional parent entry | Root/child/grandchild fixtures pass |
| 2 | TDF traversal | Copy direct and recursive child tables in deterministic label order | Empty and nested trees pass |
| 3 | TDF attributes | Enumerate copied attribute GUID, runtime type, and label-entry metadata | Multiple attributes on one label pass |
| 4 | TDataStd text | Set/get/remove copied Name, Comment, and ASCII string values | Unicode and absent-value fixtures pass |
| 5 | TDataStd scalar | Set/get/remove integer and real values | Boundary, finite-value, and absent-value fixtures pass |
| 6 | TDataStd arrays | Set/get/remove copied integer and real arrays with explicit logical bounds | Empty/invalid/bounded arrays pass |
| 7 | Mutation | Replace or forget supported attributes only inside an open command | Outside-command and abort behavior pass |
| 8 | TDF reference | Set/get/clear a direct same-document label reference | Target replacement and removal pass |
| 9 | Reference collections | Set/get/clear an ordered copied reference array | Empty, duplicate, and missing-target behavior pass |
| 10 | TDataStd tree | Connect, move, and detach application tree nodes by copied entries | Parent/child order and reparenting pass |
| 11 | TNaming | Copy optional named topology as an independent owning `Shape` | Source/document disposal tests pass |
| 12 | Document snapshot | Copy the complete label/attribute table into an immutable managed snapshot | Snapshot survives document disposal |
| 13 | Dependency graph | Build copied outgoing edges from direct references, reference arrays, tree nodes, and XDE occurrences | All edge kinds are classified |
| 14 | Reverse graph | Query deterministic incoming dependents without retaining document state | Multi-source target fixture passes |
| 15 | Graph diagnostics | Report roots, leaves, cycles, strongly connected groups, and topological order where acyclic | Acyclic and cyclic fixtures pass |
| 16 | Named commands | Begin, commit, and abort one explicitly named document command | Name and changed/no-change results pass |
| 17 | History configuration | Get/set bounded or unlimited undo depth with validation | Zero, finite, and unlimited policies pass |
| 18 | Undo | Undo one committed command and expose the resulting availability/dirty state | Attribute and reference rollback pass |
| 19 | Redo | Redo one undone command and expose the resulting availability/dirty state | Attribute and reference replay pass |
| 20 | History snapshots | Copy undo/redo entries with name, time range, changed labels, and delta counts | Multiple named commands pass |
| 21 | History branching | Clear undo/redo and invalidate redo after a new post-undo command | Branch and limit-trimming fixtures pass |
| 22 | Dirty/savepoint | Expose changed state and explicitly mark/restore a savepoint boundary | Commit, save, undo, and redo transitions pass |
| 23 | Binary/XML persistence | Round-trip equivalent generic and XDE state through BinOcaf, XmlOcaf, BinXCAF, and XmlXCAF | Four-format logical snapshot comparison passes |
| 24 | STEP/XDE/package | Execute STEP import, document mutation/graph/history, persistence reload, STEP export, and source-disposal in repository and clean package runtime | 62-DLL Preview.6 workflow passes |

No attribute-only, graph-only, undo-only, XML-only, XDE-only, numbered, or dotted
fragment is a Batch I completion point.

## Root-declaration audit

The Preview.5 final inventory was queried for exactly 24 decision-driving roots:
`TDocStd_Application`, `TDocStd_Document`, `TDF_Data`, `TDF_Label`, `TDF_Tool`,
`TDF_ChildIterator`, `TDF_AttributeIterator`, `TDF_Attribute`, `TDF_Reference`,
`TDF_Delta`, `TDataStd_Name`, `TDataStd_Comment`, `TDataStd_Integer`,
`TDataStd_Real`, `TDataStd_AsciiString`, `TDataStd_IntegerArray`,
`TDataStd_RealArray`, `TDataStd_ReferenceArray`, `TDataStd_TreeNode`,
`TNaming_NamedShape`, `BinDrivers`, `BinXCAFDrivers`, `XmlDrivers`, and
`XmlXCAFDrivers`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 288 | Requires native-local document/iterator/delta/driver work or copied snapshots |
| `Emitted` | 219 | Reused only where generated ownership already matches |
| `Manual` | 4 | Existing label/name/real-array behavior is not counted again |
| `Skipped` | 165 | Destructors, metadata, protected helpers, and unsafe declarations remain excluded |
| **Total** | **676** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only newly direct blocked overloads used by the implementation will be reconciled under
SC-045. This audit will not be bulk-marked manual.

## Cross-family dependency closure

- `TDF_Data`, label/attribute iterators, TDataStd/TNaming attributes, TDF delta lists,
  TDocStd command state, and all binary/XML/STEP drivers remain document- or call-local.
- Stable entries remain parent-bound. Attribute, reference, history, and document
  snapshots contain copied scalars, strings, GUID/type names, arrays, entries, and
  independently owning topology only; no native iterator, attribute, delta, or label
  handle crosses the ABI.
- Undo/redo mutate one owned document only when no command is open. Managed history
  snapshots are descriptive copies and never become executable native delta owners.
- Savepoint and dirty state follow `TDocStd_Document` time semantics. Saving cannot run
  during an open command, and opening a persisted file starts from a clean savepoint.
- Dependency analysis is managed-owned over copied edges. Cycles are reported rather
  than silently traversed, and cross-document references are rejected at mutation time.
- One managed assembly, one native DLL, one package, stable public full names, and the
  accepted generated shard dependency graph remain unchanged.

## Validation gates

Batch I reaches 24/24 only when SC-045 reconciliation, focused tests, full Release and
Debug builds, Generator and Runtime suites, real STEP/XDE plus all four OCAF/XCAF
persistence formats, undo/redo/lifetime behavior, the clean 62-DLL package consumer,
generated freshness, byte-identical regeneration, API compatibility, full inventory,
runtime hashes, SBOM/provenance/checksums, documentation, and the complete Preview.6
local release check all pass together.

## Explicit non-goals

Arbitrary third-party/custom attribute serializers, remote document links, multi-
document atomic commands, concurrent mutation, native delta editing/replay outside the
owning document, collaborative merge, database persistence, physical assembly/DLL/
package splitting, hosted release, signing, NuGet publication, and GitHub work are
outside Batch I.

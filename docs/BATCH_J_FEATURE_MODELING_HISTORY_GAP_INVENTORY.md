# Batch J advanced feature modeling, history, and recovery gap inventory

This document locks the product denominator and complete cross-family dependency closure
for Batch J before implementation. It measures one editable feature-modeling workflow,
not individual BRepFilletAPI, BRepOffsetAPI, BRepFeat, BOPAlgo, BRepTools, ShapeUpgrade,
or BRepCheck class counts.

Preparation status: **COMPLETE**. Implementation status: **NOT STARTED (0/24)**. The
denominator below is immutable for Batch J.

## Product outcome

A Windows x64 .NET application can build and revise local solid features, configure and
diagnose robust Boolean work, recover from invalid input, and retain copied generated,
modified, and deleted topology history through STEP/XDE and viewer review without
exposing builders, maps, progress indicators, borrowed subshapes, or native history.

```text
owning input shapes and explicit feature selections/options
  -> native-local fillet/chamfer/draft/feature/BOP builders
  -> independently owning result and copied diagnostics/history
  -> validation, healing, simplification, STEP/XDE and viewer review
  -> input/result disposal and clean-package consistency workflow
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch J exit evidence |
|---:|---|---|---|
| 1 | Fillet | Apply constant-radius fillets to an explicit owning edge selection | Single and multiple contour fixtures pass |
| 2 | Variable fillet | Apply start/end-radius laws along selected edges | Unequal-radius fixture and invalid-law diagnostics pass |
| 3 | Chamfer | Apply symmetric and two-distance chamfers to selected edges | Both modes and wrong-support failure pass |
| 4 | Planar feature | Apply planar wire fillet/chamfer editing without borrowed explorer state | Closed-profile fixture passes |
| 5 | Draft | Draft selected faces around an axis and neutral plane | Positive/negative angle fixtures pass |
| 6 | Boss | Add a bounded prismatic boss from a profile and support face | Owning solid result passes |
| 7 | Pocket | Remove a bounded prismatic pocket from a profile and support face | Depth and through-all fixtures pass |
| 8 | Hole | Cut cylindrical through/blind holes with explicit placement | Blind/through and invalid-radius fixtures pass |
| 9 | Revolved feature | Add or remove a bounded revolved profile feature | Additive/subtractive fixtures pass |
| 10 | Pipe feature | Add or remove a profile swept along a spine | Additive/subtractive fixtures pass |
| 11 | Split | Split one or more arguments by multiple tools in one operation | Deterministic piece table passes |
| 12 | Defeaturing | Remove selected faces while healing the surrounding solid | Hole/boss removal fixture passes |
| 13 | Boolean cells | Select material/non-material cells from multiple arguments | Inclusion/exclusion fixture passes |
| 14 | Batch Boolean | Fuse, cut, common, and section multiple arguments/tools | Multi-body fixture passes |
| 15 | Robust options | Configure fuzzy tolerance, parallel mode, non-destructive mode, and glue | Option validation and execution pass |
| 16 | Preflight | Audit self-interference, small edges, rebuild risk, and argument compatibility | Valid and deliberately bad fixtures pass |
| 17 | Diagnostics | Copy completion state, errors, warnings, bad-shape counts, and stage text | Success and failure diagnostics pass |
| 18 | Modified history | Copy input-to-modified topology as independently owning shapes | Source disposal test passes |
| 19 | Generated history | Copy input-to-generated topology as independently owning shapes | Fillet/feature fixture passes |
| 20 | Deleted history | Report deleted selected/input topology by stable request index | Delete/retain fixture passes |
| 21 | Recovery | Validate, heal, same-domain simplify, and optionally retry a failed operation | Recovery policy fixture passes |
| 22 | STEP/XDE | Import, feature-edit, preserve copied metadata, and export/re-read | Real STEP/XDE round trip passes |
| 23 | Viewer review | Present result/history groups, select them, fit, and capture a screenshot | Real HWND workflow passes |
| 24 | Package/lifetime | Repeat the complete feature/history/recovery chain from a clean package | 62-DLL Preview.7 workflow passes |

No fillet-only, chamfer-only, Boolean-only, history-only, recovery-only, numbered, or
dotted fragment is a Batch J completion point.

## Root-declaration audit

The Preview.6 final inventory was queried for exactly 24 decision-driving roots:
`BRepFilletAPI_MakeFillet`, `BRepFilletAPI_MakeChamfer`,
`BRepFilletAPI_MakeFillet2d`, `BRepOffsetAPI_DraftAngle`,
`BRepOffsetAPI_MakeDraft`, `BRepOffsetAPI_MakeOffset`,
`BRepOffsetAPI_MakeOffsetShape`, `BRepFeat_MakePrism`, `BRepFeat_MakeRevol`,
`BRepFeat_MakePipe`, `BRepFeat_MakeDPrism`, `BRepFeat_MakeCylindricalHole`,
`BRepFeat_MakeLinearForm`, `BRepFeat_MakeRevolutionForm`, `BRepFeat_SplitShape`,
`BRepAlgoAPI_Defeaturing`, `BOPAlgo_RemoveFeatures`, `BOPAlgo_Splitter`,
`BOPAlgo_CellsBuilder`, `BOPAlgo_ArgumentAnalyzer`, `BRepTools_History`,
`BRepTools_ReShape`, `ShapeUpgrade_UnifySameDomain`, and `BRepCheck_Analyzer`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 374 | Requires native-local builder/history/map work or copied results |
| `Emitted` | 16 | Reused only where generated ownership already matches |
| `Manual` | 16 | Existing basic modeling behavior is reused, not counted again |
| `Skipped` | 300 | Destructors, operators, protected helpers, and unsafe declarations remain excluded |
| **Total** | **706** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only newly direct blocked overloads used by the implementation will be reconciled under
SC-046. This audit will not be bulk-marked manual.

## Cross-family dependency closure

- BRepFilletAPI/BRepOffsetAPI/BRepFeat/BOPAlgo builders, progress objects, indexed maps,
  contour state, and BRepTools history remain operation-local.
- Inputs and explicit edge/face/profile/spine selections are borrowed only for one call.
  Every result, modified shape, and generated shape crossing the ABI is an independent
  registered owning `Shape`; deleted state crosses only as copied request indices.
- Options and diagnostics cross as validated copied scalars/enums/UTF-8 strings. Native
  error/warning lists, alerts, maps, iterators, and context objects never cross the ABI.
- Recovery is explicit and bounded: validate, optional ShapeFix, optional same-domain
  unification, and at most one retry. It cannot silently replace the caller's inputs.
- XDE labels remain parent-bound stable entries and viewer presentations remain creating-
  thread-affine parent IDs. The feature result/history snapshots retain neither owner.
- One managed assembly, one native DLL, one package, stable public full names, and the
  accepted generated shard dependency graph remain unchanged.

## Validation gates

Batch J reaches 24/24 only when SC-046 reconciliation, focused tests, full Release and
Debug builds, Generator and Runtime suites, real STEP/XDE plus real-HWND evidence, input,
result, and history lifetime tests, the clean 62-DLL package consumer, generated
freshness, byte-identical regeneration, API compatibility, full inventory, runtime
hashes, SBOM/provenance/checksums, documentation, and the complete Preview.7 local
release check all pass together.

## Explicit non-goals

Interactive native callbacks, arbitrary user-defined fillet laws, feature-tree solver
plugins, concurrent mutation, persistent native builder/history objects, cross-document
transactions, custom BOP allocators, physical assembly/DLL/package splitting, hosted
release, signing, NuGet publication, and GitHub work are outside Batch J.

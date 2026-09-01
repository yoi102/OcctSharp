# ADR-0075: Implement interactive assembly placement editing as Batch M

- Status: Accepted and implemented
- Date: 2026-09-01
- Scope: Batch M product denominator, viewer/manipulator ownership, XDE placement editing, and validation

## Context

Batch D provides production review interaction, Batch I provides named document history,
Batch K provides assembly occurrence editing, and Batch L provides occurrence-aware DMU
analysis. The missing product closure is a safe interactive path connecting those
families: manipulate one displayed occurrence, preview or cancel its local placement,
commit it transactionally, validate the moved assembly, and persist/review the result.

Directly exposing `AIS_Manipulator`, `AIS_InteractiveObject`, `V3d_View`, or their C++
layouts would violate the accepted HWND/thread/parent ownership boundary. Treating
preview as immediate document mutation would also make cancel, replacement occurrence
labels, and undo/redo semantics ambiguous.

## Decision

Open Batch M as the indivisible 24-capability wave in
`BATCH_M_INTERACTIVE_PLACEMENT_EDITING_GAP_INVENTORY.md`.

Keep manipulators in the existing viewer native registry and identify them through
viewer-parent-bound integer IDs. Keep all AIS/V3d handles native-local and thread-affine.
Return transformations through the existing registry-owned `GpTrsf` bridge. Presentation
get/set/reset uses the same local-placement semantics as manipulator preview.

Add a managed occurrence placement edit session that separates preview from document
mutation. It captures the original presentation transform and occurrence local location,
rejects non-rigid placement, commits through a named XDE transaction and
`RelocateOccurrence`, returns the replacement occurrence label, and restores preview on
cancel/disposal. Existing document undo/redo and DMU/STEP/viewer APIs compose the rest of
the closure rather than creating duplicate native object models.

The completed additive wave uses package `8.0.1-preview.11`, native ABI 1.55, bridge
0.63.0, and schema 1.13. It keeps the Preview.10 managed module graph and exactly one
`OcctSharp.Native.dll`/62-DLL runtime package.

## Alternatives considered

- Exposing generated AIS shared wrappers was rejected because they do not encode the
  viewer registry, UI-thread affinity, or presentation-parent lifetime.
- A standalone native manipulator DLL was rejected because the creator-owned AIS/viewer
  registry is not cross-DLL safe and ADR-0074 deliberately retains one native bridge.
- Mutating XDE on every mouse move was rejected because replacement-label and undo history
  would become unstable and cancel would require compensating document mutations.
- Allowing scale/mirror in occurrence editing was rejected because XDE assembly placement
  is a rigid location contract; scaling remains available for ordinary presentations.

## Consequences

- Viewer/presentation/manipulator ownership and thread errors are deterministic.
- Preview is cheap and reversible; document identity changes only at explicit commit.
- Eight exact direct blocked declarations enter SC-049; the root audit is not bulk-marked
  manual.
- Existing public/generated APIs are retained. The facade gains cross-family workflow
  types while the managed module and native package topology remains unchanged.
- Hosted release, signing, publication, GitHub, and push remain outside this decision.

## Validation required

Focused apply/cancel/transform/ownership and occurrence-history tests; full Release and
Debug Generator/Runtime suites; real STEP/XDE, DMU, HWND screenshot, and clean-package
workflow; generation/freshness/compatibility/inventory/runtime/SBOM/provenance/checksum
gates; documentation synchronization; and `git diff --check`.

## Related decisions

- ADR-0018 and ADR-0019: opaque transform/location value bridges.
- ADR-0045: parent-bound XDE labels.
- ADR-0046: HWND/thread-affine viewer and presentation IDs.
- ADR-0065: OCCT-aligned preview versions.
- ADR-0070: named document history and undo/redo.
- ADR-0073: occurrence-aware DMU analysis.
- ADR-0074: managed module split and one shared native runtime.

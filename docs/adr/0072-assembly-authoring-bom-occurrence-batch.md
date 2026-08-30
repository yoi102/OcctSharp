# ADR-0072: Implement assembly authoring, BOM, and occurrence workflows as Batch K

- Status: Accepted for implementation
- Date: 2026-08-30
- Scope: Batch K product denominator, dependency closure, ownership, and validation

## Context

Existing XDE APIs can create parts, assemblies, components, and copied occurrence
snapshots, while production product-structure editing still requires unmanaged OCCT code
for relocation, relinking, removal policies, subtree cloning, reverse usage, structured
BOMs, assembly-item/external references, SHUO, instance overrides, and structure
diagnostics. These requirements cross XCAFDoc, TDF/TDataStd, STEPCAFControl,
STEPControl, TopLoc, TopoDS, BRepGProp, XCAFPrs, and AIS.

## Decision

Open Batch K as one 24-capability product wave named **assembly authoring, BOM, and
occurrence workflows**. The immutable denominator and 24-root/1,228-declaration audit are
in `BATCH_K_ASSEMBLY_AUTHORING_BOM_GAP_INVENTORY.md`.

XCAF/TDF tools, labels, attributes, iterators, graphs, and STEP sessions remain native-
local. Managed code addresses document-bound objects through stable entries and receives
copied immutable structure/BOM/diagnostic records plus independently owning topology.
All structural mutation is named-transaction-bound and rollback-safe.

The batch retains one `OcctSharp.dll`, one `OcctSharp.Native.dll`, one NuGet package,
stable public type full names, and the accepted generated shard graph. Implementation
targets Preview.8, native ABI 1.53, bridge 0.61.0, and configuration schema 1.11; these
identities are targets, not current validated artifacts during preparation.

## Locked non-goals

PLM/PDM integration, implicit network loading, proprietary translators, native callbacks,
concurrent mutation, cross-document atomic transactions, persistent native tool/iterator
wrappers, physical deliverable splitting, hosted release, signing, publication, and
GitHub work.

## Consequences

- Preparation freezes all 24 capabilities before implementation starts.
- Editing, BOM, references, metadata, properties, exchange, viewer, and package evidence
  are not separate completion checkpoints.
- SC-047 will record only newly direct blocked declarations actually used by the final
  implementation; the 610 blocked audit candidates are not bulk-marked manual.
- Prior Batch B-J evidence and Preview.7 artifacts remain immutable.

## Validation required

Focused structure/edit/BOM/reference/metadata/property/history/lifetime tests, real
STEP/XDE, real-HWND presentation and screenshot, the clean-package workflow,
Release/Debug, generator/runtime suites, regeneration, compatibility, inventory, runtime
manifest, SBOM/provenance/checksums, and the complete Preview.8 local release gate must
pass together before Batch K is complete.

All implementation validation is `NOT RUN` at this preparation checkpoint.

## Related decisions

- ADR-0005/0009: native status and owning registered shapes.
- ADR-0044/0045: owned OCAF/XDE documents and parent-bound stable-entry labels.
- ADR-0046: thread-affine viewer resources.
- ADR-0049: implementation and publication gates remain separate.
- ADR-0052: native-local algorithms and exact manual stable-ID accounting.
- ADR-0061/0062: generated layering and cross-shard closure.
- ADR-0065: OCCT-aligned preview numbering.
- ADR-0070: document state, dependency graph, history, and persistence foundation.
- ADR-0071: completed feature-modeling/history/recovery workflow.

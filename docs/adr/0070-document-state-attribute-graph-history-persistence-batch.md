# ADR-0070: Implement document state, attribute graphs, history, and persistence as Batch I

- Status: Accepted for implementation
- Date: 2026-08-30
- Scope: Batch I product denominator, dependency closure, ownership, and validation

## Context

Batch H closes advanced mesh and scene interchange, while routine parametric and
application-document workflows still need unmanaged OCAF code to inspect typed label
state, understand references, expose command history, undo/redo safely, track savepoints,
and persist the same logical state through binary and XML OCAF/XCAF formats. The missing
workflow crosses TDF labels/attributes/references/deltas, TDataStd values and trees,
TDocStd document commands, TNaming topology, persistence drivers, and existing XDE/STEP.

## Decision

Open Batch I as one 24-capability product wave named **document state, attribute graph,
history, and persistence**. The immutable denominator and 24-root/676-declaration audit
are in `BATCH_I_DOCUMENT_HISTORY_PERSISTENCE_GAP_INVENTORY.md`.

The existing owned-document/stable-entry boundary remains authoritative. Label,
attribute, reference, iterator, delta, driver, and STEPCAF objects stay native-local.
Managed code receives copied typed attribute and history snapshots, copied reference
edges, and independent owning topology. Undo/redo are parent-document operations;
history snapshots never own or replay native deltas. Managed dependency graphs retain no
document or label lifetime.

The batch retains one `OcctSharp.dll`, one `OcctSharp.Native.dll`, one NuGet package,
stable public type full names, and the accepted generated shard graph. Implementation
advances the package to Preview.6, native ABI to 1.51, and bridge to 0.59.0.

## Locked non-goals

Custom attribute-driver plugins, remote links, cross-document atomic transactions,
concurrent mutation, user-editable native delta streams, collaborative merge, database
persistence, physical deliverable splitting, hosted release, signing, publication, and
GitHub work.

## Consequences

- Preparation freezes all 24 capabilities before implementation starts.
- Attributes, dependency graphs, history, undo/redo, savepoints, persistence formats,
  and STEP/XDE are not separate completion checkpoints.
- SC-045 records only newly direct blocked declarations actually used by implementation.
- Prior Batch B-H evidence remains immutable.

## Validation required

Focused label/attribute/reference/graph/history/dirty/lifetime tests, real BinOcaf,
XmlOcaf, BinXCAF, XmlXCAF, and STEP/XDE round trips, the clean-package workflow,
Release/Debug, generator/runtime suites, regeneration, compatibility, inventory, runtime
manifest, SBOM/provenance/checksums, and the complete Preview.6 local release gate must
pass together before Batch I is complete.

## Related decisions

- ADR-0044: owned OCAF documents and stable-entry labels.
- ADR-0045: parent-bound XDE metadata and assembly references.
- ADR-0049: implementation and publication gates remain separate.
- ADR-0052: native-local algorithms and exact manual stable-ID accounting.
- ADR-0059: committed Windows runtime and MIT licensing.
- ADR-0061/0062: generated layering and cross-shard closure.
- ADR-0065: OCCT-aligned preview numbering.
- ADR-0069: completed advanced mesh/scene boundary.

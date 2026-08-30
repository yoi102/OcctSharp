# ADR-0071: Implement advanced feature modeling, history, and recovery as Batch J

- Status: Accepted for implementation
- Date: 2026-08-30
- Scope: Batch J product denominator, dependency closure, ownership, and validation

## Context

Batch F supplies freeform/profile authoring and basic result-oriented features, while
production solid editing still requires unmanaged OCCT code for selected and variable
fillets, asymmetric chamfers, draft, local bosses/pockets/holes, multi-argument BOP work,
defeaturing, robust options, diagnostics, and generated/modified/deleted history. These
requirements cross BRepFilletAPI, BRepOffsetAPI, BRepFeat, BOPAlgo, BRepTools,
ShapeUpgrade, BRepCheck, STEP/XDE, and AIS.

## Decision

Open Batch J as one 24-capability product wave named **advanced feature modeling,
history, and recovery**. The immutable denominator and 24-root/706-declaration audit are
in `BATCH_J_FEATURE_MODELING_HISTORY_GAP_INVENTORY.md`.

Builders, progress state, native maps, alerts, and history remain native-local. Managed
code supplies owning shapes and copied options, then receives independently owning result
and history topology plus copied diagnostics/deleted indices. Recovery is explicit,
bounded, and never mutates or substitutes the caller's inputs silently.

The batch retains one `OcctSharp.dll`, one `OcctSharp.Native.dll`, one NuGet package,
stable public type full names, and the accepted generated shard graph. Implementation
targets Preview.7, native ABI 1.52, bridge 0.60.0, and configuration schema 1.10.

## Locked non-goals

Native callbacks, arbitrary user-defined law objects, persistent builder wrappers,
feature-tree solver plugins, concurrent mutation, editable native history maps, custom
allocators, physical deliverable splitting, hosted release, signing, publication, and
GitHub work.

## Consequences

- Preparation freezes all 24 capabilities before implementation starts.
- Feature families, robust BOP, diagnostics/history, recovery, exchange, viewer, and
  package evidence are not separate completion checkpoints.
- SC-046 records only newly direct blocked declarations actually used by implementation.
- Prior Batch B-I evidence remains immutable.

## Validation required

Focused feature/options/diagnostics/history/recovery/lifetime tests, real STEP/XDE,
real-HWND presentation and screenshot, the clean-package workflow, Release/Debug,
generator/runtime suites, regeneration, compatibility, inventory, runtime manifest,
SBOM/provenance/checksums, and the complete Preview.7 local release gate must pass
together before Batch J is complete.

## Related decisions

- ADR-0005/0009: native status and owning registered shapes.
- ADR-0045/0046: parent-bound XDE labels and thread-affine viewer resources.
- ADR-0049: implementation and publication gates remain separate.
- ADR-0052: native-local algorithms and exact manual stable-ID accounting.
- ADR-0059: committed Windows runtime and MIT licensing.
- ADR-0061/0062: generated layering and cross-shard closure.
- ADR-0065: OCCT-aligned preview numbering.
- ADR-0067: completed freeform and profile-to-solid foundation.
- ADR-0070: completed document-state/history/persistence boundary.

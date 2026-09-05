# ADR-0087: Typed parametric recompute and fallible persistent selection

- Status: Accepted and locally validated; final package-content/commit evidence in STATUS.
- Date: 2026-09-05.

## Context

Implement the unchanged forty [T capabilities](../BATCH_T_PARAMETRIC_DOCUMENT_RECOMPUTE_GAP_INVENTORY.md)
after S commit `580bb22`. Preserve the original Preview.15 preparation and record the
post-S entry separately. Q/R/S are validated prerequisites, not new T capabilities.

## Decision

Documents owns copied versioned feature/parameter/expression contracts and internal
document storage, TFunction graph, logbook and TNaming operations. The existing facade
owns built-in evaluation and XDE/viewer orchestration. Existing OcafDocument/XdeDocument
types retain their assembly identity. Internal storage accepts their existing shared
document SafeHandle; it creates no second native owner or registry. Use cohesive native
Documents units; one DLL and the existing managed project graph remain unchanged.

Feature identifiers, definition revisions, result generations and document identities
are explicit persisted values. Stable label entries alone do not validate cached results.
Named parameters distinguish absent values from zero/empty. Dimensions are checked before
execution and scalar expressions are bounded immutable arithmetic trees (literal,
reference, add/subtract/multiply/divide/negate/min/max), never arbitrary executable code.
Persist recipe values and native TDF references, not serialized managed handles.

Evaluate a deterministic DAG into temporary owning results. Publish successful target
closures in one named document transaction; a failed/cancelled run releases candidates
and does not partially replace old results. Failure/blocked diagnostics are distinct from
accepted geometry. Last-good access must explicitly disclose staleness. Cancellation is
between synchronous calls, not inside an arbitrary OCCT operation. Persisted executing
states are recovered as dirty after reopen. Read state from the document after undo/redo;
no mutable managed cache is authoritative.

Record only actual generated/modified/deleted history in TNaming. A root replacement is
not proof of subshape identity. Dedicated selector children isolate TNaming_Selector.Select
from feature/user metadata. Resolve against the current context and expected type;
deleted, ambiguous and unsupported mappings are explicit rather than proximity guesses.
History snapshots own their extracted topology independently of the document. TDF
relocation copies closed feature subgraphs; external dependencies require a stated policy.

OCCT TDF_Data.Transaction is an active transaction nesting level, not a durable
document version. Published evolution labels are append-only and indexed by persisted
result-generation GUIDs. GetHistory can select an older generation across save/reopen;
undo removes a reverted generation and redo restores it. Algorithm input-owner
associations (Boolean/S) are copied separately and never advertised as exact source-
subshape correspondence. Exact source transforms record the real ModifiedShape map.

## Alternatives and consequences

Reject reverse Documents dependencies, managed TFunction_Driver callbacks, arbitrary scripts,
pointer-based identities and a new project/DLL. Do not relabel old attribute/graph getters
as execution support. New C functions require additive ABI/bridge versions; the OCCT-aligned
package slot is Preview.19. No generator schema change is implied by a recipe schema.
Exact directly called manual exceptions are recorded under SC-057 during implementation.

## Validation

All forty original acceptance rows plus Release/Debug/actual Debug-native, lifetime,
missing/unit/expression/cycle/failure/cancellation/atomicity, real four-format persistence
and recompute, selection evolution, undo/redo, repeated XDE definitions, STEP/IGES/HWND,
exact manual accounting, source/private-header/dependency and additive compatibility,
both clean consumers, clean generation, runtime/package bytes and full local release
checks are required. All forty focused cases and ten repeats, Release/Debug and actual
Debug-native Runtime 313/313, strict headers/layout, additive compatibility, exact accounting,
both consumers, 94-file cold regeneration and the full local release-check pass. Final
document-package bytes and completion-commit evidence are recorded in STATUS. Proceed
immediately to U after the whole T commit without routine confirmation or publication.

Related: ADR-0070, ADR-0074, ADR-0081, ADR-0082, ADR-0083, ADR-0084, ADR-0085, ADR-0086.

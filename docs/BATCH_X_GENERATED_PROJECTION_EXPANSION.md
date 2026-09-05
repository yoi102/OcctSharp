# Batch X: Broad generated numeric and copied-reference migration

- State: implementation and local product gates passed under ADR-0092 and WORKFLOW;
  final documentation-package evidence is recorded in STATUS.
- Entry: W `15fd671`, Preview.22; 16,353 generated + 1,217 manual stable IDs.
- Entry inventory: 116,272 declarations; 49,358 Blocked; 49,344 Skipped.
- Entry inventory SHA256: `260AE9A603E7B68F717C38C3FAF9C68A83F866D0154E84E1CD7FD811DB0C6A2C`.
- The ADR-0091 maintenance at `f84e869` is preserved; no frozen Q-W inputs change.

## Accepted outcomes

1. General numeric mapping across canonical float and signed/unsigned integer types,
   aliases and const-reference inputs; native width assertions and exact input casts.
2. Copied const-reference scalar, enum and point returns, plus independently retained
   selected shared-handle returns. Mutable/out references and unknown ownership remain blocked.
3. Compatible overload numbering for the existing generated surface; compare both
   native parameter/body correspondence and managed signatures against Preview.22.
4. Regenerated native/raw/friendly bindings across all eligible selected modules,
   with exact newly emitted IDs and per-package/module counts from actual output.
5. Meaningful numeric boundary, overload selection, copied-value/source disposal and
   retained-handle lifetime tests, including direct-module and clean facade consumers.
6. Full product-delivery validation: Release/Debug, actual Debug native, fresh inventory,
   deterministic clean generation, ABI/API compatibility, packages and local release check.

No fixed capability quota. These outcomes remain required even if discovery or build
finds additional constraints. Candidate counts are not completed migration. Current
results, remaining work and baseline identities are maintained in [STATUS](STATUS.md).

## Implemented breadth

The final regenerated manifest contains 19,682 IDs: 3,329 added, zero removed. The
native signature/body audit preserves all 28,747 old generated functions and groups
the new functions by their physical target module as follows (one new export per ID).

| Module | Added declaration bindings |
|---|---:|
| DataExchange | 1,377 |
| Visualization | 745 |
| Modeling | 334 |
| Documents | 273 |
| Geometry | 218 |
| Foundation | 139 |
| Xde | 87 |
| Mesh | 71 |
| Draw | 71 |
| MeshData | 11 |
| IVtk | 3 |
| Total | 3,329 |

These are declaration counts, not independent user workflows. Source-package attribution
differs from physical emission: Standard accounts for 2,587 additions, predominantly
macro-origin DynamicType references on classes across modules. The public managed API
diff adds 3,287 signatures and removes zero; some generated static bindings remain internal.

The twelve focused runtime cases exercise real byte-array bounds and byte values,
size_t image dimensions, unsigned high-bit flags, null and retained parameter handles,
retained runtime type after source disposal, copied point mutation/disposal independence,
64-bit camera counters, four known half-float encodings, signed values beyond 32 bits,
old/new float overload coexistence, and null-output rejection/error clearing. The shared
public subset also runs in fresh facade and direct-module package consumers.

Evidence files under `OcctSharp/artifacts/`: `batch-x-entry-delta.json`,
`batch-x-source-compatibility.log`, `batch-x-api-diff.json`, `batch-x-focused.log`,
`batch-x-accounting.json` and `batch-x-release-check.log`. Execution results and final
delivery state belong in STATUS; merely listing an evidence path does not claim PASS.

## Exit accounting boundary

The unchanged 116,272-ID denominator contains 19,682 Emitted, 1,217 Manual, 46,028
Blocked and 49,345 Skipped IDs. Exactly 3,329 Blocked IDs become Emitted and the Cocoa
artifact exception becomes Skipped. FlatBezierKnots remains Blocked with BL209.

Another 57 IDs remain Blocked while their first unresolved reason changes after numeric
or reference support: 25 raw-pointer contracts, three mutable/stream references, six
typed-ID templates, one unselected Driver handle, fourteen unsupported value parameters,
and eight nested GeomEval descriptor return targets. Those eight still report BL104:
the discovered target is transient but SharedHandlePackageScopeExpander only selects
simple native identifiers, so no generated nested-descriptor owner exists. They are
explicit remaining work, not successful migration or a reason to suppress the family.

The exact [reason refinement ledger](../OcctSharp/config/batches/batch-x-disposition-refinements.json)
records old/new codes and evidence per ID. It validates accounting and does not override
the generator. `eng/verify-batch-x-accounting.ps1` requires every other disposition and
all header classifications to stay unchanged. No Manual ID, dependency exclusion or
pending state is silently changed.

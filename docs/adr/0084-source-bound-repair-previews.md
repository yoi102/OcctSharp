# ADR-0084: Source-bound repair previews and atomic publication

- Status: Accepted and locally validated as the complete 40-capability Batch Q
- Date: 2026-09-05

## Decision

Implement ADR-0082's complete 40-row Q matrix using immutable Modeling-owned repair
snapshots, typed ordered stages, copied diagnostics/history, protected source selections,
explicit tolerance/geometry budgets and disposable previews. Twenty typed operation
contracts cover the native repair/normalization algorithms; they are not twenty new
delivery batches. The facade owns XDE, exchange and viewer workflow composition.

Every selection combines a snapshot ID, revision and zero-based TopExp-map index.
Never substitute a native address, insertion position after a topology change, or
equal result cardinality for correspondence. Compose native ReShape/history mappings
across stages; unknown/deleted/ambiguous mappings cannot resolve a required selection.
Portable recipes additionally bind to a canonical BRep content fingerprint, explicit
linear unit and revision; serialization contains no native handles or callbacks.

Every stage executes on an independent mutable copy. The copier retains source
occurrence orientation in correspondence and restores copied external pcurves only
for orphan edges; it does not accumulate original-surface representations on copied
faces. Wire traversal uses a complete connected explorer when available, because raw
TopoDS insertion order is not guaranteed to be connected. Contour-consuming hole
removal explicitly orders its working wires. Division repairs SameParameter on its
private output before budget/validity acceptance, with actual tolerance growth checked.

Use one registered native repair-result owner with matching release, the existing
shared Runtime registry/error owners, copied result buffers, and owning Shape values.
Do not add projects or DLLs. Domain translation units compile independently and remain
below the 1,000-line source ceiling. XDE/Visualization integration uses public copied
ABI contracts and existing local domain helpers, not backwards private dependencies.

Failed stages dispose all intermediate outputs. A preview may be accepted only after
all stages complete or explicitly skip and every required budget is verified. Missing
area/closed-volume evidence is Unavailable, not zero drift. Hole/feature removal and
unification remain explicit modeling-intent changes. A protected boundary whose mapping
or preservation cannot be proven rejects the stage.

Shared-definition publication uses a single document transaction. Keep reusable
definition/occurrence labels and placements; update unambiguously mapped subshape
TNaming topology on the same metadata labels. Ambiguous/split/merged/deleted metadata
is reported and blocks publication. The source geometry must still match the captured
definition; a completed publication invalidates the session. Review navigation binds
to the current snapshot and rejects stale defect IDs after replacement.

## Validation and continuation

The complete 40-row test map now includes 25 passing focused tests, explicit vertex
merge history, corner protection, actual placement baking, successful defective-shell
repair and real continuity splits. Final review additionally measures 2D/3D adjacent-edge
residuals and preserves their result-bound edge-pair provenance; missing pcurves remain
unavailable. Full Release passes Generator 91/91 and Runtime
205/205. All 35 private headers pass standalone MSVC checks with warnings as errors.
The STEP style fallback now transfers product roots first, then disconnected style
targets, avoiding context-free styled-face binders breaking shell-based assemblies.
SC-054 reconciles 106 exact direct calls, with no other inventory changes. Bundled
DLL/manifest identity matches ABI 1.60 / bridge 0.68.0. Final Release/Debug and actual
Debug-native sweeps pass 205/205; both clean consumers, fresh-source regeneration,
inventory, compatibility and every required local release gate pass. The 40-row map is
fully locally validated. No public release or physical DLL split is claimed.
STATUS and the continuous journal record current evidence; proceed Q-W without routine
reconfirmation only after each whole batch passes and is locally committed.

Related: ADR-0074, ADR-0081, ADR-0082, ADR-0083; OWNERSHIP, NATIVE_ABI,
BATCH_Q_SHAPE_REPAIR_TOPOLOGY_GAP_INVENTORY and BATCH_CONTINUOUS_EXECUTION.

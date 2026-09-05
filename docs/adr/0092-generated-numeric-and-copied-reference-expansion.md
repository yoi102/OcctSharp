# ADR-0092: Generated numeric and copied const-reference expansion

- Status: Accepted; implementation and local validation recorded in STATUS
- Date: 2026-09-06
- Scope: Batch X, broadly reusable generated projections across the selected Windows core.

## Decision

Extend the centralized type map with TM008: float, signed/unsigned 8/16/32/64-bit
integer representations, including Windows 32-bit long and 64-bit size_t aliases.
Match canonical AST types, cast input values to their exact native canonical type,
and assert native widths and IEEE floating representations in generated compilation.
Plain char, long double, pointers, mutable/out references and rvalue transfers remain
unmapped by this rule. Existing scalar/point rules remain unchanged for direct values.

Copy const lvalue-reference returns only for known scalar/enum/gp_Pnt value projections,
or retain a copy of a selected OCCT intrusive handle before returning an independent
registered wrapper. Never expose the referenced storage address. Existing receiver,
thread and no-concurrent-mutation restrictions still apply during the call. This does
not introduce a general borrowed-object or TopoDS reference-return rule.

Preserve all Preview.22 generated overload ordinals by an immutable embedded stable-ID
baseline captured from its 16,353-ID manifest. Within each existing overload group,
existing IDs retain their old signature order; new IDs are appended deterministically.
Future expansions must preserve X's identities too, through a new ordered baseline or
a persistent signature ledger; never rebuild an old ordinal from only newly eligible
signatures. Native signature/behavior and managed API comparisons are required, not
just checking that old export names still exist.

## Scope and evidence

New static/value ABI calls use status plus initialized output, null-output rejection,
exception containment and the shared thread-local error channel. Existing Preview.22
direct-return calls retain their exact signatures and bodies. Managed internal wrappers
check the new status before returning the copied value.

Global exact binding exclusions may cover static methods outside shared-handle scopes.
Every emitter exclusion still requires a disposition, and the discovery pass rejects
unknown exact IDs. Cocoa_Window::VirtualKeyFromNative(int) has no export in the pinned
Windows runtime (SK008). BSplCLib::FlatBezierKnots returns the start of a static knot
sequence despite its scalar-reference spelling; EL008 / BL209 keeps it Blocked pending
an explicit sequence contract. See SC-061 for source and binary evidence.

Batch X implements the general rules, compatible emission, numeric/reference semantic
regressions, cross-module generated compilation/runtime and package consumers, exact
entry/exit accounting and complete product-delivery gates. It has no arbitrary row
quota or unrelated WPF feature requirement. The entry is W `15fd671` plus the separately
authorized ADR-0091 maintenance recorded in `f84e869`; preserve those changes. Snapshot evidence
is under `OcctSharp/artifacts/preparation-baselines/preview22-batch-x/`.

No bulk reclassification substitutes for emitted APIs. Remaining unsupported signatures
and external dependencies keep precise reasons. New linker/ownership failures must be
resolved or narrowly accounted from actual SDK evidence, never hidden by package-wide
exclusions. The public package target is Preview.23; final ABI/bridge identities and
actual counts are recorded after implementation, not inferred from candidate totals.

## Alternatives and validation

Per-class hand wrappers were rejected for this cross-cutting mapping gap. Returning
borrowed native addresses was rejected because receiver disposal would invalidate them.
Sorting all newly eligible overloads together was rejected because it can silently
redirect old ABI entry points even when export names remain present.

Require rule/emission and stable-ordinal tests, native/managed Release and Debug builds,
numeric limits and copied/retained-return lifetime tests, actual Debug-native runtime,
exact manifest/inventory delta, compatibility, deterministic clean regeneration and
fresh package consumers plus full local release checks. Unexecuted gates are NOT RUN.
One native DLL, existing modules and separate publication authority remain unchanged.

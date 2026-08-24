# ADR-0052: Keep common modeling algorithms native-local and reconcile manual stable IDs

- Status: Accepted
- Date: 2026-08-24
- Scope: B19.3 common modeling APIs on OCCT 8.0.1, Windows x64

## Context

Common application workflows need more than the initial box/sphere/cylinder and Boolean
surface, but OCCT builder objects own mutable execution state, edge contours, history,
progress objects, and references that have no safe general cross-ABI ownership model.
The previous manual APIs were documented but were invisible in the full-inventory binding
accounting, so coverage could not distinguish audited manual declarations from unresolved
blocked declarations.

## Decision

- Keep cone/torus, prism/revolution, fillet/chamfer, offset, section, bounding-box, and
  validity algorithm objects inside one native call. Return normal registered owning
  `Shape` values or fixed copied values only.
- All-edge fillet/chamfer first collect unique edges in a native indexed map. The
  single-edge overload accepts a live edge value and lets the OCCT builder validate
  whether it belongs to the source topology.
- Bounding boxes cross as a fixed 48-byte structure containing six doubles. No `Bnd_Box`
  state, tolerance reference, or open/void representation crosses the ABI.
- Configuration schema 1.6 contains `(stableId, specialCaseId)` manual-binding records.
  Discovery must find every ID; duplicates, malformed special-case IDs, unknown IDs, and
  emitted/manual overlap fail generation or inventory. The full inventory reports these
  declarations as `Manual/MN001` without counting them as generated.
- Algorithm history, contour editing, per-face offset removal, progress/cancellation,
  and underlying builder access remain native-local non-goals for this profile.

## Consequences

The public surface gains common safe workflows without exposing OCCT C++ layouts. The
native closure adds `TKFillet` and `TKOffset`. OCCT upgrades must rediscover every manual
stable ID, so signature drift cannot silently retain stale coverage credit. This is an
accepted manual profile under SC-032, not a claim that the general emitter supports these
algorithm classes.

## Validation

Release and Debug must compile the native and managed surfaces and run cone/torus,
extrusion/revolution, all-edge and single-edge fillet/chamfer, offset, section, bounds,
validity/count, invalid/null/disposed/wrong-kind, fixed-layout, and source-independence
tests. A clean package consumer must load the 47-DLL `occt/` closure and exercise the new
surface. Full inventory must report exactly the configured manual IDs as `Manual/MN001`.

## Upgrade impact

Re-check builder completion/null-result behavior, unique-edge selection, offset defaults,
section semantics, bounding tolerance, analyzer flags, toolkit closure, and all 18 stable
IDs on every OCCT/compiler upgrade.

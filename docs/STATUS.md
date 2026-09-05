# Current Status

- Last updated: 2026-09-06.
- Current work: [Batch Y](BATCH_Y_GEOMETRIC_VALUE_EXPANSION.md), under
  [ADR-0093](adr/0093-generated-geometric-value-projections.md) and WORKFLOW.
- Target: `8.0.1-preview.24`, ABI 1.68, bridge 0.76.0. Configuration 1.13,
  binding model 1.3 and assembly/file 0.1.0.0 remain unchanged.
- Entry: validated Batch X `2cbe55e`, with all evidence preserved in
  [Preview.23 status history](STATUS_HISTORY_THROUGH_PREVIEW_23.md).
- Implementation and focused validation pass. Full Y product delivery is NOT RUN;
  this local implementation checkpoint is not a completed product delivery.

## Implemented scope

Thirty central TM009 geometric value mappings generate fixed C records and immutable
Foundation records in OcctSharp.Values. New bindings cover eight existing modules;
the regenerated manifest has 20,650 IDs, exactly 968 added and none removed. Manual
IDs remain 1,217. Final full-inventory accounting is pending; the first development
scan captured an earlier manifest and is not final accounting evidence.

Existing generated overload epochs preserve Preview.22 and Preview.23 identities.
Inputs validate finite fields, normalize directions without overflow and validate
coordinate-system orthogonality/handedness. Returned values are independent copies.
SC-062 records two exact missing-artifact signatures, one reverse-module contract,
and the initial point-constructor emitter limitation. Arrays, mutable/output references
and general transformation internals remain outside this batch.

## Executed validation

- PASS: Release native/managed build and 144/144 generator regressions on final source.
- PASS: all 32,076 X generated native signatures/bodies preserved in the source audit.
- PASS: Release/Debug native value fixture, thirty native size/offset contracts,
  non-symmetric matrix semantics and all 21 generated C headers as strict C11.
- PASS: 12/12 focused runtime cases, including geometry round trips, copied lifetime,
  layout, derivatives, invalid frames/domains/nonfinite inputs, large directions,
  Documents/XDE/IGES values and error/output handling.
- PASS: bundled Release closure matches the built candidate; 62 DLLs and eleven notices.
- PASS: managed API against X adds 1,085 signatures and removes none.
- PASS: 3,491 generated facade forwarders; dependency closure has no reverse edge/cycle.
- NOT RUN: final full suites/configurations, actual Debug-native, native export comparison,
  fresh final inventory/accounting, cold regeneration, package consumers and release check.
- GitHub push, signing, hosted CI and NuGet publication: NOT RUN, outside this task.

## Next action

Save this local implementation checkpoint, then finish the full product gates and
documentation/package verification.
There is no additional fixed-size migration queue in this request.

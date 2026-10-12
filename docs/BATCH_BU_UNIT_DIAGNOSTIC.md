# BU: Bounded units, numeric limits and copied diagnostics

State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.
Candidate Preview.70 retains ABI1.111/bridge0.119.0. BU composes existing generated
native unit/message contracts and adds managed API without a new native export.

## Generation and behavior

The standard generator validates25 exact reused dependency signatures and emits
Foundation.UnitDiagnosticWorkflow.Generated.cs from an embedded template.
Missing, duplicate, unsafe or changed dependencies fail generation; minimal
profiles with none emit no composition. All318 BT outputs must remain unchanged.
No new SDK declaration or Manual migration credit. See [admission](BATCH_BU_UNIT_DIAGNOSTIC_ADMISSION.json).

UnitDiagnosticWorkflow.Convert materializes at most4096 rows before native work,
disposing enumerators on success, infinite input rejection and enumeration failure.
This bounds storage/call count, not execution time of caller code. Each row requires
finite values,1–256-byte ASCII units without NUL, an explicit dimension kind and
optional finite ordered target-unit limits. Existing serialized UnitsAPI values
perform conversion; all nine dimension exponents must match the requested dimension.
No working-unit configuration is set. Success publishes one read-only batch of
copied values and complete invariant Message_Msg diagnostics, with no borrowed SDK
text/owner. Failure publishes no batch, with a one-based row ordinal and explicit
InvalidInput, EnumerationFailure, DimensionMismatch, ValueOutOfRange or ConversionFailure.
Existing native row owners are disposed on success/failure. Caller mutation during
enumeration requires synchronization. Compound dimensions require separate admission.

## Consumer and Manual audit

Both clean consumers convert cm/m/mm to10/20/30 millimetres with explicit length
and numeric bounds, copy geometry through BT constraints, construct a valid six-face
box and write/read STEP with exact bounding-size checks. Input replacement and shape
disposal leave copied values/diagnostics intact. Tests cover culture, angle/time,
failure categories, lazy enumeration cleanup,4096-row boundary and40 concurrent calls.

All10 Foundation Manual rows have exact caller/source audits in
OcctSharp/artifacts/batch-bu/manual-caller-audit.json. RGB/RGBA, GUID and copied
attribute-type helpers are generator candidates. CB must prove document command,
ordering and epoch equivalence; CC/CF must preserve float rounding, borrowed GetRGB
lifetime, Viewer thread ownership and actual style pixels before reclassification.
These are authorized follow-up migration candidates, not permanent exclusions.

The nested-enumeration regression retained an initial failure: an enumerable's
inner UnitWorkflowException escaped with its inner ordinal/category. Enumeration
now wraps every caller exception with the outer row's EnumerationFailure; row
budget rejection occurs after successful enumerator disposal. The failed
reproduction remains in runtime-nested-enumeration-repro.log.

## Delivery

Generation, compile, runtime and final freeze are separate gates. Full Release/Debug/
cold, actual Debug62 DLL identities, ABI/lifetime, unchanged exports, additive API,
14 packages, two clean consumers and source/package freeze are required. Sole
authority: OcctSharp/artifacts/batch-bu/delivery-result.json PASS. Hosted CI, signing,
push, public publication, external CAD and full downstream acceptance: NOT RUN.
After BU freezes, BV–CI14 remain authorized.

Local evidence:

0 admitted / 0 deferred, 0 new prerequisites / 25 reused; generated +0 / Manual +0.
Release/Debug/cold Generator 326, Runtime 1363: PASS.
Actual Debug-native 1363, 62 actually observed DLL paths/hashes: PASS.
Exports 41461 (+0/-0), Debug parity; API 58925 (+51/-0).
All 318 baseline generated files unchanged; current generated files 319.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-bu/delivery-result.json PASS completes BU.

# AV Foundation copied values

State: IMPLEMENTED / LOCAL GATES PASS; delivered only after final freeze PASS. Delivered baseline Preview.45 / ABI 1.88 / bridge 0.96.0.
Development candidate Preview.46 / ABI 1.89 / bridge 0.97.0.
TM029 pins OCCT source revision b8f597c677811d1f9f4d8a97f5ae2825c0353a42.
Generation is not compile/runtime proof.

`OcctSharp.FoundationValues.SdkValues` provides private ASCII/Unicode operations,
pure OSD path component edits/projections, color parsing/space conversions, and
explicit units conversion with copied dimension exponents. Copied colors feed
the existing QuantityColor owner; converted lengths feed actual shape builders.
Receivers are initialized call-private values. Success publishes a registry
snapshot; managed code copies every result and releases it before returning.
No mutable SDK string/vector/dimension reference escapes.

Inputs have at most 1,048,576 UTF8 bytes; embedded NUL, invalid UTF8/UTF16 and
non-seven-bit ASCII are rejected. Unicode positions count UTF16 code units.
Substring, insert, remove, truncate and split reject a boundary inside a surrogate
pair. CodeUnit intentionally returns one UTF16 code unit as a scalar.
ToUTF8CString has LengthOfCString + 1 capacity, including the terminator.
ParseInteger requires a complete in-range signed integer; SDK predicate methods
preserve the SDK's permissive classification semantics. ParseReal requires
a complete finite input. Outputs have bounded storage and independent lifetimes.

OSD paths use explicit OSD_SysType ordinals (default 1). They expose pure syntax;
no filesystem lookup or environment expansion is admitted. Traits order:
absolute, relative, DOS, Unix, UNC, UNC extended, NT extended, remote protocol,
content protocol. Traits describe the original input. Color vectors retain SDK
`SdkIsValid` is a copied SDK predicate, not a gate for full path parsing: its DOS
implementation rejects the colon in ordinary drive-qualified paths. Parsing does
not certify filesystem validity. AV serializes the complete private unit validation
and conversion under one bridge mutex; per-method SDK locks do not span sentences.
conventions; Quantity_Color omits alpha. Finite out-of-gamut vectors are permitted
where SDK conversion supports them.

Units use initialized Units_UnitSentence, IsDone, Evaluate and dimensions;
expression structure is bounded to 16 nested levels, 32 factors and exponents
with magnitude at most 16 before SDK evaluation (alphabetic symbols, *, /, **).
unit-to-unit conversion requires dimension equality before calling UnitsAPI.
Valid zero is a result; unknown units and incompatible dimensions are errors.
No current-unit-system/dictionary mutation is exposed. SDK lazy dictionaries and
recursive mutexes retain their existing lifetime/locking contract.

Initial native link proved the header's static ASCII IsEqual(string,char*)
overload absent from the installed TKernel import library. It is deferred without
emulation; exact admission is now 341 primary, 141 admitted / 200 deferred,
3 new prerequisites / 18 reused; generated delta 144, Manual delta 0.

Focused validation passes 23 runtime / 4 generator tests. The reproduced native
unit concurrency regression additionally passes five fresh processes with 32
concurrent calls each; single-thread unit isolation also passes. Initial path and
native crash records are retained in batch-av artifacts.

All required local checks passed: full suites 267 Generator / 1048
Runtime in Release, Debug and cold source; actual Debug-native 1048; cold
bytes 232, unchanged old bytes 229; inventory/ABI/export/API/private headers;
two clean consumers and 14 packages. Final package/source freeze is authoritative.
Continuous authorization proceeds AW, AX and AY after AV freeze.

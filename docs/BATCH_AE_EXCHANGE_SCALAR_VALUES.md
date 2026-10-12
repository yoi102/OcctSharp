# Batch AE: exchange scalar values

Batch AE selects 45 exact OCCT 8.0.1 declarations on two owned DataExchange
classes. The baseline is Batch AD's 25,830 generated IDs in 155 files. The
target manifest has 25,875 IDs, with no removal; verify this against the
generated manifest and full inventory at delivery.

## IGES global section

`IGESData_GlobalSection` exposes its default constructor, fourteen scalar
setter/getter pairs and the maximum-coordinate flag/accumulation operations.
Although the header says that no default value is provided, the locked 8.0.1
constructor implementation initializes every selected scalar and flag. The
runtime test checks both this actual constructor behavior and setter/getter
round trips. String handles, `Interface_ParamSet`/`Interface_Check` outputs,
`char` delimiters and other unselected methods remain outside the contract.

## STEP field

`StepData_Field` exposes its empty constructor, scalar integer, Boolean and
real transitions, scalar accessors, kind/arity inspection and derived-state
transition. Its constructor calls `Clear()`, which initializes integer and
real storage. Arbitrary-kind `Clear`, list constructors and indexed setters
are excluded: those require a separate state and bounds contract. The indexed
getter signatures are selected only for scalar fields; the native scalar
branch does not index a list, and this batch offers no way to construct one.

Both classes use the existing ADR-0096 typed token registry, call-duration
leases and typed release. There are no manual generated-code edits or new
ownership exceptions.

## Local validation (2026-09-23)

- Manifest: 25,830 to 25,875 IDs, exactly the 45 selected IDs added, zero
  removals; 155 generated files. The refreshed inventory reports 25,875
  Emitted, 39,832 Blocked, 49,347 Skipped and 1,218 Manual among 116,272
  declarations, with zero pending and the same 32 header parse exclusions.
- Release/Debug native and managed builds passed; Generator 189/189 and Runtime
  596/596 passed per configuration. The Debug test-output native runtime
  matches all 62 actual Debug DLL hashes.
- Strict C11 compilation of 40 generated headers and owned-registry lifetime
  and concurrent release fixtures passed in Release and Debug. Relative to
  bundled Preview.28, native exports add 289 and remove zero; Release and
  Debug exports match. The 47 additional exports since AD comprise 45
  generated operations and two typed releases. Relative to the Batch AC API
  snapshot, public signatures add 313 and remove zero, 49 more additions
  than the AD source tree.
- Fourteen local `8.0.1-preview.28.batchae.1` development packages passed
  both facade and direct-module clean consumers, including the AE workflow.
  All 62 packaged native DLLs match the current Release build by SHA256.
  The source-tree sample smoke passed.
- A clean source copy rebuilt Release, passed Generator 189/189 and Runtime
  596/596 against its own newly built native bridge, and reproduced all 155
  generated files byte for byte.

The complete local release-check, hosted CI, signing, push and publication
remain NOT RUN for this development suffix. See [STATUS](STATUS.md).

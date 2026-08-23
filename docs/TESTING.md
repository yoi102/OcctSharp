# Testing and Validation

## Evidence levels

Validation status must be reported per layer. Passing one layer never implies that a
later layer passed.

| Layer | Proves | Does not prove |
|---|---|---|
| Parser/model unit tests | Rules interpret controlled declarations correctly | Generated OCCT code compiles |
| Determinism test | Same normalized input produces the same output | Native behavior is correct |
| Native compile test | Generated bridge compiles and links | Managed calls are correct |
| Managed compile test | Raw/friendly managed source compiles | Native library loads or behaves correctly |
| ABI contract test | Layout, exports, version, and errors match | OCCT semantic correctness |
| Runtime binding test | Selected calls execute against OCCT | Broad lifetime or workflow reliability |
| Lifetime test | Ownership paths survive stress and diagnostics | All modules are covered |
| Integration test | End-to-end workflows succeed | Other files/platforms are compatible |
| Real-file test | Specific CAD data behaves as expected | All CAD files are supported |
| Packaging test | Fresh consumer can restore and load dependencies | Every API is correct |

The initial package test restores only the newly built local `OcctSharp` package,
publishes a Windows x64 console consumer, verifies that all 36 native DLLs are below
the output `occt` directory, and runs runtime identity plus box creation. A local clean
consumer pass is not a public-release license, provenance, signing, or CI pass.

## Generator tests

Use small controlled C++ fixtures for parsing and generation behavior, including:

- Aliases, namespaces, overloads, default arguments, and nested declarations.
- Const/value/pointer/reference combinations.
- Inheritance, multiple inheritance, and virtual methods.
- Enums and platform-dependent primitive widths.
- Templates, `Handle<T>`, callbacks, arrays, and containers.
- Naming collisions and unsupported constructs.
- Stable skip reasons and source diagnostics.
- Stable sorting, line endings, and removal of stale generated files.
- Coverage/diagnostics state totals, package grouping, disposition codes, and two-run
  byte stability through isolated report staging.
- Full-library header inventory filtering and deterministic package grouping; semantic
  batch failure isolation must preserve declarations from unaffected headers.

At least one test set must parse real headers from the pinned OCCT baseline.

## Native and ABI tests

- Export and calling-convention verification.
- ABI version compatibility and mismatch diagnostics.
- Boolean, enum, struct, string, and array layout verification.
- Error conversion for OCCT, standard, and unknown exceptions.
- Invalid, null, disposed, and wrong-type handle behavior.
- Native dependency closure inspection.

## Runtime and lifetime tests

The first closed loop should cover point creation, box creation, topology traversal,
STEP write, STEP read, disposal, and repeated stress. XDE/STEPCAF tests are required
before claiming assembly, color, name, layer, or material preservation.

Use native diagnostics where available, including sanitizers, heap diagnostics, and
a debug handle registry. The owning shape bridge now rejects stale handles through its
live registry and tests repeated native release; this does not yet cover shared,
borrowed, or parent-bound semantics. Tests must distinguish owning, shared, borrowed,
and parent-bound paths.

The shared lifetime probe must verify that cloning retains one OCCT object, reference
counts change predictably, null handles remain valid values, and the last wrapper release
destroys the native object without exposing its pointer or counter layout.
It must also verify exact runtime type names, valid base-kind relationships, rejection
of unknown type names, and checked cast behavior. Successful derived casts must retain
one additional reference; null and wrong dynamic kinds must return `false` from the
managed `TryCast` path without producing a wrapper, while the throwing cast must report
`InvalidCastException`.

Each generated typed shared wrapper additionally requires construction, all emitted
member paths, clone/reference-count, shared mutation visibility, either-wrapper-first
disposal, access-after-dispose, RTTI, and generated error-contract tests.

Each generated topology value wrapper additionally requires null/type/orientation,
copy independence, partner/same/equal distinctions, reversal/location behavior,
either-wrapper-first disposal, access-after-dispose, and invalid-handle tests.

Each typed topology cast additionally requires successful `ShapeType` validation,
wrong-kind `TryCast` and throwing-cast paths, identity/equality preservation, and
source-disposal independence. Every configured subtype export must compile even when
the current fixture set cannot construct that subtype directly.

## Real CAD fixtures

Every committed fixture must have:

- Proven redistribution rights.
- Source and purpose.
- Stable checksum.
- Expected semantic assertions.
- Size classification and Git/LFS decision.
- A note if it previously reproduced a defect.

## Reporting vocabulary

Only use:

- `PASS` — the named check ran and passed.
- `FAIL` — it ran and failed.
- `NOT RUN` — it did not run.
- `BLOCKED` — it could not run because a named prerequisite is unavailable.
- `UNSUPPORTED` — the feature is deliberately not supported.

Every handoff and `STATUS.md` update must name the exact commands or CI jobs behind a
PASS claim once implementation exists.

# Testing and Validation

## Batch S authoring validation

The unchanged 40-row matrix maps to BatchSAuthoringTests, BatchSClosureTests and
BatchSConversionTests. `dotnet test tests/OcctSharp.Runtime.Tests -c Release --no-build
--filter FullyQualifiedName~BatchS` runs all 44 cases, including 48 low-sampling,
high-iteration G2 solves with owning history. Repeated full focused runs are required
after the intermittent OCCT residual-getter overwrite fix; an earlier pass never
overrides a later crash. Use the freshly built bridge, not a stale bundled copy.
Shared BatchSGuidedWorkflow runs real BinXCAF Unicode/STEP/IGES/real-HWND acceptance
both in repository tests and the clean facade package consumer. Optional evidence
directory `OCCTSHARP_BATCH_S_EVIDENCE` retains three screenshots.

After both configurations build, `eng/verify-debug-native-runtime.ps1` creates an
isolated consumer with all 62 actual Debug DLLs and verifies their hashes before the
full runtime suite. `eng/verify-native-exports.ps1` checks exact additive baseline
counts and Debug/Release equality. `eng/test-batch-manual-accounting.ps1` verifies
wrong-hash, overwrite and unimplemented-transition rejection without mutating inputs.
These checks complement the existing strict headers, source-layout negatives,
exact-ID inventory, clean regeneration, both package consumers and full release-check.

## Batch O final validation

`BatchOCompletionTests` exercises all five curve families, reversed trim, similarity
transforms, negative conic projection parameters, copied arrays, shuffled open/mixed
chains, overlap and self-intersection, near-boundary hole nesting, numeric area/bounds,
explicit gap tolerance, offset, hole-aware faces, extrusion/revolution/add-cut, lifetime,
STEP/IGES colors and layers, and actual HWND selection/screenshots.
Run it with `dotnet test tests/OcctSharp.Runtime.Tests --configuration Release --filter
FullyQualifiedName~BatchOCompletionTests` from the inner workspace. The clean facade
consumer repeats the end-to-end planar-feature/exchange/viewer workflow. Release/Debug,
clean regeneration, inventory, package isolation, and compatibility remain required;
STATUS records the checks actually completed. NuGet publication is not a batch step.

Repository Debug builds normally consume the committed Release runtime, matching sample
deployment. To test the native Debug build itself after building it, run
`dotnet test tests/OcctSharp.Runtime.Tests --configuration Debug --no-restore
-p:OcctSharpNativeRuntimeDir=<absolute-workspace>/artifacts/native/Debug`.
This additional sweep exposed KI-030; its corrected suite passes 164/164.

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

The package test restores only the newly built local `OcctSharp` package,
publishes a Windows x64 console consumer, verifies that all 62 current native DLLs are below
the output `occt` directory, and runs runtime identity, modeling, and exchange calls. A local clean
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
- Full-inventory final classification must assign every discovered stable ID and every
  catalogued header a disposition, report zero pending/HD099 entries, and produce a
  byte-identical report in two runs with the same batch size and normalized inputs.

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

When generated shared members reference native enums, tests must also cover every enum
family's numeric definition and at least one managed-to-native-to-managed round-trip.
Inventory evidence must reconcile generated manifest IDs as `Emitted`; leaving those IDs
as `SupportedUnselected` is a reporting failure even when runtime tests pass.

Each generated topology value wrapper additionally requires null/type/orientation,
copy independence, partner/same/equal distinctions, reversal/location behavior,
either-wrapper-first disposal, access-after-dispose, and invalid-handle tests.

Each typed topology cast additionally requires successful `ShapeType` validation,
wrong-kind `TryCast` and throwing-cast paths, identity/equality preservation, and
source-disposal independence. Every configured subtype export must compile even when
the current fixture set cannot construct that subtype directly.

Each BRep adaptor snapshot requires fixed-layout assertions on both sides of the ABI,
representative analytic geometry values, exact topology-kind rejection, finite-bound
handling, source-disposal behavior, and proof that the copied snapshot remains usable
without a native parent or release call.

Each modeling-result operation requires algorithm completion and null-result checks,
null/disposed input failures, result independence after both inputs are disposed, and
representative geometry assertions. Value-copy extrema results additionally require
fixed-layout, distance, point ordering, and solution-count checks.

An owning-result/no-history profile additionally requires an explicit history exclusion,
result use after all inputs are disposed, null and disposed input failures, native
completion/null-result diagnostics, and a package consumer that executes every included
algorithm family. It does not imply coverage of modes or generated/modified/deleted maps.

A geometry-only provider profile additionally requires explicit provider configuration,
non-empty files for every supported writer, non-null/non-empty topology for every
supported reader, source/result lifetime independence, documented unsupported directions,
and clean-package verification of the expanded native toolkit closure.

An OCAF document profile additionally requires commit and abort behavior, mutation
rejection outside commands, save rejection during commands, UTF-8 attribute copies,
stable-entry lookup, parent-disposal failures, empty-label abort semantics, binary
persistence round-trip, and clean-package verification of persistence drivers.

An XDE profile additionally requires parent disposal, same-document label guards,
shape and location ownership, assembly occurrence/referred-part checks, copied name/
color/layer/material records, and the same metadata assertions in memory, BinXCAF, and
STEPCAF round-trips. Color tests must account for Gen/Surf/Curv channel normalization.

A visualization-core profile additionally requires a real HWND, OpenGL view creation,
shape display independent of source disposal, hide/show/remove, resize/fit/redraw,
creating-thread enforcement, mouse detection/selection, copied selection IDs, and child
invalidation. Package validation must load TKOpenGl from `occt`; compiling an interactive
sample alone is not runtime evidence that the user closed or visually inspected it.

A production viewport/model-review profile additionally requires copied XDE occurrence
identity, exact owning detected and selected topology, point/rectangle/polygon selection
schemes, built-in filter replacement/reset, reversible isolate, selection bounds and fit,
per-subshape style/reset, complete camera snapshot/restore validation, screen/world and
pick-ray numerics, window zoom, parent-bound clip-plane lifecycle, hidden-line/trihedron
state, and a non-empty screenshot with path/failure behavior. One real STEP/XDE assembly
must execute the complete workflow on a real HWND in Release and Debug and from a clean
package consumer. Interactive sample compilation or manual visual inspection alone is
not this evidence.

Alpha.55 satisfies this profile as one complete Batch D run. `BatchDCompletionTests`
passes inside the 115/115 Release and Debug runtime suites and uses a real STEP/XDE file
plus real HWND. The clean alpha.55 package consumer repeats the complete review-to-
screenshot workflow with the application-local 62-DLL closure. Both paths cover copied
identity and owning topology after source disposal, parent/viewer/thread rejection,
selection/filter/isolate/style reset, camera and coordinate numerics, clip-plane/review
state, and non-empty Unicode-path screenshot output. The full local release check also
passes Generator 91/91, dependency profiles 6/6, freshness, byte-identical clean
regeneration, inventory, API compatibility, runtime hashes, SBOM/provenance/checksums,
and Git whitespace validation.

An engineering-inspection/PMI profile additionally requires all 24 ADR-0066 capabilities
to run together: exact extrema solutions and topology supports, interference/contact
classification, length/area/volume/centroid/inertia and angle/radius/diameter numerics,
unit behavior, semantic dimension/tolerance/datum reference graphs, transactional
mutation and rollback, AP242 GDT plus saved-view import/export, viewer-owned annotation
lifecycle, and a durable screenshot. The same real AP242-to-inspection-to-annotation-to-
screenshot workflow must pass in Release and Debug and from a clean package consumer.
Preview.2 satisfies this profile as one complete Batch E run. Four focused completion
tests cover exact measurement, transactions and persistence, complete PMI snapshots and
reference graphs, invalid/cross-document/disposal guards, saved views, four viewer-owned
dimension kinds, a real HWND, and screenshot output. The full Release and Debug suites
pass Generator 91/91 and Runtime 119/119; the clean 62-DLL package consumer repeats the
AP242/BinXCAF/annotation workflow, and the complete local release check passes.

A freeform-authoring profile additionally requires all 24 ADR-0067 capabilities to run
together: rational Bezier/B-spline curve/surface definitions, copied arrays and immutable
edits, interpolation/approximation numerics, projection/extrema/intersection solutions,
planar profile construction/offset, surface trim/rule/fill/offset, owning split groups,
controlled loft/pipe shell, freeform analysis/repair, STEP/XDE definition/topology
retention, and real-HWND selection/measurement/mesh/screenshot evidence. Invalid array
shape, non-finite values, knot/multiplicity/degree mismatch, non-positive weights,
wrong-kind/disposed topology, algorithm failure, and source-disposal paths are mandatory.
The same design-to-STEP/XDE-to-viewer workflow passes in Release and Debug and from a
clean package consumer. Batch F is 24/24: four focused tests and full Runtime 123/123
cover copied definitions, immutable edits, algorithms, ownership/failure behavior,
STEP/XDE, mesh/measurement, real-HWND face selection, and screenshot output.

A repository-native bootstrap change additionally requires a recoverable missing-bridge
simulation: remove or rename only the expected configuration's bridge, run the ordinary
Sample build, verify native-only CMake recreation and the current 62-DLL output `occt/` closure,
then execute a non-UI OCCT Sample operation. The test must also prove that an
unconfigured clone receives actionable SDK/archive instructions. This is separate from
NuGet clean-consumer validation because package consumers carry native assets already.

A common-modeling result profile additionally requires cone/torus, extrusion/revolution,
all-edge and single-edge fillet/chamfer, offset, section, bounding-box fixed layout and
numerics, full-topology validity, public subshape counts, invalid/null/disposed/wrong-kind
paths, and source/result lifetime independence in both Release and Debug. Package
validation must load `TKFillet` and `TKOffset` from application-local `occt/`.

An interactive assembly-placement profile additionally requires all 24 ADR-0075
capabilities to execute together: presentation transform round-trip/reset; one
viewer-parent-bound manipulator; translation, rotation, scaling, plane, axis, activation,
position, size, gap, skin, and zoom configuration; copied state; custom and real-view
mouse transforms; apply/cancel; thread, parent, presentation-removal, viewer-disposal,
and repeated-disposal guards; rigid XDE occurrence preview/commit with a replacement
label; named history and undo/redo; DMU recheck; STEP/XDE round-trip; and a non-empty
real-HWND screenshot. Managed and native layout tests must both hold the copied state at
144 bytes. OCCT 8.0.1's unsafe generic fit with an attached flat-skin manipulator must
fail deterministically; fitting after detach must pass. The same workflow must pass in Release and Debug and from the clean shared
62-DLL package consumer. Preview.11 satisfies this profile as one complete Batch M run.

A metadata-aware IGES/XDE interoperability profile additionally requires all 24
ADR-0077 capabilities to execute together: explicit/extension format routing; IGESCAF
read/import/write; all transferable roots; names, generic/surface/curve colors, layers,
and visibility; copied source/root/transfer diagnostics and units; non-ASCII input/output
with failure cleanup; mixed STEP/IGES composition; destination-parent-bound labels;
source/session disposal; independent writer modes; geometry and metadata round-trip;
XDE-label WPF display; real HWND; and clean package consumption. Preview.13 satisfies
this profile through focused 4/4 and full Release/Debug Runtime 156/156.

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

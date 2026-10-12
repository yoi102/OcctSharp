# Status history through 2026-10-01

This is the status snapshot preserved before the 2026-10-01 progress review.
Its incremental package versions, counts and checks belong to their stated
checkpoints. Use [STATUS](STATUS.md) for current facts and
[ROADMAP](ROADMAP.md) for active priorities.

- Last updated: 2026-10-01.
- Cadoryx D2 prerequisite locally validated: additive copied self-interference
  report via BOPAlgo_ArgumentAnalyzer on a deep private geometry copy.
  Release native/managed builds passed; 4,117 facade forwarders regenerated.
  Ten focused self-interference/mirror semantics and lifetime tests passed.
  Build and test-output bridge SHA256 both equal
  `A3B60FFA1C7E55F6200F61894B601796AADAF2681D98802D209D27974C1560AD`.
  Evidence: `OcctSharp/artifacts/cadoryx-d2-self-interference-tests/d2-self-interference.trx`.
  Full Release Runtime passed 609/609. Against immutable mirror.2, Release
  export names have exactly one addition and no removals (40,872 total).
  Fourteen new immutable `8.0.1-preview.28.cadoryx.modeling.sweep.1` local
  packages and isolated facade/direct-module consumers passed; both consumer
  outputs match all 62 built DLLs. Cadoryx locked consumption passed 41/41
  and independently matched all 62 DLLs against NuGet payloads.
  Evidence: `OcctSharp/artifacts/cadoryx-d2-package-hashes.json`,
  `OcctSharp/artifacts/cadoryx-d2-release-export-delta.json`, and Cadoryx
  `artifacts/occt-capability-20261001-204643`.
  Debug native, complete Generator/Debug suites and full delivery gates
  are NOT RUN for this prerequisite. Existing immutable mirror.2
  packages and their Cadoryx D1 evidence remain unchanged.
- Cadoryx C3 STEP correction: styled mirrored compounds previously reopened
  with duplicate solids/components from presentation recovery. Geometry
  transfer now excludes recovery-only results; fallback styles attach only to
  unique exact partners in existing part topology and create no extra roots.
  Release native rebuild has zero observed warnings/errors. Nineteen focused
  mirror/style/STEP/XDE/assembly/IGES regressions pass against the built bridge;
  effective color coverage and 48 mm3 instance volume survive roundtrip.
  Fourteen local `8.0.1-preview.28.cadoryx.modeling.mirror.2` packages and clean
  facade/direct-module consumers passed. Cadoryx's independent locked consumer
  passed 36/36 scenarios and matched all 62 native DLLs against NuGet; evidence
  is `artifacts/occt-capability-20261001-030720` in Cadoryx. The loaded test
  bridge matches the build SHA-256
  `4C96DEFAD7A3DE6845D7C7F4799B4216C523D28B282CB030DB55418279ABAD76`.
  Full upstream Release/Debug delivery gates and publication are NOT RUN.
- Cadoryx C3 plane mirror prerequisite: `GpTrsf.CreatePlaneMirror` composes
  `gp_Trsf::SetMirror(gp_Ax2)` through one additive manual C export. The existing
  exact `Shape.Transformed` adapter applies reflection with determinant -1.
  Release native build and focused runtime/error/lifetime tests passed 4/4;
  the loaded test bridge matches the built bridge SHA-256
  `1C0AED0B1EDC40AFF01E469B69CE4DA0D7953FE0CA1DD6E9E5500ADBC8B22A39`.
  Debug native, complete upstream suites and public release are NOT RUN for
  this prerequisite. Fourteen local packages were built as
  `8.0.1-preview.28.cadoryx.modeling.mirror.1`; clean facade and direct-module
  consumers passed with 62 shared native DLLs. Cadoryx's independent locked
  package probe passed 35/35 scenarios with all 62 native DLLs matching NuGet.
  Evidence: `OcctSharp/artifacts/cadoryx-c3-mirror` and Cadoryx
  `artifacts/occt-capability-20261001-022523`. No public release was performed.
- Cadoryx occurrence gizmo support: viewer-owned manipulators are placed in
  Topmost on attachment, and `SetPart` now recomputes both presentation and
  active selections so hidden modes actually disappear. The local package
  family `8.0.1-preview.28.cadoryx.handles.gizmo.2` contains 14 packages and
  62 native DLLs. Focused real-viewer runtime tests passed 4/4 using the
  built bridge DLL with matching SHA-256; Cadoryx's independent locked NuGet
  probe passed 34/34 scenarios and matched all 62 packaged native DLLs.
  Cadoryx Release 621/621 tests and desktop/window smokes passed. These are
  local validation results; no public package release was performed.
- Cadoryx M10 located-occurrence topology: `RepairSnapshot.FindTopologyPartnerIndex`
  and its native `occtsharp_repair_find_topology_partner_index` export match a
  Viewer-picked shape to the original BRep by `TopoDS_Shape::IsPartner` only
  when the source map has exactly one matching TShape. Foreign and ambiguous
  repeated partners return -1; the caller must still check source asset,
  revision, fingerprint and instance path. Release native/managed build passed
  with zero warnings/errors; the focused native test passed 2/2 against the
  matching bridge hash. Fourteen paired local packages were built as
  `8.0.1-preview.28.cadoryx.assembly.partner.1`; clean facade and direct
  module consumers passed with all 62 native DLLs. This is not a public release.
- ViewCube appearance extension: the viewer-owned cube now accepts managed
  `ViewerCubeAppearance` labels (UTF-8), font, font height and idle transparency,
  plus a hovered state. The C ABI copies values into the existing AIS cube;
  labels update without replacing the visible presentation. The decorative
  axes are hidden; face/edge/corner and roll-arrow hit paths remain available.
  Release native and full managed builds passed with zero warnings/errors; the
  focused real-Viewer runtime test passed 1/1 against the built native bridge.
  Fourteen local packages were built as
  `8.0.1-preview.28.cadoryx.topology.viewcube.1`; Cadoryx's locked consumer
  passed 563/563 tests, published desktop/window smokes and an independent
  33/33-scenario, 62/62-native-DLL package probe. Debug-native and public
  release checks for this style increment are NOT RUN.
- Cadoryx exact-local-topology integration: the manual
  `occtsharp_repair_find_topology_index` export and
  `RepairSnapshot.FindTopologyIndex` map a Viewer-picked native edge or face
  against the original shape's `TopExp::MapShapes` order. This avoids comparing
  it with a deep-copied repair snapshot. The zero-based result is only valid for
  the exact source asset and revision; Cadoryx freezes on upstream changes and
  requires explicit reselection. See [SC-065](SPECIAL_CASES.md).
  Release/Debug native and managed builds and the focused runtime test (1/1
  in each configuration) passed against their actual bridges; the Debug test
  bridge SHA-256 matches the Debug build. Against the packaged Preview.28
  baseline, this development tree has 40,866 Release/Debug-identical native
  export names, 297 additions including this export and zero removals; other
  additions come from concurrent work. Report:
  `OcctSharp/artifacts/topology-native-export-compatibility.json`. The local
  `8.0.1-preview.28.cadoryx.topology.1` package bridge SHA-256 matches the
  Release build. Cadoryx's independent NuGet consumer passed 20/20 scenarios
  with all 62 native DLLs matching the package; its locked Release build,
  403/403 tests and published desktop/window/recovery smokes passed. Complete
  upstream release-check, hosted CI, signing and public publication are NOT RUN.
- Cadoryx ViewCube extension: the viewer-owned `AIS_ViewCube` and two curved
  45-degree arrow presentations support highlighted face/edge/corner and arrow
  hover, 26 orientation hits, left/right roll actions, and principal-face-only
  arrow visibility. The current local
  `8.0.1-preview.28.cadoryx.viewcube.5` managed/native package family also
  computes cube and standard-view animation targets on a detached camera,
  without changing or drawing the live view first. The target preserves the
  current center and viewing distance. `OcctViewer.GetRolledCamera` likewise
  computes a roll without display mutation; Cadoryx controls the step (default
  45 degrees) and 320 ms animation. Release/Debug native builds and their
  focused real-Viewer runtime tests passed, including 26 cube directions,
  seven projections, unchanged live camera, and roll equivalence. Cadoryx's
  independent package consumer passed 19 scenarios with all 62 native DLLs
  matching the actual package, and the Debug test bridge matched its build.
  Cadoryx's locked Release build, 372 tests, and published desktop/window/recovery
  smokes passed on the `.5` pair; the desktop smoke keeps first-animation-frame
  camera invariants. The physical display buffer was not captured before a
  repaint, and mixed-DPI/RDP visual confirmation remains open.
  The complete upstream release suite, hosted CI, signing and publication are
  NOT RUN.
- Cadoryx viewport integration: the native Viewer bridge now exposes OCCT's
  vertical two-color background gradient through `OcctViewer.SetBackgroundGradient`.
  Release native/managed builds and the focused real-Viewer runtime test pass.
  Fourteen local `8.0.1-preview.28.cadoryx.gradient.1` packages were built;
  Cadoryx's independent package consumer passed 17 scenarios and verified all
  62 packaged native DLLs. The local package includes the ongoing Batch AE
  source state. Debug-native verification, the complete upstream test/release
  gates, hosted CI, signing and publication have not been run for this suffix.
- Active migration: [Batch AE](BATCH_AE_EXCHANGE_SCALAR_VALUES.md) selects
  45 exact DataExchange declarations on two owned scalar classes. The generated
  manifest contains 25,875 IDs in 155 files. Relative to the Batch AD full
  inventory, the added set equals the 45 contract IDs exactly and no prior
  emitted ID was removed. Release/Debug native and managed builds, Generator
  189/189 and Runtime 596/596 per configuration passed. The Debug test bridge
  matches all 62 actual Debug DLL hashes. Release/Debug strict C11 and owned
  registry lifetime/concurrency fixtures passed. Versus bundled Preview.28,
  native exports add 289 and remove zero, with Debug parity. Versus the Batch
  AC public API snapshot, 313 signatures are additive with zero removals.
  The refreshed full inventory classifies 116,272 declarations: 25,875
  Emitted, 1,218 Manual, 39,832 Blocked and 49,347 Skipped, with 32 header
  parse exclusions and zero pending. Fourteen local
  `8.0.1-preview.28.batchae.1` packages and both clean consumers pass; all
  62 packaged native DLLs match the Release build by SHA256. Source-tree
  sample smoke passes. A clean source copy rebuilt Release, passed Generator
  189/189 and Runtime 596/596, and reproduced all 155 generated files byte
  for byte. The complete
  local release-check, hosted CI, signing, GitHub push and publication are
  NOT RUN for this development suffix.
- Previous migration: [Batch AD](BATCH_AD_OWNED_DATA_WORKFLOWS.md) adds 203 exact
  generated declarations on 14 owned data classes across Foundation, Geometry,
  Modeling, DataExchange and Visualization. The generated manifest now contains
  25,830 IDs in 155 files, with the 203 selected IDs added and no previous ID removed.
  The current source tree includes the separate Boolean history and SC-061 knot
  buffer changes below. Release/Debug native and managed builds, Generator 188/188
  and Runtime 592/592 per configuration, and Batch AD's 14 focused runtime tests
  passed. The Debug test-output native DLL matches its actual Debug build by SHA256.
  The facade has 4,114
  current type forwarders. Relative to the Batch AC API snapshot there are 264
  additive public signatures and zero removals; relative to bundled Preview.28
  there are 242 additive native exports (240 generated plus the two earlier
  development exports), zero removals and Release/Debug export parity. Release
  and Debug strict C11/owned-registry ABI fixtures passed. Fourteen local
  `8.0.1-preview.28.batchad.1` development packages and both clean consumers,
  including the AD workflow, passed; all 62 packaged native DLLs match the
  Release build by SHA256. A clean source copy rebuilt Release, passed
  Generator 188/188 and Runtime 592/592 against its own newly built bridge,
  and reproduced all 155 generated files byte for byte. The full
  local release-check, hosted CI, signing, GitHub push and publication are
  NOT RUN for this development suffix.
  The fresh full inventory classified 116,272 unique declarations across 7,058
  parsed headers, with 32 documented header exclusions: 25,830 Emitted, 1,218
  Manual, 39,877 Blocked and 49,347 Skipped, with no pending classification.
- Development migration: SC-061 now provides a bounded, copied
  `BSplCLib::FlatBezierKnots` buffer through Geometry and the facade. The exact stable
  ID moved from Pending to Manual; current coverage is 61,570 Pending, 32,008 Skipped,
  21,467 Supported and 1,218 Manual out of 116,263 parsed declarations. At that
  checkpoint, the generated 25,627 IDs and 155 files were unchanged. Release/Debug
  native and managed builds,
  Generator 177/177 and Runtime 578/578 per configuration, focused buffer/error tests,
  deterministic generation, C11 header fixture, facade forwarders, dependency profiles and native export
  comparison passed. Both test-output bridge hashes match their actual builds. Against
  the Batch AC API snapshot, that source tree had 10 additive signatures and zero removals
  (including the earlier Boolean companion). Against bundled Preview.28, it has exactly
  two additive native exports (Boolean history and this knot copy), no removals, and
  Release/Debug parity. The Preview.28 bundle does not contain these development
  exports. Clean source rebuild/regeneration, repack, isolated NuGet consumers, the
  complete release-check, hosted CI, signing and publication: NOT RUN for this migration.
- Instruction maintenance: AGENTS and WORKFLOW now route reading by task, distinguish
  historical checklists, avoid extra duplicate verification and pause only work dependent
  on a required scope decision. See the [ADR-0091 clarification](adr/0091-proportionate-development-workflow.md#clarification-2026-09-17).
  Documentation QA passed (205 documents, 497 local links and paired fences), as did
  whitespace checks for this change. Product builds/tests/repacking: NOT RUN for this
  documentation-only maintenance; existing package evidence remains tied to its baseline.
- Development follow-up: [Cadoryx Boolean topology history](BOOLEAN_TOPOLOGY_HISTORY.md), Release/Debug native and affected managed builds, 6/6 focused tests per actual native configuration (including multi-argument history), C11/exports and 62-DLL identity checks passed. The 2026-09-23 focused rerun confirmed test-output bridge hashes match the local Release/Debug builds; the default bundled Preview.28 runtime lacks this new export and must not be used for these source-tree tests. Fourteen local .2 packages, native payload/lock hashes, an isolated direct-module NuGet consumer and Cadoryx 306 tests passed before this test-only addition. Cadoryx owns the final desktop evidence in its ROADMAP; full upstream release gates are NOT RUN. The following Batch AC delivery describes the unchanged Preview.28 baseline.
- Baseline work: [Batch AC](BATCH_AC_OWNED_UTILITY_EXPANSION.md). Implementation and
  Release/Debug tests pass; local delivery checks are complete. Public-release
  readiness remains false because hosted CI, signing and publication are not run,
  and 32 SDK headers remain explicit Clang parse exclusions.
- Last fully packaged local candidate: 8.0.1-preview.28 / ABI 1.72 / bridge 0.80.0.
- Entry: c55a969, completed Preview.27;
  [its evidence is archived](STATUS_HISTORY_THROUGH_PREVIEW_27.md).
- Twelve modules, one facade, one Native DLL and fourteen packages remain the product layout.
- GitHub push, hosted CI, signing and NuGet publication: NOT RUN, outside this task.

## Implemented migration

AC added 1,924 exact generated IDs and 225 owned classes across 53 OCCT packages and
nine modules. AD adds 203 exact IDs across 14 owned classes and five modules;
AE adds 45 exact IDs on two DataExchange owned classes. The generated manifest
currently contains 25,875 IDs and 155 files.
The current development coverage manifest classifies 116,263 parsed declarations:
61,570 Pending, 32,008 Skipped, 21,467 Supported and 1,218 accepted Manual IDs. It
emits 25,875 IDs
across 155 files. The full inventory scanned 7,058/7,090 headers; the remaining 32
are explicit parser exclusions. This is an accounting snapshot, not a claim that all
OCCT APIs are migrated.

The new catalogue covers analytic spline conversion/projection, surface construction,
intersection and interval workflows, HLR, wire ordering, initialized numeric/color/date
values, 88 STEP SELECT containers and independent exchange/mesh/material settings.
Registry lifetime and copied-reference publication reuse ADR-0096. New const receivers
resolve const overloads; a frozen owner epoch preserves AB bodies. Supporting inherited
operations, typed SELECT setters, factories and releases are counted separately from
migrated declarations. Full-library migration is incomplete.

## Batch AC executed checks

- PASS: Release/Debug native and managed build/link; Generator 177/177; Runtime 568/568,
  including 26 new AC tests.
- PASS: deterministic generation and complete, acyclic module dependency closure.
- PASS: 36,408 previous generated native functions and header declarations preserved,
  including 122 old owned release functions; no changed bodies/prototypes or removals.
- PASS: 3,633 managed API additions and zero removals, for 55,076 signatures and
  4,097 facade type forwarders. Native export audit: 40,569 exports, with 3,408 generated
  C ABI additions and eight exact MSVC constructor closures; zero removals and Debug parity.
- PASS: final Release/Debug C11 checks for 40 headers, geometric ABI fixtures and registry
  fixtures with 250 release races / 16,000 acquisitions per configuration.
- PASS: actual Debug-native Runtime 568/568 in an isolated directory; all 62 DLL hashes checked.
- PASS: positive and 11 rejection accounting fixtures using an isolated synthetic inventory.
- PASS: bundled 62-DLL runtime matches the current Release build.
- PASS: clean source rebuild and full suites, with all 155 generated files byte-identical.
- PASS: fourteen Preview.28 packages and both clean facade/direct-module consumers.
- PASS: full release-check final inventory/classification, release metadata, checksums
  and provenance; 32 parser-limited headers are explicitly excluded.
- PASS: final package-content verification (203 stable documents, 73 runtime/license
  files, five checksums and final provenance).

The Batch AC Preview.28 Release bridge is 21,829,632 bytes with SHA256
`FBB48038700D99DDDA301CF68DD35A1C5560DDA2DD4E2DED0AAE4BA62F538A98`.
These checks mark the AC local engineering delivery complete. They do not make this a
full OCCT migration or a public release.

## Remaining migration

Borrowed adaptors, retained external buffers, general sequences, other dimensioned
array-start references and mutable owning outputs remain future work. Four declared SDK
methods lack both Release and Debug
symbols and remain Blocked; see the batch record. Follow [WORKFLOW](WORKFLOW.md)
for impact-based verification and useful batch sizing without a fixed capability count.

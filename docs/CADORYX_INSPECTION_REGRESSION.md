# Cadoryx imported inspection and C0 finite-curve regression

This implementation correction is independent of Preview.34 / Batch AJ's frozen
delivery. Cadoryx remains on the Preview.31 suffixed package until a fresh immutable
package family and its consumers pass. No new generated scope, ABI entry, public
API, topology identity rule or ownership rule is introduced.

## Reproductions and implementation

- MiniPC3.step: 88,258,425 bytes, SHA256
  `f3773a7f37bd0cb8e717e4be6f1070a72e5c6e7b2f36f26f9501183df8c4c68e`.
  XDE read/save, topology and bounds complete before volume inspection crashes.
- The extracted original BRep is 27,170 bytes, SHA256
  `475c57dc10c8603522c3523abb2bcf373f80aa6214d8b1d4c6cc116b03cb604c`.
  It was saved before inspection; geometry was not repaired or approximated.
- The fault RIP resolves to `Geom2d_BSplineCurve::Knot`; the first native return
  address candidate resolves to `ShapeUpgrade_SplitCurve2dContinuity::Compute`.
  Other scanned stack pointers are candidate addresses, not a formally unwound
  symbol stack. The SDK source removes knots while retaining old iteration bounds.
- Inspection now supplies exact CN adaptor intervals to private curve/surface
  split tools. It never invokes the continuity knot-removal tools. Temporary
  topology stays call-local; original fingerprint and topology remain unchanged.
- Volume now uses vector quadrature directly on ORIGINAL exact surfaces and
  trimming pcurves when conic/rational geometry or continuity breaks require it.
  Topological partitioning is retained for area only; rebuilding temporary
  pcurves within BRep tolerances can change volume. Reversed pcurve parameters,
  all original knot spans, V-knot and finite-domain crossings are explicit.
  Spatial normalization uses the SDK's vertex-mean reference point: residual
  flux in tolerance-closed imports makes different reference points disagree.
  Ten moments share evaluations, with propagated uncertainty. The work cap is
  per smooth boundary interval, not accumulated over detailed geometry. At the
  depth limit, local error is retained; final global mass/moment checks still
  reject unmet accuracy. Source geometry and fingerprint remain unchanged.
- Sketch signed area now uses adaptive interior 8/16-point Gauss on every native
  knot interval. Endpoint Simpson derivatives were not reliable at C0 knots:
  a one-ULP normalized nudge may still map onto the exact native knot.

## Verification at 2026-10-03

- First candidate upstream Release native build: PASS; focused 19/19, 868 ms reported
  suite duration; complete Runtime 778/778, 36 s.
- First candidate actual Debug native: focused 19/19 and complete Runtime
  778/778 PASS; all 62 copied DLL hashes match the Debug output.
- Tests include independent GProp mass, centroid, diagonal inertia and every
  imported face's area; the C0 prism also compares positional polygon quadrature,
  both directions, extrusion volume and unchanged source fingerprint.
- The new source snapshot starts from the hash-bound suffixed Preview.31 baseline;
  only the inspection/area implementations and regression test fixture/project
  inputs change. Generated baseline remains Batch AG. Native compilation is running.
- Immutable `.3` packages: 14 packages, 62 native and 13 managed payloads;
  isolated Release/Debug 674/674, native exports 41,075 (+0/-0), package consumer
  52/52 PASS. Cadoryx periodic 46/46 and 24 production samples also pass.
  Application full regression exposed two late-result timeouts while GK occupied
  the modeling queue. That run was deliberately terminated (211 PASS / 2 FAIL,
  213 completed of 1424); the runner's generic "host crashed" message is not a
  spontaneous access violation. The live dump is retained. `.3` is not accepted
  as the final application dependency.
- `.4` narrowed GK selection. Upstream Release/actual Debug each passed 778/778,
  isolated Release/Debug 674/674 and NuGet consumer 52/52. Application regression
  nonetheless aborted after 1232 PASS/0 FAIL of 1424 when Blame killed its host
  after 60 seconds without progress. Its dump retains GK inertia-integration
  address candidates; it is not spontaneous access-violation evidence. The
  large-model default-color diagnostic hit its five-minute deadline: original
  part 1546 spent 130 seconds in properties, then part 1547 remained in properties.
  Both failed runs are retained; `.4` is not application acceptance.
- The added original part-1547 fixture has SHA256
  `0ce6c5ed2ceec126a4cb73b69377179130fb51c8761ec75ebd17d7c0dfd74558`.
  Its default fixed-order SDK GProp mass is inaccurate (0.15995295160360305).
  An independent SDK-only executable on the ORIGINAL geometry gives adaptive
  Gauss mass 0.18952421132937064 at 1e-12 and GK mass-only
  0.18952421132969796, with all centroid/inertia values retained. Production
  vector quadrature agrees with that independent reference and takes about
  36 ms in the focused Release run; this is a small-solid regression measurement,
  not overall MiniPC3 performance acceptance.
- Revised vector quadrature upstream Release Runtime: 779/779 PASS, 23 s.
  Initial overly strict per-span budgets failed six focused checks; their logs
  remain. Actual Debug and immutable `.5` packaging/consumer checks are in progress.
  Cadoryx production Open is **NOT RUN**. Large-model performance, screen input and external CAD acceptance
  remain separate gates. No signing, installation, push or publication occurred.

Logs and TRX files are under `OcctSharp/artifacts/cadoryx-inspection-*`; isolated
source is `OcctSharp/artifacts/cadoryx-inspection/isolated-source-1`.

## Follow-up original geometry and full-model verification

- Immutable `.5`: 14 packages, NuGet consumer 53/53, isolated Release/actual
  Debug each 675/675, actual test native parity 62/62, Cadoryx 1425/1425.
  Its all-color MiniPC3 diagnostic failed at original definition 43 because
  normal accumulated work exceeded a per-shape cap. A per-face experiment
  passed 17 focused checks but then failed at definition 1978. Both remain.
- Original many-face fixture: SHA256
  `08de662b2b91349e09e93c0aad998743b6100828169d5ad56160d7b5a50b7f58`.
  Independent SDK adaptive Gauss 1e-12 mass is 5277.5345493580207; GK mass-only
  agrees within 1.2e-10. All centroid/inertia values are retained.
- Original dense-trim fixture: 580,413 bytes, SHA256
  `f58c54e9a1052685595dd0d19b0869f95ddabbac0c0d279f8e92279d3317c46e`.
  One trimming boundary has 264 exact spans. Ordinary SDK adaptive GProp/GK
  does not converge all moments. Independent SDK-only fixed Legendre
  4096 outer / 256 inner nodes with long-double accumulation gives mass
  223.84908567032869. Doubling outer 2048 to 4096 changes mass by 3.2e-8;
  doubling inner 128 to 256 at outer 2048 changes mass by 7e-11. The complete
  centroid and nine inertia entries are regression references. Production's
  1e-9 mass is 223.84908566797932; neither tool changes original geometry.
- Original-boundary implementation upstream Release: 781/781. Actual Debug
  780/780 passed before adding the dense-trim reference test; its next focused
  and isolated candidate checks remain pending. One earlier focused run used
  the bundled older DLL after a build overwrote its copied native file;
  that failed/aborted run is retained and is not current-native validation.
- Development-only full MiniPC3 all-color read/check: PASS 1185 simple
  definitions and 1347 assembly definitions, 51.08 seconds under concurrent
  diagnostic load. This is NOT immutable package acceptance or production
  Open/performance acceptance. Final finite-domain crossing candidate recheck
  and fresh `.6` isolated builds/packages/consumers are in progress.
- Same `.5` WPF periodic gate: 24 samples, 48 screenshots and 24 current
  revision/isolation records inspected; no bindings, zero assets. The earlier
  repeated/overlaid screenshots stay as a visual-evidence gap, now covered by
  the new capture. Physical mouse/IME, hardware performance and external CAD
  remain separate pending gates. G installation/signing/delivery is deferred.

## Immutable `.6` local integration checkpoint

1225 frozen input files retain Batch AG generated output byte-for-byte. Both
native configurations build; same-version isolated Release and actual Debug
Runtime each pass 677/677. The test binaries use all 62 matching native DLLs;
Release also matches all 13 packaged managed DLLs. Fourteen fresh packages,
41,075 exports (+0/-0, Release/Debug parity), 61 unchanged SDK payload DLLs,
NuGet-only 55/55 and Cadoryx 1427/1427 pass. WPF publish and actual application
test/consumer bins each match 62 native plus 13 managed packaged DLLs. The final
finite-domain crossing development diagnostic also passes 1185 simple/1347
assembly definitions, 52.35 seconds under diagnostic load. Same-version WPF
19/19 gates pass; all 48 periodic screenshots and their 24 source records have
metadata and visual review. Two production Open runs retain 4964 instances and
1185 geometry assets, pass fixed PNG checks and release assets on close. Full
scene arrival is 69.9530/71.3474 seconds. Direct loading is 56.6951 seconds;
first batch submission is 1.2083 seconds and full submission 9.1202 seconds,
which still misses the 6.482-second submission target. Native per-instance
Display remains the measured bottleneck. Physical input, controlled-cache
performance and external CAD acceptance remain open.

The previous 4975/1196 expected counts included 11 erroneous style-restoration
free roots. The unchanged PCB tree has 2532 definitions, 1185 bodies and 4964
instances, with identical source entries, hierarchy, poses, visibility and
BRep hashes. An independent SDK-only helper proves every extra root is an
original PCB subshape partner (TopoDS_Shape::IsPartner), at a different
location (no IsSame partner). This is topological identity evidence, not a
geometric similarity match; PMI and all face styles are outside that tree
hash audit. Cadoryx retains the initial failed count assertion and the new
audits under artifacts/a3-model-identity-context-1,
artifacts/a3-model-product-tree-1.json and artifacts/a3-inspection-verification-4.json.
The frozen package documents reflect its preparation checkpoint; later root
status updates do not replace any frozen source or package.

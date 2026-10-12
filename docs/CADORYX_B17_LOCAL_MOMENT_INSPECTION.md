# Cadoryx B17: local evaluation of exact support surfaces

2026-10-06. The application's previous freeze locks immutable private
`8.0.1-preview.31.cadoryx.curve-inspection.10`. Three independent application
probes each execute 111 cases and retain the same four failures: round-offset
rectangles at million-unit XY positions, rotated .73 radians, author scales
.01/1, both boundary directions. `InspectAdaptiveProperties(Volume, 1e-10)`
exhausts its exact integration budget. Translating and reflecting the actual
BReps does not repair the failure; their support geometry still carries large
centers plus locations. All twelve original/translated/reflected BReps are
retained with hashes. This is not a topology reconstruction or polygonal fixture.

The change remains within SC-067. For each face, the original support and its
location are read without changing the BRep. The world reference point is
converted once to support coordinates. A call-local support copy is translated
by the negative reference point before D1 evaluation. The original location's
linear part is then applied to the local point and both derivatives; their
cross product and the original reversed-face flag determine the normal.
Only translation is applied to the support, preserving its u/v domain even
when the original location includes reflection or scale. Pcurves, knot spans,
finite-domain clamps, orientation, all ten divergence moments and the final
global error test remain unchanged. The source is read-only; no repair,
rebuilding, fitting or triangulation is introduced.

The tolerance, two-million evaluation budget per smooth interval, depth 20 and
existing imported-regression deadlines remain unchanged. The numerical
convergence estimate remains separate from geometry tolerance.

Current-root Release builds pass with zero warnings/errors. Focused runtime
passes 38/38, including the twelve actual BRep variants, eight independent
large-center circular-extrusion moment cases with located/tilted/reflected
transforms, and eighteen existing adaptive/imported cases. Current-root full
Release passes 837/837. These runs use the newly built actual Release bridge;
they are initial algorithm evidence, not private .11 package or Cadoryx
acceptance evidence. The current root also includes existing Preview.34/AJ
changes that are absent from Cadoryx's .10 packages.

The private .11 candidate is therefore prepared independently from the retained
.10 isolated source, with exactly sixteen file differences: one inspection
implementation, package identity, one test-copy declaration, one 20-case test
source and twelve byte-identical BRep fixtures. Generated code, generator, XDE
and ABI additions are not copied from the current root. See
`OcctSharp/artifacts/cadoryx-b17/private-source-1.json` and the broader source
comparison in `source-comparison-1.json`.

Private .10-based Release builds pass with zero warnings/errors; full Runtime
733/733 passes, containing all 713 baseline names and exactly twenty new
localized-moment cases. The thirteen public managed assemblies and sixty-two
actual native DLLs are bound; the managed native-package carrier is accounted
for separately. Only the bridge differs from the application's .10 native
closure. Source layout passes 94 units/642 manual exports, and the 41,081 public
export names match .10 exactly. Evidence is
`OcctSharp/artifacts/cadoryx-b17/private-release-verification-2.json`; the first
verification fixture's carrier-count assertion is retained with its explanation.
Actual Debug Runtime also passes 733/733 with all sixty-two actual Debug DLLs,
including debug TBB. Generator passes 205/205, standalone headers45/45, exports
match across configurations and the isolated solution's locked restore and
Release build pass with zero warnings/errors. These gates are bound in
`OcctSharp/artifacts/cadoryx-b17/private-gates-1.json`.

Fourteen immutable .11 packages now match the actual Release build, dependencies,
license files and packaged documentation. A fresh NuGet-only public consumer
passes20/20, including all twelve retained original/translated/reflected BReps
and eight independent circular-prism transforms, with unchanged1e-10 integration.
All thirteen managed and sixty-two loaded native DLLs match the actual build;
its fourteen-package isolated cache/lock graph contains no ProjectReference.
Initial NU1008 from inherited central package settings is retained; local
consumer settings isolate it without changing the SDK or oracles. Evidence:
`package-audit-1.json`, `package-consumer-verification-1.json`, `feed-copy-1.json`
under the same artifact directory. Packages have been copied to the local feed,
without replacing .10. The packaged document truthfully records consumer and
application checks as pending at packaging time; later evidence is recorded here.
Cadoryx has now locked .11 and frozen a sixth application/test candidate with
unchanged production source, five updated solution locks and zero build
warnings/errors. The same128 focused cases and all111 native original-world
BRep oracles pass, including all four old failures, at unchanged1e-10 and work
budgets. No external translation/reflection substitutes for the actual original
inspection input; source fingerprints/topology remain unchanged. Application
evidence is `artifacts/b17-native-verification-4.json`. The sixth app freeze full run completed
2927 PASS/1 FAIL/0 skip, retaining the20s late-insertion fixture failure;
its actual native queue owner remains unproven. Test-only isolation and
guaranteed artificial release preserve20s and all assertions. Seventh tests
pass142/142 plus three rounds of both close cases; production payload remains
byte-identical. New36 editor cases/18 actual-file workflows, reviewed36 images/
18 vector PDFs and six related windows pass. Seventh frozen full regression completes2928/2928 PASS,
zero failed/skipped, with the same fifth/sixth case set and all2848 prior B16
cases plus all142 focused cases. Actual .11 candidate/source/native payloads,
111 original-world BReps,36 editor cases/18 workflows and reviewed screenshots/
PDFs plus six related windows are verified. B17 analytical-join local contract
is accepted; physical input/freeform offsets/A5 performance/full A-F remain
open. Immutable package documents retain their packaging-time state. Packaged docs remain the immutable packaging-time snapshot.
The application fifth freeze has 2928 full tests, 36 editor windows and 18
workflow consumers with reviewed screenshots/PDFs, but still retains its four
independent native failures. B17 and the complete A–F goal remain unaccepted;
G installation/device/delivery is deferred by the user. AJ/AK admission and
public release status are unchanged. No package publication is authorized.

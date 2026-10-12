# Release notes: 8.0.1-preview.29

Preview.29 consolidates the already implemented AD/AE migration and subsequent
Cadoryx prerequisites into one local engineering candidate. It adds no new
migration scope during this delivery-validation task.

## Included changes

- Audited owning data operations across Foundation, Geometry, Modeling,
  DataExchange and Visualization, including generated index preconditions.
- IGES global-section and STEP scalar-field owning wrappers.
- Copied Boolean topology history and bounded B-spline knot buffers.
- Exact topology and unique-partner lookup for selected/located occurrences.
- Viewer gradient, ViewCube camera/appearance and manipulator corrections.
- Plane mirror, styled STEP/XDE compound roundtrip correction and copied
  self-interference diagnostics.

## Identity and compatibility

- Package: 8.0.1-preview.29; native ABI: 1.73; bridge implementation: 0.81.0.
- OCCT 8.0.1, Windows x64, .NET 10; managed assembly/file versions remain 0.1.0.0.
- Twelve modules and the facade share one native bridge and one 62-DLL runtime
  package; fourteen packages retain the existing dependency layout.
- Existing immutable Preview.28 development packages keep their original contents.
- Full-library migration remains incomplete. Parser exclusions, borrowed data,
  retained external buffers and unaudited sequence/pointer projections remain explicit.

## Validation and distribution

Local engineering validation is complete: Release/Debug Generator and Runtime suites,
actual isolated Debug-native execution, clean byte-identical generation, ABI/lifetime
fixtures, additive API/export comparisons, both clean consumers and the unified release
check pass. An isolated copy of the downstream capability probe passes against the
candidate. Full OCCT migration, downstream application delivery and public release
remain separate claims.

[STATUS](STATUS.md) records the current candidate's completed checks and open gates.
The reproducible local entry point is `eng/release-check.ps1`, supplemented by actual
Debug-native, ABI/lifetime/export and final package-content verification as documented
in [BUILD_AND_RELEASE](BUILD_AND_RELEASE.md). Hosted CI, signing, GitHub push and
public NuGet publication are separate gates and are outside this local delivery task.

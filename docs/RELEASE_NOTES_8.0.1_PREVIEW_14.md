# OcctSharp 8.0.1-preview.14 release notes

Preview.14 adds Batch O's complete 2D sketch to planar-feature API. The package uses
OCCT 8.0.1, native ABI 1.58, bridge 0.66.0, schema 1.13, and managed assembly identity
0.1.0.0. It retains twelve managed modules, the compatibility facade, and one shared
62-DLL native runtime package.

## Added workflow

- Immutable copied 2D points, vectors, directions, sketch planes, and analytic/freeform
  curve definitions: segment, circle/arc, ellipse/arc, rational Bezier, and B-spline.
- Point/derivative evaluation, sorted projection and intersection results, trim/split,
  reverse, translation, rotation, uniform scaling, and mirroring. Negative periodic
  conic solution parameters are mapped back into the original definition domain.
- Ordered mixed open/closed chains, duplicate/gap/self-intersection diagnostics,
  coincident-span boundary solutions, adaptive signed-area integration, native length
  and conservative bounds, exact hole containment, and ambiguous nesting rejection.
- Owning edge/wire/face creation, hole-aware extrusion and revolution, offset, additive
  and subtractive features, named/colored/layered XDE, STEP/IGES, and real HWND review.

The root README contains a complete plate-with-hole extrusion and STEP example.

## Contracts and limits

Definitions copy input arrays; edits return new definitions. Native geometry and
algorithm objects remain call-local. Edge, wire, face, and feature results are
independent disposable owners. Wire construction changes tolerances on copied topology.
XDE labels and viewer objects retain their existing parent/thread ownership.

Derivatives use the native parameter even when evaluation input is normalized.
Interpolate explicitly creates a degree-one, piecewise-linear B-spline; callers can
supply higher-degree rational definitions. Projections return OCCT's perpendicular
solutions on the bounded curve; a coincident intersection span returns its endpoints.
Bounds include topology tolerance. IGES may map named layers to numeric assignments on
transferred sublabels. Parametric constraints and a feature-history solver are not included.

## Validation and distribution

The native Debug sweep also exposed and corrected an inherited feature-history bug:
OCCT history accepts vertex/edge/face/solid inputs, so wire/shell/compound containers
must not be queried directly. Container workflows now use supported descendants, with
a dedicated regression spanning feature Boolean, basic wire history, and freeform split.

The seven Batch O regression tests cover all curve families, copied values, parameter
mapping, transforms, mixed chains, exact nesting, numeric measurements, tolerance copy
isolation, self-intersection, features, actual STEP/IGES colors/layers, and real HWND
selection/screenshots. The clean facade consumer runs the sketch-to-feature-to-exchange
workflow; the direct Modeling consumer verifies facade-free use of the same runtime.
SC-052 reconciles 52 exact directly invoked declarations without promoting whole families.

Release and Debug build all 19 projects with zero code warnings/errors. Generator
91/91 and Runtime 164/164 pass; the runtime suite also passes against the actual native
Debug bridge. A clean source copy builds, passes both suites, and reproduces all 94
generated files byte-for-byte. Its temporary build directory produces MSVC's advisory
MSB8029 warning, not a compile or runtime failure. Dependency profiles pass 6/6.

The complete inventory classifies 116,272 declarations and 7,090 headers: 16,353
generated, 609 accepted manual, 49,344 skipped, 49,966 narrowly blocked, and zero pending
or supported-unselected declarations. API comparison with alpha.38 is additive at
39,046 additions and zero removals. The committed native bridge is 15,395,840 bytes,
SHA256 `0E4CA204356B83C158A40B74D99CA6047D59D9FB975A62C1168FF2A650979D90`.

Package-isolation, final package hashes, and release-metadata results are recorded in
repository STATUS and the local artifacts/release gate report. Preview.14 is for local
package verification only; no NuGet publication, signing, or GitHub push is performed.

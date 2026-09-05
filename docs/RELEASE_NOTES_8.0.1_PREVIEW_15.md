# OcctSharp 8.0.1-preview.15

Local-only Batch P preview. NuGet publication and GitHub push are not part of delivery.
Native ABI 1.59, bridge 0.67.0, OCCT 8.0.1, schema 1.13; assembly identity stays 0.1.0.0.

## Surface and UV workflows

The broadened wave contains 32 capabilities: copied descriptors/evaluation/curvature,
bounded complete and batched projection, trimmed-domain grids, iso edges, copied and
derived pcurves, smooth interpolation/approximation, offsets, 3D lifting/sampling,
seams and continuous tracing, independent topology repair, wires/loops/holes, trimmed
faces and splits, analytic faces, surface sections and curve intersection intervals,
XDE/STEP/IGES metadata, real-HWND review and isolated package consumption.

Three native translation units replace further growth of the legacy source monolith.
The closed 12-module managed graph, facade and single Native DLL are retained; a new
cross-DLL ownership protocol is not introduced. SC-053 reconciles 100 exact direct calls,
including native-local analytic constructors, location/vector helpers and measurements.

## Historical Native source organization

ADR-0081 subsequently extracts the complete 13,510-line historical implementation into
39 domain-owned source files and 33 private headers. Including the existing surface
files, 42 manual translation units compile independently without PCH or unity builds.
Runtime registry/error storage each has one owner, and the public ABI and generated
sources remain unchanged. The same-baseline API has zero additions/removals; this
architecture work adds no product capabilities and does not increment Preview.15.
See [the complete responsibility map](NATIVE_SOURCE_LAYOUT.md) and repository STATUS
for the final refactor validation, which is separate from the original Batch P results.
The completed extraction passes Generator 91/91 and Runtime 180/180 in Release/Debug,
including an isolated actual Debug-native run. All 34 private headers compile alone,
29,402 native exports are unchanged, and fresh-source regeneration and local release
checks pass. No public NuGet release is made by this workstream.

## Contracts and limitations

- Topology outputs are independently owning; copied DTOs retain no native parents.
- Repair/wire/trim/split copy topology and curve representations before modification.
  External-support pcurves are explicitly retained; supporting surfaces are read-only.
- Holes use native classification and real interval clipping. Degenerate boundaries and
  singular UV charts are explicit. Reversed normals/curvatures retain a documented sign.
- UV offset and fitting tolerances are in UV units. Derivation residual is measured at
  65 world-space samples, not a certified global error bound. Conic-to-B-spline conversion
  may preserve geometry without preserving the original parameterization.
- No constraint solver, global UV atlas, geodesics, cross-platform renderer, D3DImage,
  physical Native DLL split, package signing or publication is included.

## Validation

Release and Debug solution builds pass with zero warnings/errors. Generator tests
pass 91/91, Runtime tests pass 177/177, and the focused surface suite passes 13/13.
An isolated sweep using the actual Debug native runtime also passes 177/177. Fresh
source regeneration rebuilds successfully with all 94 generated files byte-identical.
Its native build reports only the known MSB8029 temporary-directory advisory.

The shared public-only surface workflow runs in repository tests and the clean NuGet
consumer, including real STEP/IGES names/colors/layers, selection and screenshots.
All 14 packages use one 62-DLL runtime; direct Modeling consumption stays facade-free.
Final gate counts, hashes and completion status are maintained in repository
`docs/STATUS.md`, which is deliberately excluded from package documentation to avoid
package-hash self-reference.

OcctSharp code is MIT. Bundled OCCT remains LGPL-2.1 with its exception (or a separately
obtained commercial license); other native dependencies retain their own licenses.

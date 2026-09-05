# OcctSharp 8.0.1-preview.18

Local-only Batch S preview; complete validation status is tracked in STATUS.
No NuGet publication or GitHub push. ABI 1.62, bridge 0.70.0, OCCT 8.0.1,
schema 1.13 and unchanged assembly/file identity 0.1.0.0.

## Guided authoring

The original [40-capability closure](BATCH_S_GUIDED_SWEEP_CONSTRAINED_SURFACE_GAP_INVENTORY.md)
adds copied constant/linear/interpolated/B-spline/smooth/composite scalar laws with
mapping, trimming, derivatives and sampling; fixed/binormal/discrete/support/auxiliary
sweeps with contact, scaling, attached sections, simulation, history and solidification;
compatible lofts and endpoint provenance; per-edge G0/G1/G2, support/interior/UV/seeded
filling with independent fulfilment reports; boundary patches, B-spline assembly,
Bezier decomposition, span extraction and join residuals; XDE recipe publication and
existing-viewer preview/result review.

Geometry owns laws, Modeling owns plans/results and the facade composes conversion,
documents and review. Eight independent Native units add nine C exports; 68 exact
manual IDs are listed in SC-056. No new managed project, Native DLL or registry.
Inputs are copied as one dependency graph; result/history shapes remain owning.
Source TShape.Free is restored after temporary compound assembly. Default XDE Save
uses the Unicode-aware BinXCAF path, including non-ASCII filenames.

## Important limits

- Sampling is not proof of global extrema or constraint error bounds. Positive
  scalar control hulls supply conservative sweep admissibility.
- Auxiliary guides and scale laws conflict; contact is C0-limited. Border contact
  requires one nondegenerate planar section. Simulation stations are equally spaced.
- Required residuals must be available and within tolerance on the final surface;
  IsDone is not acceptance. OCCT 8.0.1 per-index G*Error getters are not called due
  to unsafe temporary-array sizing; those three declarations remain Blocked.
- Zero-speed/singular derivatives are nullable. Coons boundaries are copied and
  exactly degree-elevated. Loft endpoint records do not invent missing end caps.
- Recipes persist in BinXCAF. STEP/IGES preserve tested geometry/names/colors, not
  arbitrary OCAF recipes. Viewer use is HWND/thread-bound; headless rendering,
  D3DImage and other platforms are not claimed.

## Validation

Preparation entry, focused 44/44 and ten complete repeats pass after the retained
heap-failure evidence. Tests include low-sampling/high-iteration G2 lifetime loops,
punctual lofts and foreign-review atomicity. Release/Debug Generator 91/91 and Runtime
273/273 pass, as does the isolated actual Debug-native 273/273 sweep. All 39 private
headers, six layout negatives, additive Native/managed comparison, both clean package
consumers and a fresh-source build with 94 byte-identical generated files pass.
The existing R affinity fixture now uses a distinct Thread to avoid Task inlining.
Full local release-check and exact 68-ID inventory accounting pass with zero other
changes. Final inventory has 16,353 Emitted, 931 Manual, 49,644 Blocked and 49,344
Skipped declarations, with zero pending. Final package-content/provenance evidence
and the local completion commit are tracked in STATUS.

OcctSharp code is MIT. Bundled OCCT remains LGPL-2.1 with the Open CASCADE exception
(or a separately obtained commercial license); dependencies retain their own terms.
Applicable native notices/license texts are included unchanged.

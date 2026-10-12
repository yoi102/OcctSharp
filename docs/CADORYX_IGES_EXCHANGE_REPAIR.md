# Cadoryx IGES export repair

2026-10-03. This private export correction is separate from Batch AJ/AK and public
delivery. It introduces no public ABI, generated type or ownership-category change.

The XDE IGESCAF writer silently omitted finite side faces whose underlying surface
was a linear extrusion of an unbounded line. A Cadoryx manufacturing sample lost
eight faces and 312 mm2: 18 to 10 faces, 1436 to 1124 mm2. Independent FreeCAD and
Cadoryx reads both reproduced the loss.

`CreateIgesExportCopy` clones the XDE roots into a private document. A nonmutable
`BRepTools_Modifier` converts only surfaces whose exact adaptor type is
`SurfaceOfExtrusion` and whose basis curve is `Line`. It verifies validity, face
count and area before changing the private definition. Subshape metadata moves
only through unique `ModifiedShape` correspondence. The author's source document
and application topology identity remain untouched. Assemblies are updated in
the private document before IGESCAF transfer. Raw `ShapeExchange.WriteIges` is not
changed by this correction.

The intermediate immutable `.7` candidate converted every extrusion basis.
Although its native suite passed, Cadoryx full regression exposed four circular/
arc seam failures. Its packages and failed evidence are retained. The final `.8`
candidate preserves circular and other bounded basis representations and fixes
the failing cases. No different payload was packed under an existing version.

Final local evidence for `8.0.1-preview.31.cadoryx.curve-inspection.8`:

- Isolated source6 derives from source5, which derives from frozen `.6`; only the
  private helper and its test file changed from source5. Generated outputs were
  not patched and AJ/AK admission did not change.
- 12 focused runtime cases and 689/689 full Release cases pass; actual Debug
  native with its own SDK dependencies passes 689/689. All 124 test/native DLL
  hashes match their corresponding configuration.
- 41,075 public exports match the verified `.6` package in Release and Debug.
  Fourteen immutable packages were copied to the local feed; Cadoryx locked
  consumption passes 55/55 with all 62 native DLLs matching NuGet.
- Cadoryx Release has zero warnings/errors, 87/87 focused and 1722/1722 complete
  tests. Four frozen-publish WPF gates pass.
- Actual FreeCAD 1.1.3 / OCCT 7.8.1 reads 30/30 Cadoryx outputs. Its STEP/IGES
  returns are reread by Cadoryx in 20/20 cases. The manufacturing IGES now retains
  18 faces and 1436 mm2.

Cadoryx evidence lives in its `artifacts/f4-exchange-verification-1.json` and
`artifacts/f4-external-roundtrip-3/`. The separate Part-only external return does
not prove names, sharing, per-face color assignment or PMI preservation. Bounds
and mass properties do not prove full geometry equivalence or topology transfer.
No public release, signing, installer or remote publication was performed.

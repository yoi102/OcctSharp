# CD fixed-SDK STEP field fidelity

These are independently read back fixture values under OCCT commit
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`, not arbitrary PMI/PBR coverage.
The fixture exports a10×20×30 solid, Unicode part/layer, RGBA colors, physical
material and PBR record, semantic linear dimension, flatness tolerance and datum.
AP242 retains type/nominal value, tolerance value, datum identification and
geometric associations; other fields may be lost or changed as shown below.
Each profile checks actual FILE_SCHEMA and volume6000 after file/source-owner
deletion. Native-only configured export additionally verifies volume24 and four
surface-color fields. Native rewrite means a full STEPCAF native-document import
and second export; it cannot restore attributes already lost on initial import.

`supported-exact` means the initialized fixture field equals its independently
read value; `supported-numeric-tolerance` means absolute difference below1e-6,
including nested numeric color/point values. `lost-or-changed` records observed
absence/difference and is never reported as full fidelity. Null/empty defaults and
unassigned semantic fields are excluded. No cross-file label equality is inferred.

| Field | AP203 | AP214 | AP242 | AP242 native rewrite |
|---|---|---|---|---|
| Part.Name | supported-exact | supported-exact | supported-exact | supported-exact |
| Part.Layers | supported-exact | supported-exact | supported-exact | supported-exact |
| Color.Red | supported-numeric-tolerance | supported-numeric-tolerance | supported-numeric-tolerance | supported-numeric-tolerance |
| Color.Green | supported-numeric-tolerance | supported-numeric-tolerance | supported-numeric-tolerance | supported-numeric-tolerance |
| Color.Blue | supported-numeric-tolerance | supported-numeric-tolerance | supported-numeric-tolerance | supported-numeric-tolerance |
| Color.Alpha | supported-exact | supported-exact | supported-exact | supported-exact |
| PhysicalMaterial.Name | supported-exact | supported-exact | supported-exact | lost-or-changed |
| PhysicalMaterial.Description | supported-exact | supported-exact | supported-exact | lost-or-changed |
| PhysicalMaterial.Density | supported-exact | supported-exact | supported-exact | supported-exact |
| PhysicalMaterial.DensityName | supported-exact | supported-exact | supported-exact | supported-exact |
| PhysicalMaterial.DensityType | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.Name | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.BaseColor | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.Metallic | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.Roughness | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.EmissiveFactor | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.RefractionIndex | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.AlphaMode | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| VisualMaterial.AlphaCutoff | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Dimension.Type | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Dimension.Values | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Dimension.LeftDecimalPlaces | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Dimension.RightDecimalPlaces | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Dimension.FirstPoint | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Dimension.SecondPoint | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Dimension.TextPosition | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Dimension.SemanticName | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Dimension.Description | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Dimension.DescriptionName | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Tolerance.Type | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Tolerance.TypeOfValue | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Tolerance.Value | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Tolerance.SemanticName | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Datum.Name | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Datum.Description | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Datum.Identification | lost-or-changed | lost-or-changed | supported-exact | supported-exact |
| Datum.SemanticName | lost-or-changed | lost-or-changed | lost-or-changed | lost-or-changed |
| Datum.Position | lost-or-changed | lost-or-changed | supported-exact | supported-exact |

Visual PBR records are absent on these STEP readbacks, even though base RGB may
survive as a presentation color. Physical density unit/type naming and some names
change. AP203/AP214 do not retain this semantic-PMI fixture. AP242 loses decimal
places, text position and several semantic names; datum identification survives.
These are deliberate published limits, not silently reconstructed data.

The fixed SDK's duplicate cleaner makes this valid AP242 semantic-PMI fixture
fail serialization with IFSelect_RetFail(2). Explicit cleanup propagates
FileIoError through OcctException and preserves the old destination/removes the
staging file. New WriteConfigured defaults exclude cleanup; requesting it is never
silently downgraded. Existing Write keeps its historical mode behavior.

External CAD interoperability, arbitrary semantic annotations/PBR textures and
complete downstream acceptance: NOT RUN. See [CD](BATCH_CD_STEP.md).

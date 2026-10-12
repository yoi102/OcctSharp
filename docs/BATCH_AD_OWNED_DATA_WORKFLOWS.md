# Batch AD: Owned data workflows

- Scope: 203 exact owned-operation declarations on 14 new owners across five modules.
- Base: Preview.28 generated manifest, 25,627 IDs. The current source tree also
  contains the separately validated Boolean-history and SC-061 knot-copy extensions.
- Contract: [ADR-0096](adr/0096-generated-owned-nontransient-values.md),
  [OWNERSHIP](OWNERSHIP.md), [TYPE_MAPPING](TYPE_MAPPING.md) and [WORKFLOW](WORKFLOW.md).
- Current validation and delivery state: [STATUS](STATUS.md).

## Selected families

| Module | Owners | Declarations |
| --- | --- | ---: |
| Foundation | `Message_ExecStatus`, `OSD_Protection` | 30 |
| Geometry | `HatchGen_Domain`, `HatchGen_PointOnElement`, `HatchGen_PointOnHatching` | 27 |
| Modeling | `IntPolyh_StartPoint`, `IntTools_CommonPrt`, `IntTools_Curve`, `IntTools_Root`, `Intf_SectionPoint` | 91 |
| DataExchange | `IGESConvGeom_GeomBuilder`, `IGESData_DirChecker` | 37 |
| Visualization | `SelectBasics_PickResult`, `SelectMgr_ViewClipRange` | 18 |
| Total | 14 owners | 203 |

Every owner uses the existing typed, non-reused registry tokens, call-duration
leases and matching SafeHandle release. Endpoint and nearest-pick copies have
independent storage; IGES result handles retain their entities after builder
disposal. The exact stable-ID and signature catalogue is in
`OcctSharp/src/OcctSharp.Generator/Transformation/owned-value-operation-contracts.json`.
Supporting inherited operations and releases are separate from the 203 migrated IDs.

## Explicit exclusions

- `IGESData_DirChecker::IsSet` remains excluded because constructors leave its
  `isitset` flag uninitialized. Initialized setters and `Correct` are selected.
- Default `IntTools_Curve` can contain null curves. Its `D0` and `Type` queries
  remain excluded; `Bounds` and `HasBounds` handle the null case explicitly.
- Arbitrary `Message_Status` bit operations and `StatusByIndex` remain excluded
  because invalid shifts or signed overflow are possible.
- Mutable owner/handle references, external arrays, diagnostic streams,
  unselected sequence views and template vector accessors remain excluded.
- Hatch constructors requiring `IntRes2d_IntersectionPoint` and inherited
  `TopAbs_State` projections remain excluded to preserve module direction.
- Three indexed point operations use the exact native range guards in
  [SC-064](SPECIAL_CASES.md#sc-064-owned-point-index-preconditions).

The full OCCT library is not migrated by this selection. Delivery checks and
remaining release gates are recorded in [STATUS](STATUS.md).

## Local validation (2026-09-23)

- The generated manifest grew from 25,627 to 25,830 exact IDs, with the 203
  selected declarations added and no prior ID removed; all 155 files regenerated.
- Release and Debug native/managed builds, Generator 188/188 and Runtime 592/592
  per configuration passed. The Debug test bridge SHA256 matches the Debug build.
  The 14 AD runtime tests exercise copied storage, disposal, invalid indices,
  default-null results, output preservation, flags and picking.
- Strict C11 compilation of 40 generated headers and owned-registry lifetime and
  concurrent release fixtures passed in Release and Debug. Native exports versus
  Preview.28 added 242 and removed zero, with Debug parity; two additions belong
  to the earlier Boolean and knot development work. The Batch AC managed API
  snapshot has 264 additions and zero removals in this combined source tree.
- Full inventory classified 116,272 declarations and 7,090 headers, including
  32 explicit parse exclusions. It reports 25,830 Emitted, 1,218 Manual,
  39,877 Blocked and 49,347 Skipped, with no pending classification. The selected
  generator coverage model has 116,263 declarations, a distinct input set.
- Fourteen local `8.0.1-preview.28.batchad.1` development packages were built.
  Facade and direct-module clean consumers ran the shared AD workflow. All 62
  native package DLLs match the Release build by SHA256. This suffix does not
  replace the previously validated Preview.28 release candidate.
- A clean source copy rebuilt Release with its own native bridge, passed
  Generator 188/188 and Runtime 592/592, and reproduced all 155 generated
  files byte for byte. The build script now selects that newly built bridge
  for its tests; a bundled Preview.28 DLL would lack these development exports.

Remaining release gates are recorded in [STATUS](STATUS.md).

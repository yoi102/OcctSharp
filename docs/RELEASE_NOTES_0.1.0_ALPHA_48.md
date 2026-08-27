# OcctSharp 0.1.0-alpha.48

This local experimental package expands common IGES entity coverage inside the single
migration batch B. It is not a public-release-readiness declaration.

## Identity

- Package: `OcctSharp.0.1.0-alpha.48.nupkg`.
- Target: .NET 10, Windows x64.
- OCCT baseline: 8.0.1.
- Native ABI: 1.40.
- Bridge implementation: 0.48.0.

## Additions

- Generated `IGESAppli`, `IGESBasic`, `IGESDefs`, `IGESDimen`, `IGESDraw`,
  `IGESGeom`, `IGESGraph`, and `IGESSolid` entity families.
- 162 public wrappers and 984 additional emitted declaration IDs.
- Runtime construction/clone/RTTI/retention/disposal coverage for all 156 public
  default-constructible wrappers, plus complete public family-count assertions.
- Focused IGES dimension tolerance scalar/boolean state validation.
- Session, selector, reader, and conversion infrastructure remains outside this entity
  ownership wave.

## Coverage boundary

- Selected scope: 34,573 declarations.
- Emitted: 4,060 (11.7433%).
- Emitted plus 61 accepted manual declarations: 4,121 (11.9197%).
- Full observed inventory: 116,214 declarations from 7,058 semantically parsed headers
  out of 7,090 catalogued headers; `Emitted` 4,060, `Manual` 61,
  `SupportedUnselected` 11,144, `Skipped` 27,310, and `Blocked` 73,639.
- Inventory SHA256: `D46B10BFF1A5246721A19E19DA13A26E55E27242F8F95E0EFC7A2C7555A43963`.
- Batch B remains in progress; broad supported-unselected and blocked surfaces remain.

## Validation

Complete local release evidence and refreshed full-inventory classification are recorded
in `STATUS.md`; the final run used SDK 10.0.400 from the inner workspace and had no SDK
11 preview warning. Public publication, signing, hosted CI execution, project licensing, and
final third-party legal review remain outside this local validation.

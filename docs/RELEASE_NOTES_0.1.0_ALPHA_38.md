# OcctSharp 0.1.0-alpha.38 Local Release Notes

This is a locally validated prerelease candidate for .NET 10 and Windows x64. It has not
been signed or published.

## Included profiles

- Generated foundation: 58 emitted declarations from the selected 3,062-declaration scope.
- Safe topology, transforms, strings/collections, geometry values, properties, BRep
  construction/traversal/modeling/healing, mesh snapshots, and geometry exchange.
- OCAF documents and XDE metadata/assembly workflows.
- Windows HWND/OpenGL/V3d/AIS visualization core with copied selection IDs.
- Interactive console samples for creation, STEP/STL/IGES, XDE STEP assembly, and Viewer.

Native ABI is 1.30, bridge version is 0.38.0, and the package contains 45 native DLLs
under application-local `occt/`.

## Validation summary

Release and Debug builds pass Generator 37/37 and Runtime 68/68. Generated freshness,
clean package restore/publish/runtime, a 606-signature managed API baseline, clean-copy
regeneration, B18 dependency profiles, and B19 full classification are release gates.

B19 classifies 116,214/116,214 discovered declarations and 7,090/7,090 catalogued
headers with zero pending/HD099. This is classification completeness, not full generated
binding coverage.

## Publication blockers

- The user has not selected a project license (PD-012).
- Exact versions, notices, and redistribution/source obligations for non-OCCT native DLLs
  need legal/provenance review.
- Hosted full CI requires a configured immutable OCCT artifact URL and SHA256.
- Package signing and NuGet publication were not authorized and were not run.

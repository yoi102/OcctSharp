# OcctSharp 0.1.0-alpha.42

## Scope

This experimental .NET 10 / Windows x64 package advances the single product-scale
migration batch B with a large high-frequency geometry, topology, modeling, and XDE
workstream. It is locally validated but not approved for public publication.

## Identity

- Package: `OcctSharp.0.1.0-alpha.42.nupkg`.
- OCCT: 8.0.1 VC14 x64 combined artifact.
- Native ABI: 1.34.
- Bridge implementation: 0.42.0.
- Managed target: `net10.0`, Windows x64.

## Added APIs

- Circle, ellipse, three-point arc, Bezier, and interpolated edge construction.
- Edge length, point/tangent evaluation, and closest-point projection.
- Face point/normal evaluation and closest UV projection.
- Owning topology-adjacency snapshots with copied compact relationship indices.
- Loft, pipe, sewing, wedge, and thick-solid modeling operations.
- Boolean result plus copied modified/generated/deleted history summaries.
- `XdeDocument.ImportStep` for composable STEPCAF import into an existing transaction.
- The assembly Sample now composes imported labels through `XdeDocument`; the old
  `StepAssembly` facade is retained only as an obsolete compatibility API.

## Ownership and coverage

- Builders, adaptors, projectors, history objects, OCCT lists/maps, and STEPCAF source
  documents remain native-local.
- Topology results are independent registered owners. Evaluations, projections, and
  history summaries cross as copied immutable data.
- Schema 1.6 records 43 new SC-033 stable IDs and 61 accepted manual IDs total.
- Selected discovery reports 333 emitted and 61 accepted manual declarations out of
  10,956 selected declarations. Full inventory reports 10,176 declarations still
  `SupportedUnselected`; complete C# coverage is not claimed.

## Validation completed

- Release and Debug: Generator 44/44, Runtime 90/90, dependency profiles 6/6.
- Generated freshness: 13 manifest-owned files current.
- Full inventory: 116,214 declarations and 7,090 headers classified; 333 emitted,
  61 manual, 10,176 supported-unselected, 27,310 skipped, and 78,334 blocked.
- Clean NuGet consumer: 47 DLLs below `occt/`; ABI 1.34, bridge 0.42.0, OCCT 8.0.1;
  all new public workflow families execute successfully.

## Remaining gates

- Batch B is not complete while bindable declarations remain unselected and broad
  LT001-LT004 projection/ownership blockers remain.
- Hosted CI, signing, and NuGet publication are `NOT RUN`.
- Third-party legal review remains blocked. Public upload requires explicit authority.

# Current Status

- Last updated: 2026-09-01
- Current phase: Batch B through Batch M are complete locally; Preview.11 completes interactive assembly placement editing over the Preview.10 managed split while retaining one native DLL and compatibility facade
- Batch B engineering progress: 100% for the accepted local implementation scope (not a claim that every OCCT declaration is a managed API or that public release is ready)
- Batch C implementation progress: 100% of the finite local implementation denominator; locked wave denominators are 14/14, 7/7, 8/8, and final 15/15 capabilities validated
- Batch D implementation progress: 24/24 capabilities (100%); ADR-0064's one large cross-family wave passes all implementation, compile, runtime, real-HWND, clean-package, inventory, and local release gates
- Batch E implementation progress: 24/24 capabilities (100%); ADR-0066's one large cross-family wave passes implementation, compile, runtime/lifetime/transaction, AP242/BinXCAF, real-HWND, clean-package, inventory, and local release gates
- Batch F implementation progress: 24/24 capabilities (100%); ADR-0067's one cross-family definition/edit/topology/exchange/viewer wave passes compile, runtime/lifetime, real STEP/XDE, real-HWND, clean-package, inventory, and local release gates
- Batch G implementation progress: 24/24 capabilities (100%); ADR-0068's one cross-family exact/polygonal HLR, section, copied-polyline, SVG, standard-view, exchange, and viewer wave passes every local gate
- Batch H implementation progress: 24/24 capabilities (100%); ADR-0069's one grouped-mesh/material/LOD/copied-scene/interchange/viewer wave passes every local gate
- Batch I implementation progress: 24/24 capabilities (100%); ADR-0070's indivisible OCAF/XDE attribute/graph/history/undo-redo/persistence wave passes every local gate
- Batch J implementation progress: 24/24 capabilities (100%); ADR-0071's selected-feature/robust-Boolean/copied-history/recovery wave passes every local gate
- Batch K implementation progress: 24/24 capabilities (100%); ADR-0072's assembly-authoring/BOM/reference/metadata/review wave passes implementation, compile, runtime/lifetime/transaction, STEP/XDE, real-HWND, clean-package, inventory, and local release gates
- Batch L implementation progress: 24/24 capabilities (100%); ADR-0073's occurrence-aware bounds/interference/clearance/containment/incremental/review wave passes every local gate
- Batch M implementation progress: 24/24 capabilities (100%); ADR-0075's presentation/manipulator, rigid XDE placement, named history, DMU, exchange, and real-HWND wave passes every local gate
- Complete-migration batch progress: B, C, D, E, F, G, H, I, J, K, L, and M are complete locally; retired B00-B20 and forbidden numbered/dotted batch labels are not counted as batches
- Accepted surface: 16,353 generated manifest IDs plus 542 accepted manual stable IDs; Release and Debug native/managed builds, Generator 91/91, Runtime 151/151, discovery/report determinism, generated dependency closure, and dependency profiles 6/6 pass
- Last complete full inventory: 116,272/116,272 declarations and 7,090/7,090 headers have final dispositions; `Emitted` 16,353, `Manual` 542, `SupportedUnselected` 0, `Skipped` 49,344, `Blocked` 50,033, pending 0, HD099 0; SHA256 `71E921851AF636875BCA5BBAABE1B673521071A86873E6A8B538683CEAD9C4C1`
- Overall state: Preview.11 completes ADR-0075 over ADR-0074's managed physical split. Twelve module assemblies plus the `OcctSharp` compatibility/facade assembly share one `OcctSharp.Native.dll` and one `OcctSharp.Native.win-x64` runtime package. Native-DLL splitting remains deliberately deferred. Managed assembly/file identity remains `0.1.0.0`; native ABI is 1.55, bridge is 0.63.0, and schema is 1.13. `publicReleaseReady` remains false because hosted release execution, signing, and NuGet publication are `NOT RUN`

### WPF MVVM viewer sample

- Added `samples/OcctSharpViewer.Wpf` with `CommunityToolkit.Mvvm`, a WPF command/status
  surface, and an OCCT OpenGL viewport hosted by `HwndHost`. It opens STEP/IGES, supports
  rotation, pan, wheel zoom, selection modifiers, fit, standard views, and shaded or
  wireframe display. The project is part of `OcctSharp.slnx` and receives the committed
  62-DLL Windows x64 runtime through the normal project graph.
- STEP loading uses XDE/STEPCAF and displays leaf occurrences independently. Occurrence
  color/material style takes precedence over the referred definition's color/material
  style; unstyled topology keeps a neutral fallback. Independent face/subshape colors and
  IGES metadata colors are explicitly not projected by the current sample.
- Release and Debug sample builds pass with zero warnings/errors. A real Release process
  created the WPF window and OCCT OpenGL child viewport; `ArduinoUnoRev3PCB.step` loaded
  successfully with its green STEP/XDE color visible. D3DImage interop is `NOT IMPLEMENTED`
  because the OpenGL-to-D3D9Ex sharing, synchronization, and device-recovery cost is not
  justified for this sample; WPF airspace constraints therefore remain documented.

### Preview.11 Batch M interactive assembly placement editing completion

- ADR-0075 and `BATCH_M_INTERACTIVE_PLACEMENT_EDITING_GAP_INVENTORY.md` lock all
  24 capabilities as one indivisible wave across presentation transforms, viewer-parent-
  bound manipulators, custom/mouse preview, apply/cancel, rigid occurrence placement,
  named history, undo/redo, DMU recheck, STEP/XDE, real HWND, and package evidence.
- The Preview.10 audit covers exactly 24 roots and 1,650 candidates: 662 blocked, 516
  emitted, 60 manual, and 412 skipped. SC-049 reconciles exactly eight directly used
  blocked declarations; the other 654 retain their previous dispositions.
- Release and Debug build all 19 projects with zero warnings/errors; Generator 91/91,
  Runtime 151/151, focused Batch M 4/4, and dependency profiles 6/6 pass. Repository
  runtime and the clean facade consumer execute presentation transform round-trips,
  full manipulator configuration, custom/mouse preview, apply/cancel, deterministic
  ownership/thread guards, named rigid occurrence relocation, replacement labels,
  undo/redo, DMU recheck, real STEP/XDE, and real-HWND screenshot workflows.
- All 94 generated files are fresh and byte-identical after clean regeneration. Full
  inventory closes 116,272 declarations and 7,090 headers. API comparison against
  alpha.38 is additive at 38,781 additions and zero removals. Fourteen
  `8.0.1-preview.11` packages retain one 62-DLL native package and zero native
  duplication across the 13 managed packages.
- The committed bridge is 15,321,600 bytes with SHA256
  `4D1FE8F36D93D7732337FDF7D9D6D6038A29EE78D084AAE6C7A827C4D071514E` and is byte-
  identical to the complete Release rebuild. The facade nupkg SHA256 is
  `365545377020E0E4D54A92294830B11504A85CE8B24DC0692CE13C8078DC6AF7`.
  SBOM, provenance, checksums, Git whitespace, and the complete Preview.11 local
  release check pass.

### Preview.10 physical managed modules and shared native package

- ADR-0074 turns the previously split-ready generated graph into physical Runtime,
  Foundation, Geometry, MeshData, Modeling, Mesh, Documents, Visualization,
  DataExchange, Xde, IVtk, and Draw assemblies. Public namespaces remain `OcctSharp`.
- `Shape`, typed topology, `ShapeFactory`, their direct DTO/interop closure, and generated
  Modeling declarations have one `OcctSharp.Modeling` owner. `GpPoint` belongs to
  Geometry. Cross-family application workflows remain in the `OcctSharp` facade.
- The facade contains 3,233 deterministic CLR type forwarders. Aggregate comparison
  against the immediately preceding single-assembly build covers 39,301 signatures and
  reports zero additions, zero removals, and no breaking change.
- All 12 modules plus the facade compile in Release and Debug with zero warnings/errors.
  The full 19-project solution also compiles in both configurations with zero warnings/
  errors; Generator 91/91 and Runtime 147/147 pass in both configurations.
- Fourteen `8.0.1-preview.10` packages are created: 13 managed packages each contain
  one managed DLL and zero native DLLs; `OcctSharp.Native.win-x64` alone contains the
  62-DLL runtime and 11 notice/license files. `OcctSharp.Runtime` depends transitively on
  that one native package.
- The clean `OcctSharp` compatibility-package consumer passes the complete inherited
  Batch D-L runtime workflow with ABI 1.54, bridge 0.62.0, OCCT 8.0.1, and 62 DLLs under
  `occt/`. A direct `OcctSharp.Modeling` package consumer creates a six-face solid,
  loads the same 62-DLL runtime, and confirms that `OcctSharp.dll` is absent.
- Full generation produces 94 manifest-owned files from 16,353 bindings and records
  116,263 discovery declarations with `managedProjectSplitReady=true`. Generated
  forwarder freshness, deterministic discovery/model reports, and the integrated
  14-package asset/facade/direct-module consumer verifier pass. A clean source copy
  rebuilds the native bridge and all managed projects, passes Generator 91/91 and Runtime
  147/147, and reproduces all 94 generated files byte-for-byte.
- The complete local Preview.10 release check passes Release/Debug, bundled-runtime
  identity, generated freshness, clean regeneration, package consumers, API compatibility,
  full classification, SBOM, provenance, checksums, and Git whitespace gates. Hosted
  execution, package signing, and NuGet publication remain separately `NOT RUN`.

### Preview.9 Batch L digital mock-up interference and clearance completion

- ADR-0073 and `BATCH_L_DIGITAL_MOCKUP_INTERFERENCE_GAP_INVENTORY.md` lock all 24
  capabilities as one indivisible wave across occurrence expansion, AABB/OBB, broad-phase
  filtering, exact distance/contact/penetration/containment, pair matrices, diagnostics,
  owning issue topology, incremental rerun, STEP/XDE, viewer, lifetime, and clean package.
- The Preview.8 audit covers exactly 24 roots and 1,351 candidates: 656 blocked, 194
  emitted, 51 manual, and 450 skipped. SC-048 reconciles exactly ten directly used
  blocked declarations; the other 646 retain their previous dispositions.
- Preview.9 identities are native ABI 1.54, bridge 0.62.0, and schema 1.12. Release and
  Debug pass with zero code warnings/errors; Generator 91/91, Runtime 147/147, focused
  Batch L 4/4, and dependency profiles 6/6 pass. Repository runtime and the clean 62-DLL
  consumer execute XDE occurrence expansion, AABB/OBB, broad/exact phase, independent
  filtering, clearance/contact/penetration/containment/coincident states, witnesses and
  interference groups, complete matrices, aggregation, diagnostics, incremental rerun,
  real STEP/XDE, real HWND screenshots, and source/document-disposal workflows.
- All 83 generated files are fresh and byte-identical after clean regeneration. Full
  inventory closes 116,272 declarations and 7,090 headers. The committed bridge is
  15,303,680 bytes with SHA256
  `11F2FADC1A615617317987541470342BA4EE394B8B81E67501070F75E48A5FFE` and is byte-
  identical to the complete Release rebuild. API comparison is additive at 38,695
  additions and zero removals. Package SHA256, SBOM,
  provenance, and final checksums are recorded below from the complete release check.

### Preview.8 Batch K assembly authoring, BOM, and occurrence completion

- ADR-0072 and `BATCH_K_ASSEMBLY_AUTHORING_BOM_GAP_INVENTORY.md` lock all 24
  capabilities as one indivisible wave across product-structure edits, occurrence paths,
  reverse usage, graph/BOM snapshots, structure diagnostics, item/external references,
  SHUO/effective metadata, property rollups, transactions, STEP/XDE, viewer, lifetime,
  and clean-package evidence.
- The Preview.7 baseline audit covers exactly 24 roots and 1,228 candidates: 610 blocked,
  292 emitted, 34 manual, and 292 skipped. SC-047 reconciles exactly 24 directly used
  blocked declarations; the other 586 retain their prior dispositions.
- Preview.8 identities are native ABI 1.53, bridge 0.61.0, and schema 1.11. Release and
  Debug pass with zero code warnings/errors; Generator 91/91, Runtime 143/143, focused
  Batch K 4/4, and dependency profiles 6/6 pass. Repository runtime and the clean 62-DLL
  consumer execute structure edits, occurrence paths, where-used, graph/BOM/diagnostics,
  external/item references, SHUO/effective metadata, rollups, transactions, real STEP/XDE,
  real HWND screenshot, and source/document-disposal workflows.
- All 83 generated files are fresh and byte-identical after clean regeneration. Full
  inventory closes 116,272 declarations and 7,090 headers; API comparison against
  alpha.38 is additive at 38,436 additions and zero removals. The committed bridge is
  15,290,880 bytes with SHA256
  `2585B9CA96E7022914F6759F5E9CA863AC4D4140D8CB65DD049F8B3558619D2E` and is byte-
  identical to the complete Release rebuild. The final 40,947,046-byte nupkg has SHA256
  `F0BD9E01691AA8242E58DFBB0C5FA5E43A077CC9D7050DCB1245D56FE4065898`.

### Preview.7 Batch J advanced feature modeling, history, and recovery completion

- ADR-0071 and `BATCH_J_FEATURE_MODELING_HISTORY_GAP_INVENTORY.md` lock all 24
  capabilities as one wave across selected/variable edge finishing, local solid features,
  multi-shape BOP/defeaturing, robust options, preflight, copied diagnostics/history,
  recovery, STEP/XDE, viewer, lifetime, and clean-package evidence.
- The Preview.6 baseline inventory audit covers exactly 24 roots and 706 candidates: 16 emitted,
  16 manual, 374 blocked, and 300 skipped. Only directly used blocked declarations may
  be reconciled through SC-046; exactly 73 are reconciled and the audit is not bulk-marked manual.
- All builders, maps, alerts, contours, progress, and history objects remain native-local.
  Results and history topology are owning copies; options, diagnostics, deletion, and
  request association are copied values.
- Preview.7 identities are native ABI 1.52, bridge 0.60.0, and schema 1.10. Release and
  Debug pass with zero code warnings/errors; Generator 91/91, Runtime 139/139, focused
  Batch J 4/4, and dependency profiles 6/6 pass. Repository runtime and the clean 62-DLL
  package consumer execute all selected/variable/planar/draft/local-feature modes, four
  Boolean modes, multi-tool split, defeaturing, cells, robust options, bad preflight,
  bounded recovery, copied history/deletion, real STEP/XDE, and real HWND screenshots.
- All 83 generated files are fresh and byte-identical after clean regeneration. Full
  inventory closes 116,272 declarations and 7,090 headers; API comparison against
  alpha.38 is additive at 38,232 additions and zero removals. The committed bridge is
  15,267,328 bytes with SHA256
  `CADE8816FDD3638B702E5A80FB2AC287E7B31375702455D7D21ECE239A966F37` and is byte-
  identical to the complete Release rebuild. The final 40,907,579-byte nupkg has SHA256
  `AF77CA3E048277192DFB349F6C122F7CDD5909C06DAF910CD939C3BE2F95B3EC`.

### Preview.6 Batch I document state, attribute graph, history, and persistence completion

- ADR-0070 and `BATCH_I_DOCUMENT_HISTORY_PERSISTENCE_GAP_INVENTORY.md` close all 24
  capabilities as one wave across copied label/attribute state, reference/dependency
  graphs, named commands, undo/redo history, dirty/savepoint state, four OCAF/XCAF
  persistence formats, STEP/XDE, lifetime, and clean-package evidence.
- The focused audit covers exactly 24 roots and 676 candidates: 219 emitted, four manual,
  288 blocked, and 165 skipped. SC-045 reconciles exactly 54 directly used blocked stable
  IDs without bulk-marking the root audit manual.
- The ownership closure preserves owned documents, parent-bound stable entries, copied
  snapshots/graphs, and independent owning topology. No TDF iterator, attribute handle,
  delta, undo/redo list, or persistence driver crosses the ABI.
- Release and Debug build with zero warnings/errors; Generator 91/91, Runtime 135/135,
  focused Batch I 4/4, and dependency profiles 6/6 pass. Repository runtime and the clean
  62-DLL package consumer execute typed state, graph/history/undo-redo/savepoint, all four
  OCAF/XCAF formats, STEP/XDE, and source-disposal workflows.
- All 83 generated files are fresh and byte-identical after clean regeneration. The
  generated dependency graph has 16,353 declarations, 27 resolved edges, zero violations,
  and zero cycles. Full inventory closes 116,272 declarations and 7,090 headers.
- The committed bridge is 15,216,128 bytes with SHA256
  `F8C7825FC770963068ADAE008FDAD95EC2C3155A2DAADA2CD3245DAFEAC76E00` and is byte-
  identical to the complete Release rebuild. The nupkg SHA256 is
  `3502A1F36C1D4F44805A5AF9173F7DED46405300ED0D29E1B7D11A60ECEB70AD`.
- API comparison against alpha.38 is additive at 38,128 additions and zero removals.
  Inventory SHA256 is `D8C4F6C1CC1F2AD378F5722DEB507E5F1C6E4AE62E153F07F6A5C65464307A64`;
  SBOM, provenance, checksums, Git whitespace, and the complete Preview.6 local release
  check pass.

### Preview.5 Batch H advanced mesh and scene completion

- ADR-0069 and `BATCH_H_ADVANCED_MESH_SCENE_GAP_INVENTORY.md` close all 24 capabilities
  as one wave across grouped triangulation attributes, diagnostics, LODs, colors,
  physical/PBR materials, copied XDE hierarchy/shared instances/transforms, mesh
  interchange, STEP/XDE, and real-HWND evidence.
- The focused audit covers exactly 24 roots and 840 candidates: 154 emitted, 10 manual,
  450 blocked, and 226 skipped. SC-044 reconciles exactly 24 directly used blocked stable
  IDs without bulk-marking the root audit manual.
- Release and Debug build with zero warnings/errors; Generator 91/91, Runtime 131/131,
  focused Batch H 4/4, and dependency profiles 6/6 pass. Runtime tests are serialized to
  honor OCCT viewer global/thread ownership. Repository runtime and the clean 62-DLL
  package consumer execute the complete Batch H flow.
- All 83 generated files are fresh and byte-identical after clean regeneration. The
  generated dependency graph has 16,353 declarations, 27 resolved edges, zero violations,
  and zero cycles. Full inventory closes 116,272 declarations and 7,090 headers.
- The committed bridge is 15,159,808 bytes with SHA256
  `26432903E96CA6AA981078596ADEE3A5866AE94DD93E71354E54D2638208A1BB` and is byte-
  identical to the complete Release rebuild. The nupkg SHA256 is
  `C84EA10C3F222A2C7FE0B67AAA1DBC17FAEE029F5597C86BB4E38177BEA283C2`.
- API comparison against alpha.38 is additive at 37,904 additions and zero removals.
  Inventory SHA256 is `75BD35320CA769AA54FEE0B09F17A15A1560352B0CDACEA6BEEFBDD8494AD695`;
  SBOM, provenance, checksums, Git whitespace, and the complete Preview.5 local release
  check pass.

### Preview.4 Batch G technical drawing completion

- ADR-0068 and `BATCH_G_TECHNICAL_DRAWING_GAP_INVENTORY.md` close all 24 capabilities
  as one wave across exact/polygonal HLR, orthographic/perspective projectors, ten owning
  edge-category layers, planar sections, copied polylines, layered SVG, standard views,
  STEP/XDE, and real-HWND evidence.
- The focused audit covers exactly 24 roots and 1,069 candidates: 233 emitted, 18 manual,
  535 blocked, and 283 skipped. SC-043 reconciles exactly 33 directly used blocked stable
  IDs without bulk-marking the root audit manual.
- Release and Debug build with zero warnings/errors; Generator 91/91, Runtime 127/127,
  focused Batch G 4/4, and dependency profiles 6/6 pass. Repository runtime and the clean
  62-DLL package consumer execute the complete HLR/section/SVG/STEP-XDE/real-HWND flow.
- All 83 generated files are fresh and byte-identical after clean regeneration. The
  generated dependency graph has 16,353 declarations, 27 resolved edges, zero violations,
  and zero cycles. Full inventory closes 116,272 declarations and 7,090 headers.
- The committed bridge is 15,139,840 bytes with SHA256
  `725C014D637E3100619A4F626B4AE6F626E3D0162761F85124ABB8C8E563FE14` and is byte-
  identical to the complete Release rebuild. The nupkg SHA256 is
  `44C59BE285F5388C468D2FBFFD7D17649A51AC53A0AD50FD0E6D10333EB71C0D`.
- API comparison against alpha.38 is additive at 37,731 additions and zero removals.
  Inventory SHA256 is `78BDED2909920DD037D99608604E85EBC87BDD5FD144FAAC247C88D69DE1A318`;
  SBOM, provenance, checksums, Git whitespace, and the complete Preview.4 local release
  check pass.

### Preview.2 Batch E engineering inspection and PMI completion

- ADR-0066 and `BATCH_E_INSPECTION_PMI_GAP_INVENTORY.md` close all 24 capabilities as
  one wave across exact inspection/measurement, units, semantic dimensions/tolerances/
  datums, complete PMI reference graphs, transactional mutation, AP242 GDT/saved views,
  viewer-owned annotations, and screenshot evidence.
- SC-041 reconciles exactly 102 direct blocked OCCT 8.0.1 stable IDs. It also records
  tolerance-datum graph replacement/detach, zero-based dimension descriptions, non-Area
  datum-target validation, and the OCCT 8.0.1 datum-point X persistence correction.
- Release and Debug native/managed builds pass with zero errors, Generator 91/91,
  Runtime 119/119, and dependency profiles 6/6. Four focused Batch E completion tests
  pass. Repository runtime and the clean package consumer execute authored AP242/BinXCAF,
  saved-view, four viewer-dimension kinds, real HWND, and non-empty screenshot evidence.
- Preview.2 pack/clean-consumer verification passes with 62 DLLs and 11 notice/license
  files. Package/assembly/informational identities are `OcctSharp`/`8.0.1-preview.2`,
  `0.1.0.0`, and `8.0.1-preview.2`; native ABI is 1.47 and bridge is 0.55.0.
- The committed bridge is 15,044,096 bytes with SHA256
  `4A5B67B886146E704E5A31F25CDB87C75CC351EB496854D77F0435D0142B22B1` and is byte-
  identical to the complete Release rebuild. The nupkg SHA256 is
  `D858CE7DB977AE79FB465D5582EF3B5691F852E824A55AF248D27D41DB94BDB3`.
- API comparison against alpha.38 is additive at 37,490 additions and zero removals.
  Inventory, 83-file freshness/clean regeneration, runtime hashes, SBOM, provenance,
  checksums, Git whitespace, and the complete Preview.2 local release check pass.

### Preview.3 Batch F freeform authoring completion

- ADR-0067 and `BATCH_F_FREEFORM_AUTHORING_GAP_INVENTORY.md` close one 24-capability
  Batch F across rational Bezier/B-spline curve/surface definitions, interpolation and
  approximation, immutable edits, profiles, offsets, filling, splitting, controlled
  loft/pipe-shell, repair, STEP/XDE, viewer selection/measurement/mesh, and screenshots.
- The focused audit covers exactly 24 decision-driving roots and 1,122 candidate
  declarations: 215 emitted, 23 manual, 598 blocked, and 286 skipped. It guides
  implementation and does not replace the immutable 24-row product denominator.
- Definitions and diagnostics cross as immutable copied records; point/pole/weight/
  knot/multiplicity/grid inputs are copied and validated; algorithm state remains
  native-local; every topology result is independently owning. XDE and viewer ownership
  categories remain unchanged.
- SC-042 reconciles exactly 94 directly used blocked stable IDs. Release and Debug pass
  with zero errors, Generator 91/91, Runtime 123/123, and dependency profiles 6/6.
- Repository runtime and the clean 62-DLL package consumer execute the complete authored
  definition/edit/profile/fill/split/loft/pipe/heal-to-STEP/XDE-to-mesh/measurement/
  real-HWND-selection/screenshot workflow.
- The committed bridge is 15,120,384 bytes with SHA256
  `B36051D6E1B9E8E5A5BD8BED9D06EF0C44D59DB83A9A2195BF9082827CDE7075` and is byte-
  identical to the complete Release rebuild. The nupkg SHA256 is
  `43954141FB8CA19CBF176065F3D8E6B38F0DBD7E4923D4A8F76DF5E21D41E40C`.
- API comparison against alpha.38 is additive at 37,636 additions and zero removals.
  Inventory, 83-file freshness/clean regeneration, runtime hashes, SBOM, provenance,
  checksums, Git whitespace, and the complete Preview.3 local release check pass.

### Preview.1 version transition and Batch E preparation (historical)

- ADR-0065 sets NuGet package identity to `8.0.1-preview.1`: OCCT 8.0.1 supplies the
  numeric core and the OcctSharp preview counter starts at 1. The managed assembly/file
  version remains `0.1.0.0`; native ABI remains 1.46 and bridge remains 0.54.0.
- ADR-0066 and `BATCH_E_INSPECTION_PMI_GAP_INVENTORY.md` lock one 24-capability Batch E
  across exact inspection/measurement, units, semantic dimensions/tolerances/datums,
  complete PMI reference graphs, transactional mutation, AP242 GDT/saved views,
  viewer-owned annotations, and screenshot evidence.
- The focused Batch E root audit covers 990 candidate declarations: 142 emitted, two
  manual, 604 blocked, and 242 skipped. It is implementation guidance, not the 24-row
  product denominator or a full-OCCT coverage claim.
- Batch E preparation is complete and implementation is 0/24. Native/managed compile
  after Batch E changes, runtime/lifetime/transaction tests, real AP242 round trip,
  real-HWND annotation/screenshot, clean package consumer, and full release check are
  all `NOT RUN` for Batch E.
- Preview.1 pack and clean-package verification pass with 62 DLLs. Direct nupkg
  inspection confirms package `OcctSharp`/`8.0.1-preview.1`, assembly/file version
  `0.1.0.0`, exact informational version `8.0.1-preview.1`, ABI 1.46, and bridge 0.54.0.
  Final nupkg SHA256 is
  `53C42170B46194B2EDDC6F6F5722EA1A48D34C26D25BE97AC51F49277B98C411` and matches
  `checksums.sha256`. SBOM/provenance metadata generation and the full Preview.1 local
  release check pass.
  Alpha.55 evidence below remains the inherited implementation baseline and is not
  rewritten as Batch E evidence; Preview.1 release gates validate that inherited Batch D
  implementation plus the version transition, not Batch E implementation.

### Alpha.55 Batch D production viewport/model-review completion

- ADR-0064's one 24-capability Batch D denominator is complete across STEP/XDE occurrence
  identity, TopoDS owning topology, AIS/SelectMgr point and area selection, built-in
  filters, AIS colored subshape overrides, V3d camera/coordinate state, Graphic3d clip
  planes, standard review aids, and screenshot evidence.
- `BATCH_D_VIEWPORT_GAP_INVENTORY.md` records the alpha.54 baseline, all 24 completed rows,
  the end-to-end dependency closure, non-goals, and a focused 52-overload inventory audit:
  19 emitted, 31 blocked, and two skipped candidate roots.
- Ownership remains fixed: the viewer is HWND-bound and
  creation-thread-affine; presentations, filters, and clip planes are parent-bound;
  XDE identity/camera/coordinate data is copied; detected/selected topology is an
  independent owning `Shape`; screenshot output exposes no native image container.
- Batch D remains one wave. Selection, presentation, camera, clipping, and screenshot
  families are not sub-batches or partial completion checkpoints. One real STEP/XDE plus
  real-HWND workflow must pass in repository runtime and a clean 62-DLL package consumer.
- SC-040 reconciles exactly 18 newly direct blocked stable IDs. Package identity is
  `0.1.0-alpha.55`, native ABI is 1.46, and bridge implementation is 0.54.0. The
  committed `OcctSharp.Native.dll` is 14,946,304 bytes with SHA256
  `B6BE4DFCA9D0DC576A118C832036EF8131AFDBF2D88F91F8B4D42D45B765F4CC`.
- `release-check.ps1 -PackageVersion 0.1.0-alpha.55` passes Release and Debug builds,
  Generator 91/91, Runtime 115/115, dependency profiles 6/6, 83-file freshness and
  byte-identical clean regeneration, runtime build/hash identity, API compatibility,
  complete inventory classification, SBOM/provenance/checksums, and Git whitespace.
- `BatchDCompletionTests` and the clean 62-DLL package consumer execute the complete
  real STEP/XDE-to-real-HWND review-to-screenshot workflow. They cover copied identity,
  owning topology, area selection/filtering, reversible isolate, subshape styling,
  camera/conversions, clipping/review aids, cross-thread/cross-viewer rejection, and a
  non-empty Unicode-path screenshot.
- API comparison against alpha.38 is additive at 37,125 additions, 0 removals, and no
  breaking changes. Final classification is 16,353 emitted, 120 manual, 49,344 skipped,
  50,455 blocked, and zero supported-unselected/pending/HD099.

### Alpha.54 final Batch C selective-import, topology-edit, and viewer-input closure

- The locked final denominator is 15/15 across edge first/second derivatives, face U/V
  derivatives and oriented normals, edge-on-face pcurves, edge/face trim, wire building,
  replace/remove topology edits, bidirectional adjacency, owning STEP reader sessions,
  file units and selective root transfer, explicit target units, whole/subshape selection,
  owning selected topology snapshots, application input, and one end-to-end integration.
- ADR-0063 fixes the ownership boundary and SC-039 reconciles exactly 17 newly direct
  OCCT 8.0.1 stable IDs. Call-local adaptor/builder/reshape state never crosses the ABI;
  returned topology and selected subshapes are independent owning copies; STEP sessions
  own their reader; viewer presentations and input remain parent-bound and thread-affine.
- Package identity is `0.1.0-alpha.54`, native ABI is 1.45, and bridge implementation is
  0.53.0. The committed `OcctSharp.Native.dll` is 14,920,192 bytes with SHA256
  `57593BC8B66870DE0373BFBDEFF47B1731C20DF6066EFF22764254EB416E54AA`.
- The full `release-check.ps1 -PackageVersion 0.1.0-alpha.54` run passes Release and
  Debug builds with 0 errors, Generator 91/91, Runtime 114/114, dependency profiles 6/6,
  83/83 generated freshness, and 83/83 byte-identical clean regeneration. The committed
  runtime matches the Release build byte-for-byte.
- The clean alpha.54 package consumer restores, publishes, and executes the final Batch C
  workflow with all 62 DLLs. API comparison against alpha.38 is additive at 37,018
  additions, 0 removals, and no breaking changes.
- Complete classification passes at 116,272/116,272 declarations and 7,090/7,090 headers:
  16,353 emitted, 102 manual, 49,344 skipped, 50,473 blocked, and zero supported-
  unselected/pending/HD099. Generated dependency closure has 27 direct edges, zero
  unresolved references, zero graph violations, and zero cycles.
- `batchImplementationComplete` is true and Batch C is closed. Advanced filters, custom
  rendering, optional integrations, low-frequency schema, and exhaustive mesh attributes
  are out of scope rather than unfinished C work. `publicReleaseReady` is false because
  hosted CI execution, signing, and NuGet publication were not run.

### ADR-0062 generated cross-shard dependency-closure checkpoint

- The generator now resolves emitted return, parameter, base, enum, `Handle<T>`,
  `gp_Pnt`, and `TopoDS_Shape` references against the complete emitted model. `SD001`
  blocks unresolved emitted types and `SD002` blocks dependencies outside the accepted
  target graph before generated source can be accepted.
- The deterministic closure report covers all 16,353 emitted declarations and 83
  generated files. Its 27 observed direct cross-shard edges have zero unresolved
  references, zero target-graph violations, and zero cyclic groups; SHA256 is
  `A1635978C80B75D4D85507E9D5A4C0DB614B9ADD1557F97D53A3B697BDB36F30`.
- `MeshData` now owns the `Poly` data contracts, `FEmTool` and `Law` are classified as
  Geometry, and `TopAbs_Orientation` is a Foundation value contract. The accepted graph
  also records the real Documents, DataExchange, Visualization, and Xde directions.
- The generated managed graph is split-eligible, but physical managed-project splitting
  is deferred until assembly-qualified identity, type forwarding, facade ownership, and
  package compatibility have a separate accepted design. Native-DLL splitting is not
  eligible until creator-owned registries, allocators, validation, and release routing
  work across DLL boundaries. This checkpoint keeps one `OcctSharp.dll`, one
  `OcctSharp.Native.dll`, and one package.
- Release and Debug builds pass with 0 warnings/errors; Generator 91/91, Runtime
  108/108, and dependency profiles 6/6 pass in both. Freshness and clean regeneration
  verify 83/83 files, the clean alpha.53 package consumer passes with 62 DLLs, and the
  complete local release check records the dependency-closure gate as `PASS`.

### ADR-0061 generated domain/layer partition checkpoint

- Binding-model schema 1.3 assigns every declaration to Foundation, Geometry,
  Modeling, Mesh, Documents, DataExchange, Xde, Visualization, IVtk, OpenGles, Draw,
  or Runtime. The classifier is fail-closed for emitted packages, and tests lock the
  target acyclic managed-project graph plus representative package identities.
- Manifest schema 1.1 now owns 83 generated files and records `productModule`, `apiLayer`,
  and `outputShard` for each. The distribution is 61 Raw, 21 SafeManaged, and one
  Runtime file across 12 manifest modules; obsolete centralized generated
  paths are removed by normal manifest stale cleanup.
- Native shared-handle registries and allocators remain creator-owned inside one
  `OcctSharp.Native.dll`; managed public types retain the `OcctSharp` namespace and
  compile into one `OcctSharp.dll`. No ownership rule, ABI name/version, package ID,
  or public type full name moved.
- ADR-0062 supersedes this checkpoint's previously open dependency audit: all current
  generated cross-shard references are now resolved and acyclic. That graph result is
  necessary but deliberately not sufficient authority for a physical assembly or DLL
  split.
- The largest generated file is now the 9.09 MiB DataExchange native shard, down from
  the former 15-19 MiB centralized translation-unit range. Native CMake recursively
  collects the product-module sources.
- Release and Debug builds pass with 0 warnings/errors; Generator 91/91, Runtime
  108/108, and dependency profiles 6/6 pass in both. Generated freshness and clean
  source regeneration verify 83/83 byte-identical files. The clean alpha.53 package
  consumer passes with the synchronized 62-DLL runtime.
- API comparison with alpha.38 remains additive at 36,883 additions, 0 removals, and
  no breaking change. Full inventory remains 116,272/116,272 declarations and
  7,090/7,090 headers classified with the same 16,353 emitted, 85 manual, 49,344
  skipped, and 50,490 blocked dispositions.

### Alpha.53 XDE validation properties, occurrences, and STEP options checkpoint

- The locked 8-capability denominator spans BRepGProp property computation, optional
  XCAF area/volume/centroid attributes, direct/recursive occurrence flattening, composed
  world locations and located shapes, STEPCAF metadata/model options, and integration.
- `ValidationProperties` reads, replaces, or clears copied optional attributes;
  `UpdateValidationPropertiesFromShape` computes them through existing owning property
  wrappers. `XdeOccurrence` owns its composed location and returns an independent located
  shape. STEP read/write overloads validate explicit common metadata and model switches.
- SC-038 reconciles nine directly used manual stable IDs. ABI is 1.44, bridge is 0.52.0,
  package is alpha.53, and the committed 62-DLL runtime is updated and hash-pinned.
- Release and Debug build with 0 warnings/errors; Generator 62/62 and Runtime 108/108
  pass in both. Generated freshness, byte-identical clean source regeneration, clean
  alpha.53 package consumer, API compatibility, inventory, SBOM/provenance/checksums,
  and local release gates all pass.
- The API diff against alpha.38 is additive: 36,883 additions, 0 removals, no breaking
  change. `batchImplementationComplete` is true for the local checkpoint while
  `publicReleaseReady` remains false because hosted full release, signing, and NuGet
  publication were not run.

### Alpha.52 import diagnostics and repair checkpoint

- The locked 7-capability denominator spans STEPControl/XSControl read and transfer
  reporting, copied per-subshape BRepCheck issues and check options, ShapeFix before/
  after validation, V3d mouse rotation, and runtime/package integration.
- `ReadStepWithReport` returns an owning shape plus typed status, candidate/transferred
  root counts, shape count, and system length unit. `GetValidationReport` returns copied
  shape-kind/status issues without retaining analyzer state. `RepairWithReport` owns the
  repaired shape and immutable before/after reports.
- SC-037 reconciles six directly used manual stable IDs. ABI is 1.43, bridge is 0.51.0,
  package is alpha.52, and the committed 62-DLL runtime is updated and hash-pinned.
- Release and Debug build with 0 warnings/errors; Generator 62/62 and Runtime 107/107
  pass in both. Generated freshness, byte-identical clean source regeneration, clean
  alpha.52 package consumer, API compatibility, inventory, SBOM/provenance/checksums,
  and local release gates all pass.
- The API diff against alpha.38 is additive: 36,821 additions, 0 removals, no breaking
  change. `batchImplementationComplete` is true for the local checkpoint while
  `publicReleaseReady` remains false because hosted full release, signing, and NuGet
  publication were not run.

### Alpha.51 first Batch C common-API checkpoint

- The active product outcome is the routine CAD path: create/import, inspect underlying
  geometry and topology, perform common modeling edits, validate/measure, mesh, preserve
  document metadata, export, display, and select.
- C advances three coverage lanes together: model/inspect, build/modify/deliver, and
  present/interact. They are not sub-batches or independently completable phases.
- A normal implementation wave must cross at least three connected API families and
  finish an end-to-end workflow. Single-class/method work is folded into the active wave
  unless it is a demonstrated blocker across several common families.
- The first large wave is complete for its locked 14-capability denominator in
  `COMMON_API_GAP_INVENTORY.md`: topology counts/closedness/validity/tolerances, detailed
  mesh normals/UV/face mapping, native BREP, atomic XDE part metadata, and common viewer
  appearance/camera/selection controls.
- SC-036 reconciles nine directly used manual stable IDs. ABI is 1.42, bridge is 0.50.0,
  package is alpha.51, and the committed 62-DLL runtime is updated and hash-pinned.
- Release and Debug builds pass with 0 warnings/errors; Generator 62/62 and Runtime
  107/107 pass in both configurations. Generated freshness, byte-identical clean source
  regeneration, clean package consumer, API compatibility, inventory, SBOM/provenance,
  checksums, and local release gates all pass.
- The API diff against alpha.38 is additive: 36,729 additions, 0 removals, no breaking
  change. `batchImplementationComplete` is true for the local checkpoint while
  `publicReleaseReady` remains false because hosted full release, signing, and NuGet
  publication were not run.

### Alpha.50 clone-and-run distribution wave

- Package version is `0.1.0-alpha.50`; native ABI remains 1.41 and bridge remains
  0.49.0. The generated/public API is unchanged from alpha.49.
- The repository now commits the accepted 62-DLL Windows x64 Release closure under
  `OcctSharp/runtime/win-x64/occt/`: 98,990,032 bytes total, largest file 14,863,872
  bytes, with no GitHub 100 MiB object-limit violation.
- `runtime-manifest.json` pins 62 DLLs and 11 notice/license files by path, size, and
  SHA256. The complete Release rebuild is byte-identical to the committed DLL closure.
- Repository project builds prefer the committed runtime and clean stale output DLLs;
  ADR-0051's OCCT SDK bootstrap remains an explicit contributor override.
- A genuinely new local Git clone with no `local.settings.json` and no OCCT environment
  variables passed manifest verification, Release smoke, Debug smoke, and alpha.50 pack.
  Both smokes loaded ABI 1.41/bridge 0.49.0/OCCT 8.0.1, copied exactly 62 DLLs, and
  created a six-face box.
- MIT project licensing and packaged OCCT, oneTBB, FreeImage, FreeType, OpenVR, FFmpeg,
  and jemalloc notice/license material resolve PD-012. The unavailable jemalloc bundle
  version is disclosed rather than guessed.
- The complete alpha.50 release check passes: Release/Debug Generator 62/62 and Runtime
  105/105, byte-identical 13-file clean regeneration, clean package consumer, API diff
  36,602 additions/0 removals, 116,272-declaration/7,090-header classification, runtime
  manifest/build identity, SBOM/provenance/checksums, and Git whitespace gates.

### Alpha.49 final long-tail and completion-gate wave

- Package version is `0.1.0-alpha.49`; native ABI is 1.41 and bridge implementation is 0.49.0.
- Generated stable IDs increased from 15,892 at the continuation checkpoint to 16,353.
  Standalone named Int32 enums, verified void/static value calls, and the export-proven
  Standard foundation free-function profile are generated. Anonymous enums remain
  `SK017`; free functions without exact export evidence remain `BL002` or `BL003`.
- The final inventory classifies 116,272 declarations: 16,353 emitted, 61 accepted
  manual, 49,344 skipped, and 50,514 narrowly blocked. `SupportedUnselected`,
  LT001-LT004, declaration/header pending, and HD099 are all zero. Inventory SHA256 is
  `EC57888D76FD7726806EB5D4247CBB2020C588481651FDF834E2A13F1F3E0DB6`.
- Release and Debug native/managed builds pass with zero warnings/errors; Generator
  62/62, Runtime 105/105, deterministic discovery/reports, and dependency profiles 6/6
  pass in both configurations.
- `release-check.ps1` now requires both zero `SupportedUnselected` and zero broad
  LT001-LT004 reasons, plus all local implementation gates, before setting
  `batchImplementationComplete`. It cannot derive completion from classification alone.
- The complete alpha.49 release check passes: generated freshness 13/13, byte-identical
  clean regeneration, a clean 62-DLL package consumer at ABI 1.41/bridge 0.49.0,
  API compatibility with 36,602 additions and zero removals, release metadata,
  checksums, and Git whitespace gates. Public release authority and external
  legal/signing/hosted-CI gates are not implied by local Batch B completion.

### Core toolkit closure and optional-package isolation

- The full generated C++ wave compiled all 16,017 manifest bindings successfully before
  link, including the generated translation-unit completion header.
- The native target now uses one explicit `OCCTSHARP_CORE_TOOLKITS` list for both linking
  and runtime DLL copying. It includes the selected FoundationClasses, ModelingData,
  ModelingAlgorithms, ApplicationFramework, DataExchange, and supported Windows
  visualization toolkits while excluding IVtk, OpenGL ES, and Draw/test toolkits.
- The expanded core link closure reduced the observed Release link failure from 454 to
  141 unresolved symbols without regenerating source. The remaining list is dominated by
  Draw/test and IVtk declarations plus a small number of artifact-specific core symbols.
- Schema 1.8 `excludedAutoPackages` now gives Draw/test packages `SK009 / TestHarness`
  and IVtk packages `SK010 / OptionalExternalDependency`. The same configuration controls
  generation eligibility and full-inventory disposition, so excluded declarations remain
  auditable rather than silently disappearing.
- Current validation: Generator 55/55 PASS on .NET SDK 10.0.400; complete 16,017-binding
  C++ compile PASS; Release native link FAIL with 141 unresolved symbols before package
  exclusion regeneration. Managed compile, Runtime, Debug, inventory, determinism,
  package consumer, and release gates are `NOT RUN` for this wave.
- Next: regenerate after package isolation, recompile/relink the reduced core candidate,
  then classify any remaining exact missing symbols using import-library evidence.

### Full-selection ABI and allocator hardening

- Static value-copy exports now use a dedicated `_static_` ABI segment and generated
  shared instance methods use `_method_`; constructor, infrastructure, static, and
  instance entry points can no longer collide solely because OCCT calls a method
  `Create`, `Clone`, or another infrastructure name.
- Shared methods are assigned one deterministic ordinal sequence per normalized member
  name. Case variants such as `Clear`/`clear` and repeated inherited or macro declarations
  therefore receive unique native and raw-managed names. Friendly C# overload names are
  retained unless the complete managed parameter signature duplicates an earlier member,
  in which case a deterministic `GeneratedN` suffix is applied.
- Configuration schema 1.8 records exact shared types requiring
  `NCollection_IncAllocator` placement construction. `BRepMeshData_Curve` is emitted with
  `new (allocator)` and its native wrapper retains the allocator before the object field,
  ensuring the object is destroyed while allocator storage is still alive; clones retain
  the same allocator. This replaces the invalid ordinary `new` expression without using
  an allocation/deallocation mismatch.
- Current validation: Generator 53/53 PASS on .NET SDK 10.0.400. Full regeneration,
  native/managed compilation, Runtime tests, deterministic generation, inventory, Debug,
  package, and release gates are `NOT RUN` after these fixes.
- Next: regenerate the 116,190-declaration model, compile the 16,017-ID native wave, and
  continue fixing the next real compiler/linker errors before any completion claim.
- The first post-fix Release compile passed export-name and allocator construction but
  stopped because `RWGltf_GltfLatePrimitiveArray.hxx` instantiates
  `NCollection_Sequence<RWGltf_GltfPrimArrayData>` while only forward-declaring its
  element type. Schema 1.8 now supports ordered `generatedPreambleHeaders`; the exact
  `RWGltf_GltfPrimArrayData.hxx` completion header is emitted before shared scope headers.
  Generator 53/53 passes after this rule; the next full Release retry is pending.

### Generated foundation, adaptor, topology, and infrastructure closure

- Expanded package-scoped intrusive-handle generation across foundation, adaptor,
  modeling infrastructure, BRep/TopoDS implementation records, mesh infrastructure,
  healing context, STEP infrastructure, and related common API families. Emitted stable
  IDs increased from 4,060 to 6,555 without adding hand-maintained generated output.
- Constructor dependency closure now accepts generated `Handle<T>` inputs, rejects
  non-`Standard_Transient` records, suppresses creation for abstract or otherwise
  nonconstructible shared types, and keeps return-only wrappers for infrastructure types.
- Added exact configured exclusions: `SK007 / SuppressedConstruction` for the deliberately
  return-only generic `Standard_Transient` creation surface, and `SK008 /
  ArtifactUnavailable` for the two StepData symbols absent from the pinned import
  libraries. Broad package suppression is no longer used to hide linkable StepData
  constructors.
- Native/C# reserved words and managed `Object` member collisions are renamed
  deterministically. Generated native constructor/return locals use collision-free names;
  this fixes the observed `TCollection_HAsciiString(int)` access violation caused by a
  shadowed constructor argument.
- Static value generation now accepts the verified `gp_Pnt` projection, enabling common
  three-point mesh deflection helpers alongside scalar/enum methods.
- Current Release evidence: .NET SDK 10.0.400, native and managed build with zero warnings
  and errors, deterministic two-run generation, Generator 51/51, Runtime 105/105, and
  dependency profile audit 6/6. Full inventory regeneration is currently running.

### Alpha.48 IGES wave closure

- Added generated IGESAppli, IGESBasic, IGESDefs, IGESDimen, IGESDraw, IGESGeom,
  IGESGraph, and IGESSolid shared-handle families: 984 additional emitted stable IDs,
  162 public wrappers, and 156 default-constructible wrapper lifecycle checks.
- Native ABI is 1.40 and bridge implementation is 0.48.0. Release and Debug builds,
  Generator 44/44, Runtime 147/147, 13-file freshness, byte-identical clean regeneration,
  alpha.38 API diff (10,272 additions/0 removals), and clean package consumer (47 DLLs)
  passed using .NET SDK 10.0.400.
- Final inventory: 7,058/7,090 headers semantically parsed, 116,214 declarations,
  0 pending declarations/headers, 0 HD099; `Emitted` 4,060, `Manual` 61,
  `SupportedUnselected` 11,144, `Skipped` 27,310, `Blocked` 73,639.
- Discovery/coverage/diagnostics hashes are
  `0AAD3A7F9571D3BE584498AB302FF59712D27234BE28815FADE4A656A14EC5F6`,
  `AAE90902FE8D4779A365A4D6DF6C8CEF20EAD087F28211DA95B42320A7734F0B`, and
  `6542FD0D9809231F56ADB3B97A9767F56515CEE740063A700D8D347566DE389F`.
- At the alpha.48 checkpoint, Batch B remained in progress; that release gate report
  deliberately kept bindable emission completeness blocked while 11,144 supported
  declarations remained unselected.

## Current focus

All declared product batches B through M are complete for their finite accepted local
denominators. Preview.11 closes Batch M's interactive presentation/manipulator and rigid
XDE occurrence-placement workflow at 24/24 while retaining ADR-0074's managed module
graph and one native DLL. A later product batch starts only after a new cross-family gap
inventory and whole-letter ADR are explicitly accepted. Hosted release execution,
signing, and publication remain separate release-readiness work.

### Shared-handle dependency-closure hardening

- Package-scope shared-handle discovery now recognizes constructors that receive other
  OCCT intrusive handles, then retains only the closed set whose handle targets have a
  generated scope. This removes an artificial value-only restriction without allowing
  an unwrapped native pointer through the ABI.
- The first attempt to add constructor-less abstract base scopes was rejected by the
  Release native compile: the current AST's `IsAbstract` fact does not account for all
  inherited pure virtual members, and nested non-`Standard_Transient` records can be
  mistaken for handle targets. The rejected generated output was regenerated away.
- Current selected emission remains 4,060 declarations; this is generator hardening,
  not a coverage increase. The next expansion must add verified gp value projections
  and an inheritance-complete transient/abstract classifier before introducing more
  Geom/Geom2d base handles.

### Completed transformation and location capability milestone

- Added `GpTrsf` identity/creation, clone, inversion, multiplication, matrix-value,
  and shape-application APIs over an opaque native registry handle.
- Added finite-value and 1-based matrix-index validation; operation results are
  independent values after source disposal.
- Kept the scalar `ShapeTransform` API and added an explicit `ToGpTrsf()` conversion.
- Added `TopLocLocation` identity/from-`GpTrsf`, clone, inversion, multiplication,
  identity query, conversion to `GpTrsf`, and disposal APIs.
- Added absolute `Locate` and relative `Move` placement on `TopLocLocation`, plus
  `Shape.Located` and `Shape.Moved` convenience methods.
- Added `GpVec`, `GpDir`, `GpAx1`, and `GpMat` opaque owners with finite/non-zero
  validation, clone/components, vector math, direction/axis reversal, matrix access,
  determinant, and vector/axis-to-`GpTrsf` conversion.
- This is the accepted B05 manual bridge under SC-005/SC-006/SC-007 and ADR-0018–0020;
  it is deliberately counted as one batch rather than split into smaller migration units.

### Completed strings and scalar-collection capability milestone

- Added `OcctAsciiString` for UTF-8 byte copies, append, clone, and extended conversion.
- Added `OcctExtendedString` for UTF-8 conversion, UTF-16 code-unit access, append,
  clone, and ASCII conversion.
- Added `OcctRealSequence` over `NCollection_Sequence<double>` with clone, count,
  0-based managed indexing, append, set, remove, enumeration, and finite-value checks.
- Added `OcctRealArray` over `NCollection_Array1<double>` with explicit native lower-bound
  reporting, 0-based managed indexing, clone, mutation, enumeration, and finite-value checks.
- Added `OcctRealVector` over OCCT 8's `NCollection_DynamicArray<double>` backing for the
  deprecated `NCollection_Vector<double>` alias, with clone, append, mutation, enumeration,
  and finite-value checks.
- Added `OcctIntRealMap` over `NCollection_DataMap<int,double>` with lookup, bind, unbind,
  clone, duplicate-key rejection, and finite-value checks.
- Added `OcctIntIndexedMap` over `NCollection_IndexedMap<int>` with ordered key/index lookup,
  append, last-item removal, clone, duplicate-key rejection, and 0-based managed indexing.
- Added explicit caller-owned UTF-8 buffers, registry validation, and no native pointer
  exposure under SC-008/ADR-0021, SC-009/ADR-0022, and SC-010/ADR-0023.
- Added one-shot caller-owned snapshots for sequence/array/vector/map families; snapshots
  copy values without crossing native iterators and remain independent after mutation or disposal.
- B06 is complete for the declared scalar/map profile; richer element mappings, sets, and
  borrowed/parent-bound iterator views remain pending future subprofiles.

### Completed immutable geometry capability milestone

- Added the immutable `GpPoint` facade over the already generated `gp_Pnt` value-copy
  constructors/default/copy exports.
- Added finite-coordinate validation, origin creation, copy independence, and managed
  Euclidean distance semantics without crossing a native layout.
- The complete declared immutable value family is implemented and validated; mutation
  and broader Geom/Geom2d handles remain later profiles rather than B07 exit blockers.
- Added the `GpXyz` value facade and ABI 1.19 bridge for OCCT vector algebra, including
  cross/dot/modulus/normalize and fail-closed zero normalization.
- Added the `GpLine` value facade and ABI 1.20 bridge for default/create/reverse, point
  distance, and line angle; zero direction remains an OCCT construction failure.
- Added the `GpCircle` value facade and ABI 1.21 bridge for default/create, radius,
  area, circumference, point distance, and axis/radius construction failures.
- Added `GpAx2Value` and `GpPlane` value facades with ABI 1.22/1.23 bridges for
  right-handed orientation, plane distance, signed distance, and fail-closed normal
  construction.
- Added `GpAx3Value` with ABI 1.24/bridge 0.25.0 for copied coordinate-system axes,
  OCCT directness evaluation, and parallel/zero-direction construction failures.

### Completed safe adaptor/property capability milestone

- Added `GPropProperties` over an opaque registry-validated `GProp_GProps` owner.
- Added shape-driven linear/surface/volume computation, mass, centre of mass, inertia
  matrix reads, clone, and density-weighted composition; mode, density, and index
  validation remain fail-closed.
- `BRepGProp` and native property state remain inside the bridge; managed code receives
  copied values and owns disposal through `GPropsHandle`.
- Added `EdgeCurveSnapshot` over a call-local `BRepAdaptor_Curve`: curve type,
  finite first/last parameters, and copied endpoint values cross the ABI.
- Added `FaceSurfaceSnapshot` over a call-local `BRepAdaptor_Surface`: surface type and
  copied UV bounds cross the ABI, with an explicit restricted/unrestricted flag.
- Wrong topology kinds fail with `TypeMismatch`; snapshots have no native lifetime and
  remain usable after the source shape is disposed. Release/Debug, fixed-layout,
  generated-freshness, and alpha.33 clean-consumer evidence closes B08 for this profile.
- Borrowed adaptor objects, underlying curve/surface handles, and broader GeomAdaptor/
  Adaptor2d views remain excluded; they are not hidden completion criteria for B08.

### Completed basic BRep construction capability milestone

- Added `ShapeFactory.CreateSphere` and `CreateCylinder` over native
  `BRepPrimAPI_MakeSphere`/`BRepPrimAPI_MakeCylinder` with finite-positive validation,
  OCCT exception containment, and normal owning `Shape` handles.
- Added straight-edge, polygon-wire, and planar-face builders with copied point buffers,
  kind validation, builder completion checks, and independent owning results. B09 is
  complete for its basic construction profile.

### Completed owning topology snapshot capability milestone

- Added `Shape.GetFaces()` and `Shape.GetSubShapes(ShapeKind)` over caller-owned native
  snapshot buffers. Face, edge, wire, and vertex copies are independent owning `Shape`
  values; no native iterator crosses the ABI, and returned children remain valid after
  parent disposal.
- Invalid kinds, empty snapshots, partial cleanup, parent disposal, and all four child
  kinds are covered; B10 is complete for its owning-snapshot profile.

### Completed basic modeling result capability milestone

- Added `Shape.Fuse` and `Shape.Cut` over native `BRepAlgoAPI_Fuse` and
  `BRepAlgoAPI_Cut` with validated input handles, contained OCCT failures, independent
  result ownership, and source-disposal independence.
- Added `Shape.Common` over `BRepAlgoAPI_Common` with the same owning-result contract.
- Added `Shape.DistanceTo` over native-local `BRepExtrema_DistShapeShape`; managed code
  receives only minimum distance, one copied point pair, and solution count. Layout,
  null/disposed failures, source independence, and alpha.34 package gates pass.
- B11 is complete for the declared owning/value result profile. Projections, offsets,
  fillets, feature builders, support topology, and algorithm history remain later profiles.

### Completed initial mesh bulk-transfer capability milestone

- Added `Shape.CreateMesh` with a two-call count/snapshot contract over
  `BRepMesh_IncrementalMesh` and face-local `Poly_Triangulation` values.
- Added caller-owned copied positions, face normals, winding-corrected triangle indices,
  finite-positive deflection validation, 32-bit capacity checks, and no native array or
  triangulation pointer exposure.
- B13 is complete for this first bulk-transfer profile; Poly algorithms, RWMesh formats,
  stable shared vertex identity, zero-copy views, and benchmark gates remain pending.

### Completed owning-result healing capability milestone

- Added `Shape.Fixed` over native `ShapeFix_Shape::Perform` with contained OCCT
  diagnostics and an independent owning result.
- The B12 batch remains in progress: boolean failure/status detail, BOP history,
  ShapeFix/ShapeUpgrade mode and history contracts, and invalid/empty-shape fixtures
  are still required beyond these narrow result operations.
- Added `Shape.UnifiedSameDomain` over native `ShapeUpgrade_UnifySameDomain` with
  default edge/face unification, BSpline concatenation disabled, and independent
  result ownership. History and mode state remain native-local.
- Added explicit `ShapeFactory.CreateNull` diagnostics and native `IsNull` guards for
  Fuse, Cut, ShapeFix, and UnifySameDomain; invalid inputs now return stable
  `InvalidArgument` diagnostics before OCCT dereference.
- B12 is complete for the owning-result/no-history profile: Cut, ShapeFix, and
  UnifySameDomain results survive input disposal, while BOP/ShapeFix/ShapeUpgrade
  history, modes, and modified/generated/deleted maps remain explicitly native-local.

### Completed geometry-exchange capability milestone

- Added `ShapeExchange.ReadIges` over `IGESControl_Reader` to complement the existing
  BRep-mode IGES writer. Reader and transfer-root state remain native-local; one owning
  shape is returned after file/transfer/null checks.
- Added `ShapeExchange.ReadStl` over `StlAPI_Reader`; the one-shot result is a faceted
  owning shape and remains independent after source disposal.
- Added one-shot `DEOBJ_Provider`, `DEGLTF_Provider`, and `DEVRML_Provider` geometry
  read/write loops plus `DEPLY_Provider` write. OCCT 8.0.1 explicitly does not support
  PLY import. Providers and document/scene state remain native-local.
- Release/Debug native/managed builds, Generator 32/32, Runtime 65/65, generated
  freshness, and the alpha.35 clean consumer pass. The package writes OBJ/PLY/GLB/VRML,
  reads OBJ/GLB/VRML, and loads the 41-DLL closure from `occt`.
- B14 is complete for geometry-only exchange. Generated provider/options APIs, XDE
  metadata/document surfaces, richer format options, and broader licensed fixtures are
  explicit remaining B work rather than hidden completion evidence.

### Completed OCAF document/label capability milestone

- Added `OcafDocument` over an owning native application/document pair, with BinOcaf
  create/open/save and an application-local TKBin/TKBinL persistence closure.
- Added `OcafLabel` as a stable TDF entry parent-bound to its document; no `TDF_Label`
  layout, node pointer, or independently released label crosses the ABI.
- Added `OcafTransaction` begin/commit/abort with mutation guards and abort-on-dispose.
  UTF-8 `TDataStd_Name` values are copied in both directions.
- OCCT abort rolls attributes back but retains newly allocated empty label nodes in
  memory; the contract and tests preserve this fact, while default BinOcaf save omits
  the empty labels.
- Release/Debug native/managed builds, Generator 32/32, Runtime 66/66, freshness, and
  alpha.36 clean consumer pass. The package creates, commits, saves, reopens, and reads
  names with 43 DLLs loaded below `occt`.
- B15 is complete for the document/label profile. Broader TDataStd attributes,
  references, child iterators, undo/redo surfaces, XML persistence, and generated OCAF
  declarations remain in the B long-tail workstream.

### Completed XDE metadata/assembly capability milestone

- Added `XdeDocument`/`XdeLabel` on the B15 owner/stable-entry contract with explicit
  transactions, BinXCAF save/open, and STEPCAF read/write.
- Added top-level shapes, assemblies, component occurrences, referred-part entries,
  free/component snapshots, independent shape/location owners, and same-document guards.
- Added copied names, effective RGBA, multiple layer names, and physical-material records.
  Effective color writes Gen/Surf/Curv and reads in that order because STEPCAF may
  normalize overall colors into surface or curve channels.
- Release/Debug native/managed builds, Generator 32/32, Runtime 67/67, freshness, and
  alpha.37 clean consumer pass. The same shape/metadata/assembly is verified in memory,
  after BinXCAF open, and after STEPCAF import with 44 DLLs below `occt`.
- B16 is complete for the metadata/assembly profile. Visual materials/textures, GD&T,
  SHUO, named properties, arbitrary XCAF attributes, and generated tool classes remain
  explicit remaining B workstreams.

### Completed Windows visualization-core capability milestone

- Added `OcctViewer` as a creating-thread-affine owner of the display connection,
  OpenGL driver, V3d viewer, AIS context, view, and application-owned `WNT_Window`.
- Added parent-bound `ViewerPresentation` IDs for display, show, hide, and remove. AIS
  objects and selector pointers remain native-local; selection crosses as copied IDs.
- Added resize, fit, redraw, mouse detection, and click-selection forwarding without a
  native-to-managed callback or a cross-thread dispatch promise.
- Added a sixth interactive sample with a `CS_OWNDC` Win32 window and standard message
  loop. Automated tests use a real off-screen HWND; the interactive UI was compiled but
  not manually launched during this validation.
- Release/Debug native/managed builds, Generator 32/32, Runtime 68/68, 12-file freshness,
  and the alpha.38 clean consumer pass with ABI 1.30/bridge 0.38.0 and 45 DLLs in `occt`.
- B17 is complete for this Windows core profile. Camera/style/light/clip-plane APIs,
  native callbacks, off-screen buffers, and richer AIS/Prs3d/SelectMgr declarations are
  explicit remaining B work.

### Completed optional-integration classification milestone

- Added a versioned six-profile manifest and deterministic audit integrated into normal
  Release/Debug builds.
- Confirmed the WNT/OpenGL visualization profile is available from the pinned artifact.
- Classified IVtk as `BlockedExternalDependency`: 23 OCCT IVtk headers and TKIVtk DLLs
  exist, but required VTK 9.4 development headers and runtime DLLs are absent.
- Classified OpenGL ES as `BlockedExternalDependency` because EGL/GLES headers and
  `libEGL.dll`/`libGLESv2.dll` are absent despite TKOpenGles being present.
- Classified Draw as `IgnoredByDesign` for public runtime packaging, Cocoa/X11 as
  `UnavailablePlatform`, and `NCollection_Haft.h` as `ExcludedLanguage` (C++/CLI).
- ADR-0047 isolates future `OcctSharp.IVtk` and
  `OcctSharp.Visualization.OpenGles` packages. Both Release and Debug report 6/6 profile
  classifications matching the pinned dependency state.

### Completed full-inventory classification foundation inside B

- Added a separate final classifier that preserves raw generator states while assigning
  every discovered stable ID to `SupportedUnselected`, `Skipped`, `Manual`, or `Blocked`.
- Added LT001-LT004 reason codes for declaration projection, instance ownership, return
  projection, and parameter projection. No blocked or eligible-unselected item is counted
  as emitted/generated coverage.
- Added HD001-HD005 final states for VTK, EGL/GLES, RapidJSON, C++/CLI, and missing
  generated OCCT headers. All 7,090 catalogued headers have a disposition.
- Generalized the inventory preamble with `StepData_Factors.hxx`, recovering 11
  `StepToTopoDS_*` false failures and restoring 7,058 parsed headers/116,214 declarations.
- Two BatchSize=128 scans produce identical 50,117,128-byte reports with SHA256
  `C8C7EC3913F97068138E162C16ADB187EC590446A5F3EF2E33815AB48B586CEA`.
- Final declaration classification is 10,486 supported-unselected, 27,310 skipped,
  78,418 blocked, and zero pending; header classification is 7,090/7,090 with zero
  pending and zero HD099.

### Completed initial StepBasic scalar/shared entity milestone

- Generalized enum discovery/emission now records explicit values and underlying types,
  resolves qualified and unqualified enum spellings deterministically, and emits typed
  public enums through the verified 32-bit `TM004` ABI.
- Added ten generated `StepBasic` intrusive shared-handle scopes: Address, Date,
  CalendarDate, OrdinalDate, WeekOfYearAndDayDate, LocalTime,
  CoordinatedUniversalTimeOffset, DimensionalExponents, Person, and SiUnit.
- Generated coverage rises from 58/3,062 to 171/3,406 across 13 manifest-owned files.
  The full inventory now reconciles manifest IDs as `Emitted/EM001`: 171 emitted,
  10,338 supported-unselected, 27,310 skipped, and 78,395 blocked.
- Release/Debug native and managed builds pass with Generator 40/40 and Runtime 73/73.
  Runtime coverage includes scalar/boolean/enum round-trips, shared mutation,
  1-to-2-to-1 reference counts, RTTI, idempotent disposal, and disposed-use rejection.
- `0.1.0-alpha.39` advances to ABI 1.31/bridge 0.39.0. Its clean consumer loads the
  unchanged 45-DLL `occt` closure and executes a generated StepBasic clone/enum path.

### Completed StepBasic package shared-entity milestone

- Configuration schema 1.5 adds deterministic header patterns and a package-level
  shared-handle scope. `StepBasic_*.hxx` expands only discovered public
  `Standard_Transient` descendants with a usable public default constructor; configured
  exclusions remain explicit and stable.
- Generated StepBasic coverage grows from ten to 129 public managed shared-entity types.
  The committed 13-file manifest now owns 333 stable IDs across a 5,503-declaration
  selected scope; 453 declarations are safely supported and 333 are emitted.
- Full inventory reconciles 333 `Emitted`, 10,177 `SupportedUnselected`, 27,310
  `Skipped`, and 78,394 `Blocked` declarations. Classification remains complete while
  batch B remains open.
- Release and Debug pass Generator 41/41 and Runtime 75/75. Runtime and package tests
  construct every generated StepBasic type, clone it, verify intrusive reference counts,
  dispose both owners, and retain the focused scalar/boolean/enum mutation paths.
- `0.1.0-alpha.40` advances to ABI 1.32/bridge 0.40.0. Its clean package consumer loads
  all 45 native DLLs below `occt/` and exercises all 129 generated StepBasic types.
- Repository Sample builds now have an incremental native-only bootstrap under ADR-0051;
  a simulated missing Debug bridge was rebuilt and copied to Sample output before an
  English entity-creation workflow loaded OCCT successfully.

### Completed high-frequency common modeling milestone

- Added cone and torus solid builders; extrusion and revolution; all-edge and single-edge
  fillet/chamfer; skin/join offset; shape/shape section; public subshape occurrence count;
  copied finite bounding boxes; and full-topology validity checks.
- Builder, algorithm, indexed-edge, bounding, and analyzer objects remain native-local.
  Shape results are independent registered owners, bounding boxes are fixed 48-byte value
  copies, and source disposal does not invalidate results.
- Configuration schema 1.6 adds validated stable-ID manual-binding declarations. Missing
  stable IDs, duplicates, malformed special-case references, and emitted/manual overlap
  fail closed. The selected scope has 9,567 declarations: 333 emitted, 18 manual,
  740 supported, 6,781 pending, and 2,028 skipped.
- Full inventory reconciles 333 `Emitted`, 18 `Manual`, 10,177 `SupportedUnselected`,
  27,310 `Skipped`, and 78,376 `Blocked`; declaration/header classification remains complete.
- Release and Debug pass Generator 44/44 and Runtime 81/81. The alpha.41 clean consumer
  loads 47 DLLs from `occt/` and exercises the new modeling families with ABI 1.33 and
  bridge 0.41.0.

### Current large high-value API workstream inside B

- Added circle, ellipse, arc, Bezier, and interpolated edge construction; edge length,
  point/tangent evaluation, and closest-point projection; face point/normal evaluation
  and closest UV projection.
- Added copied topology-adjacency maps, loft, pipe, sewing, wedge, thick-solid, and
  copied Boolean modified/generated/deleted history summaries with owning results.
- Added composable `XdeDocument.ImportStep`; the assembly Sample now imports STEPCAF
  roots into an owned document and composes assemblies with normal XDE operations.
  `StepAssembly` remains only as an obsolete compatibility facade.
- Schema 1.6 expands the selected scope to 10,956 declarations and records 43 new
  SC-033 stable IDs, for 61 accepted manual declarations in total. Generated emission
  remains 333 declarations; selected safe support is 852 declarations.
- Release and Debug pass Generator 44/44, Runtime 90/90, and dependency profiles 6/6.
  Full inventory reconciles all 116,214 declarations and 7,090 headers; freshness passes;
  the alpha.42 clean consumer executes the new APIs with 47 DLLs under `occt/`. Complete
  release-check and documentation checks remain to be run for this changed evidence chain.

### Current generated Geom/Geom2d expansion inside B

- Generalized package-level shared-handle selection from StepBasic to the complete
  `Geom_*.hxx` and `Geom2d_*.hxx` header families. The same O004 registry, intrusive
  retention, clone, RTTI, exception containment, and disposal contract is reused.
- Added eight generated public types: `Geom2dCartesianPoint`, `Geom2dDirection`,
  `Geom2dTransformation`, `Geom2dVectorWithMagnitude`, `GeomDirection`, `GeomPlane`,
  `GeomTransformation`, and `GeomVectorWithMagnitude`.
- Generated constructors and supported scalar/value-copy members cover coordinates,
  direction/vector magnitudes, mutation/normalization, plane evaluation/reversal, and
  2D/3D transformation form, scale, matrix values, inversion, power, mirror, scale, and
  translation where the pinned headers expose a safe mapped signature.
- Selected discovery is now 12,633 declarations with 400 emitted IDs, 61 accepted manual
  IDs, and 1,346 safely supported declarations. Full inventory reconciles 400 emitted,
  61 manual, 10,110 supported-unselected, 27,310 skipped, and 78,333 blocked.
- Release and Debug pass Generator 44/44 and Runtime 93/93. The alpha.43 clean consumer
  exercises all eight new types with ABI 1.35/bridge 0.43.0 and 47 DLLs under `occt/`.
- The complete alpha.43 local release check passes after temporarily staging only the six
  changed manifest/shared-handle generated files for the HEAD-based freshness gate. Clean
  source regeneration produced 13 byte-identical generated files; API comparison against
  alpha.38 reports 1,387 additions, zero removals, and no breaking change. The temporary
  staging was removed after the check; this does not create a batch-B commit boundary.

### Completed generated mesh, analysis, and healing workstream

- Added semantic `IsAbstract` facts to binding-model schema 1.2 and made package-level
  shared-handle selection exclude abstract records before emission. This generalized
  rule replaced the initial compile-discovered exclusions for three abstract BRepMesh
  bases; no per-class deny list was used.
- Added header-pattern and package-level generation for `BRepMesh`, `Poly`,
  `ShapeAnalysis`, `ShapeFix`, and `ShapeUpgrade` under the existing TM006/O003
  intrusive shared-owner contract.
- Added 61 public generated types and 375 emitted stable IDs: 14 BRepMesh types, six
  Poly types, four ShapeAnalysis types, 13 ShapeFix types, and 24 ShapeUpgrade types.
  The manifest now owns 775 stable IDs across a 16,633-declaration selected scope.
- Added representative runtime coverage for mesh status, triangulation parameters,
  analysis conversion, healing/upgrade tolerance state, retained clones, RTTI,
  idempotent disposal, and disposed-use rejection.
- Release and Debug pass Generator 44/44 and Runtime 96/96. The alpha.44 clean package
  consumer passes with 47 DLLs under `occt/`, ABI 1.36, bridge 0.44.0, and direct calls
  through all five new package families.
- Full inventory remains classification-complete at 116,214 declarations and 7,090
  headers: 775 emitted, 61 manual, 9,738 supported-unselected, 27,310 skipped, and
  78,330 blocked; SHA256 `556A1C3DC664AE44DE2CAF716BB980F93373BBB4D70326A4FC1F09A7CEC0FB9D`.
- The complete alpha.44 release check passes twice for the changed evidence chain.
  Clean-source regeneration produced 13 byte-identical files; the alpha.38 API baseline
  comparison reports 2,160 additions, zero removals, and no breaking change. Release
  metadata and both Git whitespace gates pass. The temporary six-file staging used by
  the HEAD freshness gate was removed after validation and no files remain staged.

### Completed generated STEP model expansion inside B

- Added header-pattern and package-level generation for `StepGeom`, `StepRepr`,
  `StepShape`, and `StepVisual` on top of the existing `StepBasic` profile. The four
  packages contribute 85, 79, 92, and 110 concrete public shared-handle types.
- The selected semantic scope is now 22,879 declarations with 1,594 emitted IDs,
  61 accepted manual IDs, 2,576 supported declarations, and 4,568 skipped declarations.
  Emitted coverage is 6.9671%; emitted plus manual coverage is 7.2337%.
- Representative Cartesian point, representation item, box-domain, and RGB-colour
  wrappers pass scalar mutation, RTTI, clone retention, and idempotent disposal checks.
- Release and Debug pass Generator 44/44 and Runtime 98/98. The alpha.45 clean package
  consumer passes with 47 DLLs under `occt/`, ABI 1.37, bridge 0.45.0, and direct calls
  through all four new STEP package families.
- The complete alpha.45 release check passes. Full inventory remains classification-complete
  at 116,214 declarations and 7,090 headers: 1,594 emitted, 61 manual, 8,934
  supported-unselected, 27,310 skipped, and 78,315 blocked; SHA256
  `1CFD48B7967CE4F2EB5FAA1D43453886509D9FF8E153D5FDCB7ECEF259E1ADE4`.
- Clean-source regeneration produced 13 byte-identical generated files; the alpha.38 API
  baseline comparison reports 5,251 additions, zero removals, and no breaking change.

### Completed cross-generated shared-handle wave inside B

- Generalized generated `Handle<T>` parameters and returns when both source and target
  wrappers are selected `Standard_Transient` descendants. Nullable managed inputs map to
  null OCCT handles; non-null inputs use target-specific registry validation; non-null
  results allocate independent retained target wrappers.
- Kept package admission fail-closed: a type still needs an independently supported
  value-copy constructor and cannot be selected solely through a handle-dependent constructor.
- Fixed the managed null marshalling boundary after focused runtime evidence showed that
  the source-generated `SafeHandle` marshaller rejects null before P/Invoke. Raw handle
  arguments now use explicit `nint`, while managed disposal checks and native registry
  validation remain mandatory.
- The manifest now owns 2,235 stable IDs, a gain of 641 from alpha.45. Selected emitted
  coverage is 9.7688%, and emitted plus 61 accepted manual declarations is 10.0354%.
- Release and Debug pass Generator 44/44 and Runtime 99/99. Runtime tests cover null
  round-trip, setter/getter relationships, source-disposal independence, independent
  returned-wrapper disposal, and disposed-argument rejection.
- The complete alpha.46 release check passes: the 47-DLL clean package consumer loads
  ABI 1.38/bridge 0.46.0 and directly exercises cross-generated handles; 13-file clean
  regeneration is byte-identical; the alpha.38 API diff reports 5,892 additions and zero
  removals.
- Full inventory remains classification-complete: 2,235 emitted, 61 manual, 12,890
  supported-unselected, 27,310 skipped, and 73,718 blocked; SHA256
  `04FCD3F9888802E5FE6BA557D98F1D203B412BABCAFDB5044A3A7A8354B03180`.

### Completed extended STEP entity wave inside B

- Added discovery and package-level generation for `StepAP203`, `StepAP214`, `StepAP242`,
  `StepDimTol`, `StepElement`, `StepFEA`, and `StepKinematics`. The selected semantic
  scope increased from 22,879 to 28,836 declarations and the generated manifest from
  2,235 to 3,076 stable IDs, a net gain of 841.
- `StepData` headers remain selected for discovery and classification, but the package is
  not treated as ordinary constructible entities. The supplied OCCT binary lacks linkable
  implementations for two declared `StepData` members; KI-013 records the package-level
  boundary rather than introducing per-class generation exclusions.
- All 249 new public constructible wrappers are runtime-tested through construction,
  clone retention, reference count, RTTI, and disposal. Focused tests additionally cover
  AP214-to-StepRepr relationships, FEA relationships, Element scalar state, and
  Kinematics scalar state.
- Release and Debug pass Generator 44/44 and Runtime 101/101 with zero build warnings or
  errors. Current discovery/coverage/diagnostics hashes are
  `C4B1A53DFCB1D5B207A43BC37C574EDAD8317D3264F184DA77431A25DC037278`,
  `6ADBA881B09D444003AB91458F11BF8E27047CE12C158DD920227C9BA872695A`, and
  `C1EE17E035FE7F92A0179D7E8E860CE13EA72C5F44A6ED0B1D8C6DE459DDD1D8`.
- The complete alpha.47 release check passes. The clean consumer loads 47 DLLs at ABI
  1.39/bridge 0.47.0; clean regeneration produces 13 byte-identical files; the alpha.38
  API diff reports 8,316 additions and zero removals. Full inventory is classification-
  complete at 3,076 emitted, 61 manual, 12,102 supported-unselected, 27,310 skipped, and
  73,665 blocked; SHA256
  `A4ED928E835A7C244D3FD5FD77C70DCC2B50E953E3B9344B4A3B20360402F1DF`.

### Alpha.38 release-engineering checkpoint; batch B exit remained open

- Added a 606-signature schema-1.0 managed public API baseline and compatibility diff;
  the current alpha.38 assembly reports zero additions and zero removals.
- Added one `eng/release-check.ps1` entry point covering Release/Debug, freshness,
  clean consumer, full inventory regeneration, byte-identical clean-source regeneration,
  API compatibility, release metadata, and both Git whitespace checks.
- Added root GitHub Actions CI: generator tests run without OCCT; the complete Windows
  job acquires an archive only from configured URL/SHA256 variables and runs the same
  release entry point. Hosted execution remains `NOT RUN`.
- Added CycloneDX SBOM, provenance, fixed-order SHA256 checksums, release notes,
  third-party review status, and a machine-readable gate report. The local package and
  45 native DLLs are recorded, but unresolved non-OCCT versions/licenses remain blocked.
- The earlier local release-engineering validation passed Generator 37/37 and Runtime 68/68 in Release and Debug,
  6/6 dependency profiles, 12-file clean regeneration, alpha.38 clean consumer, API
  diff 0/0, complete inventory classification, JSON parsing, and whitespace checks.
- At the alpha.38 checkpoint, `releaseEngineeringImplemented` was true while
  `batchImplementationComplete` and `publicReleaseReady` were false: bindable emission,
  PD-012, third-party legal review, hosted CI, signing, and publication were not silently
  waived.

## Completed

- Outer documentation and inner code-workspace boundary established.
- One Git repository initialized at the outer root on branch `main`.
- .NET SDK locked to 10.0.400; all managed projects target `net10.0` and `win-x64`.
- OCCT 8.0.1 combined VC14 x64 Debug/Release distribution recorded as the initial
  dependency baseline with representative SHA256 hashes.
- Visual Studio 2026 CMake/MSVC environment resolved automatically by `eng/build.ps1`.
- Native C ABI bridge builds against OCCT in Debug and Release.
- ABI, bridge, and loaded OCCT version identity queries implemented.
- Stable status enum, native exception containment, and thread-local UTF-8 diagnostics implemented.
- Opaque owned shape handle, OCCT box creation, face enumeration, native release,
  managed `SafeHandle`, `Shape`, and `ShapeFactory` implemented.
- Native shape handles are registered while live; shape operations reject stale or
  arbitrary non-null handles before dereference, and repeated native release is safe.
- ABI 1.5 and bridge 0.6.0 add `InvalidHandle` status 8 with a thread-local diagnostic;
  Release/Debug runtime tests cover stale-handle access and repeated release.
- ABI 1.6 and bridge 0.7.0 add the experimental `SharedTransient` wrapper over
  OCCT `opencascade::handle<Standard_Transient>` with clone, null, reference-count,
  and release semantics. Release/Debug tests verify 1→2→1 retention and null copies.
- ABI 1.7 and bridge 0.8.0 add OCCT RTTI `TypeName` and `IsKind` checks through the
  shared wrapper, including exact derived and `Standard_Transient` base validation.
- ABI 1.8 and bridge 0.9.0 add `TypeMismatch` plus a checked derived shared-handle
  cast. `TryCastDerived` returns no wrapper for null/wrong kinds, while `CastDerived`
  throws `InvalidCastException`; successful casts retain one intrusive reference.
- ABI 1.9 and bridge 0.10.0 generate a real typed OCCT shared wrapper for
  `Geom_CartesianPoint`. `TM006`, shared-handle eligibility, configured schema 1.3,
  per-type registries, RTTI, retained clone, coordinate/value methods, and disposal
  behavior are generated rather than manually wrapped.
- ABI 1.10 and bridge 0.11.0 generate eight `TopoDS_Shape` value-semantic operations:
  copy, null state, kind, orientation, reversal, `IsPartner`, `IsSame`, and `IsEqual`.
  `TM007`, configuration schema 1.4, and ADR-0016 preserve independent wrapper-owned
  C++ values with normal shared internal `TShape` semantics and no C++ layout crossing.
- ABI 1.11 and bridge 0.12.0 generate eight checked typed topology casts and managed
  wrappers for `Compound`, `CompSolid`, `Solid`, `Shell`, `Face`, `Wire`, `Edge`, and
  `Vertex`. `Standard_TypeMismatch` maps to ABI 9; wrong-kind `TryCast` is false and
  successful typed values remain independent after source disposal.
- ABI 1.12 and bridge 0.13.0 add the B05.1 opaque `gp_Trsf` value bridge with clone,
  inversion, composition, matrix reads, finite validation, and shape application.
- ABI 1.13 and bridge 0.14.0 add the B05 `TopLoc_Location` portion with composition,
  inversion, conversion, identity checks, and absolute/relative placement.
- ABI 1.14 and bridge 0.15.0 complete B05 with opaque `gp_Vec`, `gp_Dir`, `gp_Ax1`,
  and `gp_Mat` values plus vector/axis transform creation; all four families use
  registry validation and independent owning results.
- B05 adds friendly/manual bridge coverage only; generated declaration coverage remains
  58/3,062 by design and is not inflated by these hand-authored opaque wrappers.
- ABI 1.15 and bridge 0.16.0 add the first B06 string/collection wave: UTF-8 and UTF-16
  OCCT string owners plus `NCollection_Sequence<double>` with explicit buffer/index rules.
- ABI 1.16 and bridge 0.17.0 add opaque `NCollection_Array1<double>` and
  `NCollection_Vector<double>`/`NCollection_DynamicArray<double>` value collections with
  explicit lower-bound, zero-based managed indexing, clone, mutation, and lifetime rules.
- ABI 1.17 and bridge 0.18.0 add opaque integer-key `NCollection_DataMap<int,double>` and
  `NCollection_IndexedMap<int>` values with key/index lookup, mutation, clone, and release.
- ClangSharp/libClangSharp semantic discovery implemented with versioned generation config.
- Controlled Clang fixture and real OCCT header discovery validated.
- Deterministic model and OCCT discovery reports validated across consecutive runs.
- Documentation navigation consolidated under the root `README.md`; nested README
  files were replaced by `docs/DOCUMENTATION_INDEX.md` and the existing build guide.
- Canonical declarations now represent native signatures, structured parameter and
  return types, per-indirection const/reference facts, access, method qualifiers,
  inheritance, template/OCCT handle facts, and source package/toolkit identity.
- Controlled semantic tests validate signatures, default parameters, per-layer const,
  references, inheritance, virtual/static method facts, and OCCT handle recognition.
- The selected real OCCT scope assigns all 3,062 declarations to 19 source packages
  and their source-confirmed toolkits.
- An ordered support-classification pass assigns stable `SK001`–`SK006` reasons and
  emits a deterministic summary without treating pending type work as supported.
- Central type rules `TM001`–`TM005` map verified integer, real, boolean, enum, and
  `gp_Pnt` value-copy forms while rejecting unsafe pointer/reference projections.
- Native ABI 1.1 and bridge 0.2.0 add ordinary STEP read/write, explicit STL meshing
  and output, BRep-mode IGES output, rigid transforms, and compound construction.
- One .NET 10 console project exposes five commands for entity creation, STEP/STL/IGES
  output, and transformed multi-STEP XDE assembly.
- The console entry point now presents an interactive menu and reads output/input paths
  with prompts; the five workflows remain separate English-named sample classes.
- All five interactive workflows were rerun with redirected user input: entity creation,
  STEP, STL, IGES, and seven-file metadata-preserving XDE assembly.
- Seven local STEP inputs were read, transformed, assembled into a 701-face compound,
  and written to a 2,412,254-byte STEP file.
- Native ABI 1.2 and bridge 0.3.0 add one-shot STEPCAF/XDE assembly exchange. Source
  and output XDE documents remain native-local while shape-label trees, colors, styles,
  names, layers, properties, physical materials, and supported assembly structure are
  copied into one transformed output assembly.
- The XDE sample consumed seven local STEP files, retained color/style records and four
  material-property records, and wrote seven `NEXT_ASSEMBLY_USAGE_OCCURRENCE` records.
- ADR-0007 resolves generated-source commit policy and raw/friendly naming separation:
  deterministic generated source and its ownership manifest are committed, raw managed
  bindings remain internal under `OcctSharp.Generated`, and friendly APIs remain curated.
- The first real generated binding selects `gp_Pnt(double,double,double)` by stable Clang
  ID, emits a native `OcctSharp_Point3d` value-copy ABI plus internal managed raw binding,
  and never projects the native C++ object layout.
- Generation writes three files through isolated staging, verifies SHA256 hashes, and
  removes stale output only when the previous manifest owns the path.
- The normal build now bootstraps the generator and regenerates before native configure/
  compile; `eng/verify-generated.ps1` verifies tracked output and a clean generated diff.
- Native ABI 1.3 and bridge 0.4.0 include the first generated export. Debug and Release
  runtime tests execute it and verify the 24-byte X/Y/Z value copy.
- A generalized eligibility pass promotes only constructors and static methods whose
  complete parameter/return mappings are proven value copies. The selected OCCT scope
  now has 147 supported candidates, 2,160 pending declarations, and 737 skipped declarations.
- The first emitter now selects the point constructor by supported semantic signature;
  its generated source still records the discovered stable ID for traceability.
- The generalized emitter now emits all 20 eligible `Precision::*` static scalar methods
  in addition to the `gp_Pnt(double,double,double)` constructor. It emits 21 declarations
  across four manifest-owned files, with native and raw managed overload ordinals ordered
  by normalized signature then stable ID.
- Native ABI 1.4 and bridge 0.5.0 add the generated `Precision` and `TopAbs` value-copy
  exports. `double` crosses directly, enums use validated `int32_t`, and
  `Standard_Boolean` is normalized to `int32_t` zero or one; the selected functions have
  no native object lifetime.
- Generation configuration schema 1.2 now declares static-method scopes explicitly,
  including source package, native prefix, header, export prefix, and managed prefix.
  Schema 1.1 remains readable with the default `Precision` scope.
- The configured emitter now emits three `TopAbs` enum static methods in addition to the
  20 `Precision` methods, for 24 generated declarations total. `TM004` maps enum inputs
  and returns through validated `int32_t`, with native enum casts generated at the C++ call.
- The next ownership-neutral expansion emits `gp_Pnt` default/copy constructors plus
  `gp::Resolution`, `TopLoc_Location::ScalePrec`, `Standard::GetAllocatorType`,
  `Standard_Dump::JsonKeyLength`, and `Standard_Failure::DefaultStackTraceLength`.
  These five static methods and two constructors bring the generated set to 31
  declarations (28 static, three constructors). `Standard::Purge` remains deliberately
  unselected because its process-wide side effects are not a value-copy contract.
- Configuration schema 1.3 and ADR-0013 add generated typed shared-handle scopes. The
  first `Geom_CartesianPoint` scope emits 11 constructors/members plus clone, RTTI,
  reference-count, registry validation, and release infrastructure across four new
  generated files, bringing the manifest to eight files and 42 source declarations.
- Configuration schema 1.4 and ADR-0016 add a fail-closed topology scope. The initial
  `TopoDS_Shape` scope emits eight declarations across four module-partitioned Topology
  files, bringing the manifest to twelve files and 50 source declarations.
- `AI_MIGRATION_LOOP_PROMPT.md` defines a re-entrant single-batch B execution state machine,
  recovery protocol, validation matrix, error handling, completion gates, and a stable
  `CONTINUE`/`BLOCKED`/`COMPLETE` footer for repeated AI polling.
- ADR-0017 records B04 typed topology ownership, checked conversion, and TypeMismatch
  behavior; the generated topology scope now emits eight additional cast declarations,
  bringing the selected generated set to 58 declarations.
- ADR-0008 selects one initial `OcctSharp` NuGet package and an application-local flat
  `occt` directory for the Windows x64 Release native closure.
- Package build assets copy 36 native DLLs below `occt` for build/publish. The managed
  assembly automatically loads the bridge from that exact path without changing `PATH`.
- `OcctSharp.0.1.0-alpha.5.nupkg` was restored into a package-only consumer, published,
  and executed successfully: ABI 1.8, bridge 0.9.0, OCCT 8.0.1, and six-face box creation.
- `OcctSharp.0.1.0-alpha.6.nupkg` advances to ABI 1.9/bridge 0.10.0 and passes
  package-only restore, publish, application-local native loading, and generated
  `GeomCartesianPoint` behavior with all 36 native DLLs below `occt`.
- `OcctSharp.0.1.0-alpha.7.nupkg` advances to ABI 1.10/bridge 0.11.0 and passes
  package-only restore, publish, application-local native loading, generated typed
  shared-handle behavior, and generated topology value semantics with the same 36-DLL
  `occt` layout.
- `OcctSharp.0.1.0-alpha.8.nupkg` advances to ABI 1.11/bridge 0.12.0 and passes
  package-only restore, publish, application-local native loading, generated typed
  shared-handle behavior, base topology behavior, and checked typed topology casts.
- `OcctSharp.0.1.0-alpha.9.nupkg` passed the first B05 package consumer; alpha.10
  passed the location portion, and alpha.11 passes the complete B05 Release package
  consumer with ABI 1.14/bridge 0.15.0.
- `OcctSharp.0.1.0-alpha.12.nupkg` passes the complete B05 plus B06 first-wave
  consumer with ABI 1.15/bridge 0.16.0 and all 36 native DLLs under `occt`.
- Generation now emits transient `coverage.json` and `diagnostics.json` under
  `artifacts/generator-reports/` through isolated report staging. They cover all 3,062
  declarations, including package/toolkit totals, status counts, skip reasons, source
  locations, emitted state, and stable `EL`/`EM`/`MN`/`SK` disposition codes.
- The build runs generation twice and verifies report byte stability. Release and Debug
  report hashes for the current scope are coverage `00DA3284880AAD6F31C32C45CCF3ED3E7056A4EA6A925DB4589AE3F5304CA1FA`
  and diagnostics `3C5102A282166174C2A8A44406ACEE8A8F9B535C398FB209E5DAC0D43C1A63E5`.
- Support classification now preserves declarations deliberately marked `Manual`, so
  future manual bridge coverage is not erased by classification.
- A separate full-library inventory workflow catalogs 7,090 public entry headers across
  407 filename-derived source packages. Semantic scanning uses deterministic batches,
  stable-ID deduplication, and recursive failure isolation without slowing normal builds.
- The first full-library audit scanned 7,058 headers and isolated 32 failures while
  preserving 116,214 unique declarations from successful batches. Configured common
  preamble headers removed 26 initial false failures. The remaining blockers are 19
  IVtk headers needing VTK, ten references to absent generated OCCT headers, one
  RapidJSON dependency, one C++/CLI-only header, and one OpenGL ES platform header.

## Current validated scope

- Managed: .NET 10, Windows x64.
- Native: VS 2026/MSVC 19.51 consuming OCCT 8.0.1 VC14 x64 binaries.
- Generator input: the schema 1.12 selected Windows-core closure in
  `config/generation.json`, resolved against the pinned OCCT 8.0.1 headers.
- Discovery output: 116,263 unique normalized declarations, including 16,353 emitted
  and 534 reconciled manual stable IDs.
- Runtime workflow: create/transform/compound shapes; count faces; STEP round-trip;
  STL/IGES output; geometry and XDE STEP assembly; detailed mesh and validation reports;
  ShapeFix repair comparison; XCAF validation properties, nested occurrence/world
  placement, STEPCAF options; derivatives/pcurves/trim/wire/reshape/adjacency; selective
  STEP sessions and units; viewer appearance, whole/subshape selection, owning selected
  topology, camera and application input; Batch D copied XDE presentation identity,
  exact owning detection, area selection/filtering, reversible isolate, subshape review
  styling, camera conversions, clipping/review aids, and screenshots; Batch E exact
  inspection, complete PMI/reference transactions, AP242/BinXCAF saved views, four
  viewer-owned dimension kinds, and screenshots; Batch F copied Bezier/B-spline curve/
  surface definitions and immutable edits, interpolation/approximation, profiles,
  offsets/fills/splits, controlled loft/pipe, sew/heal, STEP/XDE, mesh/measurement,
  real-HWND selection, and screenshots; Batch G technical drawing/SVG; Batch H grouped
  mesh/LOD/material/scene interchange; Batch I copied typed document state, dependency
  graphs, named history, undo/redo/savepoints, BinOcaf/XmlOcaf/BinXCAF/XmlXCAF, STEP/XDE,
  and source disposal; Batch J feature modeling/history/recovery; Batch K assembly edits,
  graph/BOM/references/effective metadata/rollups/history/review; Batch L occurrence-
  aware bounds/interference/clearance/containment/incremental/review; validate errors,
  ownership, threading, and disposal.

## Current gaps and deferred work

- All Batch C gap denominators pass: 14/14, 7/7, 8/8, and final 15/15. Batch C is complete
  for this finite routine CAD workflow contract.
- Batch D is complete at 24/24. Built-in filters, standard V3d/Graphic3d review features,
  copied model identity/owning picks, subshape styles, and durable screenshots pass the
  complete repository-runtime and clean-package workflow.
- Batch E is complete at 24/24. Exact measurement, semantic PMI/reference graphs,
  transactional mutation, AP242/BinXCAF/saved views, four viewer annotation kinds, and
  screenshot gates pass together in repository runtime and the clean package consumer.
- Batch F is complete at 24/24. Definition/edit, interpolation/approximation, profile/
  offset/fill/split/loft/pipe-shell/repair, STEP/XDE, viewer evidence, and clean-package
  gates pass together; no curve-only or surface-only fragment was counted as completion.
- Batch G, H, and I are complete at 24/24 each. Their technical-drawing, advanced-mesh/
  scene, and document-state/history/persistence gates pass in repository runtime and the
  clean package consumer without family fragments being counted as completion.
- Batch J and K are complete at 24/24 each. Their feature-modeling/history/recovery and
  assembly-authoring/BOM/reference/occurrence gates pass in repository runtime and the
  clean package consumer without family fragments being counted as completion.
- Batch L is complete at 24/24. Its occurrence expansion, AABB/OBB, broad/exact phase,
  filtering, pair classification/matrix/aggregation/diagnostics, incremental, STEP/XDE,
  viewer, lifetime, and clean-package gates pass together.
- Batch M is complete at 24/24. Presentation-local transforms, the complete parent-bound
  manipulator policy, custom/mouse preview, apply/cancel, rigid occurrence relocation,
  named history, undo/redo, DMU recheck, STEP/XDE, real HWND, lifetime, and clean-package
  gates pass together.
- Custom rendering pipelines, arbitrary callbacks, optional integrations, low-frequency
  schema entities, exhaustive mesh attributes, and unrelated long-tail APIs remain out of
  scope for D.
- Hosted full release execution, signing, credentials, and NuGet publication remain
  separate release-readiness work and require explicit authorization.
- Automated OCCT source builds on a clean machine remain deferred; the committed,
  manifest-verified Windows x64 runtime is the current clone-and-run baseline.

## Next tasks

1. Preserve ADR-0074's managed module/facade ownership and one native DLL. Regenerate
   facade forwarders whenever exported module types move; do not split the native bridge
   until a separate ADR proves cross-DLL registry, allocator, validation, and release routing.
2. Start no further product implementation until a new cross-family gap inventory and
   whole-letter batch ADR lock its finite denominator and complete dependency closure.
3. If publication is authorized later, run hosted release, signing, and NuGet publication
   as separate release-readiness work without rewriting completed Batch evidence.

## Do not change without an ADR

- One Git repository at the outer root and all code-related files under inner `OcctSharp/`.
- .NET 10-only initial managed target.
- Generated/manual separation.
- C ABI boundary between managed code and OCCT C++.
- Canonical binding model between AST parsing and emitters.
- Ownership rules O001–O012.
- Native exception containment and ABI-major compatibility check.
- Fact-based validation status vocabulary.

## Last validation

| Check | Result | Evidence |
|---|---|---|
| Git root and ignore boundary | PASS | Outer `.git`; build output and local settings ignored |
| Release native build | PASS | `eng/build.ps1 -Configuration Release` |
| Release managed build | PASS | Preview.10 full 19-project solution, 0 warnings, 0 errors |
| Debug native/managed build | PASS | Preview.10 full 19-project solution, 0 warnings, 0 errors |
| Generator unit tests | PASS | ADR-0062 Release and Debug `eng/build.ps1`: 91/91 |
| Runtime/lifetime tests | PASS | Preview.11 Release and Debug builds: 151/151; native integration tests serialize real-HWND fixtures |
| Controlled semantic Clang parse | PASS | Record, method, constructor, and enum discovery |
| OCCT semantic discovery | PASS | Selected model: 116,263 declarations; 542 configured manual stable IDs reconciled |
| Full OCCT header catalog | PASS | 7,090 entry headers: 7,084 `.hxx`, 6 `.h`, 407 filename-derived packages |
| Full OCCT semantic inventory | BLOCKED | 7,058/7,090 headers semantically scanned; 32 named dependency/artifact failures retain stable dispositions |
| Full-inventory classification | PASS | 16,353 emitted, 542 manual, 0 supported-unselected, 49,344 skipped, 50,033 blocked; 116,272/116,272 declarations and 7,090/7,090 headers classified; current report SHA256 `71E921851AF636875BCA5BBAABE1B673521071A86873E6A8B538683CEAD9C4C1` |
| Discovery determinism | PASS | Two-run SHA256 `F2AF680FA367BAA831AF6891C06A651A2F241F7E15C97972D6060272157294C9` |
| Model determinism | PASS | Two runs SHA256 `B4C30059AE03D16D78F032ADACD3FCD0BF674D3BE8F203FD924523EE611F9DA1` |
| Documentation navigation | PASS | Preview.6 closeout: 130 tracked/untracked source Markdown files checked; zero broken local targets |
| Structured canonical model compile | PASS | Generator Release build, 0 warnings and 0 errors |
| Structured canonical model tests | PASS | 3 generator tests; signature, qualifier, inheritance, template/handle facts |
| Structured OCCT fact inventory | PASS | Binding-model schema 1.3 retains abstract-record facts and fail-closed product-module identity in the selected semantic model |
| Source package/toolkit identity | PASS | 22,879 of 22,879 declarations classified in the selected scope |
| Support classification tests | PASS | 2 tests; rule order, stable codes, complete/sorted summary |
| Selected-scope support summary | PASS | Current Preview.11 surface has 16,353 emitted plus 542 accepted manual stable IDs and zero supported-unselected declarations |
| Simple binding eligibility | PASS | Value-copy constructors/static methods promoted; instance/pointer/unknown-lifetime cases remain pending |
| Coverage and diagnostics reports | PASS | 116,263 declarations; all states and stable disposition codes reported |
| Report determinism | PASS | Coverage SHA256 `ACA60FB80E77283E36AAF1E03D7E44F279C412FE4C8C69FF0336AD0E5AEBE07E`; diagnostics SHA256 `3645D952E23A68CD84CDDCB2BC0357C2A11CE1034F64D5467B03EF63FBF7374A` |
| Initial TypeMap tests | PASS | 9 tests; `TM001`–`TM007`, const-reference/top-level const input, unsafe pointer/reference rejection |
| Native TypeMap compile fixture | PASS | OCCT scalar and enum width assertions in Release native build |
| Configured generation scopes | PASS | Schema 1.13 reconciles 542 validated manual stable IDs through SC-049 with no duplicate IDs |
| Generated value-copy bindings | PASS | Three `gp_Pnt` constructors plus 28 scalar static methods (20 `Precision`, three `TopAbs`, and five ownership-neutral additions) emitted to native/managed source; compiled and called in Release and Debug |
| Generated typed shared binding | PASS | Selected Geom/Geom2d, STEP entity, mesh/Poly/analysis/healing public types with 3,076 manifest IDs; scalar/value/enum and cross-handle mutation, all 249 newly added entity constructors/clones, sharing, null, RTTI, retention, and disposal pass in Release and Debug |
| Generated topology binding | PASS | 8 base `TopoDS_Shape` operations plus 8 checked typed casts; solid/compound success, wrong-kind rejection, and source-disposal independence pass |
| B05.1 opaque `gp_Trsf` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, finite/index validation, and shape application |
| B05.2 opaque `TopLoc_Location` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, conversion, and absolute/relative shape placement |
| B05 complete opaque `gp` value family | PASS | Debug/Release runtime tests cover `GpVec`, `GpDir`, `GpAx1`, `GpMat`, validation, conversion, and disposal; B05 is reported as one coarse batch |
| Common modeling APIs | PASS | Release/Debug cover cone/torus, extrusion/revolution, all/single-edge fillet/chamfer, offset, section, finite bounds, validity/count, failures, layouts, and source independence |
| Current geometry/topology/XDE APIs | PASS | Release/Debug and the clean package consumer cover curve/surface derivatives/projection, pcurves, trim/wire/reshape, bidirectional adjacency, loft/pipe/sewing, wedge/thick solid, Boolean history, selective STEP sessions/units, composable XDE import, validation properties, recursive occurrences/world placement, and STEPCAF options |
| B06 string/sequence/array/vector/map wave | PASS | Debug/Release runtime tests (40) cover UTF-8/UTF-16 conversion, finite mutation, lower-bound translation, map lookup/bind/unbind, ordered keys, clone ownership, one-shot snapshots, empty collections, stale disposal, and early-exit enumeration |
| Generated staging and stale cleanup | PASS | Generator tests cover deterministic module/layer output, manifest-owned stale removal, and acyclic dependency direction |
| Generated cross-shard dependency closure | PASS | 16,353 declarations and 94 files; 27 direct edges, 0 unresolved references, 0 target-graph violations, 0 cyclic groups; SHA256 `7A92FA90504502DD4E7762CE0E7A619C046178FA4BBDDFD5268946F32AFE9424` |
| Generated source freshness | PASS | Preview.10 release check verified all 94 manifest-owned files and byte-identical clean regeneration |
| Generated value ABI layout | PASS | Native 24-byte/8-byte assertions and managed 24-byte runtime assertion |
| STEP geometry round-trip | PASS | Generated box and transformed two-box compound round-tripped with 6 and 12 faces |
| STL/IGES file output | PASS | Binary STL and BRep-mode IGES created and checked non-empty |
| Real STEP assembly sample | PASS | 7 inputs, 701 faces, 2,412,254-byte output |
| Interactive console samples | PASS (scoped) | Seven-workflow menu compiles; alpha.55 non-interactive runtime/package workflows include the complete real-HWND Batch D review closure; manual interactive Viewer UI inspection is not used as automated evidence |
| B17 HWND visualization core | PASS | Release/Debug real HWND display, source-independent AIS shape, appearance/display mode, standard projections, zoom/pan/rotation, thread rejection, detection, replace/add/remove/toggle/clear selection, snapshot, and removal |
| Batch D production viewport/model review | PASS | 24/24 through real STEP/XDE and real HWND: copied occurrence identity, owning detected/selected topology, rectangle/polygon selection, filters, bounds/fit/isolate, subshape styles/reset, camera/conversions/ray, zoom/background, clip planes, hidden-line, trihedron, and Unicode screenshot; repository runtime and clean package both pass |
| Batch E inspection/measurement/PMI/AP242 | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 119/119 cover exact inspection, complete PMI/reference graphs and transactions, AP242/BinXCAF, saved views, four viewer dimensions, real HWND/screenshot, and clean-package execution |
| Batch F freeform curve/surface/profile authoring | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 123/123 cover copied definitions, immutable edits, interpolation/approximation, profiles/offsets/fills/splits, controlled loft/pipe, sew/heal, real STEP/XDE, mesh/measurement, real HWND selection/screenshot, and clean-package execution |
| Batch G technical drawing/HLR/section/vector output | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 127/127 cover exact/polygonal HLR, ten owning layers, sections, copied polylines, layered SVG, standard views, real STEP/XDE, real HWND screenshot, and clean-package execution |
| Batch H advanced mesh/scene/material/LOD/interchange | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 131/131 cover grouped attributes/statistics/diagnostics, independent LODs, PBR/physical metadata, nested/shared scene snapshots, glTF/GLB/OBJ read/write, PLY/VRML write, real STEP/XDE, real HWND screenshot, and clean-package execution |
| Batch I document state/graph/history/persistence | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 135/135 cover copied typed state, dependency/reverse/SCC diagnostics, named commands, undo/redo/branching/savepoints, BinOcaf/XmlOcaf/BinXCAF/XmlXCAF, STEP/XDE, owning topology, source disposal, and clean-package execution |
| Batch J advanced feature modeling/history/recovery | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 139/139 cover selected/variable/planar finishing, draft, boss/pocket/hole, additive/subtractive revolve and pipe, split, defeaturing, cells, four batch Boolean modes, robust options, preflight/recovery, copied history/deletion, STEP/XDE, real HWND screenshots, source disposal, and clean-package execution |
| Batch K assembly authoring/BOM/occurrence | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 143/143 cover definition/occurrence edits, where-used, paths, graph/BOM/diagnostics, external/item references, SHUO/effective metadata, rollups, named transactions, STEP/XDE, real HWND screenshot, source/document disposal, and clean-package execution |
| Batch L digital mock-up interference/clearance | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 147/147 cover XDE occurrences, AABB/OBB, broad/exact phase, independent filtering, every pair state, witnesses and face/edge groups, self-checks, matrices, aggregation, diagnostics, incremental rerun, owning issue topology, STEP/XDE, real HWND screenshot, source/document disposal, and clean-package execution |
| Batch M interactive assembly placement editing | PASS | 24/24; focused 4/4 and full Release/Debug Runtime 151/151 cover presentation transforms, complete manipulator configuration, custom/mouse preview, apply/cancel, ownership/thread guards, named rigid occurrence relocation, replacement labels, undo/redo, DMU recheck, STEP/XDE, real HWND screenshot, source/document disposal, and clean-package execution |
| B18 optional dependency profiles | PASS | Release/Debug build audit classifies 6/6 profiles; IVtk/VTK and EGL/GLES blockers are named; core package unchanged |
| Native runtime dependency closure | PASS | 62 DLLs in the committed Preview.11 shared-native manifest with ABI 1.55/bridge 0.63.0; the complete Release rebuild is byte-identical and loads from `occt`; bridge SHA256 `4D1FE8F36D93D7732337FDF7D9D6D6038A29EE78D084AAE6C7A827C4D071514E` |
| XDE two-box assembly | PASS | One XDE assembly root, two occurrences, and 12-face STEP round-trip |
| STEPCAF/XDE metadata | PASS (scoped) | Existing seven-input metadata workflow plus alpha.53 area/volume/centroid attributes, nested occurrence placement, metadata filters, and BinXCAF/STEPCAF round trips |
| XDE native runtime libraries | PASS | `TKXCAF`, `TKCAF`, `TKLCAF`, and `TKCDF` present in Debug and Release runtime directories |
| Checked shared-handle cast | PASS | Release/Debug `TryCastDerived` and `CastDerived`: retained success, wrong/null rejection, and `InvalidCastException` |
| NuGet package contents | PASS | `8.0.1-preview.11`; 13 managed packages each contain one managed DLL and zero native DLLs; `OcctSharp.Native.win-x64` alone contains 62 DLLs and 11 notices/licenses; facade nupkg SHA256 `365545377020E0E4D54A92294830B11504A85CE8B24DC0692CE13C8078DC6AF7` |
| Package output layout | PASS | Published executable has `occt/` closure and no root `OcctSharp.Native.dll` |
| Packaging/clean consumer | PASS | Clean SDK 10.0.400 facade restore/publish/runtime passes the complete inherited Batch D-M workflow; direct Modeling restore/publish creates a six-face solid without `OcctSharp.dll`; both use ABI 1.55/bridge 0.63.0, OCCT 8.0.1, and the same 62-DLL closure |
| Fresh-clone Sample bundled runtime | PASS | New clone without local settings/OCCT environment passed manifest, Release/Debug `--smoke`, exact 62-DLL output, box creation, and package creation |
| Git whitespace checks | PASS | `git diff --check` and `git diff --cached --check` |
| CI configuration | PASS | Generator tests, clone-only bundled-runtime Release/Debug smoke, and immutable URL/SHA full Windows release-check jobs configured |
| Hosted CI execution | PASS (clone/runtime); full release NOT RUN | GitHub run 33064559589: generator-tests and bundled-runtime manifest/Release/Debug smoke succeeded at commit c8a38c2; SDK-dependent full-windows was conditionally skipped because artifact variables are not configured |
| API compatibility | PASS | Alpha.38 606-signature baseline comparison: 38,781 additions, zero removals, non-breaking |
| Release engineering | PASS (Preview.11 local) | Complete local release check passes Release/Debug, Generator 91/91, Runtime 151/151, dependency profiles 6/6, 94-file freshness/clean regeneration, 14-package isolation, facade/direct-module consumers, API/inventory/SBOM/provenance/checksums, and Git whitespace |
| Public release readiness | BLOCKED | MIT and bundled notices PASS; hosted release execution, signing, and NuGet publication are NOT RUN |

## Migration loop state

```text
LOOP_STATE: COMPLETE
CURRENT_BATCH: NONE — B THROUGH M ARE COMPLETE; NO NEW PRODUCT BATCH IS DECLARED
CURRENT_WORKSTREAM: NONE — PREVIEW.11 BATCH M AND ALL DECLARED PRODUCT BATCHES ARE COMPLETE
COMPLETED_THIS_TURN: COMPLETED BATCH M 24/24 AND THE COMPLETE PREVIEW.11 LOCAL RELEASE GATE
NEXT_WORKSTREAM: NONE
NEXT_ACTION: NONE UNTIL A NEW PRODUCT GAP INVENTORY AND WHOLE-LETTER BATCH ADR ARE EXPLICITLY ACCEPTED
ENGINEERING_PROGRESS: B 100% COMPLETE; C 100% COMPLETE; D 24/24 COMPLETE (100%); E 24/24 COMPLETE (100%); F 24/24 COMPLETE (100%); G 24/24 COMPLETE (100%); H 24/24 COMPLETE (100%); I 24/24 COMPLETE (100%); J 24/24 COMPLETE (100%); K 24/24 COMPLETE (100%); L 24/24 COMPLETE (100%); M 24/24 COMPLETE (100%)
BATCH_PROGRESS: ALL DECLARED BATCHES B THROUGH M COMPLETE FOR THEIR ACCEPTED LOCAL DENOMINATORS (100%)
B_BASELINE_BINDING_COVERAGE: 16353 generated plus 542 accepted manual stable IDs; zero supported-unselected declarations
C_COMMON_WORKFLOW_COVERAGE: FIRST-WAVE 14/14, SECOND-WAVE 7/7, THIRD-WAVE 8/8, AND FINAL-WAVE 15/15 DENOMINATORS VALIDATED (100% EACH)
D_VIEWPORT_REVIEW_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
E_INSPECTION_PMI_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
F_FREEFORM_AUTHORING_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
G_TECHNICAL_DRAWING_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
H_ADVANCED_MESH_SCENE_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
I_DOCUMENT_HISTORY_PERSISTENCE_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
J_FEATURE_MODELING_HISTORY_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
K_ASSEMBLY_AUTHORING_BOM_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
L_INTERFERENCE_CLEARANCE_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
M_INTERACTIVE_PLACEMENT_COVERAGE: 24/24 IMPLEMENTED AND VALIDATED (100%)
FULL_PROFILE_ACCOUNTING: 116272/116272 declarations classified; 50033 have narrow blocked dispositions and are not claimed as managed APIs
INVENTORY_COMPLETENESS: 7058/7090 headers semantically scanned (99.5487%); 116272/116272 discovered declarations and 7090/7090 catalogued headers classified
LAST_VALIDATION: Preview.11 complete local release check PASS; Release/Debug, Generator 91/91, Runtime 151/151, dependency profiles 6/6, 94-file regeneration, 14-package isolation, facade/direct-module consumers, additive API, inventory, SBOM, provenance, and checksums PASS
L_COMPLETION_VALIDATION: Focused 4/4, independent adjacent and face/face-edge/edge-face/edge evidence, ABI 1.54, bridge 0.62.0, schema 1.12, SC-048 exact accounting, and all local gates PASS
M_COMPLETION_VALIDATION: Focused 4/4, presentation/manipulator and rigid occurrence edit/history/DMU/STEP-XDE/HWND evidence, ABI 1.55, bridge 0.63.0, schema 1.13, SC-049 exact accounting, and all local gates PASS
BLOCKER: NONE FOR LOCAL IMPLEMENTATION; HOSTED RELEASE, SIGNING, AND PUBLICATION REMAIN NOT RUN
```

## Known risks

- The supplied prebuilt OCCT bundle is locally verified but not automatically acquired.
- Consuming VC14-labeled OCCT binaries with MSVC 19.51 relies on MSVC binary
  compatibility and must remain runtime-tested.
- Current AST discovery is not yet a complete binding representation.
- Broad OCCT ownership semantics remain the highest implementation risk.
- OCCT 8.0.1 writes physical materials only from top-level part labels; subshape-only
  assignments are promoted to their part root for STEP round-trip (KI-010).
- Windows non-ASCII paths through OCCT narrow file APIs are not yet validated.

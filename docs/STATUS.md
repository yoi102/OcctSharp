# Current Status

- Last updated: 2026-08-27
- Current phase: single migration batch B and alpha.50 clone-and-run distribution are locally complete; publication authority remains separate
- Batch B engineering progress: 100% for the accepted local implementation scope (not a claim that every OCCT declaration is a managed API or that public release is ready)
- Complete-migration batch progress: B is complete; retired B00-B20 labels are not counted as batches
- Accepted generated surface: 16,353 manifest IDs from the 116,263-declaration selected discovery model; Release and Debug native/managed builds, Generator 62/62, Runtime 105/105, discovery/report determinism, and dependency profiles 6/6 pass
- Last complete full inventory: 116,272/116,272 declarations and 7,090/7,090 headers have final dispositions; `Emitted` 16,353, `Manual` 61, `SupportedUnselected` 0, `Skipped` 49,344, `Blocked` 50,514, pending 0, HD099 0
- Overall state: broad LT001-LT004 buckets are eliminated and replaced by generated bindings or narrow evidence-backed ABI/ownership dispositions. The complete alpha.50 local release check passes and records `batchImplementationComplete: true`. MIT licensing, bundled third-party notices, and committed runtime evidence pass; `publicReleaseReady` remains false because hosted release execution, signing, and NuGet publication are separate external gates

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

Batch B is locally complete. The final accounting has zero `SupportedUnselected`, zero
LT001-LT004, zero pending declarations/headers, and all local implementation gates pass.
Keep emitted coverage, classification completeness, batch completion, and public-release
readiness as four independent facts. Any later product-scale migration starts a new whole-
letter batch; project licensing, third-party legal review, hosted CI, signing, and NuGet
publication remain external release work and are not silently included in Batch B.

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
- Generator input: `BRepPrimAPI_MakeBox.hxx`, `Geom_CartesianPoint.hxx`, `gp_Pnt.hxx`, and `TopoDS_Shape.hxx`
  plus their included OCCT headers.
- Discovery output: 3,062 unique normalized declarations, zero Clang diagnostics.
- Runtime workflow: create/transform/compound shapes; count faces; STEP round-trip;
  STL/IGES output; geometry and XDE STEP assembly; validate errors, ownership, and
  disposal.

## Not implemented

- Broad type-map and ownership passes beyond the initial value-copy rules, manually
  designed shape handle, and the B05 transformation bridges.
- Generalized native and managed source emission beyond the configured value-copy,
  first typed shared, and topology scopes; B05 values remain manual until generalized
  opaque-value emission is safe, and general XDE/OCAF bindings remain pending.
- Broader collection profiles remain open: sets, richer element mappings, and borrowed
  parent-bound views; the safe scalar array/vector/map snapshot subset is covered by
  SC-009/SC-010 and ABI 1.18.
- Broad `Handle<T>` coverage, generalized borrowed/parent-bound objects, generated
  XDE/OCAF/visualization declarations, and advanced bulk mesh APIs.
- Project licensing, complete third-party legal/notice review, signing, and an authorized
  package publication workflow. SBOM/provenance/checksum generation is implemented.
- Hosted execution of the configured immutable-artifact CI path and automated source
  build of OCCT on a clean machine.

## Next tasks

1. Continue replacing LT001-LT004 with enum/value/shared/parent-bound rules until all
   bindable declarations are emitted or accepted manual.
2. Expand common Geom/Geom2d, BRep, mesh, exchange, XDE, and visualization families in
   large related waves before low-value data entities.
3. Resolve license/notices/hosted CI/signing only after the B binding gate closes;
   public publication still requires explicit authorization.

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
| Release managed build | PASS | 5 projects, 0 warnings, 0 errors |
| Debug native/managed build | PASS | 5 projects, 0 warnings, 0 errors |
| Generator unit tests | PASS | Current Debug `eng/build.ps1`: 44/44 |
| Runtime/lifetime tests | PASS | Current Release and Debug builds: 96/96 |
| Controlled semantic Clang parse | PASS | Record, method, constructor, and enum discovery |
| OCCT semantic discovery | PASS | Selected scope: 22,879 declarations; 61 configured manual stable IDs reconciled |
| Full OCCT header catalog | PASS | 7,090 entry headers: 7,084 `.hxx`, 6 `.h`, 407 filename-derived packages |
| Full OCCT semantic inventory | BLOCKED | 7,058/7,090 headers; 116,214 partial unique declarations; 32 named dependency/artifact failures |
| Full-inventory classification | PASS | 4,060 emitted, 61 manual, 11,144 supported-unselected, 27,310 skipped, 73,639 blocked; 116,214/116,214 declarations and 7,090/7,090 headers classified; SHA256 `D46B10BFF1A5246721A19E19DA13A26E55E27242F8F95E0EFC7A2C7555A43963` |
| Discovery determinism | PASS | Two-run SHA256 `2E93694B6DDC90BD5F9381B288A5AD7187DD0A83D37EF55043385043B8DDFB4B` |
| Model determinism | PASS | Two runs SHA256 `5C0FAF4B37C0D5A56ADCB11A0729C6FB5BCF79D5789EAA4356465CB354D0C064` |
| Documentation navigation | PASS | One repository README; local Markdown targets checked |
| Structured canonical model compile | PASS | Generator Release build, 0 warnings and 0 errors |
| Structured canonical model tests | PASS | 3 generator tests; signature, qualifier, inheritance, template/handle facts |
| Structured OCCT fact inventory | PASS | Binding-model schema 1.2 retains abstract-record facts in the selected 22,879-declaration semantic model |
| Source package/toolkit identity | PASS | 22,879 of 22,879 declarations classified in the selected scope |
| Support classification tests | PASS | 2 tests; rule order, stable codes, complete/sorted summary |
| Selected-scope support summary | PASS | Current selected scope has 6,494 supported; 4,060 emitted plus 61 accepted manual stable IDs |
| Simple binding eligibility | PASS | Value-copy constructors/static methods promoted; instance/pointer/unknown-lifetime cases remain pending |
| Coverage and diagnostics reports | PASS | 22,879 declarations; all states and stable disposition codes reported |
| Report determinism | PASS | Coverage SHA256 `50CAFAD12E347A3E76B6CBF959FCA42F548B59373C673641191AAACC79DE6BBE`; diagnostics SHA256 `0BC79186F9564A1CEAF911FCF3EB073C38AE6B174FA43B7020AE8ADB6A8EE03C` |
| Initial TypeMap tests | PASS | 9 tests; `TM001`–`TM007`, const-reference/top-level const input, unsafe pointer/reference rejection |
| Native TypeMap compile fixture | PASS | OCCT scalar and enum width assertions in Release native build |
| Configured generation scopes | PASS | Schema 1.6 adds 18 validated SC-032 manual stable IDs to the existing generated scopes |
| Generated value-copy bindings | PASS | Three `gp_Pnt` constructors plus 28 scalar static methods (20 `Precision`, three `TopAbs`, and five ownership-neutral additions) emitted to native/managed source; compiled and called in Release and Debug |
| Generated typed shared binding | PASS | Selected Geom/Geom2d, STEP entity, mesh/Poly/analysis/healing public types with 3,076 manifest IDs; scalar/value/enum and cross-handle mutation, all 249 newly added entity constructors/clones, sharing, null, RTTI, retention, and disposal pass in Release and Debug |
| Generated topology binding | PASS | 8 base `TopoDS_Shape` operations plus 8 checked typed casts; solid/compound success, wrong-kind rejection, and source-disposal independence pass |
| B05.1 opaque `gp_Trsf` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, finite/index validation, and shape application |
| B05.2 opaque `TopLoc_Location` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, conversion, and absolute/relative shape placement |
| B05 complete opaque `gp` value family | PASS | Debug/Release runtime tests cover `GpVec`, `GpDir`, `GpAx1`, `GpMat`, validation, conversion, and disposal; B05 is reported as one coarse batch |
| Common modeling APIs | PASS | Release/Debug cover cone/torus, extrusion/revolution, all/single-edge fillet/chamfer, offset, section, finite bounds, validity/count, failures, layouts, and source independence |
| Current geometry/topology/XDE APIs | PASS | Release/Debug and the clean package consumer cover curve/surface evaluation and projection, adjacency, loft/pipe/sewing, wedge/thick solid, Boolean history, and composable STEP import |
| B06 string/sequence/array/vector/map wave | PASS | Debug/Release runtime tests (40) cover UTF-8/UTF-16 conversion, finite mutation, lower-bound translation, map lookup/bind/unbind, ordered keys, clone ownership, one-shot snapshots, empty collections, stale disposal, and early-exit enumeration |
| Generated staging and stale cleanup | PASS | Generator tests cover deterministic output and manifest-owned stale removal |
| Generated source freshness | PASS | Alpha.44 temporarily staged only the intended six changed generated files; 13 manifest-owned files were current, then temporary staging was removed |
| Generated value ABI layout | PASS | Native 24-byte/8-byte assertions and managed 24-byte runtime assertion |
| STEP geometry round-trip | PASS | Generated box and transformed two-box compound round-tripped with 6 and 12 faces |
| STL/IGES file output | PASS | Binary STL and BRep-mode IGES created and checked non-empty |
| Real STEP assembly sample | PASS | 7 inputs, 701 faces, 2,412,254-byte output |
| Interactive console samples | PASS (scoped) | Six-class menu compiles; first five workflows have redirected-input evidence; Viewer UI launch NOT RUN, while its HWND path is runtime/package tested |
| B17 HWND visualization core | PASS | Release/Debug real HWND display, source-independent AIS shape, hide/show/resize/fit/redraw, thread rejection, detection/selection snapshot, and removal |
| B18 optional dependency profiles | PASS | Release/Debug build audit classifies 6/6 profiles; IVtk/VTK and EGL/GLES blockers are named; core package unchanged |
| Native runtime dependency closure | PASS | 62 DLLs in committed alpha.50 runtime; complete Release rebuild is byte-identical and loads from `occt` |
| XDE two-box assembly | PASS | One XDE assembly root, two occurrences, and 12-face STEP round-trip |
| STEPCAF/XDE metadata | PASS (scoped) | Seven local inputs: color/style records retained, 4 material-property records retained, 7 assembly occurrences |
| XDE native runtime libraries | PASS | `TKXCAF`, `TKCAF`, `TKLCAF`, and `TKCDF` present in Debug and Release runtime directories |
| Checked shared-handle cast | PASS | Release/Debug `TryCastDerived` and `CastDerived`: retained success, wrong/null rejection, and `InvalidCastException` |
| NuGet package contents | PASS | `0.1.0-alpha.50`; managed/XML/docs, MIT metadata/license, 62 native DLLs, and 11 bundled notice/license files |
| Package output layout | PASS | Published executable has `occt/` closure and no root `OcctSharp.Native.dll` |
| Packaging/clean consumer | PASS | Local alpha.49 package plus clean SDK 10.0.400 NuGet restore/publish/runtime, ABI 1.41/bridge 0.49.0, 62 DLLs, 16,353 generated declarations, IGES/session infrastructure, generated cross-type handles, mesh/Poly/analysis/healing, Geom/Geom2d, current geometry/topology/XDE APIs, and prior profiles |
| Fresh-clone Sample bundled runtime | PASS | New clone without local settings/OCCT environment passed manifest, Release/Debug `--smoke`, exact 62-DLL output, box creation, and package creation |
| Git whitespace checks | PASS | `git diff --check` and `git diff --cached --check` |
| CI configuration | PASS | Generator tests, clone-only bundled-runtime Release/Debug smoke, and immutable URL/SHA full Windows release-check jobs configured |
| Hosted CI execution | PASS (clone/runtime); full release NOT RUN | GitHub run 33064559589: generator-tests and bundled-runtime manifest/Release/Debug smoke succeeded at commit c8a38c2; SDK-dependent full-windows was conditionally skipped because artifact variables are not configured |
| API compatibility | PASS | Alpha.38 606-signature baseline comparison: 36,602 additions, zero removals, non-breaking |
| Release engineering | PASS (alpha.50) | Complete alpha.50 release check passes, including committed-runtime hash/build identity and fresh-clone smoke; `batchImplementationComplete` is true |
| Public release readiness | BLOCKED | MIT and bundled notices PASS; hosted release execution, signing, and NuGet publication are NOT RUN |

## Migration loop state

```text
LOOP_STATE: COMPLETE
CURRENT_BATCH: B
CURRENT_WORKSTREAM: BATCH B LOCAL IMPLEMENTATION COMPLETE
COMPLETED_THIS_TURN: Added MIT, committed and hash-pinned the 62-DLL Windows x64 runtime with complete bundled notices, made ordinary clones run Debug/Release samples without OCCT, and passed the complete alpha.50 release check
NEXT_WORKSTREAM: NONE INSIDE B; PUBLIC-RELEASE GATES ARE SEPARATE
NEXT_ACTION: Configure hosted full-release artifact variables only when an approved SDK source exists; signing credentials and NuGet publication still require separate owner authorization
ENGINEERING_PROGRESS: 100% FOR B LOCAL IMPLEMENTATION
BATCH_PROGRESS: B COMPLETE (declaration coverage and public-release readiness reported separately)
SELECTED_BINDING_COVERAGE: 16414/16414 generated plus accepted manual stable IDs (100%)
FULL_PROFILE_ACCOUNTING: 116272/116272 declarations classified; 50514 have narrow blocked dispositions and are not claimed as managed APIs
INVENTORY_COMPLETENESS: 7058/7090 headers semantically scanned (99.5487%); 116272/116272 discovered declarations and 7090/7090 catalogued headers classified
LAST_VALIDATION: Complete alpha.50 release check plus an independent fresh clone passed; Generator 62/62 and Runtime 105/105 in Release/Debug and clean copy, committed/build-identical 62-DLL runtime at ABI 1.41/bridge 0.49.0, 13-file byte-identical regeneration, 36602 additions/0 removals, full inventory, package consumer, SBOM/provenance/checksums, and whitespace checks passed; GitHub run 33064559589 then passed generator and clone-only manifest/Release/Debug jobs at c8a38c2
BLOCKER: NONE FOR B OR CLONE-AND-RUN; PUBLIC PACKAGE RELEASE STILL REQUIRES HOSTED RELEASE EXECUTION, SIGNING, AND NUGET PUBLICATION AUTHORIZATION
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

# Current Status

- Last updated: 2026-08-23
- Current phase: B19 long-tail binding migration in progress; B20 release engineering implemented but not closed
- Engineering roadmap progress: 92% estimate (not OCCT API coverage or release readiness)
- Complete-migration batch progress: 19 of 21 batches complete (90.5%); only B00-B18 satisfy their batch exits
- Selected-scope emitted coverage: 171 of 3,406 declarations (5.0206%); selected-scope safe-support coverage: 273 of 3,406 (8.02%)
- Full-OCCT coverage: not yet established; the audit scans 7,058 of 7,090 entry headers and finds 116,214 unique declarations in the successful portion, so 171 emitted declarations are at most 0.1471% of the eventual complete denominator
- Full-inventory classification: 116,214/116,214 discovered declarations and 7,090/7,090 catalogued headers have final dispositions; both pending counts and HD099 are zero
- Overall state: classification is 100% for observed declarations/headers, the generated manifest reconciles 171 declarations as `Emitted`, 10,338 bindable declarations remain unselected, and B19/B20 are not complete

## Current focus

B19 continues with coherent emitted-binding sub-batches. Complete classification is an
accounting foundation, not a batch exit: every `SupportedUnselected` declaration must be
emitted or accepted manual, and LT001-LT004 must be replaced by implemented rules or
narrow evidence-backed reasons. Keep emitted coverage, classification completeness,
batch completion, and public-release readiness as four independent facts.

### B05 completed migration batch

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

### B06 first migration wave

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

### B07 completed immutable geometry profile

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

### B08 completed safe value/owner profile

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

### B09 first sub-batch

- Added `ShapeFactory.CreateSphere` and `CreateCylinder` over native
  `BRepPrimAPI_MakeSphere`/`BRepPrimAPI_MakeCylinder` with finite-positive validation,
  OCCT exception containment, and normal owning `Shape` handles.
- Added straight-edge, polygon-wire, and planar-face builders with copied point buffers,
  kind validation, builder completion checks, and independent owning results. B09 is
  complete for its basic construction profile.

### B10 first sub-batch

- Added `Shape.GetFaces()` and `Shape.GetSubShapes(ShapeKind)` over caller-owned native
  snapshot buffers. Face, edge, wire, and vertex copies are independent owning `Shape`
  values; no native iterator crosses the ABI, and returned children remain valid after
  parent disposal.
- Invalid kinds, empty snapshots, partial cleanup, parent disposal, and all four child
  kinds are covered; B10 is complete for its owning-snapshot profile.

### B11/B12 first sub-batch

- Added `Shape.Fuse` and `Shape.Cut` over native `BRepAlgoAPI_Fuse` and
  `BRepAlgoAPI_Cut` with validated input handles, contained OCCT failures, independent
  result ownership, and source-disposal independence.
- Added `Shape.Common` over `BRepAlgoAPI_Common` with the same owning-result contract.
- Added `Shape.DistanceTo` over native-local `BRepExtrema_DistShapeShape`; managed code
  receives only minimum distance, one copied point pair, and solution count. Layout,
  null/disposed failures, source independence, and alpha.34 package gates pass.
- B11 is complete for the declared owning/value result profile. Projections, offsets,
  fillets, feature builders, support topology, and algorithm history remain later profiles.

### B13 first sub-batch

- Added `Shape.CreateMesh` with a two-call count/snapshot contract over
  `BRepMesh_IncrementalMesh` and face-local `Poly_Triangulation` values.
- Added caller-owned copied positions, face normals, winding-corrected triangle indices,
  finite-positive deflection validation, 32-bit capacity checks, and no native array or
  triangulation pointer exposure.
- B13 is complete for this first bulk-transfer profile; Poly algorithms, RWMesh formats,
  stable shared vertex identity, zero-copy views, and benchmark gates remain pending.

### B12 healing sub-batch

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

### B14 completed geometry-exchange profile

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
  explicit B16/B19 work rather than hidden B14 exit criteria.

### B15 completed OCAF document/label profile

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
  declarations remain B19 long-tail work.

### B16 completed XDE metadata/assembly profile

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
  explicit B19 long-tail profiles.

### B17 completed Windows visualization-core profile

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
  explicit B19 long-tail work.

### B18 completed optional-integration dependency profiles

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

### B19 classification foundation complete; binding batch remains open

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

### B19.1 StepBasic scalar/shared entity family complete

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

### B20 release engineering implemented; batch exit remains open

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
- Local B20 validation passed Generator 37/37 and Runtime 68/68 in Release and Debug,
  6/6 dependency profiles, 12-file clean regeneration, alpha.38 clean consumer, API
  diff 0/0, complete inventory classification, JSON parsing, and whitespace checks.
- `releaseEngineeringImplemented` is true while `batchImplementationComplete` and
  `publicReleaseReady` are false: bindable emission, PD-012, third-party legal review,
  hosted CI, signing, and publication are not silently waived.

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
- `AI_MIGRATION_LOOP_PROMPT.md` defines a re-entrant B00-B20 execution state machine,
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

1. Execute B19.2 as a larger StepBasic/StepRepr scalar shared-entity closure and keep
   manifest-reconciled emitted counts separate from raw support classification.
2. Continue replacing LT001-LT004 with enum/value/shared/parent-bound rules until all
   bindable declarations are emitted or accepted manual.
3. Resolve license/notices/hosted CI/signing only after the B19 binding gate closes;
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
| Generator unit tests | PASS | B19.1 Release/Debug `eng/build.ps1`: 40/40 |
| Runtime/lifetime tests | PASS | B19.1 Release/Debug `eng/build.ps1`: 73/73 in both configurations |
| Controlled semantic Clang parse | PASS | Record, method, constructor, and enum discovery |
| OCCT semantic discovery | PASS | Selected scope: 3,406 declarations, zero diagnostics |
| Full OCCT header catalog | PASS | 7,090 entry headers: 7,084 `.hxx`, 6 `.h`, 407 filename-derived packages |
| Full OCCT semantic inventory | BLOCKED | 7,058/7,090 headers; 116,214 partial unique declarations; 32 named dependency/artifact failures |
| B19 full-inventory classification | PASS | 171 emitted, 10,338 supported-unselected, 27,310 skipped, 78,395 blocked; 116,214/116,214 declarations and 7,090/7,090 headers classified; SHA256 `2972136A83100B61731736CC5EA8449A050D01105271D0AE00910E10E304EC38` |
| Discovery determinism | PASS | Two Release runs SHA256 `0D2367057194346A208EE0BFD27BC7A1FC9ED7C50370346B85BAC2D8281E6BDF` |
| Model determinism | PASS | Two runs SHA256 `980C73635039CAC3066E33413928F27060A8EF2F7043097BE13A7D4C51B292F9` |
| Documentation navigation | PASS | One repository README; local Markdown targets checked |
| Structured canonical model compile | PASS | Generator Release build, 0 warnings and 0 errors |
| Structured canonical model tests | PASS | 3 generator tests; signature, qualifier, inheritance, template/handle facts |
| Structured OCCT fact inventory | PASS | 1,965 parameterized, 2,388 returning, 61 inherited, 29 templated declarations; 236 handle uses |
| Source package/toolkit identity | PASS | 3,406 of 3,406 declarations mapped; 0 unresolved in selected scope |
| Support classification tests | PASS | 2 tests; rule order, stable codes, complete/sorted summary |
| Selected-scope support summary | PASS | 273 supported; 2,370 pending; 763 skipped |
| Simple binding eligibility | PASS | Value-copy constructors/static methods promoted; instance/pointer/unknown-lifetime cases remain pending |
| Coverage and diagnostics reports | PASS | 3,406 declarations; all states and stable disposition codes reported |
| Report determinism | PASS | Release/Debug two-run match: coverage SHA256 `B2FC8309DD04C934DDDE8D4B2D991539F33F523CD1261EC719B77AEB66A13723`; diagnostics SHA256 `61E76817BE352F1FC2CB7CCDA0E80BCC80AC24DD67B9FDEA2330565506737A01` |
| Initial TypeMap tests | PASS | 9 tests; `TM001`–`TM007`, const-reference/top-level const input, unsafe pointer/reference rejection |
| Native TypeMap compile fixture | PASS | OCCT scalar and enum width assertions in Release native build |
| Configured generation scopes | PASS | Schema 1.4 selects seven value scopes, eleven typed shared scopes, one topology scope, and eight checked typed topology identities |
| Generated value-copy bindings | PASS | Three `gp_Pnt` constructors plus 28 scalar static methods (20 `Precision`, three `TopAbs`, and five ownership-neutral additions) emitted to native/managed source; compiled and called in Release and Debug |
| Generated typed shared binding | PASS | 11 `Geom_CartesianPoint` plus 106 StepBasic scalar/shared declarations; construction, scalar/boolean/enum mutation, sharing, RTTI, and disposal pass in Release and Debug |
| Generated topology binding | PASS | 8 base `TopoDS_Shape` operations plus 8 checked typed casts; solid/compound success, wrong-kind rejection, and source-disposal independence pass |
| B05.1 opaque `gp_Trsf` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, finite/index validation, and shape application |
| B05.2 opaque `TopLoc_Location` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, conversion, and absolute/relative shape placement |
| B05 complete opaque `gp` value family | PASS | Debug/Release runtime tests cover `GpVec`, `GpDir`, `GpAx1`, `GpMat`, validation, conversion, and disposal; B05 is reported as one coarse batch |
| B06 string/sequence/array/vector/map wave | PASS | Debug/Release runtime tests (40) cover UTF-8/UTF-16 conversion, finite mutation, lower-bound translation, map lookup/bind/unbind, ordered keys, clone ownership, one-shot snapshots, empty collections, stale disposal, and early-exit enumeration |
| Generated staging and stale cleanup | PASS | Generator tests cover deterministic output and manifest-owned stale removal |
| Generated source freshness | PASS | `eng/verify-generated.ps1 -Configuration Release`; 13 tracked files, no generated diff |
| Generated value ABI layout | PASS | Native 24-byte/8-byte assertions and managed 24-byte runtime assertion |
| STEP geometry round-trip | PASS | Generated box and transformed two-box compound round-tripped with 6 and 12 faces |
| STL/IGES file output | PASS | Binary STL and BRep-mode IGES created and checked non-empty |
| Real STEP assembly sample | PASS | 7 inputs, 701 faces, 2,412,254-byte output |
| Interactive console samples | PASS (scoped) | Six-class menu compiles; first five workflows have redirected-input evidence; Viewer UI launch NOT RUN, while its HWND path is runtime/package tested |
| B17 HWND visualization core | PASS | Release/Debug real HWND display, source-independent AIS shape, hide/show/resize/fit/redraw, thread rejection, detection/selection snapshot, and removal |
| B18 optional dependency profiles | PASS | Release/Debug build audit classifies 6/6 profiles; IVtk/VTK and EGL/GLES blockers are named; core package unchanged |
| Native runtime dependency closure | PASS | 45 DLLs in the alpha.39 Release package; TKOpenGl and TKDESTEP load from `occt` in the clean consumer |
| XDE two-box assembly | PASS | One XDE assembly root, two occurrences, and 12-face STEP round-trip |
| STEPCAF/XDE metadata | PASS (scoped) | Seven local inputs: color/style records retained, 4 material-property records retained, 7 assembly occurrences |
| XDE native runtime libraries | PASS | `TKXCAF`, `TKCAF`, `TKLCAF`, and `TKCDF` present in Debug and Release runtime directories |
| Checked shared-handle cast | PASS | Release/Debug `TryCastDerived` and `CastDerived`: retained success, wrong/null rejection, and `InvalidCastException` |
| NuGet package contents | PASS | `0.1.0-alpha.39`; managed/XML/docs, 45 native DLLs, OCCT license and exception |
| Package output layout | PASS | Published executable has `occt/` closure and no root `OcctSharp.Native.dll` |
| Packaging/clean consumer | PASS | Local alpha.39 package plus NuGet restore/publish, ABI 1.31/bridge 0.39.0, 45 DLLs, generated StepBasic shared/enum behavior, and prior profiles |
| Git whitespace checks | PASS | `git diff --check` and `git diff --cached --check` |
| CI configuration | PASS | Generator job plus immutable URL/SHA full Windows release-check job configured in `.github/workflows/ci.yml` |
| Hosted CI execution | NOT RUN | No remote workflow was dispatched from this local task |
| B20 release engineering | PASS (implementation) | API baseline 606, diff 0/0, 12-file clean regeneration, SBOM/provenance/checksums/gates, Release/Debug and package gates |
| Public release readiness | BLOCKED | Project license and non-OCCT third-party review unresolved; signing/publication NOT RUN |

## Migration loop state

```text
LOOP_STATE: CONTINUE
CURRENT_BATCH: B19.1
COMPLETED_THIS_TURN: Generated and validated ten StepBasic shared entities plus typed enums; manifest-aware inventory now distinguishes 171 emitted declarations
NEXT_BATCH: B19.2 STEP BASIC AND STEP REPR SHARED ENTITIES
NEXT_ACTION: Select and generate the next 100-500 scalar/shared declarations from the manifest-reconciled inventory
ENGINEERING_PROGRESS: 92%
BATCH_PROGRESS: 19/21 (90.5%)
SELECTED_BINDING_COVERAGE: 171/3406 (5.0206%)
FULL_PROFILE_COVERAGE: NOT ESTABLISHED
INVENTORY_COMPLETENESS: 7058/7090 headers semantically scanned (99.5487%); 116214/116214 discovered declarations and 7090/7090 catalogued headers classified
LAST_VALIDATION: B19.1 Release/Debug Generator 40/40 and Runtime 73/73, 13-file freshness, alpha.39 45-DLL clean consumer, and manifest-aware full inventory classification
BLOCKER: NONE
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

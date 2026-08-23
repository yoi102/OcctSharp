# Current Status

- Last updated: 2026-08-22
- Current phase: Phase 2 lifetime foundation; B06 foundation strings/collections is in progress
- Engineering roadmap progress: 96% (foundation roadmap estimate, not OCCT API coverage or release readiness)
- Complete-migration batch progress: 6 of 21 batches complete (28.6%); B05 is complete as one coarse ownership batch
- Selected-scope emitted coverage: 58 of 3,062 declarations (1.8948%); selected-scope safe-support coverage: 147 of 3,062 (4.80%)
- Full-OCCT coverage: not yet established; the first audit scanned 7,058 of 7,090 entry headers and found 116,214 unique declarations in the successful portion, so 58 emitted declarations are at most 0.0499% of the eventual complete denominator
- Overall state: Value-copy, typed shared-handle, base topology, checked typed-topology generation, opaque B05 transformation values, and B06 string/sequence/array/vector/map contracts; deterministic coverage/diagnostics; 58 generated declarations; registry-validated owning/shared handles; manual geometry/XDE workflows; and the alpha.14 package are validated

## Current focus

B06 collection waves now cover strings, real sequences, arrays, vectors, and safe integer-key maps; continue the same coarse
batch with richer elements and iterator contracts over the proven opaque-handle/error contract.
Keep the incomplete full-library denominator and borrowed/parent-bound projections explicitly gated.
Preserve registries as safety guards rather than concurrency claims.

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
  exposure under SC-008/ADR-0021, SC-009/ADR-0022, and SC-010/ADR-0023. B06 remains in progress
  until richer elements and iterator contracts have equivalent evidence.

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
- Broader B06 collections remain open: sets, richer element mappings, borrowed views,
  and iterator/early-exit lifetime contracts; the safe scalar array/vector/map subset is
  now covered by SC-009/SC-010.
- Broad `Handle<T>` coverage, borrowed/parent-bound objects, typed `TopoDS_*`, general XDE/OCAF
  document APIs, bulk mesh APIs, visualization, CI, and public release.
- Project licensing, complete third-party notices/provenance, SBOM, signing, and an
  authorized package publication workflow.
- Automated acquisition/build of OCCT on a clean machine.

## Next tasks

1. Continue B06 in the same coarse batch with arrays, vectors/maps, richer element
   mappings, iterators, and collection ownership tests.
2. Resolve the project license and audit notices/provenance for all 36 packaged native
   DLLs before any public NuGet publication.
3. Add generated/package verification to the future CI workflow.
4. Expand XDE fixture coverage, including layers, names, nested assemblies, and
   material-placement fidelity.

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
| Generator unit tests | PASS | `eng/build.ps1`: 32 tests in Release and Debug |
| Runtime/lifetime tests | PASS | `eng/build.ps1`: 34 tests in Release and Debug; native closure loaded from `occt` |
| Controlled semantic Clang parse | PASS | Record, method, constructor, and enum discovery |
| OCCT semantic discovery | PASS | Selected scope: 3,062 declarations, zero diagnostics |
| Full OCCT header catalog | PASS | 7,090 entry headers: 7,084 `.hxx`, 6 `.h`, 407 filename-derived packages |
| Full OCCT semantic inventory | BLOCKED | 7,058/7,090 headers; 116,214 partial unique declarations; 32 isolated header/dependency failures; report SHA256 `BE715223512E70FF6C3203BD2CF301DF634E5BF7900F78FA576D19D1907D17AD` |
| Discovery determinism | PASS | Two Release runs SHA256 `1EB6E280BB546C9E64436A38ED1A1021AD63A3EF9C63F312E39DB45C61805CCF` |
| Model determinism | PASS | Two runs SHA256 `D292AFC1076A5066FB9A0A9FC5AA837E6C296A9AA3BC893036BAD6268829A524` |
| Documentation navigation | PASS | One repository README; local Markdown targets checked |
| Structured canonical model compile | PASS | Generator Release build, 0 warnings and 0 errors |
| Structured canonical model tests | PASS | 3 generator tests; signature, qualifier, inheritance, template/handle facts |
| Structured OCCT fact inventory | PASS | 1,965 parameterized, 2,388 returning, 61 inherited, 29 templated declarations; 236 handle uses |
| Source package/toolkit identity | PASS | 3,062 of 3,062 declarations mapped; 0 unresolved in selected scope |
| Support classification tests | PASS | 2 tests; rule order, stable codes, complete/sorted summary |
| Selected-scope support summary | PASS | 147 supported; 2,160 pending; 737 skipped |
| Simple binding eligibility | PASS | Value-copy constructors/static methods promoted; instance/pointer/unknown-lifetime cases remain pending |
| Coverage and diagnostics reports | PASS | 3,062 declarations; 19 package/toolkit groups; all states and stable disposition codes reported |
| Report determinism | PASS | Release/Debug two-run match: coverage SHA256 `00DA3284880AAD6F31C32C45CCF3ED3E7056A4EA6A925DB4589AE3F5304CA1FA`; diagnostics SHA256 `3C5102A282166174C2A8A44406ACEE8A8F9B535C398FB209E5DAC0D43C1A63E5` |
| Initial TypeMap tests | PASS | 9 tests; `TM001`–`TM007`, const-reference/top-level const input, unsafe pointer/reference rejection |
| Native TypeMap compile fixture | PASS | OCCT scalar and enum width assertions in Release native build |
| Configured generation scopes | PASS | Schema 1.4 selects seven value scopes, one typed shared scope, one topology scope, and eight checked typed topology identities |
| Generated value-copy bindings | PASS | Three `gp_Pnt` constructors plus 28 scalar static methods (20 `Precision`, three `TopAbs`, and five ownership-neutral additions) emitted to native/managed source; compiled and called in Release and Debug |
| Generated typed shared binding | PASS | 11 `Geom_CartesianPoint` declarations plus generated clone/RTTI/ref-count/release infrastructure; construction, mutation, sharing, and disposal pass in Release and Debug |
| Generated topology binding | PASS | 8 base `TopoDS_Shape` operations plus 8 checked typed casts; solid/compound success, wrong-kind rejection, and source-disposal independence pass |
| B05.1 opaque `gp_Trsf` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, finite/index validation, and shape application |
| B05.2 opaque `TopLoc_Location` bridge | PASS | Debug/Release runtime tests cover identity, composition, clone, inverse, conversion, and absolute/relative shape placement |
| B05 complete opaque `gp` value family | PASS | Debug/Release runtime tests cover `GpVec`, `GpDir`, `GpAx1`, `GpMat`, validation, conversion, and disposal; B05 is reported as one coarse batch |
| B06 string/sequence/array/vector/map wave | PASS | Debug/Release runtime tests cover UTF-8/UTF-16 conversion, finite sequence/array/vector mutation, lower-bound translation, map lookup/bind/unbind, indexed-map order, clone ownership, enumeration, and invalid indices |
| Generated staging and stale cleanup | PASS | Generator tests cover deterministic output and manifest-owned stale removal |
| Generated source freshness | PASS | `eng/verify-generated.ps1 -Configuration Release`; 12 tracked files, no generated diff |
| Generated value ABI layout | PASS | Native 24-byte/8-byte assertions and managed 24-byte runtime assertion |
| STEP geometry round-trip | PASS | Generated box and transformed two-box compound round-tripped with 6 and 12 faces |
| STL/IGES file output | PASS | Binary STL and BRep-mode IGES created and checked non-empty |
| Real STEP assembly sample | PASS | 7 inputs, 701 faces, 2,412,254-byte output |
| Interactive console samples | PASS | Menu-driven Release build and redirected-input run; all five workflows remain separate classes |
| Native runtime dependency closure | PASS | 36 DLLs in Debug and Release; 0 missing OCCT/third-party dependencies |
| XDE two-box assembly | PASS | One XDE assembly root, two occurrences, and 12-face STEP round-trip |
| STEPCAF/XDE metadata | PASS (scoped) | Seven local inputs: color/style records retained, 4 material-property records retained, 7 assembly occurrences |
| XDE native runtime libraries | PASS | `TKXCAF`, `TKCAF`, `TKLCAF`, and `TKCDF` present in Debug and Release runtime directories |
| Checked shared-handle cast | PASS | Release/Debug `TryCastDerived` and `CastDerived`: retained success, wrong/null rejection, and `InvalidCastException` |
| NuGet package contents | PASS | `0.1.0-alpha.14`, SHA256 `0FA91CCFB113AA72D7D65919E721DAAB141C932E96C163E686F63307AADDF091`; managed/XML/docs, 36 native DLLs, OCCT license and exception |
| Package output layout | PASS | Published executable has `occt/` closure and no root `OcctSharp.Native.dll` |
| Packaging/clean consumer | PASS | Local-only restore/publish from inner workspace, ABI 1.17/bridge 0.18.0 identity, native load, generated topology, complete B05 values, and B06 string/sequence/array/vector/map behavior |
| Git whitespace checks | PASS | `git diff --check` and `git diff --cached --check` |
| CI | NOT RUN | Not configured |

## Migration loop state

```text
LOOP_STATE: CONTINUE
CURRENT_BATCH: B06
COMPLETED_THIS_TURN: B06 collection waves: arrays, vectors, NCollection_DataMap<int,double>, and NCollection_IndexedMap<int>
NEXT_BATCH: B06
NEXT_ACTION: 调查并实现安全的值元素迭代器/游标契约，再补齐 B06 lifetime and early-exit tests
ENGINEERING_PROGRESS: 96%
BATCH_PROGRESS: 6/21 (28.6%)
SELECTED_BINDING_COVERAGE: 58/3062 (1.8948%)
FULL_PROFILE_COVERAGE: NOT ESTABLISHED
INVENTORY_COMPLETENESS: 7058/7090 headers scanned for windows-core-related full inventory (99.5487%; full semantic profile still blocked)
LAST_VALIDATION: Debug/Release build; generator 32/32; runtime 38/38; verify-generated Release; alpha.14 package consumer with 36 DLLs
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

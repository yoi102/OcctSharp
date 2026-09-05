# U-W preparation and extended Q-W coverage

- Decision: [ADR-0083](adr/0083-extended-batches-and-continuous-execution.md).
- New scope: U/V/W, **40 capabilities each, 120 total; implementation 0/120**.
- Combined queue: Q-W, **7 prepared scopes, 280 capabilities; implementation 0/280**.
- Baseline commit: `eacd0ed`; product/DLL baseline remains the completed `6b04bd9` Preview.15.
- Continuous execution policy: [Q-W runbook](BATCH_CONTINUOUS_EXECUTION.md).
- Current request is preparation for the next run, not API implementation now.

## Additional whole batches

| Batch | Outcome | Actual increment over delivered/prepared work | Planned local version |
|---|---|---|---|
| U | [Advanced contour finishing and limit-driven local features](BATCH_U_ADVANCED_LOCAL_FEATURES_GAP_INVENTORY.md) | Contour laws/simulation/failure evidence, advanced chamfer modes, support/limit-driven BRepFeat features | 8.0.1-preview.20 |
| V | [Exact partition, material regions and volume construction](BATCH_V_PARTITION_VOLUME_GAP_INVENTORY.md) | Complete partition membership, multi-rule material regions/interfaces, face-set volume and bounded-void construction | 8.0.1-preview.21 |
| W | [Viewer lighting, presentation materials and copied frame capture](BATCH_W_VIEWER_LIGHTING_FRAME_CAPTURE_GAP_INVENTORY.md) | Viewer light rigs and texture/material programs, capability-aware rendering, copied RGBA/depth/tiled frames | 8.0.1-preview.22 |

All three scopes cross data/algorithm/result/document/viewer boundaries; the 40-row
matrices define finite acceptance. They are not class/getter batches and do not reopen
completed J/D/H or recount Q-T dependencies. The seven batches are a next high-value
tranche, not a promise to finish all remaining OCCT APIs.

Source comparison used `FeatureModeling.cs`, `FeatureModelingTypes.cs`,
`OcctViewer.cs`, `ViewerTypes.cs`, native `Modeling/Features.cpp`, plus completed
D/F/H/J and prepared Q-T matrices. The actual FeatureModeling implementation exposes
start/end radii and simple vector-depth Boolean-composed features, not the proposed
contour programs/limit-driven contracts. Existing cells have one take/avoid/material
selection; existing screenshot APIs already include RGB/RGBA/depth files. Those are
explicit reuse boundaries even where older broad matrix wording could suggest more.

## Exact-root evidence

The pinned inventory hash remains
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.
Its declaration totals remain 116,272 classified; 16,353 emitted, 709 accepted manual,
49,866 blocked, 49,344 skipped, zero pending/HD099. Preparation changes none of them.

| Scope | Exact roots | Candidate stable IDs | Blocked | Emitted | Manual | Skipped |
|---|---:|---:|---:|---:|---:|---:|
| U | 44 | 2045 | 943 | 431 | 113 | 558 |
| V | 43 | 2075 | 803 | 414 | 120 | 738 |
| W | 54 | 2582 | 1128 | 850 | 76 | 528 |
| U-W deduplicated | 105 | 4,290 | 1,886 | 927 | 181 | 1,296 |
| Q-W deduplicated | 219 | 7,668 | 3,513 | 1,561 | 305 | 2,289 |

U-W totals 6,702 candidate occurrences with 2,412 repeats. It overlaps Q-T by 1,911
distinct IDs and introduces 2,379 additional audit candidates (1,019 blocked, 447
emitted, 66 manual, 847 skipped). Across all seven batches, 15,634 occurrences contain
7,966 repeats; the distinct pool is 7,668, not 15,634 new APIs. Candidate IDs and
capability rows are separate denominators. Even blocked candidates are not promised
manual migration; reconcile only exact directly invoked declarations at implementation.

| Batch | Root audit SHA256 |
|---|---|
| U | `1F1780A57E636DECE9EA0C6BC786EECB2F80054A95086373917FB5110D22710A` |
| V | `D87D3206B4FD9F14E71D857D18077D7B53902EE45D6FF1FBEDA7B19A8DEF0AFF` |
| W | `185C479CD79C5F8AE09269C72958CD693435655EFE7EE7E751483EBB18DAD3AC` |

Each config is the disjoint union of decision and reused support roots. All exact roots
have current inventory declarations and matching SDK headers. U-W has 105 distinct
root headers; Q-W has 219 root headers plus the four previously recorded Q/S template
dependencies, totaling 223 distinct header checks. This is a product/root closure,
not proof that every future method compiles or every transitive header is exposed.

## SDK semantics checked before freezing

- Fillet Add/SetRadius supports copied radius laws and normalized parameter/radius
  samples. Simulate/Sect can expose copied section data. FaultyContour/FaultyVertex/
  StripeStatus and HasResult/BadShape must distinguish a failed partial result from
  an accepted shape.
- Chamfer AddDA, constant-throat and penetration modes exist; their dimensions are
  not interchangeable with the existing ordinary two-distance chamfer.
- BRepFeat_MakeDPrism exposes height, Until, From/Until and supported end/through modes;
  profile/base/support/sliding membership still needs implementation-time validation.
- CellsBuilder material 0 prevents boundary removal, a cell cannot carry conflicting
  materials, and cross-dimensional internal-boundary removal is unsupported.
  MakeContainers is finalized after region selection; subsequent additions do not
  automatically rebuild it. Native history does not guarantee every solid identity.
- MakerVolume builds zero/one/many solids; with SetIntersect(false), callers must
  guarantee non-interfering input or OCCT documents unpredictable results.
- Graphic3d_Texture2D supports Image_PixMap inputs; the proposed Texture2Dmanual name
  is not in this SDK and is not used. CubeMapPacked can wrap source memory internally,
  so the bridge must keep owned backing pixels, never borrow a released managed array.
- V3d_View.ToPixMap uses an offscreen render buffer associated with the existing view.
  V3d_ImageDumpOptions exposes size, aspect, tiling and layer targeting. This does not
  make the current HWND renderer headless or establish D3DImage composition.
- Driver capabilities and in-memory color/depth conventions require explicit runtime
  evidence. Pixel-exact reproducibility across different GPUs is not a contract.

| Existing CMake toolkit | Representative SDK export |
|---|---|
| TKFillet | `BRepFilletAPI_MakeFillet::Simulate` |
| TKFillet | `BRepFilletAPI_MakeChamfer::AddDA` |
| TKFeat | `BRepFeat_MakeDPrism::Perform` |
| TKBO | `BOPAlgo_MakerVolume::Perform` |
| TKBO | `BOPAlgo_CellsBuilder::RemoveInternalBoundaries` |
| TKBO | `BOPAlgo_ShellSplitter::Perform` |
| TKService | `Graphic3d_CLight::SetIntensity` |
| TKService | `Graphic3d_PBRMaterial::SetMetallic` |
| TKV3d | `V3d_View::ToPixMap` |

All nine symbols were found in the installed SDK DLLs. The verifier also checks toolkit
membership in the existing CMake closure. Null inventory SourceToolkit fields are not
used as link evidence. Native compiler/linker, driver and runtime checks are still
required for implementation.

## Source and lifetime closure

Keep twelve managed modules plus facade and one Native DLL. U/V use new cohesive
Modeling source units, not growth of the current generic feature switch. W uses
Visualization Lighting/Appearance/FrameCapture units sharing the existing viewer context.
T feature execution integration stays in higher existing owners; Documents does not
gain Modeling-feature/Mesh/XDE/viewer orchestration dependencies. Core frame DTOs do not
reference WPF. The WPF example consumes copied pixels only.

Reuse Runtime registry/TLS state and existing Shape ownership. Builders, PaveFiller,
fillet sections and iterators stay native-local; recipes/maps/frames are copied.
Shapes are owning; labels and viewer resource IDs are parent-bound, with existing
thread-affinity rules. Actual new light/texture/result lifecycle contracts must be
recorded in OWNERSHIP/SPECIAL_CASES/NATIVE_ABI during implementation. No new ownership
category, generated edit, ABI or version change occurs during preparation.

## Verification and future baseline changes

From the inner code workspace:

```powershell
.\eng\verify-prepared-batches.ps1 -Scope UVW
.\eng\verify-prepared-batches.ps1 -Scope QW
.\eng\verify-batch-q-t-preparation.ps1
```

The shared engine reads
[continuous-plan.json](../OcctSharp/config/batches/continuous-plan.json), validates the
queue and prerequisite ordering, matrices, root partitions, exact-root audit determinism,
headers, representative exports and wrong-hash/input-overwrite negative cases. The
old Q-T command is retained as a compatibility wrapper with the same report shape/hash.
Reports remain under ignored artifacts; no product bindings are built/generated.

These are frozen-preparation replay checks. After implementation baselines diverge,
run the next batch's individual `audit-batch-roots.ps1` against its explicitly updated
config/current inventory. Do not rebase all seven configs to make a shared replay pass.
Preserve the original evidence in Git and use the hash-named Preview.15 snapshot where
needed. The runbook specifies the per-batch delta and completion journal.

Executed preparation checks: UVW passes 120 rows, 105 headers, 9 symbols and 6 negative
cases; QW passes 280 rows, 223 headers, 16 symbols and 14 negative cases, with repeated
byte-identical root audits. U-W summary SHA256:
`38404B079DDD71866085898B0FB248CBA5B41F1ADDA42AA3850160618A2B3A53`.
Q-W summary SHA256:
`B62D2786E20D3B63016AF3811471EC2504FC42E7750AB610357E21D7FBA21458`.
The rerun Q-T wrapper preserves its original summary SHA256
`11CB6DA68AF4659AD14573A982F2DF068B1C12C5BA706427E8209FBDC68D2FB2`.
Sixteen touched Markdown files pass 297 local-link checks and balanced fences;
whitespace checks pass, with production/generated/version/runtime files unchanged.
Native source layout also passes (42 units, 530 C definitions, 22 unique storage owners;
binary comparison NOT RUN). Final documentation/compatibility evidence is in STATUS. New API
Release/Debug compilation, runtime/driver-success testing, packaging, DLL refresh,
signing, hosted CI, NuGet publication and GitHub push are **NOT RUN** in this checkpoint.

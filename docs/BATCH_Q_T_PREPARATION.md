# Prepared large batches Q through T

- Date: 2026-09-05.
- Decision: [ADR-0082](adr/0082-broad-batch-q-through-t-preparation.md).
- Scope preparation: **4/4 batches**, **160 capability rows** (40 per batch).
- Active implementation and local validation: **40/160**, all in Q. The frozen preparation evidence below is unchanged.
- Baseline: `6b04bd9`, completed B-P plus full historical Native extraction (ADR-0081).
- Delivery after implementation is one local checked commit per whole batch. No automatic NuGet publication or GitHub push.

ADR-0083 extends this initial queue with [U-W](BATCH_U_W_PREPARATION.md), producing
seven 40-capability batches. The [continuous runbook](BATCH_CONTINUOUS_EXECUTION.md)
governs next-run automatic advancement after each verified local commit. Q-T's scope,
frozen baseline and original audit evidence below remain unchanged.

## Why these are the next broad waves

The preceding P wave contained 32 capabilities; earlier product waves generally contained
24. Each new batch has 40 observable capabilities: 25% more than P and about 67% more than
a 24-row wave. Breadth comes from completing data -> algorithms -> owning result ->
document/exchange/viewer workflows, not subdividing methods or tests to inflate counts.
These four batches are a high-value next tranche, **not all remaining OCCT migration**.

| Order | Whole-batch outcome | New scope | Planned local version | Implementation |
|---|---|---|---|---|
| Q | [Shape repair and topology normalization](BATCH_Q_SHAPE_REPAIR_TOPOLOGY_GAP_INVENTORY.md) | 40 capabilities | 8.0.1-preview.16 | 40/40 locally validated |
| R | [Mesh authoring editing and discrete model delivery](BATCH_R_MESH_AUTHORING_EDITING_GAP_INVENTORY.md) | 40 capabilities | 8.0.1-preview.17 | 0/40 |
| S | [Guided sweeps and constrained surface authoring](BATCH_S_GUIDED_SWEEP_CONSTRAINED_SURFACE_GAP_INVENTORY.md) | 40 capabilities | 8.0.1-preview.18 | 0/40 |
| T | [Parametric document recompute and persistent topology selection](BATCH_T_PARAMETRIC_DOCUMENT_RECOMPUTE_GAP_INVENTORY.md) | 40 capabilities | 8.0.1-preview.19 | 0/40 |

At preparation the versions reserved OCCT-aligned slots under ADR-0065 against package
`8.0.1-preview.15`, ABI `1.59`, bridge `0.67.0` and schema `1.13`. Q now targets
Preview.16 / ABI 1.60 / bridge 0.68.0, retaining schema 1.13; all required local gates pass.
If a separate authorized fix consumes a preview slot, explicitly revise the preparation
record before implementation. Do not infer ABI/bridge numbers from package counters.

## Existing capabilities must not be counted twice

| Batch | Reused delivered surface | Actual increment |
|---|---|---|
| Q | Shape.RepairWithReport, ShapeFactory.Sew, FreeformAuthoring.Heal, J recovery/history, P pcurve consistency repair | Selected/protected multi-stage repair, defect provenance, tolerance/change budgets, normalization and composed mapping |
| R | AdvancedMesh.Create/CreateLods, H diagnostic counts, copied scenes/materials and exporters | Caller-authored/edited attributed meshes, actual topology changes and index maps, discrete owning topology and delivery |
| S | F rational definitions, fitting, basic FillBoundary including points, CreatePipeShell and loft; P UV/evaluation | Copied scalar laws, richer framing/contact/attachments, per-constraint support/residual contracts and patch provenance |
| T | I copied attributes/reference graph/topological order, transactions/history, NamedShape and four persistence formats | Executable typed feature graph, incremental dirty propagation, atomic recompute and fallible persistent selection |

Source evidence was checked in `Shape.cs`, `ShapeFactory.cs`,
`FreeformAuthoring.cs`, `AdvancedMesh.cs`, `AdvancedMeshTypes.cs`,
`OcafDocument.cs`, `DocumentStateApi.cs` and `DocumentStateTypes.cs`
under `OcctSharp/src/OcctSharp/`. Lower generated wrappers may already expose some
building blocks; a missing end-to-end contract is not automatically a missing raw method.

## Dependency closure and implementation order

Q -> R -> S -> T is the **delivery sequence**, not a claim that all algorithms depend
on the previous batch. Q/R/S reuse completed B-P capabilities. T genuinely depends on
Q repair recipes, R discrete results and S law/guide/constraint contracts for its typed
built-in operations.

| Layer | Shared prerequisite and rule |
|---|---|
| Inputs | Copied values/definitions, finite sizes, units, revision-scoped IDs; no borrowed OCCT collections |
| Algorithms | Existing OCCT SDK and core CMake toolkit closure; builders, iterators, laws and helpers native-local |
| Result | Existing Shape registry/release; copied diagnostics/maps; explicit unknown mapping and failed acceptance |
| Document | Existing OCAF/XDE owner/transactions; dedicated feature/selector labels; shared definitions versus occurrences |
| Integration | Existing exact/discrete format providers and real-HWND viewer; format and parent-lifetime boundaries retained |
| Distribution | Existing managed dependency DAG, facade, one Native DLL/runtime package and two clean-consumer paths |

All configured decision and support roots are audited together. Header-only templates
are recorded separately. Toolkit `SourceToolkit` values in the inventory are often null:
they are not used as evidence of linking. The already accepted full explicit
`OCCTSHARP_CORE_TOOLKITS` set is reused; seven representative exports were checked in
SDK DLLs and their toolkits were found in CMake. Every newly used method still requires
actual compile/link and lifetime verification during implementation. This is a frozen
product/root/ownership closure, not a proven new binary call graph.

R pure copied mesh data belongs below Modeling (MeshData); material objects and XDE
resolution stay above it. T storage/naming belongs in Documents, but built-in evaluation
that uses Mesh/Q/S/XDE is orchestrated by the existing facade/higher owners. Neither is
allowed to create a backwards managed/native dependency.

## Full-inventory and overlap evidence

Preparation baseline SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

The baseline contains 116,272 final-classified declarations and 7,090 headers (7,058
semantically parsed, 32 explicitly excluded). Generated = 16,353, accepted Manual = 709,
Blocked = 49,866, Skipped = 49,344; pending/HD099 = 0. These are classification facts,
not a claim that all APIs are wrapped, bindable or runtime-tested.

| Batch | Decision roots | Support roots | Total exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Q | 30 | 22 | 52 | 2306 | 1031 | 604 | 91 | 580 |
| R | 23 | 23 | 46 | 2213 | 1011 | 526 | 85 | 591 |
| S | 24 | 28 | 52 | 2432 | 1063 | 611 | 158 | 600 |
| T | 24 | 28 | 52 | 1981 | 866 | 536 | 101 | 478 |
| Deduplicated union | - | - | 149 | 5,289 | 2,494 | 1,114 | 239 | 1,442 |

Adding the four candidate counts produces 8,932 **occurrences**, including 3,643 repeated
occurrences. It does not produce 8,932 new APIs. Pairwise shared stable IDs are Q/R 1,175,
Q/S 1,286, Q/T 1,175, R/S 1,175, R/T 1,175 and S/T 1,175; these pairwise overlaps cannot
simply be subtracted because triple/quadruple intersections exist.

Candidates include existing emitted/manual members and skipped/blocked declarations.
The union's 2,494 blocked IDs are an investigation pool, not a promise to migrate all
of them in these 160 capabilities. Exact newly called manual exceptions are reconciled
at implementation time; root membership alone never justifies reclassification.
The total bindable-public denominator is not established by this preparation.

### SDK alias and semantic findings

- The initial exact-root audit correctly rejected `TopTools_IndexedMapOfShape`,
  `GeomLProp_CLProps` and `GeomLProp_SLProps`: those names have no exact declaration
  roots in the current inventory. OCCT 8 defines the first as a deprecated typedef and
  the latter two as template aliases. Q uses `NCollection_IndexedMap.hxx` with
  `TopTools_ShapeMapHasher.hxx`; S lists the two GeomLProp headers separately.
  The relevant headers are semantically parsed, not missing or HD099. No generator
  disposition is changed merely to make this audit pass.
- All 149 exact-root headers plus four explicit template/support headers exist: 153
  distinct header checks. `Poly_MergeNodesTool` is the real mesh merge class.
- ShapeDivideContinuity uses C0/C1/C2/C3/CN criteria, not G1/G2.
- MakePipeShell auxiliary-spine and homothetic-law modes are incompatible; auxiliary
  keep-contact has C0-only continuity. The S contract rejects incompatible requests.
- MakeFilling may ignore incompatible constraints; per-constraint residuals are required.
- TNaming_Selector.Select clears descendants, so T uses a dedicated selector label.
  Solve failure/ambiguity is a supported result, not a reason to guess topology.
- TDataStd_NamedData Has checks are required before getters to distinguish missing from zero.

| SDK toolkit already in CMake | Checked export |
|---|---|
| TKShHealing | ShapeFix_Wireframe::FixWireGaps |
| TKTopAlgo | BRepBuilderAPI_Sewing::Perform |
| TKMath | Poly_CoherentTriangulation::RemoveDegenerated |
| TKOffset | BRepOffsetAPI_MakePipeShell::SetLaw |
| TKOffset | BRepOffsetAPI_MakeFilling::Add |
| TKLCAF | TFunction_GraphNode::AddPrevious |
| TKCAF | TNaming_Selector::Solve |

## Frozen baseline and entry protocol

All four scopes were prepared on Preview.15. R/S/T are **scope-prepared**, not already
baseline-current after a future Q commit and not implementation-complete.

1. Before implementing Q, verify the accepted baseline/hash, configuration, SDK,
   source-layout checks and all 40 row contracts. Recover the exact initial inventory
   from the hash-named local preparation snapshot or regenerate it from `6b04bd9`
   with the pinned SDK if local artifacts are absent.
2. Finish one whole batch and its local gates, then make its local checkpoint commit.
   Do not begin the next batch on top of incomplete or unvalidated changes.
3. Before each following batch, rerun inventory on that new completed commit and compare
   stable IDs, states, callable signatures, source ownership and toolkit assumptions
   against this preparation snapshot. Explicitly record prior/new commit and hash plus
   added/removed/reclassified IDs and their effects on every impacted row.
4. Preserve the original configuration/evidence in Git history. Update the following
   batch's baseline configuration and evidence only through an explicit reviewed delta
   in that next batch's preparation, never a silent hash replacement. Keep its 40-row
   denominator; reuse capabilities completed in earlier waves rather than recounting them.
5. A genuine unsupported capability, new callback/ownership contract, optional SDK
   dependency or material scope change requires a documented decision before continuing.
   Do not drop rows, count tests as replacements or call partial completion 40/40.

A work interruption is not a new batch boundary. Recover the current matrix and complete
the same whole wave. Preparation-only requests stop after the preparation checkpoint.

## Source and lifetime constraints

Keep twelve managed module assemblies plus the facade and one native bridge/runtime
package. [The source map](NATIVE_SOURCE_LAYOUT.md) remains authoritative: 42 manual
translation units, 34 private headers and ten folders at this baseline. Suggested new
cohesive files are listed in each batch; none exists merely because it is planned here.
Use explicit CMake registration, independent compilation, acyclic private dependencies,
the existing 1,000-line ceiling and no manual PCH/unity. Do not rebuild a monolithic
Freeform.cpp or universal private header.

Reuse the unique registry/TLS error owners. Shapes are owning; DTOs and recipe/diagnostic
data are copied; labels are document-bound and viewer IDs remain viewer-bound/thread-affine.
Temporary algorithms and iterators never escape. New result owners require matching
release and recorded ownership. No new ownership categories are introduced by preparation.

## Required whole-batch implementation gates

These gates apply to all 40 rows, not a small selected demo subset:

1. Record exact public contracts, source placement, raw calls/manual stable IDs,
   ownership and ABI layout; fix generation rules and regenerate rather than hand-edit
   generated files. Update SPECIAL_CASES/OWNERSHIP/NATIVE_ABI and version identities only
   for actual changes.
2. Map every capability row to focused assertions, including unsupported/negative input,
   null/disposed/foreign/stale parent IDs, source mutation isolation and failure cleanup.
   Use numerical/geometric assertions, not only IsDone or non-null checks.
3. Release and Debug native/managed builds; complete Generator and Runtime regression;
   an isolated sweep with the **actual Debug native DLL**. Include bounded resource,
   repeated disposal and existing cross-domain/TLS boundary tests where applicable.
4. Run each batch's complete product scenario with deterministic fixtures and applicable
   real STEP/IGES/XDE and real-HWND review. Keep format limitations explicit for mesh-only
   or parametric-only data. Algorithm/source disposal must not invalidate accepted output.
5. Verify native source layout and private headers, dependency profiles and acyclic
   generated/managed closure, additive public compatibility, precise manual inventory
   reconciliation, deterministic reports and byte-identical clean regeneration.
6. Refresh the committed runtime/notice manifest for actual binary changes; build all
   existing local packages, validate facade and direct-module clean consumers and the
   complete local release metadata/checksum/SBOM/provenance checks. Packing is a check,
   not an instruction to publish.
7. Update STATUS, matrix acceptance evidence, roadmap and release documentation; run
   whitespace checks and create one local completion commit. Never include unrelated
   user changes. NuGet publication, hosted CI, signing and GitHub push remain separate.

## Reproduction and this preparation's validation boundary

From the inner code workspace:

```powershell
.\eng\verify-batch-q-t-preparation.ps1
```

The verifier uses four committed configs and the pinned full inventory. It reruns each
exact-root audit twice, verifies each 40-row matrix, disjoint decision/support partitions,
all headers and seven representative SDK exports/CMake toolkits, and exercises wrong-hash
and input-overwrite rejection for each batch (8 negative checks). It verifies real input
hashes did not change and writes the deduplicated/overlap report under ignored
`artifacts/generator-reports/batch-q-t-preparation-audit.json`. No production bindings
are generated or altered. The frozen baseline copy is under ignored
`artifacts/preparation-baselines/preview15-CCB81F47/full-inventory.json`.

Root audit hashes:

| Batch | SHA256 of both runs |
|---|---|
| Q | `DD1DAEE0D8D22F99EC8E3242FCF3136FB8326B39F243A8C9D5C68226D5A18F9F` |
| R | `5E684FCEE0294B9CC26D0069F8BA0D0CADF73FB4BE67268C6E9017EF69B055EF` |
| S | `3ADBD36992588EC691E20A2FD917228FB041B0B122B567943AB07C578F44E70C` |
| T | `9D26EC4CCCB68A7960B888023D8285FD6E682BBFB1A47960224F5C366508E428` |

The verifier passes: 160 rows, 153 headers, 7 representative symbols, 8 negative cases,
unchanged input hashes and deterministic per-batch reports. Summary report SHA256:
`11CB6DA68AF4659AD14573A982F2DF068B1C12C5BA706427E8209FBDC68D2FB2`.
At the original ADR-0082 checkpoint, the source-layout audit also passed (42 units,
530 C exports, 22 unique shared storage definitions; binary comparison NOT RUN).
Fifteen touched Markdown documents
passed 265 local-link target checks and balanced-fence checks; whitespace checks passed.
Current preparation evidence is also recorded in STATUS.
New API compilation, new runtime tests, packaging, DLL refresh, hosted CI, signing,
NuGet publication and GitHub push are **NOT RUN** in this preparation-only checkpoint.
Earlier 91/91 Generator and 180/180 Runtime results belong to the completed baseline;
they are not evidence for any Q-T implementation.

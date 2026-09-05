# Batch S: Guided sweeps and constrained surface authoring

- Status: scope preparation complete; implementation **0/40**; new compile/runtime gates **NOT RUN**.
- Decision: [ADR-0082](adr/0082-broad-batch-q-through-t-preparation.md).
- Preparation baseline: commit `6b04bd9`, package `8.0.1-preview.15`, OCCT 8.0.1.
- Planned local package slot: `8.0.1-preview.18`; not a version change or publication.
- Execution contract and shared gates: [Q-T preparation](BATCH_Q_T_PREPARATION.md).
- Frozen configuration: [batch-s-guided-sweep-constrained-surface.json](../OcctSharp/config/batches/batch-s-guided-sweep-constrained-surface.json).

## Product outcome and existing coverage

Batch F already supplies rational curve/surface definitions, fits, immutable edits, basic FillBoundary (including interior points), CreatePipeShell (profiles plus Frenet flag), controlled loft, sewing and healing. Batch P already supplies UV/evaluation/projection/trim and repair. S counts richer law/guide/constraint contracts and provenance, not these existing operations.

Copied profiles plus scalar law -> simulated sections -> guided sweep and history;
supported boundary/face/UV constraints -> seeded filling -> per-constraint residual
acceptance -> copied patch conversion -> XDE/STEP/IGES -> real viewer review. Include
an incompatible auxiliary/law combination, C0-only contact, ignored constraint, singular
derivative, reversed section and released input shape.

One row below is one independently observable workflow capability, not one getter,
native class, test case or commit. All 40 rows plus their dependencies and validation
form one batch. Internal source groups are implementation responsibilities, not smaller
delivery batches. The `Integration`/`Execution` groups compose the audited roots and
existing public operations; they do not imply new OCCT class roots.

## Frozen capability and acceptance matrix

All rows are prepared and unimplemented. Each acceptance statement is a required future
test, not a report that it has passed. Shared lifetime/negative/package gates apply to
every applicable row and are not counted as additional capabilities.

| ID | Root group | New capability | Required observable acceptance |
|---|---|---|---|
| S-01 | Law | Copied scalar-law definition contract | Represent bounded domains, type and coefficients without exposing Law_Function handles or mutable OCCT arrays. |
| S-02 | Law | Constant and linear scalar laws | Create and evaluate positive scale laws with domain and derivative contracts, including endpoint behavior. |
| S-03 | Law | Interpolated scalar laws with tangency | Interpolate parameter/value samples with optional endpoint derivatives; reject duplicate parameters and overshoot violating scale policy. |
| S-04 | Law | B-spline scalar-law authoring | Validate copied knots/multiplicities/poles and evaluate a Law_BSpline definition over its declared domain. |
| S-05 | Law | Smooth transition law authoring | Use Law_S for bounded transitions and verify supported endpoint values/derivatives numerically. |
| S-06 | Law | Composite piecewise scalar law | Combine ordered law spans, validate domain coverage/continuity and report discontinuities without implicit extrapolation. |
| S-07 | Law | Scalar-law domain mapping and trimming | Return independent mapped/trimmed definitions with chain-rule derivatives and explicit out-of-domain policy. |
| S-08 | Law | Scalar-law evaluation and extrema sampling report | Return copied values/derivatives and bounded samples for scale admissibility; distinguish sampled bounds from a proof of global positivity. |
| S-09 | Sweep | Typed guided-sweep definition | Freeze sections, attachments, frame, contact/correction modes and limits in one copied plan with source lifetime validation. |
| S-10 | Sweep | Fixed-frame sweep | Sweep with a declared gp_Ax2 frame and verify section orientation relative to the spine. |
| S-11 | Sweep | Fixed-binormal sweep | Use a declared direction and report degeneracy when the frame cannot be constructed. |
| S-12 | Sweep | Discrete-trihedron sweep | Expose supported discrete framing for eligible spines with explicit continuity diagnostics. |
| S-13 | Sweep | Support-surface framing | Use eligible support faces to define the trihedron and reject unsupported/missing spine support. |
| S-14 | Sweep | Auxiliary-spine sweep | Use an auxiliary guide with declared curvilinear-equivalence mode; verify section placement and source isolation. |
| S-15 | Sweep | Auxiliary-spine contact policies | Expose supported contact/keep-contact/border modes and report the actual continuity limitation. |
| S-16 | Sweep | Law-scaled profile sweep | Bind a positive scalar law to the supported profile path and reject incompatible auxiliary-spine combinations. |
| S-17 | Sweep | Location-attached multi-section sweep | Attach sections to explicit spine vertices with per-section contact/correction choices and deterministic ordering. |
| S-18 | Sweep | Sweep compatibility and readiness report | Preflight section counts/types/closure, mode conflicts and builder readiness with actionable copied diagnostics. |
| S-19 | Sweep | Section simulation before build | Return owning simulated section wires at requested stations for preview, with documented supported sampling limits. |
| S-20 | Sweep | Guided sweep error/status result | Expose algorithm status and approximation error separately from shape validity and requested continuity. |
| S-21 | Sweep | Guided sweep generated-topology history | Map input profile/spine topology to generated/modified result topology with explicit missing mapping. |
| S-22 | Sweep | Guided sweep solidification | Close eligible swept shells with end-section identity, reject invalid solidification and preserve the valid shell only under explicit policy. |
| S-23 | Sweep | Loft section compatibility control | Expose explicit compatibility correction/orientation behavior and report any changed section topology beyond F basic loft options. |
| S-24 | Sweep | Loft endpoint and edge provenance | Return first/last section and generated edge/face relationships for multi-profile lofts. |
| S-25 | Fill | Per-edge continuity constraints | Accept independent G0/G1/G2 requests per boundary edge rather than one global filling-continuity value. |
| S-26 | Fill | Boundary edge with support-face constraint | Associate an edge with its explicit support face and verify the requested tangential/curvature relationship. |
| S-27 | Fill | Interior edge constraint | Constrain non-boundary edges with optional support faces and keep them distinct from the outer boundary. |
| S-28 | Fill | Surface-parameter point constraint | Specify a point by support face and UV with continuity order; distinguish it from F existing free 3D point constraints. |
| S-29 | Fill | Initial-surface seeded filling | Load an eligible initial surface and report whether the constrained solve meets the same residual contract. |
| S-30 | Fill | Constrained-filling solver controls | Expose supported degree/discretization/iteration/anisotropy and separate geometric/angular tolerances with bounded resource inputs. |
| S-31 | Fill | Per-constraint fulfilment report | Return each constraint ID and G0/G1/G2 residual where defined; reject required constraints ignored by the algorithm. |
| S-32 | Fill | Constrained-fill topology and boundary history | Return an owning face with boundary-to-result correspondence and explicit degenerate/unmapped boundaries. |
| S-33 | Fill | Boundary-curve patch construction | Build eligible Bezier/B-spline patches using the supported GeomFill styles, distinct from variational MakeFilling. |
| S-34 | Convert | Composite curve-to-B-spline assembly | Join eligible curve spans with explicit tolerance/parameter policy and preserve span correspondence. |
| S-35 | Convert | Curve Bezier-span decomposition | Return copied Bezier pieces and original parameter intervals rather than borrowing converter-owned arrays. |
| S-36 | Convert | Surface Bezier-patch decomposition | Return a copied rectangular patch grid with UV span provenance and orientation. |
| S-37 | Convert | Curve and surface knot-span extraction | Extract independent requested B-spline spans/patches for downstream section/constraint authoring, retaining parameter maps. |
| S-38 | Convert | Constraint-boundary continuity verification | Compare positional/tangent/curvature residuals along supported joins with undefined derivative flags, separate from P single-face evaluation. |
| S-39 | Integration | Guide/constraint provenance in XDE delivery | Store result and recipe references, then STEP/IGES reopen geometry and supported metadata; OCAF-only recipe data is not promised in exchange formats. |
| S-40 | Integration | Guided authoring preview and result review | Review simulated sections, unsatisfied constraints and accepted result in the existing viewer; accepted geometry survives all temporary inputs. |

## Native decision roots and dependency closure

| Root group | Exact inventory roots |
|---|---|
| Law | `Law_Function`, `Law_Constant`, `Law_Linear`, `Law_Interpol`, `Law_Interpolate`, `Law_BSpline`, `Law_S`, `Law_Composite` |
| Sweep | `BRepOffsetAPI_MakePipeShell`, `BRepFill_PipeShell`, `BRepOffsetAPI_ThruSections` |
| Fill | `BRepOffsetAPI_MakeFilling`, `BRepFill_Filling`, `GeomFill_BSplineCurves`, `GeomFill_BezierCurves` |
| Convert | `Geom_BSplineCurve`, `Geom_BSplineSurface`, `GeomConvert_CompCurveToBSplineCurve`, `GeomConvert`, `GeomConvert_BSplineCurveToBezierCurve`, `GeomConvert_BSplineSurfaceToBezierSurface`, `BRepAdaptor_Curve`, `BRepAdaptor_Surface`, `GeomAPI_ProjectPointOnSurf` |

These 24 decision roots are a candidate audit, not a commitment to expose
every declaration. Reused support roots (28) are:

`TopoDS_Shape`, `BRepBuilderAPI_Copy`, `BRepCheck_Analyzer`, `BRepGProp`, `BRep_Tool`, `TopExp`, `TopLoc_Location`, `TDocStd_Document`, `XCAFDoc_ShapeTool`, `XCAFDoc_ColorTool`, `STEPCAFControl_Reader`, `STEPCAFControl_Writer`, `IGESCAFControl_Reader`, `IGESCAFControl_Writer`, `AIS_InteractiveContext`, `AIS_Shape`, `V3d_View`, `BRepTools_History`, `BRepBuilderAPI_MakeEdge`, `BRepBuilderAPI_MakeWire`, `BRepBuilderAPI_MakeFace`, `BRepBuilderAPI_Sewing`, `GeomAbs_Shape`, `BRepBuilderAPI_TransitionMode`, `BRepFill_TypeOfContact`, `gp_Ax2`, `gp_Dir`, `gp_Vec`.

Additional header-only/template dependencies: `GeomLProp_CLProps.hxx`, `GeomLProp_SLProps.hxx`.
They are intentionally outside exact-root declaration counts. See the alias explanation
in the shared preparation document.

Dependencies close through copied value/definition inputs, native-local algorithm and
container use, registered owning topology results, parent-bound documents and viewer
objects, and existing exchange providers. OCCT toolkit dependencies reuse the existing
explicit CMake core closure; availability evidence is not link/runtime proof for new code.

### Baseline audit evidence

Full inventory SHA256:
`CCB81F47CE09A7712D346C16EE45A9AF783D000DCFC64DF4B69FA3C1DE96DF48`.

| Exact roots | Candidates | Blocked | Emitted | Manual | Skipped |
|---:|---:|---:|---:|---:|---:|
| 52 | 2432 | 1063 | 611 | 158 | 600 |

Two audit runs are byte-identical. Report SHA256:
`3ADBD36992588EC691E20A2FD917228FB041B0B122B567943AB07C578F44E70C`.
Regenerate with `eng/audit-batch-roots.ps1` using the linked config and the pinned
inventory; report path is `artifacts/generator-reports/batch-s-root-audit.json`
inside the code workspace. Reused support accounts for much of these counts.
Candidates are neither 40 capabilities nor an implementation/API denominator.
Do not mark unrelated blocked/template/unsupported IDs manual merely because their
root appears here.

## Implementation ownership and source placement

Native: new `Modeling/ScalarLaws.cpp`, `Modeling/GuidedSweep.cpp`,
`Surfaces/ConstrainedFilling.cpp` and `Surfaces/PatchConversion.cpp`, with minimal private
contracts. Do not grow the existing 855-line `Modeling/Freeform.cpp`.
Managed definitions/operations remain with Geometry/Modeling as appropriate; facade
orchestrates XDE and viewer integration. Header-only GeomLProp aliases are native-local
template dependencies, not new generated root declarations.

Builders, adaptors, iterators and temporary arrays remain native-call-local; copied
results contain no borrowed pointers. Any owning result container needs a matching
release path and source-disposal tests. Shape owners reuse the current registration
and release family. Document labels and viewer IDs remain parent-bound and thread rules
remain unchanged. Concurrent release/use is not newly supported. Before introducing an
actual handle/layout/manual binding exception, update OWNERSHIP, NATIVE_ABI and
SPECIAL_CASES with exact directly invoked stable IDs; this preparation does not add one.

## Constraints and non-goals

Auxiliary-spine framing and homothetic scaling laws are incompatible in the current
MakePipeShell contract; reject that combination before build. Auxiliary-spine keep-contact
produces only C0 surfaces; do not advertise G1/G2 there. MakeFilling may ignore incompatible
constraints, so IsDone alone is not successful constraint satisfaction. Undefined curvature
at singularities is reported explicitly. No arbitrary procedural laws, managed virtual
proxies, general surface-network solver or interactive sketch constraint solver.

## Entry and completion gates

Use the shared [entry/delta protocol and validation gates](BATCH_Q_T_PREPARATION.md).
S follows the previous whole-batch checkpoint in the delivery sequence. It has no artificial hard dependency on every earlier new algorithm; the shared baseline must nevertheless be re-audited after preceding commits.
The capability count stays 40 when the baseline changes; already delivered capabilities
are prerequisites, not a reason to pad the denominator. A substantive unsupported
capability or changed product outcome requires an explicit documented scope decision,
not silent deletion or a smaller completion claim.

Completion requires a 40-row test mapping, Release/Debug builds and regression with the
actual Debug native DLL, source-layout/dependency checks, exact stable-ID reconciliation,
applicable real-file/HWND workflows, both clean package consumers, clean regeneration,
API/ABI compatibility, runtime manifest and local release evidence, documentation and
one local batch commit. No automatic NuGet publication or GitHub push.

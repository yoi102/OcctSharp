# ADR-0076: Recover disconnected STEP presentation styles into XDE

- Status: Accepted and implemented
- Date: 2026-09-02
- Scope: STEPCAF import, XDE presentation-style snapshots, viewer rendering, and WPF sample behavior

## Context

The existing WPF sample used one color per leaf occurrence. That preserved simple part
colors but could not render independent face, edge, material, alpha, or visibility styles.
Some valid-in-practice STEP files also attach `STYLED_ITEM` records to representation
targets that OCCT 8.0.1's normal product transfer does not transfer or map into the XDE
label tree. Those files imported usable geometry while most or all of the viewer fell
back to neutral RGB `(216, 224, 236)`.

Exposing STEP model entities, transfer binders, XCAF maps, labels, or AIS handles directly
would violate the accepted fixed-C-ABI and ownership rules. Reimplementing STEP styling in
WPF would duplicate OCCT's inheritance and occurrence-location semantics.

## Decision

Keep STEP model inspection and repair native-local. Before the normal STEPCAF document
transfer, explicitly transfer styled AP242 targets. After transfer, inspect transferred
styled targets, apply representation-relationship transforms, decode surface, boundary,
curve, rendering, and invisibility styles, and install those values in the destination
XDE color/visibility tools. If a styled shell-or-higher target is disconnected from the
ordinary product tree, add its recovered presentation geometry as a free XDE shape so it
is not silently omitted.

Expose `XdeLabel.GetPresentationStyles()` as a copied snapshot. Each entry contains an
independently owned, already located `Shape` plus copied visibility and optional surface,
curve, and material colors. No label, map, iterator, style object, or transfer session
crosses the ABI.

Expose `OcctViewer.Display(XdeLabel)` as one viewer-parent-bound `AIS_ColoredShape`.
Native code applies XCAF-inherited occurrence, part, subshape, material, alpha, and
visibility overrides before registration. The WPF sample displays whole XDE free roots
through this path and retains its neutral root color only as a fallback.

This additive change produces package `8.0.1-preview.12`, native ABI 1.56, bridge
0.64.0, and retains schema 1.13, managed assembly identity `0.1.0.0`, the 14-package
managed graph, and one 62-DLL Windows x64 native runtime package.

## Alternatives considered

- Continuing to display leaf occurrences with one color was rejected because it cannot
  represent independently styled faces, edges, transparency, invisibility, or malformed
  disconnected presentation targets.
- Reconstructing colors in managed/WPF code was rejected because STEP/XCAF style
  inheritance, mapped-item locations, and topology identity belong to OCCT.
- Returning native XCAF style or STEP entity handles was rejected because their lifetime
  depends on document/session-local C++ objects and containers.
- Switching the WPF renderer to `D3DImage` was not relevant to the missing-style cause and
  remains outside the sample due to OpenGL/D3D9Ex sharing and recovery cost.

## Consequences

- Common and atypically structured STEP files retain substantially more effective
  presentation color information after XDE import.
- A recovered disconnected styled target can appear as an additional free XDE root. This
  is deliberate: preserving visible authored presentation geometry is preferred to
  silently dropping it.
- Style snapshots must be disposed because each returned topology value owns a native
  shape handle. Viewer presentations remain creation-thread-affine and parent-bound.
- The recovery is an OCCT-8.0.1-specific native exception recorded as SC-050 and must be
  revalidated on each OCCT upgrade.

## Validation required

- Release and Debug native/managed builds and complete Generator/Runtime suites.
- Focused XDE style snapshot, ownership, STEP round-trip, and real-HWND rendering tests.
- Real-file checks on simple colored STEP and atypically structured multi-color STEP.
- WPF sample build and visual verification of non-uniform component/face colors.
- All 14 package contents, icon/readme metadata, clean facade/module consumers, committed
  runtime identity, generated freshness, full inventory, SBOM/provenance/checksums, and
  `git diff --check`.

## Related decisions

- ADR-0002: fixed native C ABI.
- ADR-0045: parent-bound XDE metadata and assemblies.
- ADR-0046: HWND/thread-affine viewer ownership.
- ADR-0053: composable XDE STEP import.
- ADR-0059: committed Windows runtime.
- ADR-0065: OCCT-aligned preview versioning.
- ADR-0074: managed modules and one shared native package.

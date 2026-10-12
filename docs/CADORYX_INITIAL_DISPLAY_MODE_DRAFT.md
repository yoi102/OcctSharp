# Cadoryx private initial display mode draft

2026-10-11. This downstream draft is separate from the active upstream delivery and development sources. It extends the hash-identified Cadoryx SDK26 snapshot under `OcctSharp/artifacts/cadoryx-b18/source-occurrence-1/OcctSharp`. The active checkout and SDK26 packages remain unchanged.

The private tested identity is `8.0.1-preview.31.cadoryx.initial-display.27`, ABI 1.86, bridge 0.94.0. The isolated source and 30 focused scenarios are frozen in [preparation-4.json](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/preparation-4.json). Native and managed Release and actual native Debug builds passed, with four managed APIs and three C exports added and none removed. The full requested inventory passed 1,346/1,346 in each configuration, retaining the original 1,316 plus 30 new cases. Fourteen immutable local packages and fresh locked public consumers in both managed configurations passed content, loaded-module and runtime audits. These consumers use the packaged Release native runtime; actual SDK Debug runtime evidence is separate. No application performance improvement or publication is claimed.

Evidence: [build/API/export audit](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/build-2-verification.json), [Release full 1346](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/runtime-release-full1346-3-verification.json), [actual Debug full 1346](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/runtime-debug-full1346-3-verification.json), [private package14](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/pack14-3-verification.json), [public consumer audit](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/public-consumer-6-verification.json), and [40 complete public images](../OcctSharp/artifacts/cadoryx-a5-initial-display-27-draft-1/public-images-1-verification.json).

Failures remain preserved. The first managed test build hit xUnit1031; only the new wrong-thread fixture was changed to an explicit secondary thread and Join. The first full Release run passed 1,328 and failed 18 existing corrupt-texture cases because the optional evidence directory was missing; only the launcher now creates that directory. The original public consumers completed successfully without manual native copying. Their independent auditor needed extended Windows path access for two cached packages over 260 characters, the correct `OcctViewer` assembly owner (`OcctSharp`), and the launcher's actual stderr filenames. No consumer cases, package contents, assertions, budgets or successful runtime artifacts were replaced or rerun to fix these audit errors.

## Behavior and manual binding exception record

`OcctViewer.DisplayInMode` adds four explicitly named managed routes: `Shape`, `XdeLabel`, `XdeOccurrence`, and `ViewerSource` plus a copied `GpTrsf` and optional existing `ViewerSourceIdentity`. These reuse the existing manual visualization binding category; generated files are unchanged. The corresponding three C ABI declarations and definitions are:

| Export | Managed route | Source owner |
| --- | --- | --- |
| `occtsharp_viewer_display_shape_in_mode` | Shape and located occurrence | `Visualization/Presentations.cpp` |
| `occtsharp_viewer_display_xde_label_in_mode` | Styled XDE label | `Visualization/Presentations.cpp` |
| `occtsharp_viewer_source_display_in_mode` | Viewer-owned copied source instance | `Visualization/ViewerSources.cpp` |

Each new native route validates mode 0/1 and sets the presentation mode before the first `AIS_InteractiveContext::Display`. The legacy managed methods and native export implementations remain unchanged. Named methods avoid making an existing call such as `Display(source, transform, default)` ambiguous.

On native failure the output ID remains zero and the new presentation registration is removed. The source instance continues to use the existing copied source, styles and correspondence map. The change must not reconstruct identity from coordinates, counts or traversal positions. The managed entry rejects a native ABI below 1.86 before calling a new export; matching package/runtime identity remains required.

## Ownership record

The existing viewer/thread/parent restrictions apply. Inputs are borrowed during the call; the native AIS presentation retains its own shape references or the existing copied source snapshot. Returned `ViewerPresentation` is tracked by its viewer and follows its existing removal/disposal contract. Source identity is retained only from an explicitly supplied identity or the existing occurrence properties. Isolation visibility continues through the existing viewer tracking path. No new ownership category or public structure layout is introduced.

This is a draft exception and ownership record. At SDK binding delivery, reconcile the implemented private variant with `SPECIAL_CASES.md` and `OWNERSHIP.md`, refresh its actual API/export inventory and report its exact delta. Do not add it to the active upstream delivered inventory merely because the source exists.

## Required verification

1. Build native and managed Release and actual native Debug; verify ABI, exports and public API. Retain original build and runtime cases.
2. Verify old defaults and both requested modes for shape, label, located occurrence and shared instance, with full image comparison.
3. Verify styles, topology selection, isolation, source/shape/document release, viewer disposal, wrong thread, foreign/removed source, invalid enum and direct C ABI error/output behavior.
4. Produce immutable private packages and test a public consumer against their actual loaded managed/native paths and hashes.
5. Use the original Cadoryx MiniPC3 input and the original fresh-process/20 dual-view-cycle requirements. Preserve the 6,482 ms target and existing failed runs. A different initial mode alone does not prove that computation was eliminated or that the application meets its performance requirement.

Gates 1–4 passed for this immutable private SDK variant. Gate 5 and Cadoryx application acceptance remain pending and separately tracked in the Cadoryx repository.

The 30 new scenarios cover five presentation families in both modes (10 complete-frame comparisons with legacy defaults), ordinary/styled shared sources under three transforms (six exact original-member selection cases), five caller-release and appearance restoration cases, five managed thread/enum/disposed-owner cases, foreign/removed sources with active isolation (one case), and direct C ABI output/error behavior for three exports (three cases). Existing 1,316 runtime tests remain, with only their exact ABI and bridge version expectations updated. Discovery and complete runs confirmed the exact 1,346 in each configuration. The three legacy C ABI display bodies and all generated inputs are byte-preserved in the source guard. Forty focused complete PNGs passed 10 direct reviews and 30 exact whole-byte reuses; the public consumer's 40 complete PNGs passed seven direct reviews and 33 exact reuses.

Cadoryx's original unified material/F5/cleanup candidate passed full 6,109 in both configurations, WPF publishing, desktop radial/window checks, and the original compiled material82 consumer with 346 full PNGs. The small initial-display application patch and the control-only AI response fix are now prepared in a new isolated unified source with expected inventory 6,122 (original 6,109 plus 13). Its locked restore/build/regression queue remains a separate application gate, followed by the original MiniPC3 two fresh processes and 20 dual-view cycles each with the unchanged 6,482 ms requirement. The production Cadoryx SDK26 baseline, upstream development sources and published inventories remain unchanged.

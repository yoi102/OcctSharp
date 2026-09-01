# Batch M interactive placement editing gap inventory

This document locks the product denominator, dependency closure, and exit evidence for
Batch M. It measures one interactive assembly-placement workflow, not isolated AIS,
V3d, gp, TopLoc, XDE, OCAF, DMU, or STEP class counts.

Preparation status: **COMPLETE**. Implementation status: **COMPLETE (24/24)**. The
denominator below is immutable for Batch M.

## Product outcome

A Windows x64 .NET application can attach an OCCT manipulator to a managed viewer
presentation, preview rigid occurrence placement, apply or cancel it, commit the result
inside named XDE history, recheck assembly interference, and round-trip the edited
assembly without exposing AIS pointers or native object layouts.

```text
viewer-parent-bound presentation plus finite manipulator policy
  -> native-local AIS manipulator and copied gp_Trsf preview
  -> optional rigid XDE occurrence-placement edit session
  -> named OCAF transaction, undo/redo, DMU recheck, STEP/XDE and real-HWND evidence
```

## Locked 24-capability denominator

| # | Family | Stable capability | Batch M exit evidence |
|---:|---|---|---|
| 1 | Viewer | Create one viewer-parent-bound manipulator | Parent registry fixture passes |
| 2 | AIS | Attach it to one live presentation | Attach/detach fixture passes |
| 3 | AIS | Enable translation mode | Mode configuration fixture passes |
| 4 | AIS | Enable rotation mode | Mode configuration fixture passes |
| 5 | AIS | Enable scaling for ordinary presentations | Scale-preview fixture passes |
| 6 | XDE | Reject scale/mirror for rigid occurrence editing | Rigid validation fixture passes |
| 7 | AIS | Enable/disable each axis/mode visual part | All axis/mode combinations pass |
| 8 | AIS | Configure activation on detection | State/configuration fixture passes |
| 9 | Geometry | Set copied position/orientation with `gp_Ax2` values | Finite orthonormal fixture passes |
| 10 | AIS | Configure finite size and gap | Boundary validation passes |
| 11 | AIS | Configure zoom persistence | State fixture passes |
| 12 | AIS | Select shaded or flat skin | State fixture passes |
| 13 | AIS | Copy attached/active mode, axis, transformation, and appearance state | Snapshot fixture passes |
| 14 | Interaction | Start a mouse transformation | Real-view fixture passes |
| 15 | Interaction | Update a mouse transformation and return an owning transform | Real-view fixture passes |
| 16 | Interaction | Preview a caller-supplied owning `GpTrsf` | Custom-preview fixture passes |
| 17 | Interaction | Apply a started transformation | Apply fixture passes |
| 18 | Interaction | Cancel and restore the start transformation | Rollback fixture passes |
| 19 | Ownership | Enforce detach, disposal, parent mismatch, and thread affinity | Negative/lifetime fixtures pass |
| 20 | Presentation | Get, set, and reset one presentation-local transform | Round-trip fixture passes |
| 21 | XDE | Preview and commit one occurrence-local rigid placement | Replacement-label fixture passes |
| 22 | OCAF | Commit under a named transaction and preserve undo/redo | History fixture passes |
| 23 | DMU | Recheck moved occurrence clearance/interference | Incremental/full comparison passes |
| 24 | Exchange/package | STEP/XDE round-trip plus real HWND screenshot from a clean package | Preview.11 consumer passes |

No AIS-only, XDE-only, mouse-only, placement-only, numbered, or dotted fragment is a
Batch M completion point.

## Root-declaration audit

The Preview.10 final inventory was queried for exactly these 24 decision-driving roots:
`AIS_Manipulator`, `AIS_InteractiveObject`, `AIS_InteractiveContext`,
`AIS_ColoredShape`, `AIS_DragAction`, `AIS_ManipulatorMode`,
`PrsMgr_PresentableObject`, `V3d_View`, `Graphic3d_Camera`, `gp_Trsf`, `gp_Ax2`,
`gp_Ax1`, `gp_Pnt`, `gp_Dir`, `TopLoc_Location`, `TopLoc_Datum3D`,
`XCAFDoc_ShapeTool`, `XCAFDoc_Location`, `TDocStd_Document`, `TDF_Label`,
`BRepBuilderAPI_Transform`, `BRepExtrema_DistShapeShape`, `Bnd_OBB`, and
`STEPCAFControl_Writer`.

| Inventory state | Count | Meaning |
|---|---:|---|
| `Blocked` | 662 | Requires native-local handles, copied values, or workflow composition |
| `Emitted` | 516 | Reused only where generated ownership already matches |
| `Manual` | 60 | Existing transform/XDE/DMU/viewer behavior is reused |
| `Skipped` | 412 | Destructors, operators, protected helpers, and unsafe declarations stay excluded |
| **Total** | **1,650** | Deduplicated audit candidates; product completion remains the 24 rows above |

Only the eight blocked overloads directly invoked by the new bridge may be reconciled
under SC-049. The remaining 654 blocked candidates keep their prior dispositions.

## Cross-family dependency closure

- `AIS_Manipulator`, AIS presentations, contexts, V3d views/cameras, and their selection
  owners stay viewer-local and thread-affine. A managed manipulator carries only a
  viewer-parent-bound integer ID.
- `gp_Trsf`, `TopLoc_Location`, and `gp_Ax2` cross through existing opaque owners or
  fixed copied values; no C++ layout is exposed.
- Presentation removal and viewer disposal detach and invalidate dependent manipulators
  before releasing AIS state. Parent mismatch and post-disposal calls fail deterministically.
- An occurrence edit session retains copied original/preview transforms. Preview changes
  only the viewer presentation. Commit validates rigid placement and calls
  `XdeDocument.RelocateOccurrence` inside a named transaction, returning the replacement
  label; cancel restores the original presentation transform.
- DMU algorithms and STEP/XDE transfer state remain call-local. Results retain the
  existing copied/owning contracts.
- Managed module ownership stays in the `OcctSharp` cross-family facade. Native ownership
  remains one `OcctSharp.Native.dll`; no native split or VTK dependency is introduced.
- The completed additive wave reserves package `8.0.1-preview.11`, native ABI 1.55,
  bridge 0.63.0, and binding schema 1.13.

## Validation gates

Batch M reaches 24/24 only when SC-049 exact reconciliation, focused tests, complete
Release and Debug builds, Generator/Runtime suites, real STEP/XDE plus real-HWND evidence,
the clean 62-DLL package consumer, deterministic generation, clean regeneration,
compatibility/inventory/runtime hashes, SBOM/provenance/checksums, documentation, and the
complete Preview.11 local release check pass together.

All gates above pass. Focused Batch M tests pass 4/4; Release and Debug each build all
19 projects with zero warnings and zero errors and pass Generator 91/91, Runtime 151/151,
and dependency profiles 6/6. The clean facade consumer executes the complete inherited
Batch D-M workflow and the direct Modeling consumer remains facade-free. Generation and
clean regeneration agree on 94 files and 16,353 bindings. Final classification is
16,353 emitted, 542 manual, 49,344 skipped, 50,033 blocked, and zero supported-unselected
or pending declarations. API comparison is additive at 38,781 additions and zero
removals. The complete Preview.11 local release check passes; hosted execution, signing,
publication, GitHub, and push remain `NOT RUN`.

## Explicit non-goals

Native-DLL splitting, VTK/IVtk integration, a cross-platform viewer backend, freeform
deformation, physics/motion planning, arbitrary callbacks, concurrent viewer/document
mutation, hosted release, signing, NuGet publication, GitHub, and push are outside Batch M.

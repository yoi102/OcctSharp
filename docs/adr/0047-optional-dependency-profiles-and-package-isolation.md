# ADR-0047: Optional dependency profiles and package isolation

- Status: Accepted
- Date: 2026-08-23
- Scope: B18 optional integrations for the OCCT 8.0.1 Windows x64 artifact

## Decision

Optional integrations are classified by a versioned dependency-profile manifest and a
reproducible artifact audit. A toolkit or header being present is not sufficient to mark
a profile available: its development headers, transitive runtime files, target platform,
language mode, and intended product use must all be satisfied.

The normal `OcctSharp` package remains independent of VTK, EGL/GLES, Tcl/Tk, Draw, and
non-Windows window systems. When prerequisites are supplied and validated, IVtk belongs
in `OcctSharp.IVtk` and OpenGL ES belongs in `OcctSharp.Visualization.OpenGles`; they do
not add assets to the core package. Draw/test harness toolkits have no public runtime
package. Cocoa/X11 adapters require future platform profiles. C++/CLI-only declarations
are excluded from the native-C-ABI generator.

Profile states are explicit: `Available`, `BlockedExternalDependency`,
`UnavailableInArtifact`, `UnavailablePlatform`, `ExcludedLanguage`, or
`IgnoredByDesign`. The audit fails when observed prerequisites no longer match the
expected state, forcing an intentional compatibility update.

## Validation

`eng/audit-dependency-profiles.ps1` validates all six configured profiles against the
pinned artifact and writes `artifacts/generator-reports/dependency-profiles.json`.
The current audit is 6/6 classified: Windows WNT/OpenGL is available; IVtk and OpenGL ES
are blocked by named absent third-party headers/runtime files; Draw is test-only;
Cocoa/X11 are unavailable on `windows-x64`; and `NCollection_Haft.h` is C++/CLI-only.
Release and Debug builds invoke the audit so dependency drift fails the normal build.

## Upgrade impact

Re-run the audit after any OCCT artifact, compiler, platform, or third-party SDK change.
If VTK or EGL/GLES becomes available, pin its exact version and provenance, add compile
and runtime validation, create the isolated package, and update the expected state rather
than silently adding its binaries to `OcctSharp`.

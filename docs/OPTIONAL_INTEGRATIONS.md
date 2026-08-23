# Optional Integrations

Optional OCCT surfaces are kept outside the core dependency closure. Their reproducible
source is `OcctSharp/config/dependency-profiles.json`; run the audit from the inner
workspace:

```powershell
.\eng\audit-dependency-profiles.ps1
```

The generated report is
`OcctSharp/artifacts/generator-reports/dependency-profiles.json`. It is build evidence,
not a committed substitute for the manifest.

## Current Windows x64 profile states

| Profile | State | Package boundary | Evidence |
|---|---|---|---|
| WNT/OpenGL viewer | Available | Current `OcctSharp`; future `OcctSharp.Visualization` | 7 WNT headers and TKService/TKOpenGl/TKV3d lib+DLL pairs; B17 runtime/package tests pass |
| IVtk | BlockedExternalDependency | Future `OcctSharp.IVtk` | 23 IVtk headers and TKIVtk/TKIVtkDraw exist, but VTK 9.4 headers and DLLs such as `vtkCommonCore-9.4.dll`/`vtksys-9.4.dll` are absent |
| OpenGL ES | BlockedExternalDependency | Future `OcctSharp.Visualization.OpenGles` | TKOpenGles exists, but EGL/GLES headers plus `libEGL.dll` and `libGLESv2.dll` are absent |
| Draw/test harness | IgnoredByDesign | No public package | 63 Draw headers and TKDraw exist; Tcl/Tk runtime is absent and the surface is an OCCT test/command harness |
| Cocoa/X11 adapters | UnavailablePlatform | Future platform-specific profile | Cocoa, Xw, and XAtom entry headers are catalogued but are not Windows adapters |
| C++/CLI Haft | ExcludedLanguage | No package | `NCollection_Haft.h` explicitly requires C++/CLI, while OcctSharp uses a portable native C ABI |

`BlockedExternalDependency` is not treated as a failed core build. It means the profile
cannot be compiled or runtime-tested from the pinned artifact alone. Installing an SDK
does not automatically enable it: version pinning, licensing/provenance, native closure,
ownership rules, package tests, and a compatibility row are required first.

## Package rules

- Optional packages depend inward on stable OcctSharp packages; core never depends on them.
- Optional native assets still deploy beneath the application's single `occt/` directory.
- Duplicate native filenames with different hashes are a package error.
- Draw/test binaries are development tools and are not redistributed through runtime NuGet.
- Platform adapters are validated and packaged per RID; a header catalog on Windows is
  not evidence for Linux or macOS support.

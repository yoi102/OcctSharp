# Pinned Windows dependency recovery

The independent `vcpkg-occt801-d3d-media-original-headers-v2` provider is defined in
`OcctSharp/config/dependencies/`. It fixes the registry commit, signed vcpkg
tool hash/version, OCCT source archive, x64-windows target/host triplets,
features and port versions. Transitive versions are fixed by the registry
baseline. The vcpkg checkout lives outside this Git repository.
This is an OcctSharp contributor build toolchain. Cadoryx and other NuGet
applications restore the published package family, including native DLLs;
they do not install or run vcpkg.

`OcctSharp.Native.win-x64` remains the shared native runtime distribution package
for the managed package family. It supplies the qualified Windows x64 bridge
and its exact dependent DLL set through transitive build assets. No replacement
runtime package is selected by this source-build route, so the package remains
supported and is not deprecated. Building OcctSharp with vcpkg does not replace
the native runtime dependency in applications.

The OCCT overlay is copied from port tree
`03ee9b71f9ce479ebafeab28f92182d13c779553` at registry commit
`e456309491fd875487bf02b8a0dca76c1b1b00cc`. Its explicit changes enable D3D,
enable fixed FFmpeg9.0.2#1, disable OpenVR, and preserve original SDK header bytes for the existing
generator fingerprint contracts. Draw and IVtk remain outside the core bridge.
OCCT V8_0_1 identifies source commit
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`.

The first provider disabled FFmpeg and compiled successfully, but its complete
Release Runtime run passed2253/2255: the two signed64-bit Media time conversions
returned0 in OCCT's disabled-feature branch. The v2 profile preserves the original
native calls and assertions and enables FFmpeg with fixed avcodec/avformat/swscale
features; the fixed port also resolves avdevice, avfilter and swresample.
Its source archive hash and port tree are recorded in `provider.json`;
the complete dependency and runtime/license qualifications have separate receipts.

From the inner `OcctSharp/` workspace, on Windows x64 with .NET 10 and Visual
Studio 2026 C++ tools installed:

```powershell
./eng/restore-dependencies.ps1 -RequireEmpty -NoBinaryCache
./eng/build-vcpkg.ps1 -InstallRoot ./artifacts/dependencies/vcpkg-installed
./eng/build-vcpkg.ps1 -InstallRoot ./artifacts/dependencies/vcpkg-installed -Configuration Debug
./eng/verify-vcpkg-runtime.ps1 -BuildRoot ./build/vcpkg-native
./eng/verify-vcpkg-runtime.ps1 -BuildRoot ./build/vcpkg-native -Configuration Debug
./eng/audit-vcpkg-runtime.ps1 -InstallRoot ./artifacts/dependencies/vcpkg-installed -BuildRoot ./build/vcpkg-native
./eng/audit-vcpkg-runtime.ps1 -InstallRoot ./artifacts/dependencies/vcpkg-installed -BuildRoot ./build/vcpkg-native -Configuration Debug
```

`-VcpkgRoot` selects an external machine cache. `-InstallRoot` selects a local
installation prefix. `-RequireEmpty` refuses existing contents; choose a new
prefix for each independent recovery. `-UseCurlDownloads` uses vcpkg's documented
asset download hook where the native Windows downloader stalls; vcpkg retains
its original SHA512 checks. `-NoBinaryCache` proves source compilation rather
than reusing compiled packages. Downloaded source archives may be reused only
after their fixed hashes pass.

Recovery produces `occtsharp-provider.json` and an exact-byte `occtsharp-sdk/inc`
view of the installed headers. An existing view must have the same names and
bytes, with no stale headers. The build rechecks provider inputs, runs the
generator's existing exact header contracts, compares all public generated
files, compiles the native bridge in a separate output root, and resolves its
actual recursive PE dependency closure. Unresolved imports, conflicting DLL
names or dependencies outside the selected prefix/system directories fail.

The core installation contains7295 flat SDK headers. The generator view retains
the full7478-header set:183 optional Draw/test/IVtk headers are recovered from
the exact source archive, with explicit archive paths and SHA256 identities in
`generator-header-supplement.json`. Both the archive SHA512 and every header
hash must match. This also resolves installed legacy aliases referring to those
headers without building optional toolkits. Required generation headers, exact
support fingerprints and399 generated-file comparisons remain enforced. The
public116366-declaration inventory stays unchanged; recovering optional headers
does not establish optional module runtime delivery.

The configuration-specific manifests record DLL names, sizes and hashes.
Compilation alone is recorded as `BUILD_PASS_RUNTIME_NOT_RUN`. Runtime and
consumer evidence must be recorded separately. The historical precompiled
provider still requires its exact 62-DLL set. A vcpkg closure has its own exact
manifest and cannot reuse that count or the precompiled provider's raw-header,
DLL, runtime, license or package evidence.

Runtime verification isolates the managed full Runtime suite, replaces its
entire native directory with the selected configuration's exact manifest,
and requires every test to execute and pass. The provenance audit maps each
non-bridge DLL back to one installed port file and version and copies the
exact port copyright text. Missing/changed payloads and unknown DLL owners
fail. The project bridge carries the project license. These results supplement
the build manifest; they never turn compilation alone into runtime evidence.

Preview.85 publication continues to use the precompiled provider qualified by
its separate full delivery gates. Switching a published payload to vcpkg
requires new full configuration, runtime, license and clean consumer evidence.
Current recovery/build evidence is linked from STATUS and the Preview.85 plan.

# Pinned Windows dependency recovery

The independent `vcpkg-occt801-d3d-original-headers-v1` provider is defined in
`OcctSharp/config/dependencies/`. It fixes the registry commit, signed vcpkg
tool hash/version, OCCT source archive, x64-windows target/host triplets,
features and port versions. Transitive versions are fixed by the registry
baseline. The vcpkg checkout lives outside this Git repository.

The OCCT overlay is copied from port tree
`03ee9b71f9ce479ebafeab28f92182d13c779553` at registry commit
`e456309491fd875487bf02b8a0dca76c1b1b00cc`. Its explicit changes enable D3D,
disable FFmpeg/OpenVR, and preserve original SDK header bytes for the existing
generator fingerprint contracts. Draw and IVtk remain outside the core bridge.
OCCT V8_0_1 identifies source commit
`b8f597c677811d1f9f4d8a97f5ae2825c0353a42`.

From the inner `OcctSharp/` workspace, on Windows x64 with .NET 10 and Visual
Studio 2026 C++ tools installed:

```powershell
./eng/restore-dependencies.ps1 -RequireEmpty -NoBinaryCache
./eng/build-vcpkg.ps1 -InstallRoot ./artifacts/dependencies/vcpkg-installed
./eng/build-vcpkg.ps1 -InstallRoot ./artifacts/dependencies/vcpkg-installed -Configuration Debug
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

The configuration-specific manifests record DLL names, sizes and hashes.
Compilation alone is recorded as `BUILD_PASS_RUNTIME_NOT_RUN`. Runtime and
consumer evidence must be recorded separately. The historical precompiled
provider still requires its exact 62-DLL set. A vcpkg closure has its own exact
manifest and cannot reuse that count or the precompiled provider's raw-header,
DLL, runtime, license or package evidence.

Preview.85 publication continues to use the precompiled provider qualified by
its separate full delivery gates. Switching a published payload to vcpkg
requires new full configuration, runtime, license and clean consumer evidence.
Current recovery/build evidence is linked from STATUS and the Preview.85 plan.

# OcctSharp 0.1.0-alpha.50

## Summary

Alpha.50 is the clone-and-run distribution release. It keeps the alpha.49 generated API,
native ABI 1.41, bridge 0.49.0, and OCCT 8.0.1 baseline, and makes the Windows x64 Sample
runnable from an ordinary Git clone without a separate OCCT SDK or C++ toolchain.

## Distribution changes

- Added the MIT project license and NuGet `MIT` license expression.
- Committed the accepted 62-DLL Release runtime below `runtime/win-x64/occt/`.
- Added OCCT, oneTBB, FreeImage, FreeType, OpenVR, FFmpeg, and jemalloc license texts and
  component notices to both the repository runtime and package.
- Added a manifest containing path, size, and SHA256 for every DLL and notice file.
- Added CI verification plus Debug/Release Sample smoke jobs that require no OCCT SDK.
- Kept ADR-0051's native SDK bootstrap as an explicit contributor override.

## Run

```powershell
git clone https://github.com/yoi102/OcctSharp.git
cd OcctSharp\OcctSharp
dotnet run --project .\samples\OcctSharp.Samples -- --smoke
```

The smoke validates 62 copied DLLs, runtime identity, and six-face OCCT box creation.

## Validation

- Independent fresh clone without local settings or OCCT environment: manifest,
  Release smoke, Debug smoke, and pack PASS.
- Complete release check: Release/Debug Generator 62/62 and Runtime 105/105 PASS.
- Rebuilt Release 62-DLL closure is byte-identical to the committed runtime.
- Clean-source regeneration: 13 generated files byte-identical; API diff 36,602 added,
  0 removed; full classification 116,272 declarations and 7,090 headers.
- Alpha.50 clean package consumer, SBOM, provenance, checksums, and whitespace gates PASS.

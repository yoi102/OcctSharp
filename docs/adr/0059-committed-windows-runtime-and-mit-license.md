# ADR-0059: Commit the Windows x64 runtime and license OcctSharp under MIT

- Status: Accepted
- Date: 2026-08-27
- Scope: Repository distribution, samples, and package inputs
- Supersedes in part: ADR-0051

## Context

ADR-0051 deliberately kept native binaries out of Git and required a pinned OCCT SDK
or immutable SDK archive before the repository Sample could build. In practice this
meant an ordinary clone could restore managed dependencies but could not run even the
basic example without private machine configuration and a C++ toolchain.

The project owner now requires the public repository itself to contain the runtime DLLs,
an MIT project license, and enough third-party license material for another Windows x64
user to clone and run the Sample directly. The accepted Release closure is 62 DLLs,
98,990,032 bytes in total, and no individual file exceeds GitHub's 100 MiB object limit.

## Decision

- License OcctSharp project code under the MIT License at repository-root `LICENSE` and
  declare `MIT` in NuGet metadata.
- Commit the accepted Release runtime below `OcctSharp/runtime/win-x64/occt/`. Both Debug
  and Release repository Sample builds use this ABI-compatible Release runtime by default.
- Commit upstream license texts and a component/DLL notice below
  `OcctSharp/runtime/win-x64/licenses/` and include the same material in the package.
- Pin every committed runtime and notice file by path, byte size, and SHA256 in
  `runtime-manifest.json`; `eng/verify-bundled-runtime.ps1` is a local and hosted-CI gate.
- Prefer the committed runtime when `OcctSharpNativeRuntimeDir` is not overridden.
  Setting `OcctSharpUseBundledNativeRuntime=false` preserves ADR-0051's developer
  bootstrap for regeneration and native-source validation.
- Keep the runtime below the inner code workspace and copy it application-locally below
  output `occt/`; do not load from `PATH` or a machine-wide OCCT installation.

## Alternatives considered

- Keeping only the SDK bootstrap was rejected because it does not satisfy clone-and-run.
- Git LFS was not selected because no DLL exceeds GitHub's 100 MiB per-file limit and
  ordinary `git clone` must receive the runnable assets without an extra LFS dependency.
- Committing Debug and Release copies was rejected because the Release bridge and runtime
  load correctly from both managed configurations and a second closure would duplicate
  roughly 99 MB without adding a supported platform.

## Consequences

- A Windows x64 user with .NET SDK 10.0.400 can clone and run the console smoke without
  OCCT, CMake, Visual Studio C++, private settings, or an artifact URL.
- The repository grows by approximately 94.4 MiB before Git compression, and ordinary
  clones receive that cost.
- LGPL/exception and other upstream terms still apply to their respective DLLs; MIT
  applies only to OcctSharp project code and the repository-built bridge.
- Native source changes require rebuilding and deliberately refreshing the committed
  runtime plus manifest; generated output is still changed only through regeneration.

## Validation required

- Verify the manifest and exact 62-DLL closure.
- Clone the committed tree into a clean directory with no local settings or OCCT
  environment variables, then run Debug and Release Sample smoke commands.
- Pack and run the clean package consumer from the committed runtime.
- Run hosted bundled-runtime smoke on every push and pull request.

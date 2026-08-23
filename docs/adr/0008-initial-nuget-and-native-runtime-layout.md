# ADR-0008: Use One Initial NuGet Package and an occt Runtime Directory

- Status: Accepted
- Date: 2026-08-21

## Context

The first package supports only .NET 10 on Windows x64 and requires the OcctSharp
native bridge plus its complete OCCT and third-party DLL dependency closure. Consumers
should not install OCCT separately, modify `PATH`, or copy native files by hand. Native
files should also remain visibly separate from the application's managed files.

## Decision

- Produce one initial `OcctSharp` NuGet package containing the managed assembly and the
  Release Windows x64 native runtime closure.
- Store package-native files below `buildTransitive/win-x64/occt/`. An imported
  `OcctSharp.targets` copies them to `$(TargetDir)/occt/` for build and publish.
- Keep the runtime directory flat and name it exactly `occt`. The application executable
  and `OcctSharp.dll` remain one level above it.
- Register one managed assembly-level native-library resolver. Every manual and generated
  P/Invoke class ensures registration before its first native call. The resolver loads
  `AppContext.BaseDirectory/occt/OcctSharp.Native.dll` by absolute path so its colocated
  dependencies participate in native loading.
- Do not search the current directory, mutate process `PATH`, install system-wide DLLs,
  or fall back silently to an unrelated OCCT installation.
- Keep this package experimental and local until project licensing, complete third-party
  notices, native provenance, CI production, and public release gates are resolved.

## Alternatives

- A managed package plus a separate RID-specific package was deferred because the initial
  matrix has only one RID. The split can be reconsidered when a second platform exists.
- Copying native DLLs beside the executable was rejected because the complete closure is
  large and should have one recognizable application-local boundary.
- Requiring users to set `PATH` or install OCCT globally was rejected because it is fragile,
  can load the wrong binary set, and breaks clean-consumer reproducibility.
- Using only standard `runtimes/win-x64/native` flattening was rejected because it does not
  preserve the required `occt` subdirectory layout.

## Consequences

- The initial package is larger but self-contained for the validated Windows x64 runtime.
- Build and publish outputs have a deterministic `occt` directory containing the bridge
  and all inspected native dependencies.
- Consumers receive automatic native loading and actionable missing-runtime diagnostics.
- A future multi-RID package split is a package-architecture change requiring another ADR.

## Validation

- Inspect the `.nupkg` and count the expected native closure and required OCCT license files.
- Restore from only the newly created local package into a clean consumer project.
- Build and publish the consumer, verify the root has no flattened native bridge, and
  verify all native files are below `occt`.
- Run runtime identity and a real OCCT box operation from the published executable.

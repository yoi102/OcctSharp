# OcctSharp 8.0.1-preview.10

## Managed architecture

- Physically splits the generated and selected hand-written managed implementation into
  Runtime, Foundation, Geometry, MeshData, Modeling, Mesh, Documents, Visualization,
  DataExchange, Xde, IVtk, and Draw assemblies/packages.
- Keeps public namespaces under `OcctSharp`.
- Keeps `Shape`, typed topology, `ShapeFactory`, and their direct lifetime/interop closure
  in `OcctSharp.Modeling`; `GpPoint` belongs to `OcctSharp.Geometry`.
- Retains `OcctSharp.dll` as the cross-family facade and compatibility entry, with 3,233
  deterministic CLR type forwarders for moved public types.
- Aggregate comparison with the preceding single assembly covers 39,301 signatures with
  zero additions, zero removals, and no breaking API diff.

## Packaging

- Produces 14 packages at the same `8.0.1-preview.10` version.
- Each of the 13 managed packages contains one managed assembly and zero native DLLs.
- `OcctSharp.Native.win-x64` alone contains the 62-DLL Windows x64 runtime, transitive
  copy target, and 11 notice/license files.
- `OcctSharp.Runtime` supplies the transitive dependency on the shared native package.
- The compatibility package preserves the former complete surface; direct module
  consumers can avoid the facade and optional Draw/IVtk modules.

## Runtime and compatibility

- Native ABI remains 1.54, bridge version remains 0.62.0, and OCCT remains 8.0.1.
- All managed P/Invoke assemblies resolve the same application-local
  `occt/OcctSharp.Native.dll`; native registries, allocators, and release routing are not
  split.
- The clean compatibility consumer repeats the inherited Batch D-L workflows with the
  62-DLL runtime.
- A clean direct `OcctSharp.Modeling` consumer creates a six-face solid and confirms that
  `OcctSharp.dll` is not deployed.
- Release and Debug build all 19 solution projects with zero warnings and errors;
  Generator 91/91 and Runtime 147/147 pass in both configurations.
- Full generation emits 94 files from 16,353 bindings, confirms the 116,263-declaration
  dependency closure is split-ready and acyclic, and verifies all 3,233 facade forwarders
  are current.
- An independent clean source copy rebuilds the native bridge and all managed projects,
  passes Generator 91/91 and Runtime 147/147, and reproduces all 94 generated files
  byte-for-byte.

## Publication boundary

The package set is locally validated but not published. Hosted release execution,
package signing, and NuGet publication remain `NOT RUN` and require separate authority.

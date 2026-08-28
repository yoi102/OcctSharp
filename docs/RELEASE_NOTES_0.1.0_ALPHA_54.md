# OcctSharp 0.1.0-alpha.54

Alpha.54 closes Batch C's finite common-CAD workflow denominator. It keeps OCCT 8.0.1
and .NET 10, advances the native ABI to 1.45 and bridge to 0.53.0, and ships the matching
62-DLL Windows x64 runtime in the repository and package.

## Added

- Copied edge point/first/second derivatives, bounded face U/V derivatives and oriented
  normals, and edge-on-face pcurve bounds/evaluation.
- Independently owned edge and face trims, edge-based wire construction, and copied
  topology replace/remove results.
- Bidirectional owning topology adjacency through item-to-ancestor and ancestor-to-item
  index views.
- `StepReadSession` with copied read/root/unit metadata, explicit target system length
  unit, and selective one-root or multi-root transfer to independent owning shapes.
- Whole-object or common topology-kind selection per presentation, owning selected
  topology snapshots, and parent/thread-bound mouse, wheel, and semantic-key forwarding.
- A clean-package real STEP import, inspect/edit, export, re-read, display, and subshape
  selection workflow covering the complete final dependency closure.

## Architecture and compatibility

ADR-0063 and SC-039 preserve the established ownership categories. Adaptors, curves,
builders, reshapers, and transfer helpers remain call- or session-local. Geometry crosses
as copied values; topology results cross as registered independent owners; viewer
presentations and input remain parent-bound and creating-thread-affine. The changes are
additive: API comparison against alpha.38 reports 37,018 additions, 0 removals, and no
breaking changes.

Generated output remains logically partitioned by product module and API layer while
shipping as one `OcctSharp.dll`, one `OcctSharp.Native.dll`, and one NuGet package.
Physical project/native-DLL splitting is not part of this release.

## Local evidence

- Release and Debug native/managed builds: PASS, 0 errors.
- Generator 91/91, Runtime 114/114, and dependency profiles 6/6 pass.
- Generated freshness and clean regeneration: 83/83 current and byte-identical.
- Generated dependency closure: 27 direct edges, 0 unresolved references, 0 target-graph
  violations, and 0 cycles.
- Clean alpha.54 package consumer: PASS with ABI 1.45, bridge 0.53.0, OCCT 8.0.1, and
  all 62 application-local DLLs.
- Full classification: 116,272 declarations and 7,090 headers classified; 16,353 emitted,
  102 manual, 49,344 skipped, 50,473 blocked, and zero supported-unselected/pending/HD099.
- Committed native bridge: 14,920,192 bytes, SHA256
  `57593BC8B66870DE0373BFBDEFF47B1731C20DF6066EFF22764254EB416E54AA`.
- Local release gate: `batchImplementationComplete=true`.

Hosted CI execution, package signing, and NuGet publication remain `NOT RUN`, so
`publicReleaseReady=false`. Those publication gates do not keep Batch C active.

## Batch C exit boundary

Advanced selection filters, custom rendering pipelines, optional integrations,
low-frequency schema entities, and exhaustive mesh attributes are outside Batch C.
Future work in those areas requires a new finite product denominator and does not reopen
the completed common-CAD batch.

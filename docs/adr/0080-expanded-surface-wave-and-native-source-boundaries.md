# ADR-0080: Broaden Batch P and separate new native source responsibilities

- Status: Accepted and implemented
- Date: 2026-09-05

## Context and decision

The user explicitly requests broader next-batch scope and a project-structure review.
At decision time Batch P had not started implementation, so its original 24-row
preparation was expanded once, before implementation, to the 32-row matrix in its gap inventory.
The eight additions cover sections/intersections, batch projection, boundary metrics,
analytic faces, UV offsets, and continuous surface tracing. No old row is removed.
The expanded 32-root baseline audit contains 1,178 exact declarations: 608 blocked,
204 emitted, 41 manual and 325 skipped. Preview.15 identities are unchanged.

The current dependency report is complete and acyclic with 12 managed modules plus
the facade, 27 cross-shard edges, 94 generated files and 16,353 emitted declarations.
`nativeDllSplitReady` is false. Additional assemblies or native DLLs are not justified:
the shared registry/allocator/release protocol remains owned by one bridge.

The hand-written native implementation is 584,082 bytes and 13,504 lines. New Batch P
code must not extend that monolith. Use separate surface inspection, curve projection,
and topology translation units with a private shared C++ helper header. Reuse the existing
shape registry, allocation and error APIs; introduce no second registry. Add only a
private sketch-curve construction adapter in the legacy unit. Managed surface workflows
use separate files in a `Surfaces` directory, without changing namespaces or assemblies.

This is implementation organization, not physical project/DLL splitting. A wholesale
move of existing viewer/XDE/feature code is not part of the user's read-only split review.
Generated source is untouched. Further legacy extraction requires its own scoped work.

## Alternatives and consequences

- Adding another DLL was rejected because creator-routed cross-DLL lifetime is unproved.
- Adding a managed module was rejected because these cross-family workflows belong to
  the existing facade, and current dependency closure does not require another owner.
- Extending the monolith was rejected because the new features have a narrow reusable
  owning-shape boundary and can compile independently.

## Validation

All 32 capabilities, existing runtime regression, actual Debug native tests, cross-unit
allocation/release, unchanged public ownership, additive API, complete generation,
package isolation/consumers, metadata and local commit are required. Implementation
gates were NOT RUN at acceptance and now pass for all 32 capabilities: focused 13/13,
Generator 91/91, Runtime 177/177, actual Debug-native sweep, both clean consumers,
94-file clean regeneration, additive API and complete local release checks. The final
generated graph remains complete and acyclic with 27 cross-shard edges. STATUS records
the final hashes. No NuGet publication or GitHub push is authorized.

Related: ADR-0074, ADR-0078, ADR-0079; the Batch P gap inventory.

# ADR-0055: Generated Operation Namespaces and Placement Allocation

- Status: Accepted
- Date: 2026-08-25

## Context

The 16,017-binding full-selection wave exposed native entry-point collisions between
static functions, shared constructors, instance methods, and case-normalized OCCT member
names. It also selected `BRepMeshData_Curve`, whose `DEFINE_INC_ALLOC` contract requires
`NCollection_IncAllocator` placement new and no-op class deletion. Ordinary generated
`new` does not compile, and global allocation would not match deletion.

## Decision

- Static exports use an explicit `_static_` operation segment.
- Shared instance exports use an explicit `_method_` operation segment.
- Shared method ordinals are assigned across the full normalized member-name group.
- Raw managed method names include `Method`; friendly methods keep legal overload names
  and receive a deterministic `GeneratedN` suffix only for duplicate managed signatures.
- Configuration schema 1.8 may mark exact package-native types in
  `placementAllocatorNativeTypes`.
- Each marked constructor must have exactly one generated
  `Handle<NCollection_IncAllocator>` parameter. The emitter uses allocator placement new,
  requires a non-null allocator, and retains it in the native wrapper and clones.
- The retained allocator field precedes the OCCT object field so reverse C++ destruction
  destroys the object before releasing allocator storage.

## Alternatives considered

- Keeping a shared export namespace was rejected because adding suffixes only after a
  collision makes ABI naming dependent on unrelated selected declarations.
- Case-sensitive ordinal groups were rejected because C ABI normalization erases those
  distinctions.
- Dropping duplicate declarations was rejected because every stable ID must retain
  deterministic coverage accounting.
- Global `::new` and ordinary `new` were rejected because they violate or cannot satisfy
  the class-specific allocation/deletion contract.
- Retaining the allocator only in managed code was rejected because native clones and
  independent native wrapper lifetime must remain safe without managed object coupling.

## Consequences

- Generated static and shared-method entry-point names change and require a native ABI
  version update before packaging the accepted full wave.
- Placement allocation remains an exact configured exception until discovery models
  class allocation operators semantically.
- Generated wrapper infrastructure gains a second intrusive handle only for marked
  native types.

## Validation required

- Generator tests for static/shared/Create/case/duplicate-signature collisions.
- Generator tests for placement new, field destruction order, clone retention, and
  non-null friendly construction.
- Full Release and Debug native/managed builds after regeneration.
- Runtime construction, clone, allocator-disposal ordering, repeated disposal, and stress
  tests for every marked type.
- Deterministic regeneration, inventory reconciliation, package consumer, and release
  gates before batch B completion.

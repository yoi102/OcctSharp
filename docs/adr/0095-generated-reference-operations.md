# ADR-0095: Audited copied reference operations

- Status: Accepted for implementation.
- Date: 2026-09-08.
- Entry: Batch Z c80f09c, Preview.25 / ABI 1.69 / bridge 0.77.0.

## Decision

Introduce a reusable emitter for audited fixed-value mutable reference parameters.
The explicit stable-ID catalogue is checked in with signature evidence. Unlisted
methods remain outside this rule, even if their C++ types look compatible. Dimensioned
array-start references in BSplCLib, BSplSLib and PLib require a separate buffer contract.
The general type map continues rejecting mutable references; only this rule normalizes
an audited parameter to its copied underlying value.

Public static classes in OcctSharp.ReferenceOperations expose existing shared const
receivers, copied geometric const receivers and audited static methods. Shared inputs
use existing typed SafeHandles and registry validation. No new owner is introduced.
Ref arguments are initialized snapshots, reconstructed in local native storage. Native
results and every ref argument are converted before publishing successful outputs.
Managed ref arguments remain unchanged on exceptions. A boolean false return is a
normal kernel result, not an exception, and publishes the kernel's resulting copies.
Aliased managed ref arguments are independent snapshots; assignments occur in parameter
order. Callers must use distinct variables if native reference aliasing matters.

Copied geometric output arguments must be initialized with valid input records, since
this is an in/out contract, not inferred pure-output direction. Native sentinel values
on successful output retain their existing meaning. Shared object/internal state cannot
be rolled back; const queries may maintain native caches. Existing thread restrictions
remain. Mutable shared-handle refs, mutable ref returns, arrays and pointers are excluded.

New exports use occtsharp_ref_ and separate generated files. Preserve all old exports,
public identities and generated native function bodies. No existing API is rewritten.

## Delivery

Batch AA targets Preview.26 / ABI 1.70 / bridge 0.78.0 with unchanged module/schema
boundaries. Require generator regressions, native/managed builds, semantic multi-output
tests, failure atomicity, shared lifetime validation, Release/Debug and actual Debug
runtime, exact inventory/source/API/export audits, cold regeneration, clean facade/module
consumers and full local release-check. Local commits are authorized; no push/publication.

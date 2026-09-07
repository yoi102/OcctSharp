# ADR-0094: Generated geometric value operations and transformations

- Status: Accepted for implementation.
- Date: 2026-09-07.
- Entry: Y Preview.24, product commit 2083f99 plus README update 845e3d6.

## Decision

Batch Z extends copied geometric values to constructors and instance operations.
Generate Geometry-module static operation classes for the centrally mapped gp value
types and gp_Pnt. Queries take a copied receiver. Mutators take a managed ref receiver,
reconstruct a native temporary, and publish the changed copy only after success.
Failed calls leave the managed receiver unchanged. No native owner or pointer escapes.
Only already mapped value/const-reference inputs and copied returns participate;
mutable reference parameters, reference-return mutators, arrays and operators remain
outside the rule. Native preconditions on indices and degenerate geometry remain.

Add explicit 2D/3D similarity and affine transform records to the central table.
Their contract is geometric coefficients, not native form flags or object layout.
Reconstruct similarity matrices through SetValues, rejecting coefficients that cannot
round-trip to finite coefficients within 1e-12 * max(1, abs(input)). Affine matrices use a canonical similarity form when this check succeeds, otherwise retain general coefficients. Form queries report reconstructed state. Affine matrices support shear and singular
values; inverse operations retain native failure semantics. Operations that directly
edit native form flags are excluded from the value-operation surface. Existing opaque
transform wrappers and all existing type identities remain unchanged.

Freeze Y's generated stable-ID epoch before expanding shared/static overloads.
Preserve all old native generated signatures/bodies and managed public signatures.
The new operations use a separate C prefix and Geometry class namespace, leaving all
old manual and generated constructors intact. Manual stable IDs remain manual even
when the generic emitter could implement the same native declaration.

## Delivery

Target Preview.25 / ABI 1.69 / bridge 0.77.0, unchanged schemas, modules and Native DLL.
Require generator regressions, generated native/managed compilation, operation and
transform semantics, failed mutation atomicity, pure C layouts, exact accounting,
Release/Debug suites, actual Debug-native, source/export/API compatibility, cold
regeneration, fresh facade/module consumers and full local release-check. No fixed
declaration quota; actual emitted and validated IDs determine progress. Local commits
are authorized; pushing and publication remain outside scope.

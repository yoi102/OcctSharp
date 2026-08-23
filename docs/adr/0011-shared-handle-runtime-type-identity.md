# ADR-0011: Shared Handle Runtime Type Identity

- Status: Accepted
- Date: 2026-08-21

## Context

Reference counting alone is insufficient for typed `Handle<T>` projections. The
managed boundary must distinguish the dynamic OCCT type, verify base class
relationships, and reject unknown type names without exposing C++ RTTI or layouts.

## Decision

- Extend the experimental `Standard_Transient` shared-handle probe with a native
  derived test object using OCCT RTTI registration.
- Expose its stable OCCT type name and `IsKind` query through status/out-parameter
  C ABI functions. Type names remain native-local descriptor strings.
- Expose `SharedTransient.CreateDerived`, `TypeName`, and `IsKind(string)` in managed
  code. This validates type relationships but is not yet a general cast or generated
  typed wrapper.
- The additive contract increments native ABI minor version to 1.7 and bridge version
  to 0.8.0. The package advances to `0.1.0-alpha.4`.

## Alternatives considered

- C++ `typeid().name()` was rejected because it is compiler-mangled and unstable.
- Passing `Standard_Type*` across the ABI was rejected because it would leak another
  OCCT object lifetime and ownership category.
- Treating a successful pointer conversion as a cast was rejected without an explicit
  `IsKind` check.

## Validation

- Release and Debug tests verify exact derived type, base `Standard_Transient` kind,
  and rejection of an unknown type name.
- The package consumer reports ABI 1.7, bridge 0.8.0, and OCCT 8.0.1.

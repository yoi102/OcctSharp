# ADR-0002: Use a Native C ABI Boundary

- Status: Accepted
- Date: 2026-08-21

## Context

OCCT exposes a large C++ API with compiler-specific ABI, templates, exceptions, and
ownership semantics. Directly binding managed code to that ABI would make upgrades,
cross-platform packaging, and NativeAOT support harder.

## Decision

Generate and maintain a versioned native C ABI bridge implemented in C++. Managed
raw bindings call that bridge through P/Invoke or `LibraryImport`. C++ class layouts,
STL types, and exceptions do not cross the ABI.

## Consequences

- The project owns explicit ABI, error, string, and ownership contracts.
- Native bridge code must be rebuilt for each supported OCCT/toolchain combination.
- Native runtime dependencies must be packaged and validated.
- C++/CLI is not the primary binding architecture.

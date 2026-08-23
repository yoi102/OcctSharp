# ADR-0006: Use ClangSharp as the Initial AST Front End

- Status: Accepted
- Date: 2026-08-21

## Context

The generator requires a semantic C++ parser that can process real OCCT headers,
produce source locations and stable declaration identities, and avoid regex-based C++
parsing.

## Decision

Use ClangSharp 21.1.8.4 with libClangSharp/runtime 21.1.8.2 for the first generator.
Pass explicit C++20, MSVC compatibility, OCCT include, and Visual Studio system include
arguments. Normalize selected AST declarations into the canonical binding model and
use Clang USRs as stable IDs where available.

The checked-in generation configuration selects headers and expected OCCT version.
Machine-specific include paths come from the resolved Visual Studio developer
environment and are excluded from output.

## Alternatives

- Regex/header splitting was rejected because it cannot represent OCCT C++ semantics.
- Clang JSON AST dump was rejected as the primary interface because its JSON schema is
  not intended as a stable library API.
- CppSharp remains an option for future evaluation but was not needed for the first
  semantic discovery path.

## Consequences

- Generator packages and native Clang runtime are pinned centrally.
- Upgrading ClangSharp/libclang requires parsing and determinism regression tests.
- The current discovery model covers records, enums, functions, methods, and
  constructors; complete signatures, templates, ownership, and emitters remain work.

## Validation

- A controlled semantic C++ fixture test passes.
- `BRepPrimAPI_MakeBox.hxx`, `gp_Pnt.hxx`, and `TopoDS_Shape.hxx` plus included OCCT
  headers parse with zero diagnostics.
- The normalized report contains 3,007 declarations and is byte-identical across two
  consecutive runs in both Release and Debug workflows.

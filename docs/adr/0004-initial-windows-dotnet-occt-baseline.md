# ADR-0004: Initial Windows, .NET, and OCCT Baseline

- Status: Accepted
- Date: 2026-08-21

## Context

The first implementation needs one concrete environment before cross-platform and
multi-version abstractions can be validated. A local combined OCCT distribution is
available with headers, CMake metadata, Release/Debug libraries, runtime binaries,
PCH metadata, and third-party dependencies.

## Decision

Use this initial baseline:

- .NET SDK 10.0.400 and target framework `net10.0` only.
- Windows x64 only.
- Visual Studio 2026, MSVC 19.51, and VS-bundled CMake 4.3.1.
- OCCT 8.0.1 combined `vc14-64` PCH distribution with Debug and Release artifacts.
- Machine-specific OCCT paths live in ignored `config/local.settings.json`.
- A committed dependency manifest records expected paths and representative SHA256
  hashes.

## Alternatives

- Building OCCT from source immediately was rejected for the first closed loop because
  the supplied binary distribution is sufficient to validate architecture sooner.
- Multi-target .NET and cross-platform support were deferred because the user requested
  .NET 10 only and the available OCCT baseline is Windows x64.

## Consequences

- Current compatibility claims are limited to the validated local Windows x64 matrix.
- The `vc14` OCCT binaries are consumed by a newer binary-compatible MSVC toolchain;
  this combination must remain explicitly runtime-tested.
- A reproducible acquisition mechanism and CI-hosted OCCT artifact remain future work.

## Validation

Release and Debug native builds, .NET builds, OCCT runtime tests, and Clang discovery
passed on 2026-08-21.

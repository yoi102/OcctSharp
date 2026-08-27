# OcctSharp 0.1.0-alpha.49

Alpha.49 closes the local engineering side of the single Batch B long-tail wave. It does
not authorize public publication and does not claim that every OCCT declaration is a
managed API.

## Identity

- Package: `OcctSharp.0.1.0-alpha.49.nupkg`.
- Target: .NET 10, Windows x64.
- OCCT baseline: 8.0.1.
- Native ABI: 1.41.
- Bridge implementation: 0.49.0.

## Additions and generator hardening

- Generated manifest coverage increases to 16,353 stable IDs.
- Every named Int32-compatible enum in the selected generated profile is emitted even
  when no selected callable references it. Anonymous Clang enum declarations receive
  the stable `SK017 / AnonymousOrUnnameableEnum` disposition.
- Verified void returns and the export-proven Standard foundation free-function profile
  use the same fixed-width value-copy C ABI as existing static methods.
- Exact method/function scopes include the declaring header and no longer reselect an
  explicitly configured method through a broader automatic prefix.
- Generated output replacement preserves timestamps for byte-identical files, and
  freshness verification compares pre/post-regeneration SHA256 values while still
  requiring all manifest-owned files to be tracked.
- Completion reporting now requires zero `SupportedUnselected`, zero broad LT001-LT004
  reasons, and all local build/runtime/freshness/package/compatibility gates.

## Inventory boundary

- Semantically parsed headers: 7,058/7,090; all 7,090 headers have a final disposition.
- Classified declarations: 116,272/116,272.
- `Emitted` 16,353; `Manual` 61; `SupportedUnselected` 0; `Skipped` 49,344;
  narrowly `Blocked` 50,514; declaration/header pending 0/0; HD099 0.
- LT001-LT004 are zero. Remaining blockers identify exact export provenance, receiver
  ownership, pointer/reference lifetime, handle target, template, or unmapped value
  boundaries and are not counted as generated coverage.
- Inventory SHA256:
  `EC57888D76FD7726806EB5D4247CBB2020C588481651FDF834E2A13F1F3E0DB6`.

## Validation

- Release and Debug native/managed/Samples builds: PASS, 0 warnings/errors.
- Generator tests: 62/62 PASS in Release and Debug.
- Runtime/lifetime/integration tests: 105/105 PASS in Release and Debug.
- Discovery/report determinism and dependency profiles 6/6: PASS.
- Generated freshness: 13/13 PASS.
- Clean regeneration: PASS; all 13 generated files are byte-identical, with Generator
  62/62 and Runtime 105/105 passing in the fresh source copy.
- Alpha.49 clean package consumer: PASS with 62 DLLs, ABI 1.41, bridge 0.49.0,
  and OCCT 8.0.1.
- API compatibility against the alpha.38 baseline: PASS; 36,602 additions, zero
  removals, and no breaking change.
- Complete local release-check, provenance/SBOM/checksums, and Git whitespace gates:
  PASS. `release-gates.json` records `batchImplementationComplete: true` and
  `publicReleaseReady: false`.

Project license selection, third-party legal review, hosted CI execution, package
signing, NuGet credentials, and publication authorization remain separate public-release
gates and are not converted to PASS by local engineering completion.

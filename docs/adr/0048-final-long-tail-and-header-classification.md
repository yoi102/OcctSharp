# ADR-0048: Final long-tail declaration and header classification

- Status: Accepted
- Date: 2026-08-23
- Scope: B19 complete OCCT 8.0.1 inventory classification

## Decision

Full-library inventory has two independent completion facts. Semantic inventory reports
which entry headers Clang successfully parsed. Final classification assigns every
successfully discovered declaration and every catalogued entry header a deterministic
disposition, including headers that cannot be parsed from the pinned artifact.

Raw discovery states remain unchanged for generator development. A separate long-tail
pass maps them into final states: eligible but not selected declarations become
`SupportedUnselected`; existing stable `SK001`–`SK006` exclusions remain `Skipped`;
manual mappings remain `Manual`; and unresolved public candidates become `Blocked` with
`LT001`–`LT004` reason codes for declaration projection, instance ownership, return
projection, or parameter projection. No blocked declaration is counted as generated.

Every header becomes `Parsed`, `BlockedExternalDependency`, `ExcludedLanguage`,
`UnavailableInArtifact`, or a named blocked parse reason. `HD001`–`HD005` cover VTK,
EGL/GLES, RapidJSON, C++/CLI, and absent OCCT generated headers. An `HD099` fallback is
allowed to keep reports total, but B19 cannot close while any such unowned fallback exists.

## Validation

Adding `StepData_Factors.hxx` to the inventory preamble removed 11 false
`StepToTopoDS_*` failures. Two full BatchSize=128 scans then produced byte-identical
50,117,128-byte reports with SHA256
`C8C7EC3913F97068138E162C16ADB187EC590446A5F3EF2E33815AB48B586CEA`.
They classify 116,214/116,214 discovered declarations and 7,090/7,090 catalogued headers
with zero pending dispositions and zero `HD099` entries. Semantic parsing remains
7,058/7,090 because 32 named dependency/artifact headers cannot be parsed.

## Upgrade impact

An OCCT upgrade must regenerate the full report and diff stable IDs, final states,
reason codes, header availability, and package/toolkit counts. A change from blocked to
supported is not emitted automatically; ownership/type-map/emitter and validation gates
still apply. New `HD099` or pending dispositions fail the classification gate.

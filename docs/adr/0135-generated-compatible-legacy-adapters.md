# ADR-0135: Generated compatible legacy adapters

Status: Accepted within the user-authorized BR–CI round (2026-10-11).

BR preserves existing C ABI and managed type identities while moving five audited
compatibility implementations into embedded generator templates. Foundation
Text/Collections/Transient still use the existing private runtime registry/header
contracts; these support files do not claim additional SDK declaration coverage.
BSplCLib::FlatBezierKnots uses exact TM053 signature admission and the existing
degree/capacity-checked whole-sequence copy. Its prior Manual ID becomes generated;
the generic scalar-reference path remains forbidden. No ownership category changes.

The generated manifest also records sorted SHA256 fingerprints of every embedded
generation resource. This describes contracts/templates and pinned catalogues,
not all parser/emitter source code, algorithm correctness, or runtime acceptance.
Existing API/export names remain unchanged. Compatibility templates are explicit
human-authored inputs, not inferred whole-SDK bindings. Runtime registries, error
handling and unsafe support boundaries stay explicit pending separate audit.

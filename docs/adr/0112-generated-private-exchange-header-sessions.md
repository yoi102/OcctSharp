# ADR-0112: Generated private exchange header sessions

Status: Accepted for implementation. Date: 2026-10-08.

## Context

AT follows delivered Preview.43. AK/AN already own geometry transfer. Pinned
source b8f597c677811d1f9f4d8a97f5ae2825c0353a42 shows MakeHeader(model) aliases
header attributes; GiveListCombined dereferences a null output sequence; IGES
GlobalSection::Params uses unbounded sprintf into a 200-byte Hollerith buffer.

## Decision

TM027 ExchangeSessions owns a private STEP/IGES reader and mutex. Sessions
require a nonempty loaded model with a computed SDK graph. Managed IDs
carry a unique domain and bounded one-based number. No mutable model/entity,
selector or actor escapes. Copies preserve absent/empty strings, array bounds,
schema identifiers, entity types/labels and checks. Private all/root/pointed/type
selectors use same-session exemplars; type matching uses SDK IsKind semantics.
Label resolution retains ambiguity and all matches; never choose silently.

Writes reuse WorkSession::WriteFile/SendAll over the existing model. STEP edits
prepare new FileName/FileDescription attributes with copied strings and arrays,
then replace the private header list. Schema and extra header entities keep
their exact handles and order. IGES edits copy global-section strings with
CopyRefs. Units, scale, protocol and geometry retain their original values.
Disable the private reader's default final modifiers and clear its ShareOut
modifier list before publication: SDK
SendAll otherwise rewrites IGES sender and filename on the original model.
STEP temporary headers retain schema so pinned SetDescription's schema guard
does not silently skip the edit. IGES edited strings use explicit Hollerith
input so digit-H prefixes remain literal; pointed input is deduplicated before
SetList because its pinned implementation preserves duplicate entries.
Restore original headers after success/failure; retire on restoration failure.
Never reread the source file to export. Serialize operations and release via
existing registry leases. Snapshots outlive the session without native aliases.

Every effective IGES global string must fit 195 UTF-8 bytes before serialization,
including unchanged unit/protocol text. STEP array edits must be nonempty; absent
edits preserve values, empty scalars set explicit empty values. Schema conversion,
unit edits and general raw mutation remain deferred. GiveListCombined is deferred
for its concrete pinned SDK defect, never called or counted as migrated.

## Validation

Require independent snapshots, wrong-domain/disposed rejection, selector inputs,
label ambiguity, bounded raw buffers, failed-write restoration, STEP/IGES actual
metadata and geometry readback, and AK/AN integration. Full local delivery gates
remain required. External CAD, hosted CI/signing/publication are separate NOT RUN.

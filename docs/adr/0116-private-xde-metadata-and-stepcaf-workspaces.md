# ADR-0116: private xde metadata and stepcaf workspaces

Status: Accepted (2026-10-08).

Extend the exact private workflow catalogue to existing identity-checked XCAF
documents and fresh call-private STEPCAF sessions. Reuse parent SafeHandles,
context identity and command-atomic mutations. Imported owning documents implement
the SDK BeforeClose protocol before destruction; bare generated receivers do not
qualify for naming-backed camera mutation. Detached immutable metadata owns
its values; an imported generated document owns its native data independently.
Borrowed tool/session/container APIs and inferred topology history stay excluded.
Exact XML colors extend the existing SC-062 application-local persistence boundary without changing the legacy color type. View removals preserve outer-command history with pre-backed-up attributes. STEPCAF imports register both participating managed assemblies before payload access, ensuring one bridge registry.
The single bridge and existing registry/error contracts remain unchanged.

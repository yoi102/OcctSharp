# ADR-0106: private writer diagnostics

Status: Accepted for implementation, 2026-10-06.

AN gives each STEP/IGES writer a private single export epoch. Creation deep-copies
the complete input shape set. Transfer and write happen once under the session lock.
Immutable managed snapshots contain input statuses, copied checks, model entity
types and only exact mappings proved by the finder process. Snapshots survive source
and session disposal; repeated inspection does not query a mutable native writer.
Input, finder-map and model-number domains remain separate and session-bound.

TransferBRep::ShapeState is an orientation relation, not a success status.
ShapeMapper can return an unregistered mapper; MapIndex must prove membership.
Single transient projection is distinguished from complete binder-chain/list
enumeration. Unknown binder kinds make completeness false. No geometric matching,
transitive model graph inference or guessed replacement is allowed.

ResultCheckList changes Interface_Check::Entity, so the adapter clones checks and
their messages before conversion. Finder TransferResultInfo assumes every mapper
is a ShapeMapper; all entries must pass that type guard before calling it, otherwise
statistics are explicitly unavailable. SetTransientFromShape stays deferred because
rewriting transfer bindings would invalidate diagnostic evidence.

Return-code failures and partial transfers remain inspectable in the result. No
entity means no file write. File write failure is reported independently of transfer
success. Kernel exceptions retire the session rather than retry a mutated process.
Argument validation occurs before retiring a fresh session. No actor, external
process, native settings object or callback enters this API. The pinned SDK still
reads process-global exchange defaults and unit settings internally; callers must
synchronize external changes to those settings and exchange operations that share
them. The per-session mutex is not a process-global OCCT exchange lock.
The existing ShapeExchange APIs remain compatible. IGES uses its explicit MM/BRep
writer mode; it does not promise XDE repair policies or a lossless format conversion.

Bounded binder traversal rejects cycles; text, model and map sizes have explicit
limits. SafeHandle and typed leases protect disposal. Native output buffers must be
valid and disjoint. Test STEP/IGES output and independent readback, mixed outcomes,
write failures, source disposal, repeated snapshots, and cross-session index rejection.
Run the complete local delivery gates in WORKFLOW before advancing to AO.

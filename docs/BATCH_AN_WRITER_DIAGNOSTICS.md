# Batch AN: private writer diagnostics

AN closes shape set -> private STEP/IGES writer -> copied diagnostics -> file ->
independent readback. It builds on Preview.37, with [ADR-0106](adr/0106-private-writer-diagnostics.md)
and the exact [admission ledger](BATCH_AN_WRITER_DIAGNOSTICS_ADMISSION.json).
Six of seven AK writer candidates are admitted, one remains deferred, and ten
previously Blocked writer prerequisites are admitted separately. TM020 targets
DataExchange; no generic finder, actor, mutable writer or model receiver escapes.

## Public boundary

`WriterTransferSession.Create` deep-copies every input position (including duplicates)
with geometry but without mesh copies. Callers synchronize source mutation during
creation. The resulting session is independent of source and collection disposal.
Empty input is valid. A session serializes one export epoch; argument preflight leaves
it fresh, whereas kernel exceptions retire it. Return-code failures remain observable.
`TransferAndWrite` creates an immutable, fully managed `WriterTransferSnapshot`;
`Snapshot` copies the frozen metadata without querying a surviving native writer.

STEP uses `STEPControl_AsIs`. IGES uses MM and BRep mode, with native shape processing;
it does not promise the old XDE extrusion repair policy or lossless conversion.
The existing ShapeExchange API and behavior are unchanged. Writing uses native
overwrite semantics. Missing entities suppress writing; an existing target file
alone is never evidence of successful output. Transfer status, write status and
exact mapping are separate observations. STEP stores IFSelect return codes, IGES
stores booleans, and write status -1 means no write attempted.
The pinned SDK reads process-global exchange defaults and unit settings internally.
Callers synchronize external changes to those settings and other exchange work
sharing them; a session mutex does not provide a process-global OCCT lock.

Each source index, finder-map index and model number has a separate managed type and
private session domain. Default, foreign and out-of-bounds values are rejected.
Snapshots retain all model dynamic type names, all finder-map entries, per-source
native status, orientation relation, optional single-transient projection, exact
binder results, copied checks and guarded statistics. ShapeState is an orientation
relation, never a success flag. MapIndex proves membership because ShapeMapper can
manufacture an unregistered mapper for an absent shape. IGES successful transfer
may have no exact source mapping after native shape processing.

Simple transient, transient-list and multiple binders are enumerated in chain/list
order, preserving duplicates. Unknown binder kinds and results outside the model
set completeness false while retaining proved model numbers. A missing binder is
explicit. No geometric matching, transitive graph traversal or replacement guessing
is performed. Binder chains are cycle-checked and limited to 4096 links; collection
and text inputs are bounded at 1,048,576, snapshot JSON at 64 MiB. These are rejection
limits, not promises of a bounded SDK algorithm memory footprint.

ResultCheckList mutates checks, so every check and its messages are copied first;
the source check snapshot must remain unchanged. TransferResultInfo is called only
after every finder is verified to be a nonnull ShapeMapper with a nonnull shape.
Otherwise statistics are explicitly unavailable. The nine statistics rows correspond
to TopAbs shape kinds 0..8; they are process-wide buckets, not input success counts.
Native output buffers must be valid and disjoint. Typed registry leases and SafeHandle
prevent stale/wrong token use and protect concurrent disposal.

## Validation

Source audit uses pinned SDK commit b8f597c677811d1f9f4d8a97f5ae2825c0353a42.
The baseline and source hashes are in `OcctSharp/artifacts/batch-an/preview37-baseline`.
Focused and full delivery evidence is recorded under
`OcctSharp/artifacts/batch-an/delivery-result.json`. Implementation does not by itself
establish delivery. Hosted CI, signing, push, public NuGet and whole downstream
application acceptance remain NOT RUN.

## Local delivery

Preview.38 / ABI 1.82 / bridge 0.90.0 passes Release, Debug and cold-source Release:
238 Generator and 924 Runtime tests in each configuration, zero failures or skips.
Actual Debug-native passes 924 tests with all 62 loaded module paths/hashes matched.
All 204 generated files regenerate byte-identically; 201 baseline files are unchanged.
Exports add four entries without removals and match Debug. Both configurations pass
53 C11 headers, 34 geometric layouts and 250 release races / 16,000 acquisitions.
All 45 private headers compile independently. Full inventory preserves the 116,272
declarations and 32 parser failures, with exactly 16 Blocked -> Emitted transitions.
The final delivery report binds all 14 packages, both consumers and payload identities.

Thirteen runtime cases include STEP/IGES independent readback, detached sources,
write failure, empty input, source/map/model domain rejection, raw stale/wrong tokens,
bounded buffers and concurrent disposal. Empty compound + box reports native partial
failure in IGES; STEP reports successful transfer for the empty compound. Tests retain
that distinction and independently read the six box faces in both cases. Four rule
tests cover exact signature admission, incomplete catalog rejection, determinism and
the continued exclusion of binding mutation. A hash-bound native probe extracts the
actual template helpers and passes in Release and Debug: simple/list/multiple and
unknown chained binders, duplicate/outside-model results, copied check entity identity
and escaped messages. No whole-SDK/public-release claim follows.

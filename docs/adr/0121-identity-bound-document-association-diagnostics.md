# ADR-0121: identity-bound document association diagnostics

Status: Accepted
Date: 2026-10-10

## Decision

TM038 extends the closed-workflow emitter over the existing document projection
identity, transaction and payload protocols. It publishes copied diagnostics and
explicit command-scoped reference/naming/GDT/clipping associations, never native
attributes, labels, mutable lists or tools. Preserve single-bridge typed registry leases.

Initialize direct naming descriptors completely from an exact same-document named
shape. Do not call Solve or infer source history. Avoid the default-uninitialized
ShapeType/Orientation getters. Imported label flags are read-only in this scope
because their setter does not participate in attribute undo backup.

Retain existing nested undo/abort and BeforeClose handling. Use the existing close-safe document subclass in the BC native factory, reject unsafe history mutations, and explicitly back up attributes before removal. Structural modern
dimension links are explicitly separate from AX legacy measurement values.
Clipping creation verifies returned geometry against the requested coefficients;
the pinned SDK's parallel-plane name collision is rejected atomically.

## Consequences

Source identity remains document/context specific, including repeated occurrences.
Empty OCAF labels may remain after undo; associated attributes and edits must restore.
Reads, lazy inputs and copied payloads remain bounded. Native access is caller
synchronized. Bin/XML reopen and full release/cold/actual-Debug/two-consumer/freeze
gates are retained before delivery completion.

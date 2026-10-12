# ADR-0127: direct collection transactions and persistence

Status: Accepted for the continuously authorized BF–BK implementation.
Date: 2026-10-10.

## Decision

Expose direct TDataStd arrays/lists as immutable typed copies through the existing
DocumentProjectionContext. Keep attribute GUID/type identity, native lower bounds,
list order, missing/empty separation, and exact context-scoped reference labels.
No native array/list receiver, mutable container or borrowed TDF label is public.
Use prevalidation followed by the existing nested transaction inside a caller command.

Support empty arrays only through audited public NCollection/attribute operations.
BooleanArray's unsafe logical-empty state remains unsupported. Keep native type
identity while extending private application's Bin/XML drivers to preserve supported
empty arrays and complete nullable reference slots. Disclose that the empty-array
envelope requires an OcctSharp reader. Standard nonempty native content uses the
existing drivers, except the Bin/XML ReferenceArray slot-completeness corrections. XML null markers are an explicitly disclosed extension; a stock reader has no null-slot fidelity guarantee. A private ReferenceArray virtual backup adapter keeps native type/GUID and uses public SetInternalArray to restore empty intervals without invalid Init.

## Consequences

Direct attributes and NamedData remain distinct storage domains. Detached numeric
and text data survive owner closure; references retain issuing context identity and
cannot silently bind to a reopened or different document. Native-driver limitations
are explicit, including the unsupported empty BooleanArray and external reader
behavior for extended empty arrays. Actual persistence/history/lifetime tests and
full delivery gates remain required; generation alone is not delivery.

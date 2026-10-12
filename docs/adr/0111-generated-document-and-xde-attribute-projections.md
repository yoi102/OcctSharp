# ADR-0111: generated document and XDE attribute projections

Status: Accepted. Date: 2026-10-07.

## Decision

AS adds exact TM025 Documents and TM026 Xde workflow catalogues. Existing
parametric storage, transactions, undo/redo, persistence, assembly/BOM and PMI
remain the implementation of those capabilities. Add copied general label data
and missing layer/note projections without changing their assembly identities.

A projection context retains the existing managed document SafeHandle, never
disposes that parent, and pins it on every native call. It accepts a generated
TDocStdDocument publicly and an existing facade document through an internal
bridge. Those native owner domains are explicitly distinguished and validated;
no public raw pointer, mutable attribute, TDF label or native map escapes.
The facade caches its context. Bound label/note/layer identities belong to one
context; cross-context inputs are rejected before mutation. A disposed parent,
missing label, removed layer or removed note cannot authorize an operation.
Copied values and diagnostic tables remain usable after every owner is disposed.
Callers synchronize document mutation, including old APIs, while projecting.

NamedData copies all six keyed categories, including native integer/real array
bounds. Absent attributes, absent keys, zero/empty values and null array handles
remain distinct. Detached input maps and arrays are validated before application.
Bulk replacement runs inside a private nested TDF_Transaction within the user's
already open document command, committing into that command only on success.
Replace the whole initialized NamedData attribute rather than restoring maps
into a previously populated receiver: the pinned Restore implementation skips
empty categories and is not a general clearing operation. Prior attribute and
array storage remain detached from the new value and available to history.
Failure aborts the private transaction and leaves previous outer-command edits
available. Existing transaction naming, undo/redo and save/reopen are reused.
TDF_Tool outputs are copied entry/tag paths, subtree counts and self-containment;
arbitrary relocation, filters and streams are outside this rule.

Layer snapshots copy names, visibility and associated stable label entries.
Comment/balloon notes copy author, timestamp, text, orphan status and exact
labeled-item associations. Copied imported associations also retain exact native
assembly paths, attribute GUID/subshape domains and orphan diagnostics. Other
native note kinds remain explicitly unsupported
for text editing; they are not silently reported as empty comments. Notes on
definitions and on repeated occurrences remain separate. No topology matching
or automatic rebinding is introduced. Reads do not lazily create missing XCAF
tools or notes; initialization belongs to an atomic write within an open command.

Bounded versioned byte payloads are an internal C ABI transport, with validated
counts, lengths, kinds, bounds, finite numbers and complete decoding before writes.
Capacity/type/stale-input failures do not partially write caller buffers or publish
native result owners. This does not globally map native containers or pointers.

## Persistence driver boundary

Pinned OCCT persistence is not sufficient for this value contract: binary
NamedData dereferences null/empty arrays and treats [0,0] bounds as absent;
XML additionally normalizes array lower bounds, formats reals with insufficient
precision, and cannot preserve all keyed strings. Its note driver writes the
timestamp as comment text and decodes UTF-8 through an ASCII constructor.
Application-local drivers therefore replace only NamedData and XML comment
text handling, preserving the existing four format names and all other drivers.
Install them as the first registrations: DefineFormat uses IndexedDataMap::Add
and later registrations do not replace a driver. No global plugin/factory or
managed callback is introduced. Nine exact private calls are Manual SC-062,
separate from 62 generated IDs; no additional C ABI export is needed.

The XML writer declares the pinned class-name prefixes for note comments,
assembly-item references and binary notes through its inherited protected
namespace helper. Native attribute names stay unchanged. Both a standard .NET
XML parser and the OCCT reader must accept the resulting XML documents; this
syntax validation does not establish external CAD fidelity.

Unsafe NamedData is written as an SDK-readable safe subset plus a bounded,
versioned complete extension (binary marker 0x4F534E44 and hex payload, XML
occtsharp_nameddata_v1). XML note fields retain corrected standard text and a
bounded occtsharp_note_v1 extension for exact Unicode, empty/numeric text.
Restore into a fresh decoded value before modifying the retrieval target;
the live storage source is never changed. Legacy attributes without extensions
continue through the original drivers (XML comments use corrected text decoding).
Native boolean results for layer unlink remain native acknowledgments; they do
not independently prove a previously present association changed.

Extensions require these OcctSharp drivers for full fidelity. Ordinary SDK or
external CAD readers may see only the safe subset, normalized bounds/rounded
numbers, or incorrect Unicode note fields. This is not a promise of external
full-fidelity interoperability. External CAD acceptance is NOT RUN. Existing
data that a legacy writer already lost cannot be reconstructed.

## Required evidence

Generic and XDE labels, all NamedData categories, Unicode, missing/zero/null/empty
distinctions, copied bounds, atomic failure inside an existing command, undo/redo,
four-format persistence where applicable, repeated assembly occurrence associations,
non-mutating reads, cross-document and disposed/stale identities, raw malformed
payload/capacity/type/release guards, and both clean package consumers. Preserve
complete local binding-delivery gates; hosted CI/publication remain separate.

# ADR-0114: Generated call-private Foundation value workflows

Status: Accepted (2026-10-08).

Foundation strings, paths, colors and unit dimensions contain borrowed references,
encoding boundaries and lazy state. TM029 reuses exact-signature closed workflow
admission, the native registry, exception guard and success-only publication.
It initializes receivers privately, validates bounded inputs, and copies output
into immutable managed records before releasing native snapshot storage.
Existing scalar color owners are reused.

Unicode indices remain SDK UTF16 code units; edits reject surrogate splitting.
Units prove parsed dimensions before conversion; zero does not signal failure.
One bridge mutex spans the complete sentence-validation/conversion transaction;
individual SDK method locks cannot protect the private sentence's shared dictionary
reads. SDK path IsValid is projected as a predicate and does not reject ordinary
drive-qualified paths; its DOS predicate rejects a colon even in such paths.
Current unit systems/dictionary mutation stay deferred. Paths expose pure syntax.
Borrowed references never cross the C ABI.

The one native bridge and current module boundaries are preserved. Exports are
additive and Foundation owns the workflow. Delivery requires actual generated
compile/runtime evidence and precise baseline StableId accounting.

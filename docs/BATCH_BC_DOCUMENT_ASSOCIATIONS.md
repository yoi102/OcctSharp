# BC document and XDE association diagnostics

State: IMPLEMENTED / LOCAL GATES PASS; final freeze remains delivery authority.
Development identity: Preview.53 / ABI 1.96 / bridge 0.104.0.
See actual local evidence below.

[Admission](BATCH_BC_DOCUMENT_ASSOCIATIONS_ADMISSION.json) records 47 admitted
and 115 deferred candidates, eight new prerequisites and 29 reused operations.
The generated delta is 55; Manual delta is zero.
[Candidate input](BATCH_BC_DOCUMENT_ASSOCIATIONS_CANDIDATES.json) is an audit pool.
TM038 follows [ADR-0121](adr/0121-identity-bound-document-association-diagnostics.md).

`DocumentAssociationContext` borrows one existing document projection context.
Labels from another document or another context for the same document are rejected.
All output is detached: label flags, attribute GUIDs, exact generic references,
initialized naming diagnostics, assembly/component/referred-definition entries,
dimension/tolerance/datum associations and clipping-plane/view associations.
There is no borrowed attribute, map, naming argument, shape or tool output.

The BC CreateNativeDocument factory reuses the accepted close-safe XCAF document protocol. Raw generated documents lacking it are rejected before naming/clipping/GDT history mutations.

New direct naming references explicitly initialize an IDENTITY descriptor from
one exact existing named-shape attribute in this document. No Solve or topology
history is inferred. Pinned TNaming_Name default construction leaves ShapeType
and Orientation uninitialized: their general getters remain deferred. The
descriptor sets them before storage; diagnostics derive shape type from the
initialized stored shape instead of reading unknown naming fields.

Bulk generic-reference replacement and all association edits require an open
command and use the existing nested TDF transaction/abort protocol. Imported flags
are diagnostic only: their SDK setter lacks an attribute undo backup. NewChild
returns a context-bound label; OCAF labels may remain empty after undo, while
their attributes/associations follow command history. Removal paths explicitly back up affected attributes before forgetting them. Naming/GDT creation requires
the existing application-owned or close-safe document protocol, retaining BeforeClose.

Modern dimension association attributes carry explicit first/second shape links
and lock state, not a measurement object. Existing AX numeric legacy dimensions,
material/RGBA and saved cameras remain reused. Occurrence/definition domains are
not substituted. Clipping planes have finite nonzero normalized coefficients,
bounded Unicode names and explicit capping. An absent native capping attribute is copied as null, distinct from false. Pinned SDK AddClippingPlane may reuse
a parallel same-name plane without comparing its position: returned coefficients
are verified, and a collision aborts rather than retargeting the association.

Reads bound the native document tree and association lists to 4096 labels, depth
256 and 16 MiB copied output. Association graph father/child counts are checked before SDK traversal, and creation reserves 64 label slots for tool initialization. Lazy input budgets stop at 4097 and dispose enumerators.
Callers synchronize access to each native document. Independent documents may
operate concurrently. Private record discriminators 50/51 avoid older payload tags.

Acceptance covers command requirements, bulk atomicity, abort/undo/redo, foreign
identity, native naming initialization, clipping collisions, duplicate/lazy limits,
detached lifetime, repeated instances and actual BinXCAF/XMLXCAF reopening.
Both clean consumers execute the shared native workflow; facade acceptance also
checks assembly persistence. BC completes only at batch-bc/delivery-result.json PASS.

Local evidence:

47 admitted / 115 deferred, 8 new prerequisites / 29 reused; generated +55 / Manual +0.
Release/Debug/cold Generator 282, Runtime 1147, all passed.
Actual Debug-native 1147, 62 observed DLL paths/hashes. Exports
41386 (+9/-0), Debug parity; API 57740
(+115/-0); 4328 facade forwarders. All 258 baseline
generated files unchanged; current files 261. ABI/lifetime/private headers,
14 packages and both clean consumers passed. Final source/payload freeze is
delivery authority; BC counts complete only when delivery-result.json is PASS.

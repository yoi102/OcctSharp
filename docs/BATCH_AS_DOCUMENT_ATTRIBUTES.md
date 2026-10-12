# Batch AS: document data and XDE layer/note projections

Baseline: delivered Preview.42. Status: full local gates PASS; final identities require delivery-result.json PASS.

163 primary candidates: 49 generated admitted / 2 private Manual / 112 deferred. Thirteen new private
prerequisites and sixteen reused workflow dependencies, plus twenty-one reused
persistence calls; expected exact
Blocked-to-Emitted delta: 62; separate Blocked-to-Manual delta: 9 (SC-062).
The final audit includes whole-attribute replacement
to preserve empty-category history and copied native annotation path/domain data.

[Frozen candidates](BATCH_AS_DOCUMENT_ATTRIBUTES_CANDIDATES.json) and
[exact admission](BATCH_AS_DOCUMENT_ATTRIBUTES_ADMISSION.json) distinguish new
generated calls from existing emitted/manual prerequisites. All decisions use
pinned SDK source and the frozen Preview.42 inventory.
[ADR-0111](adr/0111-generated-document-and-xde-attribute-projections.md) defines
document ownership, atomic writes, copied data and label/note identity guards.

Existing NamedData parameter storage is feature-specific. AS projects complete
generic label maps without replacing parameter evaluation or recomputation.
Existing XDE layer names/assignments remain; add table visibility/associations
and comment/balloon note workflows. Existing transactions, history, persistence
and repeated assembly instances compose these generated results.

Required: copying/error/domain/lifetime tests, real named transactions and
undo/redo/save/reopen, occurrence/definition separation, malformed raw ABI,
clean facade/direct module consumers and full local delivery freeze.
Generation produces 223 files and 26,602 bindings. Native/managed Release builds
and complete local checks pass; final identities require delivery-result.json PASS. The pinned binary/XML drivers
need private persistence adapters to preserve null/empty/zero-bound arrays,
exact real/string data and XML note text. Full values require OcctSharp extension
drivers; ordinary SDK/external CAD readers may see only safe subsets. External
CAD interoperability is NOT RUN; see ADR-0111 for the exact format boundary.
Hosted CI, signing, push, public NuGet and whole downstream acceptance: NOT RUN.

## Local delivery evidence

Preview.43 / ABI 1.86 / bridge 0.94.0. Release/Debug/cold: 255 Generator and
996 Runtime, zero failures/skips. Actual isolated Debug-native: 996/996 with
62 observed DLL hashes. 223 reproducible generated files, 216 baseline files
unchanged. Exports 41,305 (+11/-0); managed API 56,762
(+132/-0); 4,249 forwarders; 59 C11 headers, 34 layouts,
250 races / 16,000 acquisitions per configuration; 46 private headers.
Eighteen AS runtime cases cover four actual formats, complete/null/empty data,
arbitrary/zero-singleton bounds, precision/Unicode/empty/numeric notes, repeated
instances, history, native mid-batch rollback, non-mutating reads, removed
tools/notes/layers, malformed/version/type/UTF8/count/finite input, capacity,
stale/wrong-owner/double/concurrent release and corrupt-extension retrieval.
Standard .NET XML parsing and OCCT XML readback both pass after declaring the
pinned note/association prefixes. Four generator contract cases plus both clean
consumers pass. Ordinary safe
integer/byte/array files need no extension; extending values requires the private
drivers. Fourteen packages and final source/doc/payload/provenance/SBOM/checksums
are independently checked before freeze. No external/public release claim.

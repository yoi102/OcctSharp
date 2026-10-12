# ADR-0138: owned mesh resource import

Status: Accepted within the authorized CE scope.
Date: 2026-10-12.

Extend ADR-0129 with a separate generated ReadResources profile for glTF JSON/OBJ
local nested resources. Existing flat Read and its rejection behavior remain.
Resolve bounded explicit relative files under the source directory, preserve the
relative graph in an unpublished staging directory and verify hashes/byte budgets
before native import. No arbitrary network URI, filesystem escape or implicit
missing-resource substitution is admitted.

Before publishing the close-safe native document, copy all admitted material
texture contents into separately allocated SDK NCollection_Buffer/Image_Texture
owners. The buffer is held by the native material handle graph, including copied
SDK handle aliases; no managed result-owned temporary directory substitutes for
that lifetime. Delete staging before return. No raw SDK pointer, decoder, callback
or mutable provider session escapes. Failure releases every unpublished owner.

Admit bounded PNG texture data first and disclose other image/compression profiles
as deferred until validated. Verify encoded bytes and decode dimensions, source
deletion, independent document/resource lifetime, real material use and error
rollback. Keep the existing Xde/DataExchange managed dependency direction; native
image support in the single bridge does not create a managed reverse module edge.

This is owned resource import, not a claim of complete PBR or format equivalence.
Existing exporter limitations and the precise format/material loss ledger remain
explicit. External CAD and physical GPU faults require their own evidence.

Pinned OBJ path adaptation: RWObj_MtlReader/RWObj_Tools resolve texture references
against the OBJ folder and convert Unicode tokens to ASCII. Preserve the captured
source graph/bytes/hashes, then create an unpublished deterministic ASCII reference
copy with a complete native-path-to-source map. Only explicit references change;
no geometry/material value or missing resource is substituted. Original source
paths, staging locks, admitted resource budgets and rollback remain applicable.

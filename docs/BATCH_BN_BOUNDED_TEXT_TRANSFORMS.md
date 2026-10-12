# Batch BN: bounded text transforms and atomic batch copies

State: IMPLEMENTED / LOCAL GATES PASS; frozen delivery authority: batch-bn/delivery-result.json.

BM frozen local delivery PASS is the prerequisite. BN selects 14 of 160 family
candidates; 146 are deferred. Existing AV/SC-008 length, storage and UTF8 copy
dependencies are reused without migration credit. Exact signatures and fixed
SDK source identity are in the [admission record](BATCH_BN_BOUNDED_TEXT_TRANSFORMS_ADMISSION.json).

## Contract

TM050 privately constructs ASCII or Unicode strings using explicit lengths.
Concatenate, append, assign and overwrite invoke their exact length-bearing SDK
overloads; Unicode additionally supports prepend, BMP character replacement/removal
and token extraction. Existing AV substring/split/insert operations are reused and
receive no new credit. Global char pointer/string_view mapping does not change.

Every source and operand is an independent immutable managed string. Native
mutation is confined to a call-owned SDK value, then UTF8 bytes and UTF16 lengths
are copied. Batches enumerate at most the item budget plus one, validate every
operation and reserve cumulative output storage before executing native calls.
The public read-only batch appears only after every result copy succeeds; failure
does not return a partial list. No borrowed string or native pointer escapes.

Positions are one-based UTF16 code-unit positions. Overwrite can grow at the end
and must preserve both surrogate boundaries. Invalid encoding and embedded NUL
are rejected. ASCII is seven-bit, consistent with AV. Replacement/removal use
non-NUL BMP scalars; Unicode token separators are a nonempty set of such scalars
so supplementary characters in the input cannot be split by a separator.

Inputs and each output have a 1 MiB UTF8 limit; batches contain at most 4096 items
and reserve at most 8 MiB output. Overwrite conservatively reserves source plus
replacement bytes; shrinking transforms reserve at most the original bytes.
Replacement accounts for actual UTF8 expansion before execution. These are storage
and enumeration bounds, not execution timeouts. Model Unicode names are separate
from the filesystem naming rules required by BP.

## Verification

Focused checks cover ASCII/Chinese/supplementary characters, exact lengths,
empty values/collections, both surrogate boundaries, invalid encodings/NUL,
per-item expansion, cumulative reservation and lazy budget-plus-one enumeration.
C ABI probes cover cleared failed outputs, empty results, UTF8 buffer canaries
and stale tokens. Both consumers execute the native Unicode transforms and direct collection copies.
Both save/reopen a real XBF containing that list: Facade uses XdeDocument and
the direct module consumer verifies the existing document/projection C ABI. Full local product-delivery gates remain
required; only `OcctSharp/artifacts/batch-bn/delivery-result.json` PASS closes BN.

Hosted CI, external CAD interoperability, human/full downstream acceptance,
signing, push and public publication: NOT RUN.

Local evidence:

14 admitted / 146 deferred, 0 new prerequisites / 6 reused; generated +14 / Manual +0.
Release/Debug/cold Generator 308, Runtime 1320: PASS.
Actual Debug-native 1320, 62 actually observed DLL paths/hashes: PASS.
Exports 41449 (+3/-0), Debug parity; API 58687 (+53/-0).
All 298 baseline generated files unchanged; current generated files 301.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final frozen authority: OcctSharp/artifacts/batch-bn/delivery-result.json PASS. Only that authority closes BN; BO–BQ remain authorized.

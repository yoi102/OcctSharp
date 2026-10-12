# Batch BI: direct document array/list projections

State: FROZEN LOCAL DELIVERY PASS at 8.0.1-preview.59; authority: OcctSharp/artifacts/batch-bi/delivery-result.json.
Delivered baseline: Preview.58 / ABI 1.101 / bridge 0.109.0 (BH frozen PASS).
Development target: Preview.59 / ABI 1.102 / bridge 0.110.0.
[Admission](BATCH_BI_DIRECT_COLLECTIONS_ADMISSION.json): 39 admitted, 77 deferred,
0 new prerequisites. Existing initialized receiver/value operations are reused and
are not new migration. TM045 covers six arrays and three lists:
IntegerArray, RealArray, ExtStringArray, ByteArray, BooleanArray, ReferenceArray;
IntegerList, RealList, ExtStringList. There is no ReferenceList claim.

## Direct attribute domain

ReadCollection returns null for a missing attribute and copied typed values for a
present direct TDataStd attribute. It preserves GUID, lower bound, list ordering,
duplicates, empty strings and strict UTF-8 Unicode. Custom GUIDs can distinguish
multiple attributes of one family on one label; a GUID occupied by another family
fails without replacing it. Reads create no attributes and never substitute NamedData.
Reference slots are nullable exact labels scoped to the issuing document/context.
Replacing references validates every non-null target's context and live native label.
Detached scalar values survive document closure; reference identities cannot be rebound.

Typed construction enumerates lazily with bounded values/bytes, stopping after the
first value beyond its limit. Batch input stops at 4,097 rows before rejecting a
4,096-row limit. Total values are capped at 1,048,576 and payload bytes at 16 MiB.
The native parser independently checks envelope, counts, GUID/type collisions,
bounds, UTF-8, reference labels and duplicate label/GUID targets before any edit.
An existing caller command is required. Attribute removal/addition runs in a nested
TDF transaction, retaining outer edits on failure and participating in abort/undo/redo.
The inherited BeforeClose protocol remains unchanged.

## Native driver limits and explicit persistence adapter

The pinned array Init methods reject upper < lower. NCollection_HArray1 supports
zero-size arrays; four typed ChangeArray operations and ReferenceArray's audited
SetInternalArray retain their lower bound without calling invalid Init. These five
families can therefore represent empty direct attributes independently of absence.

BooleanArray stores logical lower/upper bounds in private fields and has no safe
public way to initialize a zero-length logical interval. Its uninitialized bounds
are not read. Constructing an empty BooleanArray is explicitly unsupported; absence,
nonempty packed booleans and empty lists remain separate. No private-field mutation,
placeholder logical BooleanArray or NamedData substitution is used.

The stock Bin/XML array drivers cannot preserve empty arrays. Private application
drivers retain the same native attribute type and GUID, writing a valid one-element
native prefix followed by the versioned `occtsharp_direct_empty_v1` envelope (binary
marker 0x4F434431), then restoring an actual zero-size typed array in OcctSharp.
The default GUID is written explicitly before the binary extension because the SDK's
optional GUID decoder would otherwise consume extension bytes as a GUID.
This is an OcctSharp persistence extension: an unmodified reader may see the native
prefix and does not receive a claim of empty-array interoperability.

The pinned binary ReferenceArray writer omits null slots although its reader expects
one entry per index. The private binary adapter writes every slot, parses empty entries
as null, and preserves the GUID through the native format helper. XML reference
uses a disclosed versioned slot/null marker because its stock writer also omits null slots. Missing/null target slots do not shift subsequent ordinals. Private ReferenceArray virtual backup/restore/paste adapters copy via SetInternalArray, retaining the native type/GUID while avoiding the stock Restore -> Init(empty) Debug range error.
All adapters operate on direct attributes, not NamedData dictionaries.

## Required verification

Nine typed families, custom/default GUIDs, negative bounds, Unicode, null reference
slots, repeated-instance isolation, missing and supported empty collections; real
BinXCAF and XMLXCAF save/open; caller commands, prepublication failure, nested rollback,
abort, undo/redo, foreign contexts, BeforeClose, source/result lifetime and bounded lazy
enumeration. The BH source-mutation isolation case also projects copied intersection
coordinates and domain identity into direct attributes.

Retain full Release/Debug/cold, actual Debug-native DLL identities, ABI/API/export,
inventory, both consumers, HWND/DXGI identity/stale-query and final freeze gates.
Only batch-bi/delivery-result.json PASS completes BI.
See [ADR-0127](adr/0127-direct-collection-transaction-and-persistence.md).

Local evidence:

39 admitted / 77 deferred, 0 new prerequisites / 62 reused; generated +39 / Manual +0.
Release/Debug/cold Generator 298, Runtime 1219: PASS.
Actual Debug-native 1219, 62 actually observed DLL paths/hashes: PASS.
Exports 41424 (+2/-0), Debug parity; API 58296 (+48/-0).
All 281 baseline generated files unchanged; current generated files 285.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze PASS is recorded in batch-bi/delivery-result.json.

# CB: private generated document, attribute, naming and transaction operations

Baseline: CA frozen Preview.76 / ABI1.113 / bridge0.125.0.
State: IMPLEMENTED / LOCAL GATES PASS; final package/source freeze required.

[Admission](BATCH_CB_DOCUMENTS_ADMISSION.json) selects125 Manual operations;
1 unused SDK-default XML format registration stays Manual. New SDK declarations0.
TM061 generates native-only operation delegates from the semantic binding model.
Exact overload types are compiled; constructors retain their SDK bases; caller
copy/command/epoch/thread/foreign/input budget/UTF-8/publication policy remains.
Borrowed SDK handles, labels, strings and collections stay in the original native
call/owner. Public managed/C ABI receivers and mutable borrowed data are unchanged.

The typed FindAttribute helper preserves the pinned nongeneric lookup and checked
downcast short-circuit: missing values leave the old typed handle untouched;
wrong-type found values clear it. NamedData driver delegates preserve qualified
base dispatch instead of recursively invoking their overriding methods. The XML
storage base retains original namespace registration, application-local driver
composition and inherited runtime type identity. SDK default DefineFormat is
not substituted for the established exact driver profile.

Source audit covers29 native files,313 calls/124 directly observed signature
profiles, with XML storage constructor instantiated through the original template.
The semantic conversion changes17 files and retains12 related files exactly.
All325 prior generated files are unchanged; one native-private header is added.
Generation is not compile/runtime validation. Complete direct attribute/reference/
transaction/naming/storage/lifetime and real HWND workflows in both consumers,
full local delivery and final package/source/frame freeze before CC.
Hosted CI, signing, push, publication and full downstream/device/external-CAD
acceptance remain NOT RUN without evidence.

Local evidence:

125 admitted / 1 deferred, 0 new prerequisites / 0 reused; generated +125 / Manual -125 (new SDK declarations 0, Manual to Emitted 125).
Release/Debug/cold Generator 345, Runtime 1405: PASS.
Actual Debug-native 1405, 62 actually observed DLL paths/hashes: PASS.
Exports 41464 (+0/-0), Debug parity; API 58938 (+0/-0).
All 325 baseline generated files unchanged; current generated files 326.
Explicitly audited modified generated files: 0.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final package/source freeze remains required. Only batch-cb/delivery-result.json PASS completes CB.

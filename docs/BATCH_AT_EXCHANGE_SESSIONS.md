# Batch AT: exchange headers and private session selections

Status: complete local gates PASS; final identities require delivery-result.json PASS. Baseline: delivered Preview.43, 2026-10-08.

AP–AS delivered (4/6). Complete AT local delivery before AU expansion.
[Candidates](BATCH_AT_EXCHANGE_SESSIONS_CANDIDATES.json): 182 primary Blocked.
[Admission](BATCH_AT_EXCHANGE_SESSIONS_ADMISSION.json): 63 admitted, 119 deferred,
4 new prerequisites, 30 reused; generated delta 67, Manual delta 0.
[ADR-0112](adr/0112-generated-private-exchange-header-sessions.md) governs TM027.

Own a reader model; copy STEP/IGES headers and entities/checks; select all, roots,
pointed lists and exemplar types; resolve labels without guessing; export audited
metadata with original geometry. Reuse AK/AN for transfer/readback. Schema/unit
conversion, actors, mutable external models, streams and GiveListCombined remain
deferred. Bound all effective IGES global strings to 195 UTF-8 bytes for writing.

Implementation, Release native/managed build, four focused Generator and 19
focused Runtime checks: PASS. Full Release/Debug/cold/isolated Debug/package
delivery: RUNNING; no delivery claim before final freeze.
Actual file readback, failure restoration, lifetime/ABI and both consumers remain
required. External CAD, hosted CI/signing/publication: NOT RUN.

## Local delivery evidence

Preview.44 / ABI 1.87 / bridge 0.95.0. Release/Debug/cold: 259 Generator and
1015 Runtime, zero failures/skips. Actual isolated Debug-native: 1015/1015,
62 observed DLL hashes. 226 reproducible generated files, 223 baseline unchanged.
Exports 41,315 (+10/-0); managed API 56,925 (+163/-0);
4,263 forwarders; 60 C11 headers, 34 layouts; 250 races / 16,000
acquisitions per configuration; 46 private headers. Nineteen AT
runtime cases cover metadata/geometry readback, AK/AN reuse, Unicode/Hollerith
literal text, 195-byte bound, failed restoration, duplicate/type selections,
strict numeric labels, detached/disposed/cross-session IDs, typed/stale/raw
buffers and malformed edits. An independent SDK fixture reproduces a negative
ambiguous label result with all ordered candidates [1,2], choosing none. Four
Generator cases and both clean consumers
pass. Final packages, source/document/payload/provenance/SBOM/checksums are
verified before freeze. No external/public release claim.

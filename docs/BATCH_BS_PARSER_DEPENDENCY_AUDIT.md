# BS: evidence-based parser/dependency classification

State: TOOLING IMPLEMENTATION AND VALIDATION PASS (2026-10-11); frozen authority: batch-bs/delivery-result.json.
Baseline: frozen BR Preview.68 / ABI1.110 / bridge0.118.0.

The inventory classifier previously assigned known dependency/language exclusions
from familiar filenames alone, and treated every missing include as an omitted
SDK-generated header. An unrelated compiler error in one of those files could be
masked as a known deferred dependency and escape the HD099 delivery gate.
BS now requires matching actual diagnostic evidence. Unknown or unrelated errors
remain HD099/Blocked. Existing positive fixtures now use the real cause strings;
six negative familiar/unknown filename fixtures prove unrelated missing headers
are not falsely classified as expected SDK exclusions.

All28 frozen parser failures were checked against installed header bytes and the
fixed SDK source commit b8f597c677811d1f9f4d8a97f5ae2825c0353a42. Their exact causes:

| Cause | Headers | Disposition |
|---|---:|---|
| Missing optional VTK toolchain | 19 | Deferred outside the Windows core CAD round |
| Deprecated aliases reference unavailable dependencies | 6 | Artifact unavailable; no fabricated aliases |
| C++/CLI-only header | 1 | Excluded from the native C ABI generator language |
| GLES helper requires another API context | 1 | Deferred; exact GL-types preamble probe still failed |
| Missing RapidJSON headers for the stream helper | 1 | Deferred dependency; generic ostream/JSON helper is not a promised CAD API |

The GLES probe failure is retained under artifacts/batch-bs. It does not justify
inventing GL_APIENTRY definitions or activating a GLES/VTK toolchain to increase
coverage. No installed SDK file was changed, no source alias was invented and no
new SDK binding was admitted. Header exclusions remain28; semantic scan remains
incomplete. This is completed cause audit and classifier correction, not repair
or migration of every excluded declaration.

Release/Debug full Generator322/322 pass. A probe reclassifies all28 actual frozen
failure records with exact known causes; unknown-error tests remain blocked.
Regeneration and native/managed compile pass. All315 generated files and both
native bridges/13 managed runtime payloads remain byte-identical to frozen BR.
BR's full runtime, ABI/lifetime, actual Debug and package evidence is reused by
those exact unchanged hashes; it was not rerun for this inventory-only change.
No new product package or version is created for BS tooling.

Full refreshed inventory and exact generator-source evidence are frozen only by
OcctSharp/artifacts/batch-bs/delivery-result.json PASS. BR's immutable packages
retain their original source snapshot; they do not contain this later tooling.

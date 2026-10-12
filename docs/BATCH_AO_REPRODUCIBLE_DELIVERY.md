# Batch AO: SDK exclusions and reproducible local delivery

AO builds on the fully delivered Preview.38 / AN snapshot. It changes local tooling
and evidence, with no new binding IDs, public API, native ABI or generated-file delta.
[ADR-0107](adr/0107-reproducible-local-delivery-inputs.md) defines the concurrency and
package-input boundary. [The exact SDK ledger](BATCH_AO_SDK_BLOCKERS_LEDGER.json)
contains header hashes, diagnostics, stable IDs, signatures and Release/Debug export
proof. Source baseline: `OcctSharp/artifacts/batch-ao/preview38-baseline`.

## SDK scope

The 33 configured header exclusions consist of 19 VTK headers, one EGL/GLES header,
one RapidJSON header, one C++/CLI header, ten missing generated-prerequisite headers,
and one parseable private ExprIntrp yacc/lex interface. Exactly 32 reproduce parser
failures. The private lexer interface is retained without claiming it fails parsing.
One-header parser probes include positive gp_Pnt and ExprIntrp cases and all 32
negative cases. Seventeen configured absent-symbol exclusions and two AK BRepCheck
deferrals remain unavailable in both pinned native closures. Six positive export
proofs distinguish existing overloads from the three easily confused exclusions.
No configuration exclusions are removed; no declaration is newly emitted.

## Build and package behavior

The native workspace lock covers both configurations and dependency caches. It is
released before managed builds, avoiding child bootstrap deadlock. Missing bridge
recovery and failing bootstrap release are exercised with real Windows PowerShell
processes using the same launch policy as Directory.Build.targets.

Package snapshots include source, generator rules, configuration, stable documents,
assets, packaging inputs, bundled runtime and actual managed/native payloads.
Existing files are temporarily read-locked; attempted writes/deletes/renames fail at
the writer. New or transient inputs invalidate the pack. Sources remain editable
after the guard is disposed. Build outputs are excluded from the source watcher;
selected binary payloads receive their own read locks after the managed refresh.
No successful package is promoted until the entire candidate set and content match
the captured hashes. Failed candidates remain in a unique staging directory with
FAILED.txt. Source/consumer checks can inspect package-inputs.<version>.json, which
is also bound into provenance and release checksums. The existing -SkipBuild option
still skips native rebuilding while refreshing managed binaries once.

The tooling fixtures test unchanged inputs, blocked existing-file changes, new and
reverted inputs, ignored output renames, handle release and package-document identity.
A real pack is interrupted by adding a temporary document after its snapshot starts;
the expected outcome is failure with zero packages promoted. Tests use isolated
probe package versions and preserve previous batch artifacts.

Full delivery is recorded under `OcctSharp/artifacts/batch-ao/delivery-result.json`.
Implementation or a successful probe is not the final product-delivery gate.
Hosted CI, signing, push, public publication and whole downstream acceptance are
NOT RUN. AO does not install VTK or change the pinned SDK/platform boundary.

## Final local validation

Preview.39 retains ABI 1.82 / bridge 0.90.0. Release, Debug and cold-source Release
each pass 238 Generator and 924 Runtime tests with zero failures/skips. Actual
Debug-native passes all 924 tests and 62 loaded path/hash identities. All 204
generated files and manifest remain byte-identical to AN and fresh regeneration.
Exports remain 41,267 with Debug parity; API remains 56,413 with 4,214 forwarders.
Both configurations pass 53 C11 headers, 34 layouts and 250 release races / 16,000
acquisitions; all 45 private headers compile independently. The 116,272-declaration
inventory has zero state changes and the same 32 parser failures, with zero Pending.

Thirteen snapshot fixture cases pass. Two real Windows PowerShell bootstrap
processes have nonoverlapping native critical sections; a missing bridge is restored,
and a failed bootstrap releases its lock before a successful Debug invocation.
A real 14-package probe passes; a real concurrent document addition is rejected
with no candidate packages promoted. Final packages run both clean consumers and
bind source/document/payload identities, package-input manifest, provenance, SBOM
and all fourteen package checksums. Detailed identities live in delivery-result.json.
All AL-AO local batches are complete; external release/downstream gates remain NOT RUN.

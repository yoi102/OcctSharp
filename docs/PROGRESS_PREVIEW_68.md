# Preview.68 generated compatibility progress

Development candidate: 8.0.1-preview.68 / ABI1.110 / bridge0.118.0.
State: ALL LOCAL EXECUTION GATES PASS; frozen state is owned by delivery-result.json.
Baseline before BR: Preview.67. BR is frozen only when
OcctSharp/artifacts/batch-br/delivery-result.json records PASS.

BR migrates four native compile units and one managed implementation into exact
generator compatibility templates. Existing C ABI, managed type identities and
registry ownership are unchanged. One SDK declaration changes Manual→Emitted;
no new public API/export or CAD capability is claimed.

Current generated output: 315 files, including all309 byte-identical baseline
files plus six compatibility outputs. All152 embedded generator resource hashes
are traced to current source inputs. Compatibility template bodies match original
implementations; only the knot function signature is wrapped onto one line for
the existing generated-header extraction rule. Templates stay explicit authored
inputs, not automatic inference of ownership from C++ signatures.

Full Release/Debug/cold Generator316 and Runtime1349 passed. Cold regenerated
315 files are byte-identical. Native exports41460 and API signatures58819 have
zero additions/removals;4437 facade forwarders remain current. Both clean package
consumers exercise the preserved whole static knot sequence, independent mutable
managed copies and degree-zero rejection. ABI checks cover86 C11 headers,34
geometric layouts and the250 registry release races/16000 acquisitions per
configuration;46 private headers compile independently without PCH.

Inventory116366: Emitted27368, Manual1226, Blocked38365, Skipped49407, Pending0.
Covered total28594 and non-Skipped coverage42.7037% are unchanged. Parser failures28;
full semantic scan remains incomplete. See [counts](PROGRESS_PREVIEW_68_COUNTS.json),
[BR](BATCH_BR_GENERATOR_COMPATIBILITY.md) and [authorized18-batch plan](NEXT_ROUND_BR_CI_PLAN.md).

Actual isolated Debug-native1349/1349 and all62 observed DLL paths/hashes passed.
Final package/source identity is recorded by the frozen delivery authority. Hosted CI, signing, push, public NuGet, external CAD
interoperability and full downstream application acceptance: NOT RUN.
Private Cadoryx render-health evidence remains separate from this public candidate.

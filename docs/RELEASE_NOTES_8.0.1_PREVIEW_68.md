# 8.0.1-preview.68 development candidate

BR moves existing Foundation Text/Collections/Transient native adapters and the
BSplineKnotBuffers native/managed adapter into reproducible generator templates.
The existing API, ABI1.110 and bridge0.118.0 are preserved; no CAD behavior or public
API is added. Generic scalar reference emission remains forbidden for the SDK's
static FlatBezierKnots sequence; exact TM053 copying retains degree/capacity checks.

The generation manifest now fingerprints all152 embedded template, contract and
catalogue resources. Compatibility output is source-traceable; the underlying
templates are still authored and audited by engineers. Generator template inputs
have explicit LF checkout rules. Minimal point-only generation retains its scope.

Release/Debug/cold Generator316 and Runtime1349,315 byte-identical generated files,
ABI/lifetime/export/API and two clean package consumer checks pass. Independent
actual Debug-native1349/1349 and all62 observed DLL identities also passed.
Final source/package identity and completion authority are recorded in [progress](PROGRESS_PREVIEW_68.md) and
[STATUS](STATUS.md). No public publication or commit is claimed.

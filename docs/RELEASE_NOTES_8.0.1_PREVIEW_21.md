# OcctSharp 8.0.1-preview.21 (local validation target)

Batch V retains all forty partition/material-region/volume capabilities. Full local
exit evidence is tracked in [STATUS](STATUS.md); this document does not imply release.
No NuGet publication or GitHub push is authorized.

## New workflows

- Full partition snapshots, exact input membership, ordered finite region expressions,
  disjoint materials, shared interfaces, explicit boundary removal and typed containers.
- Source-indexed diagnostics, dimension-specific measures, conservation/sliver policies
  and independent result copies with revision-bound identities.
- Intersecting-face volume construction, verified fast mode, shell/cavity inspection,
  bounded voids, point-selected owning volumes and explicit ON-boundary policies.
- Occurrence-aware XDE inputs/products, Q repair acceptance, T atomic multi-output
  recipes, R attributed interface meshes and real-HWND cell/interface/void review.

## Compatibility and limits

Package version follows OCCT 8.0.1 with preview counter 21. ABI 1.65 / bridge 0.73.0
add five C exports; assembly/file 0.1.0.0 and schema 1.13 stay unchanged. Twelve modules,
the compatibility facade and one shared native-runtime package retain one bridge DLL.
Exact manual overloads are recorded as SC-059, not whole-root migration.

Local evidence includes focused 37/37 and ten repeated runs; Generator 91/91 and
Runtime 446/446 in Release/Debug, plus isolated actual Debug-native Runtime 446/446.
The clean source build reproduces all 94 generated files byte-for-byte. Both clean
consumers and fourteen local packages pass. Managed compatibility against Preview.20
adds 561 signatures with no removals. The five additive native exports preserve all
previous exports and match between Release and Debug at 29,475 names. Full local release-check passes;
final inventory has 16,353 emitted and 1,131 accepted manual declarations, with exactly
27 new SC-059 transitions and no other changes. Final documentation delivery and the
local completion commit are recorded in STATUS. Hosted CI, signing and publication
remain NOT RUN and are not required or authorized for this local batch.

Solid lineage may be unavailable; repair composition reports supported input owners,
not general subshape history. Mixed-dimensional internal removal is unsupported.
Voids require an explicit bounded envelope; unknown point containment rejects.
STEP supports explicit product names/styles. On this IGES assembly path geometry,
colors and the root name survive, but nested names do not. Region semantics remain
application/OCAF metadata. Windows x64 is the validated platform target.

OcctSharp code is MIT; bundled OCCT remains LGPL-2.1 with its OCCT exception or a
separate commercial license. Other native dependencies retain their own licenses;
see [runtime notices](../OcctSharp/runtime/win-x64/THIRD_PARTY_NOTICES.md).

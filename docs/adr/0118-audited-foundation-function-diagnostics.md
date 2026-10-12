# ADR-0118: audited Foundation function diagnostics

Status: Accepted (2026-10-10).

Extend the existing closed private-workflow catalogue to free functions only when
each exact StableId/signature additionally records Kind=Function, a public .hxx
header, source package and toolkit. Missing discovery toolkit information does not
promote any general function profile: AZ audits the fixed SDK commit, package
manifests and byte-equivalent installed headers; external calls require SDK export
and native link evidence. Header-only operations are compiled inside the bridge.
Wrong kind/header/package/toolkit, signature drift or incomplete closure fails.

TM035 copies all matrix/polynomial/message inputs into initialized call-private
storage. Matrix calculations require finite, exactly symmetric inputs with bounded
dimensions and exponent; optional absence/nonfinite output publishes no values.
Rebased one-based native matrices avoid mismatched native bounds. Detached results
restore the caller's copied grid domains. No existing mutable solver is republished.

Message_Msg is initialized with Set(body), without global message-resource lookup.
The SDK uses a 64-byte sprintf buffer for numeric arguments. Restrict formats to
%s, %d, %f, %e, %g or bounded %.0f through %.17f, real magnitude at most 1e12,
32 placeholders and bounded strict Unicode. Unsupported percent escapes, flags,
widths and length modifiers are rejected before the SDK call.
Format bodies also reject non-ASCII UTF16 units whose low byte is percent because
the pinned SDK truncates units when detecting placeholders. String arguments
support these characters; inserted text is not reparsed. The result copies
original, intermediate and complete text plus absent-argument diagnostics.

Polynomial refinement reports estimate/residual/derivative, not asserted convergence.
The descending SDK refinement uses a five-element local array: limit it to degree
four. Lazy inputs stop at budget+1 with enumerator disposal. Gauss/Kronrod data and
BMP-set searches copy all results, retaining explicit completion and UTF16 ranges.
Native buffer callers supply valid disjoint writable buffers; managed wrappers own
their arrays. Registry/error ownership and the single bridge remain unchanged.

Validation: exact admission regressions, generated compilation, focused numerical
identities, optional absence, unsafe formats, Unicode, lazy budgets and concurrency;
full product-delivery gates and both clean consumers before delivery is complete.

See [AZ contract](../BATCH_AZ_FOUNDATION_DIAGNOSTICS.md) and [ownership](../OWNERSHIP.md).

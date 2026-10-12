# ADR-0137: private normalization of located assembly definitions

Status: Accepted for authorized CC implementation. Delivery remains separately gated.

Pinned OCCT `b8f597c677811d1f9f4d8a97f5ae2825c0353a42` clones an assembly
definition into an identity compound and copies its component placements, omitting
the definition's own root placement. The CC native Release/Debug probe reproduces
this loss with rotations and translations that do not commute.

Normalize only the unpublished private clone. For each exact assembly definition
in the shared label map, multiply its original placement onto every outgoing
component: `definitionLocation * componentLocation`. Leave the cloned definition
itself at identity. Repeat once for each shared definition, independent of map
iteration order. Nested products then retain root, component and definition
placements in their original order. No wrapper, topology matching or new label
identity is introduced. Geometry copying, selected-root overlap/cycle/depth/budget
checks, supported names/colors/layers, private writer parameters and atomic output
publication retain ADR-0134's contracts.
Depth16 and occurrence4096 apply to complete paths and expanded visits across all
selected roots. A cache of unique labels cannot enforce either limit. Cache subtree
height and expanded occurrences, add the summary at every incoming component and
reject excessive active depth before recursive traversal. Validate the source
document domain before cloning. Native fixtures reproduce the previous acceptance
of depth26 and65536 leaves; public C ABI tests reject both before publication.

Require exact source/destination document domains, initialized component locations,
consistent assembly named-shape/location attributes, finite bounded matrices and
positive rigid placements for newly supported located assembly definitions.
Reject contradictions, scale or reflection rather than inventing an interpretation.
Do not mutate the source. Failure discards the unpublished clone.

TM062 generates private semantic SDK delegation for existing XDE and six color
operations. Constructors for SDK value types return the exact SDK type; only
audited construction expressions change. Private XML storage retains SDK dynamic
type identity. Borrowed returns, original guards and public C ABI remain unchanged.
Member delegates expose a bound postfix call so C++17 evaluates the original
receiver before argument expressions. The closure borrows its address without
copying or destroying the SDK receiver; temporary owning handles retain their
original full-expression lifetime, including borrowed results consumed afterward.
Generated counts describe declaration migration, not new CAD capability coverage.

See [CC admission](../BATCH_CC_ASSEMBLY_ADMISSION.json) and
[CC workflow](../BATCH_CC_ASSEMBLY.md).

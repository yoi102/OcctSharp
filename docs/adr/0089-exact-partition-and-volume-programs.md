# ADR-0089: Exact partition and volume programs

- Status: Accepted, implemented and locally validated
- Date: 2026-09-05

## Context and decision

Retain all forty rows of the [V matrix](../BATCH_V_PARTITION_VOLUME_GAP_INVENTORY.md).
Entry is committed U (`2c48460`, Preview.20); the frozen Preview.15 configuration
is not rewritten. Fresh inventory and exact-root delta evidence must precede builds.

Modeling owns immutable input graphs, finite membership expressions, material rules,
partition revisions, volume policies and copied reports. Native PartitionRegions,
RegionInspection and VolumeConstruction own call-local OCCT algorithms. The existing
FeatureResult temporarily carries copied region data; existing Shape owners carry all
topology. No builder/PaveFiller, iterator, registry, project or DLL is added.
The facade alone integrates Q repair, R mesh, T evaluation, XDE, exchange and review.

Full cells come from GetAllParts, not the initially empty selected Shape. The checked
CellsBuilder adapter reads its actual part-to-argument index and finalizes explicit
cell assignments through the pinned SDK material maps. Membership is never inferred
from centers, bounding boxes, nearest geometry or equal topology counts. Each result
has a new revision; IDs from any other revision are rejected. Reselection is a new
call-local build from copied original arguments, not mutation of a persistent builder.
Finite postfix expressions are bounded and contain only input/constant/set operators.
Single-input programs satisfy General Fuse's two-identity precondition with an explicit
container alias of the same private input, folding membership to input zero; no helper
geometry or extra cell is invented. Empty point selections return an owning empty
compound, with copied classifications and an explicit include/exclude/reject ON policy.
Rule effects and conflicting materials are observable. Internal removal happens after
selection; material zero and distinct materials retain boundaries. Incompatible
mixed-dimension removal is rejected before the SDK operation. Containers are last.

Boundary IDs refer to exact shared TShape/location identity within one partition;
oriented uses are separately copied, including repeated seam uses. History queries
are restricted to OCCT-supported vertex/edge/face shapes. Solid lineage is explicitly
unavailable. Public topology access returns copies, never mutable aliases into plans.

MakerVolume uses explicit intersection and internal-shape policies. Its non-intersecting
fast path requires an actual interference check; a caller assertion is insufficient.
Helper-box faces/solid are checked against published topology. Empty output remains a
valid zero-cardinality diagnostic outcome, not a fabricated solid. Void construction
requires an explicit bounded envelope. Nested-shell and point decisions use native
classification, report ON/unknown, and never infer containment from signed volume alone.

## Alternatives and consequences

Reusing J's single selected-result operation would not expose the complete partition.
Long-lived builders would introduce hidden mutable sessions and disposal complexity.
Geometric membership guesses and silent tolerance escalation are rejected. Keep the
existing managed dependency DAG; Documents does not depend on Modeling/Mesh/XDE.
Private result records extend an existing owner and have the same release symmetry.

## Required validation

All original forty rows need numerical/topological named assertions and complete
Release/Debug, actual Debug-native, real-file/HWND, lifetime, negative ABI, deterministic
generation, exact manual-ID accounting, two clean consumers and local release checks.
These gates pass for the completed forty-row implementation. Current evidence is in STATUS and
OWNERSHIP/SPECIAL_CASES/NATIVE_ABI. One complete V commit, then automatic W entry;
no push, publication or signing.

STEP delivery uses explicit solid body products, because a non-assembly compound
lost internal body geometry in the real roundtrip fixture. IGES preserves geometry,
colors and root assembly name on this path, not nested names. Region keys/materials/
correspondence are retained in OCAF metadata; tests reopen BinXCAF and compare them.
Parametric named outputs share one accepted generation and validate direct-child
entry paths before access. Q repair composition is supported input-owner provenance,
not an asserted complete source-subshape chain.

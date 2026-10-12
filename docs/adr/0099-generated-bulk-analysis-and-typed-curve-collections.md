# ADR-0099: Sized bulk operations and typed owning curve collections

- Status: Accepted; implementation and delivery evidence tracked separately.
- Date: 2026-10-02.
- Baseline: Preview.31 / ABI 1.75 / bridge 0.83.0.

## Decision

Admit the 42 exact signatures in the AH admission matrix. Emit AH into separate
files and preserve AF/AG output. TM014 is declaration-scoped; general pointer,
template, matrix, evaluator and raw contiguous-reference mappings remain closed.
Mutable scalar references are copied in and published only after all outputs have
been prepared. They are admitted only for exact scalar-output signatures.

Numeric algorithms use owning one-based Array1/Array2 copies. Check degree,
multiplicity sums, index ranges, complete packed-pole capacities and output lengths
before calling the pinned kernel. Representational edits and derivatives in the
workflow use the existing owning geometry interfaces when low-level storage is
not proved. Kernel-specific rationality semantics remain documented.

Typed curve sets contain only Geom_BSplineCurve or Geom2d_BSplineCurve. A leased
non-transient registry owner holds intrusive handles to deep copies, never a
shared_ptr to a transient curve. Managed input factories retain every SafeHandle
during native copying. Null elements and empty input sets are rejected. Concat
requires at least two curves and exactly n-1 nonnegative junction tolerances.
The native algorithm's zero-based arrays are assembled explicitly.

Split always clones its source, including the pinned singleton path that would
otherwise return the input handle. Concat clones all input curves before any
kernel mutation. Its result owner retains independent updated-input curves,
concatenated curves, index metadata and the returned closure flag. Accessors
produce independent sets/curves, so they outlive either owner. Dispose is idempotent;
typed leases protect active calls. No input mutation is published to the caller.
Both output preparation and registry publication are transactional.

Valid distinct raw output storage starts empty; capacity failures publish no partial
buffer. Metadata scalar slots must be distinct and outside the index buffer;
storage aliases are rejected before those slots are touched. All errors are
contained in the C ABI. Concurrent caller mutations of source geometry remain
caller-synchronized. SDK symbol, size, lifetime and both clean consumers are
required before declaring AH locally delivered.

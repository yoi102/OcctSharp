# ADR-0136: Call-private coupled constraint copies

Status: Accepted within the user-authorized BR–CI round (2026-10-11).

TM054 admits exact constructors/getters of AppDef_MultiPointConstraint and its
AppParCurves_MultiPoint base for a bounded nested workflow. Each row initializes
positions and optional tangency/curvature by copying array constructors. Curvature
requires tangency; both nonempty dimensions use the same optional-channel mode.
Empty dimensions take single-side constructors; fully empty rows use the default.
Count-only, setter, shallow-copy and move constructors are not admitted.

Rows are flattened with independent3D/2D counts and caller lower bounds. Native
construction normalizes to one-based;2D getters use global indices after3D points.
Full input/capacity/finite-value budgets are checked before construction. Absent
channel getters are never called. SDK objects are call-private, and all copied
rows are staged before success-only publication. No registry owner, borrowed
buffer or retained input category is introduced.

Managed lazy inputs are copied once within bounded enumeration, with enumerators
disposed on success/failure. Null dimensions or empty positions denote absent
sides; optional channels on absent sides are rejected. Snapshots own independent
arrays and preserve caller bounds separately from SDK ordinals. Empty output
sides are null arrays; empty batches are valid. Input mutation requires caller
synchronization during enumeration.

Maximum4096 rows/65536 total positions limits storage, not execution time.
Human-authored exact contracts/templates generate native/managed outputs.
Runtime/consumer evidence is independent of admission, generation and compilation.

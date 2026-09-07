# 8.0.1-preview.25: Geometric value operations and transforms

Batch Z implements [ADR-0094](adr/0094-generated-geometric-value-operations.md).
It adds 1,388 generated declaration IDs, from 20,650 to 22,038. The public managed
API adds 1,487 signatures and removes none. Geometry now exposes copied constructors,
queries and mutations through OcctSharp.ValueOperations for 35 geometric value families.
Existing manual bindings and owning wrappers keep their public identities.

Point, vector, direction, frame, matrix, quaternion, line, conic and elementary surface
operations share one generator rule. Const queries take values; mutating calls take a
managed ref receiver and publish its copied result only after success. Native temporary
values do not escape. Failed mutations preserve the original managed receiver.

Four additional Foundation records represent 2D/3D similarity and affine transforms
with explicit row-major coefficient fields. Workflows include translation, rotation,
scale, reflection, composition, inversion and transformation of shared geometry.
Affine values retain shear and singular coefficients. Similarity inputs must round-trip
to finite coefficients; native form/cache flags are not serialized. Singular inverse
and negative-power operations explicitly reject under SC-063.

Mutable-reference arguments/returns, bulk arrays, operators, pointers and direct native
form-cache editing remain outside this rule. Full OCCT migration is incomplete.

Preview.25 / ABI 1.69 / bridge 0.77.0 preserve twelve modules, one facade, one Native
DLL and fourteen packages. [STATUS](STATUS.md) owns current validation and delivery
evidence. This is a local preview; no GitHub push or NuGet publication is requested.

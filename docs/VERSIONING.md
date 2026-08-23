# Versioning

OcctSharp has several related but distinct versions. They must never be collapsed
into one ambiguous version string.

## Version identities

| Identity | Purpose |
|---|---|
| OcctSharp package version | Public managed SDK release |
| Generator version | Generation behavior and model/emitter rules |
| Native ABI version | Compatibility between managed raw bindings and native bridge |
| Binding model schema version | Compatibility of canonical manifests and baselines |
| Configuration schema version | Compatibility of generator configuration files |
| OCCT version/build ID | Exact upstream native API and ABI input |

## Proposed policy

- Use semantic versioning for released OcctSharp packages and generator tooling.
- Increment the native ABI major version for incompatible ABI changes.
- Increment the native ABI minor version for compatible additive changes only after
  compatibility tests establish that property.
- Treat changes to ownership, disposal, error behavior, or public managed signatures
  as compatibility-significant even if native export names remain unchanged.
- Record the exact OCCT build identity in package metadata and runtime diagnostics.
- Do not force the OcctSharp package version to equal the OCCT version.

This policy is proposed until package layout and release ADRs are accepted.

## Runtime identity

The managed SDK should be able to query and report:

- OcctSharp managed package version.
- Generator version that produced the raw binding.
- Expected and loaded native ABI versions.
- Native bridge build identity.
- OCCT version/build identity.
- Platform, architecture, and compiler identity where useful for diagnosis.

Version mismatch diagnostics must show both expected and actual values.

The current workspace reports native ABI 1.15 and native bridge 0.16.0. ABI 1.4 adds the
generated `Precision`, `TopAbs`, `Standard`, `TopLoc`, and `gp` value-copy exports plus
the `gp_Pnt` default/copy constructors over ABI 1.3's coordinate constructor. ABI 1.5
adds the invalid-handle status and live shape-handle registry; the compatibility claim
is limited to the currently tested Windows x64 configuration. ABI 1.6 adds the
experimental `Standard_Transient` shared-handle probe and the package advances to
`0.1.0-alpha.3`. ABI 1.7 adds shared-handle runtime type identity queries and the
package advances to `0.1.0-alpha.4`. ABI 1.8 adds the checked derived shared-handle
cast and the package advances to `0.1.0-alpha.5`.
ABI 1.9 adds generated typed shared handles, beginning with `Geom_CartesianPoint`,
and the package advances to `0.1.0-alpha.6`. Configuration schema 1.3 adds explicit
shared-handle scopes without changing older value-scope configuration semantics.
ABI 1.10 adds generated `TopoDS_Shape` copy/null/kind/orientation/reversal and identity
semantics. Configuration schema 1.4 adds explicit topology scopes, and the package
advances to `0.1.0-alpha.7`.
ABI 1.11 adds generated checked `TopoDS_*` conversions for the eight topology subtypes;
the package advances to `0.1.0-alpha.8`.
ABI 1.12 adds the opaque `gp_Trsf` transformation value bridge and the package
advances to `0.1.0-alpha.9`.
ABI 1.13 adds the opaque `TopLoc_Location` value bridge and the package advances to
`0.1.0-alpha.10`.
ABI 1.14 completes the B05 opaque `gp_Vec`, `gp_Dir`, `gp_Ax1`, and `gp_Mat` value
family, including vector/axis transform creation, and the package advances to
`0.1.0-alpha.11`.
ABI 1.15 begins B06 with opaque OCCT string and `NCollection_Sequence<double>` values;
the package advances to `0.1.0-alpha.12`.
ABI 1.16 adds opaque `NCollection_Array1<double>` and the OCCT 8 dynamic-array-backed
`NCollection_Vector<double>` value collections; the package advances to `0.1.0-alpha.13`.
ABI 1.17 adds opaque integer-key `NCollection_DataMap<int,double>` and
`NCollection_IndexedMap<int>` collections; the package advances to `0.1.0-alpha.14`.

## Upgrade classification

An OCCT upgrade report must classify:

- Source/API changes in the selected generation scope.
- Native ABI changes in OcctSharp.
- Managed raw API changes.
- Friendly public API changes.
- Behavioral, ownership, and packaging changes.
- Compatibility impact and required migration.

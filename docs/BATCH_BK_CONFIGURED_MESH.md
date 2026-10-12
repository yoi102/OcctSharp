# Batch BK: configured mesh coordinates, Common materials and whole-round acceptance

State: IMPLEMENTED / LOCAL GATES PASS; final frozen delivery authority is batch-bk/delivery-result.json.
The authorized BJ delivery is frozen PASS before BK activation.
Development target: Preview.61 / ABI 1.104 / bridge 0.112.0.
The prepared exact audit selects 15 of 61 candidate declarations, defers 46 and
adds one necessary document length-unit prerequisite. Existing coordinate setters,
writer Perform operations, MeshUpAxis, PBR projection and definition mutation are
reused; they do not receive new migration credit.

## Private coordinate and material receivers

Each glTF JSON/OBJ operation owns its reader or writer and converter. Immutable
configuration uses finite positive meters-per-unit and the existing Z/Y MeshUpAxis.
glTF wire coordinates require meters/Y-up; model units and OBJ wire units are explicit.
Reader document units are initialized before setting its final converter, preventing
implicit unit adjustment from replacing the requested configuration.

Common material values copy ambient/diffuse/specular/emissive RGB, shininess,
transparency and presence. Texture handles do not escape. Mutation targets an exact
existing material label in a live issuing context, requires a caller command and uses
the existing nested transaction. Common replacement retains the original PBR values.
glTF uses PBR; SDK Common-to-PBR conversion is lossy. OBJ does not promise preservation
of glTF hierarchy, shared definitions or all material properties.

Textures, Draco/meshopt compression, GLB and subset writer overloads are excluded from
this additional profile. Existing default exchange APIs remain separate reused paths.
Diagnostics distinguish requested write metadata from metadata actually returned by
the native reader; unsupported custom keys are not described as stored native data.

## Bounded files and complete results

Managed input requires flat ASCII basenames, excluding URI escapes, reserved Windows
names, separators, reparse paths and missing side files. Private staged copies have
captured SHA256, bounded streaming and read locks. Admission permits at most 128 files
and 64 MiB aggregate bytes. glTF JSON preflight rejects cyclic/overdeep node graphs,
duplicate parents, invalid indices, nonfinite transforms and excessive accessor/buffer
cardinalities before invoking the native reader. This is not a computation timeout.

WriteNew stages outputs privately, admits bounded files and publishes only to absent
destinations. Failure removes only outputs created by that invocation. Source paths
and hashes remain detached diagnostics. Native readers may publish only a complete
close-safe document and independently copied triangulated shape; partial documents
and incomplete statuses fail without publication.

The pinned glTF writer emits repeated identical mesh records even when their complete
accessor/material resources are shared. The private managed writer consolidates only
byte-identical complete mesh records and remaps node mesh indices before publication.
Different names, materials, accessor indices or other mesh fields prevent merging.
This is serialized resource identity; it neither compares geometric tolerances nor
infers a source label or topology-history binding.

Native mesh budgets require existing triangulation on every face: at most 100,000
faces and 1,048,576 aggregate nodes and triangles. Source material textures are rejected.
Assembly graph validation bounds references and depth before recursive SDK writers.

## Controlled mesh copies and exact definitions

ControlledMeshing and BJ shape copies own independent topology. Exporting the original
document after meshing such a copy would not export the newly created triangulation.
ReplaceTriangulatedDefinition therefore copies geometry and triangulation into an exact
existing top-level simple definition, under the caller command and nested transaction,
then updates assemblies. Occurrence/assembly labels are rejected. Shared definitions,
occurrence labels and locations remain explicit; no topology history or face identity
across this replacement is inferred. Old topology-based consumers must refresh.

## Required delivery evidence

Both consumers must execute BG/BH geometry, BI source projection, actual BJ external
assembly, controlled independent meshing, exact definition writeback, configured glTF
and OBJ write/read, material/PBR preservation and copied lifetime. Position, instance
translation/rotation, normal and unit behavior require actual JSON/buffer and SDK
readback checks. An actual HWND/DXGI acceptance imports that exact glTF, persists and
reopens XBF, queries its saved occurrence/definition, registers that occurrence's
presentation and compares selected/picked IDs. Visibility and transform changes must
invalidate captured selection volumes. The direct module consumer must run this new
pixel/identity chain as well as inherited acceptance.

Generator, full Release/Debug/cold/actual Debug, 62 observed DLL identities, ABI/API,
five new exports, fourteen packages, source/payload checksums and final freeze remain
required. Only batch-bk/delivery-result.json PASS completes BK and this six-batch round.
See [ADR-0129](adr/0129-configured-mesh-document-exchange.md).

Local evidence:

15 admitted / 46 deferred, 1 new prerequisites / 44 reused; generated +16 / Manual +0.
Release/Debug/cold Generator 302, Runtime 1257: PASS.
Actual Debug-native 1257, 62 actually observed DLL paths/hashes: PASS.
Exports 41434 (+5/-0), Debug parity; API 58491 (+91/-0).
All 288 baseline generated files unchanged; current generated files 291.
ABI/lifetime/private headers and unified local package/consumer gates passed.
Final frozen delivery authority: OcctSharp/artifacts/batch-bk/delivery-result.json state PASS. This authority completes BF–BK 6/6; signing, hosted CI and public publication remain separate.

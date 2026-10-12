# ADR-0129: private configured mesh document exchange

Status: Accepted. The BK topic and frozen delivery authority record implementation and verification.
Date: 2026-10-11.

## Decision

Keep configured glTF JSON/OBJ readers, writers, converters and style receivers private
in the existing Xde module. Return immutable bounded configuration/material/file
diagnostics, complete close-safe documents and detached triangulated shape copies.
Reuse existing MeshUpAxis and coordinate operations. Maintain the existing Xde to
DataExchange dependency direction; do not create an inverse module edge.

Validate and stage explicit flat external payloads before native parsing. Require
meters/Y-up glTF wire coordinates, explicit model/OBJ units, finite values, bounded
graphs and existing triangulation. Preserve PBR during Common edits while disclosing
format conversion loss. Separate requested writer metadata from observed reader data.

The STEP assembly to mesh export chain must write independently meshed copies back
to exact existing simple definition labels under an existing document command.
Copy geometry/triangulation again, preflight assembly recursion, update assemblies
atomically and preserve the exact occurrence relations/locations. No replacement
history for constituent topology or automatic rebinding is promised.

## Consequences

Configuration does not leak mutable converters or borrowed native material fields.
Failing reads cannot publish partial document handles. The additional profile excludes
textures/compression and cannot claim full glTF/OBJ material/hierarchy equivalence.
Both facade and direct module consumers must provide actual saved-source selection
pixels and source invalidation evidence for the newly exported file.

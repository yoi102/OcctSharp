# OcctSharp 8.0.1-preview.1

This preview rebases the NuGet package identity onto the pinned OCCT 8.0.1 line. It
contains the same completed Batch D API/ABI implementation as alpha.55 and adds the
accepted preparation record for Batch E; it does not claim Batch E implementation.

## Version identity

- NuGet package: `OcctSharp` `8.0.1-preview.1`.
- OCCT baseline: 8.0.1 VC14 x64 combined runtime.
- Native ABI: 1.46.
- Bridge: 0.54.0.
- Managed assembly identity: 0.1.0.0, preserved for compatibility.
- Target: .NET 10, Windows x64.

The `preview.N` counter sequences OcctSharp prereleases for one OCCT three-part version.
It does not replace ABI/runtime/build checks or imply public-release readiness.

## Included implementation

Batch D remains complete at 24/24: copied XDE viewer identity, owning detection and
selection topology, rectangle/polygon selection, filters, isolate/fit, per-subshape
review styling, camera/conversions, clipping/review aids, and durable screenshot output.

## Local validation

- Release and Debug native/managed builds pass with Generator 91/91, Runtime 115/115,
  and dependency profiles 6/6.
- The nupkg and clean consumer pass with 62 DLLs, ABI 1.46, bridge 0.54.0, and OCCT 8.0.1.
- Direct binary inspection confirms assembly/file version `0.1.0.0` and exact
  informational version `8.0.1-preview.1`.
- Generated freshness, 83-file byte-identical clean regeneration, API compatibility,
  full classification, SBOM, provenance, checksums, and the complete local release check
  pass.

## Prepared next wave

ADR-0066 and `BATCH_E_INSPECTION_PMI_GAP_INVENTORY.md` lock a single 24-capability
engineering inspection, exact measurement, semantic PMI/AP242, saved-view, and viewer-
dimension closure. Its implementation and runtime/package gates are `NOT RUN`.

Hosted full release execution, signing, and NuGet publication remain separate and are
not authorized by this local preview package.

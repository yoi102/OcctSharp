# Release notes: 8.0.1-preview.30

Preview.30 delivers the Batch AF reusable copied-array/sequence generation profile.
It follows the fully validated Preview.29 development baseline.

## Included changes

- Exact Array1<double/int/gp_Pnt/gp_Pnt2d> and Sequence<double> copy contracts.
- Immutable managed CopiedArray<T>/CopiedSequence<T> snapshots with native bounds.
- 2D/3D Bezier and B-spline definition snapshots, copied output buffers and bulk knots.
- Point-set fitting with explicit/supplied parameterization and variational smoothing.
- Copied knot/pole reversal, reparameterization, multiplicity queries and interval merge.
- Domain/capacity/type/lifetime checks and success-only managed ref replacement.

Exactly 60 previously Blocked stable IDs migrate. Fourteen existing Manual curve and
fitting entries supply compatible supporting constructors/status/results; their
dispositions are unchanged. All generated code comes from audited generator rules
and templates. Existing generated files and interfaces remain compatible.

## Identity and boundaries

Package 8.0.1-preview.30; native ABI 1.74; bridge 0.82.0; Windows x64/.NET 10,
OCCT 8.0.1. Twelve managed modules, the facade and one native bridge remain. Fourteen
packages share one 62-DLL runtime. Earlier package versions are immutable.

This profile has one dimension. Array2, retained HArray inputs, arbitrary templates,
borrowed addresses, pointer results and zero-copy transfer require further contracts.
AG's surface-grid/interpolation scope and subsequent waves are planned in
[Batch AF](BATCH_AF_COPIED_COLLECTIONS.md); candidates are not implementation claims.

## Validation and distribution

Local engineering delivery validation is complete. Release/Debug Generator 197/197
and Runtime 625/625, isolated actual Debug-native 625/625, byte-identical cold
regeneration of 162 files, 14 packages, both clean consumers and the unified local
release check passed. Native exports are 40,968 (+96/-0), managed API delta is +96/-0
versus Preview.29, and fresh inventory reconciles the exact 60-ID migration with
zero Pending and unchanged 32 parser exclusions.

[STATUS](STATUS.md) and [Batch AF](BATCH_AF_COPIED_COLLECTIONS.md) record executed checks.
The unified local entry is `OcctSharp/eng/release-check.ps1`, supplemented by actual
Debug-native, ABI/lifetime and Preview.29 preservation checks. Hosted CI, signing,
push and public NuGet publication are separate and have not been executed. Existing
visual and downstream full-application acceptance gaps remain separate.

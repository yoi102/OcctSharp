# ADR-0124: Finite copied numeric results and pinned SDK include supplements

Status: Accepted. Date: 2026-10-10.

## Context

BF requires initialized LU/SVD/linear aggregates rather than exposing mutable native
result structs or publishing fields before all outputs are validated. The binary SDK
also omits GeomBndLib_InfiniteHelpers.pxx, which exists in the fixed source commit.

## Decision

TM041 admits eleven exact MathLin/MathUtils function signatures with fixed public
header/package/toolkit provenance. Private one-based matrices are limited to 64 by 64;
vectors to 4096. Inputs and thresholds must be finite. LU requires square matrices;
SVD and pseudoinverse permit rectangular matrices. SDK completion and all required
optional fields are checked before copying a complete aggregate. Native noncompletion
returns status and no data; exceptions and nonfinite outputs publish no aggregate.

Generated AtomicBuffer and WorkflowCopies support staged output validation, exact
result counts and budget+1 enumerator disposal for subsequent workflow profiles.
Output grids restore copied bounds. LU pivots are explicitly one-based private row
ordinals; SVD values preserve SDK order without assuming descending sorting.
No general arbitrary-struct, callback or borrowed-result mapping is introduced.

Supplemental includes are files relative to the discovery configuration, restricted
to its directory and SHA256-verified before parsing. Include search uses the installed
SDK first, then supplement directories. The source fragment retains its upstream
license and exact commit/hash provenance; the installed SDK is never patched.
Generation, configured discovery and recursive inventory isolation use the same
validated include directories. Missing/changed supplements fail closed.

## Alternatives

Patching generated output or inventing omitted SDK types would hide the defect and
break provenance. Globally adding the mutable SDK source checkout to INCLUDE would
silently change discovery identities. Both are rejected.

## Consequences and validation

Existing solver/owned-value APIs remain independent and reused. Numeric aggregate
inputs/results are detached; callers do not own private native solver state.
Strict execution timeout is not promised. C ABI callers supply valid disjoint buffers
of the declared dimensions; managed callers use internally allocated arrays.

Required evidence includes exact admission and positive/negative supplement fixtures,
LU/SVD reconstruction, inverse products, solve residuals, singular absence, invalid
budgets, lazy enumerator disposal, concurrent calls, actual fixed-header parse repair,
refreshed inventory accounting and full product delivery with both clean consumers.
See [BF](../BATCH_BF_NUMERIC_RESULTS.md) and [workflow](../WORKFLOW.md).

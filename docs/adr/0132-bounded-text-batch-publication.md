# ADR-0132: bounded text batch publication

Status: Accepted within the user-authorized BL–BQ scope. Date: 2026-10-11.

## Decision

TM050 extends Foundation with exact explicit-length private string operations and
Unicode BMP/token transforms. AV/SC-008 remain unchanged. Native values are local
to one operation; only independently copied UTF8/UTF16 data escapes.

Managed batch input uses bounded enumeration, complete preflight validation and
conservative cumulative output reservation. All native outputs are copied and
released before a read-only batch is published. Failure returns no partial batch.
Encoding/NUL, UTF16 surrogate boundaries and per-item/cumulative limits remain
explicit. Model text is Unicode; filesystem names require separate BP validation.
No global raw pointer/view mapping, native locale state or module boundary changes.

## Verification

Exact source/length admission, actual Unicode and atomic-batch regressions,
C canaries and full local delivery are required. Source generation, compilation
or focused checks alone do not prove frozen batch delivery.

# 8.0.1-preview.79 local candidate

11 existing private STEP reader/configuration operations now use generated exact
delegates. Generated STEPCAF WriteConfigured adds per-call AP203/AP214/AP242 and
session resolution with observed model diagnostics and staged output publication.
The full document retains SDK-supported semantic PMI. Exact field comparisons
publish geometry, Unicode metadata, color/material and PMI limitations.
Duplicate cleaning is opt-in because the pinned SDK fails the AP242 semantic-PMI
fixture when cleaning is enabled; explicit requested flags are never downgraded.
ABI1.114 / bridge0.128.0. Full local gates passed; final frozen authority is batch-cd/delivery-result.json.
Hosted CI/external CAD/full downstream/signing/push/public NuGet: NOT RUN.

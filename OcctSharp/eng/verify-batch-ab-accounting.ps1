[CmdletBinding()]
param(
    [string]$BaselineDirectory = 'artifacts/preparation-baselines/preview26-batch-ab',
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [string]$ManifestPath = 'generated/manifest.json',
    [string]$CataloguePath = 'src/OcctSharp.Generator/Transformation/owned-value-operation-contracts.json',
    [string]$DiscoveryPath = 'artifacts/generator-determinism/gp-pnt-discovery-1.json',
    [string]$OutputPath = 'artifacts/batch-ab-accounting.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$baseline = [IO.Path]::GetFullPath($BaselineDirectory, $workspace)
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$beforeFile = Join-Path $baseline 'full-inventory.json'
if ((Get-FileHash -LiteralPath $beforeFile -Algorithm SHA256).Hash -cne
    'AE0C2EA053355D9D94E2DD20F9FB0B2EF41A10E22610EF745C59EF7DA887966E') {
    throw 'Batch AB entry inventory is not the accepted Preview.26 baseline.'
}
$before = Get-Content -LiteralPath $beforeFile -Raw | ConvertFrom-Json
$after = Get-Content -LiteralPath $inventoryFile -Raw | ConvertFrom-Json
$oldManifest = Get-Content -LiteralPath (Join-Path $baseline 'manifest.json') -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath ([IO.Path]::GetFullPath($ManifestPath, $workspace)) -Raw | ConvertFrom-Json
$oldIds = [Collections.Generic.HashSet[string]]::new([string[]]$oldManifest.sourceStableIds, [StringComparer]::Ordinal)
$currentIds = [Collections.Generic.HashSet[string]]::new([string[]]$manifest.sourceStableIds, [StringComparer]::Ordinal)
if ($currentIds.Count -ne $manifest.sourceStableIds.Count) { throw 'The generated manifest contains duplicate IDs.' }
if ($oldIds.Count -ne 22636 -or -not $oldIds.IsSubsetOf($currentIds)) { throw 'Baseline generated IDs were lost or changed.' }
$added = [Collections.Generic.HashSet[string]]::new($currentIds, [StringComparer]::Ordinal)
$added.ExceptWith($oldIds)

$catalogue = Get-Content -LiteralPath ([IO.Path]::GetFullPath($CataloguePath, $workspace)) -Raw | ConvertFrom-Json
$contracts = @($catalogue.operations)
if ($catalogue.schemaVersion -ne '1.0' -or $contracts.Count -eq 0) { throw 'Missing owned operation catalogue.' }
$discovered = (Get-Content -LiteralPath ([IO.Path]::GetFullPath($DiscoveryPath, $workspace)) -Raw | ConvertFrom-Json).Model.Declarations
$discoveryIndex = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
foreach ($item in $discovered) { $discoveryIndex.Add($item.StableId, $item) }
foreach ($contract in $contracts) {
    if (-not $discoveryIndex.ContainsKey($contract.stableId) -or
        $discoveryIndex[$contract.stableId].NativeName -cne $contract.nativeName -or
        $discoveryIndex[$contract.stableId].NativeSignature -cne $contract.signature) {
        throw "Catalogue signature does not match semantic discovery: $($contract.stableId)"
    }
}
$contractIds = [Collections.Generic.HashSet[string]]::new([string[]]$contracts.stableId, [StringComparer]::Ordinal)
if ($contractIds.Count -ne $contracts.Count -or -not $contractIds.SetEquals($added)) {
    throw 'Generated additions differ from the exact audited owned-operation catalogue.'
}
$rows = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
foreach ($row in $after.FinalClassification.Declarations) {
    if ($rows.ContainsKey($row.StableId)) { throw "Duplicate exit ID: $($row.StableId)" }
    $rows.Add($row.StableId, $row)
}
if ($rows.Count -ne $before.FinalClassification.Declarations.Count -or $rows.Count -ne 116272) {
    throw 'The inventory denominator changed.'
}
$changes = [Collections.Generic.List[object]]::new()
$emitted = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($old in $before.FinalClassification.Declarations) {
    $id = $old.StableId
    if (-not $rows.ContainsKey($id)) { throw "Missing inventory ID: $id" }
    $new = $rows[$id]
    if ($new.State -eq 'Emitted') { [void]$emitted.Add($id) }
    if ($old.NativeName -cne $new.NativeName -or $old.Header -cne $new.Header -or
        $old.Kind -cne $new.Kind -or $old.SourcePackage -cne $new.SourcePackage -or
        $old.SourceToolkit -cne $new.SourceToolkit) {
        throw "Discovery identity changed: $id"
    }
    $changed = $old.State -cne $new.State -or $old.Code -cne $new.Code -or $old.Category -cne $new.Category
    if ($added.Contains($id)) {
        if ($old.State -ne 'Blocked' -or $new.State -ne 'Emitted' -or $new.Code -ne 'EM001' -or
            $new.Category -ne 'GeneratedBinding') {
            throw "New generated ID lacks Blocked-to-Emitted accounting: $id"
        }
    }
    elseif ($changed) { throw "Unrelated disposition changed: $id" }
    if ($changed) { $changes.Add([ordered]@{ stableId=$id; before=$old.State; after=$new.State; oldCode=$old.Code; newCode=$new.Code }) }
}
if (-not $emitted.SetEquals($currentIds)) { throw 'Manifest and inventory Emitted IDs differ.' }
foreach ($summary in $after.FinalClassification.DeclarationStates) {
    if (@($after.FinalClassification.Declarations | Where-Object State -eq $summary.State).Count -ne $summary.Count) {
        throw 'Inventory state summary does not match declaration rows.'
    }
}
$oldHeaders = $before.FinalClassification.Headers | ConvertTo-Json -Depth 5 -Compress
$newHeaders = $after.FinalClassification.Headers | ConvertTo-Json -Depth 5 -Compress
if ($oldHeaders -cne $newHeaders) { throw 'Header coverage or exclusions changed.' }
if (-not $after.FinalClassification.IsComplete -or $after.FinalClassification.DeclarationPending -ne 0 -or
    $after.FinalClassification.HeaderPending -ne 0 -or
    @($after.FinalClassification.Declarations | Where-Object State -eq 'SupportedUnselected').Count) {
    throw 'The exit inventory has pending work without a precise disposition.'
}
$report = [ordered]@{
    state='PASS'; baselineGenerated=$oldIds.Count; generated=$currentIds.Count; added=$added.Count
    inventorySha256=(Get-FileHash -LiteralPath $inventoryFile -Algorithm SHA256).Hash
    states=$after.FinalClassification.DeclarationStates; blockedReasonRefinements=0; transitions=$changes
}
$outputFile = [IO.Path]::GetFullPath($OutputPath, $workspace)
if ($outputFile -in @($beforeFile, $inventoryFile, (Join-Path $baseline 'manifest.json'), ([IO.Path]::GetFullPath($ManifestPath, $workspace)))) {
    throw 'Accounting report must not overwrite an input.'
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine)
Write-Host "Batch AB accounting PASS: $($added.Count) Blocked-to-Emitted IDs; zero Blocked reason refinements; no other disposition or header changes."

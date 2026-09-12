[CmdletBinding()]
param(
    [string]$BaselineDirectory = 'artifacts/preparation-baselines/preview27-batch-ac',
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [string]$ManifestPath = 'generated/manifest.json',
    [string]$CataloguePath = 'src/OcctSharp.Generator/Transformation/owned-value-operation-contracts.json',
    [string]$DiscoveryPath = 'artifacts/generator-determinism/gp-pnt-discovery-1.json',
    [string]$OutputPath = 'artifacts/batch-ac-accounting.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$baseline = [IO.Path]::GetFullPath($BaselineDirectory, $workspace)
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$beforeFile = Join-Path $baseline 'full-inventory.json'
if ((Get-FileHash -LiteralPath $beforeFile -Algorithm SHA256).Hash -cne
    '14B0D49AB1929D51D2E9BA17240641C6537C11DDEAF89F7F6F0AFAA0A9A24BE2') {
    throw 'Batch AC entry inventory is not the accepted Preview.27 baseline.'
}
$before = Get-Content -LiteralPath $beforeFile -Raw | ConvertFrom-Json
$after = Get-Content -LiteralPath $inventoryFile -Raw | ConvertFrom-Json
$oldManifest = Get-Content -LiteralPath (Join-Path $baseline 'manifest.json') -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath ([IO.Path]::GetFullPath($ManifestPath, $workspace)) -Raw | ConvertFrom-Json
$oldIds = [Collections.Generic.HashSet[string]]::new([string[]]$oldManifest.sourceStableIds, [StringComparer]::Ordinal)
$currentIds = [Collections.Generic.HashSet[string]]::new([string[]]$manifest.sourceStableIds, [StringComparer]::Ordinal)
if ($currentIds.Count -ne $manifest.sourceStableIds.Count) { throw 'The generated manifest contains duplicate IDs.' }
if ($oldIds.Count -ne 23703 -or -not $oldIds.IsSubsetOf($currentIds)) { throw 'Baseline generated IDs were lost or changed.' }
$added = [Collections.Generic.HashSet[string]]::new($currentIds, [StringComparer]::Ordinal)
$added.ExceptWith($oldIds)

$catalogue = Get-Content -LiteralPath ([IO.Path]::GetFullPath($CataloguePath, $workspace)) -Raw | ConvertFrom-Json
$contracts = @($catalogue.operations)
if ($catalogue.schemaVersion -ne '1.0' -or $contracts.Count -eq 0) { throw 'Missing owned operation catalogue.' }
$entryCataloguePath = Join-Path $baseline 'owned-value-operation-contracts.json'
if ((Get-FileHash -LiteralPath $entryCataloguePath).Hash -cne
    '4283609E14BF44E43805C9823165B8C4FCD41CEE0E45DC1ACC948F8EB84587DC') {
    throw 'The Preview.27 owned catalogue baseline changed.'
}
$entryCatalogue = Get-Content -LiteralPath $entryCataloguePath -Raw | ConvertFrom-Json
foreach ($group in @('owners', 'operations', 'supportingOperations')) {
    $key = if ($group -eq 'owners') { 'nativeName' } else { 'stableId' }
    foreach ($entry in $entryCatalogue.$group) {
        $entryMatches = @($catalogue.$group | Where-Object {
            $_.$key -ceq $entry.$key -and ($group -ne 'supportingOperations' -or $_.owner -ceq $entry.owner)
        })
        if ($entryMatches.Count -ne 1 -or ($entryMatches[0] | ConvertTo-Json -Depth 8 -Compress) -cne
            ($entry | ConvertTo-Json -Depth 8 -Compress)) {
            throw "Existing owned catalogue entry changed: $group / $($entry.$key)"
        }
    }
}
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
if ($contractIds.Count -ne $contracts.Count) { throw 'Duplicate owned catalogue IDs.' }
$contractIds.ExceptWith([string[]]$entryCatalogue.operations.stableId)
if (-not $contractIds.SetEquals($added)) {
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
if ($after.FinalClassification.DeclarationTotal -ne $rows.Count) {
    throw 'Inventory declaration total does not match declaration rows.'
}
$stateSummaries = @($after.FinalClassification.DeclarationStates)
$summaryStates = [Collections.Generic.HashSet[string]]::new([string[]]$stateSummaries.State, [StringComparer]::Ordinal)
$actualStates = [Collections.Generic.HashSet[string]]::new([string[]]($rows.Values | ForEach-Object State), [StringComparer]::Ordinal)
if ($summaryStates.Count -ne $stateSummaries.Count -or -not $summaryStates.SetEquals($actualStates)) {
    throw 'Inventory state summary set is missing, duplicated or unrelated.'
}
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
if ($outputFile.StartsWith($baseline + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
    $outputFile -in @($beforeFile, $inventoryFile, ([IO.Path]::GetFullPath($CataloguePath, $workspace)), ([IO.Path]::GetFullPath($DiscoveryPath, $workspace)), ([IO.Path]::GetFullPath($ManifestPath, $workspace)))) {
    throw 'Accounting report must not overwrite an input.'
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine)
Write-Host "Batch AC accounting PASS: $($added.Count) Blocked-to-Emitted IDs; zero Blocked reason refinements; no other disposition or header changes."

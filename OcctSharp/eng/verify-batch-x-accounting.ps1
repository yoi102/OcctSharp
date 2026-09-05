[CmdletBinding()]
param(
    [string]$BaselineDirectory = 'artifacts/preparation-baselines/preview22-batch-x',
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [string]$OutputPath = 'artifacts/batch-x-accounting.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$baseline = [IO.Path]::GetFullPath($BaselineDirectory, $workspace)
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$beforeFile = Join-Path $baseline 'full-inventory.json'
if ((Get-FileHash -LiteralPath $beforeFile -Algorithm SHA256).Hash -cne
    '260AE9A603E7B68F717C38C3FAF9C68A83F866D0154E84E1CD7FD811DB0C6A2C') {
    throw 'Batch X entry inventory is not the accepted Preview.22 baseline.'
}
$before = Get-Content -LiteralPath $beforeFile -Raw | ConvertFrom-Json
$after = Get-Content -LiteralPath $inventoryFile -Raw | ConvertFrom-Json
$oldManifest = Get-Content -LiteralPath (Join-Path $baseline 'manifest.json') -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath (Join-Path $workspace 'generated/manifest.json') -Raw | ConvertFrom-Json
$oldIds = [Collections.Generic.HashSet[string]]::new([string[]]$oldManifest.sourceStableIds, [StringComparer]::Ordinal)
$currentIds = [Collections.Generic.HashSet[string]]::new([string[]]$manifest.sourceStableIds, [StringComparer]::Ordinal)
if ($currentIds.Count -ne $manifest.sourceStableIds.Count) { throw 'The generated manifest contains duplicate IDs.' }
if ($oldIds.Count -ne 16353 -or -not $oldIds.IsSubsetOf($currentIds)) { throw 'Baseline generated IDs were lost or changed.' }
$added = [Collections.Generic.HashSet[string]]::new($currentIds, [StringComparer]::Ordinal)
$added.ExceptWith($oldIds)
$rows = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
foreach ($row in $after.FinalClassification.Declarations) {
    if ($rows.ContainsKey($row.StableId)) { throw "Duplicate exit ID: $($row.StableId)" }
    $rows.Add($row.StableId, $row)
}
if ($rows.Count -ne $before.FinalClassification.Declarations.Count -or $rows.Count -ne 116272) {
    throw 'The inventory denominator changed.'
}
$changes = [Collections.Generic.List[object]]::new()
$refinements = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
$refinementFile = Join-Path $workspace 'config/batches/batch-x-disposition-refinements.json'
foreach ($item in (Get-Content -LiteralPath $refinementFile -Raw | ConvertFrom-Json).declarations) {
    if ([string]::IsNullOrWhiteSpace($item.reason)) { throw 'Refinement evidence must include a reason.' }
    $refinements.Add($item.stableId, $item)
}
$seenRefinements = 0
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
    elseif ($id -ceq 'c:@S@Cocoa_Window@F@VirtualKeyFromNative#I#S') {
        if ($old.State -ne 'Blocked' -or $new.State -ne 'Skipped' -or $new.Code -ne 'SK008') {
            throw 'The exact Windows artifact exception was not preserved.'
        }
    }
    elseif ($id -ceq 'c:@S@BSplCLib@F@FlatBezierKnots#I#S') {
        if ($old.State -ne 'Blocked' -or $new.State -ne 'Blocked' -or $new.Code -ne 'BL209') {
            throw 'The reference-sequence contract must remain Blocked.'
        }
    }
    elseif ($refinements.ContainsKey($id)) {
        $expected = $refinements[$id]
        if ($old.State -ne 'Blocked' -or $new.State -ne 'Blocked' -or
            $old.Code -cne $expected.beforeCode -or $new.Code -cne $expected.afterCode -or
            $old.Category -cne $expected.beforeCategory -or $new.Category -cne $expected.afterCategory -or
            $new.NativeName -cne $expected.nativeName) { throw "Audited Blocked refinement changed: $id" }
        $seenRefinements++
    }
    elseif ($changed) { throw "Unrelated disposition changed: $id" }
    if ($changed) { $changes.Add([ordered]@{ stableId=$id; before=$old.State; after=$new.State; oldCode=$old.Code; newCode=$new.Code }) }
}
if (-not $emitted.SetEquals($currentIds)) { throw 'Manifest and inventory Emitted IDs differ.' }
if ($refinements.Count -ne 57 -or $seenRefinements -ne $refinements.Count) { throw 'Refinement audit is incomplete.' }
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
    states=$after.FinalClassification.DeclarationStates; blockedReasonRefinements=$seenRefinements; transitions=$changes
}
$outputFile = [IO.Path]::GetFullPath($OutputPath, $workspace)
if ($outputFile -in @($beforeFile, $inventoryFile, (Join-Path $baseline 'manifest.json'), (Join-Path $workspace 'generated/manifest.json'))) {
    throw 'Accounting report must not overwrite an input.'
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine)
Write-Host "Batch X accounting PASS: $($added.Count) Blocked-to-Emitted IDs; two exact exceptions; $seenRefinements audited Blocked reason refinements; no other disposition or header changes."

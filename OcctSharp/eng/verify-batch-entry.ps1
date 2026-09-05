[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$FrozenConfigPath,
    [Parameter(Mandatory)][string]$EntryConfigPath,
    [Parameter(Mandatory)][string]$FrozenInventoryPath,
    [Parameter(Mandatory)][string]$EntryInventoryPath,
    [Parameter(Mandatory)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
function Resolve-Input([string]$Path) { [IO.Path]::GetFullPath($Path, $workspaceRoot) }
$inputs = @($FrozenConfigPath, $EntryConfigPath, $FrozenInventoryPath, $EntryInventoryPath) |
    ForEach-Object { Resolve-Input $_ }
$outputFile = Resolve-Input $OutputPath
if ($outputFile -in $inputs) { throw 'Entry evidence must not overwrite inputs.' }
$frozen = Get-Content -LiteralPath $inputs[0] -Raw | ConvertFrom-Json
$entry = Get-Content -LiteralPath $inputs[1] -Raw | ConvertFrom-Json
if ($frozen.batch -cne $entry.batch -or $frozen.capabilityCount -ne $entry.capabilityCount -or
    @((Compare-Object $frozen.roots $entry.roots -CaseSensitive)).Count) {
    throw 'Entry revalidation must preserve the prepared batch and exact root set.'
}
$maps = @()
for ($i = 0; $i -lt 2; $i++) {
    $file = $inputs[$i + 2]
    $expected = @($frozen, $entry)[$i].baselineInventorySha256
    if ((Get-FileHash -LiteralPath $file).Hash -cne $expected) { throw 'Inventory baseline hash mismatch.' }
    $inventory = Get-Content -LiteralPath $file -Raw | ConvertFrom-Json
    if (-not $inventory.FinalClassification.IsComplete -or
        $inventory.FinalClassification.DeclarationPending -ne 0 -or $inventory.FinalClassification.HeaderPending -ne 0) {
        throw 'Entry requires complete classification.'
    }
    $map = [Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
    foreach ($item in $inventory.FinalClassification.Declarations) {
        if (-not $map.TryAdd($item.StableId, $item)) { throw 'Duplicate case-sensitive declaration ID.' }
    }
    $maps += ,$map
}
$changes = [Collections.Generic.List[object]]::new()
foreach ($id in $maps[1].Keys | Sort-Object -CaseSensitive) {
    if (-not $maps[0].ContainsKey($id)) { throw "New declaration requires a separate signature review: $id" }
    $old = $maps[0][$id]; $new = $maps[1][$id]
    foreach ($field in @('NativeName', 'Kind', 'Header', 'SourcePackage', 'SourceToolkit')) {
        if ($old.$field -cne $new.$field) { throw "Declaration identity changed: $id / $field" }
    }
    if ($old.State -cne $new.State -or $old.Code -cne $new.Code -or $old.Category -cne $new.Category) {
        $changes.Add([ordered]@{
            stableId = $id; nativeName = $new.NativeName; header = $new.Header
            previousState = $old.State; entryState = $new.State
            previousCode = $old.Code; entryCode = $new.Code
            previousCategory = $old.Category; entryCategory = $new.Category
            affectsBatchRoots = ($new.NativeName -split '::', 2)[0] -cin $entry.roots
        })
    }
}
if ($maps[0].Count -ne $maps[1].Count) { throw 'Declaration removal requires a separate review.' }
$affected = @($changes | Where-Object affectsBatchRoots)
$report = [ordered]@{
    batch = $entry.batch; entryCommit = $entry.baselineCommit; capabilityCount = $entry.capabilityCount
    frozenInventorySha256 = $frozen.baselineInventorySha256; entryInventorySha256 = $entry.baselineInventorySha256
    declarations = $maps[1].Count; added = 0; removed = 0; identityChanges = 0
    classificationChanges = $changes.Count; affectedBatchRootChanges = $affected.Count
    changes = @($changes.ToArray())
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Write-Host "Entry PASS: $($changes.Count) classification changes; $($affected.Count) affect Batch $($entry.batch) roots; zero identity changes."
$affected | ForEach-Object { Write-Host "$($_.stableId) : $($_.previousState) -> $($_.entryState)" }

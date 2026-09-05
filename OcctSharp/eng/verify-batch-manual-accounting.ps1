[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ConfigPath,
    [Parameter(Mandatory)][string]$BaselineInventoryPath,
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$configFile = [IO.Path]::GetFullPath($ConfigPath, $workspaceRoot)
$baselineFile = [IO.Path]::GetFullPath($BaselineInventoryPath, $workspaceRoot)
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspaceRoot)
$config = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
$generationFile = Join-Path $workspaceRoot 'config/generation.json'
$generation = Get-Content -LiteralPath $generationFile -Raw | ConvertFrom-Json
$baselineHash = (Get-FileHash -LiteralPath $baselineFile -Algorithm SHA256).Hash
if ($baselineHash -cne $config.baselineInventorySha256) {
    throw 'The manual-call baseline hash does not match the frozen input.'
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = "artifacts/generator-reports/batch-$($config.batch.ToLowerInvariant())-manual-accounting.json"
}
$outputFile = [IO.Path]::GetFullPath($OutputPath, $workspaceRoot)
if ($outputFile -in @($configFile, $baselineFile, $inventoryFile, $generationFile)) {
    throw 'The accounting output must not overwrite an input.'
}

function Read-CompleteInventory([string]$Path) {
    $inventory = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    if (-not $inventory.FinalClassification.IsComplete -or
        $inventory.FinalClassification.DeclarationPending -ne 0 -or
        $inventory.FinalClassification.HeaderPending -ne 0) {
        throw "Incomplete classification: '$Path'."
    }
    $map = [Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
    foreach ($item in $inventory.FinalClassification.Declarations) {
        if (-not $map.TryAdd($item.StableId, $item)) { throw "Duplicate declaration: '$($item.StableId)'." }
    }
    return ,$map
}

$before = Read-CompleteInventory $baselineFile
$after = Read-CompleteInventory $inventoryFile
$accepted = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($call in $config.calls) {
    if (-not $accepted.Add($call.stableId)) { throw "Duplicate manual call: '$($call.stableId)'." }
    if (-not $before.ContainsKey($call.stableId) -or -not $after.ContainsKey($call.stableId)) {
        throw "Manual call is not present in both inventories: '$($call.stableId)'."
    }
    $old = $before[$call.stableId]
    $new = $after[$call.stableId]
    if ($old.State -ne 'Blocked' -or $new.State -ne 'Manual' -or
        $new.Code -ne 'MN001' -or $new.Category -ne 'ManualBinding' -or
        $old.NativeName -cne $call.nativeName -or $old.Header -cne $call.header) {
        throw "Invalid exact manual-call transition: '$($call.stableId)'."
    }
}
$configured = @($generation.manualBindings | Where-Object specialCaseId -CEQ $config.specialCaseId | ForEach-Object stableId)
if ($configured.Count -ne $accepted.Count -or
    @($configured | Sort-Object -CaseSensitive -Unique).Count -ne $accepted.Count -or
    @($configured | Where-Object { -not $accepted.Contains($_) }).Count) {
    throw 'The special-case rule does not match the exact audited call set.'
}
if ($before.Count -ne $after.Count) { throw 'The declaration set changed; record and review that baseline delta separately.' }
foreach ($old in $before.Values) {
    if (-not $after.ContainsKey($old.StableId)) { throw "Declaration removed: '$($old.StableId)'." }
    $new = $after[$old.StableId]
    foreach ($field in @('NativeName', 'Kind', 'Header', 'SourcePackage', 'SourceToolkit')) {
        if ($old.$field -cne $new.$field) { throw "Declaration identity changed: '$($old.StableId)' / $field." }
    }
    if (-not $accepted.Contains($old.StableId)) {
        foreach ($field in @('State', 'Code', 'Category')) {
            if ($old.$field -cne $new.$field) { throw "Unaudited classification change: '$($old.StableId)' / $field." }
        }
    }
}
$report = [ordered]@{
    schemaVersion = '1.0'
    batch = $config.batch
    specialCaseId = $config.specialCaseId
    baselineInventorySha256 = $baselineHash
    inventorySha256 = (Get-FileHash -LiteralPath $inventoryFile -Algorithm SHA256).Hash
    declarationCount = $after.Count
    exactBlockedToManualCount = $accepted.Count
    otherDeclarationOrClassificationChanges = 0
    stableIds = @($accepted | Sort-Object -CaseSensitive)
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 5) + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Write-Host "Manual accounting PASS: $($accepted.Count) exact $($config.specialCaseId) transitions; no other declaration/classification changes."

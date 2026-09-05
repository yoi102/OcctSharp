[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ConfigPath,
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$configFile = [IO.Path]::GetFullPath($ConfigPath, $workspaceRoot)
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspaceRoot)
$config = Get-Content -LiteralPath $configFile -Raw -Encoding UTF8 | ConvertFrom-Json
$inventoryHash = (Get-FileHash -LiteralPath $inventoryFile -Algorithm SHA256).Hash
if ($config.baselineInventorySha256 -cne $inventoryHash) {
    throw 'The inventory does not match the frozen batch baseline. Do not silently rebase a prepared batch.'
}
$inventory = Get-Content -LiteralPath $inventoryFile -Raw -Encoding UTF8 | ConvertFrom-Json
if (-not $inventory.FinalClassification.IsComplete -or
    $inventory.FinalClassification.DeclarationPending -ne 0 -or
    $inventory.FinalClassification.HeaderPending -ne 0) {
    throw 'Batch preparation requires a complete final-classification baseline.'
}

$roots = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($root in $config.roots) {
    if ([string]::IsNullOrWhiteSpace($root) -or -not $roots.Add($root)) {
        throw "Empty or duplicate batch root: '$root'."
    }
}
if ($roots.Count -eq 0) { throw 'At least one exact native root is required.' }
$candidates = @($inventory.FinalClassification.Declarations | Where-Object {
    $root = ($_.NativeName -split '::', 2)[0]
    $roots.Contains($root)
} | Sort-Object StableId)
$ids = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($candidate in $candidates) {
    if (-not $ids.Add($candidate.StableId)) { throw "Duplicate stable ID '$($candidate.StableId)'." }
}
$rootCounts = @($candidates | Group-Object { ($_.NativeName -split '::', 2)[0] } |
    Sort-Object Name | ForEach-Object {
        [ordered]@{ root = $_.Name; count = $_.Count }
    })
if ($rootCounts.Count -ne $roots.Count) { throw 'One or more exact roots have no inventory declarations.' }
$states = @($candidates | Group-Object State | Sort-Object Name | ForEach-Object {
    [ordered]@{ state = $_.Name; count = $_.Count }
})
$report = [ordered]@{
    schemaVersion = '1.0'
    batch = $config.batch
    baselinePackageVersion = $config.baselinePackageVersion
    baselineInventorySha256 = $inventoryHash
    plannedPackageVersion = $config.plannedPackageVersion
    capabilityCount = $config.capabilityCount
    rootCount = $roots.Count
    candidateCount = $candidates.Count
    states = $states
    roots = $rootCounts
    candidates = $candidates
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = "artifacts/generator-reports/batch-$($config.batch.ToLowerInvariant())-root-audit.json"
}
$outputFile = [IO.Path]::GetFullPath($OutputPath, $workspaceRoot)
if ($outputFile -eq $configFile -or $outputFile -eq $inventoryFile) {
    throw 'The audit output must not overwrite its inputs.'
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
Write-Host "Batch $($config.batch): $($roots.Count) exact roots, $($candidates.Count) candidates, $($config.capabilityCount) product capabilities."
$states | ForEach-Object { Write-Host "$($_.state): $($_.count)" }
Write-Host "Audit: '$outputFile'. No binding disposition was changed."

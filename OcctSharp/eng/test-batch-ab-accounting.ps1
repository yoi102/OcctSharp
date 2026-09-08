[CmdletBinding()]
param([string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$source = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$hash = (Get-FileHash -LiteralPath $source).Hash
$output = Join-Path $workspace ('artifacts/batch-ab-accounting-negatives/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($output) | Out-Null
$cases = @('denominator','unrelated','migration','header')
foreach ($case in $cases) {
    $inventory = Get-Content -LiteralPath $source -Raw | ConvertFrom-Json
    switch ($case) {
        'denominator' { $inventory.FinalClassification.Declarations = @($inventory.FinalClassification.Declarations | Select-Object -Skip 1) }
        'unrelated' { ($inventory.FinalClassification.Declarations | Where-Object State -eq 'Manual' | Select-Object -First 1).Code = 'MN999' }
        'migration' { ($inventory.FinalClassification.Declarations | Where-Object StableId -ceq (Get-Content (Join-Path $workspace 'src/OcctSharp.Generator/Transformation/owned-value-operation-contracts.json') -Raw | ConvertFrom-Json).operations[0].stableId | Select-Object -First 1).State = 'Skipped' }
        'header' { $inventory.FinalClassification.Headers[0].Code = 'HD099' }
    }
    $fixture = Join-Path $output "$case.json"
    [IO.File]::WriteAllText($fixture, ($inventory | ConvertTo-Json -Depth 100))
    $rejected = $false
    try { & (Join-Path $PSScriptRoot 'verify-batch-ab-accounting.ps1') -InventoryPath $fixture -OutputPath (Join-Path $output "$case-result.json") }
    catch {
        $expected = switch ($case) {
            'denominator' { '*denominator changed*' }
            'unrelated' { '*Unrelated disposition changed*' }
            'migration' { '*New generated ID lacks Blocked-to-Emitted accounting*' }
            'header' { '*Header coverage or exclusions changed*' }
        }
        if ($_.Exception.Message -notlike $expected) { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw "Accounting accepted invalid $case fixture." }
    Write-Host "PASS: rejected $case mutation."
}
if ((Get-FileHash -LiteralPath $source).Hash -cne $hash) { throw 'The inventory input changed during negative validation.' }
foreach ($case in @('signature','missing','extra')) {
    $catalogue = Get-Content (Join-Path $workspace 'src/OcctSharp.Generator/Transformation/owned-value-operation-contracts.json') -Raw | ConvertFrom-Json
    $manifest = Get-Content (Join-Path $workspace 'generated/manifest.json') -Raw | ConvertFrom-Json
    if ($case -eq 'signature') { $catalogue.operations[0].signature += ' changed' }
    if ($case -eq 'missing') { $manifest.sourceStableIds = @($manifest.sourceStableIds | Where-Object { $_ -cne $catalogue.operations[0].stableId }) }
    if ($case -eq 'extra') { $manifest.sourceStableIds += 'unreviewed:new-id' }
    $catalogueFile = Join-Path $output "$case-catalogue.json"
    $manifestFile = Join-Path $output "$case-manifest.json"
    [IO.File]::WriteAllText($catalogueFile, ($catalogue | ConvertTo-Json -Depth 100))
    [IO.File]::WriteAllText($manifestFile, ($manifest | ConvertTo-Json -Depth 100))
    $rejected = $false
    try { & (Join-Path $PSScriptRoot 'verify-batch-ab-accounting.ps1') -InventoryPath $source -CataloguePath $catalogueFile -ManifestPath $manifestFile -OutputPath (Join-Path $output "$case-result.json") }
    catch {
        $expected = if ($case -eq 'signature') { '*Catalogue signature does not match*' } else { '*Generated additions differ*' }
        if ($_.Exception.Message -notlike $expected) { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw "Accounting accepted invalid $case fixture." }
    Write-Host "PASS: rejected $case mutation."
}
Write-Host 'Batch AB accounting negative fixtures PASS: seven cases, source inventory unchanged.'

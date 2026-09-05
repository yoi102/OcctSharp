[CmdletBinding()]
param([string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$source = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$hash = (Get-FileHash -LiteralPath $source).Hash
$output = Join-Path $workspace ('artifacts/batch-y-accounting-negatives/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($output) | Out-Null
$cases = @('denominator','unrelated','refinement','header')
foreach ($case in $cases) {
    $inventory = Get-Content -LiteralPath $source -Raw | ConvertFrom-Json
    switch ($case) {
        'denominator' { $inventory.FinalClassification.Declarations = @($inventory.FinalClassification.Declarations | Select-Object -Skip 1) }
        'unrelated' { ($inventory.FinalClassification.Declarations | Where-Object State -eq 'Manual' | Select-Object -First 1).Code = 'MN999' }
        'refinement' { ($inventory.FinalClassification.Declarations | Where-Object Code -eq 'BL210' | Select-Object -First 1).State = 'Skipped' }
        'header' { $inventory.FinalClassification.Headers[0].Code = 'HD099' }
    }
    $fixture = Join-Path $output "$case.json"
    [IO.File]::WriteAllText($fixture, ($inventory | ConvertTo-Json -Depth 100))
    $rejected = $false
    try { & (Join-Path $PSScriptRoot 'verify-batch-y-accounting.ps1') -InventoryPath $fixture -OutputPath (Join-Path $output "$case-result.json") }
    catch {
        $expected = switch ($case) {
            'denominator' { '*denominator changed*' }
            'unrelated' { '*Unrelated disposition changed*' }
            'refinement' { '*Audited Blocked refinement changed*' }
            'header' { '*Header coverage or exclusions changed*' }
        }
        if ($_.Exception.Message -notlike $expected) { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw "Accounting accepted invalid $case fixture." }
    Write-Host "PASS: rejected $case mutation."
}
if ((Get-FileHash -LiteralPath $source).Hash -cne $hash) { throw 'The inventory input changed during negative validation.' }
Write-Host 'Batch Y accounting negative fixtures PASS: four cases, source inventory unchanged.'

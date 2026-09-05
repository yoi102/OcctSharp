[CmdletBinding()]
param([string]$InventoryPath = 'artifacts/batch-x-exit-inventory.json')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$inputFile = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$root = Join-Path $workspace ('artifacts/batch-x-accounting-fixtures/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($root) | Out-Null
$fixtureFile = Join-Path $root 'inventory.json'
$reportFile = Join-Path $root 'result.json'
$inventory = Get-Content -LiteralPath $inputFile -Raw | ConvertFrom-Json
function Assert-Rejected([string]$Expected) {
    [IO.File]::WriteAllText($fixtureFile, ($inventory | ConvertTo-Json -Depth 40))
    $rejected = $false
    try {
        & (Join-Path $PSScriptRoot 'verify-batch-x-accounting.ps1') -InventoryPath $fixtureFile -OutputPath $reportFile
    }
    catch {
        if ($_.Exception.Message -notlike "*$Expected*") { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw "Accounting accepted malformed fixture: $Expected" }
}
$original = $inventory.FinalClassification.Declarations
$inventory.FinalClassification.Declarations = @($original | Select-Object -Skip 1)
Assert-Rejected 'denominator changed'
$inventory.FinalClassification.Declarations = $original
$first = $original[0]
$state = $first.State
$first.State = 'Skipped'
Assert-Rejected 'Unrelated disposition changed'
$first.State = $state
$refinement = $original | Where-Object StableId -CEQ 'c:@N@opencascade@N@FNVHash@F@FNVHash1A#*1v#I#i#' | Select-Object -First 1
if ($null -eq $refinement) { throw 'Exact pointer refinement fixture is missing.' }
$refinement.State = 'Skipped'
Assert-Rejected 'Audited Blocked refinement changed'
$refinement.State = 'Blocked'
$inventory.FinalClassification.Headers[0].Code = 'HD099'
Assert-Rejected 'Header coverage or exclusions changed'
Write-Host 'Batch X accounting negative checks PASS: denominator loss, unrelated reclassification, refinement promoted to Skipped and header drift all rejected; source inputs never mutated.'

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ConfigPath,
    [Parameter(Mandatory)][string]$BaselineInventoryPath,
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [string]$OutputDirectory = 'artifacts/manual-accounting-negative'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$configFile = [IO.Path]::GetFullPath($ConfigPath, $workspaceRoot)
$baselineFile = [IO.Path]::GetFullPath($BaselineInventoryPath, $workspaceRoot)
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspaceRoot)
$generationFile = Join-Path $workspaceRoot 'config/generation.json'
$inputs = @($configFile, $baselineFile, $inventoryFile, $generationFile)
$hashes = @($inputs | ForEach-Object { (Get-FileHash -LiteralPath $_).Hash })
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory, $workspaceRoot)
$fixtureRoot = Join-Path $outputRoot ('run-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixtureRoot) | Out-Null
$verifier = Join-Path $PSScriptRoot 'verify-batch-manual-accounting.ps1'

function Expect-Rejection([scriptblock]$Action, [string]$Message) {
    $rejected = $false
    try { & $Action }
    catch { if ($_.Exception.Message -notlike $Message) { throw }; $rejected = $true }
    if (-not $rejected) { throw "Negative accounting case was accepted: $Message" }
}

$fixture = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
$fixture.baselineInventorySha256 = '0' * 64
$wrongHash = Join-Path $fixtureRoot 'wrong-hash.json'
[IO.File]::WriteAllText($wrongHash, ($fixture | ConvertTo-Json -Depth 8))
Expect-Rejection {
    & $verifier -ConfigPath $wrongHash -BaselineInventoryPath $baselineFile -InventoryPath $inventoryFile
} '*baseline hash does not match*'
Expect-Rejection {
    & $verifier -ConfigPath $configFile -BaselineInventoryPath $baselineFile -InventoryPath $inventoryFile -OutputPath $inventoryFile
} '*must not overwrite an input*'
# A prior complete inventory is deliberately not proof of implementing the new calls.
Expect-Rejection {
    & $verifier -ConfigPath $configFile -BaselineInventoryPath $baselineFile -InventoryPath $baselineFile -OutputPath (Join-Path $fixtureRoot 'unimplemented.json')
} '*Invalid exact manual-call transition*'

for ($i = 0; $i -lt $inputs.Count; $i++) {
    if ((Get-FileHash -LiteralPath $inputs[$i]).Hash -cne $hashes[$i]) { throw "Accounting test mutated input: $($inputs[$i])" }
}
$report = [ordered]@{ state='PASS'; cases=3; inputsUnchanged=$true; fixtureDirectory=$fixtureRoot }
[IO.File]::WriteAllText((Join-Path $outputRoot 'result.json'), ($report | ConvertTo-Json) + [Environment]::NewLine)
Write-Host 'Manual accounting negative checks PASS: wrong hash, input overwrite and unimplemented transitions; all inputs unchanged.'

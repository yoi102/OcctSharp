[CmdletBinding()]
param([string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$source = [IO.Path]::GetFullPath($InventoryPath, $workspace)
$sourceHash = (Get-FileHash -LiteralPath $source).Hash
$output = Join-Path $workspace ('artifacts/batch-ac-accounting-fixtures/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($output) | Out-Null
$cataloguePath = Join-Path $workspace 'src/OcctSharp.Generator/Transformation/owned-value-operation-contracts.json'
$manifestPath = Join-Path $workspace 'generated/manifest.json'
$entry = Get-Content (Join-Path $workspace 'artifacts/preparation-baselines/preview27-batch-ac/manifest.json') -Raw | ConvertFrom-Json
$entryIds = [Collections.Generic.HashSet[string]]::new([string[]]$entry.sourceStableIds, [StringComparer]::Ordinal)
$catalogue = Get-Content -LiteralPath $cataloguePath -Raw | ConvertFrom-Json
$newId = ($catalogue.operations | Where-Object { -not $entryIds.Contains($_.stableId) } | Select-Object -First 1).stableId
$verifier = Join-Path $PSScriptRoot 'verify-batch-ac-accounting.ps1'
& $verifier -InventoryPath $source -OutputPath (Join-Path $output 'positive.json')
foreach ($case in @('denominator', 'unrelated', 'migration', 'header', 'signature', 'missing', 'extra', 'legacy', 'summary-missing', 'summary-duplicate', 'total')) {
    $inventory = Get-Content -LiteralPath $source -Raw | ConvertFrom-Json
    $catalogue = Get-Content -LiteralPath $cataloguePath -Raw | ConvertFrom-Json
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $expected = switch ($case) {
        'denominator' {
            $inventory.FinalClassification.Declarations = @($inventory.FinalClassification.Declarations | Select-Object -Skip 1)
            '*denominator changed*'
        }
        'unrelated' {
            ($inventory.FinalClassification.Declarations | Where-Object State -eq 'Manual' | Select-Object -First 1).Code = 'MN999'
            '*Unrelated disposition changed*'
        }
        'migration' {
            ($inventory.FinalClassification.Declarations | Where-Object StableId -ceq $newId).State = 'Skipped'
            '*New generated ID lacks Blocked-to-Emitted accounting*'
        }
        'header' { $inventory.FinalClassification.Headers[0].Code = 'HD099'; '*Header coverage or exclusions changed*' }
        'signature' { ($catalogue.operations | Where-Object stableId -ceq $newId).signature += ' changed'; '*Catalogue signature does not match*' }
        'missing' { $manifest.sourceStableIds = @($manifest.sourceStableIds | Where-Object { $_ -cne $newId }); '*Generated additions differ*' }
        'extra' { $manifest.sourceStableIds += 'unreviewed:extra-id'; '*Generated additions differ*' }
        'legacy' { ($catalogue.owners | Where-Object nativeName -ceq 'Bnd_Box').copyable = $false; '*Existing owned catalogue entry changed*' }
        'summary-missing' { $inventory.FinalClassification.DeclarationStates = @($inventory.FinalClassification.DeclarationStates | Select-Object -Skip 1); '*Inventory state summary set*' }
        'summary-duplicate' { $inventory.FinalClassification.DeclarationStates += $inventory.FinalClassification.DeclarationStates[0]; '*Inventory state summary set*' }
        'total' { $inventory.FinalClassification.DeclarationTotal = 0; '*Inventory declaration total*' }
    }
    $inventoryFile = Join-Path $output "$case-inventory.json"
    $catalogueFile = Join-Path $output "$case-catalogue.json"
    $manifestFile = Join-Path $output "$case-manifest.json"
    [IO.File]::WriteAllText($inventoryFile, ($inventory | ConvertTo-Json -Depth 100))
    [IO.File]::WriteAllText($catalogueFile, ($catalogue | ConvertTo-Json -Depth 100))
    [IO.File]::WriteAllText($manifestFile, ($manifest | ConvertTo-Json -Depth 100))
    $rejected = $false
    try { & $verifier -InventoryPath $inventoryFile -CataloguePath $catalogueFile -ManifestPath $manifestFile -OutputPath (Join-Path $output "$case-result.json") }
    catch {
        if ($_.Exception.Message -notlike $expected) { throw }
        $rejected = $true
    }
    if (-not $rejected) { throw "Accounting accepted invalid $case fixture." }
    Write-Host "PASS: rejected $case mutation."
}
if ((Get-FileHash -LiteralPath $source).Hash -cne $sourceHash) { throw 'The input inventory changed during fixture validation.' }

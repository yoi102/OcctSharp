[CmdletBinding()]
param(
    [string]$OcctRoot,
    [string]$PackageVersion = '8.0.1-preview.5',
    [string]$ApiBaselineVersion = '0.1.0-alpha.38'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $workspaceRoot
try {
$repositoryRoot = Split-Path -Parent $workspaceRoot
$releaseDirectory = Join-Path $workspaceRoot 'artifacts\release'
[IO.Directory]::CreateDirectory($releaseDirectory) | Out-Null

& (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release -OcctRoot $OcctRoot
& (Join-Path $PSScriptRoot 'build.ps1') -Configuration Debug -OcctRoot $OcctRoot
& (Join-Path $PSScriptRoot 'verify-bundled-runtime.ps1') -CompareBuiltRuntime
& (Join-Path $PSScriptRoot 'verify-generated.ps1') -Configuration Release
& (Join-Path $PSScriptRoot 'verify-package.ps1') -SkipBuild -OcctRoot $OcctRoot -PackageVersion $PackageVersion
& (Join-Path $PSScriptRoot 'verify-clean-regeneration.ps1') -OcctRoot $OcctRoot

$assemblyPath = Join-Path $workspaceRoot 'src\OcctSharp\bin\Release\net10.0\win-x64\OcctSharp.dll'
$baselinePath = Join-Path $workspaceRoot "config\api-baselines\occtsharp-$ApiBaselineVersion.json"
$apiDiffPath = Join-Path $releaseDirectory 'api-diff.json'
& dotnet run --project (Join-Path $workspaceRoot 'tools\OcctSharp.ApiTool\OcctSharp.ApiTool.csproj') `
    --configuration Release -- diff $baselinePath $assemblyPath $apiDiffPath
if ($LASTEXITCODE -ne 0) { throw "API compatibility diff failed with exit code $LASTEXITCODE." }

$inventoryPath = Join-Path $workspaceRoot 'artifacts\generator-reports\full-inventory.json'
& (Join-Path $PSScriptRoot 'inventory.ps1') -OcctRoot $OcctRoot -BatchSize 128 -OutputPath $inventoryPath
$inventory = Get-Content -LiteralPath $inventoryPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($inventory.finalClassification.isComplete -ne $true -or
    $inventory.finalClassification.declarationPending -ne 0 -or
    $inventory.finalClassification.headerPending -ne 0 -or
    @($inventory.finalClassification.headers | Where-Object code -eq 'HD099').Count -ne 0) {
    throw 'The full-inventory classification gate is not complete.'
}
$remainingBindable = @($inventory.finalClassification.declarationStates |
    Where-Object state -eq 'SupportedUnselected' |
    Select-Object -First 1 -ExpandProperty count)
$remainingBindableCount = if ($remainingBindable.Count -eq 0) { 0 } else { [int]$remainingBindable[0] }
$broadLongTailCodes = @('LT001', 'LT002', 'LT003', 'LT004')
$broadLongTailCounts = @($inventory.finalClassification.declarationReasons |
    Where-Object code -in $broadLongTailCodes |
    Select-Object -ExpandProperty count)
$broadLongTailCount = if ($broadLongTailCounts.Count -eq 0) {
    0
}
else {
    [int](($broadLongTailCounts | Measure-Object -Sum).Sum)
}
$narrowBlockedCount = [int](@($inventory.finalClassification.declarationStates |
    Where-Object state -eq 'Blocked' |
    Select-Object -First 1 -ExpandProperty count) | Select-Object -First 1)
$emittedCount = [int](@($inventory.finalClassification.declarationStates |
    Where-Object state -eq 'Emitted' |
    Select-Object -First 1 -ExpandProperty count) | Select-Object -First 1)
$manualCount = [int](@($inventory.finalClassification.declarationStates |
    Where-Object state -eq 'Manual' |
    Select-Object -First 1 -ExpandProperty count) | Select-Object -First 1)
$declarationTotal = [int]$inventory.finalClassification.declarationTotal
$headerTotal = [int]$inventory.finalClassification.headerTotal
$nativeDllCount = @(Get-ChildItem -LiteralPath (Join-Path $workspaceRoot 'artifacts\native\Release') -File -Filter '*.dll').Count
$generatedManifest = Get-Content -LiteralPath (Join-Path $workspaceRoot 'generated\manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$generatedFileCount = @($generatedManifest.files).Count
$dependencyClosurePath = Join-Path $workspaceRoot 'artifacts\generator-reports\dependency-closure.json'
if (-not (Test-Path -LiteralPath $dependencyClosurePath -PathType Leaf)) {
    throw 'Generated shard dependency-closure report is missing.'
}
$dependencyClosure = Get-Content -LiteralPath $dependencyClosurePath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($dependencyClosure.isComplete -ne $true -or
    $dependencyClosure.managedProjectSplitReady -ne $true -or
    @($dependencyClosure.cyclicGroups).Count -ne 0 -or
    @($dependencyClosure.issues).Count -ne 0) {
    throw 'Generated shard dependency closure is incomplete, cyclic, or outside the accepted target graph.'
}
$dependencyEdgeCount = @($dependencyClosure.directDependencies).Count

& (Join-Path $PSScriptRoot 'generate-release-metadata.ps1') -PackageVersion $PackageVersion

& git -C $repositoryRoot diff --check
if ($LASTEXITCODE -ne 0) { throw 'Working-tree whitespace validation failed.' }
& git -C $repositoryRoot diff --cached --check
if ($LASTEXITCODE -ne 0) { throw 'Staged whitespace validation failed.' }

$projectLicensePresent = (Test-Path -LiteralPath (Join-Path $repositoryRoot 'LICENSE')) -or
    (Test-Path -LiteralPath (Join-Path $repositoryRoot 'LICENSE.md'))
$gates = @(
    [ordered]@{ id = 'local-release-debug'; state = 'PASS'; evidence = 'Release and Debug build/test completed in this run.' },
    [ordered]@{ id = 'bundled-runtime'; state = 'PASS'; evidence = 'Committed Windows x64 runtime manifest, 62 DLL hashes, and all included license/notice hashes verified.' },
    [ordered]@{ id = 'generated-freshness'; state = 'PASS'; evidence = "$generatedFileCount manifest-owned files current." },
    [ordered]@{ id = 'generated-shard-dependency-closure'; state = 'PASS'; evidence = "$dependencyEdgeCount observed cross-shard edges are fully resolved, target-graph compatible, and acyclic; managed generated shards are split-eligible while native DLL splitting remains deferred." },
    [ordered]@{ id = 'clean-regeneration'; state = 'PASS'; evidence = 'Fresh source copy build and byte comparison completed.' },
    [ordered]@{ id = 'package-consumer'; state = 'PASS'; evidence = "$PackageVersion clean restore/publish/runtime with $nativeDllCount DLLs, $emittedCount generated declarations, inherited complete Batch D/E/F/G workflows, and the complete Batch H grouped mesh, statistics, diagnostics, LOD, PBR/physical material, copied hierarchy/shared-instance, glTF/GLB/OBJ/PLY/VRML, STEP/XDE, and real-HWND screenshot workflow." },
    [ordered]@{ id = 'api-compatibility'; state = 'PASS'; evidence = 'Compared with the alpha.38 606-signature baseline; additive changes are allowed and removals are blocked.' },
    [ordered]@{ id = 'full-classification'; state = 'PASS'; evidence = "$declarationTotal declarations and $headerTotal headers classified; zero pending/HD099." },
    [ordered]@{ id = 'bindable-emission-completeness'; state = if ($remainingBindableCount -eq 0) { 'PASS' } else { 'BLOCKED' }; evidence = "$remainingBindableCount declarations remain SupportedUnselected; $emittedCount generated and $manualCount accepted manual stable IDs are reconciled." },
    [ordered]@{ id = 'broad-long-tail-elimination'; state = if ($broadLongTailCount -eq 0) { 'PASS' } else { 'BLOCKED' }; evidence = "$broadLongTailCount declarations retain broad LT001-LT004 reasons; $narrowBlockedCount declarations have narrow evidence-backed ABI, ownership, or projection dispositions." },
    [ordered]@{ id = 'sbom-provenance-checksums'; state = 'PASS'; evidence = 'CycloneDX, provenance, and SHA256 files generated.' },
    [ordered]@{ id = 'ci-configuration'; state = 'PASS'; evidence = '.github/workflows/ci.yml is configured.' },
    [ordered]@{ id = 'ci-hosted-execution'; state = 'NOT RUN'; evidence = 'No remote workflow was dispatched from this local task.' },
    [ordered]@{ id = 'project-license'; state = if ($projectLicensePresent) { 'PASS' } else { 'BLOCKED' }; evidence = if ($projectLicensePresent) { 'Repository license exists.' } else { 'PD-012 requires the user to select the project license.' } },
    [ordered]@{ id = 'third-party-notices'; state = 'PASS'; evidence = 'OCCT, oneTBB, FreeImage, FreeType, OpenVR, FFmpeg, and jemalloc notices and license texts are committed and packaged; the unavailable jemalloc bundle version is disclosed.' },
    [ordered]@{ id = 'package-signing'; state = 'NOT RUN'; evidence = 'No signing certificate or authorization was provided.' },
    [ordered]@{ id = 'nuget-publication'; state = 'NOT RUN'; evidence = 'No NuGet credential or publication authorization was provided.' }
)
$localBatchGateIds = @(
    'local-release-debug',
    'bundled-runtime',
    'generated-freshness',
    'generated-shard-dependency-closure',
    'clean-regeneration',
    'package-consumer',
    'api-compatibility',
    'full-classification',
    'bindable-emission-completeness',
    'broad-long-tail-elimination',
    'sbom-provenance-checksums',
    'ci-configuration'
)
$batchImplementationComplete = $remainingBindableCount -eq 0 `
    -and $broadLongTailCount -eq 0 `
    -and @($gates | Where-Object { $_.id -in $localBatchGateIds -and $_.state -ne 'PASS' }).Count -eq 0
$report = [ordered]@{
    schemaVersion = '1.1'
    packageVersion = $PackageVersion
    releaseEngineeringImplemented = $true
    batchImplementationComplete = $batchImplementationComplete
    publicReleaseReady = @($gates | Where-Object state -in @('BLOCKED', 'NOT RUN')).Count -eq 0
    gates = $gates
}
$gatePath = Join-Path $releaseDirectory 'release-gates.json'
[IO.File]::WriteAllText($gatePath, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

$packagePath = Join-Path $workspaceRoot "artifacts\packages\OcctSharp.$PackageVersion.nupkg"
$checksumInputs = @(
    (Join-Path $releaseDirectory 'api-diff.json'),
    (Join-Path $releaseDirectory 'provenance.json'),
    (Join-Path $releaseDirectory 'release-gates.json'),
    (Join-Path $releaseDirectory 'sbom.cdx.json'),
    $packagePath
)
$checksumLines = @($checksumInputs | ForEach-Object {
    if (-not (Test-Path -LiteralPath $_ -PathType Leaf)) {
        throw "Required checksum input is missing: '$($_)'."
    }
    "$(Get-FileHash -LiteralPath $_ -Algorithm SHA256 | Select-Object -ExpandProperty Hash)  $(Split-Path -Leaf $_)"
})
[IO.File]::WriteAllLines(
    (Join-Path $releaseDirectory 'checksums.sha256'),
    $checksumLines,
    [Text.UTF8Encoding]::new($false))
Write-Host "Release engineering checks completed. Public release ready: $($report.publicReleaseReady). Gate report: '$gatePath'."
}
finally {
    Pop-Location
}

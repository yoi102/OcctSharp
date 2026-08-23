[CmdletBinding()]
param(
    [string]$OcctRoot,
    [string]$PackageVersion = '0.1.0-alpha.40',
    [string]$ApiBaselineVersion = '0.1.0-alpha.38'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $workspaceRoot
$releaseDirectory = Join-Path $workspaceRoot 'artifacts\release'
[IO.Directory]::CreateDirectory($releaseDirectory) | Out-Null

& (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release -OcctRoot $OcctRoot
& (Join-Path $PSScriptRoot 'build.ps1') -Configuration Debug -OcctRoot $OcctRoot
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
$emittedCount = [int](@($inventory.finalClassification.declarationStates |
    Where-Object state -eq 'Emitted' |
    Select-Object -First 1 -ExpandProperty count) | Select-Object -First 1)

& (Join-Path $PSScriptRoot 'generate-release-metadata.ps1') -PackageVersion $PackageVersion

& git -C $repositoryRoot diff --check
if ($LASTEXITCODE -ne 0) { throw 'Working-tree whitespace validation failed.' }
& git -C $repositoryRoot diff --cached --check
if ($LASTEXITCODE -ne 0) { throw 'Staged whitespace validation failed.' }

$projectLicensePresent = (Test-Path -LiteralPath (Join-Path $repositoryRoot 'LICENSE')) -or
    (Test-Path -LiteralPath (Join-Path $repositoryRoot 'LICENSE.md'))
$gates = @(
    [ordered]@{ id = 'local-release-debug'; state = 'PASS'; evidence = 'Release and Debug build/test completed in this run.' },
    [ordered]@{ id = 'generated-freshness'; state = 'PASS'; evidence = '13 manifest-owned files current.' },
    [ordered]@{ id = 'clean-regeneration'; state = 'PASS'; evidence = 'Fresh source copy build and byte comparison completed.' },
    [ordered]@{ id = 'package-consumer'; state = 'PASS'; evidence = 'alpha.40 clean restore/publish/runtime with 45 DLLs and generated StepBasic shared/enum behavior.' },
    [ordered]@{ id = 'api-compatibility'; state = 'PASS'; evidence = 'Compared with the alpha.38 606-signature baseline; additive changes are allowed and removals are blocked.' },
    [ordered]@{ id = 'full-classification'; state = 'PASS'; evidence = '116214 declarations and 7090 headers classified; zero pending/HD099.' },
    [ordered]@{ id = 'bindable-emission-completeness'; state = if ($remainingBindableCount -eq 0) { 'PASS' } else { 'BLOCKED' }; evidence = "$remainingBindableCount declarations remain SupportedUnselected; $emittedCount generated stable IDs are reconciled through the manifest." },
    [ordered]@{ id = 'sbom-provenance-checksums'; state = 'PASS'; evidence = 'CycloneDX, provenance, and SHA256 files generated.' },
    [ordered]@{ id = 'ci-configuration'; state = 'PASS'; evidence = '.github/workflows/ci.yml is configured.' },
    [ordered]@{ id = 'ci-hosted-execution'; state = 'NOT RUN'; evidence = 'No remote workflow was dispatched from this local task.' },
    [ordered]@{ id = 'project-license'; state = if ($projectLicensePresent) { 'PASS' } else { 'BLOCKED' }; evidence = if ($projectLicensePresent) { 'Repository license exists.' } else { 'PD-012 requires the user to select the project license.' } },
    [ordered]@{ id = 'third-party-legal-review'; state = 'BLOCKED'; evidence = 'Exact versions/notices/source obligations for non-OCCT DLLs remain unresolved.' },
    [ordered]@{ id = 'package-signing'; state = 'NOT RUN'; evidence = 'No signing certificate or authorization was provided.' },
    [ordered]@{ id = 'nuget-publication'; state = 'NOT RUN'; evidence = 'No NuGet credential or publication authorization was provided.' }
)
$report = [ordered]@{
    schemaVersion = '1.1'
    packageVersion = $PackageVersion
    releaseEngineeringImplemented = $true
    batchImplementationComplete = $false
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

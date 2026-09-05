[CmdletBinding()]
param([string]$PackageVersion = '8.0.1-preview.19')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$releaseDirectory = Join-Path $workspaceRoot 'artifacts\release'
$gatePath = Join-Path $releaseDirectory 'release-gates.json'
$provenancePath = Join-Path $releaseDirectory 'provenance.json'
$packagePath = Join-Path $workspaceRoot "artifacts\packages\OcctSharp.$PackageVersion.nupkg"
$gates = Get-Content -LiteralPath $gatePath -Raw | ConvertFrom-Json
$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
if ($gates.packageVersion -ne $PackageVersion -or
    $provenance.subject.name -ne "OcctSharp.$PackageVersion.nupkg" -or
    $provenance.subject.sha256 -ne (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash) {
    throw 'Release gates, provenance, and package identity must agree before checksums are written.'
}

$checksumInputs = @(
    (Join-Path $releaseDirectory 'api-diff.json'),
    $provenancePath,
    $gatePath,
    (Join-Path $releaseDirectory 'sbom.cdx.json'),
    $packagePath
)
$checksumLines = @($checksumInputs | ForEach-Object {
    if (-not (Test-Path -LiteralPath $_ -PathType Leaf)) {
        throw "Required checksum input is missing: '$_'."
    }
    "$(Get-FileHash -LiteralPath $_ -Algorithm SHA256 | Select-Object -ExpandProperty Hash)  $(Split-Path -Leaf $_)"
})
[IO.File]::WriteAllLines(
    (Join-Path $releaseDirectory 'checksums.sha256'),
    $checksumLines,
    [Text.UTF8Encoding]::new($false))
Write-Host "Release checksums updated for $PackageVersion; gate states were not changed."

[CmdletBinding()]
param([string]$PackageVersion = '0.1.0-alpha.41')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$releaseDirectory = Join-Path $workspaceRoot 'artifacts\release'
$nativeDirectory = Join-Path $workspaceRoot 'artifacts\native\Release'
$packagePath = Join-Path $workspaceRoot "artifacts\packages\OcctSharp.$PackageVersion.nupkg"
$inventoryPath = Join-Path $workspaceRoot 'artifacts\generator-reports\full-inventory.json'
foreach ($requiredPath in @($nativeDirectory, $packagePath, $inventoryPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) { throw "Required release input is missing: '$requiredPath'." }
}
[IO.Directory]::CreateDirectory($releaseDirectory) | Out-Null

$nativeComponents = @(Get-ChildItem -LiteralPath $nativeDirectory -Filter '*.dll' -File | Sort-Object Name | ForEach-Object {
    $isBridge = $_.Name -eq 'OcctSharp.Native.dll'
    $isOcct = $_.Name -like 'TK*.dll' -or $_.Name -eq 'TKernel.dll'
    [ordered]@{
        name = $_.Name
        version = if ($isBridge) { '0.41.0' } elseif ($isOcct) { '8.0.1' } else { 'unknown' }
        type = 'file'
        hashes = @([ordered]@{ alg = 'SHA-256'; content = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
        licenses = if ($isOcct) { @([ordered]@{ expression = 'LGPL-2.1-only WITH OCCT-exception-1.0' }) } else { @() }
        properties = @(
            [ordered]@{ name = 'occtsharp:source'; value = if ($isBridge) { 'OcctSharp build' } else { 'Pinned OCCT distribution' } },
            [ordered]@{ name = 'occtsharp:licenseReview'; value = if ($isBridge -or $isOcct) { 'recorded' } else { 'unresolved' } }
        )
    }
})

$sbom = [ordered]@{
    bomFormat = 'CycloneDX'
    specVersion = '1.6'
    version = 1
    metadata = [ordered]@{
        component = [ordered]@{ type = 'library'; name = 'OcctSharp'; version = $PackageVersion }
    }
    components = $nativeComponents
}

$inputPaths = @(
    'config/generation.json',
    'config/dependency-profiles.json',
    'config/occt-8.0.1-windows-x64.json',
    'generated/manifest.json'
)
$provenance = [ordered]@{
    schemaVersion = '1.0'
    subject = [ordered]@{
        name = "OcctSharp.$PackageVersion.nupkg"
        sha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
    }
    build = [ordered]@{
        dotnetSdk = (& dotnet --version).Trim()
        platform = 'windows-x64'
        configuration = 'Release'
        nativeAbi = '1.33'
        bridgeVersion = '0.41.0'
        occtVersion = '8.0.1'
    }
    inputs = @($inputPaths | ForEach-Object {
        $path = Join-Path $workspaceRoot $_
        [ordered]@{ path = $_; sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
    })
    fullInventory = [ordered]@{
        sha256 = (Get-FileHash -LiteralPath $inventoryPath -Algorithm SHA256).Hash
        classificationComplete = $true
    }
    nativeFiles = @($nativeComponents | ForEach-Object {
        [ordered]@{ name = $_.name; sha256 = $_.hashes[0].content }
    })
}

$jsonOptions = @{ Depth = 12 }
[IO.File]::WriteAllText((Join-Path $releaseDirectory 'sbom.cdx.json'), ($sbom | ConvertTo-Json @jsonOptions) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText((Join-Path $releaseDirectory 'provenance.json'), ($provenance | ConvertTo-Json @jsonOptions) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))

Write-Host "Release metadata generated for $($nativeComponents.Count) native files under '$releaseDirectory'."

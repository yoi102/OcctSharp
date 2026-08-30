[CmdletBinding()]
param([string]$PackageVersion = '8.0.1-preview.6')

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
    $component = switch -Regex ($_.Name) {
        '^OcctSharp\.Native\.dll$' { @{ version = '0.59.0'; license = 'MIT'; source = 'OcctSharp build' }; break }
        '^(TK.*|TKernel)\.dll$' { @{ version = '8.0.1'; license = 'LGPL-2.1-only WITH OCCT-exception-1.0'; source = 'Pinned OCCT distribution' }; break }
        '^tbb12\.dll$' { @{ version = '2021.13.0'; license = 'Apache-2.0'; source = 'Pinned OCCT third-party bundle' }; break }
        '^FreeImage\.dll$' { @{ version = '3.18.0'; license = 'LicenseRef-FreeImage'; source = 'Pinned OCCT third-party bundle' }; break }
        '^freetype\.dll$' { @{ version = '2.13.3'; license = 'FTL'; source = 'Pinned OCCT third-party bundle' }; break }
        '^openvr_api\.dll$' { @{ version = '1.14.15'; license = 'BSD-3-Clause'; source = 'Pinned OCCT third-party bundle' }; break }
        '^av(codec-57|format-57|util-55)\.dll$' { @{ version = '3.3.4'; license = 'LGPL-2.1-or-later'; source = 'Pinned OCCT third-party bundle' }; break }
        '^swscale-4\.dll$' { @{ version = '3.3.4'; license = 'LGPL-2.1-or-later'; source = 'Pinned OCCT third-party bundle' }; break }
        '^jemalloc\.dll$' { @{ version = 'unknown-bundle-build'; license = 'LicenseRef-jemalloc-BSD-style'; source = 'Pinned OCCT third-party bundle' }; break }
        default { throw "Unclassified native runtime component '$($_.Name)'." }
    }
    [ordered]@{
        name = $_.Name
        version = $component.version
        type = 'file'
        hashes = @([ordered]@{ alg = 'SHA-256'; content = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
        licenses = @([ordered]@{ expression = $component.license })
        properties = @(
            [ordered]@{ name = 'occtsharp:source'; value = $component.source },
            [ordered]@{ name = 'occtsharp:licenseReview'; value = 'recorded' }
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
    'generated/manifest.json',
    'artifacts/generator-reports/dependency-closure.json'
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
        nativeAbi = '1.51'
        bridgeVersion = '0.59.0'
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

[CmdletBinding()]
param(
    [string]$OcctRoot,

    [string]$PackageVersion = '0.1.0-alpha.49',

    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$settingsPath = Join-Path $workspaceRoot 'config\local.settings.json'

if (-not $OcctRoot -and (Test-Path -LiteralPath $settingsPath)) {
    $settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $OcctRoot = $settings.occtRoot
}

if (-not $OcctRoot) {
    throw 'OCCT root is required. Pass -OcctRoot or configure config/local.settings.json.'
}

$resolvedOcctRoot = (Resolve-Path -LiteralPath $OcctRoot).Path
$nativeRuntimeDirectory = Join-Path $workspaceRoot 'artifacts\native\Release'
$packageDirectory = Join-Path $workspaceRoot 'artifacts\packages'

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release -OcctRoot $resolvedOcctRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed with exit code $LASTEXITCODE."
    }
}

$nativeFiles = @(Get-ChildItem -LiteralPath $nativeRuntimeDirectory -File -Filter '*.dll')
if ($nativeFiles.Count -eq 0) {
    throw "No native runtime DLLs were found at '$nativeRuntimeDirectory'."
}

foreach ($licenseName in @('LICENSE_LGPL_21.txt', 'OCCT_LGPL_EXCEPTION.txt')) {
    $licensePath = Join-Path $resolvedOcctRoot $licenseName
    if (-not (Test-Path -LiteralPath $licensePath)) {
        throw "Required OCCT license file was not found: '$licensePath'."
    }
}

New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null
Push-Location $workspaceRoot
try {
    & dotnet pack .\src\OcctSharp\OcctSharp.csproj `
        --configuration Release `
        --no-build `
        --no-restore `
        --output $packageDirectory `
        "-p:PackageVersion=$PackageVersion" `
        "-p:OcctSharpNativeRuntimeDir=$nativeRuntimeDirectory" `
        "-p:OcctSharpOcctRoot=$resolvedOcctRoot"
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

$packagePath = Join-Path $packageDirectory "OcctSharp.$PackageVersion.nupkg"
if (-not (Test-Path -LiteralPath $packagePath)) {
    throw "Expected NuGet package was not created: '$packagePath'."
}

Write-Host "Created '$packagePath' with $($nativeFiles.Count) native runtime DLLs."

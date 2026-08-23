[CmdletBinding()]
param(
    [string]$OcctRoot,

    [ValidateRange(1, 512)]
    [int]$BatchSize = 64,

    [string]$OutputPath,

    [switch]$CatalogOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$settingsPath = Join-Path $workspaceRoot 'config\local.settings.json'
$settings = if (Test-Path -LiteralPath $settingsPath) {
    Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
} else {
    $null
}

if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = [Environment]::GetEnvironmentVariable('OCCTSHARP_OCCT_ROOT')
}

if ([string]::IsNullOrWhiteSpace($OcctRoot) -and $null -ne $settings) {
    $OcctRoot = $settings.occtRoot
}

if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw 'Set OCCTSHARP_OCCT_ROOT, pass -OcctRoot, or create config/local.settings.json.'
}

$resolvedOcctRoot = (Resolve-Path -LiteralPath $OcctRoot).Path
if (-not (Test-Path -LiteralPath (Join-Path $resolvedOcctRoot 'inc\Standard_Version.hxx'))) {
    throw "The OCCT root '$resolvedOcctRoot' does not contain inc/Standard_Version.hxx."
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $fileName = if ($CatalogOnly) { 'header-inventory.json' } else { 'full-inventory.json' }
    $OutputPath = Join-Path $workspaceRoot "artifacts\generator-reports\$fileName"
}

$resolvedOutputPath = [IO.Path]::GetFullPath($OutputPath, $workspaceRoot)

$visualStudioRoot = if ($null -ne $settings) { $settings.visualStudioRoot } else { $null }
if ([string]::IsNullOrWhiteSpace($visualStudioRoot)) {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw 'Visual Studio Installer vswhere.exe was not found.'
    }

    $visualStudioRoot = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
}

$vcvars = Join-Path $visualStudioRoot 'VC\Auxiliary\Build\vcvars64.bat'
if (-not (Test-Path -LiteralPath $vcvars)) {
    throw "Visual Studio x64 developer environment was not found at '$vcvars'."
}

$includeCommand = 'call "' + $vcvars + '" >nul && set INCLUDE'
$includeLine = & cmd.exe /d /c $includeCommand
if ($LASTEXITCODE -ne 0 -or $includeLine -notmatch '^INCLUDE=') {
    throw 'Unable to resolve the Visual Studio C++ include environment.'
}

$env:INCLUDE = $includeLine -replace '^INCLUDE=', ''

Push-Location $workspaceRoot
try {
    & dotnet restore .\src\OcctSharp.Generator\OcctSharp.Generator.csproj
    if ($LASTEXITCODE -ne 0) { throw "Generator restore failed with exit code $LASTEXITCODE." }

    & dotnet build .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-restore --configuration Release
    if ($LASTEXITCODE -ne 0) { throw "Generator build failed with exit code $LASTEXITCODE." }

    if ($CatalogOnly) {
        & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration Release -- inventory-catalog --occt-root $resolvedOcctRoot --output $resolvedOutputPath
    } else {
        & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration Release -- inventory --occt-root $resolvedOcctRoot --config .\config\generation.json --output $resolvedOutputPath --batch-size $BatchSize
    }

    if ($LASTEXITCODE -ne 0) {
        throw "OCCT inventory failed or was incomplete with exit code $LASTEXITCODE. Inspect '$resolvedOutputPath' when present."
    }
}
finally {
    Pop-Location
}

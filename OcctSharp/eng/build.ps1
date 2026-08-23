[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$OcctRoot,

    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$settingsPath = Join-Path $workspaceRoot 'config\local.settings.json'

if (Test-Path -LiteralPath $settingsPath) {
    $settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
} else {
    $settings = $null
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
$occtConfig = Join-Path $resolvedOcctRoot 'cmake\OpenCASCADEConfig.cmake'
if (-not (Test-Path -LiteralPath $occtConfig)) {
    throw "OpenCASCADEConfig.cmake was not found at '$occtConfig'."
}

$dependencyManifestPath = Join-Path $workspaceRoot 'config\occt-8.0.1-windows-x64.json'
$dependencyManifest = Get-Content -LiteralPath $dependencyManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($relativePath in $dependencyManifest.requiredRelativePaths) {
    $requiredPath = Join-Path $resolvedOcctRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "The OCCT baseline is missing required path '$relativePath'."
    }
}

foreach ($hashEntry in $dependencyManifest.verificationHashes.PSObject.Properties) {
    $hashPath = Join-Path $resolvedOcctRoot ($hashEntry.Name -replace '/', [IO.Path]::DirectorySeparatorChar)
    $actualHash = (Get-FileHash -LiteralPath $hashPath -Algorithm SHA256).Hash
    if ($actualHash -ne $hashEntry.Value) {
        throw "The OCCT baseline hash for '$($hashEntry.Name)' is '$actualHash', expected '$($hashEntry.Value)'."
    }
}

$visualStudioRoot = if ($null -ne $settings) { $settings.visualStudioRoot } else { $null }
if ([string]::IsNullOrWhiteSpace($visualStudioRoot)) {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw 'Visual Studio Installer vswhere.exe was not found.'
    }

    $visualStudioRoot = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
}

$cmake = Join-Path $visualStudioRoot 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
if (-not (Test-Path -LiteralPath $cmake)) {
    throw "Visual Studio CMake was not found at '$cmake'."
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

$env:OCCTSHARP_OCCT_ROOT = $resolvedOcctRoot

Push-Location $workspaceRoot
try {
    $sdkVersion = (& dotnet --version).Trim()
    if (-not $sdkVersion.StartsWith('10.0.', [StringComparison]::Ordinal)) {
        throw "Expected the .NET 10 SDK selected by global.json, but dotnet reported '$sdkVersion'."
    }

    & dotnet restore .\OcctSharp.slnx
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE." }

    & dotnet build .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-restore --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Generator bootstrap build failed with exit code $LASTEXITCODE." }

    & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration $Configuration -- generate --occt-root $resolvedOcctRoot --config .\config\generation.json --output-root .
    if ($LASTEXITCODE -ne 0) { throw "Binding generation failed with exit code $LASTEXITCODE." }

    $coverageReport = Join-Path $workspaceRoot 'artifacts\generator-reports\coverage.json'
    $diagnosticsReport = Join-Path $workspaceRoot 'artifacts\generator-reports\diagnostics.json'
    if (-not (Test-Path -LiteralPath $coverageReport) -or -not (Test-Path -LiteralPath $diagnosticsReport)) {
        throw 'Binding generation did not produce the required coverage and diagnostics reports.'
    }

    $firstCoverageHash = (Get-FileHash -LiteralPath $coverageReport -Algorithm SHA256).Hash
    $firstDiagnosticsHash = (Get-FileHash -LiteralPath $diagnosticsReport -Algorithm SHA256).Hash

    & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration $Configuration -- generate --occt-root $resolvedOcctRoot --config .\config\generation.json --output-root .
    if ($LASTEXITCODE -ne 0) { throw "Second binding generation failed with exit code $LASTEXITCODE." }

    $secondCoverageHash = (Get-FileHash -LiteralPath $coverageReport -Algorithm SHA256).Hash
    $secondDiagnosticsHash = (Get-FileHash -LiteralPath $diagnosticsReport -Algorithm SHA256).Hash
    if ($firstCoverageHash -ne $secondCoverageHash -or $firstDiagnosticsHash -ne $secondDiagnosticsHash) {
        throw 'Generation coverage or diagnostics reports are not deterministic.'
    }

    Write-Host "Configuring native bridge with OCCT at '$resolvedOcctRoot'."
    & $cmake --preset windows-x64-local
    if ($LASTEXITCODE -ne 0) { throw "CMake configure failed with exit code $LASTEXITCODE." }

    $buildPreset = $Configuration.ToLowerInvariant()
    & $cmake --build --preset $buildPreset
    if ($LASTEXITCODE -ne 0) { throw "CMake build failed with exit code $LASTEXITCODE." }

    & dotnet build .\OcctSharp.slnx --no-restore --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE." }

    $determinismDirectory = Join-Path $workspaceRoot 'artifacts\generator-determinism'
    New-Item -ItemType Directory -Path $determinismDirectory -Force | Out-Null
    $firstReport = Join-Path $determinismDirectory 'model-smoke-1.json'
    $secondReport = Join-Path $determinismDirectory 'model-smoke-2.json'

    & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration $Configuration -- model-smoke $firstReport
    if ($LASTEXITCODE -ne 0) { throw "First generator smoke run failed with exit code $LASTEXITCODE." }

    & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration $Configuration -- model-smoke $secondReport
    if ($LASTEXITCODE -ne 0) { throw "Second generator smoke run failed with exit code $LASTEXITCODE." }

    $firstHash = (Get-FileHash -LiteralPath $firstReport -Algorithm SHA256).Hash
    $secondHash = (Get-FileHash -LiteralPath $secondReport -Algorithm SHA256).Hash
    if ($firstHash -ne $secondHash) {
        throw 'Generator determinism smoke reports differ.'
    }

    $firstDiscoveryReport = Join-Path $determinismDirectory 'gp-pnt-discovery-1.json'
    $secondDiscoveryReport = Join-Path $determinismDirectory 'gp-pnt-discovery-2.json'

    & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration $Configuration -- discover --occt-root $resolvedOcctRoot --config .\config\generation.json --output $firstDiscoveryReport
    if ($LASTEXITCODE -ne 0) { throw "First OCCT AST discovery run failed with exit code $LASTEXITCODE." }

    & dotnet run --project .\src\OcctSharp.Generator\OcctSharp.Generator.csproj --no-build --configuration $Configuration -- discover --occt-root $resolvedOcctRoot --config .\config\generation.json --output $secondDiscoveryReport
    if ($LASTEXITCODE -ne 0) { throw "Second OCCT AST discovery run failed with exit code $LASTEXITCODE." }

    $firstDiscoveryHash = (Get-FileHash -LiteralPath $firstDiscoveryReport -Algorithm SHA256).Hash
    $secondDiscoveryHash = (Get-FileHash -LiteralPath $secondDiscoveryReport -Algorithm SHA256).Hash
    if ($firstDiscoveryHash -ne $secondDiscoveryHash) {
        throw 'OCCT AST discovery reports are not deterministic.'
    }

    if (-not $SkipTests) {
        & dotnet test .\OcctSharp.slnx --no-build --configuration $Configuration
        if ($LASTEXITCODE -ne 0) { throw "dotnet test failed with exit code $LASTEXITCODE." }
    }

    & .\eng\audit-dependency-profiles.ps1 -OcctRoot $resolvedOcctRoot

    Write-Host "Build completed with .NET SDK $sdkVersion. Model SHA256: $firstHash. OCCT discovery SHA256: $firstDiscoveryHash. Coverage SHA256: $firstCoverageHash. Diagnostics SHA256: $firstDiagnosticsHash"
}
finally {
    Pop-Location
}

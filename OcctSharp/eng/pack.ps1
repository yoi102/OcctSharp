[CmdletBinding()]
param(
    [string]$OcctRoot,

    [string]$PackageVersion = '8.0.1-preview.15',

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

$resolvedOcctRoot = if ($OcctRoot) { (Resolve-Path -LiteralPath $OcctRoot).Path } else { $null }
$builtRuntimeDirectory = Join-Path $workspaceRoot 'artifacts\native\Release'
$bundledRuntimeDirectory = Join-Path $workspaceRoot 'runtime\win-x64\occt'
$nativeRuntimeDirectory = if (Test-Path -LiteralPath (Join-Path $builtRuntimeDirectory 'OcctSharp.Native.dll')) {
    $builtRuntimeDirectory
}
else {
    $bundledRuntimeDirectory
}
$packageDirectory = Join-Path $workspaceRoot 'artifacts\packages'

if (-not $SkipBuild) {
    if (-not $resolvedOcctRoot) {
        throw 'A native rebuild requires an OCCT root. Pass -OcctRoot or configure config/local.settings.json; use -SkipBuild to package the committed runtime.'
    }
    & (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release -OcctRoot $resolvedOcctRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed with exit code $LASTEXITCODE."
    }
    $nativeRuntimeDirectory = $builtRuntimeDirectory
}

& (Join-Path $PSScriptRoot 'verify-bundled-runtime.ps1')

$nativeFiles = @(Get-ChildItem -LiteralPath $nativeRuntimeDirectory -File -Filter '*.dll')
if ($nativeFiles.Count -eq 0) {
    throw "No native runtime DLLs were found at '$nativeRuntimeDirectory'."
}

New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null
Push-Location $workspaceRoot
try {
    $packProjects = @(
        '.\src\OcctSharp.Native.win-x64\OcctSharp.Native.win-x64.csproj',
        '.\src\OcctSharp.Runtime\OcctSharp.Runtime.csproj',
        '.\src\OcctSharp.Foundation\OcctSharp.Foundation.csproj',
        '.\src\OcctSharp.Geometry\OcctSharp.Geometry.csproj',
        '.\src\OcctSharp.MeshData\OcctSharp.MeshData.csproj',
        '.\src\OcctSharp.Modeling\OcctSharp.Modeling.csproj',
        '.\src\OcctSharp.Mesh\OcctSharp.Mesh.csproj',
        '.\src\OcctSharp.Documents\OcctSharp.Documents.csproj',
        '.\src\OcctSharp.Visualization\OcctSharp.Visualization.csproj',
        '.\src\OcctSharp.DataExchange\OcctSharp.DataExchange.csproj',
        '.\src\OcctSharp.Xde\OcctSharp.Xde.csproj',
        '.\src\OcctSharp.IVtk\OcctSharp.IVtk.csproj',
        '.\src\OcctSharp.Draw\OcctSharp.Draw.csproj',
        '.\src\OcctSharp\OcctSharp.csproj'
    )

    foreach ($packProject in $packProjects) {
        $packArguments = @(
            'pack',
            $packProject,
            '--configuration', 'Release',
            '--output', $packageDirectory,
            "-p:PackageVersion=$PackageVersion",
            "-p:OcctSharpPackageVersion=$PackageVersion",
            "-p:OcctSharpNativeRuntimeDir=$nativeRuntimeDirectory"
        )
        if (-not $SkipBuild) {
            $packArguments += @('--no-build', '--no-restore')
        }
        & dotnet @packArguments
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet pack failed for '$packProject' with exit code $LASTEXITCODE."
        }
    }
}
finally {
    Pop-Location
}

$packageIds = @(
    'OcctSharp.Native.win-x64', 'OcctSharp.Runtime', 'OcctSharp.Foundation',
    'OcctSharp.Geometry', 'OcctSharp.MeshData', 'OcctSharp.Modeling',
    'OcctSharp.Mesh', 'OcctSharp.Documents', 'OcctSharp.Visualization',
    'OcctSharp.DataExchange', 'OcctSharp.Xde', 'OcctSharp.IVtk',
    'OcctSharp.Draw', 'OcctSharp'
)
foreach ($packageId in $packageIds) {
    $packagePath = Join-Path $packageDirectory "$packageId.$PackageVersion.nupkg"
    if (-not (Test-Path -LiteralPath $packagePath)) {
        throw "Expected NuGet package was not created: '$packagePath'."
    }
}

Write-Host "Created $($packageIds.Count) packages; the shared native package contains $($nativeFiles.Count) runtime DLLs."

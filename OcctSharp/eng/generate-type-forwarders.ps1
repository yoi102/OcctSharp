[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$Check,

    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$moduleNames = @(
    'Runtime', 'Foundation', 'Geometry', 'MeshData', 'Modeling', 'Mesh',
    'Documents', 'Visualization', 'DataExchange', 'Xde', 'IVtk', 'Draw'
)
$forwarderPath = Join-Path $workspaceRoot 'src\OcctSharp\Compatibility\TypeForwarders.Generated.cs'
$checkPath = Join-Path $workspaceRoot 'artifacts\generator-reports\type-forwarders.check.cs'
$outputPath = if ($Check) { $checkPath } else { $forwarderPath }

Push-Location -LiteralPath $workspaceRoot
try {
    if (-not $SkipBuild) {
        & dotnet build .\OcctSharp.slnx --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "Managed solution build failed with exit code $LASTEXITCODE."
        }
    }

    $assemblyPaths = @($moduleNames | ForEach-Object {
        Join-Path $workspaceRoot "src\OcctSharp.$_\bin\$Configuration\net10.0\win-x64\OcctSharp.$_.dll"
    })
    $missingAssemblies = @($assemblyPaths | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
    if ($missingAssemblies.Count -ne 0) {
        throw "Managed module assemblies are missing: $($missingAssemblies -join ', ')."
    }

    & dotnet run --project .\tools\OcctSharp.ApiTool\OcctSharp.ApiTool.csproj `
        --configuration $Configuration --no-build -- forwarders $outputPath @assemblyPaths
    if ($LASTEXITCODE -ne 0) {
        throw "Type-forwarder generation failed with exit code $LASTEXITCODE."
    }

    if ($Check) {
        $expectedHash = (Get-FileHash -LiteralPath $forwarderPath -Algorithm SHA256).Hash
        $actualHash = (Get-FileHash -LiteralPath $checkPath -Algorithm SHA256).Hash
        if ($expectedHash -ne $actualHash) {
            throw 'The checked-in facade type-forwarder source is stale. Run eng\generate-type-forwarders.ps1.'
        }
        Write-Host "Facade type forwarders are current: $expectedHash."
    }
}
finally {
    Pop-Location
}

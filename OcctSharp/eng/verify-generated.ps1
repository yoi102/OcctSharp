[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $workspaceRoot 'generated\manifest.json'

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Generated manifest was not found at '$manifestPath'."
}

$beforeManifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$beforePaths = @($beforeManifest.files | ForEach-Object { $_.relativePath })
$beforePaths += 'generated/manifest.json'
$beforeHashes = @{}
foreach ($relativePath in $beforePaths) {
    $path = Join-Path $workspaceRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Generated output '$relativePath' listed before regeneration does not exist."
    }
    $beforeHashes[$relativePath] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
}

& (Join-Path $PSScriptRoot 'build.ps1') -Configuration $Configuration -SkipTests
if ($LASTEXITCODE -ne 0) {
    throw "Generated-source build failed with exit code $LASTEXITCODE."
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$generatedPaths = @($manifest.files | ForEach-Object { $_.relativePath })
$generatedPaths += 'generated/manifest.json'

$pathChanges = @(Compare-Object $beforePaths $generatedPaths)
if ($pathChanges.Count -ne 0) {
    throw 'Regeneration changed the generated manifest path set.'
}

Push-Location (Split-Path -Parent $workspaceRoot)
try {
    foreach ($relativePath in $generatedPaths) {
        $repositoryPath = 'OcctSharp/' + $relativePath
        & git ls-files --error-unmatch -- $repositoryPath | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Generated output '$repositoryPath' is not tracked or staged by Git."
        }

        $path = Join-Path $workspaceRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
        $afterHash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        if ($afterHash -ne $beforeHashes[$relativePath]) {
            throw "Regeneration changed generated output '$relativePath'. Regenerate and review the diff."
        }
    }
}
finally {
    Pop-Location
}

Write-Host "Generated output is current for $($manifest.occtVersion); $($manifest.files.Count) files verified."

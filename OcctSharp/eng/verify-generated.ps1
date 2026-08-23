[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $workspaceRoot 'generated\manifest.json'

& (Join-Path $PSScriptRoot 'build.ps1') -Configuration $Configuration -SkipTests
if ($LASTEXITCODE -ne 0) {
    throw "Generated-source build failed with exit code $LASTEXITCODE."
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$generatedPaths = @($manifest.files | ForEach-Object { $_.relativePath })
$generatedPaths += 'generated/manifest.json'

Push-Location (Split-Path -Parent $workspaceRoot)
try {
    foreach ($relativePath in $generatedPaths) {
        $repositoryPath = 'OcctSharp/' + $relativePath
        & git ls-files --error-unmatch -- $repositoryPath | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Generated output '$repositoryPath' is not tracked or staged by Git."
        }
    }

    & git diff --exit-code -- @($generatedPaths | ForEach-Object { 'OcctSharp/' + $_ })
    if ($LASTEXITCODE -ne 0) {
        throw 'Regeneration changed generated output. Regenerate and review the diff.'
    }
}
finally {
    Pop-Location
}

Write-Host "Generated output is current for $($manifest.occtVersion); $($manifest.files.Count) files verified."

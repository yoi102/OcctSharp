[CmdletBinding()]
param([switch]$CompareBuiltRuntime)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$runtimeRoot = Join-Path $workspaceRoot 'runtime\win-x64'
$manifestPath = Join-Path $runtimeRoot 'runtime-manifest.json'

if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
    throw "Bundled runtime manifest was not found: '$manifestPath'."
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($manifest.schemaVersion -ne '1.0' -or
    $manifest.platform -ne 'windows' -or
    $manifest.architecture -ne 'x64' -or
    $manifest.occtVersion -ne '8.0.1' -or
    $manifest.nativeAbi -ne '1.57' -or
    $manifest.bridgeVersion -ne '0.65.0') {
    throw 'Bundled runtime manifest identity is not the accepted Windows x64 OCCT 8.0.1 / ABI 1.57 / bridge 0.65.0 baseline.'
}

$expectedPaths = @($manifest.files | ForEach-Object path)
$actualPaths = @(Get-ChildItem -LiteralPath $runtimeRoot -Recurse -File |
    Where-Object Name -ne 'runtime-manifest.json' |
    ForEach-Object { [IO.Path]::GetRelativePath($runtimeRoot, $_.FullName).Replace('\', '/') } |
    Sort-Object)
$expectedSorted = @($expectedPaths | Sort-Object)
if (($actualPaths -join "`n") -ne ($expectedSorted -join "`n")) {
    throw 'Bundled runtime file set does not match runtime-manifest.json.'
}

foreach ($entry in $manifest.files) {
    $path = Join-Path $runtimeRoot ($entry.path.Replace('/', '\'))
    $file = Get-Item -LiteralPath $path
    if ($file.Length -ne [long]$entry.size) {
        throw "Bundled runtime size mismatch for '$($entry.path)'."
    }
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if ($hash -ne $entry.sha256) {
        throw "Bundled runtime SHA256 mismatch for '$($entry.path)'."
    }
}

$dllCount = @($manifest.files | Where-Object { $_.path -like 'occt/*.dll' }).Count
if ($dllCount -ne 62) {
    throw "Expected 62 bundled DLLs, found $dllCount in the manifest."
}

if ($CompareBuiltRuntime) {
    $builtRuntime = Join-Path $workspaceRoot 'artifacts\native\Release'
    $builtDlls = @(Get-ChildItem -LiteralPath $builtRuntime -File -Filter '*.dll' | Sort-Object Name)
    $bundledDlls = @(Get-ChildItem -LiteralPath (Join-Path $runtimeRoot 'occt') -File -Filter '*.dll' | Sort-Object Name)
    if (($builtDlls.Name -join "`n") -ne ($bundledDlls.Name -join "`n")) {
        throw 'Built Release DLL set does not match the committed runtime DLL set.'
    }
    foreach ($builtDll in $builtDlls) {
        $bundledPath = Join-Path (Join-Path $runtimeRoot 'occt') $builtDll.Name
        if ((Get-FileHash -LiteralPath $builtDll.FullName -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath $bundledPath -Algorithm SHA256).Hash) {
            throw "Built Release DLL differs from the committed runtime: '$($builtDll.Name)'."
        }
    }
    Write-Host 'Built Release closure is byte-identical to the committed runtime.'
}

Write-Host "Bundled runtime verified: $dllCount DLLs and $($manifest.files.Count - $dllCount) notice/license files."

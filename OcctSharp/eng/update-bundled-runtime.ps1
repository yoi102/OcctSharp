[CmdletBinding()]
param([ValidatePattern('^8\.0\.1-preview\.[0-9]+(?:\.[A-Za-z0-9]+)*$')][string]$PackageVersion = '8.0.1-preview.85')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$builtRoot = Join-Path $workspaceRoot 'artifacts/native/Release'
$runtimeRoot = Join-Path $workspaceRoot 'runtime/win-x64'
$manifestPath = Join-Path $runtimeRoot 'runtime-manifest.json'
$oldManifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$builtDlls = @(Get-ChildItem -LiteralPath $builtRoot -File -Filter '*.dll' | Sort-Object Name)
$expectedNames = @($oldManifest.files | Where-Object path -like 'occt/*.dll' |
    ForEach-Object { Split-Path -Leaf $_.path } | Sort-Object)
if ($builtDlls.Count -ne 62 -or
    @(Compare-Object $expectedNames @($builtDlls.Name) -CaseSensitive).Count -ne 0) {
    throw 'The Release closure must match the accepted 62-DLL runtime file set.'
}

# Keep the SDK notices and licenses intact; only the built Release DLLs are replaced.
foreach ($entry in @($oldManifest.files | Where-Object path -notlike 'occt/*.dll')) {
    $path = Join-Path $runtimeRoot $entry.path
    if ((Get-FileHash -LiteralPath $path).Hash -cne $entry.sha256 -or
        (Get-Item -LiteralPath $path).Length -ne [long]$entry.size) {
        throw "Existing notice/license differs from its manifest: '$($entry.path)'."
    }
}
$abiSource = Get-Content -LiteralPath (Join-Path $workspaceRoot 'src/OcctSharp.Native/src/Runtime/Abi.cpp') -Raw
if ($abiSource -notmatch 'AbiVersion\s*=\s*0x([0-9A-Fa-f]+)U') {
    throw 'Cannot read the declared native ABI identity.'
}
$abi = [Convert]::ToUInt32($Matches[1], 16)
if ($abiSource -notmatch 'BridgeVersion\s*=\s*"([^"]+)"') {
    throw 'Cannot read the declared native bridge identity.'
}
$bridgeVersion = $Matches[1]
foreach ($file in $builtDlls) {
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $runtimeRoot "occt/$($file.Name)") -Force
}
$oldManifest.packageVersion = $PackageVersion
$oldManifest.nativeAbi = '{0}.{1}' -f ($abi -shr 16), ($abi -band 0xffff)
$oldManifest.bridgeVersion = $bridgeVersion
$oldManifest.files = @(Get-ChildItem -LiteralPath $runtimeRoot -Recurse -File |
    Where-Object Name -ne 'runtime-manifest.json' | Sort-Object FullName | ForEach-Object {
        [ordered]@{
            path = [IO.Path]::GetRelativePath($runtimeRoot, $_.FullName).Replace('\', '/')
            size = $_.Length
            sha256 = (Get-FileHash -LiteralPath $_.FullName).Hash
        }
    })
[IO.File]::WriteAllText($manifestPath, ($oldManifest | ConvertTo-Json -Depth 6) + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))
& (Join-Path $PSScriptRoot 'verify-bundled-runtime.ps1') -CompareBuiltRuntime
Write-Host "Bundled Release runtime updated for $PackageVersion / ABI $($oldManifest.nativeAbi) / bridge $bridgeVersion."

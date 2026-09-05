[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$BaselineLibraryPath,
    [string]$ReleaseLibraryPath = 'artifacts/native/Release/OcctSharp.Native.dll',
    [string]$DebugLibraryPath = 'artifacts/native/Debug/OcctSharp.Native.dll',
    [Parameter(Mandatory)][ValidateRange(0, 100000)][int]$ExpectedAdditions,
    [string]$OutputPath = 'artifacts/native-export-compatibility.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$settings = Get-Content -LiteralPath (Join-Path $workspaceRoot 'config/local.settings.json') -Raw | ConvertFrom-Json
$toolset = Get-ChildItem -LiteralPath (Join-Path $settings.visualStudioRoot 'VC/Tools/MSVC') -Directory |
    Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
$dumpbin = Join-Path $toolset.FullName 'bin/Hostx64/x64/dumpbin.exe'
function Read-Exports([string]$Path) {
    $resolved = [IO.Path]::GetFullPath($Path, $workspaceRoot)
    $dump = @(& $dumpbin /nologo /exports $resolved)
    if ($LASTEXITCODE -ne 0) { throw "dumpbin failed: $resolved" }
    $names = @($dump | ForEach-Object {
        if ($_ -match '^\s+\d+\s+[0-9A-F]+\s+[0-9A-F]+\s+(\S+)') { $Matches[1] }
    } | Sort-Object -CaseSensitive)
    $unique = [Collections.Generic.HashSet[string]]::new([string[]]$names, [StringComparer]::Ordinal)
    if ($names.Count -eq 0 -or $unique.Count -ne $names.Count) { throw "Missing or duplicate exports: $resolved" }
    return ,$names
}
$before = Read-Exports $BaselineLibraryPath
$release = Read-Exports $ReleaseLibraryPath
$debug = Read-Exports $DebugLibraryPath
$oldNames = [Collections.Generic.HashSet[string]]::new([string[]]$before, [StringComparer]::Ordinal)
$releaseNames = [Collections.Generic.HashSet[string]]::new([string[]]$release, [StringComparer]::Ordinal)
if (-not $releaseNames.SetEquals([string[]]$debug)) { throw 'Release and Debug exported names differ.' }
$removed = @($before | Where-Object { -not $releaseNames.Contains($_) })
$added = @($release | Where-Object { -not $oldNames.Contains($_) })
if ($removed.Count -ne 0 -or $added.Count -ne $ExpectedAdditions) {
    throw "Unexpected Native export delta: $($removed.Count) removals, $($added.Count) additions (expected $ExpectedAdditions). Added: $($added -join ', ')"
}
$outputFile = [IO.Path]::GetFullPath($OutputPath, $workspaceRoot)
foreach ($inputFile in @($BaselineLibraryPath, $ReleaseLibraryPath, $DebugLibraryPath)) {
    if ($outputFile -eq [IO.Path]::GetFullPath($inputFile, $workspaceRoot)) { throw 'Export report must not overwrite its inputs.' }
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
$report = [ordered]@{ state='PASS'; baselineCount=$before.Count; currentCount=$release.Count; removed=0; added=$added; debugMatchesRelease=$true }
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json -Depth 4) + [Environment]::NewLine)
Write-Host "Native export compatibility PASS: $($release.Count) names, $($added.Count) additions, no removals; Debug matches Release."

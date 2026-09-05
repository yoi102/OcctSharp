[CmdletBinding()]
param([string]$OutputDirectory = 'artifacts/debug-native-validation')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$testRoot = Join-Path $workspaceRoot 'tests/OcctSharp.Runtime.Tests/bin/Debug/net10.0/win-x64'
$debugRoot = Join-Path $workspaceRoot 'artifacts/native/Debug'
$releaseBridge = Join-Path $workspaceRoot 'artifacts/native/Release/OcctSharp.Native.dll'
$debugBridge = Join-Path $debugRoot 'OcctSharp.Native.dll'
foreach ($file in @((Join-Path $testRoot 'OcctSharp.Runtime.Tests.dll'), $debugBridge, $releaseBridge)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Build both configurations first: missing '$file'." }
}
$debugHash = (Get-FileHash -LiteralPath $debugBridge -Algorithm SHA256).Hash
if ($debugHash -eq (Get-FileHash -LiteralPath $releaseBridge -Algorithm SHA256).Hash) {
    throw 'Debug and Release bridges are identical; actual Debug-native evidence is unavailable.'
}
$nativeFiles = @(Get-ChildItem -LiteralPath $debugRoot -File -Filter '*.dll')
if ($nativeFiles.Count -ne 62) { throw 'The accepted Debug runtime closure must contain exactly 62 DLLs.' }
$bundledNames = @(Get-ChildItem -LiteralPath (Join-Path $testRoot 'occt') -File -Filter '*.dll' | ForEach-Object Name | Sort-Object)
# This pinned SDK uses an explicitly different oneTBB name in Debug.
$expectedDebugNames = @($bundledNames | ForEach-Object {
    if ($_ -ceq 'tbb12.dll') { 'tbb12_debug.dll' } else { $_ }
} | Sort-Object)
if ($bundledNames.Count -ne 62 -or @(Compare-Object ($nativeFiles.Name | Sort-Object) $expectedDebugNames -CaseSensitive).Count) {
    throw 'Managed output and rebuilt Debug closure differ beyond the pinned oneTBB Debug filename.'
}
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory, $workspaceRoot)
$runRoot = Join-Path $outputRoot ('run-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($runRoot) | Out-Null
# Start with an empty native directory; never copy a Release-only DLL into the probe.
Get-ChildItem -LiteralPath $testRoot | Where-Object { $_.Name -ne 'occt' -and $_.Name -notin $bundledNames } |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $runRoot -Recurse }
$isolatedNative = Join-Path $runRoot 'occt'
[IO.Directory]::CreateDirectory($isolatedNative) | Out-Null
$hashes = @()
foreach ($file in $nativeFiles) {
    $target = Join-Path $isolatedNative $file.Name
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    $expected = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -cne $expected) { throw "Debug DLL mismatch: $($file.Name)" }
    # Some raw ABI tests also probe a bridge beside the test assembly. Keep it aligned.
    if (Test-Path -LiteralPath (Join-Path $testRoot $file.Name) -PathType Leaf) {
        Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $runRoot $file.Name) -Force
    }
    $hashes += [ordered]@{ name=$file.Name; sha256=$expected }
}
$copiedNames = @(Get-ChildItem -LiteralPath $isolatedNative -File -Filter '*.dll' | ForEach-Object Name | Sort-Object)
if (@(Compare-Object ($nativeFiles.Name | Sort-Object) $copiedNames -CaseSensitive).Count) {
    throw 'Isolated native runtime names do not exactly match the rebuilt Debug closure.'
}
& dotnet vstest (Join-Path $runRoot 'OcctSharp.Runtime.Tests.dll') "/ResultsDirectory:$runRoot" '/Logger:trx;LogFileName=actual-debug.trx'
if ($LASTEXITCODE -ne 0) { throw "Actual Debug-native Runtime regression failed; evidence retained in '$runRoot'." }
[xml]$trx = Get-Content -LiteralPath (Join-Path $runRoot 'actual-debug.trx') -Raw
$counts = $trx.TestRun.ResultSummary.Counters
if ([int]$counts.failed -ne 0 -or [int]$counts.notExecuted -ne 0 -or [int]$counts.passed -ne [int]$counts.total -or [int]$counts.total -eq 0) {
    throw 'Actual Debug-native tests must all execute and pass.'
}
$report = [ordered]@{ schemaVersion='1.0'; state='PASS'; tests=[int]$counts.total; debugBridgeSha256=$debugHash; runDirectory=$runRoot; files=$hashes }
[IO.File]::WriteAllText((Join-Path $outputRoot 'result.json'), ($report | ConvertTo-Json -Depth 5) + [Environment]::NewLine)
Write-Host "Actual Debug-native PASS: $($counts.passed)/$($counts.total), all 62 DLLs hash-verified; evidence: '$runRoot'."

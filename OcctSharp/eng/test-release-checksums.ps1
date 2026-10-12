[CmdletBinding()]
param(
    [string]$PackageVersion = '8.0.1-preview.85',
    [string]$OutputDirectory = 'artifacts/release-checksum-tests'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$output = [IO.Path]::GetFullPath($OutputDirectory, $workspace)
$artifactPrefix = [IO.Path]::GetFullPath((Join-Path $workspace 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $output.StartsWith($artifactPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw 'Test output must be in artifacts.' }
[IO.Directory]::CreateDirectory($output) | Out-Null
$checksumPath = Join-Path $workspace 'artifacts/release/checksums.sha256'
$original = [IO.File]::ReadAllBytes($checksumPath)
$lines = @(Get-Content -LiteralPath $checksumPath)
$cases = [Collections.Generic.List[string]]::new()
function Verify([string]$Name) {
    & (Join-Path $PSScriptRoot 'verify-package-content.ps1') -PackageVersion $PackageVersion -OutputPath (Join-Path $output "$Name.json")
}
function Reject([string]$Name, [string[]]$ChangedLines) {
    [IO.File]::WriteAllLines($checksumPath, $ChangedLines)
    $rejected = $false
    try { Verify $Name } catch {
        if ($_.Exception.Message -notmatch 'checksum') { throw }
        $rejected = $true
    } finally { [IO.File]::WriteAllBytes($checksumPath, $original) }
    if (-not $rejected) { throw "Expected checksum rejection: $Name" }
    $cases.Add($Name)
}
try {
    Verify 'complete'; $cases.Add('complete-14-package-set')
    $nativeName = "OcctSharp.Native.win-x64.$PackageVersion.nupkg"
    $nativeLine = @($lines | Where-Object { $_.EndsWith('  ' + $nativeName, [StringComparison]::Ordinal) })
    if ($nativeLine.Count -ne 1) { throw 'Missing native package checksum fixture.' }
    Reject 'missing-native' @($lines | Where-Object { $_ -cne $nativeLine[0] })
    Reject 'stale-native' @($lines | ForEach-Object { if ($_ -ceq $nativeLine[0]) { ('0' * 64) + '  ' + $nativeName } else { $_ } })
    Reject 'duplicate-native' @($lines + $nativeLine[0])
    Reject 'unknown-entry' @($lines + ('0' * 64 + '  unexpected.nupkg'))
    Verify 'restored'; $cases.Add('original-restored')
} finally { [IO.File]::WriteAllBytes($checksumPath, $original) }
[ordered]@{ state='PASS'; cases=$cases.ToArray(); packageVersion=$PackageVersion } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'result.json') -Encoding utf8
Write-Host "Release checksum tests PASS: $($cases.Count) cases."

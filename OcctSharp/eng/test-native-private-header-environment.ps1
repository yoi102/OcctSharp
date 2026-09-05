[CmdletBinding()]
param([string]$OutputDirectory = 'artifacts/private-header-environment-validation')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory, $workspaceRoot)
$verifier = Join-Path $PSScriptRoot 'verify-native-private-headers.ps1'

function Assert-EnvironmentUnchanged($Before) {
    $after = [Environment]::GetEnvironmentVariables('Process')
    if ($Before.Count -ne $after.Count) { throw 'Header verification changed the process environment key set.' }
    foreach ($name in $Before.Keys) {
        if ($Before[$name] -cne $after[$name]) { throw "Header verification leaked environment variable '$name'." }
    }
}

$baseline = [Environment]::GetEnvironmentVariables('Process')
& $verifier -OutputDirectory (Join-Path $outputRoot 'headers')
Assert-EnvironmentUnchanged $baseline

# Invalid Windows filename: fail after importing vcvars, before compiling a header.
# No file is removed, and the source tree is never mutated.
$rejected = $false
try { & $verifier -OutputDirectory (Join-Path $outputRoot 'invalid<path') }
catch {
    if ($_.Exception.Message -notlike '*invalid<path*') { throw }
    $rejected = $true
}
if (-not $rejected) { throw 'The invalid output path was unexpectedly accepted.' }
Assert-EnvironmentUnchanged $baseline

$report = [ordered]@{ schemaVersion='1.0'; state='PASS'; successPath=$true; failurePath=$true; environmentUnchanged=$true }
[IO.File]::WriteAllText((Join-Path $outputRoot 'result.json'), ($report | ConvertTo-Json) + [Environment]::NewLine)
Write-Host 'Header verifier environment restoration PASS: success and failure paths; all process keys/values unchanged.'

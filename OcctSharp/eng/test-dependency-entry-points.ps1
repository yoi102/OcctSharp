[CmdletBinding()]
param([string]$OutputRoot = 'artifacts/dependency-entry-tests')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot, $workspace)
$fixture = Join-Path $OutputRoot ([Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($fixture) | Out-Null
$checks = [Collections.Generic.List[string]]::new()
function Reject([string]$Name, [string]$Message, [scriptblock]$Action) {
    try { & $Action } catch {
        if ($_.Exception.Message -notlike "*$Message*") { throw }
        $checks.Add($Name)
        return
    }
    throw "Expected rejection did not occur: $Name"
}
Reject 'in-repository registry checkout' 'outside the OcctSharp repository' {
    & (Join-Path $PSScriptRoot 'restore-dependencies.ps1') -VcpkgRoot (Join-Path $workspace 'artifacts/forbidden-checkout')
}
$nonempty = Join-Path $fixture 'nonempty'
[IO.Directory]::CreateDirectory($nonempty) | Out-Null
Set-Content -LiteralPath (Join-Path $nonempty 'retained.txt') -Value 'retain'
$external = Join-Path ([IO.Path]::GetTempPath()) 'OcctSharp-registry-rejection-unused'
Reject 'clean restore rejects existing contents' 'empty install root' {
    & (Join-Path $PSScriptRoot 'restore-dependencies.ps1') -VcpkgRoot $external -InstallRoot $nonempty -RequireEmpty
}
Reject 'invalid concurrency' 'Concurrency must be positive' {
    & (Join-Path $PSScriptRoot 'restore-dependencies.ps1') -VcpkgRoot $external -InstallRoot (Join-Path $fixture 'empty') -Concurrency 0
}
$badProvider = Join-Path $fixture 'bad-provider'
[IO.Directory]::CreateDirectory($badProvider) | Out-Null
@{state='FAIL';provider='wrong';registryCommit='wrong';triplet='x64-windows'} | ConvertTo-Json |
    Set-Content -LiteralPath (Join-Path $badProvider 'occtsharp-provider.json') -Encoding utf8
Reject 'build rejects unqualified provider' 'recovery entry point first' {
    & (Join-Path $PSScriptRoot 'build-vcpkg.ps1') -InstallRoot $badProvider
}
if ((Get-Content -LiteralPath (Join-Path $nonempty 'retained.txt') -Raw).Trim() -cne 'retain') { throw 'Negative recovery checks modified retained content.' }
[ordered]@{state='PASS';checks=@($checks);count=$checks.Count;fixture=$fixture;networkUsed=$false;nativeBuildStarted=$false} |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputRoot 'result.json') -Encoding utf8
Write-Host "Dependency entry rejection checks PASS: $($checks.Count)/$($checks.Count)."

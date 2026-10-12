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
$badRuntime = Join-Path $fixture 'bad-runtime'
$badRuntimeBin = Join-Path $badRuntime 'runtime/Release'
[IO.Directory]::CreateDirectory($badRuntimeBin) | Out-Null
$runtimeManifestPath = Join-Path $badRuntime 'runtime-manifest-Release.json'
@{state='FAIL';configuration='Release'} | ConvertTo-Json | Set-Content -LiteralPath $runtimeManifestPath
Reject 'runtime rejects unqualified build' 'native build manifest is required' {
    & (Join-Path $PSScriptRoot 'verify-vcpkg-runtime.ps1') -BuildRoot $badRuntime
}
@{state='BUILD_PASS_RUNTIME_NOT_RUN';configuration='Release';provider='fixture';files=@(@{path='occt/OcctSharp.Native.dll';sha256='0'*64})} |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $runtimeManifestPath
Reject 'runtime rejects missing native payload' 'DLL names differ' {
    & (Join-Path $PSScriptRoot 'verify-vcpkg-runtime.ps1') -BuildRoot $badRuntime
}
Set-Content -LiteralPath (Join-Path $badRuntimeBin 'OcctSharp.Native.dll') -Value 'modified payload'
Reject 'runtime rejects modified native payload' 'runtime changed' {
    & (Join-Path $PSScriptRoot 'verify-vcpkg-runtime.ps1') -BuildRoot $badRuntime
}
$badArchive = Join-Path $fixture 'wrong-source.tar.gz'
Set-Content -LiteralPath $badArchive -Value 'untrusted source bytes'
Reject 'header recovery rejects different source archive' 'differs from the pinned SHA512' {
    & (Join-Path $PSScriptRoot 'restore-generator-headers.ps1') -ArchivePath $badArchive -IncludeRoot (Join-Path $fixture 'unused-headers')
}
if (Test-Path -LiteralPath (Join-Path $fixture 'unused-headers')) { throw 'Invalid source archive created an SDK view.' }
if ((Get-Content -LiteralPath (Join-Path $nonempty 'retained.txt') -Raw).Trim() -cne 'retain') { throw 'Negative recovery checks modified retained content.' }
[ordered]@{state='PASS';checks=@($checks);count=$checks.Count;fixture=$fixture;networkUsed=$false;nativeBuildStarted=$false} |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutputRoot 'result.json') -Encoding utf8
Write-Host "Dependency entry rejection checks PASS: $($checks.Count)/$($checks.Count)."

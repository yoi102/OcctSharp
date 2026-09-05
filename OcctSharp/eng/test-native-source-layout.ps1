[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $workspaceRoot 'artifacts'))
$fixtureRoot = Join-Path $artifactRoot "native-layout-test-$([Guid]::NewGuid().ToString('N'))"
$fixtureNative = Join-Path $fixtureRoot 'src/OcctSharp.Native'
$fixtureEng = Join-Path $fixtureRoot 'eng'
New-Item -ItemType Directory -Path $fixtureNative, $fixtureEng -Force | Out-Null
try {
    Copy-Item -LiteralPath (Join-Path $workspaceRoot 'src/OcctSharp.Native/src') -Destination $fixtureNative -Recurse
    Copy-Item -LiteralPath (Join-Path $workspaceRoot 'src/OcctSharp.Native/CMakeLists.txt') -Destination $fixtureNative
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'verify-native-source-layout.ps1') -Destination $fixtureEng
    $verifier = Join-Path $fixtureEng 'verify-native-source-layout.ps1'
    & $verifier

    $cases = @(
        @{ name = 'unlisted source'; path = 'CMakeLists.txt'; find = '  src/Runtime/Abi.cpp'; replace = ''; error = 'exactly once' },
        @{ name = 'duplicate shared state'; path = 'src/Runtime/Error.cpp'; find = 'thread_local std::string LastError;'; replace = "thread_local std::string LastError;`nthread_local std::string LastError;"; error = 'duplicate owner' },
        @{ name = 'implementation include'; path = 'src/Runtime/Error.cpp'; find = '#include "Runtime/Error.hxx"'; replace = '#include "Runtime/Error.cpp"'; error = 'Implementation inclusion' },
        @{ name = 'unity build'; path = 'CMakeLists.txt'; find = 'UNITY_BUILD OFF'; replace = 'UNITY_BUILD ON'; error = 'unity build' },
        @{ name = 'manual PCH'; path = 'CMakeLists.txt'; find = 'SKIP_PRECOMPILE_HEADERS ON'; replace = 'SKIP_PRECOMPILE_HEADERS OFF'; error = 'own dependencies' },
        @{ name = 'oversized responsibility'; path = 'src/Runtime/Error.cpp'; find = 'thread_local std::string LastError;'; replace = ('thread_local std::string LastError;' + ("`n// fixture" * 1001)); error = '1,000 lines' }
    )
    foreach ($case in $cases) {
        $path = Join-Path $fixtureNative $case.path
        $original = [IO.File]::ReadAllText($path)
        if (-not $original.Contains($case.find)) { throw "Fixture anchor missing: $($case.name)." }
        try {
            [IO.File]::WriteAllText($path, $original.Replace($case.find, $case.replace))
            $rejected = $false
            try { & $verifier }
            catch {
                if (-not $_.Exception.Message.Contains($case.error)) { throw }
                $rejected = $true
            }
            if (-not $rejected) { throw "Invalid native source layout was accepted: $($case.name)." }
        }
        finally { [IO.File]::WriteAllText($path, $original) }
    }
    & $verifier
    Write-Host "Native source layout negative checks PASS: $($cases.Count)/$($cases.Count); source tree was never mutated."
}
finally {
    $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
    if ($resolvedFixture.StartsWith($artifactRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedFixture).StartsWith('native-layout-test-', [StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
}

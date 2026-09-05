[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$artifactRoot = [IO.Path]::GetFullPath((Join-Path $workspaceRoot 'artifacts'))
$fixtureRoot = Join-Path $artifactRoot "native-layout-test-$([Guid]::NewGuid().ToString('N'))"
$fixtureNative = Join-Path $fixtureRoot 'src/OcctSharp.Native'
$fixtureEng = Join-Path $fixtureRoot 'eng'
$fixtureConfig = Join-Path $fixtureRoot 'config/native-source-layout-exceptions.json'
New-Item -ItemType Directory -Path $fixtureNative, $fixtureEng -Force | Out-Null
try {
    Copy-Item -LiteralPath (Join-Path $workspaceRoot 'src/OcctSharp.Native/src') -Destination $fixtureNative -Recurse
    Copy-Item -LiteralPath (Join-Path $workspaceRoot 'src/OcctSharp.Native/CMakeLists.txt') -Destination $fixtureNative
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'verify-native-source-layout.ps1') -Destination $fixtureEng
    [IO.Directory]::CreateDirectory((Split-Path -Parent $fixtureConfig)) | Out-Null
    $sourceConfig = Join-Path $workspaceRoot 'config/native-source-layout-exceptions.json'
    $originalConfig = if (Test-Path -LiteralPath $sourceConfig) { [IO.File]::ReadAllText($sourceConfig) } else { '{"schemaVersion":"1.0","exceptions":[]}' }
    [IO.File]::WriteAllText($fixtureConfig, $originalConfig)
    $baselineExceptions = @(($originalConfig | ConvertFrom-Json).exceptions)
    $verifier = Join-Path $fixtureEng 'verify-native-source-layout.ps1'
    & $verifier

    $cases = @(
        @{ name = 'unlisted source'; path = 'CMakeLists.txt'; find = '  src/Runtime/Abi.cpp'; replace = ''; error = 'exactly once' },
        @{ name = 'duplicate shared state'; path = 'src/Runtime/Error.cpp'; find = 'thread_local std::string LastError;'; replace = "thread_local std::string LastError;`nthread_local std::string LastError;"; error = 'duplicate owner' },
        @{ name = 'implementation include'; path = 'src/Runtime/Error.cpp'; find = '#include "Runtime/Error.hxx"'; replace = '#include "Runtime/Error.cpp"'; error = 'Implementation inclusion' },
        @{ name = 'unity build'; path = 'CMakeLists.txt'; find = 'UNITY_BUILD OFF'; replace = 'UNITY_BUILD ON'; error = 'unity build' },
        @{ name = 'manual PCH'; path = 'CMakeLists.txt'; find = 'SKIP_PRECOMPILE_HEADERS ON'; replace = 'SKIP_PRECOMPILE_HEADERS OFF'; error = 'own dependencies' },
        @{ name = 'oversized responsibility'; path = 'src/Runtime/Error.cpp'; find = 'thread_local std::string LastError;'; replace = ('thread_local std::string LastError;' + ("`n// fixture" * 1001)); error = 'source-size limit' }
    )
    foreach ($case in $cases) {
        $path = Join-Path $fixtureNative $case.path
        $original = [IO.File]::ReadAllText($path)
        if (-not $original.Contains($case.find)) { throw "Fixture anchor missing: $($case.name)." }
        try {
            if ($case.name -eq 'oversized responsibility') {
                $withoutTarget = @($baselineExceptions | Where-Object path -NE 'src/Runtime/Error.cpp')
                [IO.File]::WriteAllText($fixtureConfig, (@{ schemaVersion = '1.0'; exceptions = $withoutTarget } | ConvertTo-Json -Depth 5))
            }
            [IO.File]::WriteAllText($path, $original.Replace($case.find, $case.replace))
            $rejected = $false
            try { & $verifier }
            catch {
                if (-not $_.Exception.Message.Contains($case.error)) { throw }
                $rejected = $true
            }
            if (-not $rejected) { throw "Invalid native source layout was accepted: $($case.name)." }
        }
        finally {
            [IO.File]::WriteAllText($path, $original)
            [IO.File]::WriteAllText($fixtureConfig, $originalConfig)
        }
    }
    & $verifier
    Write-Host "Native source layout negative checks PASS: $($cases.Count)/$($cases.Count); source tree was never mutated."

    $oversizedPath = Join-Path $fixtureNative 'src/Runtime/Error.cpp'
    $originalSource = [IO.File]::ReadAllText($oversizedPath)
    try {
        [IO.File]::WriteAllText($oversizedPath, $originalSource + ("`n// cohesive size fixture" * 1001))
        $oversizedCount = @([IO.File]::ReadLines($oversizedPath)).Count
        $validEntry = @{ path = 'src/Runtime/Error.cpp'; maxLines = $oversizedCount; reason = 'Fixture keeps one cohesive responsibility together.' }
        function Write-SizeConfiguration([object[]]$Entries) {
            $unaffected = @($baselineExceptions | Where-Object { $_.path -ne 'src/Runtime/Error.cpp' -and $_.path -notin $Entries.path })
            $json = @{ schemaVersion = '1.0'; exceptions = @($unaffected) + @($Entries) } | ConvertTo-Json -Depth 5
            [IO.File]::WriteAllText($fixtureConfig, $json)
        }
        Write-SizeConfiguration @($validEntry)
        & $verifier
        $sizeReport = Get-Content -LiteralPath (Join-Path $fixtureRoot 'artifacts/native-source-layout.json') -Raw | ConvertFrom-Json
        $reportedTarget = @($sizeReport.sourceSizeExceptions | Where-Object path -EQ $validEntry.path)
        if ($reportedTarget.Count -ne 1 -or
            $reportedTarget[0].lineCount -ne $oversizedCount -or
            $reportedTarget[0].reason -ne $validEntry.reason) {
            throw 'Accepted native source-size exception was not reported with its rationale and actual size.'
        }
        $invalidConfigurations = @(
            @{ name = 'missing rationale'; entries = @(@{ path = $validEntry.path; maxLines = $oversizedCount; reason = ' ' }); error = 'nonblank reason' },
            @{ name = 'nonexistent source'; entries = @(@{ path = 'src/Runtime/Missing.cpp'; maxLines = $oversizedCount; reason = 'Fixture' }); error = 'exact existing' },
            @{ name = 'duplicate exception'; entries = @($validEntry, $validEntry); error = 'Duplicate native' },
            @{ name = 'exceeded exception'; entries = @(@{ path = $validEntry.path; maxLines = $oversizedCount - 1; reason = 'Fixture' }); error = 'source-size limit' },
            @{ name = 'fractional bound'; entries = @(@{ path = $validEntry.path; maxLines = 2000.5; reason = 'Fixture' }); error = 'finite integer' },
            @{ name = 'wrong source exception'; entries = @(@{ path = 'src/Runtime/Abi.cpp'; maxLines = $oversizedCount; reason = 'Fixture' }); error = 'source-size limit' }
        )
        foreach ($case in $invalidConfigurations) {
            Write-SizeConfiguration $case.entries
            $rejected = $false
            try { & $verifier }
            catch {
                if (-not $_.Exception.Message.Contains($case.error)) { throw }
                $rejected = $true
            }
            if (-not $rejected) { throw "Invalid size exception accepted: $($case.name)." }
        }
        # A valid size exception cannot exempt its file from shared-state validation.
        Write-SizeConfiguration @(@{ path = $validEntry.path; maxLines = $oversizedCount + 10; reason = $validEntry.reason })
        [IO.File]::AppendAllText($oversizedPath, "`nthread_local std::string LastError;`n")
        $rejected = $false
        try { & $verifier }
        catch {
            if (-not $_.Exception.Message.Contains('duplicate owner')) { throw }
            $rejected = $true
        }
        if (-not $rejected) { throw 'A size exception bypassed shared-state validation.' }
        Write-Host "Native source-size exception checks PASS: bounded positive/report case and $($invalidConfigurations.Count + 1) rejection cases."
    }
    finally {
        [IO.File]::WriteAllText($oversizedPath, $originalSource)
        [IO.File]::WriteAllText($fixtureConfig, $originalConfig)
    }
    & $verifier
}
finally {
    $resolvedFixture = [IO.Path]::GetFullPath($fixtureRoot)
    if ($resolvedFixture.StartsWith($artifactRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedFixture).StartsWith('native-layout-test-', [StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
}

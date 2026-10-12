[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$BuildRoot,
    [ValidateSet('Release','Debug')][string]$Configuration = 'Release',
    [string]$OutputDirectory
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $workspace
try {
    $runnerSdk = (& dotnet --version).Trim()
    if ($LASTEXITCODE -or $runnerSdk -cnotmatch '^10\.\d+\.\d+$') { throw 'The Runtime runner requires the repository-selected stable .NET10 SDK.' }
} finally { Pop-Location }
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$manifestPath = Join-Path $BuildRoot "runtime-manifest-$Configuration.json"
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($manifest.state -cne 'BUILD_PASS_RUNTIME_NOT_RUN' -or $manifest.configuration -cne $Configuration) {
    throw 'A matching vcpkg native build manifest is required.'
}
$nativeRoot = Join-Path $BuildRoot "runtime/$Configuration"
$expectedNames = @($manifest.files | ForEach-Object { [IO.Path]::GetFileName($_.path) } | Sort-Object)
$actualNames = @(Get-ChildItem -LiteralPath $nativeRoot -File -Filter '*.dll' | Select-Object -ExpandProperty Name | Sort-Object)
if (-not $actualNames.Count -or @(Compare-Object $expectedNames $actualNames -CaseSensitive).Count) {
    throw 'Vcpkg runtime DLL names differ from the configuration manifest.'
}
foreach ($file in $manifest.files) {
    $source = Join-Path $nativeRoot ([IO.Path]::GetFileName($file.path))
    if ((Get-FileHash -LiteralPath $source).Hash -cne $file.sha256) { throw "Vcpkg runtime changed: $($file.path)" }
}
$managedRoot = Join-Path $workspace "tests/OcctSharp.Runtime.Tests/bin/$Configuration/net10.0/win-x64"
if (-not (Test-Path -LiteralPath (Join-Path $managedRoot 'OcctSharp.Runtime.Tests.dll'))) { throw 'Build the managed Runtime suite first.' }
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $workspace "artifacts/vcpkg-runtime-$Configuration" }
$output = [IO.Path]::GetFullPath($OutputDirectory, $workspace)
$artifactPrefix = [IO.Path]::GetFullPath((Join-Path $workspace 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $output.StartsWith($artifactPrefix, [StringComparison]::OrdinalIgnoreCase)) { throw 'Runtime evidence must stay in artifacts.' }
$run = Join-Path $output ('run-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($run) | Out-Null
$oldNativeNames = @(Get-ChildItem -LiteralPath (Join-Path $managedRoot 'occt') -File -Filter '*.dll' | Select-Object -ExpandProperty Name)
Get-ChildItem -LiteralPath $managedRoot | Where-Object { $_.Name -cne 'occt' -and $_.Name -notin $oldNativeNames } |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $run -Recurse }
$isolated = Join-Path $run 'occt'
[IO.Directory]::CreateDirectory($isolated) | Out-Null
foreach ($file in $manifest.files) {
    $name = [IO.Path]::GetFileName($file.path)
    Copy-Item -LiteralPath (Join-Path $nativeRoot $name) -Destination (Join-Path $isolated $name)
    if (Test-Path -LiteralPath (Join-Path $managedRoot $name) -PathType Leaf) {
        Copy-Item -LiteralPath (Join-Path $nativeRoot $name) -Destination (Join-Path $run $name)
    }
    if ((Get-FileHash -LiteralPath (Join-Path $isolated $name)).Hash -cne $file.sha256) { throw 'Isolated vcpkg native copy mismatch.' }
}
$arguments = @('vstest', ('"' + (Join-Path $run 'OcctSharp.Runtime.Tests.dll') + '"'),
    ('"/ResultsDirectory:' + $run + '"'), '/Logger:trx;LogFileName=vcpkg-runtime.trx')
$process = Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList $arguments -WorkingDirectory $workspace -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput (Join-Path $run 'runtime.stdout.log') -RedirectStandardError (Join-Path $run 'runtime.stderr.log')
$observed = @{}
while (-not $process.HasExited) {
    if (-not $observed.Count) {
        $snapshot = @(Get-CimInstance Win32_Process -Filter "Name='testhost.exe' OR Name='dotnet.exe' OR Name='vstest.console.exe'")
        $descendants = [Collections.Generic.HashSet[uint32]]::new()
        [void]$descendants.Add([uint32]$process.Id)
        do {
            $added = $false
            foreach ($entry in $snapshot) {
                if ($descendants.Contains([uint32]$entry.ParentProcessId) -and $descendants.Add([uint32]$entry.ProcessId)) { $added = $true }
            }
        } while ($added)
        $hosts = @($snapshot | Where-Object { $descendants.Contains([uint32]$_.ProcessId) -and
            ($_.Name -ieq 'testhost.exe' -or ($_.CommandLine -and $_.CommandLine -match 'testhost\.dll')) })
        foreach ($hostProcess in $hosts) {
            # A testhost may exit while its modules are enumerated. Retry that
            # observation; do not suppress any path/hash mismatch afterwards.
            try { $modules = @((Get-Process -Id $hostProcess.ProcessId -ErrorAction Stop).Modules) } catch { continue }
            if (-not @($modules | Where-Object ModuleName -IEQ 'OcctSharp.Native.dll').Count) { continue }
            foreach ($module in $modules) {
                $expectedFile = @($manifest.files | Where-Object { [IO.Path]::GetFileName($_.path) -ieq $module.ModuleName })
                if (-not $expectedFile.Count) { continue }
                $path = [IO.Path]::GetFullPath($module.FileName)
                if (-not $path.StartsWith($run.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase) -or
                    (Get-FileHash -LiteralPath $path).Hash -cne $expectedFile[0].sha256) {
                    throw "Loaded vcpkg runtime escaped its exact isolated payload: $($module.ModuleName)"
                }
                $observed[$module.ModuleName] = [ordered]@{name=$module.ModuleName;path=$path;sha256=$expectedFile[0].sha256}
            }
        }
    }
    Start-Sleep -Milliseconds 300
    $process.Refresh()
}
$process.WaitForExit()
if ($process.ExitCode) { throw "Actual vcpkg $Configuration Runtime suite failed; evidence retained in $run" }
if (-not $observed.ContainsKey('OcctSharp.Native.dll')) { throw 'Actual loaded vcpkg native bridge was not observed.' }
[xml]$trx = Get-Content -LiteralPath (Join-Path $run 'vcpkg-runtime.trx') -Raw
$counts = $trx.TestRun.ResultSummary.Counters
if ([int]$counts.total -eq 0 -or [int]$counts.passed -ne [int]$counts.total -or
    [int]$counts.failed -ne 0 -or [int]$counts.notExecuted -ne 0) { throw 'The full vcpkg Runtime suite must execute and pass.' }
[ordered]@{state='PASS';configuration=$Configuration;provider=$manifest.provider;tests=[int]$counts.total;
    runnerSdk=$runnerSdk;
    nativeDlls=$actualNames.Count;nativeManifestSha256=(Get-FileHash -LiteralPath $manifestPath).Hash;
    runDirectory=$run;files=$manifest.files;loadedNativeModules=@($observed.Values | Sort-Object name)} | ConvertTo-Json -Depth 6 |
    Set-Content -LiteralPath (Join-Path $output 'result.json') -Encoding utf8
Write-Host "Actual vcpkg $Configuration Runtime PASS: $($counts.passed)/$($counts.total); $($actualNames.Count) configuration-specific DLLs."

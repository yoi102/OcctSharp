[CmdletBinding()]
param([ValidateSet('Release','Debug')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$savedEnvironment = [Environment]::GetEnvironmentVariables('Process')
try {
    $settings = Get-Content (Join-Path $workspace 'config/local.settings.json') -Raw | ConvertFrom-Json
    $vcvars = Join-Path $settings.visualStudioRoot 'VC/Auxiliary/Build/vcvars64.bat'
    $lines = @(& cmd.exe /d /c ('call "' + $vcvars + '" >nul && set'))
    if ($LASTEXITCODE) { throw 'Cannot initialize the compiler environment.' }
    foreach ($line in $lines) {
        if ($line -match '^([^=]+)=(.*)$') { [Environment]::SetEnvironmentVariable($Matches[1], $Matches[2], 'Process') }
    }
    $output = Join-Path $workspace "artifacts/geometric-value-abi/$Configuration"
    [IO.Directory]::CreateDirectory($output) | Out-Null
    $native = Join-Path $workspace 'src/OcctSharp.Native'
    $headers = @(Get-ChildItem (Join-Path $native 'generated') -Recurse -Filter '*.h' | Sort-Object FullName)
    $cProbe = Join-Path $output 'all-generated-headers.c'
    $includes = @($headers | ForEach-Object { '#include "' + [IO.Path]::GetRelativePath((Join-Path $native 'generated'), $_.FullName).Replace('\','/') + '"' })
    [IO.File]::WriteAllText($cProbe, ($includes -join "`n") + "`n")
    & cl.exe /nologo /TC /std:c11 /Zs /W4 /WX "/I$native/include" "/I$native/generated" $cProbe
    if ($LASTEXITCODE) { throw 'Strict C11 generated headers failed.' }
    $crt = if ($Configuration -eq 'Debug') { '/MDd' } else { '/MD' }
    $sdkConfig = if ($Configuration -eq 'Debug') { 'libd' } else { 'lib' }
    $binConfig = if ($Configuration -eq 'Debug') { 'bind' } else { 'bin' }
    $exe = Join-Path $output 'GeometryValueProjectionTests.exe'
    & cl.exe /nologo /std:c++20 /EHsc /W4 /WX $crt "/I$native/generated" "/I$($settings.occtRoot)/inc" `
        "/Fo$output/GeometryValueProjectionTests.obj" "/Fe$exe" (Join-Path $workspace 'tests/native/GeometryValueProjectionTests.cpp') `
        /link "/LIBPATH:$($settings.occtRoot)/win64/vc14/$sdkConfig" TKernel.lib TKMath.lib
    if ($LASTEXITCODE) { throw 'Native geometry fixture compilation failed.' }
    $env:PATH = "$($settings.occtRoot)/win64/vc14/$binConfig;$workspace/artifacts/native/$Configuration;$env:PATH"
    & $exe
    if ($LASTEXITCODE) { throw 'Native geometry fixture failed.' }
    Write-Host "Geometric ABI PASS: $Configuration, $($headers.Count) C11 headers, thirty native size/offset assertions, matrix/coordinate/direction semantics."
}
finally {
    foreach ($name in @([Environment]::GetEnvironmentVariables('Process').Keys)) {
        if (-not $savedEnvironment.Contains($name)) { [Environment]::SetEnvironmentVariable($name, [NullString]::Value, 'Process') }
    }
    foreach ($name in $savedEnvironment.Keys) { [Environment]::SetEnvironmentVariable($name, $savedEnvironment[$name], 'Process') }
}

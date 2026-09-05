[CmdletBinding()]
param([string]$OutputDirectory = 'artifacts/private-header-validation')

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$originalEnvironment = [Environment]::GetEnvironmentVariables('Process')
try {
$settings = Get-Content -LiteralPath (Join-Path $workspaceRoot 'config/local.settings.json') -Raw | ConvertFrom-Json
$vcvars = Join-Path $settings.visualStudioRoot 'VC/Auxiliary/Build/vcvars64.bat'
$environmentLines = @(& cmd.exe /d /c ('call "' + $vcvars + '" >nul && set'))
if ($LASTEXITCODE -ne 0) { throw 'Unable to initialize the x64 compiler environment.' }
foreach ($line in $environmentLines) {
    if ($line -match '^([^=]+)=(.*)$') { [Environment]::SetEnvironmentVariable($Matches[1], $Matches[2], 'Process') }
}
$nativeRoot = Join-Path $workspaceRoot 'src/OcctSharp.Native'
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory, $workspaceRoot)
[IO.Directory]::CreateDirectory($outputRoot) | Out-Null
$headers = @(Get-ChildItem -LiteralPath (Join-Path $nativeRoot 'src') -Recurse -File -Filter '*.hxx' | Sort-Object FullName)
$checked = @()
foreach ($header in $headers) {
    $relative = [IO.Path]::GetRelativePath((Join-Path $nativeRoot 'src'), $header.FullName).Replace('\', '/')
    $probe = Join-Path $outputRoot ($relative.Replace('/', '-') + '.cpp')
    [IO.File]::WriteAllText($probe, '#include "' + $relative + '"' + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
    & cl.exe /nologo /std:c++20 /EHsc /Zs /W4 /WX /permissive- /TP /DOCCTSHARP_NATIVE_EXPORTS /DNOMINMAX /DWIN32_LEAN_AND_MEAN= `
        "/I$(Join-Path $settings.occtRoot 'inc')" "/I$(Join-Path $nativeRoot 'include')" "/I$(Join-Path $nativeRoot 'src')" $probe
    if ($LASTEXITCODE -ne 0) { throw "Standalone header validation failed: $relative" }
    $checked += $relative
}
$report = [ordered]@{ schemaVersion='1.0'; state='PASS'; headerCount=$checked.Count; compiler=(Get-Command cl.exe).Source; headers=$checked }
[IO.File]::WriteAllText((Join-Path $outputRoot 'result.json'), ($report | ConvertTo-Json -Depth 4) + [Environment]::NewLine)
Write-Host "Standalone private headers PASS: $($checked.Count)/$($checked.Count), MSVC /Zs /W4 /WX, no PCH."
}
finally {
    # vcvars supplies Platform=x64; leaking it into a following managed solution
    # restore makes dotnet select the nonexistent Debug|x64 configuration.
    foreach ($name in @([Environment]::GetEnvironmentVariables('Process').Keys)) {
        if (-not $originalEnvironment.Contains($name)) {
            [Environment]::SetEnvironmentVariable($name, [NullString]::Value, 'Process')
        }
    }
    foreach ($name in $originalEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $originalEnvironment[$name], 'Process')
    }
}

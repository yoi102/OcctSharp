[CmdletBinding()]
param([ValidateSet('Release','Debug')][string]$Configuration='Release',[string]$OutputDirectory='artifacts/modeling-result-buffers')
$ErrorActionPreference='Stop'
$workspace=Split-Path -Parent $PSScriptRoot
$saved=[Environment]::GetEnvironmentVariables('Process')
try{
 $settings=Get-Content (Join-Path $workspace 'config/local.settings.json') -Raw | ConvertFrom-Json
 $vcvars=Join-Path $settings.visualStudioRoot 'VC/Auxiliary/Build/vcvars64.bat'
 $lines=& cmd.exe /d /c ('call "'+$vcvars+'" >nul && set')
 if($LASTEXITCODE -ne 0){throw 'Compiler environment failed'}
 foreach($line in $lines){if($line -match '^([^=]+)=(.*)$'){[Environment]::SetEnvironmentVariable($Matches[1],$Matches[2],'Process')}}
 $native=Join-Path $workspace "artifacts/native/$Configuration"
 $output=[IO.Path]::GetFullPath((Join-Path $OutputDirectory $Configuration),$workspace)
 [IO.Directory]::CreateDirectory($output) | Out-Null
 $crt=if($Configuration -eq 'Debug'){'/MDd'}else{'/MD'}
 & cl.exe /nologo /TC /std:c11 /W4 /WX $crt /DNOMINMAX `
  "/I$(Join-Path $workspace 'src/OcctSharp.Native/generated/Modeling')" `
  "/I$(Join-Path $settings.occtRoot 'inc')" `
  "/Fo$output/probe.obj" "/Fe$output/probe.exe" (Join-Path $workspace 'tests/native/modeling-result-buffer-tests.c') `
  /link "/LIBPATH:$(Join-Path $workspace "build/native-vs18/src/OcctSharp.Native/lib/$Configuration")" OcctSharp.Native.lib
 if($LASTEXITCODE -ne 0){throw 'Modeling result C ABI fixture compilation failed'}
 $env:PATH=$native+';'+$env:PATH
 & (Join-Path $output 'probe.exe')
 if($LASTEXITCODE -ne 0){throw 'Modeling result C ABI canary/absence/publication check failed'}
}finally{
 foreach($key in @([Environment]::GetEnvironmentVariables('Process').Keys)){if(-not $saved.Contains($key)){[Environment]::SetEnvironmentVariable($key,$null,'Process')}}
 foreach($key in $saved.Keys){[Environment]::SetEnvironmentVariable($key,[string]$saved[$key],'Process')}
}

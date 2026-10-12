param([ValidateSet('Release','Debug')][string]$Configuration='Release')
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$settings=Get-Content (Join-Path $workspace 'config/local.settings.json') -Raw | ConvertFrom-Json
$previous=[Environment]::GetEnvironmentVariables('Process')
try {
 $vcvars=Join-Path $settings.visualStudioRoot 'VC/Auxiliary/Build/vcvars64.bat'
 $lines=@(& cmd.exe /d /c ('call "'+$vcvars+'" >nul && set'))
 if($LASTEXITCODE -ne 0){throw 'VC environment failed'}
 foreach($line in $lines){if($line -match '^([^=]+)=(.*)$'){[Environment]::SetEnvironmentVariable($Matches[1],$Matches[2],'Process')}}
 $output=Join-Path $workspace ('artifacts/batch-cg/shim-'+$Configuration)
 [IO.Directory]::CreateDirectory($output)|Out-Null
 $lib=Join-Path $settings.occtRoot ('win64/vc14/'+$(if($Configuration -eq 'Debug'){'libd'}else{'lib'}))
 $crt=if($Configuration -eq 'Debug'){'/MDd'}else{'/MD'}
 & cl.exe /nologo /LD /std:c++20 /EHsc /W4 /WX /permissive- $crt /DNOMINMAX /DWIN32_LEAN_AND_MEAN= "/I$(Join-Path $settings.occtRoot 'inc')" "/I$(Join-Path $workspace 'src/OcctSharp.Native/include')" "/I$(Join-Path $workspace 'src/OcctSharp.Native/src')" (Join-Path $PSScriptRoot 'ContextFaultShim.cpp') "/Fo$output/ContextFaultShim.obj" /link "/OUT:$output/ContextFaultShim.dll" "/IMPLIB:$output/ContextFaultShim.lib" "/LIBPATH:$lib" TKernel.lib TKMath.lib TKV3d.lib TKOpenGl.lib TKService.lib
 if($LASTEXITCODE -ne 0){throw 'Test-only context shim compilation failed'}
} finally {
 foreach($name in @([Environment]::GetEnvironmentVariables('Process').Keys)){if(-not $previous.Contains($name)){[Environment]::SetEnvironmentVariable($name,[NullString]::Value,'Process')}}
 foreach($name in $previous.Keys){[Environment]::SetEnvironmentVariable($name,$previous[$name],'Process')}
}

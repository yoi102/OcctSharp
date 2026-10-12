[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InstallRoot,
    [Parameter(Mandatory)][string]$BuildRoot,
    [string]$OutputDirectory = 'artifacts/vcpkg-closure-tests'
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$workspace=Split-Path -Parent $PSScriptRoot
$InstallRoot=(Resolve-Path -LiteralPath $InstallRoot).Path
$BuildRoot=(Resolve-Path -LiteralPath $BuildRoot).Path
$output=[IO.Path]::GetFullPath($OutputDirectory,$workspace)
$artifactPrefix=[IO.Path]::GetFullPath((Join-Path $workspace 'artifacts'))+[IO.Path]::DirectorySeparatorChar
if(-not $output.StartsWith($artifactPrefix,[StringComparison]::OrdinalIgnoreCase)){throw 'Closure test output must stay in artifacts.'}
$run=Join-Path $output ([Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($run)|Out-Null
$settings=Get-Content (Join-Path $workspace 'config/local.settings.json') -Raw|ConvertFrom-Json
$cmake=Join-Path $settings.visualStudioRoot 'Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe'
$toolset=Get-ChildItem (Join-Path $settings.visualStudioRoot 'VC/Tools/MSVC') -Directory|Sort-Object {[version]$_.Name} -Descending|Select-Object -First 1
$dumpbin=Join-Path $toolset.FullName 'bin/Hostx64/x64/dumpbin.exe'
$bin=Join-Path $InstallRoot 'x64-windows/bin'
$bridge=Join-Path $run 'OcctSharp.Native.dll'
Copy-Item -LiteralPath (Join-Path $BuildRoot 'runtime/Release/OcctSharp.Native.dll') -Destination $bridge
$cases=[Collections.Generic.List[string]]::new()
function Copy-Closure([string]$Prefix,[string]$Name){
 & $cmake "-DBRIDGE=$bridge" "-DDEPENDENCY_BIN=$Prefix" "-DOUTPUT=$run" "-DDUMPBIN=$dumpbin" -P (Join-Path $PSScriptRoot 'copy-vcpkg-runtime.cmake') *> (Join-Path $run "$Name.log")
 return $LASTEXITCODE
}
if((Copy-Closure $bin 'fresh') -ne 0){throw 'Fresh exact PE closure copy failed.'}
$cases.Add('fresh exact closure')
if((Copy-Closure $bin 'repeat') -ne 0){throw 'Repeated exact PE closure copy failed.'}
$cases.Add('repeat accepts only exact installed copies')
$files=@(Get-ChildItem -LiteralPath $run -File -Filter '*.dll')
$manifest=Get-Content (Join-Path $BuildRoot 'runtime-manifest-Release.json') -Raw|ConvertFrom-Json
if(@(Compare-Object ($files.Name|Sort-Object) @($manifest.files|ForEach-Object {[IO.Path]::GetFileName($_.path)}|Sort-Object) -CaseSensitive).Count){throw 'Copied closure differs from qualified DLL names.'}
$cases.Add('configuration-specific DLL set')
$empty=Join-Path $run 'empty-prefix';[IO.Directory]::CreateDirectory($empty)|Out-Null
if((Copy-Closure $empty 'wrong-prefix') -eq 0){throw 'An unrelated dependency prefix was accepted.'}
if((Get-Content (Join-Path $run 'wrong-prefix.log') -Raw) -notmatch 'escaped the pinned prefix'){throw 'Wrong-prefix fixture failed for an unrelated reason.'}
$cases.Add('foreign prefix rejected')
$victim=$files|Where-Object Name -CNE 'OcctSharp.Native.dll'|Sort-Object Name|Select-Object -First 1
$bytes=[IO.File]::ReadAllBytes($victim.FullName)
try{
 [IO.File]::WriteAllBytes($victim.FullName,[IO.File]::ReadAllBytes($bridge))
 if((Copy-Closure $bin 'changed-copy') -eq 0){throw 'A replaced copied DLL was accepted.'}
 $message=Get-Content (Join-Path $run 'changed-copy.log') -Raw
 if($message -notmatch 'differs from its pinned port|unresolved=|conflicts='){throw 'Changed-copy fixture failed for an unrelated reason.'}
 $cases.Add('modified copied DLL rejected')
}finally{[IO.File]::WriteAllBytes($victim.FullName,$bytes)}
if((Copy-Closure $bin 'restored') -ne 0){throw 'Restored exact PE closure copy failed.'}
$cases.Add('restored exact closure')
[ordered]@{state='PASS';cases=$cases.ToArray();nativeDlls=$files.Count;fixture=$run}|ConvertTo-Json -Depth 4|Set-Content (Join-Path $output 'result.json') -Encoding utf8
Write-Host "Vcpkg PE closure tests PASS: $($cases.Count) cases."

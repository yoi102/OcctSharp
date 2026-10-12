[CmdletBinding()]
param([string]$OutputDirectory='artifacts/native-bootstrap-tests')
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$workspace=Split-Path -Parent $PSScriptRoot
$output=[IO.Path]::GetFullPath((Join-Path $workspace $OutputDirectory))
[IO.Directory]::CreateDirectory($output)|Out-Null
$shell=(Get-Command powershell.exe).Source
function Start-Probe([string]$Name,[string]$Command){
 $encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes('$ErrorActionPreference=''Stop''; '+$Command))
 Start-Process -FilePath $shell -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-OutputFormat','Text','-EncodedCommand',$encoded) -WorkingDirectory $workspace -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $output "$Name.log") -RedirectStandardError (Join-Path $output "$Name.err")
}
function Wait-Probe($Process,[bool]$Success){
 if(!$Process.WaitForExit(600000)){throw 'Bootstrap probe timeout'}
 $Process.Refresh()
 if(($Process.ExitCode -eq 0) -ne $Success){throw "Unexpected bootstrap probe exit: $($Process.ExitCode)"}
}
$ensure=Join-Path $PSScriptRoot 'ensure-native.ps1'
$bridge=[IO.Path]::GetFullPath((Join-Path $workspace 'artifacts/native/Release/OcctSharp.Native.dll'))
$expectedRoot=[IO.Path]::GetFullPath((Join-Path $workspace 'artifacts/native/Release'))+[IO.Path]::DirectorySeparatorChar
if(!$bridge.StartsWith($expectedRoot,[StringComparison]::OrdinalIgnoreCase)){throw 'Bridge target is outside the intended native output directory'}
$backup=Join-Path $output 'OcctSharp.Native.before.dll'
Copy-Item -LiteralPath $bridge -Destination $backup -Force
try{
 Remove-Item -LiteralPath $bridge -Force
 $first=Start-Probe 'parallel-1' "& '$ensure' -Configuration Release"
 $second=Start-Probe 'parallel-2' "& '$ensure' -Configuration Release"
 Wait-Probe $first $true;Wait-Probe $second $true
 if(!(Test-Path -LiteralPath $bridge)){throw 'Missing bridge was not rebuilt'}
 $events=@()
 foreach($name in @('parallel-1','parallel-2')){
  foreach($line in Get-Content (Join-Path $output "$name.log")){
   if($line -match 'Workspace lock (acquired|released): native PID=(\d+) UTC=(\S+)'){$events+=[pscustomobject]@{action=$Matches[1];pid=[int]$Matches[2];utc=[DateTime]::Parse($Matches[3]).ToUniversalTime()}}
  }
 }
 $events=@($events|Sort-Object utc)
 if($events.Count -ne 4 -or ($events.action -join ',') -ne 'acquired,released,acquired,released'){throw 'Native critical sections overlapped or evidence is incomplete'}
 $failure=Start-Probe 'failure' "& '$ensure' -Configuration Release -OcctRoot '$output/absent-sdk'"
 Wait-Probe $failure $false
 $recovery=Start-Probe 'after-failure' "& '$ensure' -Configuration Debug"
 Wait-Probe $recovery $true
 $report=[ordered]@{state='PASS';missingBridgeRecovered=$true;concurrentProcesses=2;criticalSections=$events;failedBootstrapReleasedLock=$true;debugAfterFailure=$true;bridgeSha256=(Get-FileHash $bridge).Hash}
 [IO.File]::WriteAllText((Join-Path $output 'result.json'),($report|ConvertTo-Json -Depth 5))
 Write-Host 'Native bootstrap PASS: two real Windows PowerShell processes, nonoverlapping critical sections, missing bridge recovery, failure release and subsequent Debug build.'
}finally{
 # Restore only if failure prevented recovery; preserve a successfully rebuilt bridge.
 if(!(Test-Path -LiteralPath $bridge)){Copy-Item -LiteralPath $backup -Destination $bridge}
}

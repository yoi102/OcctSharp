[CmdletBinding()]
param([string]$OutputDirectory='artifacts/package-mutation-tests')
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$workspace=Split-Path -Parent $PSScriptRoot
$repository=Split-Path -Parent $workspace
$output=[IO.Path]::GetFullPath((Join-Path $workspace $OutputDirectory))
[IO.Directory]::CreateDirectory($output)|Out-Null
$version='8.0.1-ao-mutation-'+[Guid]::NewGuid().ToString('N')
$fixture=Join-Path $repository ('docs/AO-mutation-fixture-'+[Guid]::NewGuid().ToString('N')+'.md')
$script=Join-Path $PSScriptRoot 'pack.ps1'
$command='$ErrorActionPreference=''Stop''; & '''+$script.Replace("'","''")+''' -SkipBuild -PackageVersion '''+$version+''''
$encoded=[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))
$log=Join-Path $output 'pack.log'
$process=Start-Process -FilePath (Join-Path $PSHOME 'pwsh.exe') -ArgumentList @('-NoProfile','-EncodedCommand',$encoded) -WorkingDirectory $workspace -WindowStyle Hidden -PassThru -RedirectStandardOutput $log -RedirectStandardError (Join-Path $output 'pack.err')
try{
 $deadline=[DateTime]::UtcNow.AddMinutes(5)
 while(!(Test-Path $log) -or !(Select-String -LiteralPath $log -SimpleMatch 'Package source inputs frozen:' -Quiet)){
  $process.Refresh();if($process.HasExited -or [DateTime]::UtcNow -gt $deadline){throw 'Pack did not establish its snapshot'}
  Start-Sleep -Milliseconds 100
 }
 [IO.File]::WriteAllText($fixture,'This temporary document is deliberately added after the pack snapshot.')
 if(!$process.WaitForExit(600000)){throw 'Mutation fixture pack timed out'}
 $process.Refresh()
 if($process.ExitCode -eq 0){throw 'Concurrent document addition was incorrectly accepted'}
 $errors=Get-Content (Join-Path $output 'pack.err') -Raw
 if($errors -notmatch 'Package inputs changed|Package input path set changed'){throw "Pack failed for an unrelated reason: $errors"}
 $published=@(Get-ChildItem (Join-Path $workspace 'artifacts/packages') -Filter "*.$version.nupkg")
 if($published.Count -ne 0 -or (Test-Path (Join-Path $workspace "artifacts/packages/package-inputs.$version.json"))){throw 'Failed candidates reached the normal package output'}
 $report=[ordered]@{state='PASS';packageVersion=$version;childExitCode=$process.ExitCode;concurrentDocumentAdditionRejected=$true;publishedPackages=0;completionMarkerAbsent=$true}
 [IO.File]::WriteAllText((Join-Path $output 'result.json'),($report|ConvertTo-Json))
 Write-Host 'Real pack mutation PASS: concurrent document addition rejected; zero candidate packages promoted.'
}finally{
 if(!$process.HasExited){$process.Kill($true);$process.WaitForExit()}
 if(Test-Path -LiteralPath $fixture){Remove-Item -LiteralPath $fixture -Force}
}

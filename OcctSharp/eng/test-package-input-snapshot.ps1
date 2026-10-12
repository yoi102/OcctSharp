[CmdletBinding()]
param([string]$OutputDirectory = 'artifacts/package-snapshot-tests')
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$workspace=Split-Path -Parent $PSScriptRoot
$output=[IO.Path]::GetFullPath((Join-Path $workspace $OutputDirectory))
[IO.Directory]::CreateDirectory($output)|Out-Null
if(-not ('OcctSharp.Packaging.PackageInputSnapshot' -as [type])){Add-Type -Path (Join-Path $PSScriptRoot 'PackageInputSnapshot.cs')}
. (Join-Path $PSScriptRoot 'package-snapshot.ps1')
$fixture=Join-Path $output ([Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory((Join-Path $fixture 'docs'))|Out-Null
[IO.Directory]::CreateDirectory((Join-Path $fixture 'OcctSharp/src/Example'))|Out-Null
$document=Join-Path $fixture 'docs/example.md'
$source=Join-Path $fixture 'OcctSharp/src/Example/example.cs'
[IO.File]::WriteAllText($document,'frozen document')
[IO.File]::WriteAllText($source,'class Example {}')
$cases=[Collections.Generic.List[string]]::new()
function Expect-Rejection([scriptblock]$Action,[string]$Name){$rejected=$false;try{& $Action}catch{$rejected=$true};if(!$rejected){throw "Expected rejection: $Name"};$cases.Add($Name)}
$guard=[OcctSharp.Packaging.PackageInputSnapshot]::new($fixture)
try{
 $guard.Validate();$cases.Add('unchanged snapshot')
 Expect-Rejection { [IO.File]::WriteAllText($document,'changed document') } 'existing document write prevented'
 Expect-Rejection { [IO.File]::WriteAllText($source,'class Changed {}') } 'existing source write prevented'
 Expect-Rejection { [IO.File]::Delete($document) } 'existing document delete prevented'
 Expect-Rejection { [IO.File]::Move($document,$document+'.renamed') } 'existing document rename prevented'
 if([OcctSharp.Packaging.PackageInputSnapshot]::IsSourceInput('') -or [OcctSharp.Packaging.PackageInputSnapshot]::IsSourceInput('OcctSharp/artifacts')){throw 'Output or empty rename path was treated as source'}
 $ignored=Join-Path $fixture 'OcctSharp/artifacts';[IO.Directory]::CreateDirectory($ignored)|Out-Null
 [IO.File]::WriteAllText((Join-Path $ignored 'output.tmp'),'ignored')
 [IO.File]::Move((Join-Path $ignored 'output.tmp'),(Join-Path $ignored 'output.bin'))
 Start-Sleep -Milliseconds 150
 $guard.Validate();$cases.Add('ignored build output renames do not invalidate source')
 $guard.Validate();$cases.Add('failed external mutation leaves original snapshot valid')
 $path=Join-Path $fixture 'docs/new.md';[IO.File]::WriteAllText($path,'new input')
 Expect-Rejection { $guard.Validate() } 'new input rejected'
 [IO.File]::Delete($path)
 Start-Sleep -Milliseconds 150
 Expect-Rejection { $guard.Validate() } 'reverted transient input rejected'
}finally{$guard.Dispose()}
[IO.File]::WriteAllText($document,'next epoch');$cases.Add('handles released after disposal')
$guard=[OcctSharp.Packaging.PackageInputSnapshot]::new($fixture)
try{
 $inputs=@{};foreach($row in $guard.Identities){$inputs[$row.Path]=$row.Sha256}
 $zipPath=Join-Path $output 'valid.nupkg'
 $stream=[IO.File]::Open($zipPath,[IO.FileMode]::Create)
 $zip=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
 try{$entry=$zip.CreateEntry('docs/example.md');$writer=[IO.StreamWriter]::new($entry.Open());try{$writer.Write('next epoch')}finally{$writer.Dispose()}}finally{$zip.Dispose();$stream.Dispose()}
 Assert-OcctSharpPackageSnapshot -PackagePath $zipPath -Inputs $inputs -PackageId Fixture -NativeRelativeDirectory 'OcctSharp/artifacts/native/Release'
 $cases.Add('package content matches snapshot')
 $inputs['docs/example.md']='0'*64
 Expect-Rejection { Assert-OcctSharpPackageSnapshot -PackagePath $zipPath -Inputs $inputs -PackageId Fixture -NativeRelativeDirectory 'OcctSharp/artifacts/native/Release' } 'mixed package document rejected'
 $inputs['docs/missing.md']='0'*64
 Expect-Rejection { Assert-OcctSharpPackageSnapshot -PackagePath $zipPath -Inputs $inputs -PackageId OcctSharp -NativeRelativeDirectory 'OcctSharp/artifacts/native/Release' } 'omitted frozen document rejected'
}finally{$guard.Dispose()}
$report=[ordered]@{state='PASS';cases=$cases.ToArray();fixtureDirectory=$fixture}
[IO.File]::WriteAllText((Join-Path $output 'result.json'),($report|ConvertTo-Json -Depth 4))
Write-Host "Package input snapshot PASS: $($cases.Count) positive/negative cases."

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InstallRoot,
    [Parameter(Mandatory)][string]$BuildRoot,
    [ValidateSet('Release','Debug')][string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$InstallRoot = (Resolve-Path -LiteralPath $InstallRoot).Path
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$providerPath = Join-Path $InstallRoot 'occtsharp-provider.json'
$provider = Get-Content -LiteralPath $providerPath -Raw | ConvertFrom-Json
$manifestPath = Join-Path $BuildRoot "runtime-manifest-$Configuration.json"
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ($provider.state -cne 'PASS' -or $manifest.configuration -cne $Configuration -or
    $manifest.provider -cne $provider.provider) { throw 'Provider and native build identities differ.' }
$bin = Join-Path $InstallRoot ($provider.triplet + $(if ($Configuration -ceq 'Debug') {'/debug/bin'} else {'/bin'}))
$root = Join-Path $BuildRoot "runtime/$Configuration"
$out = Join-Path $BuildRoot "runtime-evidence/$Configuration"
[IO.Directory]::CreateDirectory($out) | Out-Null
$licenses = Join-Path $out 'licenses'
[IO.Directory]::CreateDirectory($licenses) | Out-Null
$statusPath = Join-Path $InstallRoot 'vcpkg/status'
if ((Get-FileHash -LiteralPath $statusPath).Hash -cne $provider.statusSha256) { throw 'Installed port status changed after qualification.' }
$packages = @{}
foreach ($paragraph in ((Get-Content -LiteralPath $statusPath -Raw) -replace "`r`n", "`n") -split "`n`n") {
    if ($paragraph -match '(?m)^Feature:' -or $paragraph -notmatch '(?m)^Status: install ok installed$') { continue }
    $fields = @{}
    foreach ($line in $paragraph -split "`n") {
        if ($line -match '^([^:]+): (.*)$') { $fields[$Matches[1]] = $Matches[2] }
    }
    $version = @('Version','Version-Semver','Version-Date','Version-String') | Where-Object { $fields.ContainsKey($_) } | Select-Object -First 1
    if (-not $version) { throw 'Installed port has no version identity.' }
    $packages[$fields['Package']] = [ordered]@{name=$fields['Package'];version=$fields[$version];portVersion=if($fields.ContainsKey('Port-Version')){$fields['Port-Version']}else{'0'}}
}
$owners = @{}
foreach ($list in Get-ChildItem -LiteralPath (Join-Path $InstallRoot 'vcpkg/info') -File -Filter '*.list') {
    $port = ($list.BaseName -split '_')[0]
    foreach ($line in Get-Content -LiteralPath $list.FullName) {
        if ($line -notmatch '\.dll$') { continue }
        $path = [IO.Path]::GetFullPath((Join-Path $InstallRoot $line))
        if ($owners.ContainsKey($path)) { throw "Ambiguous installed DLL owner: $path" }
        $owners[$path] = $port
    }
}
$usedPorts = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$rows = [Collections.Generic.List[object]]::new()
foreach ($file in $manifest.files) {
    $name = [IO.Path]::GetFileName($file.path)
    if ((Get-FileHash -LiteralPath (Join-Path $root $name)).Hash -cne $file.sha256) { throw 'Native runtime manifest changed.' }
    if ($name -ceq 'OcctSharp.Native.dll') { $port='OcctSharp'; $version='0.133.0' }
    else {
        $source = Join-Path $bin $name
        if (-not $owners.ContainsKey($source) -or (Get-FileHash -LiteralPath $source).Hash -cne $file.sha256) {
            throw "Runtime DLL is not an exact installed port file: $name"
        }
        $port = $owners[$source]
        if (-not $packages.ContainsKey($port)) { throw "Installed port status missing: $port" }
        $version = $packages[$port].version
    }
    [void]$usedPorts.Add($port)
    $rows.Add([ordered]@{name=$name;sha256=$file.sha256;port=$port;version=$version})
}
$licenseRows = foreach ($port in @($usedPorts | Sort-Object)) {
    $source = if ($port -ceq 'OcctSharp') { Join-Path (Split-Path -Parent $workspace) 'LICENSE' }
        else { Join-Path $InstallRoot "$($provider.triplet)/share/$port/copyright" }
    if (-not (Test-Path -LiteralPath $source -PathType Leaf) -or (Get-Item -LiteralPath $source).Length -eq 0) { throw "Missing runtime port license: $port" }
    $target = Join-Path $licenses "$port.txt"
    Copy-Item -LiteralPath $source -Destination $target -Force
    $hash = (Get-FileHash -LiteralPath $source).Hash
    if ((Get-FileHash -LiteralPath $target).Hash -cne $hash) { throw 'License copy identity mismatch.' }
    [ordered]@{port=$port;path="licenses/$port.txt";sha256=$hash}
}
[ordered]@{state='PASS';provider=$provider.provider;configuration=$Configuration;registryCommit=$provider.registryCommit;
    providerSha256=(Get-FileHash -LiteralPath $providerPath).Hash;statusSha256=(Get-FileHash -LiteralPath $statusPath).Hash;
    nativeManifestSha256=(Get-FileHash -LiteralPath $manifestPath).Hash;nativeFiles=$rows.ToArray();licenses=@($licenseRows);
    installedPorts=@($packages.Values | Sort-Object name)} | ConvertTo-Json -Depth 7 |
    Set-Content -LiteralPath (Join-Path $out 'provenance.json') -Encoding utf8
Write-Host "Vcpkg runtime provenance/license PASS: $($rows.Count) DLLs, $($licenseRows.Count) licenses."

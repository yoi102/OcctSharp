[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArchivePath,
    [Parameter(Mandatory)][string]$IncludeRoot
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$identity = Get-Content (Join-Path $workspace 'config/dependencies/provider.json') -Raw | ConvertFrom-Json
$supplementPath = Join-Path $workspace 'config/dependencies/generator-header-supplement.json'
$supplement = Get-Content -LiteralPath $supplementPath -Raw | ConvertFrom-Json
if ((Get-FileHash -LiteralPath $ArchivePath -Algorithm SHA512).Hash -cne $identity.sourceArchiveSha512.ToUpperInvariant()) {
    throw 'Generator header source archive differs from the pinned SHA512.'
}
$names = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($header in $supplement.headers) {
    if (-not $names.Add($header.name) -or [IO.Path]::GetFileName($header.name) -cne $header.name -or
        $header.archivePath -cnotmatch '^OCCT-8_0_1/src/[A-Za-z0-9_./-]+$' -or
        $header.archivePath -match '(^|/)\.\.(/|$)' -or
        [IO.Path]::GetFileName($header.archivePath) -cne $header.name -or $header.sha256 -cnotmatch '^[0-9A-F]{64}$') {
        throw 'Invalid generator header supplement entry.'
    }
}
$IncludeRoot = [IO.Path]::GetFullPath($IncludeRoot)
[IO.Directory]::CreateDirectory($IncludeRoot) | Out-Null
$sdkRoot = Split-Path -Parent $IncludeRoot
$extract = Join-Path $sdkRoot ('.header-source-' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($extract) | Out-Null
$list = Join-Path $extract 'headers.txt'
[IO.File]::WriteAllLines($list, [string[]]@($supplement.headers.archivePath), [Text.UTF8Encoding]::new($false))
try {
    & tar.exe -xzf $ArchivePath -C $extract -T $list
    if ($LASTEXITCODE) { throw 'Pinned generator header extraction failed.' }
    foreach ($header in $supplement.headers) {
        $source = Join-Path $extract $header.archivePath
        $target = Join-Path $IncludeRoot $header.name
        if ((Get-FileHash -LiteralPath $source).Hash -cne $header.sha256) { throw "Source header identity mismatch: $($header.name)" }
        if ((Test-Path -LiteralPath $target) -and (Get-FileHash -LiteralPath $target).Hash -cne $header.sha256) {
            throw "Existing supplemented header changed: $($header.name)"
        }
        Copy-Item -LiteralPath $source -Destination $target -Force
    }
    [ordered]@{state='PASS';sourceArchiveSha512=$identity.sourceArchiveSha512;headers=$names.Count;
        supplementSha256=(Get-FileHash -LiteralPath $supplementPath).Hash} | ConvertTo-Json |
        Set-Content -LiteralPath (Join-Path $sdkRoot 'generator-header-supplement.json') -Encoding utf8
} finally {
    $resolved = [IO.Path]::GetFullPath($extract)
    if (-not $resolved.StartsWith([IO.Path]::GetFullPath($sdkRoot).TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase) -or
        -not (Split-Path -Leaf $resolved).StartsWith('.header-source-', [StringComparison]::Ordinal)) { throw 'Unsafe header extraction cleanup path.' }
    Remove-Item -LiteralPath $resolved -Recurse -Force
}
Write-Host "Pinned original generator header supplement PASS: $($names.Count) headers."

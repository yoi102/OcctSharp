[CmdletBinding()]
param(
    [string]$PackageVersion,
    [string]$OutputPath = 'artifacts/release/package-content.json'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $workspaceRoot
if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    [xml]$properties = Get-Content -LiteralPath (Join-Path $workspaceRoot 'Directory.Build.props') -Raw
    $PackageVersion = $properties.Project.PropertyGroup.OcctSharpPackageVersion.InnerText
}
$packages = Join-Path $workspaceRoot 'artifacts/packages'
$runtimeRoot = Join-Path $workspaceRoot 'runtime/win-x64'
$manifest = Get-Content -LiteralPath (Join-Path $runtimeRoot 'runtime-manifest.json') -Raw | ConvertFrom-Json
if ($manifest.packageVersion -cne $PackageVersion) { throw 'Runtime and package versions differ.' }

function Compare-PackagedFile($Archive, [string]$EntryName, [string]$SourcePath) {
    $entry = $Archive.GetEntry($EntryName)
    if ($null -eq $entry) { throw "Missing package content: '$EntryName'." }
    $stream = $entry.Open()
    try { $actual = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)) }
    finally { $stream.Dispose() }
    if ($actual -cne (Get-FileHash -LiteralPath $SourcePath -Algorithm SHA256).Hash) {
        throw "Stale or changed package content: '$EntryName'. Repack the final source documents/runtime."
    }
}

$facadePath = Join-Path $packages "OcctSharp.$PackageVersion.nupkg"
$facade = [IO.Compression.ZipFile]::OpenRead($facadePath)
try {
    Compare-PackagedFile $facade 'README.md' (Join-Path $repositoryRoot 'README.md')
    Compare-PackagedFile $facade 'LICENSE' (Join-Path $repositoryRoot 'LICENSE')
    $docsRoot = Join-Path $repositoryRoot 'docs'
    $documents = @(Get-ChildItem -LiteralPath $docsRoot -Recurse -File -Filter '*.md' |
        Where-Object FullName -NE (Join-Path $docsRoot 'STATUS.md'))
    $expectedDocuments = @($documents | ForEach-Object {
        'docs/' + [IO.Path]::GetRelativePath($docsRoot, $_.FullName).Replace('\', '/')
    })
    $packagedDocuments = @($facade.Entries | Where-Object FullName -Like 'docs/*.md' | ForEach-Object FullName)
    if (@(Compare-Object $expectedDocuments $packagedDocuments -CaseSensitive).Count) {
        throw 'The facade documentation set differs from the complete stable source set (STATUS excluded).'
    }
    foreach ($document in $documents) {
        $entryName = 'docs/' + [IO.Path]::GetRelativePath($docsRoot, $document.FullName).Replace('\', '/')
        Compare-PackagedFile $facade $entryName $document.FullName
    }
}
finally { $facade.Dispose() }

$nativePath = Join-Path $packages "OcctSharp.Native.win-x64.$PackageVersion.nupkg"
$native = [IO.Compression.ZipFile]::OpenRead($nativePath)
try {
    Compare-PackagedFile $native 'README.md' (Join-Path $repositoryRoot 'README.md')
    foreach ($file in $manifest.files) {
        $entryName = if ($file.path.StartsWith('occt/', [StringComparison]::Ordinal)) {
            'buildTransitive/win-x64/' + $file.path
        } elseif ($file.path -ceq 'THIRD_PARTY_NOTICES.md') {
            'licenses/THIRD_PARTY_NOTICES.md'
        } else { $file.path }
        $sourcePath = Join-Path $runtimeRoot $file.path
        if ((Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash -cne $file.sha256) {
            throw "Runtime source no longer matches its manifest: '$($file.path)'."
        }
        Compare-PackagedFile $native $entryName $sourcePath
    }
}
finally { $native.Dispose() }

$releaseRoot = Join-Path $workspaceRoot 'artifacts/release'
$checksumPaths = [Collections.Generic.Dictionary[string, string]]::new([StringComparer]::Ordinal)
foreach ($name in @('api-diff.json', 'provenance.json', 'release-gates.json', 'sbom.cdx.json')) {
    $checksumPaths.Add($name, (Join-Path $releaseRoot $name))
}
$checksumPaths.Add([IO.Path]::GetFileName($facadePath), $facadePath)
$seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($line in Get-Content -LiteralPath (Join-Path $releaseRoot 'checksums.sha256')) {
    if ($line -cnotmatch '^([0-9A-F]{64})  (.+)$') { throw 'Malformed release checksum.' }
    $hash = $Matches[1]; $name = $Matches[2]
    if (-not $checksumPaths.ContainsKey($name) -or -not $seen.Add($name) -or
        (Get-FileHash -LiteralPath $checksumPaths[$name] -Algorithm SHA256).Hash -cne $hash) {
        throw "Unexpected, duplicate or stale release checksum: '$name'."
    }
}
if ($seen.Count -ne $checksumPaths.Count) { throw 'The release checksum set is incomplete.' }
$provenance = Get-Content -LiteralPath (Join-Path $releaseRoot 'provenance.json') -Raw | ConvertFrom-Json
$facadeHash = (Get-FileHash -LiteralPath $facadePath -Algorithm SHA256).Hash
if ($provenance.subject.name -cne [IO.Path]::GetFileName($facadePath) -or $provenance.subject.sha256 -cne $facadeHash) {
    throw 'Release provenance does not identify the final facade package.'
}
$report = [ordered]@{
    state = 'PASS'; packageVersion = $PackageVersion; stableDocuments = $documents.Count
    runtimeFiles = $manifest.files.Count; releaseChecksums = $seen.Count; facadePackageSha256 = $facadeHash
}
$outputFile = [IO.Path]::GetFullPath($OutputPath, $workspaceRoot)
# Evidence output must stay in the artifact tree and must not replace a checked input.
$artifactPrefix = [IO.Path]::GetFullPath((Join-Path $workspaceRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $outputFile.StartsWith($artifactPrefix, [StringComparison]::OrdinalIgnoreCase) -or
    $outputFile -in @($checksumPaths.Values) -or $outputFile -in @($nativePath, (Join-Path $releaseRoot 'checksums.sha256'))) {
    throw 'Package-content evidence must be a separate artifact output.'
}
[IO.Directory]::CreateDirectory((Split-Path -Parent $outputFile)) | Out-Null
[IO.File]::WriteAllText($outputFile, ($report | ConvertTo-Json) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
Write-Host "Package content PASS: $($documents.Count) stable documents, $($manifest.files.Count) runtime/license files, five checksums and final provenance."

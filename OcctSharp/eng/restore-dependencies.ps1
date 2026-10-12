[CmdletBinding()]
param(
    [string]$VcpkgRoot = (Join-Path $env:LOCALAPPDATA 'OcctSharp/dependencies/vcpkg-e456309'),
    [string]$InstallRoot,
    [switch]$RequireEmpty,
    [switch]$NoBinaryCache,
    [switch]$UseCurlDownloads,
    [int]$Concurrency = 4
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$repository = Split-Path -Parent $workspace
$manifest = Join-Path $workspace 'config/dependencies'
$identity = Get-Content (Join-Path $manifest 'provider.json') -Raw | ConvertFrom-Json
$VcpkgRoot = [IO.Path]::GetFullPath($VcpkgRoot)
if ($VcpkgRoot.StartsWith($repository.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The vcpkg Git checkout must be outside the OcctSharp repository.'
}
if ([string]::IsNullOrWhiteSpace($InstallRoot)) { $InstallRoot = Join-Path $workspace 'artifacts/dependencies/vcpkg-installed' }
$InstallRoot = [IO.Path]::GetFullPath($InstallRoot)
if ($RequireEmpty -and (Test-Path -LiteralPath $InstallRoot) -and @(Get-ChildItem -LiteralPath $InstallRoot -Force).Count) {
    throw 'A clean recovery requires an empty install root; choose a new path.'
}
if ($Concurrency -lt 1) { throw 'Concurrency must be positive.' }
if (-not (Test-Path -LiteralPath (Join-Path $VcpkgRoot '.git'))) {
    if ((Test-Path -LiteralPath $VcpkgRoot) -and @(Get-ChildItem -LiteralPath $VcpkgRoot -Force).Count) { throw 'VcpkgRoot is not an empty checkout destination.' }
    & git clone --filter=blob:none --no-checkout https://github.com/microsoft/vcpkg.git $VcpkgRoot
    if ($LASTEXITCODE) { throw 'vcpkg registry clone failed.' }
    & git -C $VcpkgRoot checkout --detach $identity.registryCommit
    if ($LASTEXITCODE) { throw 'Pinned vcpkg checkout failed.' }
}
$head = (& git -C $VcpkgRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -or $head -cne $identity.registryCommit) { throw 'vcpkg checkout does not match the pinned registry commit.' }
if (@(& git -C $VcpkgRoot status --porcelain --untracked-files=no).Count) { throw 'vcpkg registry contains tracked changes.' }
$executable = Join-Path $VcpkgRoot 'vcpkg.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    & (Join-Path $VcpkgRoot 'bootstrap-vcpkg.bat') -disableMetrics
    if ($LASTEXITCODE) { throw 'Pinned vcpkg bootstrap failed.' }
}
if ((Get-FileHash -LiteralPath $executable).Hash -cne $identity.toolSha256 -or
    ((& $executable version | Out-String) -notmatch [regex]::Escape($identity.toolVersion))) {
    throw 'The vcpkg executable does not match the pinned, signature-verified tool release.'
}
$previousCache = $env:VCPKG_BINARY_SOURCES
$previousConcurrency = $env:VCPKG_MAX_CONCURRENCY
$previousAssets = $env:X_VCPKG_ASSET_SOURCES
try {
    if ($NoBinaryCache) { $env:VCPKG_BINARY_SOURCES = 'clear' }
    $env:VCPKG_MAX_CONCURRENCY = [string]$Concurrency
    if ($UseCurlDownloads) {
        $env:X_VCPKG_ASSET_SOURCES = 'clear;x-script,curl.exe --fail --location --connect-timeout 20 --max-time 1800 --retry 2 "{url}" --output "{dst}"'
    }
    & $executable install "--vcpkg-root=$VcpkgRoot" "--x-manifest-root=$manifest" "--x-install-root=$InstallRoot" "--triplet=$($identity.triplet)" "--host-triplet=$($identity.triplet)" --disable-metrics
    if ($LASTEXITCODE) { throw "Pinned dependency restore failed: $LASTEXITCODE" }
    $status = Join-Path $InstallRoot 'vcpkg/status'
    if (-not (Test-Path -LiteralPath $status)) { throw 'Dependency status database is missing.' }
    # The generator consumes the original SDK layout. The overlay retains header
    # bytes; this local view changes their directory name, not their contents.
    $sdkView = Join-Path $InstallRoot 'occtsharp-sdk'
    $sdkIncludes = Join-Path $sdkView 'inc'
    $sourceIncludes = Join-Path $InstallRoot "$($identity.triplet)/include/opencascade"
    $supplement = Get-Content (Join-Path $manifest 'generator-header-supplement.json') -Raw | ConvertFrom-Json
    $sourceNames = @(Get-ChildItem -LiteralPath $sourceIncludes -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($sourceIncludes,$_.FullName) } | Sort-Object)
    if (@($supplement.headers.name | Where-Object { $_ -cin $sourceNames }).Count) { throw 'Supplemented headers unexpectedly occur in the core port.' }
    $fullNames = @(@($sourceNames) + @($supplement.headers.name) | Sort-Object)
    if ((Test-Path -LiteralPath $sdkIncludes) -and @(Get-ChildItem -LiteralPath $sdkIncludes -Recurse -File).Count) {
        $viewNames = @(Get-ChildItem -LiteralPath $sdkIncludes -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($sdkIncludes,$_.FullName) } | Sort-Object)
        # Allow the original core-only view to be upgraded once. No unknown or
        # partial header set is accepted; final qualification requires fullNames.
        if (@(Compare-Object $sourceNames $viewNames -CaseSensitive).Count -and
            @(Compare-Object $fullNames $viewNames -CaseSensitive).Count) { throw 'SDK view has stale header names; recover into a fresh install root.' }
        foreach ($name in $sourceNames) {
            if ((Get-FileHash -LiteralPath (Join-Path $sourceIncludes $name)).Hash -cne (Get-FileHash -LiteralPath (Join-Path $sdkIncludes $name)).Hash) {
                throw "SDK view contains changed header bytes: $name"
            }
        }
    }
    [IO.Directory]::CreateDirectory($sdkIncludes) | Out-Null
    Copy-Item -Path (Join-Path $InstallRoot "$($identity.triplet)/include/opencascade/*") -Destination $sdkIncludes -Recurse -Force
    $downloadRoot = if ([string]::IsNullOrWhiteSpace($env:VCPKG_DOWNLOADS)) { Join-Path $VcpkgRoot 'downloads' } else { $env:VCPKG_DOWNLOADS }
    & (Join-Path $PSScriptRoot 'restore-generator-headers.ps1') -ArchivePath (Join-Path $downloadRoot $supplement.sourceArchive) -IncludeRoot $sdkIncludes
    $viewNames = @(Get-ChildItem -LiteralPath $sdkIncludes -Recurse -File | ForEach-Object { [IO.Path]::GetRelativePath($sdkIncludes,$_.FullName) } | Sort-Object)
    if (@(Compare-Object $fullNames $viewNames -CaseSensitive).Count) { throw 'The complete generator SDK header view is inconsistent.' }
    $headerManifestPath = Join-Path $sdkView 'header-manifest.json'
    [ordered]@{state='PASS';headers=@($viewNames | ForEach-Object {
        [ordered]@{path=$_;sha256=(Get-FileHash -LiteralPath (Join-Path $sdkIncludes $_)).Hash}
    })} | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $headerManifestPath -Encoding utf8
    $inputs = @(Get-ChildItem -LiteralPath $manifest -Recurse -File | Sort-Object FullName | ForEach-Object {
        [ordered]@{ path=[IO.Path]::GetRelativePath($manifest, $_.FullName).Replace('\','/'); sha256=(Get-FileHash -LiteralPath $_.FullName).Hash }
    })
    $report = [ordered]@{ state='PASS'; provider=$identity.provider; registryCommit=$head; triplet=$identity.triplet;
        installRoot=$InstallRoot; sdkView=$sdkView; vcpkgRoot=$VcpkgRoot; vcpkgVersion=(& $executable version | Out-String).Trim();
        statusSha256=(Get-FileHash -LiteralPath $status).Hash; sdkHeaders=$viewNames.Count; supplementedHeaders=$supplement.headers.Count;
        sdkHeaderManifestSha256=(Get-FileHash -LiteralPath $headerManifestPath).Hash;
        requiredEmpty=[bool]$RequireEmpty; binaryCacheDisabled=[bool]$NoBinaryCache;
        recordedUtc=[DateTimeOffset]::UtcNow.ToString('o'); inputs=$inputs }
    $report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $InstallRoot 'occtsharp-provider.json') -Encoding utf8
    Write-Host "Pinned vcpkg restore PASS: $InstallRoot"
} finally {
    $env:VCPKG_BINARY_SOURCES = $previousCache
    $env:VCPKG_MAX_CONCURRENCY = $previousConcurrency
    $env:X_VCPKG_ASSET_SOURCES = $previousAssets
}

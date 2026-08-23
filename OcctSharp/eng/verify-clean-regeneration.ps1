[CmdletBinding()]
param([string]$OcctRoot)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $workspaceRoot
$settingsPath = Join-Path $workspaceRoot 'config\local.settings.json'
if ([string]::IsNullOrWhiteSpace($OcctRoot) -and (Test-Path -LiteralPath $settingsPath)) {
    $OcctRoot = (Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json).occtRoot
}
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = [Environment]::GetEnvironmentVariable('OCCTSHARP_OCCT_ROOT')
}
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw 'OCCT root is required.'
}
$resolvedOcctRoot = (Resolve-Path -LiteralPath $OcctRoot).Path

$temporaryBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$temporaryRoot = Join-Path $temporaryBase "OcctSharp-clean-$([Guid]::NewGuid().ToString('N'))"
[IO.Directory]::CreateDirectory($temporaryRoot) | Out-Null
try {
    $files = @(& git -C $repositoryRoot ls-files --cached --others --exclude-standard)
    if ($LASTEXITCODE -ne 0 -or $files.Count -eq 0) {
        throw 'Unable to enumerate the repository source set for clean regeneration.'
    }
    foreach ($relativePath in $files) {
        $sourcePath = Join-Path $repositoryRoot $relativePath
        if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) { continue }
        $destinationPath = Join-Path $temporaryRoot $relativePath
        [IO.Directory]::CreateDirectory((Split-Path -Parent $destinationPath)) | Out-Null
        Copy-Item -LiteralPath $sourcePath -Destination $destinationPath
    }

    $temporaryWorkspace = Join-Path $temporaryRoot 'OcctSharp'
    & (Join-Path $temporaryWorkspace 'eng\build.ps1') -Configuration Release -OcctRoot $resolvedOcctRoot
    if ($LASTEXITCODE -ne 0) { throw "Clean regeneration build failed with exit code $LASTEXITCODE." }

    $sourceManifest = Get-Content -LiteralPath (Join-Path $workspaceRoot 'generated\manifest.json') -Raw | ConvertFrom-Json
    $temporaryManifestPath = Join-Path $temporaryWorkspace 'generated\manifest.json'
    $temporaryManifest = Get-Content -LiteralPath $temporaryManifestPath -Raw | ConvertFrom-Json
    if ((Get-FileHash -LiteralPath (Join-Path $workspaceRoot 'generated\manifest.json') -Algorithm SHA256).Hash -ne
        (Get-FileHash -LiteralPath $temporaryManifestPath -Algorithm SHA256).Hash) {
        throw 'Clean regeneration produced a different generated manifest.'
    }
    foreach ($file in $sourceManifest.files) {
        $sourcePath = Join-Path $workspaceRoot $file.relativePath
        $temporaryPath = Join-Path $temporaryWorkspace $file.relativePath
        if (-not (Test-Path -LiteralPath $temporaryPath) -or
            (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath $temporaryPath -Algorithm SHA256).Hash) {
            throw "Clean regeneration differs for '$($file.relativePath)'."
        }
    }
    if ($temporaryManifest.files.Count -ne $sourceManifest.files.Count) {
        throw 'Clean regeneration changed the generated file count.'
    }

    Write-Host "Clean source copy built successfully; $($sourceManifest.files.Count) generated files are byte-identical."
}
finally {
    $resolvedTemporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
    if ($resolvedTemporaryRoot.StartsWith($temporaryBase, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedTemporaryRoot).StartsWith('OcctSharp-clean-', [StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

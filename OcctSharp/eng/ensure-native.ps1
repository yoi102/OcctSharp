[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$OcctRoot,

    [string]$VisualStudioRoot,

    [string]$ArtifactUrl,

    [string]$ArtifactSha256
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
$settingsPath = Join-Path $workspaceRoot 'config\local.settings.json'
$settings = if (Test-Path -LiteralPath $settingsPath) {
    Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
} else {
    $null
}

function Get-LocalSetting {
    param([Parameter(Mandatory)][string]$Name)

    if ($null -eq $settings) {
        return $null
    }

    $property = $settings.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return [string]$property.Value
}

function Get-Sha256 {
    param([Parameter(Mandatory)][string]$Path)

    $stream = [IO.File]::OpenRead($Path)
    try {
        $algorithm = [Security.Cryptography.SHA256]::Create()
        try {
            return ([BitConverter]::ToString($algorithm.ComputeHash($stream))).Replace('-', '')
        }
        finally {
            $algorithm.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = [Environment]::GetEnvironmentVariable('OCCTSHARP_OCCT_ROOT')
}
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = Get-LocalSetting -Name 'occtRoot'
}

if ([string]::IsNullOrWhiteSpace($ArtifactUrl)) {
    $ArtifactUrl = [Environment]::GetEnvironmentVariable('OCCTSHARP_OCCT_ARTIFACT_URL')
}
if ([string]::IsNullOrWhiteSpace($ArtifactUrl)) {
    $ArtifactUrl = [Environment]::GetEnvironmentVariable('OCCT_ARTIFACT_URL')
}
if ([string]::IsNullOrWhiteSpace($ArtifactUrl)) {
    $ArtifactUrl = Get-LocalSetting -Name 'occtArtifactUrl'
}

if ([string]::IsNullOrWhiteSpace($ArtifactSha256)) {
    $ArtifactSha256 = [Environment]::GetEnvironmentVariable('OCCTSHARP_OCCT_ARTIFACT_SHA256')
}
if ([string]::IsNullOrWhiteSpace($ArtifactSha256)) {
    $ArtifactSha256 = [Environment]::GetEnvironmentVariable('OCCT_ARTIFACT_SHA256')
}
if ([string]::IsNullOrWhiteSpace($ArtifactSha256)) {
    $ArtifactSha256 = Get-LocalSetting -Name 'occtArtifactSha256'
}

if ([string]::IsNullOrWhiteSpace($OcctRoot) -and -not [string]::IsNullOrWhiteSpace($ArtifactUrl)) {
    if ([string]::IsNullOrWhiteSpace($ArtifactSha256)) {
        throw 'An OCCT artifact URL requires an immutable SHA256. Set OCCTSHARP_OCCT_ARTIFACT_SHA256 or occtArtifactSha256.'
    }

    $normalizedSha256 = $ArtifactSha256.Trim().ToUpperInvariant()
    if ($normalizedSha256 -notmatch '^[0-9A-F]{64}$') {
        throw "The OCCT artifact SHA256 '$ArtifactSha256' is not a 64-character hexadecimal hash."
    }

    $artifactUri = [Uri]$ArtifactUrl
    if (-not $artifactUri.IsAbsoluteUri -or $artifactUri.Scheme -ne 'https') {
        throw 'The OCCT artifact URL must be an absolute HTTPS URL.'
    }

    $dependencyDirectory = Join-Path $workspaceRoot 'artifacts\dependencies'
    [IO.Directory]::CreateDirectory($dependencyDirectory) | Out-Null
    $archivePath = Join-Path $dependencyDirectory "occt-$normalizedSha256.zip"
    if (-not (Test-Path -LiteralPath $archivePath -PathType Leaf)) {
        $downloadPath = Join-Path $dependencyDirectory "occt-$normalizedSha256.download"
        Write-Host "Downloading the pinned OCCT artifact from '$ArtifactUrl'."
        Invoke-WebRequest -UseBasicParsing -Uri $artifactUri -OutFile $downloadPath
        $downloadHash = Get-Sha256 -Path $downloadPath
        if ($downloadHash -ne $normalizedSha256) {
            throw "The downloaded OCCT artifact SHA256 is '$downloadHash', expected '$normalizedSha256'."
        }
        Move-Item -LiteralPath $downloadPath -Destination $archivePath
    }

    $archiveHash = Get-Sha256 -Path $archivePath
    if ($archiveHash -ne $normalizedSha256) {
        throw "The cached OCCT artifact SHA256 is '$archiveHash', expected '$normalizedSha256'."
    }

    $extractionDirectory = Join-Path $dependencyDirectory "occt-$normalizedSha256"
    $versionHeader = Get-ChildItem -LiteralPath $extractionDirectory -Recurse -Filter Standard_Version.hxx -File -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($null -eq $versionHeader) {
        if (Test-Path -LiteralPath $extractionDirectory) {
            throw "The cached OCCT extraction at '$extractionDirectory' is incomplete. Remove that directory and retry."
        }

        $stagingDirectory = Join-Path $dependencyDirectory ("extract-" + [Guid]::NewGuid().ToString('N'))
        [IO.Directory]::CreateDirectory($stagingDirectory) | Out-Null
        Expand-Archive -LiteralPath $archivePath -DestinationPath $stagingDirectory
        $versionHeader = Get-ChildItem -LiteralPath $stagingDirectory -Recurse -Filter Standard_Version.hxx -File |
            Select-Object -First 1
        if ($null -eq $versionHeader) {
            throw "Standard_Version.hxx was not found in '$archivePath'."
        }

        Move-Item -LiteralPath $stagingDirectory -Destination $extractionDirectory
        $versionHeader = Get-ChildItem -LiteralPath $extractionDirectory -Recurse -Filter Standard_Version.hxx -File |
            Select-Object -First 1
    }

    $OcctRoot = $versionHeader.Directory.Parent.FullName
}

if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw @'
OCCT 8.0.1 is required to build the native bridge for repository projects.
Set OCCTSHARP_OCCT_ROOT, or copy config/local.settings.example.json to
config/local.settings.json and set occtRoot. Alternatively configure both
OCCTSHARP_OCCT_ARTIFACT_URL and OCCTSHARP_OCCT_ARTIFACT_SHA256 for a pinned archive.
NuGet package consumers do not need an OCCT SDK because the package includes occt/.
'@
}

$resolvedOcctRoot = (Resolve-Path -LiteralPath $OcctRoot).Path
$dependencyManifestPath = Join-Path $workspaceRoot 'config\occt-8.0.1-windows-x64.json'
$dependencyManifest = Get-Content -LiteralPath $dependencyManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($relativePath in $dependencyManifest.requiredRelativePaths) {
    $requiredPath = Join-Path $resolvedOcctRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "The OCCT baseline is missing required path '$relativePath' below '$resolvedOcctRoot'."
    }
}

foreach ($hashEntry in $dependencyManifest.verificationHashes.PSObject.Properties) {
    $hashPath = Join-Path $resolvedOcctRoot ($hashEntry.Name -replace '/', [IO.Path]::DirectorySeparatorChar)
    $actualHash = Get-Sha256 -Path $hashPath
    if ($actualHash -ne $hashEntry.Value) {
        throw "The OCCT baseline hash for '$($hashEntry.Name)' is '$actualHash', expected '$($hashEntry.Value)'."
    }
}

if ([string]::IsNullOrWhiteSpace($VisualStudioRoot)) {
    $VisualStudioRoot = [Environment]::GetEnvironmentVariable('OCCTSHARP_VISUAL_STUDIO_ROOT')
}
if ([string]::IsNullOrWhiteSpace($VisualStudioRoot)) {
    $VisualStudioRoot = Get-LocalSetting -Name 'visualStudioRoot'
}
if ([string]::IsNullOrWhiteSpace($VisualStudioRoot)) {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw 'Visual Studio Installer vswhere.exe was not found. Install the Desktop development with C++ workload or set OCCTSHARP_VISUAL_STUDIO_ROOT.'
    }

    $VisualStudioRoot = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
}

$resolvedVisualStudioRoot = (Resolve-Path -LiteralPath $VisualStudioRoot).Path
$cmake = Join-Path $resolvedVisualStudioRoot 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
if (-not (Test-Path -LiteralPath $cmake)) {
    throw "Visual Studio CMake was not found at '$cmake'."
}

$env:OCCTSHARP_OCCT_ROOT = $resolvedOcctRoot
Push-Location $workspaceRoot
try {
    Write-Host "Configuring the $Configuration native bridge with OCCT at '$resolvedOcctRoot'."
    & $cmake --preset windows-x64-local
    if ($LASTEXITCODE -ne 0) {
        throw "CMake configure failed with exit code $LASTEXITCODE."
    }

    & $cmake --build --preset $Configuration.ToLowerInvariant()
    if ($LASTEXITCODE -ne 0) {
        throw "CMake build failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}

$nativeDirectory = Join-Path $workspaceRoot "artifacts\native\$Configuration"
$bridgePath = Join-Path $nativeDirectory 'OcctSharp.Native.dll'
if (-not (Test-Path -LiteralPath $bridgePath -PathType Leaf)) {
    throw "The native build completed without producing '$bridgePath'."
}

$runtimeCount = @(Get-ChildItem -LiteralPath $nativeDirectory -Filter *.dll -File).Count
Write-Host "Native runtime ready: '$nativeDirectory' ($runtimeCount DLLs)."

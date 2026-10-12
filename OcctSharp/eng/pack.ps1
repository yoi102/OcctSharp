[CmdletBinding()]
param(
    [string]$OcctRoot,

    [string]$PackageVersion = '8.0.1-preview.85',

    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'workspace-lock.ps1')
. (Join-Path $PSScriptRoot 'package-snapshot.ps1')
$packageLock = Enter-OcctSharpWorkspaceLock -WorkspaceRoot $workspaceRoot -Scope package
try {
$settingsPath = Join-Path $workspaceRoot 'config\local.settings.json'

if (-not $OcctRoot -and (Test-Path -LiteralPath $settingsPath)) {
    $settings = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $OcctRoot = $settings.occtRoot
}

$resolvedOcctRoot = if ($OcctRoot) { (Resolve-Path -LiteralPath $OcctRoot).Path } else { $null }
$builtRuntimeDirectory = Join-Path $workspaceRoot 'artifacts\native\Release'
$bundledRuntimeDirectory = Join-Path $workspaceRoot 'runtime\win-x64\occt'
$nativeRuntimeDirectory = if (Test-Path -LiteralPath (Join-Path $builtRuntimeDirectory 'OcctSharp.Native.dll')) {
    $builtRuntimeDirectory
}
else {
    $bundledRuntimeDirectory
}
$packageDirectory = Join-Path $workspaceRoot 'artifacts\packages'

if (-not $SkipBuild) {
    if (-not $resolvedOcctRoot) {
        throw 'A native rebuild requires an OCCT root. Pass -OcctRoot or configure config/local.settings.json; use -SkipBuild to package the committed runtime.'
    }
    & (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release -OcctRoot $resolvedOcctRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed with exit code $LASTEXITCODE."
    }
    $nativeRuntimeDirectory = $builtRuntimeDirectory
}

& (Join-Path $PSScriptRoot 'verify-bundled-runtime.ps1')

$nativeFiles = @(Get-ChildItem -LiteralPath $nativeRuntimeDirectory -File -Filter '*.dll')
if ($nativeFiles.Count -eq 0) {
    throw "No native runtime DLLs were found at '$nativeRuntimeDirectory'."
}


New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null
$repositoryRoot = Split-Path -Parent $workspaceRoot
$stagingDirectory = Join-Path $workspaceRoot ('artifacts/package-staging/' + [Guid]::NewGuid().ToString('N'))
[IO.Directory]::CreateDirectory($stagingDirectory) | Out-Null
if (-not ('OcctSharp.Packaging.PackageInputSnapshot' -as [type])) {
    Add-Type -Path (Join-Path $PSScriptRoot 'PackageInputSnapshot.cs')
}
$snapshot = [OcctSharp.Packaging.PackageInputSnapshot]::new($repositoryRoot)
Push-Location $workspaceRoot
try {
    $packProjects = @(
        '.\src\OcctSharp.Native.win-x64\OcctSharp.Native.win-x64.csproj',
        '.\src\OcctSharp.Runtime\OcctSharp.Runtime.csproj',
        '.\src\OcctSharp.Foundation\OcctSharp.Foundation.csproj',
        '.\src\OcctSharp.Geometry\OcctSharp.Geometry.csproj',
        '.\src\OcctSharp.MeshData\OcctSharp.MeshData.csproj',
        '.\src\OcctSharp.Modeling\OcctSharp.Modeling.csproj',
        '.\src\OcctSharp.Mesh\OcctSharp.Mesh.csproj',
        '.\src\OcctSharp.Documents\OcctSharp.Documents.csproj',
        '.\src\OcctSharp.Visualization\OcctSharp.Visualization.csproj',
        '.\src\OcctSharp.DataExchange\OcctSharp.DataExchange.csproj',
        '.\src\OcctSharp.Xde\OcctSharp.Xde.csproj',
        '.\src\OcctSharp.IVtk\OcctSharp.IVtk.csproj',
        '.\src\OcctSharp.Draw\OcctSharp.Draw.csproj',
        '.\src\OcctSharp\OcctSharp.csproj'
    )
    Write-Host "Package source inputs frozen: $stagingDirectory"
    # SkipBuild continues to skip the native rebuild, not the managed refresh that
    # the old pack entry point performed. Compile once, then pack without rebuilding.
    & dotnet build .\src\OcctSharp\OcctSharp.csproj -c Release "-p:OcctSharpPackageVersion=$PackageVersion" "-p:OcctSharpNativeRuntimeDir=$nativeRuntimeDirectory" -p:OcctSharpUseBundledNativeRuntime=true
    if ($LASTEXITCODE -ne 0) { throw 'Managed package refresh failed.' }
    $snapshot.Validate()
    $payloadFiles = @($nativeFiles.FullName)
    foreach ($project in $packProjects) {
        $directory = Join-Path (Split-Path -Parent $project) 'bin/Release/net10.0/win-x64'
        $payloadFiles += @(Get-ChildItem -LiteralPath $directory -File | Where-Object Extension -in @('.dll', '.xml', '.pdb', '.json') | ForEach-Object FullName)
    }
    $snapshot.AddFiles([string[]]$payloadFiles)
    $identities = @($snapshot.Identities)
    $inputs = @{}
    foreach ($identity in $identities) { $inputs[$identity.Path] = $identity.Sha256 }
    $inputPath = Join-Path $stagingDirectory 'inputs.json'
    [IO.File]::WriteAllText($inputPath, ($identities | ConvertTo-Json -Depth 4) + [Environment]::NewLine)
    foreach ($packProject in $packProjects) {
        $snapshot.CheckEvents()
        & dotnet pack $packProject --configuration Release --no-build --no-restore --output $stagingDirectory `
            "-p:PackageVersion=$PackageVersion" "-p:OcctSharpPackageVersion=$PackageVersion" "-p:OcctSharpNativeRuntimeDir=$nativeRuntimeDirectory"
        if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed for '$packProject' with exit code $LASTEXITCODE." }
    }
$packageIds = @(
    'OcctSharp.Native.win-x64', 'OcctSharp.Runtime', 'OcctSharp.Foundation',
    'OcctSharp.Geometry', 'OcctSharp.MeshData', 'OcctSharp.Modeling',
    'OcctSharp.Mesh', 'OcctSharp.Documents', 'OcctSharp.Visualization',
    'OcctSharp.DataExchange', 'OcctSharp.Xde', 'OcctSharp.IVtk',
    'OcctSharp.Draw', 'OcctSharp'
)

    $snapshot.Validate()
    $nativeRelative = [IO.Path]::GetRelativePath($repositoryRoot, $nativeRuntimeDirectory).Replace('\','/')
    $packages = @()
    foreach ($packageId in $packageIds) {
        $candidate = Join-Path $stagingDirectory "$packageId.$PackageVersion.nupkg"
        if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) { throw "Missing candidate package: $candidate" }
        Assert-OcctSharpPackageSnapshot -PackagePath $candidate -Inputs $inputs -PackageId $packageId -NativeRelativeDirectory $nativeRelative
        $packages += [ordered]@{ name = [IO.Path]::GetFileName($candidate); sha256 = (Get-FileHash -LiteralPath $candidate).Hash }
    }
    $snapshot.Validate()
    # Only validated candidates reach the normal package directory. The manifest
    # is the completion marker and binds the complete set, even after interruption.
    foreach ($package in $packages) {
        Copy-Item -LiteralPath (Join-Path $stagingDirectory $package.name) -Destination (Join-Path $packageDirectory $package.name) -Force
        if ((Get-FileHash -LiteralPath (Join-Path $packageDirectory $package.name)).Hash -cne $package.sha256) { throw 'Package promotion identity mismatch.' }
    }
    $snapshot.Validate()
    $report = [ordered]@{ schemaVersion='1.0'; state='PASS'; packageVersion=$PackageVersion;
        sourceInputs=$identities; packages=$packages; stagingDirectory=$stagingDirectory;
        sourceInputManifestSha256=(Get-FileHash -LiteralPath $inputPath).Hash; nativeDlls=$nativeFiles.Count }
    $reportPath = Join-Path $packageDirectory "package-inputs.$PackageVersion.json"
    [IO.File]::WriteAllText($reportPath, ($report | ConvertTo-Json -Depth 6) + [Environment]::NewLine)
    Write-Host "Package snapshot PASS: $($identities.Count) frozen inputs, $($packageIds.Count) packages; $reportPath"
    Write-Host "Created $($packageIds.Count) packages; the shared native package contains $($nativeFiles.Count) runtime DLLs."
}
catch {
    [IO.File]::WriteAllText((Join-Path $stagingDirectory 'FAILED.txt'), $_.ToString())
    throw
}
finally {
    Pop-Location
    $snapshot.Dispose()
}
}
finally { Exit-OcctSharpWorkspaceLock -Lock $packageLock }

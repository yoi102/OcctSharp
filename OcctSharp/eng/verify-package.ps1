[CmdletBinding()]
param(
    [string]$OcctRoot,

    [string]$PackageVersion = '8.0.1-preview.19',

    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $workspaceRoot
try {
$consumerProject = Join-Path $workspaceRoot 'tests\OcctSharp.PackageConsumer\OcctSharp.PackageConsumer.csproj'
$moduleConsumerProject = Join-Path $workspaceRoot 'tests\OcctSharp.ModuleConsumer\OcctSharp.ModuleConsumer.csproj'
$packageDirectory = Join-Path $workspaceRoot 'artifacts\packages'
$consumerRoot = Join-Path $workspaceRoot 'artifacts\package-consumer'
$packageCache = Join-Path $consumerRoot 'packages'
$publishDirectory = Join-Path $consumerRoot 'publish'
$modulePublishDirectory = Join-Path $consumerRoot 'module-publish'

& (Join-Path $PSScriptRoot 'pack.ps1') `
    -OcctRoot $OcctRoot `
    -PackageVersion $PackageVersion `
    -SkipBuild:$SkipBuild
if ($LASTEXITCODE -ne 0) {
    throw "Package creation failed with exit code $LASTEXITCODE."
}

$packageIds = @(
    'OcctSharp.Native.win-x64', 'OcctSharp.Runtime', 'OcctSharp.Foundation',
    'OcctSharp.Geometry', 'OcctSharp.MeshData', 'OcctSharp.Modeling',
    'OcctSharp.Mesh', 'OcctSharp.Documents', 'OcctSharp.Visualization',
    'OcctSharp.DataExchange', 'OcctSharp.Xde', 'OcctSharp.IVtk',
    'OcctSharp.Draw', 'OcctSharp'
)
$expectedNativeCount = @(Get-ChildItem -LiteralPath (Join-Path $workspaceRoot 'runtime\win-x64\occt') -File -Filter '*.dll').Count
foreach ($packageId in $packageIds) {
    $packagePath = Join-Path $packageDirectory "$packageId.$PackageVersion.nupkg"
    $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $readmeEntry = $archive.GetEntry('README.md')
        $iconEntry = $archive.GetEntry('occtsharp-icon.png')
        if ($null -eq $readmeEntry -or $readmeEntry.Length -eq 0) {
            throw "Package '$packageId' does not contain the shared package README."
        }
        if ($null -eq $iconEntry -or $iconEntry.Length -eq 0 -or $iconEntry.Length -gt 1MB) {
            throw "Package '$packageId' does not contain a valid NuGet icon below 1 MiB."
        }

        $nuspecEntry = @($archive.Entries | Where-Object FullName -Like '*.nuspec')
        if ($nuspecEntry.Count -ne 1) {
            throw "Package '$packageId' contains $($nuspecEntry.Count) nuspec entries; expected one."
        }
        $nuspecStream = $nuspecEntry[0].Open()
        try {
            $reader = [IO.StreamReader]::new($nuspecStream)
            try { [xml]$nuspec = $reader.ReadToEnd() }
            finally { $reader.Dispose() }
        }
        finally { $nuspecStream.Dispose() }
        if ($nuspec.package.metadata.readme -ne 'README.md' -or
            $nuspec.package.metadata.icon -ne 'occtsharp-icon.png') {
            throw "Package '$packageId' does not declare the accepted README and icon metadata."
        }

        $packagedNativeCount = @($archive.Entries | Where-Object {
            $_.FullName -like 'buildTransitive/win-x64/occt/*.dll'
        }).Count
        $managedAssemblyCount = @($archive.Entries | Where-Object {
            $_.FullName -like 'lib/net10.0/*.dll'
        }).Count
        if ($packageId -eq 'OcctSharp.Native.win-x64') {
            if ($packagedNativeCount -ne $expectedNativeCount) {
                throw "The shared native package contains $packagedNativeCount DLLs; expected $expectedNativeCount."
            }
            if ($managedAssemblyCount -ne 0) {
                throw "The shared native package unexpectedly contains $managedAssemblyCount managed assemblies."
            }
        }
        elseif ($packagedNativeCount -ne 0) {
            throw "Managed package '$packageId' duplicates $packagedNativeCount native runtime DLLs."
        }
        elseif ($managedAssemblyCount -ne 1) {
            throw "Managed package '$packageId' contains $managedAssemblyCount managed assemblies; expected one."
        }
    }
    finally {
        $archive.Dispose()
    }
}
Write-Host "Package asset audit verified the README/icon metadata, one $expectedNativeCount-DLL native package, and one assembly with zero native duplication in each managed package."

if (Test-Path -LiteralPath $consumerRoot) {
    $resolvedConsumerRoot = (Resolve-Path -LiteralPath $consumerRoot).Path
    $resolvedWorkspaceRoot = (Resolve-Path -LiteralPath $workspaceRoot).Path
    if (-not $resolvedConsumerRoot.StartsWith(
        $resolvedWorkspaceRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove package-consumer artifacts outside '$resolvedWorkspaceRoot'."
    }

    Remove-Item -LiteralPath $resolvedConsumerRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $consumerRoot -Force | Out-Null

$nugetConfig = Join-Path $consumerRoot 'NuGet.Config'
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-package" value="$packageDirectory" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8
& dotnet restore $consumerProject `
    --configfile $nugetConfig `
    --packages $packageCache `
    "-p:OcctSharpPackageVersion=$PackageVersion"
if ($LASTEXITCODE -ne 0) {
    throw "Package consumer restore failed with exit code $LASTEXITCODE."
}

& dotnet publish $consumerProject `
    --configuration Release `
    --no-restore `
    --output $publishDirectory `
    "-p:PackageVersion=$PackageVersion"
if ($LASTEXITCODE -ne 0) {
    throw "Package consumer publish failed with exit code $LASTEXITCODE."
}

$consumerExecutable = Join-Path $publishDirectory 'OcctSharp.PackageConsumer.exe'
if (-not (Test-Path -LiteralPath $consumerExecutable)) {
    throw "Package consumer executable was not created: '$consumerExecutable'."
}

& $consumerExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Package consumer failed with exit code $LASTEXITCODE."
}

$nativeDirectory = Join-Path $publishDirectory 'occt'
$nativeFiles = @(Get-ChildItem -LiteralPath $nativeDirectory -File -Filter '*.dll')
if ($nativeFiles.Count -eq 0) {
    throw "No packaged native DLLs were found in '$nativeDirectory'."
}

if (Test-Path -LiteralPath (Join-Path $publishDirectory 'OcctSharp.Native.dll')) {
    throw 'OcctSharp.Native.dll was incorrectly flattened beside the consumer executable.'
}

Write-Host "Clean package consumer verified $($nativeFiles.Count) DLLs under '$nativeDirectory'."

& dotnet restore $moduleConsumerProject `
    --configfile $nugetConfig `
    --packages $packageCache `
    "-p:OcctSharpPackageVersion=$PackageVersion"
if ($LASTEXITCODE -ne 0) {
    throw "Direct module consumer restore failed with exit code $LASTEXITCODE."
}

& dotnet publish $moduleConsumerProject `
    --configuration Release `
    --no-restore `
    --output $modulePublishDirectory `
    "-p:PackageVersion=$PackageVersion"
if ($LASTEXITCODE -ne 0) {
    throw "Direct module consumer publish failed with exit code $LASTEXITCODE."
}

$moduleConsumerExecutable = Join-Path $modulePublishDirectory 'OcctSharp.ModuleConsumer.exe'
if (-not (Test-Path -LiteralPath $moduleConsumerExecutable)) {
    throw "Direct module consumer executable was not created: '$moduleConsumerExecutable'."
}

& $moduleConsumerExecutable
if ($LASTEXITCODE -ne 0) {
    throw "Direct module consumer failed with exit code $LASTEXITCODE."
}

$moduleNativeDirectory = Join-Path $modulePublishDirectory 'occt'
$moduleNativeFiles = @(Get-ChildItem -LiteralPath $moduleNativeDirectory -File -Filter '*.dll')
if ($moduleNativeFiles.Count -ne $nativeFiles.Count) {
    throw "Direct module consumer received $($moduleNativeFiles.Count) native DLLs; expected $($nativeFiles.Count)."
}

Write-Host "Direct module package consumer verified $($moduleNativeFiles.Count) shared native DLLs without the facade package."
}
finally {
    Pop-Location
}

[CmdletBinding()]
param(
    [string]$OcctRoot,

    [string]$PackageVersion = '8.0.1-preview.1',

    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$workspaceRoot = Split-Path -Parent $PSScriptRoot
Push-Location -LiteralPath $workspaceRoot
try {
$consumerProject = Join-Path $workspaceRoot 'tests\OcctSharp.PackageConsumer\OcctSharp.PackageConsumer.csproj'
$packageDirectory = Join-Path $workspaceRoot 'artifacts\packages'
$consumerRoot = Join-Path $workspaceRoot 'artifacts\package-consumer'
$packageCache = Join-Path $consumerRoot 'packages'
$publishDirectory = Join-Path $consumerRoot 'publish'

& (Join-Path $PSScriptRoot 'pack.ps1') `
    -OcctRoot $OcctRoot `
    -PackageVersion $PackageVersion `
    -SkipBuild:$SkipBuild
if ($LASTEXITCODE -ne 0) {
    throw "Package creation failed with exit code $LASTEXITCODE."
}

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
}
finally {
    Pop-Location
}

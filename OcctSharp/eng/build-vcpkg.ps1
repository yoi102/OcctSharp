[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$InstallRoot,
    [ValidateSet('Release','Debug')][string]$Configuration = 'Release',
    [string]$BuildRoot,
    [switch]$SkipGeneration
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspace = Split-Path -Parent $PSScriptRoot
$InstallRoot = (Resolve-Path -LiteralPath $InstallRoot).Path
$provider = Get-Content (Join-Path $InstallRoot 'occtsharp-provider.json') -Raw | ConvertFrom-Json
$expected = Get-Content (Join-Path $workspace 'config/dependencies/provider.json') -Raw | ConvertFrom-Json
if ($provider.state -ne 'PASS' -or $provider.provider -cne $expected.provider -or
    $provider.registryCommit -cne $expected.registryCommit -or $provider.triplet -cne $expected.triplet) {
    throw 'Run the pinned dependency recovery entry point first.'
}
foreach ($inputIdentity in $provider.inputs) {
    $inputPath = Join-Path $workspace ('config/dependencies/' + $inputIdentity.path)
    if ((Get-FileHash -LiteralPath $inputPath).Hash -cne $inputIdentity.sha256) { throw "Dependency inputs changed: $($inputIdentity.path)" }
}
if ((Get-FileHash -LiteralPath (Join-Path $InstallRoot 'vcpkg/status')).Hash -cne $provider.statusSha256) { throw 'Installed port status changed after qualification.' }
$headerManifestPath = Join-Path $provider.sdkView 'header-manifest.json'
if ((Get-FileHash -LiteralPath $headerManifestPath).Hash -cne $provider.sdkHeaderManifestSha256) { throw 'Qualified SDK header manifest changed.' }
$headers = Get-Content -LiteralPath $headerManifestPath -Raw | ConvertFrom-Json
if ($headers.state -cne 'PASS' -or @($headers.headers).Count -ne $provider.sdkHeaders) { throw 'Qualified SDK header set changed.' }
foreach ($header in $headers.headers) {
    if ((Get-FileHash -LiteralPath (Join-Path $provider.sdkView ('inc/' + $header.path))).Hash -cne $header.sha256) { throw "Qualified SDK header changed: $($header.path)" }
}
$vswhere = 'C:/Program Files (x86)/Microsoft Visual Studio/Installer/vswhere.exe'
$vs = (& $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath).Trim()
$cmake = Join-Path $vs 'Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe'
$includeLine = & cmd.exe /d /c ('call "' + (Join-Path $vs 'VC/Auxiliary/Build/vcvars64.bat') + '" >nul && set INCLUDE')
if ($LASTEXITCODE -or $includeLine -notmatch '^INCLUDE=') { throw 'Cannot initialize MSVC includes.' }
$previousInclude = $env:INCLUDE
$env:INCLUDE = $includeLine -replace '^INCLUDE=', ''
if ([string]::IsNullOrWhiteSpace($BuildRoot)) { $BuildRoot = Join-Path $workspace 'build/vcpkg-native' }
$BuildRoot = [IO.Path]::GetFullPath($BuildRoot)
$outputRoot = Join-Path $BuildRoot 'runtime'
try {
    Push-Location -LiteralPath $workspace
    try {
        $generatedRoot = Join-Path $BuildRoot 'generation'
        if (-not $SkipGeneration) {
            & dotnet build src/OcctSharp.Generator/OcctSharp.Generator.csproj -c Release
            if ($LASTEXITCODE) { throw 'Generator bootstrap failed.' }
            & dotnet run --project src/OcctSharp.Generator/OcctSharp.Generator.csproj -c Release --no-build -- generate --occt-root $provider.sdkView --config config/generation.json --output-root $generatedRoot
            if ($LASTEXITCODE) { throw 'Pinned dependency header/generation admission failed.' }
        }
        # SkipGeneration may reuse a prior isolated generation, but never skips
        # its complete identity check against the current public outputs.
        $manifest = Get-Content (Join-Path $workspace 'generated/manifest.json') -Raw | ConvertFrom-Json
        $generatedManifestPath = Join-Path $generatedRoot 'generated/manifest.json'
        if ((Get-FileHash -LiteralPath $generatedManifestPath).Hash -cne
            (Get-FileHash -LiteralPath (Join-Path $workspace 'generated/manifest.json')).Hash) {
            throw 'Vcpkg generated manifest differs from the current public contract.'
        }
        foreach ($file in $manifest.files) {
            $candidate = Join-Path $generatedRoot $file.relativePath
            if (-not (Test-Path -LiteralPath $candidate) -or
                (Get-FileHash -LiteralPath $candidate).Hash -cne (Get-FileHash -LiteralPath (Join-Path $workspace $file.relativePath)).Hash) {
                throw "Vcpkg SDK generation differs from the audited public output: $($file.relativePath)"
            }
        }
        & $cmake -S $workspace -B $BuildRoot -G 'Visual Studio 18 2026' -A x64 `
            "-DCMAKE_TOOLCHAIN_FILE=$($provider.vcpkgRoot)/scripts/buildsystems/vcpkg.cmake" `
            -DVCPKG_MANIFEST_MODE=OFF "-DVCPKG_INSTALLED_DIR=$InstallRoot" -DVCPKG_TARGET_TRIPLET=x64-windows `
            -DOCCTSHARP_DEPENDENCY_PROVIDER=vcpkg "-DOCCTSHARP_NATIVE_OUTPUT_DIR=$outputRoot" `
            "-DOCCTSHARP_GENERATOR_SDK_INCLUDE_DIR=$($provider.sdkView)/inc"
        if ($LASTEXITCODE) { throw 'Vcpkg native configure failed.' }
        & $cmake --build $BuildRoot --config $Configuration --parallel 4
        if ($LASTEXITCODE) { throw 'Vcpkg native compilation/runtime closure failed.' }
        $files = @(Get-ChildItem -LiteralPath (Join-Path $outputRoot $Configuration) -File -Filter '*.dll' | Sort-Object Name | ForEach-Object {
            [ordered]@{ path='occt/' + $_.Name; size=$_.Length; sha256=(Get-FileHash -LiteralPath $_.FullName).Hash }
        })
        [ordered]@{ schemaVersion='1.0'; platform='windows'; architecture='x64'; packageVersion='8.0.1-preview.85';
            occtVersion='8.0.1'; nativeAbi='1.118'; bridgeVersion='0.133.0'; provider=$provider.provider;
            configuration=$Configuration; files=$files; generationChecked=$true; generationExecuted=(-not $SkipGeneration);
            generatedManifestSha256=(Get-FileHash -LiteralPath $generatedManifestPath).Hash;
            state='BUILD_PASS_RUNTIME_NOT_RUN' } |
            ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $BuildRoot "runtime-manifest-$Configuration.json") -Encoding utf8
    } finally { Pop-Location }
} finally { $env:INCLUDE = $previousInclude }

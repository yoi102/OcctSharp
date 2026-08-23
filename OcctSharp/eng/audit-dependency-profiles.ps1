[CmdletBinding()]
param(
    [string]$OcctRoot,
    [string]$ConfigPath,
    [string]$OutputPath
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

if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    $OcctRoot = [Environment]::GetEnvironmentVariable('OCCTSHARP_OCCT_ROOT')
}
if ([string]::IsNullOrWhiteSpace($OcctRoot) -and $null -ne $settings) {
    $OcctRoot = $settings.occtRoot
}
if ([string]::IsNullOrWhiteSpace($OcctRoot)) {
    throw 'Set OCCTSHARP_OCCT_ROOT, pass -OcctRoot, or create config/local.settings.json.'
}

$resolvedOcctRoot = (Resolve-Path -LiteralPath $OcctRoot).Path
$includeRoot = Join-Path $resolvedOcctRoot 'inc'
$libraryRoot = Join-Path $resolvedOcctRoot 'win64\vc14\lib'
$runtimeRoot = Join-Path $resolvedOcctRoot 'win64\vc14\bin'
if (-not (Test-Path -LiteralPath (Join-Path $includeRoot 'Standard_Version.hxx'))) {
    throw "The OCCT root '$resolvedOcctRoot' is invalid."
}

if ([string]::IsNullOrWhiteSpace($ConfigPath)) {
    $ConfigPath = Join-Path $workspaceRoot 'config\dependency-profiles.json'
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $workspaceRoot 'artifacts\generator-reports\dependency-profiles.json'
}

$configuration = Get-Content -LiteralPath $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($configuration.schemaVersion -ne '1.0') {
    throw "Unsupported dependency-profile schema '$($configuration.schemaVersion)'."
}

$results = foreach ($profile in ($configuration.profiles | Sort-Object id)) {
    $headerCounts = [ordered]@{}
    $missingHeaderGlobs = [Collections.Generic.List[string]]::new()
    foreach ($glob in $profile.headerGlobs) {
        $count = @(Get-ChildItem -LiteralPath $includeRoot -Filter $glob -File).Count
        $headerCounts[$glob] = $count
        if ($count -eq 0) { $missingHeaderGlobs.Add($glob) }
    }

    $missingToolkitFiles = [Collections.Generic.List[string]]::new()
    foreach ($toolkit in $profile.toolkits) {
        foreach ($extensionAndRoot in @(@('.lib', $libraryRoot), @('.dll', $runtimeRoot))) {
            $fileName = "$toolkit$($extensionAndRoot[0])"
            if (-not (Test-Path -LiteralPath (Join-Path $extensionAndRoot[1] $fileName))) {
                $missingToolkitFiles.Add($fileName)
            }
        }
    }

    $missingExternalHeaders = [Collections.Generic.List[string]]::new()
    foreach ($header in $profile.externalHeaders) {
        $normalizedHeader = $header -replace '/', [IO.Path]::DirectorySeparatorChar
        $directPath = Join-Path $includeRoot $normalizedHeader
        $leafName = Split-Path -Leaf $normalizedHeader
        if (-not (Test-Path -LiteralPath $directPath) -and
            @(Get-ChildItem -LiteralPath $resolvedOcctRoot -Recurse -Filter $leafName -File -ErrorAction SilentlyContinue).Count -eq 0) {
            $missingExternalHeaders.Add($header)
        }
    }

    $missingExternalRuntimeFiles = @($profile.externalRuntimeFiles | Where-Object {
        -not (Test-Path -LiteralPath (Join-Path $runtimeRoot $_))
    })

    $state = if ($profile.requiredPlatforms -notcontains $configuration.targetPlatform) {
        'UnavailablePlatform'
    } elseif ($profile.language -eq 'cpp-cli') {
        'ExcludedLanguage'
    } elseif ($missingHeaderGlobs.Count -gt 0 -or $missingToolkitFiles.Count -gt 0) {
        'UnavailableInArtifact'
    } elseif ($profile.intendedUse -eq 'TestOnly') {
        'IgnoredByDesign'
    } elseif ($missingExternalHeaders.Count -gt 0 -or $missingExternalRuntimeFiles.Count -gt 0) {
        'BlockedExternalDependency'
    } else {
        'Available'
    }

    [ordered]@{
        id = $profile.id
        package = $profile.package
        state = $state
        expectedState = $profile.expectedState
        classificationMatches = $state -eq $profile.expectedState
        intendedUse = $profile.intendedUse
        language = $profile.language
        requiredPlatforms = @($profile.requiredPlatforms)
        headerCounts = $headerCounts
        toolkits = @($profile.toolkits)
        missingHeaderGlobs = @($missingHeaderGlobs)
        missingToolkitFiles = @($missingToolkitFiles)
        missingExternalHeaders = @($missingExternalHeaders)
        missingExternalRuntimeFiles = @($missingExternalRuntimeFiles)
    }
}

$classificationComplete = @($results | Where-Object { -not $_.classificationMatches }).Count -eq 0
$report = [ordered]@{
    schemaVersion = '1.0'
    occtVersion = '8.0.1'
    targetPlatform = $configuration.targetPlatform
    classificationComplete = $classificationComplete
    profileCount = @($results).Count
    profiles = @($results)
}

$resolvedOutputPath = [IO.Path]::GetFullPath($OutputPath, $workspaceRoot)
$outputDirectory = Split-Path -Parent $resolvedOutputPath
[IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
[IO.File]::WriteAllText(
    $resolvedOutputPath,
    ($report | ConvertTo-Json -Depth 10) + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))

if (-not $classificationComplete) {
    $mismatches = @($results | Where-Object { -not $_.classificationMatches } | ForEach-Object {
        "$($_.id): expected $($_.expectedState), actual $($_.state)"
    }) -join '; '
    throw "Dependency profile classification changed: $mismatches"
}

Write-Host "Dependency profiles classified: $(@($results).Count)/$(@($results).Count); report: '$resolvedOutputPath'."

[CmdletBinding()]
param(
    [string]$NativeLibraryPath,
    [string]$BaselineNativeLibraryPath,
    [string]$DumpbinPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$nativeRoot = Join-Path $workspaceRoot 'src/OcctSharp.Native'
$sourceRoot = Join-Path $nativeRoot 'src'
$cmakeText = Get-Content -LiteralPath (Join-Path $nativeRoot 'CMakeLists.txt') -Raw
$listedSources = @([regex]::Matches($cmakeText, '(?m)^\s*(src/[^\s)]+\.cpp)') |
    ForEach-Object { $_.Groups[1].Value } | Sort-Object)
$sourceFiles = @(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File -Filter '*.cpp')
$actualSources = @($sourceFiles | ForEach-Object {
    [IO.Path]::GetRelativePath($nativeRoot, $_.FullName).Replace('\', '/')
} | Sort-Object)
if (@(Compare-Object $listedSources $actualSources).Count -ne 0 -or
    @($listedSources | Select-Object -Unique).Count -ne $listedSources.Count) {
    throw 'Every manual native translation unit must appear exactly once in the explicit CMake source list.'
}
if (Test-Path -LiteralPath (Join-Path $sourceRoot 'OcctSharp.Native.cpp')) {
    throw 'The historical native monolith must not be reintroduced.'
}
if ($cmakeText -notmatch 'UNITY_BUILD\s+OFF') {
    throw 'Native source boundaries require independent compilation, not a unity build.'
}
if ($cmakeText -notmatch 'SKIP_PRECOMPILE_HEADERS\s+ON') {
    throw 'Manual native source files must declare their own dependencies without the generated PCH.'
}
$errors = [Collections.Generic.List[string]]::new()
$sizeExceptions = @{}
$usedSizeExceptions = [Collections.Generic.List[object]]::new()
$exceptionPath = Join-Path $workspaceRoot 'config/native-source-layout-exceptions.json'
if (Test-Path -LiteralPath $exceptionPath) {
    $configuration = Get-Content -LiteralPath $exceptionPath -Raw | ConvertFrom-Json
    if ($configuration.schemaVersion -ne '1.0' -or $configuration.exceptions -isnot [array]) {
        throw 'Native source-size exceptions require schemaVersion 1.0 and an exceptions array.'
    }
    foreach ($entry in $configuration.exceptions) {
        if ($entry.path -isnot [string] -or $entry.path -cnotin $actualSources) {
            throw 'A native source-size exception must name an exact existing manual source path.'
        }
        if ($sizeExceptions.ContainsKey($entry.path)) {
            throw "Duplicate native source-size exception: $($entry.path)."
        }
        if (($entry.maxLines -isnot [int] -and $entry.maxLines -isnot [long]) -or
            $entry.maxLines -le 1000 -or [string]::IsNullOrWhiteSpace($entry.reason) -or
            $entry.reason -isnot [string]) {
            throw "Native source-size exception requires a finite integer maxLines > 1000 and a nonblank reason: $($entry.path)."
        }
        $sizeExceptions[$entry.path] = $entry
    }
}
$definitions = [Collections.Generic.List[object]]::new()
$exports = [Collections.Generic.List[string]]::new()
$sourceInventory = @()
foreach ($file in Get-ChildItem -LiteralPath $sourceRoot -Recurse -File | Where-Object Extension -in @('.cpp', '.hxx', '.h')) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    $relative = [IO.Path]::GetRelativePath($nativeRoot, $file.FullName).Replace('\', '/')
    if ($text -match '#\s*include\s*["<][^">]+\.(cpp|inc)[">]') {
        $errors.Add("Implementation inclusion is forbidden: $relative")
    }
    foreach ($match in [regex]::Matches($text, '(?m)^\s*(?:static\s+|inline\s+)?(?:thread_local\s+std::string\s+(LastError)|std::mutex\s+(LiveShapesMutex)|std::unordered_set<[^\r\n;]+>\s+(Live\w+))\s*[;={]')) {
        $name = @($match.Groups | Select-Object -Skip 1 | Where-Object Success | ForEach-Object Value)[0]
        $definitions.Add([pscustomobject]@{ Name = $name; File = $relative })
    }
    if ($file.Extension -eq '.cpp') {
        $lineCount = [IO.File]::ReadLines($file.FullName) | Measure-Object | Select-Object -ExpandProperty Count
        $sizeLimit = if ($sizeExceptions.ContainsKey($relative)) { $sizeExceptions[$relative].maxLines } else { 1000 }
        if ($lineCount -gt $sizeLimit) {
            $errors.Add("Native source exceeds its source-size limit ($sizeLimit lines): $relative ($lineCount); split the responsibility or record a bounded cohesion exception.")
        }
        if ($lineCount -gt 1000 -and $sizeExceptions.ContainsKey($relative)) {
            $usedSizeExceptions.Add([pscustomobject]@{
                path = $relative; lineCount = $lineCount; maxLines = $sizeLimit; reason = $sizeExceptions[$relative].reason
            })
        }
        $fileExports = @([regex]::Matches($text, '\bOCCTSHARP_CALL\s+(occtsharp_\w+)\s*\(') | ForEach-Object { $_.Groups[1].Value })
        foreach ($export in $fileExports) { $exports.Add($export) }
        $sourceInventory += [pscustomobject][ordered]@{ path = $relative; lineCount = $lineCount; manualExportCount = $fileExports.Count }
    }
}
foreach ($group in $definitions | Group-Object Name) {
    $expectedOwner = if ($group.Name -eq 'LastError') { 'src/Runtime/Error.cpp' } else { 'src/Runtime/Registry.cpp' }
    if ($group.Count -ne 1 -or $group.Group[0].File -ne $expectedOwner) {
        $errors.Add("Shared native state has the wrong or duplicate owner: $($group.Name)")
    }
}
foreach ($required in @('LastError', 'LiveShapesMutex', 'LiveShapes')) {
    if ($required -notin $definitions.Name) { $errors.Add("Required runtime storage is missing: $required") }
}
if (@($exports | Select-Object -Unique).Count -ne $exports.Count) {
    $errors.Add('Duplicate manual C ABI implementation names were found.')
}
if ($errors.Count) { throw ($errors -join [Environment]::NewLine) }

$symbolComparison = 'NOT RUN'
$symbolCount = $null
if ($NativeLibraryPath -or $BaselineNativeLibraryPath) {
    if (-not $NativeLibraryPath -or -not $BaselineNativeLibraryPath) { throw 'Supply both native libraries for an export comparison.' }
    if (-not $DumpbinPath) {
        $settingsPath = Join-Path $workspaceRoot 'config/local.settings.json'
        $settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
        $toolset = Get-ChildItem -LiteralPath (Join-Path $settings.visualStudioRoot 'VC/Tools/MSVC') -Directory |
            Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
        $DumpbinPath = Join-Path $toolset.FullName 'bin/Hostx64/x64/dumpbin.exe'
    }
    function Read-ExportNames([string]$LibraryPath) {
        $resolved = (Resolve-Path -LiteralPath $LibraryPath).Path
        $dump = @(& $DumpbinPath /nologo /exports $resolved)
        if ($LASTEXITCODE -ne 0) { throw "dumpbin failed for '$resolved'." }
        $names = @($dump | ForEach-Object {
            if ($_ -match '^\s+\d+\s+[0-9A-F]+\s+[0-9A-F]+\s+(\S+)') { $Matches[1] }
        } | Sort-Object -CaseSensitive)
        if ($names.Count -eq 0) { throw "No exported symbols found in '$resolved'." }
        return ,$names
    }
    $before = Read-ExportNames $BaselineNativeLibraryPath
    $after = Read-ExportNames $NativeLibraryPath
    $difference = @(Compare-Object $before $after -CaseSensitive)
    if ($difference.Count) { throw "Native exported symbols changed: $($difference | ConvertTo-Json -Compress)" }
    $symbolCount = $after.Count
    $symbolComparison = 'PASS'
}
$report = [ordered]@{
    schemaVersion = '1.1'
    independentTranslationUnitCount = $sourceFiles.Count
    manualExportCount = $exports.Count
    sharedStorageDefinitions = $definitions.Count
    sourceLayout = 'PASS'
    nativeSymbolComparison = $symbolComparison
    nativeSymbolCount = $symbolCount
    sources = @($sourceInventory | Sort-Object path)
    sourceSizeExceptions = @($usedSizeExceptions | Sort-Object path)
}
$output = Join-Path $workspaceRoot 'artifacts/native-source-layout.json'
[IO.Directory]::CreateDirectory((Split-Path -Parent $output)) | Out-Null
[IO.File]::WriteAllText($output, ($report | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
Write-Host "Native source layout PASS: $($sourceFiles.Count) independent units, $($exports.Count) manual C exports, $($definitions.Count) unique shared storage definitions. Native export comparison: $symbolComparison ($symbolCount)."
foreach ($entry in $usedSizeExceptions) {
    Write-Host "Native source-size exception: $($entry.path), $($entry.lineCount)/$($entry.maxLines) lines; $($entry.reason)"
}

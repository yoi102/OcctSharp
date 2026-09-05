[CmdletBinding()]
param(
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json',
    [ValidateSet('QT', 'UVW', 'QW')][string]$Scope = 'QW'
)

# Preparation evidence only: does not generate bindings, build, pack or publish.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$workspaceRoot = Split-Path -Parent $PSScriptRoot
$repositoryRoot = Split-Path -Parent $workspaceRoot
$inventoryFile = [IO.Path]::GetFullPath($InventoryPath, $workspaceRoot)
$inventoryHash = (Get-FileHash -LiteralPath $inventoryFile).Hash
$settings = Get-Content "$workspaceRoot/config/local.settings.json" -Raw | ConvertFrom-Json
$cmake = Get-Content "$workspaceRoot/src/OcctSharp.Native/CMakeLists.txt" -Raw
$plan = Get-Content "$workspaceRoot/config/batches/continuous-plan.json" -Raw | ConvertFrom-Json
$expectedOrder = @('Q', 'R', 'S', 'T', 'U', 'V', 'W')
if (($plan.executionOrder -join ',') -cne ($expectedOrder -join ',') -or
    ($plan.batches.batch -join ',') -cne ($expectedOrder -join ',') -or
    $plan.capabilities -ne 280 -or
    -not $plan.autoAdvanceAfterVerifiedLocalCommit -or
    -not $plan.baselineRevalidationBeforeEveryBatch -or
    -not $plan.localCommitPerCompletedBatch -or
    $plan.allowNuGetPublish -or $plan.allowGitHubPush -or $plan.allowSkippedRequiredGates) {
    throw 'The continuous queue or delivery boundary does not match ADR-0083.'
}
$preceding = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($entry in $plan.batches) {
    if ($entry.capabilities -ne 40) { throw 'Every prepared batch must retain its 40-row denominator.' }
    foreach ($dependency in $entry.requires) {
        if (-not $preceding.Contains($dependency)) { throw "Unmet or cyclic plan dependency: $($entry.batch)/$dependency." }
    }
    [void]$preceding.Add($entry.batch)
}
$selected = switch ($Scope) { 'QT' { @('Q','R','S','T') } 'UVW' { @('U','V','W') } 'QW' { $expectedOrder } }
$specs = @($plan.batches | Where-Object { $_.batch -cin $selected })
$capabilityCount = ($specs | Measure-Object capabilities -Sum).Sum
$reports = @()
$batchEvidence = @()
$headers = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$negativeCount = 0
$fixtureDirectory = Join-Path $workspaceRoot 'artifacts/preparation-negative-fixtures'
[IO.Directory]::CreateDirectory($fixtureDirectory) | Out-Null

foreach ($spec in $specs) {
    $letter = $spec.batch.ToLowerInvariant()
    $configFile = [IO.Path]::GetFullPath($spec.config, $workspaceRoot)
    $configHash = (Get-FileHash -LiteralPath $configFile).Hash
    $config = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
    if ($config.batch -cne $spec.batch -or $config.capabilityCount -ne 40) {
        throw "Unexpected capability contract for $letter."
    }
    $partition = @($config.decisionRoots) + @($config.supportRoots)
    if (@($partition | Sort-Object -Unique).Count -ne $partition.Count -or
        @(Compare-Object ($partition | Sort-Object) ($config.roots | Sort-Object)).Count -ne 0) {
        throw "Decision/support roots are not an exact disjoint partition for $letter."
    }
    $document = Get-Content "$repositoryRoot/docs/$($spec.matrix)" -Raw
    # Completed batches also have a named-assertion table containing the same IDs.
    # Only the explicitly frozen capability section defines the denominator.
    $matrices = @([regex]::Matches($document, '(?ms)^## Frozen capability(?: and acceptance)? matrix\r?\n(?<rows>.*?)(?=^## |\z)'))
    if ($matrices.Count -ne 1) { throw "Expected one frozen capability section for $letter." }
    $rows = @([regex]::Matches($matrices[0].Groups['rows'].Value, "(?m)^\| $($spec.batch)-([0-9]{2}) \|"))
    if ($rows.Count -ne 40 -or
        @(Compare-Object @($rows | ForEach-Object { [int]$_.Groups[1].Value }) @(1..40)).Count -ne 0) {
        throw "Capability matrix is not exactly $($spec.batch)-01 through $($spec.batch)-40."
    }
    foreach ($root in $config.roots) { [void]$headers.Add("$root.hxx") }
    if ($config.PSObject.Properties.Name -contains 'headerOnlyDependencies') {
        foreach ($header in $config.headerOnlyDependencies) { [void]$headers.Add($header) }
    }

    $output = "$workspaceRoot/artifacts/generator-reports/batch-$letter-root-audit.json"
    $repeat = "$workspaceRoot/artifacts/generator-reports/batch-$letter-root-audit-repeat.json"
    & "$PSScriptRoot/audit-batch-roots.ps1" -ConfigPath $configFile -InventoryPath $inventoryFile -OutputPath $output
    & "$PSScriptRoot/audit-batch-roots.ps1" -ConfigPath $configFile -InventoryPath $inventoryFile -OutputPath $repeat
    $hash = (Get-FileHash -LiteralPath $output).Hash
    if ($hash -cne (Get-FileHash -LiteralPath $repeat).Hash) { throw "Audit is not deterministic: $letter." }
    if (-not $document.Contains($hash)) { throw "Stale documented audit hash: $letter." }
    $report = Get-Content -LiteralPath $output -Raw | ConvertFrom-Json
    $reports += $report
    $batchEvidence += [ordered]@{
        batch = $spec.batch; capabilities = $rows.Count; decisionRoots = $config.decisionRoots.Count
        supportRoots = $config.supportRoots.Count; roots = $report.rootCount
        candidates = $report.candidateCount; states = $report.states; sha256 = $hash
    }

    # Generated negative fixtures stay in ignored artifacts, never mutate real inputs.
    $wrongConfig = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
    $wrongConfig.baselineInventorySha256 = '0' * 64
    $wrongFile = Join-Path $fixtureDirectory "batch-$letter-wrong-baseline.json"
    [IO.File]::WriteAllText($wrongFile, ($wrongConfig | ConvertTo-Json -Depth 8), [Text.UTF8Encoding]::new($false))
    $rejected = $false
    try { & "$PSScriptRoot/audit-batch-roots.ps1" -ConfigPath $wrongFile -InventoryPath $inventoryFile -OutputPath $repeat }
    catch { if ($_.Exception.Message -notlike '*does not match the frozen batch baseline*') { throw }; $rejected = $true }
    if (-not $rejected) { throw "Wrong baseline was accepted: $letter." }
    $negativeCount++
    $rejected = $false
    try { & "$PSScriptRoot/audit-batch-roots.ps1" -ConfigPath $configFile -InventoryPath $inventoryFile -OutputPath $inventoryFile }
    catch { if ($_.Exception.Message -notlike '*must not overwrite its inputs*') { throw }; $rejected = $true }
    if (-not $rejected) { throw "Input overwrite was accepted: $letter." }
    $negativeCount++
    if ($configHash -cne (Get-FileHash -LiteralPath $configFile).Hash -or
        $inventoryHash -cne (Get-FileHash -LiteralPath $inventoryFile).Hash) { throw 'Audit changed its input.' }
}

foreach ($header in $headers) {
    if (-not (Test-Path -LiteralPath "$($settings.occtRoot)/inc/$header")) { throw "Missing SDK header: $header." }
}
$dumpbin = Get-ChildItem "$($settings.visualStudioRoot)/VC/Tools/MSVC/*/bin/Hostx64/x64/dumpbin.exe" |
    Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
if (-not $dumpbin) { throw 'An x64 MSVC dumpbin is required for SDK availability evidence.' }
$symbolChecks = @($plan.sdkChecks | Where-Object { @($_.batches | Where-Object { $_ -cin $selected }).Count -gt 0 })
$symbolEvidence = @()
foreach ($check in $symbolChecks) {
    if ($cmake -notmatch "(?m)^\s+$($check.toolkit)\s*$") { throw "Toolkit absent from CMake closure: $($check.toolkit)." }
    $dll = "$($settings.occtRoot)/win64/vc14/bin/$($check.toolkit).dll"
    $exports = & $dumpbin /nologo /exports $dll
    if ($LASTEXITCODE -ne 0) { throw "dumpbin failed for $($check.toolkit)." }
    $matches = @($exports | Select-String -SimpleMatch $check.symbol)
    if ($matches.Count -eq 0) { throw "Missing representative SDK export: $($check.symbol)." }
    $symbolEvidence += [ordered]@{ toolkit = $check.toolkit; symbol = $check.symbol; overloads = $matches.Count }
}
$allCandidates = @($reports | ForEach-Object { $_.candidates })
$union = @($allCandidates | Sort-Object StableId -Unique)
$overlaps = @()
foreach ($left in $reports) {
    foreach ($right in $reports) {
        if ($left.batch -ge $right.batch) { continue }
        $ids = [Collections.Generic.HashSet[string]]::new([string[]]$left.candidates.StableId, [StringComparer]::Ordinal)
        $overlaps += [ordered]@{ batches = "$($left.batch)/$($right.batch)"; sharedIds = @($right.candidates | Where-Object { $ids.Contains($_.StableId) }).Count }
    }
}
$result = [ordered]@{
    schemaVersion = '1.0'; baselineInventorySha256 = $inventoryHash
    preparationOnly = $true; batches = $batchEvidence; uniqueHeaders = $headers.Count
    uniqueRoots = @($reports | ForEach-Object { $_.roots.root } | Sort-Object -Unique).Count
    uniqueCandidates = $union.Count; repeatedCandidateOccurrences = $allCandidates.Count - $union.Count
    unionStates = @($union | Group-Object State | Sort-Object Name | ForEach-Object { [ordered]@{ state = $_.Name; count = $_.Count } })
    overlaps = $overlaps; negativeChecksPassed = $negativeCount; sdkSymbols = $symbolEvidence
    newCompile = 'NOT RUN'; newRuntime = 'NOT RUN'; pack = 'NOT RUN'; publication = 'NOT RUN'
}
$reportName = switch ($Scope) { 'QT' { 'batch-q-t-preparation-audit.json' } 'UVW' { 'batch-u-w-preparation-audit.json' } 'QW' { 'batch-q-w-preparation-audit.json' } }
$output = "$workspaceRoot/artifacts/generator-reports/$reportName"
[IO.File]::WriteAllText($output, ($result | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
Write-Host "Preparation PASS: $capabilityCount rows; $($headers.Count) SDK headers; $($union.Count) unique candidates; $negativeCount negative checks; $($symbolEvidence.Count) representative SDK symbols."
Write-Host "Report: $output. New API compile/runtime and publication are NOT RUN."

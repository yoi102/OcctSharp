[CmdletBinding()]
param(
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json'
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
$specs = @(
    @('Q', 'shape-repair-topology', 'BATCH_Q_SHAPE_REPAIR_TOPOLOGY_GAP_INVENTORY.md'),
    @('R', 'mesh-authoring-editing', 'BATCH_R_MESH_AUTHORING_EDITING_GAP_INVENTORY.md'),
    @('S', 'guided-sweep-constrained-surface', 'BATCH_S_GUIDED_SWEEP_CONSTRAINED_SURFACE_GAP_INVENTORY.md'),
    @('T', 'parametric-document-recompute', 'BATCH_T_PARAMETRIC_DOCUMENT_RECOMPUTE_GAP_INVENTORY.md')
)
$reports = @()
$batchEvidence = @()
$headers = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
$negativeCount = 0
$fixtureDirectory = Join-Path $workspaceRoot 'artifacts/preparation-negative-fixtures'
[IO.Directory]::CreateDirectory($fixtureDirectory) | Out-Null

foreach ($spec in $specs) {
    $letter = $spec[0].ToLowerInvariant()
    $configFile = "$workspaceRoot/config/batches/batch-$letter-$($spec[1]).json"
    $configHash = (Get-FileHash -LiteralPath $configFile).Hash
    $config = Get-Content -LiteralPath $configFile -Raw | ConvertFrom-Json
    if ($config.batch -cne $spec[0] -or $config.capabilityCount -ne 40) {
        throw "Unexpected capability contract for $letter."
    }
    $partition = @($config.decisionRoots) + @($config.supportRoots)
    if (@($partition | Sort-Object -Unique).Count -ne $partition.Count -or
        @(Compare-Object ($partition | Sort-Object) ($config.roots | Sort-Object)).Count -ne 0) {
        throw "Decision/support roots are not an exact disjoint partition for $letter."
    }
    $document = Get-Content "$repositoryRoot/docs/$($spec[2])" -Raw
    $rows = @([regex]::Matches($document, "(?m)^\| $($spec[0])-([0-9]{2}) \|"))
    if ($rows.Count -ne 40 -or
        @(Compare-Object @($rows | ForEach-Object { [int]$_.Groups[1].Value }) @(1..40)).Count -ne 0) {
        throw "Capability matrix is not exactly $($spec[0])-01 through $($spec[0])-40."
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
        batch = $spec[0]; capabilities = $rows.Count; decisionRoots = $config.decisionRoots.Count
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
$symbolChecks = @(
    @('TKShHealing', 'FixWireGaps@ShapeFix_Wireframe'),
    @('TKTopAlgo', 'Perform@BRepBuilderAPI_Sewing'),
    @('TKMath', 'RemoveDegenerated@Poly_CoherentTriangulation'),
    @('TKOffset', 'SetLaw@BRepOffsetAPI_MakePipeShell'),
    @('TKOffset', 'Add@BRepOffsetAPI_MakeFilling'),
    @('TKLCAF', 'AddPrevious@TFunction_GraphNode'),
    @('TKCAF', 'Solve@TNaming_Selector')
)
$symbolEvidence = @()
foreach ($check in $symbolChecks) {
    if ($cmake -notmatch "(?m)^\s+$($check[0])\s*$") { throw "Toolkit absent from CMake closure: $($check[0])." }
    $dll = "$($settings.occtRoot)/win64/vc14/bin/$($check[0]).dll"
    $exports = & $dumpbin /nologo /exports $dll
    if ($LASTEXITCODE -ne 0) { throw "dumpbin failed for $($check[0])." }
    $matches = @($exports | Select-String -SimpleMatch $check[1])
    if ($matches.Count -eq 0) { throw "Missing representative SDK export: $($check[1])." }
    $symbolEvidence += [ordered]@{ toolkit = $check[0]; symbol = $check[1]; overloads = $matches.Count }
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
$output = "$workspaceRoot/artifacts/generator-reports/batch-q-t-preparation-audit.json"
[IO.File]::WriteAllText($output, ($result | ConvertTo-Json -Depth 8) + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
Write-Host "Preparation PASS: 160 rows; $($headers.Count) SDK headers; $($union.Count) unique candidates; $negativeCount negative checks; $($symbolEvidence.Count) representative SDK symbols."
Write-Host "Report: $output. New API compile/runtime and publication are NOT RUN."

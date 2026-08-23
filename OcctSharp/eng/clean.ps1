[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$workspaceRoot = Split-Path -Parent $PSScriptRoot

$targets = @(
    (Join-Path $workspaceRoot 'artifacts'),
    (Join-Path $workspaceRoot 'build')
)

foreach ($target in $targets) {
    if (Test-Path -LiteralPath $target) {
        $resolvedTarget = (Resolve-Path -LiteralPath $target).Path
        $resolvedWorkspace = (Resolve-Path -LiteralPath $workspaceRoot).Path
        if (-not $resolvedTarget.StartsWith($resolvedWorkspace + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Refusing to remove path outside the workspace: '$resolvedTarget'."
        }

        Remove-Item -LiteralPath $resolvedTarget -Recurse -Force
    }
}

Get-ChildItem -LiteralPath $workspaceRoot -Recurse -Directory -Force |
    Where-Object { $_.Name -in @('bin', 'obj', 'TestResults') } |
    Sort-Object FullName -Descending |
    ForEach-Object {
        if ($_.FullName.StartsWith($workspaceRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            Remove-Item -LiteralPath $_.FullName -Recurse -Force
        }
    }

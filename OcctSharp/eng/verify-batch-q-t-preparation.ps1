[CmdletBinding()]
param(
    [string]$InventoryPath = 'artifacts/generator-reports/full-inventory.json'
)

# Preserve the original Q-T entry point and report shape/hash.
& "$PSScriptRoot/verify-prepared-batches.ps1" -Scope QT -InventoryPath $InventoryPath

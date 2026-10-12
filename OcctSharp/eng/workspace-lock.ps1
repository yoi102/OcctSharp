# Shared by Windows PowerShell native bootstrap and PowerShell release tooling.
function Enter-OcctSharpWorkspaceLock {
    param(
        [Parameter(Mandatory)][string]$WorkspaceRoot,
        [Parameter(Mandatory)][ValidateSet('native', 'package')][string]$Scope,
        [int]$TimeoutSeconds = 1800
    )
    $canonicalRoot = [IO.Path]::GetFullPath($WorkspaceRoot).TrimEnd('\', '/').ToUpperInvariant()
    $algorithm = [Security.Cryptography.SHA256]::Create()
    try { $digest = [BitConverter]::ToString($algorithm.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonicalRoot))).Replace('-', '') }
    finally { $algorithm.Dispose() }
    $mutex = [Threading.Mutex]::new($false, "Local\OcctSharp-$Scope-$digest")
    try {
        $acquired = $false
        try { $acquired = $mutex.WaitOne([TimeSpan]::FromSeconds($TimeoutSeconds)) }
        catch [Threading.AbandonedMutexException] {
            # The OS transfers ownership to us. A complete configure/build follows.
            $acquired = $true
            Write-Host "Recovering abandoned $Scope workspace lock."
        }
        if (-not $acquired) { throw "Timed out waiting for the $Scope workspace lock at '$canonicalRoot'." }
        Write-Host "Workspace lock acquired: $Scope PID=$PID UTC=$([DateTime]::UtcNow.ToString('o'))"
        return [pscustomobject]@{ Mutex = $mutex; Scope = $Scope }
    }
    catch { $mutex.Dispose(); throw }
}

function Exit-OcctSharpWorkspaceLock {
    param([Parameter(Mandatory)]$Lock)
    try {
        Write-Host "Workspace lock released: $($Lock.Scope) PID=$PID UTC=$([DateTime]::UtcNow.ToString('o'))"
        $Lock.Mutex.ReleaseMutex()
    }
    finally { $Lock.Mutex.Dispose() }
}

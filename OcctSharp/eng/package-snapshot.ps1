function Assert-OcctSharpPackageSnapshot {
    param([Parameter(Mandatory)][string]$PackagePath, [Parameter(Mandatory)][hashtable]$Inputs,
          [Parameter(Mandatory)][string]$PackageId, [Parameter(Mandatory)][string]$NativeRelativeDirectory)
    $archive = [IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        if ($PackageId -eq 'OcctSharp') {
            foreach ($path in $Inputs.Keys) {
                if ($path -like 'docs/*.md' -and $null -eq $archive.GetEntry($path)) { throw "Package is missing frozen documentation: $path" }
            }
        }
        foreach ($entry in $archive.Entries) {
            $name = $entry.FullName
            $source = $null
            if ($name -eq 'README.md' -or $name -eq 'LICENSE' -or $name.StartsWith('docs/')) { $source = $name }
            elseif ($name -eq 'occtsharp-icon.png') { $source = 'OcctSharp/assets/occtsharp-package-icon.png' }
            elseif ($name -eq 'buildTransitive/OcctSharp.Native.win-x64.targets') { $source = 'OcctSharp/packaging/OcctSharp.targets' }
            elseif ($name.StartsWith('buildTransitive/win-x64/occt/')) { $source = "$NativeRelativeDirectory/$($entry.Name)" }
            elseif ($name.StartsWith('licenses/')) {
                $source = if ($entry.Name -eq 'THIRD_PARTY_NOTICES.md') { 'OcctSharp/runtime/win-x64/THIRD_PARTY_NOTICES.md' }
                    else { 'OcctSharp/runtime/win-x64/' + $name }
            }
            elseif ($name -match '^lib/net10\.0/[^/]+\.(dll|xml|pdb)$') {
                $source = "OcctSharp/src/$PackageId/bin/Release/net10.0/win-x64/$($entry.Name)"
            }
            if ($null -eq $source) { continue }
            if (-not $Inputs.ContainsKey($source)) { throw "Package '$PackageId' contains an unfrozen input: $name ($source)." }
            $stream = $entry.Open()
            $algorithm = [Security.Cryptography.SHA256]::Create()
            try { $hash = [BitConverter]::ToString($algorithm.ComputeHash($stream)).Replace('-', '') }
            finally { $algorithm.Dispose(); $stream.Dispose() }
            if ($hash -cne $Inputs[$source]) { throw "Package '$PackageId' differs from its frozen input: $name." }
        }
    }
    finally { $archive.Dispose() }
}

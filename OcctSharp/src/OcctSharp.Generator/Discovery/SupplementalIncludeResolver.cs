using System.Security.Cryptography;

namespace OcctSharp.Generator.Discovery;

internal static class SupplementalIncludeResolver
{
    internal static IReadOnlyList<string> Resolve(string configPath, IReadOnlyList<SupplementalIncludeConfiguration> includes)
    {
        string root = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
        var directories = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var include in includes)
        {
            if (string.IsNullOrWhiteSpace(include.Path) || Path.IsPathRooted(include.Path))
                throw new InvalidDataException("SDK supplement paths must be relative to their configuration.");
            string full = Path.GetFullPath(Path.Combine(root, include.Path));
            if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("SDK supplement escapes its configuration directory.");
            if (!File.Exists(full)) throw new FileNotFoundException("SDK supplement is missing.", full);
            string actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(full)));
            if (!actual.Equals(include.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("SDK supplement hash differs from fixed source: " + include.Path);
            directories.Add(Path.GetDirectoryName(full)!);
        }
        return directories.ToArray();
    }
}

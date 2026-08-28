using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OcctSharp.Generator.Emission;

public static class GeneratedOutputWriter
{
    public const string ManifestRelativePath = "generated/manifest.json";

    private static readonly string[] AllowedPrefixes =
    [
        "src/OcctSharp.Native/generated/",
        "src/OcctSharp/Generated/",
    ];

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static GeneratedManifest Replace(string outputRoot, GeneratedBindingSet bindingSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);
        ArgumentNullException.ThrowIfNull(bindingSet);

        string fullOutputRoot = Path.GetFullPath(outputRoot);
        GeneratedFile[] files = bindingSet.Files
            .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();
        ValidateFiles(files);

        string stagingRoot = Path.Combine(
            fullOutputRoot,
            "artifacts",
            "generator-staging",
            Path.GetRandomFileName());
        Directory.CreateDirectory(stagingRoot);

        try
        {
            foreach (GeneratedFile file in files)
            {
                WriteText(Path.Combine(stagingRoot, ToPlatformPath(file.RelativePath)), file.Content);
            }

            GeneratedManifest manifest = new(
                "1.1",
                bindingSet.OcctVersion,
                "1.3",
                bindingSet.SourceStableIds.Order(StringComparer.Ordinal).ToArray(),
                files.Select(file => new GeneratedManifestFile(
                        file.RelativePath,
                        ComputeSha256(file.Content),
                        file.ProductModule.ToString(),
                        file.ApiLayer.ToString(),
                        file.OutputShard))
                    .ToArray());
            string manifestContent = JsonSerializer.Serialize(
                manifest,
                ManifestJsonOptions) + "\n";
            WriteText(Path.Combine(stagingRoot, ToPlatformPath(ManifestRelativePath)), manifestContent);
            VerifyStaging(stagingRoot, manifest);

            string manifestPath = Path.Combine(fullOutputRoot, ToPlatformPath(ManifestRelativePath));
            GeneratedManifest? previousManifest = ReadManifest(manifestPath);
            HashSet<string> currentPaths = files
                .Select(static file => file.RelativePath)
                .ToHashSet(StringComparer.Ordinal);
            if (previousManifest is not null)
            {
                foreach (GeneratedManifestFile staleFile in previousManifest.Files
                    .Where(file => !currentPaths.Contains(file.RelativePath)))
                {
                    ValidateRelativePath(staleFile.RelativePath);
                    string stalePath = Path.Combine(fullOutputRoot, ToPlatformPath(staleFile.RelativePath));
                    if (File.Exists(stalePath))
                    {
                        File.Delete(stalePath);
                    }
                }
            }

            foreach (GeneratedFile file in files)
            {
                string source = Path.Combine(stagingRoot, ToPlatformPath(file.RelativePath));
                string destination = Path.Combine(fullOutputRoot, ToPlatformPath(file.RelativePath));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                CopyIfChanged(source, destination, manifest.Files.Single(
                    item => string.Equals(item.RelativePath, file.RelativePath, StringComparison.Ordinal)).Sha256);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
            CopyIfChanged(
                Path.Combine(stagingRoot, ToPlatformPath(ManifestRelativePath)),
                manifestPath,
                ComputeSha256(manifestContent));
            return manifest;
        }
        finally
        {
            if (Directory.Exists(stagingRoot))
            {
                Directory.Delete(stagingRoot, recursive: true);
            }
        }
    }

    private static void ValidateFiles(GeneratedFile[] files)
    {
        if (files.Length == 0)
        {
            throw new InvalidDataException("Generation produced no files.");
        }

        HashSet<string> paths = new(StringComparer.Ordinal);
        foreach (GeneratedFile file in files)
        {
            ValidateRelativePath(file.RelativePath);
            if (!paths.Add(file.RelativePath))
            {
                throw new InvalidDataException($"Duplicate generated path '{file.RelativePath}'.");
            }

            if (!file.Content.EndsWith('\n'))
            {
                throw new InvalidDataException($"Generated file '{file.RelativePath}' has no final newline.");
            }

            if (file.ProductModule == Model.OcctProductModule.Unassigned
                || string.IsNullOrWhiteSpace(file.OutputShard))
            {
                throw new InvalidDataException(
                    $"Generated file '{file.RelativePath}' has no stable module or output-shard assignment.");
            }
        }
    }

    private static void ValidateRelativePath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath)
            || relativePath.Contains("..", StringComparison.Ordinal)
            || relativePath.Contains('\\')
            || !AllowedPrefixes.Any(prefix => relativePath.StartsWith(prefix, StringComparison.Ordinal)))
        {
            throw new InvalidDataException($"Generated path '{relativePath}' is outside an allowed generated directory.");
        }
    }

    private static void VerifyStaging(string stagingRoot, GeneratedManifest manifest)
    {
        foreach (GeneratedManifestFile file in manifest.Files)
        {
            string path = Path.Combine(stagingRoot, ToPlatformPath(file.RelativePath));
            if (!File.Exists(path)
                || !string.Equals(
                    file.Sha256,
                    ComputeSha256(File.ReadAllText(path)),
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Staged generated file '{file.RelativePath}' failed hash verification.");
            }
        }
    }

    private static GeneratedManifest? ReadManifest(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        return JsonSerializer.Deserialize<GeneratedManifest>(
            File.ReadAllText(path),
            ManifestJsonOptions)
            ?? throw new InvalidDataException($"Generated manifest '{path}' is empty.");
    }

    private static void WriteText(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void CopyIfChanged(string source, string destination, string expectedSha256)
    {
        if (File.Exists(destination)
            && string.Equals(ComputeFileSha256(destination), expectedSha256, StringComparison.Ordinal))
        {
            return;
        }

        File.Copy(source, destination, overwrite: true);
    }

    private static string ComputeSha256(string content) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));

    private static string ComputeFileSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static string ToPlatformPath(string relativePath) =>
        relativePath.Replace('/', Path.DirectorySeparatorChar);
}

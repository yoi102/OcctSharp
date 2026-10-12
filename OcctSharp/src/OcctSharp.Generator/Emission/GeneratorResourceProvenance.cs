using System.Security.Cryptography;

namespace OcctSharp.Generator.Emission;

/// <summary>Fingerprints all embedded generation inputs, independent of build configuration or checkout path.</summary>
public static class GeneratorResourceProvenance
{
    public static IReadOnlyList<GeneratedResourceFingerprint> Capture()
    {
        var assembly = typeof(GeneratorResourceProvenance).Assembly;
        return assembly.GetManifestResourceNames().Order(StringComparer.Ordinal).Select(name =>
        {
            using Stream stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidDataException("Missing generation resource: " + name);
            return new GeneratedResourceFingerprint(name, Convert.ToHexString(SHA256.HashData(stream)));
        }).ToArray();
    }
}

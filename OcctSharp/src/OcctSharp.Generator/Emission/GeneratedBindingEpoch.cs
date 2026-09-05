using System.Text.Json;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Emission;

/// <summary>Keeps Preview.22 overload ordinals ahead of newly supported declarations.</summary>
internal static class GeneratedBindingEpoch
{
    private static readonly HashSet<string> Preview22Ids = ReadBaseline();

    internal static int Order(BindingDeclaration declaration) =>
        Preview22Ids.Contains(declaration.StableId) ? 0 : 1;

    private static HashSet<string> ReadBaseline()
    {
        using Stream stream = typeof(GeneratedBindingEpoch).Assembly.GetManifestResourceStream(
            "OcctSharp.Generator.Emission.preview22-stable-ids.json")
            ?? throw new InvalidDataException("The Preview.22 generated ABI baseline is missing.");
        string[] ids = JsonSerializer.Deserialize<string[]>(stream)
            ?? throw new InvalidDataException("The Preview.22 generated ABI baseline is invalid.");
        return ids.ToHashSet(StringComparer.Ordinal);
    }
}

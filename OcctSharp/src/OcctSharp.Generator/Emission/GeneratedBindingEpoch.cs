using System.Text.Json;
using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.Emission;

/// <summary>Preserves W then X overload ordinals ahead of newly supported declarations.</summary>
internal static class GeneratedBindingEpoch
{
    private static readonly HashSet<string> Preview22Ids = ReadBaseline("preview22-stable-ids.json");
    private static readonly HashSet<string> Preview23Additions = ReadBaseline("preview23-added-stable-ids.json");

    internal static int Order(BindingDeclaration declaration) =>
        Preview22Ids.Contains(declaration.StableId) ? 0 : Preview23Additions.Contains(declaration.StableId) ? 1 : 2;

    private static HashSet<string> ReadBaseline(string resource)
    {
        using Stream stream = typeof(GeneratedBindingEpoch).Assembly.GetManifestResourceStream(
            "OcctSharp.Generator.Emission." + resource)
            ?? throw new InvalidDataException("The Preview.22 generated ABI baseline is missing.");
        string[] ids = JsonSerializer.Deserialize<string[]>(stream)
            ?? throw new InvalidDataException("The Preview.22 generated ABI baseline is invalid.");
        return ids.ToHashSet(StringComparer.Ordinal);
    }
}

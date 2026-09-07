using System.Text.Json;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

internal static class ReferenceOperationContract
{
    private static readonly Dictionary<string, string> Signatures = Load();
    internal static bool Contains(BindingDeclaration declaration) =>
        Signatures.TryGetValue(declaration.StableId, out string? signature)
        && signature == declaration.NativeSignature;
    internal static bool IsMutableReference(BindingType type) => type.Layers.Count == 2
        && type.Layers[0].Kind == BindingTypeLayerKind.LValueReference
        && type.Layers[1].Kind == BindingTypeLayerKind.Value && !type.Layers[1].IsConstQualified;
    internal static BindingType CopiedType(BindingType type) => type with
    {
        NativeSpelling = type.BaseNativeSpelling,
        CanonicalSpelling = type.BaseCanonicalSpelling,
        Layers = [new(BindingTypeLayerKind.Value, false)],
    };
    internal static bool TryMap(BindingDeclaration declaration, BindingType type, BindingTypeUsage usage,
        InitialTypeMap map, out BindingTypeProjection? projection)
    {
        if (usage == BindingTypeUsage.Parameter && Contains(declaration) && IsMutableReference(type))
        {
            if (map.TryMap(CopiedType(type), usage, out projection) && projection?.Ownership == "ValueCopy") return true;
            projection = null;
            return false;
        }
        return map.TryMap(type, usage, out projection);
    }
    private static Dictionary<string, string> Load()
    {
        using Stream stream = typeof(ReferenceOperationContract).Assembly.GetManifestResourceStream(
            "OcctSharp.Generator.Transformation.reference-operation-contracts.json")
            ?? throw new InvalidDataException("Reference operation contracts are missing.");
        using JsonDocument document = JsonDocument.Parse(stream);
        return document.RootElement.EnumerateArray().ToDictionary(row => row.GetProperty("stableId").GetString()!,
            row => row.GetProperty("signature").GetString()!, StringComparer.Ordinal);
    }
}

using System.Text.Json;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

/// <summary>Exact audited ownership and declaration contracts; this never broadens InitialTypeMap.</summary>
internal static class OwnedValueOperationContract
{
    private sealed record Owner(string Header, OcctProductModule Module, bool Copyable, bool SupportingDefaultFactory);
    private sealed record Operation(string NativeName, string Signature);
    private static readonly (Dictionary<string, Owner> Owners, Dictionary<string, Operation> Operations) Catalogue = Load();
    private static readonly Dictionary<string, Dictionary<string, Operation>> Inherited = LoadInherited();

    internal static IReadOnlySet<string> Owners { get; } = Catalogue.Owners.Keys.ToHashSet(StringComparer.Ordinal);
    internal static IReadOnlySet<string> SupportingDefaultOwners { get; } = Catalogue.Owners
        .Where(pair => pair.Value.SupportingDefaultFactory).Select(pair => pair.Key).ToHashSet(StringComparer.Ordinal);
    internal static IReadOnlySet<string> StableIds { get; } = Catalogue.Operations.Keys.ToHashSet(StringComparer.Ordinal);

    internal static bool Contains(BindingDeclaration declaration) =>
        declaration.SupportState is not (BindingSupportState.Manual or BindingSupportState.Skipped)
        && declaration.SkipReason?.Category != "ArtifactUnavailable"
        && !declaration.IsDeleted && !declaration.IsUnavailable && !declaration.IsTemplated
        && !declaration.IsPureVirtual && !declaration.IsVariadic && !declaration.IsOverloadedOperator
        && declaration.Access == BindingAccess.Public
        && declaration.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && Catalogue.Operations.TryGetValue(declaration.StableId, out Operation? operation)
        && operation.NativeName == declaration.NativeName && operation.Signature == declaration.NativeSignature;

    internal static string GetOwnerName(BindingDeclaration declaration) => GetOwnerName(declaration.NativeName);
    internal static string GetOwnerName(string nativeName)
    {
        int separator = nativeName.IndexOf("::", StringComparison.Ordinal);
        return separator < 0 ? nativeName : nativeName[..separator];
    }

    internal static string ManagedName(string owner) => string.Concat(owner.Split('_')
        .Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..]));
    internal static string Header(string owner) => Catalogue.Owners[owner].Header;
    internal static OcctProductModule Module(string owner) => Catalogue.Owners[owner].Module;
    internal static bool IsCopyable(string owner) => Catalogue.Owners.TryGetValue(owner, out Owner? contract) && contract.Copyable;
    internal static bool IsMutableReference(BindingType type) => ReferenceOperationContract.IsMutableReference(type);
    internal static string NativeTypeName(BindingType type) => Normalize(type.BaseCanonicalSpelling);

    internal static IEnumerable<BindingDeclaration> SupportingOperations(BindingModel model, string owner) =>
        Inherited.TryGetValue(owner, out var methods)
            ? model.Declarations.Where(d => d.Access == BindingAccess.Public && !d.IsUnavailable && !d.IsDeleted
                && d.SupportState != BindingSupportState.Skipped && methods.TryGetValue(d.StableId, out var method)
                && d.NativeName == method.NativeName && d.NativeSignature == method.Signature).OrderBy(d => d.StableId, StringComparer.Ordinal)
            : [];

    internal static bool TryMap(BindingDeclaration declaration, BindingType type, BindingTypeUsage usage,
        InitialTypeMap map, out BindingTypeProjection? projection)
    {
        projection = null;
        bool supporting = declaration.Access == BindingAccess.Public && !declaration.IsUnavailable
            && !declaration.IsDeleted && declaration.SupportState != BindingSupportState.Skipped
            && Inherited.Values.Any(methods => methods.TryGetValue(declaration.StableId, out var method)
                && method.NativeName == declaration.NativeName && method.Signature == declaration.NativeSignature);
        if (!Contains(declaration) && !supporting) return map.TryMap(type, usage, out projection);

        string owner = NativeTypeName(type);
        if (Owners.Contains(owner))
        {
            bool value = type.Layers.Count == 1 && type.Layers[0].Kind == BindingTypeLayerKind.Value;
            bool constReference = type.Layers.Count == 2
                && type.Layers[0].Kind == BindingTypeLayerKind.LValueReference
                && type.Layers[1].Kind == BindingTypeLayerKind.Value && type.Layers[1].IsConstQualified;
            bool supported = usage == BindingTypeUsage.Parameter ? constReference
                : usage == BindingTypeUsage.ReturnValue && IsCopyable(owner) && (value || constReference);
            if (!supported) return false;
            string managed = ManagedName(owner);
            projection = new("TM010", "OcctSharp_Owned_" + owner + "Handle*", managed + ".OwnedHandle",
                managed, "OwnedValue", usage == BindingTypeUsage.Parameter
                    ? "A typed registry lease retains a const native owner during the call; the algorithm is never copied."
                    : "An audited independent native value copy is registered as a new owning result before publication.");
            return true;
        }

        if (usage == BindingTypeUsage.Parameter && IsMutableReference(type))
        {
            if (map.TryMap(ReferenceOperationContract.CopiedType(type), usage, out projection)
                && projection?.Ownership == "ValueCopy") return true;
            projection = null;
            return false;
        }
        return map.TryMap(type, usage, out projection);
    }

    private static string Normalize(string spelling) => spelling.StartsWith("const ", StringComparison.Ordinal)
        ? spelling[6..].Trim() : spelling.Trim();

    private static Dictionary<string, Dictionary<string, Operation>> LoadInherited()
    {
        using Stream stream = typeof(OwnedValueOperationContract).Assembly.GetManifestResourceStream(
            "OcctSharp.Generator.Transformation.owned-value-operation-contracts.json")!;
        using JsonDocument document = JsonDocument.Parse(stream);
        Dictionary<string, Dictionary<string, Operation>> result = new(StringComparer.Ordinal);
        foreach (JsonElement row in document.RootElement.GetProperty("supportingOperations").EnumerateArray())
        {
            string owner = Required(row, "owner");
            if (!result.TryGetValue(owner, out var methods)) result.Add(owner, methods = new(StringComparer.Ordinal));
            methods.Add(Required(row, "stableId"), new(Required(row, "nativeName"), Required(row, "signature")));
        }
        return result;
    }

    private static (Dictionary<string, Owner>, Dictionary<string, Operation>) Load()
    {
        using Stream stream = typeof(OwnedValueOperationContract).Assembly.GetManifestResourceStream(
            "OcctSharp.Generator.Transformation.owned-value-operation-contracts.json")
            ?? throw new InvalidDataException("Owned value operation contracts are missing.");
        using JsonDocument document = JsonDocument.Parse(stream);
        if (document.RootElement.GetProperty("schemaVersion").GetString() != "1.0")
            throw new InvalidDataException("Unsupported owned value contract schema.");
        Dictionary<string, Owner> owners = new(StringComparer.Ordinal);
        foreach (JsonElement row in document.RootElement.GetProperty("owners").EnumerateArray())
        {
            string name = Required(row, "nativeName");
            _ = Required(row, "reason");
            owners.Add(name, new(Required(row, "header"), Enum.Parse<OcctProductModule>(Required(row, "module")),
                row.GetProperty("copyable").GetBoolean(), row.GetProperty("supportingDefaultFactory").GetBoolean()));
        }
        Dictionary<string, Operation> operations = new(StringComparer.Ordinal);
        foreach (JsonElement row in document.RootElement.GetProperty("operations").EnumerateArray())
        {
            string name = Required(row, "nativeName");
            if (!owners.ContainsKey(GetOwnerName(name))) throw new InvalidDataException("Operation has no audited owner: " + name);
            _ = Required(row, "reason");
            operations.Add(Required(row, "stableId"), new(name, Required(row, "signature")));
        }
        return (owners, operations);
    }

    private static string Required(JsonElement row, string name) =>
        row.GetProperty(name).GetString() is { Length: > 0 } value && !string.IsNullOrWhiteSpace(value)
            ? value : throw new InvalidDataException("Missing owned contract field: " + name);
}

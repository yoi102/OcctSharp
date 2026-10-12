using System.Text.Json;
using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

/// <summary>Exact declaration and closed element catalogue. This does not broaden InitialTypeMap.</summary>
internal static class CopiedCollectionOperationContract
{
    internal sealed record Operation(string StableId, string NativeName, string Signature, string Guard, bool Supporting,
        string? Kind = null, string? Header = null, string? SourcePackage = null, string? SourceToolkit = null);
    internal sealed record Collection(string NativeType, string Element, string Name, string Abi, string Managed,
        string ToNative, string FromNative, bool Sequence = false, bool Grid = false)
    {
        internal string Friendly => (Grid ? "CopiedGrid<" : Sequence ? "CopiedSequence<" : "CopiedArray<") + Managed + ">";
    }

    internal static readonly Collection[] Collections =
    [
        new("NCollection_Array1<double>", "double", "RealArray", "double", "double", "OcctSharp_GeometryValues::Check(value)", "value"),
        new("NCollection_Array1<int>", "int", "IntegerArray", "int32_t", "int", "value", "value"),
        new("NCollection_Array1<gp_Pnt>", "gp_Pnt", "Point3dArray", "OcctSharp_Value_Coordinates3d", "global::OcctSharp.Values.Coordinates3d",
            "gp_Pnt(OcctSharp_GeometryValues::ToNative(value))", "OcctSharp_GeometryValues::FromNative(value.XYZ())"),
        new("NCollection_Array1<gp_Pnt2d>", "gp_Pnt2d", "Point2dArray", "OcctSharp_Value_Point2d", "global::OcctSharp.Values.Point2d",
            "OcctSharp_GeometryValues::ToNative(value)", "OcctSharp_GeometryValues::FromNative(value)"),
        new("NCollection_Sequence<double>", "double", "RealSequence", "double", "double", "OcctSharp_GeometryValues::Check(value)", "value", true),
    ];

    private static readonly (Dictionary<string, Operation> Operations, HashSet<string> Owners) Catalogue = Load();
    internal static IReadOnlyDictionary<string, Operation> Operations => Catalogue.Operations;
    internal static IReadOnlySet<string> Owners => Catalogue.Owners;
    internal static bool Contains(BindingDeclaration d) => Catalogue.Operations.TryGetValue(d.StableId, out var op)
        && d.NativeName == op.NativeName && d.NativeSignature == op.Signature
        && d.Access == BindingAccess.Public && !d.IsDeleted && !d.IsUnavailable && !d.IsTemplated
        && !d.IsPureVirtual && !d.IsVariadic && !d.IsOverloadedOperator
        && d.Kind is BindingDeclarationKind.Constructor or BindingDeclarationKind.Method
        && d.SupportState != BindingSupportState.Skipped
        && (op.Supporting || d.SupportState != BindingSupportState.Manual);

    internal static Collection? Find(BindingType type) => Collections.FirstOrDefault(c =>
        c.NativeType == type.BaseCanonicalSpelling.Replace("const ", "", StringComparison.Ordinal).Trim()
        && (type.Layers.Count == 1 && type.Layers[0].Kind == BindingTypeLayerKind.Value
            || type.Layers.Count == 2 && type.Layers[0].Kind == BindingTypeLayerKind.LValueReference
                && type.Layers[1].Kind == BindingTypeLayerKind.Value));

    internal static bool TryMap(BindingDeclaration d, BindingType type, BindingTypeUsage usage,
        InitialTypeMap map, out BindingTypeProjection? projection)
    {
        projection = null;
        if (!Contains(d)) return map.TryMap(type, usage, out projection);
        if (Find(type) is { } collection)
        {
            projection = new("TM011", "void*", "CopiedHandle", collection.Friendly, "CollectionCopy",
                "Exact audited one-dimensional container; independent input/output snapshots and bounded copy.");
            return true;
        }
        string owner = type.BaseCanonicalSpelling.Replace("const ", "", StringComparison.Ordinal).Trim();
        if (Owners.Contains(owner) && type.Layers.Count == 1)
        {
            projection = new("TM012", "void*", "OwnerHandle", OwnedValueOperationContract.ManagedName(owner) + "Collections", "OwnedValue",
                "Audited fitting owner, constructed in stable storage and retained by typed registry leases.");
            return true;
        }
        return map.TryMap(type, usage, out projection);
    }

    private static (Dictionary<string, Operation>, HashSet<string>) Load() => Load("copied-collection-operation-contracts.json");

    internal static (Dictionary<string, Operation> Operations, HashSet<string> Owners) Load(string resource)
    {
        using Stream stream = typeof(CopiedCollectionOperationContract).Assembly.GetManifestResourceStream(
            "OcctSharp.Generator.Transformation." + resource)
            ?? throw new InvalidDataException("Copied collection contracts are missing.");
        using JsonDocument document = JsonDocument.Parse(stream);
        if (document.RootElement.GetProperty("schemaVersion").GetString() != "1.0")
            throw new InvalidDataException("Unsupported copied collection contract schema.");
        Dictionary<string, Operation> operations = new(StringComparer.Ordinal);
        foreach (string key in new[] { "operations", "supportingOperations" })
            foreach (var row in document.RootElement.GetProperty(key).EnumerateArray())
            {
                string Field(string name) => row.GetProperty(name).GetString() is { Length: > 0 } text
                    ? text : throw new InvalidDataException("Missing collection contract field " + name);
                _ = Field("reason");
                string? Optional(string name) => row.TryGetProperty(name, out var value) ? value.GetString() : null;
                var operation = new Operation(Field("stableId"), Field("nativeName"), Field("signature"), Field("guard"), key == "supportingOperations",
                    Optional("kind"), Optional("header"), Optional("sourcePackage"), Optional("sourceToolkit"));
                if (operation.Kind is not null && (operation.Kind != "Function" || string.IsNullOrWhiteSpace(operation.Header)
                    || !operation.Header.EndsWith(".hxx", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(operation.SourcePackage)
                    || string.IsNullOrWhiteSpace(operation.SourceToolkit)))
                    throw new InvalidDataException("Audited function requires exact public header and toolkit provenance.");
                operations.Add(operation.StableId, operation);
            }
        return (operations, document.RootElement.GetProperty("owners").EnumerateArray()
            .Select(row => row.GetProperty("nativeName").GetString()!).ToHashSet(StringComparer.Ordinal));
    }
}

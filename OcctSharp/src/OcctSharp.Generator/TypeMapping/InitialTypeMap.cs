using OcctSharp.Generator.Model;

namespace OcctSharp.Generator.TypeMapping;

public sealed class InitialTypeMap
{
    private readonly IReadOnlyDictionary<string, string> _managedEnumTypes;

    public InitialTypeMap(IEnumerable<string>? enumTypes = null)
    {
        _managedEnumTypes = (enumTypes ?? [])
            .Select(NormalizeBaseType)
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(static name => name, ToManagedTypeName, StringComparer.Ordinal);
    }

    private InitialTypeMap(IReadOnlyDictionary<string, string> managedEnumTypes) =>
        _managedEnumTypes = managedEnumTypes;

    public static InitialTypeMap FromModel(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        BindingDeclaration[] enums = model.Declarations
            .Where(static declaration => declaration.Kind == BindingDeclarationKind.Enum)
            .ToArray();
        Dictionary<string, string> mappings = enums.ToDictionary(
            static declaration => NormalizeBaseType(declaration.NativeName),
            static declaration => ToManagedTypeName(declaration.NativeName),
            StringComparer.Ordinal);
        foreach (IGrouping<string, BindingDeclaration> group in enums.GroupBy(
            static declaration => GetUnqualifiedName(NormalizeBaseType(declaration.NativeName)),
            StringComparer.Ordinal))
        {
            if (group.Count() == 1)
            {
                BindingDeclaration declaration = group.Single();
                mappings.TryAdd(group.Key, ToManagedTypeName(declaration.NativeName));
            }
        }
        return new InitialTypeMap(mappings);
    }

    public bool TryMap(
        BindingType type,
        BindingTypeUsage usage,
        out BindingTypeProjection? projection)
    {
        ArgumentNullException.ThrowIfNull(type);
        projection = null;

        if (!IsSupportedPassMode(type, usage))
        {
            return false;
        }

        string nativeType = NormalizeBaseType(type.BaseNativeSpelling);
        string canonicalType = NormalizeBaseType(type.BaseCanonicalSpelling);

        if (type.IsOcctHandle && !string.IsNullOrWhiteSpace(type.HandleTargetType))
        {
            string targetType = NormalizeBaseType(type.HandleTargetType);
            projection = new BindingTypeProjection(
                "TM006",
                "OcctSharp_TransientHandle*",
                "SharedTransientHandle",
                ToManagedTypeName(targetType),
                "Shared",
                "Opaque native wrapper retains one OCCT intrusive handle reference; raw object pointers never cross the ABI.");
            return true;
        }

        if (Matches(nativeType, canonicalType, "TopoDS_Shape"))
        {
            projection = new BindingTypeProjection(
                "TM007",
                "OcctSharp_ShapeHandle*",
                "ShapeHandle",
                "Shape",
                "Owning",
                "Opaque native wrapper owns an independent TopoDS_Shape value while OCCT shares its internal TShape; native layout never crosses the ABI.");
            return true;
        }

        if (Matches(nativeType, canonicalType, "Standard_Integer", "int", "signed int"))
        {
            projection = new BindingTypeProjection(
                "TM001",
                "int32_t",
                "int",
                "int",
                "ValueCopy",
                "Direct fixed-width value conversion after compile-time width validation.");
            return true;
        }

        if (Matches(nativeType, canonicalType, "Standard_Real", "double"))
        {
            projection = new BindingTypeProjection(
                "TM002",
                "double",
                "double",
                "double",
                "ValueCopy",
                "Direct IEEE-754 binary64 value conversion.");
            return true;
        }

        if (Matches(nativeType, canonicalType, "Standard_Boolean", "bool"))
        {
            projection = new BindingTypeProjection(
                "TM003",
                "int32_t",
                "int",
                "bool",
                "ValueCopy",
                "Native bool is normalized to ABI 0 or 1; managed friendly API converts explicitly.");
            return true;
        }

        if (_managedEnumTypes.TryGetValue(nativeType, out string? managedEnumType)
            || _managedEnumTypes.TryGetValue(canonicalType, out managedEnumType))
        {
            projection = new BindingTypeProjection(
                "TM004",
                "int32_t",
                "int",
                managedEnumType,
                "ValueCopy",
                "Enum value is converted through its compile-validated 32-bit underlying representation.");
            return true;
        }

        if (Matches(nativeType, canonicalType, "gp_Pnt"))
        {
            projection = new BindingTypeProjection(
                "TM005",
                "OcctSharp_Point3d",
                "Point3dRaw",
                "Point3d",
                "ValueCopy",
                "Copy X/Y/Z through OCCT accessors and constructor; native class layout never crosses the ABI.");
            return true;
        }

        return false;
    }

    private static bool IsSupportedPassMode(BindingType type, BindingTypeUsage usage)
    {
        if (type.Layers is [{ Kind: BindingTypeLayerKind.Value }])
        {
            return true;
        }

        return usage == BindingTypeUsage.Parameter
            && type.Layers is
            [
                { Kind: BindingTypeLayerKind.LValueReference },
                { Kind: BindingTypeLayerKind.Value, IsConstQualified: true },
            ];
    }

    private static bool Matches(string nativeType, string canonicalType, params string[] candidates)
    {
        return candidates.Any(candidate =>
            string.Equals(nativeType, candidate, StringComparison.Ordinal)
            || string.Equals(canonicalType, candidate, StringComparison.Ordinal));
    }

    private static string NormalizeBaseType(string value)
    {
        string normalized = value.Trim();
        if (normalized.StartsWith("const ", StringComparison.Ordinal))
        {
            normalized = normalized[6..].TrimStart();
        }

        if (normalized.EndsWith(" const", StringComparison.Ordinal))
        {
            normalized = normalized[..^6].TrimEnd();
        }

        return normalized;
    }

    private static string ToManagedTypeName(string nativeType)
    {
        string[] parts = nativeType.Split(
            ['_', ':'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Concat(parts.Select(static part =>
            part.Length == 0 ? string.Empty : char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static string GetUnqualifiedName(string nativeType)
    {
        int separator = nativeType.LastIndexOf("::", StringComparison.Ordinal);
        return separator < 0 ? nativeType : nativeType[(separator + 2)..];
    }
}

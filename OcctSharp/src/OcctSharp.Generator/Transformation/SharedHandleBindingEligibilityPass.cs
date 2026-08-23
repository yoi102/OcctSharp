using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

public static class SharedHandleBindingEligibilityPass
{
    public static BindingModel Apply(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        HashSet<string> transientTypes = FindTransientTypes(model);
        return new BindingModel(model.Declarations.Select(declaration =>
            Promote(declaration, transientTypes, typeMap)));
    }

    private static BindingDeclaration Promote(
        BindingDeclaration declaration,
        HashSet<string> transientTypes,
        InitialTypeMap typeMap)
    {
        if (declaration.SupportState != BindingSupportState.Pending
            || declaration.Access != BindingAccess.Public)
        {
            return declaration;
        }

        string? declaringType = GetDeclaringType(declaration.NativeName);
        if (declaringType is null || !transientTypes.Contains(declaringType))
        {
            return declaration;
        }

        bool isEligible = declaration.Kind switch
        {
            BindingDeclarationKind.Constructor =>
                declaration.Parameters.All(parameter => IsValueCopy(parameter.Type, typeMap)),
            BindingDeclarationKind.Method =>
                !declaration.IsStatic
                && !declaration.IsPureVirtual
                && !declaration.IsOverloadedOperator
                && !IsDestructor(declaration, declaringType)
                && IsValueOrVoidReturn(declaration.ReturnType, typeMap)
                && declaration.Parameters.All(parameter => IsValueCopy(parameter.Type, typeMap)),
            _ => false,
        };

        return isEligible
            ? declaration with { SupportState = BindingSupportState.Supported }
            : declaration;
    }

    private static HashSet<string> FindTransientTypes(BindingModel model)
    {
        Dictionary<string, string[]> bases = model.Declarations
            .Where(static declaration => declaration.Kind == BindingDeclarationKind.Record)
            .GroupBy(static declaration => NormalizeTypeName(declaration.NativeName), StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .SelectMany(static declaration => declaration.BaseTypes)
                    .Select(static baseType => NormalizeTypeName(baseType.Type.BaseCanonicalSpelling))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        HashSet<string> result = new(StringComparer.Ordinal) { "Standard_Transient" };
        bool changed;
        do
        {
            changed = false;
            foreach ((string type, string[] baseTypes) in bases)
            {
                if (!result.Contains(type) && baseTypes.Any(result.Contains))
                {
                    changed |= result.Add(type);
                }
            }
        }
        while (changed);

        return result;
    }

    private static bool IsValueCopy(BindingType type, InitialTypeMap typeMap) =>
        typeMap.TryMap(type, BindingTypeUsage.Parameter, out BindingTypeProjection? projection)
        && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal);

    private static bool IsValueOrVoidReturn(BindingType? type, InitialTypeMap typeMap)
    {
        if (type is null)
        {
            return false;
        }

        if (string.Equals(NormalizeTypeName(type.BaseCanonicalSpelling), "void", StringComparison.Ordinal)
            && type.Layers is [{ Kind: BindingTypeLayerKind.Value }])
        {
            return true;
        }

        return typeMap.TryMap(type, BindingTypeUsage.ReturnValue, out BindingTypeProjection? projection)
            && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal);
    }

    private static bool IsDestructor(BindingDeclaration declaration, string declaringType)
    {
        string memberName = declaration.NativeName[(declaration.NativeName.LastIndexOf("::", StringComparison.Ordinal) + 2)..];
        int separator = declaringType.LastIndexOf("::", StringComparison.Ordinal);
        string unqualifiedType = separator < 0 ? declaringType : declaringType[(separator + 2)..];
        return string.Equals(memberName, "~" + unqualifiedType, StringComparison.Ordinal);
    }

    private static string? GetDeclaringType(string nativeName)
    {
        int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal);
        return separator <= 0 ? null : NormalizeTypeName(nativeName[..separator]);
    }

    private static string NormalizeTypeName(string value)
    {
        string normalized = value.Trim();
        foreach (string prefix in new[] { "class ", "struct ", "const " })
        {
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
            {
                normalized = normalized[prefix.Length..].TrimStart();
            }
        }

        if (normalized.EndsWith(" const", StringComparison.Ordinal))
        {
            normalized = normalized[..^6].TrimEnd();
        }

        return normalized;
    }
}

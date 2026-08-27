using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

public static class SimpleBindingEligibilityPass
{
    public static BindingModel Apply(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        HashSet<string> transientTypes = FindTransientTypes(model);
        return new BindingModel(model.Declarations.Select(declaration => Promote(declaration, typeMap, transientTypes)));
    }

    public static SimpleBindingEligibilityAssessment Assess(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        ArgumentNullException.ThrowIfNull(typeMap);

        return declaration.Kind switch
        {
            BindingDeclarationKind.Constructor => AssessConstructor(declaration, typeMap),
            BindingDeclarationKind.Method => AssessMethod(declaration, typeMap),
            BindingDeclarationKind.Function => AssessFunction(declaration, typeMap),
            _ => new SimpleBindingEligibilityAssessment(
                "EL001",
                "DeclarationKind",
                "Only constructors and static methods are in the initial value-copy emission scope.",
                false),
        };
    }

    private static BindingDeclaration Promote(
        BindingDeclaration declaration,
        InitialTypeMap typeMap,
        HashSet<string> transientTypes)
    {
        if (declaration.SupportState != BindingSupportState.Pending)
        {
            return declaration;
        }

        // Intrusive-handle records need receiver/constructor ownership analysis from the
        // shared-handle pass. Treating their constructors as ordinary value copies also
        // promoted constructors of abstract transient classes that cannot be invoked.
        if (declaration.Kind == BindingDeclarationKind.Constructor
            && GetDeclaringType(declaration.NativeName) is string declaringType
            && transientTypes.Contains(declaringType))
        {
            return declaration;
        }

        bool isEligible = Assess(declaration, typeMap).IsEligible;

        return isEligible
            ? declaration with { SupportState = BindingSupportState.Supported }
            : declaration;
    }

    private static SimpleBindingEligibilityAssessment AssessConstructor(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        string? declaringType = GetDeclaringType(declaration.NativeName);
        if (declaringType is null
            || !TryMapValueType(declaringType, typeMap, BindingTypeUsage.ReturnValue, out _))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL003",
                "ReturnProjection",
                "The constructed value does not have a verified value-copy return projection.",
                false);
        }

        return AssessParameters(declaration.Parameters, typeMap);
    }

    private static SimpleBindingEligibilityAssessment AssessMethod(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        if (!declaration.IsStatic)
        {
            return new SimpleBindingEligibilityAssessment(
                "EL002",
                "InstanceReceiver",
                "Instance receiver ownership and value semantics are not in the initial automatic emission scope.",
                false);
        }

        if (!IsSupportedReturn(declaration.ReturnType, typeMap))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL003",
                "ReturnProjection",
                "The return value does not have a verified value-copy projection.",
                false);
        }

        return AssessParameters(declaration.Parameters, typeMap);
    }

    private static SimpleBindingEligibilityAssessment AssessFunction(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        if (string.IsNullOrWhiteSpace(declaration.SourceToolkit))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL005",
                "ToolkitProvenance",
                "The free function has no toolkit provenance for deterministic native link closure.",
                false);
        }

        if (!declaration.Header.EndsWith(".hxx", StringComparison.OrdinalIgnoreCase))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL006",
                "InternalHeaderSurface",
                "The free function is declared by a C/parser/internal header rather than a public OCCT .hxx entry header.",
                false);
        }

        if (!string.Equals(declaration.SourcePackage, "Standard", StringComparison.Ordinal))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL007",
                "FreeFunctionExportProvenance",
                "Only the verified Standard foundation free-function profile has exact native export evidence.",
                false);
        }

        if (!IsSupportedReturn(declaration.ReturnType, typeMap))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL003",
                "ReturnProjection",
                "The return value does not have a verified value-copy or void projection.",
                false);
        }

        return AssessParameters(declaration.Parameters, typeMap);
    }

    private static bool IsSupportedReturn(BindingType? type, InitialTypeMap typeMap)
    {
        if (type is null)
        {
            return false;
        }

        return typeMap.TryMap(type, BindingTypeUsage.ReturnValue, out BindingTypeProjection? projection)
            && (string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal)
                || string.Equals(projection?.RuleId, "TM000", StringComparison.Ordinal));
    }

    private static SimpleBindingEligibilityAssessment AssessParameters(
        IReadOnlyList<BindingParameter> parameters,
        InitialTypeMap typeMap)
    {
        foreach (BindingParameter parameter in parameters)
        {
            if (!TryMapValueCopy(parameter.Type, typeMap, BindingTypeUsage.Parameter, out _))
            {
                return new SimpleBindingEligibilityAssessment(
                    "EL004",
                    "ParameterProjection",
                    $"Parameter '{parameter.Name}' does not have a verified value-copy projection.",
                    false);
            }
        }

        return new SimpleBindingEligibilityAssessment(
            "EL000",
            "Eligible",
            "All projected values have verified value-copy semantics.",
            true);
    }

    private static bool TryMapValueType(
        string nativeType,
        InitialTypeMap typeMap,
        BindingTypeUsage usage,
        out BindingTypeProjection? projection)
    {
        BindingType type = new(
            nativeType,
            nativeType,
            nativeType,
            nativeType,
            [new BindingTypeLayer(BindingTypeLayerKind.Value, false)],
            null,
            [],
            false,
            null);
        return TryMapValueCopy(type, typeMap, usage, out projection);
    }

    private static bool TryMapValueCopy(
        BindingType type,
        InitialTypeMap typeMap,
        BindingTypeUsage usage,
        out BindingTypeProjection? projection)
    {
        return typeMap.TryMap(type, usage, out projection)
            && string.Equals(projection?.Ownership, "ValueCopy", StringComparison.Ordinal);
    }

    private static string? GetDeclaringType(string nativeName)
    {
        int separator = nativeName.LastIndexOf("::", StringComparison.Ordinal);
        if (separator <= 0 || separator == nativeName.Length - 2)
        {
            return null;
        }

        string declaringType = nativeName[..separator];
        string memberName = nativeName[(separator + 2)..];
        int declaringTypeSeparator = declaringType.LastIndexOf("::", StringComparison.Ordinal);
        string unqualifiedType = declaringTypeSeparator < 0
            ? declaringType
            : declaringType[(declaringTypeSeparator + 2)..];
        return string.Equals(unqualifiedType, memberName, StringComparison.Ordinal)
            ? declaringType
            : null;
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

        return normalized.EndsWith(" const", StringComparison.Ordinal)
            ? normalized[..^6].TrimEnd()
            : normalized;
    }
}

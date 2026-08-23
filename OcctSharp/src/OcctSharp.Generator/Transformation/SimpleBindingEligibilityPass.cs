using OcctSharp.Generator.Model;
using OcctSharp.Generator.TypeMapping;

namespace OcctSharp.Generator.Transformation;

public static class SimpleBindingEligibilityPass
{
    public static BindingModel Apply(BindingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        InitialTypeMap typeMap = InitialTypeMap.FromModel(model);
        return new BindingModel(model.Declarations.Select(declaration => Promote(declaration, typeMap)));
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
            _ => new SimpleBindingEligibilityAssessment(
                "EL001",
                "DeclarationKind",
                "Only constructors and static methods are in the initial value-copy emission scope.",
                false),
        };
    }

    private static BindingDeclaration Promote(
        BindingDeclaration declaration,
        InitialTypeMap typeMap)
    {
        if (declaration.SupportState != BindingSupportState.Pending)
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

        if (declaration.ReturnType is null
            || !TryMapValueCopy(declaration.ReturnType, typeMap, BindingTypeUsage.ReturnValue, out _))
        {
            return new SimpleBindingEligibilityAssessment(
                "EL003",
                "ReturnProjection",
                "The return value does not have a verified value-copy projection.",
                false);
        }

        return AssessParameters(declaration.Parameters, typeMap);
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
}
